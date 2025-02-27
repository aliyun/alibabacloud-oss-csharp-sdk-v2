using AlibabaCloud.OSS.V2.Internal;
using System.Globalization;

namespace AlibabaCloud.OSS.V2.UnitTests;

internal class MockHttpMessageHandler : HttpMessageHandler {
    public HttpRequestMessage LastRequest;
    public IList<HttpResponseMessage> Responses;
    public IList<HttpRequestMessage> Requests;
    public IList<byte[]> RequestBodies;

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    ) {
        LastRequest = request;
        Requests ??= new List<HttpRequestMessage>();
        Requests.Add(request);

        if (Responses == null || Responses.Count == 0) throw new("Responses is null or empty");

        RequestBodies ??= new List<byte[]>();
        var body = new byte[] { };
        if (request.Content != null) {
            body = request.Content.ReadAsByteArrayAsync().GetAwaiter().GetResult();
        }
        RequestBodies.Add(body);

        var ret = Responses[0];
        Responses.RemoveAt(0);

        if (CalcCrc64) {
            var crc = Crc64.Compute(body, 0, body.Length);
            ret.Headers.Add("x-oss-hash-crc64ecma", Convert.ToString(crc, CultureInfo.InvariantCulture));
        }
        return Task.FromResult(ret);
    }

    public void Clear() {
        LastRequest = null;
        Requests = null;
        Responses = null;
        RequestBodies = null;
    }

    public bool CalcCrc64 { get; set; }
}

internal class TimeoutReadStream : WrapperStream {
    private TimeSpan _timeout;
    public TimeoutReadStream(Stream baseStream, TimeSpan timeout) : base(baseStream) {
        _timeout = timeout;
    }

    public override int Read(byte[] buffer, int offset, int count) {
        Thread.Sleep(_timeout);
        var n = base.Read(buffer, offset, count);
        return n;
    }

    public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) {
        await Task.Delay(_timeout);
        return await base.ReadAsync(buffer, offset, count, cancellationToken).ConfigureAwait(false);
    }
}

class Utils {

    public static string GetTempFileName() {
        return $"file-{Guid.NewGuid().ToString()}.tmp";
    }

    public static string GetTempPath() {
        //return Path.Join(Path.GetTempPath(), $"csharp-sdk-test-{Guid.NewGuid().ToString()}");
        return $"{Path.GetTempPath()}csharp-sdk-test-{Guid.NewGuid().ToString()}";
    }
}
