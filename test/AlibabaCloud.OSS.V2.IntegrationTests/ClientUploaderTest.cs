using System.Linq;
using System.Text;
using AlibabaCloud.OSS.V2.Models;

namespace AlibabaCloud.OSS.V2.IntegrationTests;

public class ClientUploaderTest : IDisposable
{
    private readonly string BucketNamePrefix;

    public void Dispose()
    {
        Utils.CleanBuckets(BucketNamePrefix);
    }

    public ClientUploaderTest()
    {
        BucketNamePrefix = Utils.RandomBucketNamePrefix();
    }

    private async Task<string> NewBucketAsync(Client client)
    {
        var bucketName = Utils.RandomBucketName(BucketNamePrefix);
        var result = await client.PutBucketAsync(new PutBucketRequest { Bucket = bucketName });
        Assert.Equal(200, result.StatusCode);
        return bucketName;
    }

    [Fact]
    public async Task TestUploadStreamSmall()
    {
        var client = Utils.GetDefaultClient();
        var bucketName = await NewBucketAsync(client);
        var objectName = Utils.RandomObjectName();

        var content = "hello uploader " + Utils.GetRandomString(64);
        var data = Encoding.UTF8.GetBytes(content);

        var uploader = new Uploader(client);
        var result = await uploader.UploadStreamAsync(
            new PutObjectRequest { Bucket = bucketName, Key = objectName },
            new MemoryStream(data));

        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.RequestId);
        Assert.NotNull(result.ETag);

        // Verify the stored content matches.
        var getResult = await client.GetObjectAsync(new GetObjectRequest { Bucket = bucketName, Key = objectName });
        Assert.Equal(data.Length, getResult.ContentLength);
        using var reader = new StreamReader(getResult.Body!);
        Assert.Equal(content, await reader.ReadToEndAsync());
    }

    [Fact]
    public async Task TestUploadFileMultipartWithProgress()
    {
        var client = Utils.GetDefaultClient();
        var bucketName = await NewBucketAsync(client);
        var objectName = Utils.RandomObjectName();

        var filePath = Utils.GetTempFileName();
        Utils.PrepareSampleFile(filePath, 500); // ~500 KiB
        try
        {
            var fileSize = new FileInfo(filePath).Length;

            long lastTransferred = 0;
            long lastTotal = 0;
            var uploader = new Uploader(client, o =>
            {
                o.PartSize = 100 * 1024; // force multipart (min part size)
                o.ParallelNum = 3;
            });

            var request = new PutObjectRequest
            {
                Bucket = bucketName,
                Key = objectName,
                ProgressFn = (increment, transferred, total) =>
                {
                    Interlocked.Exchange(ref lastTransferred, transferred);
                    Interlocked.Exchange(ref lastTotal, total);
                }
            };

            var result = await uploader.UploadFileAsync(request, filePath);

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.NotNull(result.UploadId); // multipart path
            Assert.NotNull(result.ETag);
            Assert.Equal(fileSize, lastTransferred);
            Assert.Equal(fileSize, lastTotal);

            // Verify size and content via HeadObject + download.
            var head = await client.HeadObjectAsync(new HeadObjectRequest { Bucket = bucketName, Key = objectName });
            Assert.Equal(fileSize, head.ContentLength);

            var getResult = await client.GetObjectAsync(new GetObjectRequest { Bucket = bucketName, Key = objectName });
            Assert.Equal(Utils.ComputeContentMd5(filePath), Utils.ComputeContentMd5(getResult.Body!));
        }
        finally
        {
            Utils.CleanPath(filePath);
        }
    }

    [Fact]
    public async Task TestUploadFileSinglePut()
    {
        var client = Utils.GetDefaultClient();
        var bucketName = await NewBucketAsync(client);
        var objectName = Utils.RandomObjectName();

        var filePath = Utils.GetTempFileName();
        Utils.PrepareSampleFile(filePath, 10); // ~10 KiB, below default part size
        try
        {
            var fileSize = new FileInfo(filePath).Length;
            var uploader = new Uploader(client);
            var result = await uploader.UploadFileAsync(
                new PutObjectRequest { Bucket = bucketName, Key = objectName }, filePath);

            Assert.Equal(200, result.StatusCode);
            Assert.Null(result.UploadId); // single PutObject path
            Assert.NotNull(result.ETag);

            var head = await client.HeadObjectAsync(new HeadObjectRequest { Bucket = bucketName, Key = objectName });
            Assert.Equal(fileSize, head.ContentLength);
        }
        finally
        {
            Utils.CleanPath(filePath);
        }
    }

    [Fact]
    public async Task TestUploadFileCheckpointResume()
    {
        var client = Utils.GetDefaultClient();
        var bucketName = await NewBucketAsync(client);
        var objectName = Utils.RandomObjectName();

        var filePath = Utils.GetTempFileName();
        Utils.PrepareSampleFile(filePath, 500);
        var cpDir = Utils.GetTempPath();
        Directory.CreateDirectory(cpDir);
        try
        {
            var fileSize = new FileInfo(filePath).Length;

            // First pass: cancel midway to leave a checkpoint on disk.
            var cts = new CancellationTokenSource();
            long transferred = 0;
            var uploader = new Uploader(client, o =>
            {
                o.PartSize = 100 * 1024;
                o.ParallelNum = 1;
                o.EnableCheckpoint = true;
                o.CheckpointDir = cpDir;
            });

            var request = new PutObjectRequest
            {
                Bucket = bucketName,
                Key = objectName,
                ProgressFn = (increment, current, total) =>
                {
                    if (Interlocked.Add(ref transferred, increment) >= 200 * 1024)
                        cts.Cancel();
                }
            };

            await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
                uploader.UploadFileAsync(request, filePath, null, cts.Token));

            // A checkpoint file should remain.
            Assert.NotEmpty(Directory.GetFiles(cpDir, "*.ucp"));

            // Second pass: resume to completion.
            var request2 = new PutObjectRequest { Bucket = bucketName, Key = objectName };
            var result = await uploader.UploadFileAsync(request2, filePath);
            Assert.Equal(200, result.StatusCode);

            var head = await client.HeadObjectAsync(new HeadObjectRequest { Bucket = bucketName, Key = objectName });
            Assert.Equal(fileSize, head.ContentLength);

            // Checkpoint removed after success.
            Assert.Empty(Directory.GetFiles(cpDir, "*.ucp"));
        }
        finally
        {
            Utils.CleanPath(filePath);
            Utils.CleanPath(cpDir);
        }
    }

    [Fact]
    public async Task TestUploadFileNotExists()
    {
        var client = Utils.GetDefaultClient();
        var uploader = new Uploader(client);
        var req = new PutObjectRequest { Bucket = "bucket", Key = "key" };
        await Assert.ThrowsAsync<FileNotFoundException>(() =>
            uploader.UploadFileAsync(req, Path.Combine(Path.GetTempPath(), "no-such-" + Guid.NewGuid())));
    }

    [Fact]
    public async Task TestUploadStreamEmpty()
    {
        var client = Utils.GetDefaultClient();
        var bucketName = await NewBucketAsync(client);
        var objectName = Utils.RandomObjectName();

        var uploader = new Uploader(client);
        var result = await uploader.UploadStreamAsync(
            new PutObjectRequest { Bucket = bucketName, Key = objectName },
            new MemoryStream(Array.Empty<byte>()));

        Assert.Equal(200, result.StatusCode);
        Assert.Null(result.UploadId); // single PutObject path

        var head = await client.HeadObjectAsync(new HeadObjectRequest { Bucket = bucketName, Key = objectName });
        Assert.Equal(0, head.ContentLength);
    }

    [Fact]
    public async Task TestUploadStreamMultipartSequential()
    {
        var client = Utils.GetDefaultClient();
        var bucketName = await NewBucketAsync(client);
        var objectName = Utils.RandomObjectName();

        var data = Encoding.UTF8.GetBytes(Utils.GetRandomString(350 * 1024));
        var uploader = new Uploader(client, o =>
        {
            o.PartSize = 100 * 1024;
            o.ParallelNum = 1;
        });

        var result = await uploader.UploadStreamAsync(
            new PutObjectRequest { Bucket = bucketName, Key = objectName },
            new MemoryStream(data));

        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.UploadId); // multipart path

        var head = await client.HeadObjectAsync(new HeadObjectRequest { Bucket = bucketName, Key = objectName });
        Assert.Equal(data.Length, head.ContentLength);

        var getResult = await client.GetObjectAsync(new GetObjectRequest { Bucket = bucketName, Key = objectName });
        Assert.Equal(Utils.ComputeContentMd5(new MemoryStream(data)), Utils.ComputeContentMd5(getResult.Body!));
    }

    [Fact]
    public async Task TestUploadStreamWithTagging()
    {
        var client = Utils.GetDefaultClient();
        var bucketName = await NewBucketAsync(client);
        var objectName = Utils.RandomObjectName();

        var data = Encoding.UTF8.GetBytes(Utils.GetRandomString(250 * 1024));
        var uploader = new Uploader(client, o => o.PartSize = 100 * 1024);

        var result = await uploader.UploadStreamAsync(
            new PutObjectRequest { Bucket = bucketName, Key = objectName, Tagging = "k1=v1&k2=v2" },
            new MemoryStream(data));

        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.UploadId); // multipart path carries tagging via Initiate

        var tagging = await client.GetObjectTaggingAsync(
            new GetObjectTaggingRequest { Bucket = bucketName, Key = objectName });
        Assert.NotNull(tagging.Tagging?.TagSet?.Tags);
        var tags = tagging.Tagging!.TagSet!.Tags!;
        Assert.Equal(2, tags.Count);
        Assert.Equal("v1", tags.Single(t => t.Key == "k1").Value);
        Assert.Equal("v2", tags.Single(t => t.Key == "k2").Value);
    }

    [Fact]
    public async Task TestUploadStreamInvalidAk()
    {
        // Create the bucket with a valid client so routing succeeds and the AK check runs.
        var bucketName = await NewBucketAsync(Utils.GetDefaultClient());

        var client = Utils.GetInvalidAkClient();
        var uploader = new Uploader(client);
        var req = new PutObjectRequest { Bucket = bucketName, Key = "key" };
        var ex = await Assert.ThrowsAsync<OperationException>(() =>
            uploader.UploadStreamAsync(req, new MemoryStream(Encoding.UTF8.GetBytes("data"))));
        var se = Assert.IsAssignableFrom<ServiceException>(ex.InnerException);
        Assert.Equal(403, se.StatusCode);
        Assert.Equal("InvalidAccessKeyId", se.ErrorCode);
    }
}
