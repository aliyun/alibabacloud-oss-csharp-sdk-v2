using System.Text;
using AlibabaCloud.OSS.V2.Models;

namespace AlibabaCloud.OSS.V2.IntegrationTests;

public class ClientFileLikeTest : IDisposable
{
    private readonly string BucketNamePrefix;

    public ClientFileLikeTest()
    {
        BucketNamePrefix = Utils.RandomBucketNamePrefix();
    }

    public void Dispose()
    {
        Utils.CleanBuckets(BucketNamePrefix);
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

    private static byte[] ReadAll(Stream s, int chunk)
    {
        using var ms = new MemoryStream();
        var buf = new byte[chunk];
        int n;
        while ((n = s.Read(buf, 0, buf.Length)) > 0)
            ms.Write(buf, 0, n);
        return ms.ToArray();
    }

    private async Task<string> NewBucket(Client client)
    {
        var bucketName = Utils.RandomBucketName(BucketNamePrefix);
        var result = await client.PutBucketAsync(new PutBucketRequest { Bucket = bucketName });
        Assert.Equal(200, result.StatusCode);
        return bucketName;
    }

    [Fact]
    public async Task TestReadOnlyFileBasic()
    {
        var client = Utils.GetDefaultClient();
        var bucket = await NewBucket(client);
        var key = Utils.RandomObjectName();
        var data = MakeData(64 * 1024 + 123);

        await client.PutObjectAsync(new PutObjectRequest
        {
            Bucket = bucket,
            Key = key,
            Body = new MemoryStream(data)
        });

        using var f = await ReadOnlyFile.OpenAsync(client, bucket, key);

        Assert.True(f.CanRead);
        Assert.True(f.CanSeek);
        Assert.False(f.CanWrite);
        Assert.Equal(data.Length, f.Length);

        var info = f.Stat();
        Assert.Equal($"oss://{bucket}/{key}", info.Name);
        Assert.Equal(data.Length, info.Size);

        // Whole-object read.
        var got = ReadAll(f, 8192);
        Assert.Equal(data, got);

        // Seek then read.
        const int seekTo = 4096;
        f.Seek(seekTo, SeekOrigin.Begin);
        var got2 = ReadAll(f, 3000);
        Assert.Equal(Slice(data, seekTo), got2);
    }

    [Fact]
    public async Task TestReadOnlyFileFromOffset()
    {
        var client = Utils.GetDefaultClient();
        var bucket = await NewBucket(client);
        var key = Utils.RandomObjectName();
        var data = MakeData(20000);

        await client.PutObjectAsync(new PutObjectRequest
        {
            Bucket = bucket,
            Key = key,
            Body = new MemoryStream(data)
        });

        const int offset = 5000;
        using var f = await ReadOnlyFile.OpenAsync(client, bucket, key, o => o.Offset = offset);
        var got = ReadAll(f, 4096);
        Assert.Equal(Slice(data, offset), got);
    }

    [Fact]
    public async Task TestReadOnlyFilePrefetch()
    {
        var client = Utils.GetDefaultClient();
        var bucket = await NewBucket(client);
        var key = Utils.RandomObjectName();
        var data = MakeData(8 * 1024 * 1024 + 777);

        await client.PutObjectAsync(new PutObjectRequest
        {
            Bucket = bucket,
            Key = key,
            Body = new MemoryStream(data)
        });

        using var f = await ReadOnlyFile.OpenAsync(client, bucket, key, o =>
        {
            o.EnablePrefetch = true;
            o.PrefetchThreshold = 0; // engage prefetch immediately
            o.ChunkSize = 1024 * 1024;
            o.PrefetchNum = 3;
        });

        // Sequential read through the prefetch path.
        var got = ReadAll(f, 128 * 1024);
        Assert.Equal(data, got);

        // Random seek should still return correct data (may fall back to serial reads).
        const int seekTo = 3 * 1024 * 1024 + 5;
        f.Seek(seekTo, SeekOrigin.Begin);
        var buf = new byte[64 * 1024];
        var n = f.Read(buf, 0, buf.Length);
        Assert.True(n > 0);
        for (var i = 0; i < n; i++)
            Assert.Equal(data[seekTo + i], buf[i]);
    }

    [Fact]
    public async Task TestReadOnlyFileErrors()
    {
        var client = Utils.GetDefaultClient();
        var bucket = await NewBucket(client);
        var key = Utils.RandomObjectName();

        // Object does not exist.
        await Assert.ThrowsAnyAsync<Exception>(() => ReadOnlyFile.OpenAsync(client, bucket, key));

        // Offset beyond size.
        await client.PutObjectAsync(new PutObjectRequest
        {
            Bucket = bucket,
            Key = key,
            Body = new MemoryStream(MakeData(100))
        });
        await Assert.ThrowsAsync<ArgumentException>(() =>
            ReadOnlyFile.OpenAsync(client, bucket, key, o => o.Offset = 1000));
    }

    [Fact]
    public async Task TestAppendOnlyFileBasic()
    {
        var client = Utils.GetDefaultClient();
        var bucket = await NewBucket(client);
        var key = Utils.RandomObjectName();

        var a = MakeData(1000);
        var b = MakeData(2000);

        // Create + first append with a create parameter applied.
        using (var f = await AppendOnlyFile.OpenAsync(client, bucket, key, o =>
                   o.CreateParameter = new AppendObjectRequest { ContentType = "text/plain" }))
        {
            Assert.Equal(0, f.Length);
            await f.WriteAsync(a, 0, a.Length);
            Assert.Equal(a.Length, f.Length);
            await f.WriteAsync(b, 0, b.Length);
            Assert.Equal(a.Length + b.Length, f.Length);
        }

        // The create parameter should have been applied on creation.
        var head = await client.HeadObjectAsync(new HeadObjectRequest { Bucket = bucket, Key = key });
        Assert.Equal("Appendable", head.ObjectType);
        Assert.Equal("text/plain", head.ContentType);
        Assert.Equal(a.Length + b.Length, head.ContentLength);

        // Reopen and continue appending.
        var c = MakeData(500);
        using (var f = await AppendOnlyFile.OpenAsync(client, bucket, key))
        {
            Assert.Equal(a.Length + b.Length, f.Length);
            await f.WriteAsync(c, 0, c.Length);
        }

        // Verify the full content.
        var getResult = await client.GetObjectAsync(new GetObjectRequest { Bucket = bucket, Key = key });
        var all = ReadAll(getResult.Body!, 8192);
        var expected = new byte[a.Length + b.Length + c.Length];
        Array.Copy(a, 0, expected, 0, a.Length);
        Array.Copy(b, 0, expected, a.Length, b.Length);
        Array.Copy(c, 0, expected, a.Length + b.Length, c.Length);
        Assert.Equal(expected, all);
    }

    [Fact]
    public async Task TestAppendOnlyFileWriteFrom()
    {
        var client = Utils.GetDefaultClient();
        var bucket = await NewBucket(client);
        var key = Utils.RandomObjectName();
        var data = MakeData(300 * 1024);

        using (var f = await AppendOnlyFile.OpenAsync(client, bucket, key))
        {
            using var src = new MemoryStream(data);
            var n = await f.WriteFromAsync(src);
            Assert.Equal(data.Length, n);
            Assert.Equal(data.Length, f.Length);
        }

        var getResult = await client.GetObjectAsync(new GetObjectRequest { Bucket = bucket, Key = key });
        var all = ReadAll(getResult.Body!, 16384);
        Assert.Equal(data, all);
    }

    [Fact]
    public async Task TestAppendOnlyFileNotAppendable()
    {
        var client = Utils.GetDefaultClient();
        var bucket = await NewBucket(client);
        var key = Utils.RandomObjectName();

        // A normal (non-appendable) object.
        await client.PutObjectAsync(new PutObjectRequest
        {
            Bucket = bucket,
            Key = key,
            Body = new MemoryStream(MakeData(10))
        });

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            AppendOnlyFile.OpenAsync(client, bucket, key));
    }

    private static byte[] ReadExact(Stream s, int count)
    {
        var buf = new byte[count];
        var read = 0;
        while (read < count)
        {
            var n = s.Read(buf, read, count - read);
            if (n == 0) break;
            read += n;
        }
        if (read != count)
            throw new IOException($"expected {count} bytes, got {read}");
        return buf;
    }

    [Fact]
    public async Task TestReadOnlyFileSeekMatrix()
    {
        var client = Utils.GetDefaultClient();
        var bucket = await NewBucket(client);
        var key = Utils.RandomObjectName();
        var data = MakeData(1024 * 1024 + 4096);

        await client.PutObjectAsync(new PutObjectRequest
        {
            Bucket = bucket,
            Key = key,
            Body = new MemoryStream(data)
        });

        using var f = await ReadOnlyFile.OpenAsync(client, bucket, key);

        // Nested seek + bounded-read matrix, mirroring the Go integration test.
        for (var i = 13; i < 20; i++)
        {
            for (var len = 100 * 1024 + 123; len < 100 * 1024 + 123 + 5; len++)
            {
                f.Seek(i, SeekOrigin.Begin);
                var got = ReadExact(f, len);
                var expected = new byte[len];
                Array.Copy(data, i, expected, 0, len);
                Assert.Equal(expected, got);
            }
        }
    }

    [Fact]
    public async Task TestAppendOnlyFileCreateParameterMetadata()
    {
        var client = Utils.GetDefaultClient();
        var bucket = await NewBucket(client);
        var key = Utils.RandomObjectName();

        using (var f = await AppendOnlyFile.OpenAsync(client, bucket, key, o =>
                   o.CreateParameter = new AppendObjectRequest
                   {
                       Acl = "private",
                       CacheControl = "no-cache",
                       Metadata = new Dictionary<string, string> { ["user"] = "jack" },
                       Tagging = "key=value"
                   }))
        {
            var a = Encoding.UTF8.GetBytes("hello");
            var b = Encoding.UTF8.GetBytes(" world");
            await f.WriteAsync(a, 0, a.Length);
            await f.WriteAsync(b, 0, b.Length);

            using var src = new MemoryStream(Encoding.UTF8.GetBytes(" 123"));
            var written = await f.WriteFromAsync(src);
            Assert.Equal(4, written);

            Assert.Equal(15, f.Length);
        }

        // The create-time parameters should have been applied to the object.
        var head = await client.HeadObjectAsync(new HeadObjectRequest { Bucket = bucket, Key = key });
        Assert.Equal("Appendable", head.ObjectType);
        Assert.Equal(15, head.ContentLength);
        Assert.Equal("no-cache", head.CacheControl);
        Assert.NotNull(head.Metadata);
        Assert.Equal("jack", head.Metadata!["user"]);
        Assert.Equal(1, head.TaggingCount);

        // Full content is correct.
        var getResult = await client.GetObjectAsync(new GetObjectRequest { Bucket = bucket, Key = key });
        var all = ReadAll(getResult.Body!, 8192);
        Assert.Equal(Encoding.UTF8.GetBytes("hello world 123"), all);
    }
}
