#nullable enable
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using AlibabaCloud.OSS.V2;
using AlibabaCloud.OSS.V2.Credentials;
using AlibabaCloud.OSS.V2.Transport;

namespace AlibabaCloud.OSS.V2.UnitTests.FileLike;

public class AppendOnlyFileTest
{
    private static (Client, MockHttpMessageHandler) MakeClient()
    {
        var mock = new MockHttpMessageHandler();
        var config = new Configuration
        {
            Region = "cn-hangzhou",
            CredentialsProvider = new AnonymousCredentialsProvider(),
            HttpTransport = new HttpTransport(mock)
        };
        var client = new Client(config, x =>
        {
            x.FeatureFlags &= ~(FeatureFlagsType.EnableCrc64CheckDownload | FeatureFlagsType.EnableCrc64CheckUpload);
        });
        return (client, mock);
    }

    // A client that keeps the upload CRC check enabled (default), used to exercise the append CRC path.
    private static (Client, MockHttpMessageHandler) MakeClientWithUploadCrc()
    {
        var mock = new MockHttpMessageHandler();
        var config = new Configuration
        {
            Region = "cn-hangzhou",
            CredentialsProvider = new AnonymousCredentialsProvider(),
            HttpTransport = new HttpTransport(mock)
        };
        var client = new Client(config, x =>
        {
            x.FeatureFlags &= ~FeatureFlagsType.EnableCrc64CheckDownload;
        });
        return (client, mock);
    }

    private static byte[] MakeData(int size)
    {
        var data = new byte[size];
        for (var i = 0; i < size; i++)
            data[i] = (byte)(i % 251);
        return data;
    }

    private static HttpResponseMessage Forbidden()
    {
        var xml = "<?xml version=\"1.0\"?><Error><Code>SignatureDoesNotMatch</Code><Message>bad sig</Message></Error>";
        return new HttpResponseMessage(HttpStatusCode.Forbidden)
        {
            Content = new StringContent(xml, Encoding.UTF8, "application/xml")
        };
    }

    // Append response that carries only the next-append-position (no crc header), so the mock's
    // CalcCrc64 can supply the correct crc for a first-write-from-empty.
    private static HttpResponseMessage AppendNoCrc(long nextPosition)
    {
        var resp = new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(Array.Empty<byte>()) };
        resp.Headers.TryAddWithoutValidation("x-oss-next-append-position", nextPosition.ToString());
        return resp;
    }

    private static HttpResponseMessage HeadAppendable(long size, string? hashCrc64 = "12345")
    {
        var resp = new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(Array.Empty<byte>()) };
        resp.Content.Headers.ContentLength = size;
        resp.Headers.TryAddWithoutValidation("x-oss-object-type", "Appendable");
        resp.Content.Headers.TryAddWithoutValidation("Last-Modified", "Wed, 01 Jan 2025 00:00:00 GMT");
        if (hashCrc64 != null)
            resp.Headers.TryAddWithoutValidation("x-oss-hash-crc64ecma", hashCrc64);
        return resp;
    }

    private static HttpResponseMessage HeadNormal(long size)
    {
        var resp = new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(Array.Empty<byte>()) };
        resp.Content.Headers.ContentLength = size;
        resp.Headers.TryAddWithoutValidation("x-oss-object-type", "Normal");
        return resp;
    }

    private static HttpResponseMessage NotFound()
    {
        var xml = "<?xml version=\"1.0\"?><Error><Code>NoSuchKey</Code><Message>not found</Message></Error>";
        return new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent(xml, Encoding.UTF8, "application/xml")
        };
    }

    private static HttpResponseMessage AppendOk(long nextPosition, string hashCrc64 = "999")
    {
        var resp = new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(Array.Empty<byte>()) };
        resp.Headers.TryAddWithoutValidation("x-oss-next-append-position", nextPosition.ToString());
        resp.Headers.TryAddWithoutValidation("x-oss-hash-crc64ecma", hashCrc64);
        return resp;
    }

    private static HttpResponseMessage PositionConflict()
    {
        var xml = "<?xml version=\"1.0\"?><Error><Code>PositionNotEqualToLength</Code><Message>position not equal to length</Message></Error>";
        return new HttpResponseMessage(HttpStatusCode.Conflict)
        {
            Content = new StringContent(xml, Encoding.UTF8, "application/xml")
        };
    }

    private static System.Collections.Generic.List<HttpResponseMessage> Responses(params HttpResponseMessage[] r)
        => new System.Collections.Generic.List<HttpResponseMessage>(r);

    [Fact]
    public async Task Open_NewFile_WhenNotFound()
    {
        var (client, mock) = MakeClient();
        mock.Responses = Responses(NotFound());

        using var f = await AppendOnlyFile.OpenAsync(client, "bucket", "key");
        Assert.Equal(0, f.Length);
        Assert.True(f.CanWrite);
        Assert.False(f.CanRead);
        Assert.False(f.CanSeek);
        Assert.Equal("oss://bucket/key", f.Name);
    }

    [Fact]
    public async Task Open_ExistingAppendable_StartsAtEnd()
    {
        var (client, mock) = MakeClient();
        mock.Responses = Responses(HeadAppendable(500));

        using var f = await AppendOnlyFile.OpenAsync(client, "bucket", "key");
        Assert.Equal(500, f.Length);
        Assert.Equal(500, f.Position);
    }

    [Fact]
    public async Task Open_NotAppendable_Throws()
    {
        var (client, mock) = MakeClient();
        mock.Responses = Responses(HeadNormal(10));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            AppendOnlyFile.OpenAsync(client, "bucket", "key"));
    }

    [Fact]
    public async Task Write_NewFile_AppendsAndAdvances()
    {
        var data = MakeData(1024);
        var (client, mock) = MakeClient();
        mock.Responses = Responses(NotFound(), AppendOk(data.Length));

        using var f = await AppendOnlyFile.OpenAsync(client, "bucket", "key");
        await f.WriteAsync(data, 0, data.Length);

        Assert.Equal(data.Length, f.Length);
        // First append on a fresh object must seed the crc64 with "0".
        Assert.Equal(data, mock.RequestBodies[1]);
    }

    [Fact]
    public async Task Open_ClientConvenienceMethod()
    {
        var data = MakeData(64);
        var (client, mock) = MakeClient();
        mock.Responses = Responses(NotFound(), AppendOk(data.Length));

        using var f = await client.AppendFileAsync("bucket", "key");
        await f.WriteAsync(data, 0, data.Length);

        Assert.Equal(data.Length, f.Length);
        Assert.Equal("oss://bucket/key", f.Name);
    }

    [Fact]
    public async Task Write_Multiple_Accumulates()
    {
        var a = MakeData(100);
        var b = MakeData(200);
        var (client, mock) = MakeClient();
        mock.Responses = Responses(NotFound(), AppendOk(100), AppendOk(300));

        using var f = await AppendOnlyFile.OpenAsync(client, "bucket", "key");
        await f.WriteAsync(a, 0, a.Length);
        Assert.Equal(100, f.Length);
        await f.WriteAsync(b, 0, b.Length);
        Assert.Equal(300, f.Length);
    }

    [Fact]
    public async Task WriteFrom_Stream_ReturnsBytesWritten()
    {
        var data = MakeData(2048);
        var (client, mock) = MakeClient();
        mock.Responses = Responses(NotFound(), AppendOk(data.Length));

        using var f = await AppendOnlyFile.OpenAsync(client, "bucket", "key");
        using var src = new MemoryStream(data);
        var n = await f.WriteFromAsync(src);

        Assert.Equal(data.Length, n);
        Assert.Equal(data.Length, f.Length);
    }

    [Fact]
    public async Task Write_PositionConflict_SelfRecovers()
    {
        var data = MakeData(300);
        var (client, mock) = MakeClient();
        // Open at offset 0 (new), append fails with conflict, HeadObject reports the real
        // position equals what we expected (0 + 300), so the write self-recovers.
        mock.Responses = Responses(NotFound(), PositionConflict(), HeadAppendable(300));

        using var f = await AppendOnlyFile.OpenAsync(client, "bucket", "key");
        await f.WriteAsync(data, 0, data.Length);

        Assert.Equal(300, f.Length);
    }

    [Fact]
    public async Task Write_PositionConflict_MismatchThrows()
    {
        var data = MakeData(300);
        var (client, mock) = MakeClient();
        // Real position (999) does not match expected (0 + 300) → error is surfaced.
        mock.Responses = Responses(NotFound(), PositionConflict(), HeadAppendable(999));

        using var f = await AppendOnlyFile.OpenAsync(client, "bucket", "key");
        await Assert.ThrowsAnyAsync<Exception>(() => f.WriteAsync(data, 0, data.Length));
    }

    [Fact]
    public async Task Write_AfterDispose_Throws()
    {
        var (client, mock) = MakeClient();
        mock.Responses = Responses(NotFound());

        var f = await AppendOnlyFile.OpenAsync(client, "bucket", "key");
        f.Dispose();
        Assert.Throws<ObjectDisposedException>(() => f.Write(new byte[1], 0, 1));
    }

    [Fact]
    public async Task Read_Seek_SetLength_NotSupported()
    {
        var (client, mock) = MakeClient();
        mock.Responses = Responses(NotFound());

        using var f = await AppendOnlyFile.OpenAsync(client, "bucket", "key");
        Assert.Throws<NotSupportedException>(() => f.Read(new byte[1], 0, 1));
        Assert.Throws<NotSupportedException>(() => f.Seek(0, SeekOrigin.Begin));
        Assert.Throws<NotSupportedException>(() => f.SetLength(1));
    }

    [Fact]
    public async Task Write_FirstWriteAppliesCreateParameter()
    {
        var data = MakeData(64);
        var (client, mock) = MakeClient();
        mock.Responses = Responses(NotFound(), AppendOk(data.Length));

        using var f = await AppendOnlyFile.OpenAsync(client, "bucket", "key", o =>
            o.CreateParameter = new AlibabaCloud.OSS.V2.Models.AppendObjectRequest { ContentType = "text/plain" });
        await f.WriteAsync(data, 0, data.Length);

        // The append request (second request) should carry the create-time content type.
        var appendReq = mock.Requests[1];
        Assert.Equal("text/plain", appendReq.Content!.Headers.ContentType!.MediaType);
    }

    [Fact]
    public async Task Stat_RefreshesWhenSizeStale()
    {
        var (client, mock) = MakeClient();
        // Open (head=appendable 500), then Stat triggers a refresh head because info is null.
        mock.Responses = Responses(HeadAppendable(500), HeadAppendable(500));

        using var f = await AppendOnlyFile.OpenAsync(client, "bucket", "key");
        var info = await f.StatAsync();
        Assert.Equal("oss://bucket/key", info.Name);
        Assert.Equal(500, info.Size);
    }

    [Fact]
    public async Task Open_HeadForbidden_Throws()
    {
        var (client, mock) = MakeClient();
        // A non-404 error from HeadObject must not be treated as "new file".
        mock.Responses = Responses(Forbidden());

        await Assert.ThrowsAnyAsync<Exception>(() => AppendOnlyFile.OpenAsync(client, "bucket", "key"));
    }

    [Fact]
    public async Task Write_CrcMatches_Succeeds()
    {
        var data = MakeData(1024);
        var (client, mock) = MakeClientWithUploadCrc();
        // First append seeds InitHashCrc64 with "0"; the mock computes the crc of the body from 0,
        // which is exactly the expected cumulative crc, so the check passes.
        mock.Responses = Responses(NotFound(), AppendNoCrc(data.Length));
        mock.CalcCrc64 = true;

        using var f = await AppendOnlyFile.OpenAsync(client, "bucket", "key");
        await f.WriteAsync(data, 0, data.Length);

        Assert.Equal(data.Length, f.Length);
    }

    [Fact]
    public async Task Write_CrcMismatch_Throws()
    {
        var data = MakeData(1024);
        var (client, mock) = MakeClientWithUploadCrc();
        // The server reports a crc that does not match the uploaded bytes → the write fails.
        mock.Responses = Responses(NotFound(), AppendOk(data.Length, hashCrc64: "6707180448768400016"));

        using var f = await AppendOnlyFile.OpenAsync(client, "bucket", "key");
        await Assert.ThrowsAnyAsync<Exception>(() => f.WriteAsync(data, 0, data.Length));
    }
}
