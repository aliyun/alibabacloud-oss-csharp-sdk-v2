#nullable enable
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace AlibabaCloud.OSS.V2.UnitTests.FileLike;

/// <summary>
/// A dynamic HTTP handler that serves an in-memory object over HEAD/GET-by-Range, mirroring the
/// httptest server used by the Go SDK's filelike mock tests. It is thread-safe so concurrent
/// prefetch GETs are served correctly, and exposes fault-injection knobs used to exercise the
/// error, resume and fallback paths.
/// </summary>
internal sealed class FileServerHandler : HttpMessageHandler
{
    private readonly object _lock = new object();
    private byte[] _data;

    public FileServerHandler(byte[] data, string etag = "\"etag-head\"", string contentType = "text/plain")
    {
        _data = data;
        Etag = etag;
        ContentType = contentType;
        LastModified = "Wed, 01 Jan 2025 00:00:00 GMT";
    }

    public string Etag { get; set; }
    public string ContentType { get; set; }
    public string LastModified { get; set; }

    /// <summary>When set, GET responses report this ETag instead of <see cref="Etag"/> (simulates a changed file).</summary>
    public string? GetEtag { get; set; }

    /// <summary>When true, GET responses omit the Content-Range header (simulates an offset mismatch).</summary>
    public bool SuppressContentRange { get; set; }

    /// <summary>When true, a GET carrying a bounded range (prefetch chunk) fails with 403 (forces direct-read fallback).</summary>
    public bool FailBoundedRange { get; set; }

    /// <summary>When true, the next GET body throws partway through (simulates a broken connection, then resumes).</summary>
    public bool HalfBodyOnce { get; set; }

    private int _getCount;
    private int _headCount;
    private readonly ConcurrentQueue<(long Start, long End)> _ranges = new();
    public int GetCount => Volatile.Read(ref _getCount);
    public int HeadCount => Volatile.Read(ref _headCount);

    /// <summary>All GET ranges seen so far; <c>End</c> is -1 for an open-ended range.</summary>
    public IReadOnlyCollection<(long Start, long End)> Ranges => _ranges.ToArray();

    public void SetData(byte[] data)
    {
        lock (_lock) _data = data;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (request.Method == HttpMethod.Head)
            return Task.FromResult(BuildHead());
        if (request.Method == HttpMethod.Get)
            return Task.FromResult(BuildGet(request));
        return Task.FromResult(new HttpResponseMessage(HttpStatusCode.MethodNotAllowed));
    }

    private HttpResponseMessage BuildHead()
    {
        Interlocked.Increment(ref _headCount);
        byte[] data;
        lock (_lock) data = _data;

        var resp = new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(Array.Empty<byte>()) };
        resp.Content.Headers.ContentLength = data.Length;
        resp.Headers.TryAddWithoutValidation("ETag", Etag);
        resp.Content.Headers.TryAddWithoutValidation("Last-Modified", LastModified);
        resp.Content.Headers.TryAddWithoutValidation("Content-Type", ContentType);
        return resp;
    }

    private HttpResponseMessage BuildGet(HttpRequestMessage request)
    {
        Interlocked.Increment(ref _getCount);
        byte[] data;
        lock (_lock) data = _data;

        var (hasRange, rangeStart, rangeEnd) = ParseRange(request);
        if (hasRange)
            _ranges.Enqueue((rangeStart, rangeEnd));

        var offset = 0L;
        var sendLen = (long)data.Length;
        var bounded = false;
        if (hasRange)
        {
            offset = rangeStart;
            sendLen = data.Length - rangeStart;
            if (rangeEnd >= 0)
            {
                bounded = true;
                sendLen = rangeEnd - rangeStart + 1;
                if (sendLen > data.Length - rangeStart)
                    sendLen = data.Length - rangeStart;
            }
        }

        if (bounded && FailBoundedRange)
        {
            var err = "<?xml version=\"1.0\"?><Error><Code>AccessDenied</Code><Message>bounded range rejected</Message></Error>";
            var e = new HttpResponseMessage(HttpStatusCode.Forbidden) { Content = new StringContent(err) };
            return e;
        }

        var slice = new byte[sendLen];
        Array.Copy(data, offset, slice, 0, sendLen);

        var etag = GetEtag ?? Etag;
        var status = hasRange ? HttpStatusCode.PartialContent : HttpStatusCode.OK;

        var throwPartway = false;
        if (HalfBodyOnce)
        {
            HalfBodyOnce = false;
            throwPartway = true;
        }

        HttpContent content = throwPartway
            ? new StreamContent(new FaultyStream(slice))
            : new ByteArrayContent(slice);

        var resp = new HttpResponseMessage(status) { Content = content };
        resp.Content.Headers.ContentLength = sendLen;
        resp.Headers.TryAddWithoutValidation("ETag", etag);
        resp.Content.Headers.TryAddWithoutValidation("Last-Modified", LastModified);
        resp.Content.Headers.TryAddWithoutValidation("Content-Type", ContentType);
        if (hasRange && !SuppressContentRange)
        {
            var last = offset + sendLen - 1;
            resp.Content.Headers.TryAddWithoutValidation("Content-Range", $"bytes {offset}-{last}/{data.Length}");
        }
        return resp;
    }

    private static (bool has, long start, long end) ParseRange(HttpRequestMessage request)
    {
        if (!request.Headers.TryGetValues("Range", out var values))
            return (false, 0, -1);

        var raw = string.Join(",", values);
        // Expected form: bytes=start-[end]
        const string prefix = "bytes=";
        if (!raw.StartsWith(prefix, StringComparison.Ordinal))
            return (false, 0, -1);

        var body = raw.Substring(prefix.Length);
        var dash = body.IndexOf('-');
        if (dash < 0)
            return (false, 0, -1);

        var startStr = body.Substring(0, dash);
        var endStr = body.Substring(dash + 1);
        if (!long.TryParse(startStr, out var start))
            return (false, 0, -1);

        var end = -1L;
        if (endStr.Length != 0 && long.TryParse(endStr, out var e))
            end = e;

        return (true, start, end);
    }

    /// <summary>A stream that serves half its bytes then throws once, simulating a broken connection.</summary>
    private sealed class FaultyStream : Stream
    {
        private readonly byte[] _data;
        private int _pos;
        private readonly int _breakAt;

        public FaultyStream(byte[] data)
        {
            _data = data;
            _breakAt = Math.Max(1, data.Length / 2);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (_pos >= _breakAt)
                throw new IOException("simulated broken connection");
            var n = Math.Min(count, _breakAt - _pos);
            Array.Copy(_data, _pos, buffer, offset, n);
            _pos += n;
            return n;
        }

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            => Task.FromResult(Read(buffer, offset, count));

        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => throw new NotSupportedException();
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public override void Flush() { }
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }
}
