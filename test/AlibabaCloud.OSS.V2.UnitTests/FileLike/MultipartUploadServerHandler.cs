#nullable enable
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AlibabaCloud.OSS.V2.Internal;

namespace AlibabaCloud.OSS.V2.UnitTests.FileLike;

/// <summary>
/// A stateful in-memory multipart-upload server used by the WriteOnlyFile tests. Mirrors the
/// httptest-based <c>mpServer</c> in the Go SDK: it stores parts, assembles the object on
/// CompleteMultipartUpload, and exposes counters and captured headers for assertions. It is
/// thread-safe so concurrent UploadPart calls are served correctly.
/// </summary>
internal sealed class MultipartUploadServerHandler : HttpMessageHandler
{
    private readonly object _lock = new object();
    private readonly Dictionary<long, byte[]> _parts = new();
    private readonly Dictionary<long, List<int>> _partSizes = new();

    public const string UploadIdValue = "upload-id-xyz";

    public string UploadId { get; set; } = UploadIdValue;

    /// <summary>When true, every UploadPart responds 403 AccessDenied.</summary>
    public bool FailUploadPart { get; set; }

    /// <summary>When true, ListParts responds 404 NoSuchUpload.</summary>
    public bool NoSuchUpload { get; set; }

    /// <summary>Part numbers whose UploadPart response omits <c>x-oss-hash-crc64ecma</c>.</summary>
    public HashSet<long> OmitPartCrc { get; } = new();

    /// <summary>Optional per-UploadPart delay, used to keep parts in flight for cancellation tests.</summary>
    public int UploadPartDelayMs { get; set; }

    public Dictionary<string, string>? InitiateHeaders { get; private set; }
    public Dictionary<string, string>? CompleteHeaders { get; private set; }
    public Dictionary<string, string>? PutObjectHeaders { get; private set; }
    public Dictionary<string, string>? UploadPartHeaders { get; private set; }
    public Dictionary<string, string>? AbortHeaders { get; private set; }
    public Dictionary<string, string>? ListPartsHeaders { get; private set; }
    public string? PutObjectQuery { get; private set; }
    public byte[]? Completed { get; private set; }
    public byte[]? PutObjectBody { get; private set; }
    public int InitiateCount { get; private set; }
    public int CompleteCount { get; private set; }
    public int AbortCount { get; private set; }
    public int UploadPartCount { get; private set; }

    /// <summary>When non-null, CompleteMultipartUpload throws this exception instead of succeeding.</summary>
    public Exception? CompleteException { get; set; }

    public IReadOnlyDictionary<long, byte[]> Parts
    {
        get { lock (_lock) return new Dictionary<long, byte[]>(_parts); }
    }

    /// <summary>Seeds a part as if a previous session had uploaded it.</summary>
    public void SeedPart(long partNumber, byte[] data)
    {
        lock (_lock)
        {
            _parts[partNumber] = data;
            _partSizes[partNumber] = new List<int> { data.Length };
        }
    }

    public IReadOnlyList<int> PartSizes(long partNumber)
    {
        lock (_lock)
            return _partSizes.TryGetValue(partNumber, out var sizes) ? sizes.ToArray() : Array.Empty<int>();
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var method = request.Method;
        var query = request.RequestUri?.Query ?? "";

        byte[]? body = null;
        if (request.Content != null)
            body = await request.Content.ReadAsByteArrayAsync().ConfigureAwait(false);

        if (method == HttpMethod.Post && query.IndexOf("uploads", StringComparison.Ordinal) >= 0)
            return Initiate(request);
        if (method == HttpMethod.Put && query.IndexOf("partNumber=", StringComparison.Ordinal) >= 0)
            return await UploadPartAsync(request, query, body ?? Array.Empty<byte>()).ConfigureAwait(false);
        if (method == HttpMethod.Get && query.IndexOf("uploadId", StringComparison.Ordinal) >= 0)
            return ListParts(request);
        if (method == HttpMethod.Post)
            return Complete(request);
        if (method == HttpMethod.Delete)
        {
            lock (_lock)
            {
                AbortCount++;
                AbortHeaders = CaptureHeaders(request);
            }
            return new HttpResponseMessage(HttpStatusCode.NoContent) { Content = new ByteArrayContent(Array.Empty<byte>()) };
        }
        if (method == HttpMethod.Put)
            return PutObject(request, body ?? Array.Empty<byte>());

        return Error(HttpStatusCode.MethodNotAllowed, "MethodNotAllowed", "unexpected request");
    }

    private HttpResponseMessage Initiate(HttpRequestMessage request)
    {
        lock (_lock)
        {
            InitiateCount++;
            InitiateHeaders = CaptureHeaders(request);
        }

        var xml = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" +
                  $"<InitiateMultipartUploadResult><Bucket>bucket</Bucket><Key>key</Key><UploadId>{UploadId}</UploadId></InitiateMultipartUploadResult>";
        return Xml(HttpStatusCode.OK, xml);
    }

    private async Task<HttpResponseMessage> UploadPartAsync(HttpRequestMessage request, string query, byte[] body)
    {
        var delay = UploadPartDelayMs;
        if (delay > 0)
            await Task.Delay(delay).ConfigureAwait(false);

        lock (_lock)
        {
            UploadPartCount++;
            UploadPartHeaders = CaptureHeaders(request);
        }

        if (FailUploadPart)
            return Error(HttpStatusCode.Forbidden, "AccessDenied", "denied");

        var pnStr = QueryValue(query, "partNumber");
        if (!long.TryParse(pnStr, NumberStyles.Integer, CultureInfo.InvariantCulture, out var partNumber))
            return Error(HttpStatusCode.BadRequest, "InvalidArgument", "bad part number");

        bool omitCrc;
        lock (_lock)
        {
            _parts[partNumber] = body;
            if (!_partSizes.TryGetValue(partNumber, out var sizes))
            {
                sizes = new List<int>();
                _partSizes[partNumber] = sizes;
            }
            sizes.Add(body.Length);
            omitCrc = OmitPartCrc.Contains(partNumber);
        }

        var resp = new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(Array.Empty<byte>()) };
        resp.Headers.TryAddWithoutValidation("ETag", $"\"etag-{pnStr}\"");
        if (!omitCrc)
            resp.Headers.TryAddWithoutValidation("x-oss-hash-crc64ecma", Crc(body));
        return resp;
    }

    private HttpResponseMessage ListParts(HttpRequestMessage request)
    {
        lock (_lock)
        {
            ListPartsHeaders = CaptureHeaders(request);
            if (NoSuchUpload)
                return Error(HttpStatusCode.NotFound, "NoSuchUpload", "The specified upload does not exist.");

            var sb = new StringBuilder();
            sb.Append("<?xml version=\"1.0\" encoding=\"UTF-8\"?><ListPartsResult>");
            sb.Append($"<Bucket>bucket</Bucket><Key>key</Key><UploadId>{UploadId}</UploadId><IsTruncated>false</IsTruncated>");
            foreach (var n in _parts.Keys.OrderBy(x => x))
            {
                var data = _parts[n];
                sb.Append($"<Part><PartNumber>{n}</PartNumber><ETag>\"etag-{n}\"</ETag><Size>{data.Length}</Size>");
                sb.Append($"<HashCrc64ecma>{Crc(data)}</HashCrc64ecma></Part>");
            }
            sb.Append("</ListPartsResult>");
            return Xml(HttpStatusCode.OK, sb.ToString());
        }
    }

    private HttpResponseMessage Complete(HttpRequestMessage request)
    {
        var toThrow = CompleteException;
        if (toThrow != null)
            throw toThrow;

        lock (_lock)
        {
            CompleteCount++;
            CompleteHeaders = CaptureHeaders(request);

            var full = new List<byte>();
            foreach (var n in _parts.Keys.OrderBy(x => x))
                full.AddRange(_parts[n]);
            Completed = full.ToArray();
        }

        const string xml = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" +
                           "<CompleteMultipartUploadResult><Bucket>bucket</Bucket><Key>key</Key><ETag>\"final-etag\"</ETag></CompleteMultipartUploadResult>";
        return Xml(HttpStatusCode.OK, xml);
    }

    private HttpResponseMessage PutObject(HttpRequestMessage request, byte[] body)
    {
        lock (_lock)
        {
            PutObjectBody = body;
            PutObjectHeaders = CaptureHeaders(request);
            PutObjectQuery = request.RequestUri?.Query;
        }

        var resp = new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(Array.Empty<byte>()) };
        resp.Headers.TryAddWithoutValidation("ETag", "\"put-etag\"");
        resp.Headers.TryAddWithoutValidation("x-oss-hash-crc64ecma", Crc(body));
        return resp;
    }

    private static string Crc(byte[] data)
    {
        Crc64.InitECMA();
        return Convert.ToString(Crc64.Compute(data, 0, data.Length), CultureInfo.InvariantCulture);
    }

    private static string QueryValue(string query, string name)
    {
        foreach (var pair in query.TrimStart('?').Split('&'))
        {
            var eq = pair.IndexOf('=');
            if (eq < 0) continue;
            if (string.Equals(pair.Substring(0, eq), name, StringComparison.Ordinal))
                return Uri.UnescapeDataString(pair.Substring(eq + 1));
        }
        return "";
    }

    private static Dictionary<string, string> CaptureHeaders(HttpRequestMessage request)
    {
        var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var h in request.Headers)
            headers[h.Key] = string.Join(",", h.Value);
        if (request.Content != null)
            foreach (var h in request.Content.Headers)
                headers[h.Key] = string.Join(",", h.Value);
        return headers;
    }

    private static HttpResponseMessage Xml(HttpStatusCode status, string xml)
        => new(status) { Content = new StringContent(xml, Encoding.UTF8, "application/xml") };

    private static HttpResponseMessage Error(HttpStatusCode status, string code, string message)
    {
        var xml = $"<?xml version=\"1.0\"?><Error><Code>{code}</Code><Message>{message}</Message></Error>";
        return Xml(status, xml);
    }
}
