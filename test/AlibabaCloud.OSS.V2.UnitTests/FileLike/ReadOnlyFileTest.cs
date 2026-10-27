using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AlibabaCloud.OSS.V2;
using AlibabaCloud.OSS.V2.Credentials;
using AlibabaCloud.OSS.V2.Transport;

namespace AlibabaCloud.OSS.V2.UnitTests.FileLike;

public class ReadOnlyFileTest
{
    private const string Etag = "\"5EB63BBBE01EEED093CB22BB8F5ACDC3\"";
    private const string LastModified = "Wed, 01 Jan 2025 00:00:00 GMT";

    private static (Client, MockHttpMessageHandler) MakeClient()
    {
        var mock = new MockHttpMessageHandler();
        return (MakeClient(mock), mock);
    }

    private static Client MakeClient(HttpMessageHandler handler)
    {
        var config = new Configuration
        {
            Region = "cn-hangzhou",
            CredentialsProvider = new AnonymousCredentialsProvider(),
            HttpTransport = new HttpTransport(handler)
        };
        return new Client(config, x =>
        {
            x.FeatureFlags &= ~(FeatureFlagsType.EnableCrc64CheckDownload | FeatureFlagsType.EnableCrc64CheckUpload);
        });
    }

    private static byte[] MakeData(int size)
    {
        var data = new byte[size];
        for (var i = 0; i < size; i++)
            data[i] = (byte)(i % 251);
        return data;
    }

    private static byte[] Slice(byte[] src, long offset)
    {
        var len = src.Length - (int)offset;
        var dst = new byte[len];
        Array.Copy(src, offset, dst, 0, len);
        return dst;
    }

    private static HttpResponseMessage HeadResponse(long size, string etag = Etag, string lastModified = LastModified)
    {
        var resp = new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(Array.Empty<byte>()) };
        resp.Content.Headers.ContentLength = size;
        resp.Headers.TryAddWithoutValidation("ETag", etag);
        resp.Content.Headers.TryAddWithoutValidation("Last-Modified", lastModified);
        return resp;
    }

    private static HttpResponseMessage GetResponse(byte[] full, long offset, string etag = Etag, string lastModified = LastModified)
    {
        var slice = Slice(full, offset);
        var resp = new HttpResponseMessage(HttpStatusCode.PartialContent) { Content = new ByteArrayContent(slice) };
        var last = full.Length - 1;
        resp.Content.Headers.TryAddWithoutValidation("Content-Range", $"bytes {offset}-{last}/{full.Length}");
        resp.Headers.TryAddWithoutValidation("ETag", etag);
        resp.Content.Headers.TryAddWithoutValidation("Last-Modified", lastModified);
        return resp;
    }

    private static byte[] ReadAll(Stream s, int chunk)
    {
        using var ms = new MemoryStream();
        var buf = new byte[chunk];
        while (true)
        {
            var n = s.Read(buf, 0, buf.Length);
            if (n == 0) break;
            ms.Write(buf, 0, n);
        }
        return ms.ToArray();
    }

    // ReadAsync may legally return fewer bytes than requested, so callers loop to fill.
    private static int ReadExactly(Stream s, byte[] buffer, int count)
    {
        var read = 0;
        while (read < count)
        {
            var n = s.Read(buffer, read, count - read);
            if (n == 0) break;
            read += n;
        }
        return read;
    }

    private static void AssertBytesEqual(byte[] expected, int offset, byte[] actual)
    {
        Assert.True(offset + actual.Length <= expected.Length);
        for (var i = 0; i < actual.Length; i++)
            Assert.Equal(expected[offset + i], actual[i]);
    }

    [Fact]
    public async Task Open_StatAndProperties()
    {
        var (client, mock) = MakeClient();
        mock.Responses = new System.Collections.Generic.List<HttpResponseMessage> { HeadResponse(1234) };

        using var f = await ReadOnlyFile.OpenAsync(client, "bucket", "key.txt");

        Assert.True(f.CanRead);
        Assert.True(f.CanSeek);
        Assert.False(f.CanWrite);
        Assert.Equal(1234, f.Length);
        Assert.Equal(0, f.Position);

        var info = f.Stat();
        Assert.Equal("oss://bucket/key.txt", info.Name);
        Assert.Equal(1234, info.Size);
        Assert.NotNull(info.LastModified);
    }

    [Fact]
    public async Task Open_ClientConvenienceMethod()
    {
        var data = MakeData(1024);
        var (client, mock) = MakeClient();
        mock.Responses = new System.Collections.Generic.List<HttpResponseMessage> { HeadResponse(data.Length), GetResponse(data, 0) };

        using var f = await client.OpenFileAsync("bucket", "key", o => o.VersionId = "v1");
        Assert.Equal("oss://bucket/key?versionId=v1", f.Name);
        Assert.Equal(data, ReadAll(f, 256));
        Assert.Contains("versionId=v1", mock.LastRequest.RequestUri!.ToString());
    }

    [Fact]
    public async Task Open_WithVersionId_NameIncludesVersion()
    {
        var (client, mock) = MakeClient();
        mock.Responses = new System.Collections.Generic.List<HttpResponseMessage> { HeadResponse(10) };

        using var f = await ReadOnlyFile.OpenAsync(client, "bkt", "obj", o => o.VersionId = "v123");
        Assert.Equal("oss://bkt/obj?versionId=v123", f.Name);
    }

    [Fact]
    public async Task Open_OffsetBeyondSize_Throws()
    {
        var (client, mock) = MakeClient();
        mock.Responses = new System.Collections.Generic.List<HttpResponseMessage> { HeadResponse(100) };

        await Assert.ThrowsAsync<ArgumentException>(() =>
            ReadOnlyFile.OpenAsync(client, "bucket", "key", o => o.Offset = 200));
    }

    [Fact]
    public async Task Read_WholeObject()
    {
        var data = MakeData(64 * 1024 + 7);
        var (client, mock) = MakeClient();
        mock.Responses = new System.Collections.Generic.List<HttpResponseMessage> { HeadResponse(data.Length), GetResponse(data, 0) };

        using var f = await ReadOnlyFile.OpenAsync(client, "bucket", "key");
        var got = ReadAll(f, 8192);

        Assert.Equal(data, got);
        Assert.Equal(data.Length, f.Position);
    }

    [Fact]
    public async Task Read_FromOffsetOption()
    {
        var data = MakeData(4096);
        const int offset = 1000;
        var (client, mock) = MakeClient();
        mock.Responses = new System.Collections.Generic.List<HttpResponseMessage> { HeadResponse(data.Length), GetResponse(data, offset) };

        using var f = await ReadOnlyFile.OpenAsync(client, "bucket", "key", o => o.Offset = offset);
        var got = ReadAll(f, 512);

        Assert.Equal(Slice(data, offset), got);
    }

    [Fact]
    public async Task Seek_ThenRead()
    {
        var data = MakeData(4096);
        const int seekTo = 2048;
        var (client, mock) = MakeClient();
        mock.Responses = new System.Collections.Generic.List<HttpResponseMessage> { HeadResponse(data.Length), GetResponse(data, seekTo) };

        using var f = await ReadOnlyFile.OpenAsync(client, "bucket", "key");
        var pos = f.Seek(seekTo, SeekOrigin.Begin);
        Assert.Equal(seekTo, pos);

        var got = ReadAll(f, 700);
        Assert.Equal(Slice(data, seekTo), got);
    }

    [Fact]
    public async Task Seek_EndAndCurrent()
    {
        var (client, mock) = MakeClient();
        mock.Responses = new System.Collections.Generic.List<HttpResponseMessage> { HeadResponse(1000) };

        using var f = await ReadOnlyFile.OpenAsync(client, "bucket", "key");
        Assert.Equal(1000, f.Seek(0, SeekOrigin.End));
        Assert.Equal(900, f.Seek(-100, SeekOrigin.Current));
        Assert.Equal(950, f.Seek(50, SeekOrigin.Current));
    }

    [Fact]
    public async Task Seek_NegativeThrows()
    {
        var (client, mock) = MakeClient();
        mock.Responses = new System.Collections.Generic.List<HttpResponseMessage> { HeadResponse(1000) };

        using var f = await ReadOnlyFile.OpenAsync(client, "bucket", "key");
        Assert.Throws<IOException>(() => f.Seek(-1, SeekOrigin.Begin));
    }

    [Fact]
    public async Task Seek_BeyondSizeThrows()
    {
        var (client, mock) = MakeClient();
        mock.Responses = new System.Collections.Generic.List<HttpResponseMessage> { HeadResponse(1000) };

        using var f = await ReadOnlyFile.OpenAsync(client, "bucket", "key");
        Assert.Throws<IOException>(() => f.Seek(1001, SeekOrigin.Begin));
    }

    [Fact]
    public async Task Read_SourceChanged_Throws()
    {
        var data = MakeData(2048);
        var (client, mock) = MakeClient();
        // GET returns a different ETag than HEAD.
        mock.Responses = new System.Collections.Generic.List<HttpResponseMessage> { HeadResponse(data.Length), GetResponse(data, 0, etag: "\"CHANGED\"") };

        using var f = await ReadOnlyFile.OpenAsync(client, "bucket", "key");
        var buf = new byte[1024];
        var ex = Assert.Throws<IOException>(() => f.Read(buf, 0, buf.Length));
        Assert.Contains("Source file is changed", ex.Message);
    }

    [Fact]
    public async Task Read_RangeOffsetMismatch_Throws()
    {
        var data = MakeData(2048);
        var (client, mock) = MakeClient();
        // Requested offset 0 but the server reports it started at 5.
        var badGet = new HttpResponseMessage(HttpStatusCode.PartialContent)
        {
            Content = new ByteArrayContent(Slice(data, 0))
        };
        badGet.Content.Headers.TryAddWithoutValidation("Content-Range", $"bytes 5-{data.Length - 1}/{data.Length}");
        badGet.Headers.TryAddWithoutValidation("ETag", Etag);
        badGet.Headers.TryAddWithoutValidation("Last-Modified", LastModified);
        mock.Responses = new System.Collections.Generic.List<HttpResponseMessage> { HeadResponse(data.Length), badGet };

        using var f = await ReadOnlyFile.OpenAsync(client, "bucket", "key");
        var buf = new byte[1024];
        var ex = Assert.Throws<IOException>(() => f.Read(buf, 0, buf.Length));
        Assert.Contains("Range get fail", ex.Message);
    }

    [Fact]
    public async Task Read_AfterDispose_Throws()
    {
        var (client, mock) = MakeClient();
        mock.Responses = new System.Collections.Generic.List<HttpResponseMessage> { HeadResponse(100) };

        var f = await ReadOnlyFile.OpenAsync(client, "bucket", "key");
        f.Dispose();
        var buf = new byte[10];
        Assert.Throws<ObjectDisposedException>(() => f.Read(buf, 0, buf.Length));
    }

    [Fact]
    public async Task Write_NotSupported()
    {
        var (client, mock) = MakeClient();
        mock.Responses = new System.Collections.Generic.List<HttpResponseMessage> { HeadResponse(100) };

        using var f = await ReadOnlyFile.OpenAsync(client, "bucket", "key");
        Assert.Throws<NotSupportedException>(() => f.Write(new byte[1], 0, 1));
        Assert.Throws<NotSupportedException>(() => f.SetLength(10));
    }

    // ---- Dynamic-handler tests (mirror Go's httptest-based mock tests) ----

    [Fact]
    public async Task DirectRead_WholeObjectAndSeekPatterns()
    {
        var data = MakeData(3 * 1024 * 1024 + 1234);
        var handler = new FileServerHandler(data);
        var client = MakeClient(handler);

        using var f = await ReadOnlyFile.OpenAsync(client, "bucket", "key");
        Assert.Equal(data.Length, f.Length);

        var whole = ReadAll(f, 64 * 1024);
        Assert.Equal(data, whole);

        // Seek back to a series of offsets and read a slice; verify against source bytes.
        var rnd = new Random(12345);
        for (var i = 0; i < 32; i++)
        {
            var pos = rnd.Next(0, data.Length - 1);
            var len = Math.Min(rnd.Next(1, 200 * 1024), data.Length - pos);
            f.Seek(pos, SeekOrigin.Begin);
            var buf = new byte[len];
            var read = 0;
            while (read < len)
            {
                var n = f.Read(buf, read, len - read);
                if (n == 0) break;
                read += n;
            }
            Assert.Equal(len, read);
            for (var j = 0; j < len; j++)
                Assert.Equal(data[pos + j], buf[j]);
        }

        // Seek to end returns zero bytes.
        f.Seek(0, SeekOrigin.End);
        Assert.Equal(0, f.Read(new byte[16], 0, 16));
    }

    [Fact]
    public async Task PrefetchRead_WholeObjectAndSeek()
    {
        var data = MakeData(11 * 1024 * 1024 + 999);
        var handler = new FileServerHandler(data);
        var client = MakeClient(handler);

        using var f = await ReadOnlyFile.OpenAsync(client, "bucket", "key", o =>
        {
            o.EnablePrefetch = true;
            o.ChunkSize = 2 * 1024 * 1024;
            o.PrefetchNum = 3;
            o.PrefetchThreshold = 0;
        });

        var whole = ReadAll(f, 128 * 1024);
        Assert.Equal(data, whole);

        const int seekTo = 5 * 1024 * 1024 + 7;
        f.Seek(seekTo, SeekOrigin.Begin);
        var buf = new byte[256 * 1024];
        var read = 0;
        while (read < buf.Length)
        {
            var n = f.Read(buf, read, buf.Length - read);
            if (n == 0) break;
            read += n;
        }
        Assert.Equal(buf.Length, read);
        for (var j = 0; j < buf.Length; j++)
            Assert.Equal(data[seekTo + j], buf[j]);
    }

    [Fact]
    public async Task PrefetchRead_OptionsNormalized_ReadsData()
    {
        var data = MakeData(4 * 1024 * 1024 + 321);
        var handler = new FileServerHandler(data);
        var client = MakeClient(handler);

        // Negative/zero options must be normalized to defaults instead of failing.
        using var f = await ReadOnlyFile.OpenAsync(client, "bucket", "key", o =>
        {
            o.EnablePrefetch = true;
            o.ChunkSize = -1;
            o.PrefetchNum = -1;
            o.OutOfOrderReadThreshold = -1;
            o.PrefetchThreshold = 0;
        });

        var whole = ReadAll(f, 64 * 1024);
        Assert.Equal(data, whole);
    }

    [Fact]
    public async Task PrefetchRead_FallsBackToDirectOnBoundedFailure()
    {
        var data = MakeData(5 * 1024 * 1024 + 123);
        var handler = new FileServerHandler(data) { FailBoundedRange = true };
        var client = MakeClient(handler);

        using var f = await ReadOnlyFile.OpenAsync(client, "bucket", "key", o =>
        {
            o.EnablePrefetch = true;
            o.ChunkSize = 1 * 1024 * 1024;
            o.PrefetchNum = 3;
            o.PrefetchThreshold = 0;
        });

        // Prefetch (bounded ranges) fails; the reader must fall back to open-ended direct GETs.
        var whole = ReadAll(f, 128 * 1024);
        Assert.Equal(data, whole);
    }

    [Fact]
    public async Task PrefetchRead_SourceChanged_Throws()
    {
        var data = MakeData(4 * 1024 * 1024);
        var handler = new FileServerHandler(data) { GetEtag = "\"etag-changed\"" };
        var client = MakeClient(handler);

        using var f = await ReadOnlyFile.OpenAsync(client, "bucket", "key", o =>
        {
            o.EnablePrefetch = true;
            o.ChunkSize = 1 * 1024 * 1024;
            o.PrefetchNum = 3;
            o.PrefetchThreshold = 0;
        });

        var buf = new byte[128 * 1024];
        var ex = Assert.Throws<IOException>(() =>
        {
            while (f.Read(buf, 0, buf.Length) > 0) { }
        });
        Assert.Contains("Source file is changed", ex.Message);
    }

    [Fact]
    public async Task PrefetchRead_Resume_AfterBrokenBody()
    {
        var data = MakeData(6 * 1024 * 1024 + 55);
        var handler = new FileServerHandler(data) { HalfBodyOnce = true };
        var client = MakeClient(handler);

        using var f = await ReadOnlyFile.OpenAsync(client, "bucket", "key", o =>
        {
            o.EnablePrefetch = true;
            o.ChunkSize = 2 * 1024 * 1024;
            o.PrefetchNum = 3;
            o.PrefetchThreshold = 0;
        });

        var whole = ReadAll(f, 128 * 1024);
        Assert.Equal(data, whole);
    }

    [Fact]
    public async Task DirectRead_TransientBodyFailure_Recovers()
    {
        var data = MakeData(2 * 1024 * 1024 + 99);
        var handler = new FileServerHandler(data) { HalfBodyOnce = true };
        var client = MakeClient(handler);

        // Prefetch is off: a broken body mid-stream must be retried from the same offset.
        using var f = await ReadOnlyFile.OpenAsync(client, "bucket", "key");
        var whole = ReadAll(f, 64 * 1024);

        Assert.Equal(data, whole);
        Assert.Equal(data.Length, f.Position);
        Assert.True(handler.GetCount >= 2, $"expected a retry GET, got {handler.GetCount}");
    }

    [Fact]
    public async Task PrefetchRead_OutOfOrderThreshold_DisablesPrefetch()
    {
        var data = MakeData(11 * 1024 * 1024 + 13435);
        var handler = new FileServerHandler(data);
        var client = MakeClient(handler);

        using var f = await ReadOnlyFile.OpenAsync(client, "bucket", "key", o =>
        {
            o.EnablePrefetch = true;
            o.ChunkSize = 2 * 1024 * 1024;
            o.PrefetchNum = 3;
            o.PrefetchThreshold = 0;
            o.OutOfOrderReadThreshold = 3;
        });

        // The first sequential read starts prefetch, which issues bounded chunk GETs.
        var head = new byte[64 * 1024];
        Assert.Equal(head.Length, ReadExactly(f, head, head.Length));
        Assert.Contains(handler.Ranges, r => r.Start == 0 && r.End >= 0);

        // Each seek away from the prefetch cursor counts as an out-of-order read. Once the
        // threshold is exhausted the reader must stop prefetching and read directly.
        var seeks = new long[]
        {
            2L * 1024 * 1024,
            4L * 1024 * 1024,
            6L * 1024 * 1024,
            8L * 1024 * 1024,
        };
        foreach (var pos in seeks)
        {
            f.Seek(pos, SeekOrigin.Begin);
            var chunk = new byte[16 * 1024];
            Assert.Equal(chunk.Length, ReadExactly(f, chunk, chunk.Length));
            AssertBytesEqual(data, (int)pos, chunk);
        }

        const long target = 10L * 1024 * 1024 + 123;
        f.Seek(target, SeekOrigin.Begin);
        var tail = new byte[8 * 1024];
        Assert.Equal(tail.Length, ReadExactly(f, tail, tail.Length));
        AssertBytesEqual(data, (int)target, tail);

        // The GET at the post-threshold offset must be open-ended (direct), not a bounded chunk.
        var atTarget = handler.Ranges.Where(r => r.Start == target).ToList();
        Assert.NotEmpty(atTarget);
        Assert.All(atTarget, r => Assert.True(
            r.End < 0, $"expected open-ended direct GET at {target}, got bounded bytes={r.Start}-{r.End}"));
    }

    [Fact]
    public async Task PrefetchRead_MixSequentialAndRandom()
    {
        var data = MakeData(9 * 1024 * 1024 + 321);
        var handler = new FileServerHandler(data);
        var client = MakeClient(handler);

        using var f = await ReadOnlyFile.OpenAsync(client, "bucket", "key", o =>
        {
            o.EnablePrefetch = true;
            o.ChunkSize = 1024 * 1024;
            o.PrefetchNum = 3;
            o.PrefetchThreshold = 0;
        });

        // Sequential prefix, crossing several prefetch chunk boundaries.
        const int prefix = 3 * 1024 * 1024 + 7;
        var head = new byte[prefix];
        Assert.Equal(prefix, ReadExactly(f, head, prefix));
        AssertBytesEqual(data, 0, head);

        // Interleave backward/forward seeks with reads that may land inside or outside the
        // prefetched window; every read must return the right bytes.
        var rnd = new Random(20250808);
        for (var i = 0; i < 20; i++)
        {
            var pos = rnd.Next(0, data.Length - 4096);
            var len = Math.Min(rnd.Next(1, 256 * 1024), data.Length - pos);
            f.Seek(pos, SeekOrigin.Begin);
            var buf = new byte[len];
            Assert.Equal(len, ReadExactly(f, buf, len));
            AssertBytesEqual(data, pos, buf);
        }

        // Finish sequentially from wherever the random phase left the cursor.
        var resume = (int)f.Position;
        var rest = ReadAll(f, 100 * 1024);
        AssertBytesEqual(data, resume, rest);
        Assert.Equal(data.Length, f.Position);
    }

    [Fact]
    public async Task DirectRead_OffsetMismatch_Throws()
    {
        var data = MakeData(4096);
        var handler = new FileServerHandler(data) { SuppressContentRange = true };
        var client = MakeClient(handler);

        using var f = await ReadOnlyFile.OpenAsync(client, "bucket", "key");
        // Seek to a non-zero offset: the server omits Content-Range, so the parsed offset (0)
        // will not match the requested offset.
        f.Seek(1000, SeekOrigin.Begin);
        var buf = new byte[512];
        var ex = Assert.Throws<IOException>(() => f.Read(buf, 0, buf.Length));
        Assert.Contains("Range get fail", ex.Message);
    }

    [Fact]
    public async Task Close_Idempotent_AndAfterCloseThrows()
    {
        var data = MakeData(2048);
        var handler = new FileServerHandler(data);
        var client = MakeClient(handler);

        var f = await ReadOnlyFile.OpenAsync(client, "bucket", "key");
        ReadAll(f, 512);
        f.Dispose();
        f.Dispose(); // idempotent

        Assert.Throws<ObjectDisposedException>(() => f.Seek(0, SeekOrigin.Begin));
        Assert.Throws<ObjectDisposedException>(() => f.Stat());
        Assert.Throws<ObjectDisposedException>(() => f.Read(new byte[8], 0, 8));
    }

    [Fact]
    public async Task Open_SizeInvalid_Throws()
    {
        var (client, mock) = MakeClient();
        // HEAD without a Content-Length header yields an unknown size.
        var head = new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(Array.Empty<byte>()) };
        head.Content.Headers.ContentLength = null;
        head.Headers.TryAddWithoutValidation("ETag", Etag);
        mock.Responses = new System.Collections.Generic.List<HttpResponseMessage> { head };

        await Assert.ThrowsAsync<ArgumentException>(() => ReadOnlyFile.OpenAsync(client, "bucket", "key"));
    }

    [Fact]
    public async Task Open_VersionId_RequestIncludesVersionId()
    {
        var (client, mock) = MakeClient();
        mock.Responses = new System.Collections.Generic.List<HttpResponseMessage> { HeadResponse(10) };

        using var f = await ReadOnlyFile.OpenAsync(client, "bkt", "obj", o => o.VersionId = "v123");
        Assert.Contains("versionId=v123", mock.LastRequest.RequestUri!.ToString());
    }
}
