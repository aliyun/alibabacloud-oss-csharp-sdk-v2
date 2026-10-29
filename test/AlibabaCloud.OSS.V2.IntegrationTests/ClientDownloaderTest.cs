using System.Text;
using AlibabaCloud.OSS.V2.Models;

namespace AlibabaCloud.OSS.V2.IntegrationTests;

public class ClientDownloaderTest : IDisposable
{
    private readonly string BucketNamePrefix;

    public void Dispose()
    {
        Utils.CleanBuckets(BucketNamePrefix);
    }

    public ClientDownloaderTest()
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

    private static async Task PutFileAsync(Client client, string bucket, string key, string filePath)
    {
        using var fs = File.OpenRead(filePath);
        var result = await client.PutObjectAsync(new PutObjectRequest { Bucket = bucket, Key = key, Body = fs });
        Assert.Equal(200, result.StatusCode);
    }

    [Fact]
    public async Task TestDownloadFileMultipartWithProgress()
    {
        var client = Utils.GetDefaultClient();
        var bucketName = await NewBucketAsync(client);
        var objectName = Utils.RandomObjectName();

        var srcPath = Utils.GetTempFileName();
        var dstPath = Utils.GetTempFileName();
        Utils.PrepareSampleFile(srcPath, 500);
        try
        {
            await PutFileAsync(client, bucketName, objectName, srcPath);
            var fileSize = new FileInfo(srcPath).Length;

            long lastTransferred = 0;
            var downloader = new Downloader(client, o =>
            {
                o.PartSize = 100 * 1024;
                o.ParallelNum = 3;
            });

            var request = new GetObjectRequest
            {
                Bucket = bucketName,
                Key = objectName,
                ProgressFn = (increment, transferred, total) => Interlocked.Exchange(ref lastTransferred, transferred)
            };

            var result = await downloader.DownloadFileAsync(request, dstPath);

            Assert.NotNull(result);
            Assert.Equal(fileSize, result.Written);
            Assert.Equal(fileSize, lastTransferred);

            // The temp file must have been renamed to the final path.
            Assert.True(File.Exists(dstPath));
            Assert.False(File.Exists(dstPath + Defaults.TempFileSuffix));
            Assert.Equal(Utils.ComputeContentMd5(srcPath), Utils.ComputeContentMd5(dstPath));
        }
        finally
        {
            Utils.CleanPath(srcPath);
            Utils.CleanPath(dstPath);
        }
    }

    [Fact]
    public async Task TestDownloadFileRange()
    {
        var client = Utils.GetDefaultClient();
        var bucketName = await NewBucketAsync(client);
        var objectName = Utils.RandomObjectName();

        var content = Utils.GetRandomString(1000);
        await client.PutObjectAsync(new PutObjectRequest
        {
            Bucket = bucketName,
            Key = objectName,
            Body = new MemoryStream(Encoding.UTF8.GetBytes(content))
        });

        var dstPath = Utils.GetTempFileName();
        try
        {
            var downloader = new Downloader(client, o => o.PartSize = 100 * 1024);
            var request = new GetObjectRequest
            {
                Bucket = bucketName,
                Key = objectName,
                Range = "bytes=100-199"
            };

            var result = await downloader.DownloadFileAsync(request, dstPath);
            Assert.Equal(100, result.Written);

            var got = File.ReadAllText(dstPath);
            Assert.Equal(content.Substring(100, 100), got);
        }
        finally
        {
            Utils.CleanPath(dstPath);
        }
    }

    [Fact]
    public async Task TestDownloadFileNoTempFile()
    {
        var client = Utils.GetDefaultClient();
        var bucketName = await NewBucketAsync(client);
        var objectName = Utils.RandomObjectName();

        var content = Utils.GetRandomString(2000);
        await client.PutObjectAsync(new PutObjectRequest
        {
            Bucket = bucketName,
            Key = objectName,
            Body = new MemoryStream(Encoding.UTF8.GetBytes(content))
        });

        var dstPath = Utils.GetTempFileName();
        try
        {
            var downloader = new Downloader(client, o => o.UseTempFile = false);
            var result = await downloader.DownloadFileAsync(
                new GetObjectRequest { Bucket = bucketName, Key = objectName }, dstPath);

            Assert.Equal(content.Length, result.Written);
            Assert.Equal(content, File.ReadAllText(dstPath));
        }
        finally
        {
            Utils.CleanPath(dstPath);
        }
    }

    [Fact]
    public async Task TestDownloadFileTruncateExistingFile()
    {
        var client = Utils.GetDefaultClient();
        var bucketName = await NewBucketAsync(client);
        var objectName = Utils.RandomObjectName();

        var srcPath = Utils.GetTempFileName();
        var dstPath = Utils.GetTempFileName();
        Utils.PrepareSampleFile(srcPath, 350);
        try
        {
            await PutFileAsync(client, bucketName, objectName, srcPath);
            var fileSize = new FileInfo(srcPath).Length;

            var downloader = new Downloader(client, o =>
            {
                o.PartSize = 100 * 1024;
                o.ParallelNum = 3;
                o.UseTempFile = false;
            });

            // First download: full object.
            var result = await downloader.DownloadFileAsync(
                new GetObjectRequest { Bucket = bucketName, Key = objectName }, dstPath);
            Assert.Equal(fileSize, result.Written);
            Assert.Equal(fileSize, new FileInfo(dstPath).Length);
            Assert.Equal(Utils.ComputeContentMd5(srcPath), Utils.ComputeContentMd5(dstPath));

            // Second download: smaller range, existing file must be truncated.
            const int rangeStart = 123;
            const int rangeEnd = 1234;
            var rangeLen = rangeEnd - rangeStart + 1;
            result = await downloader.DownloadFileAsync(
                new GetObjectRequest
                {
                    Bucket = bucketName,
                    Key = objectName,
                    Range = $"bytes={rangeStart}-{rangeEnd}"
                }, dstPath);

            Assert.Equal(rangeLen, result.Written);
            Assert.Equal(rangeLen, new FileInfo(dstPath).Length);

            var expected = File.ReadAllText(srcPath).Substring(rangeStart, rangeLen);
            Assert.Equal(expected, File.ReadAllText(dstPath));
        }
        finally
        {
            Utils.CleanPath(srcPath);
            Utils.CleanPath(dstPath);
        }
    }

    [Fact]
    public async Task TestDownloadFileCheckpointResume()
    {
        var client = Utils.GetDefaultClient();
        var bucketName = await NewBucketAsync(client);
        var objectName = Utils.RandomObjectName();

        var srcPath = Utils.GetTempFileName();
        var dstPath = Utils.GetTempFileName();
        Utils.PrepareSampleFile(srcPath, 500);
        var cpDir = Utils.GetTempPath();
        Directory.CreateDirectory(cpDir);
        try
        {
            await PutFileAsync(client, bucketName, objectName, srcPath);
            var fileSize = new FileInfo(srcPath).Length;

            var cts = new CancellationTokenSource();
            long transferred = 0;
            var downloader = new Downloader(client, o =>
            {
                o.PartSize = 100 * 1024;
                o.ParallelNum = 1;
                o.EnableCheckpoint = true;
                o.CheckpointDir = cpDir;
            });

            var request = new GetObjectRequest
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
                downloader.DownloadFileAsync(request, dstPath, null, cts.Token));

            Assert.NotEmpty(Directory.GetFiles(cpDir, "*.dcp"));

            var request2 = new GetObjectRequest { Bucket = bucketName, Key = objectName };
            var result = await downloader.DownloadFileAsync(request2, dstPath);
            Assert.Equal(fileSize, result.Written);
            Assert.Equal(Utils.ComputeContentMd5(srcPath), Utils.ComputeContentMd5(dstPath));

            Assert.Empty(Directory.GetFiles(cpDir, "*.dcp"));
        }
        finally
        {
            Utils.CleanPath(srcPath);
            Utils.CleanPath(dstPath);
            Utils.CleanPath(cpDir);
        }
    }

    [Fact]
    public async Task TestDownloadFileSmallSinglePart()
    {
        var client = Utils.GetDefaultClient();
        var bucketName = await NewBucketAsync(client);
        var objectName = Utils.RandomObjectName();

        var content = Utils.GetRandomString(4096);
        await client.PutObjectAsync(new PutObjectRequest
        {
            Bucket = bucketName,
            Key = objectName,
            Body = new MemoryStream(Encoding.UTF8.GetBytes(content))
        });

        var dstPath = Utils.GetTempFileName();
        try
        {
            var downloader = new Downloader(client, o => o.PartSize = 5 * 1024 * 1024);
            var result = await downloader.DownloadFileAsync(
                new GetObjectRequest { Bucket = bucketName, Key = objectName }, dstPath);

            Assert.Equal(content.Length, result.Written);
            Assert.Equal(content, File.ReadAllText(dstPath));
        }
        finally
        {
            Utils.CleanPath(dstPath);
        }
    }

    [Fact]
    public async Task TestDownloadEmptyObject()
    {
        var client = Utils.GetDefaultClient();
        var bucketName = await NewBucketAsync(client);
        var objectName = Utils.RandomObjectName();

        await client.PutObjectAsync(new PutObjectRequest
        {
            Bucket = bucketName,
            Key = objectName,
            Body = new MemoryStream(Array.Empty<byte>())
        });

        var dstPath = Utils.GetTempFileName();
        try
        {
            var downloader = new Downloader(client);
            var result = await downloader.DownloadFileAsync(
                new GetObjectRequest { Bucket = bucketName, Key = objectName }, dstPath);

            Assert.Equal(0, result.Written);
            Assert.True(File.Exists(dstPath));
            Assert.Empty(File.ReadAllBytes(dstPath));
        }
        finally
        {
            Utils.CleanPath(dstPath);
        }
    }

    [Fact]
    public async Task TestDownloadFileSequential()
    {
        var client = Utils.GetDefaultClient();
        var bucketName = await NewBucketAsync(client);
        var objectName = Utils.RandomObjectName();

        var srcPath = Utils.GetTempFileName();
        var dstPath = Utils.GetTempFileName();
        Utils.PrepareSampleFile(srcPath, 350);
        try
        {
            await PutFileAsync(client, bucketName, objectName, srcPath);
            var fileSize = new FileInfo(srcPath).Length;

            var downloader = new Downloader(client, o =>
            {
                o.PartSize = 100 * 1024;
                o.ParallelNum = 1;
            });
            var result = await downloader.DownloadFileAsync(
                new GetObjectRequest { Bucket = bucketName, Key = objectName }, dstPath);

            Assert.Equal(fileSize, result.Written);
            Assert.Equal(Utils.ComputeContentMd5(srcPath), Utils.ComputeContentMd5(dstPath));
        }
        finally
        {
            Utils.CleanPath(srcPath);
            Utils.CleanPath(dstPath);
        }
    }

    [Fact]
    public async Task TestDownloadObjectNotExist()
    {
        var client = Utils.GetDefaultClient();
        var bucketName = await NewBucketAsync(client);
        var dstPath = Utils.GetTempFileName();
        try
        {
            var downloader = new Downloader(client);
            var ex = await Assert.ThrowsAsync<OperationException>(() =>
                downloader.DownloadFileAsync(
                    new GetObjectRequest { Bucket = bucketName, Key = Utils.RandomObjectName() }, dstPath));
            var se = Assert.IsAssignableFrom<ServiceException>(ex.InnerException);
            Assert.Equal(404, se.StatusCode);
            Assert.Equal("NoSuchKey", se.ErrorCode);
        }
        finally
        {
            Utils.CleanPath(dstPath);
        }
    }

    [Fact]
    public async Task TestDownloadInvalidAk()
    {
        // Create the bucket with a valid client so routing succeeds and the AK check runs.
        var bucketName = await NewBucketAsync(Utils.GetDefaultClient());

        var client = Utils.GetInvalidAkClient();
        var dstPath = Utils.GetTempFileName();
        try
        {
            var downloader = new Downloader(client);
            var ex = await Assert.ThrowsAsync<OperationException>(() =>
                downloader.DownloadFileAsync(
                    new GetObjectRequest { Bucket = bucketName, Key = "key" }, dstPath));
            var se = Assert.IsAssignableFrom<ServiceException>(ex.InnerException);
            Assert.Equal(403, se.StatusCode);
            Assert.Equal("InvalidAccessKeyId", se.ErrorCode);
        }
        finally
        {
            Utils.CleanPath(dstPath);
        }
    }
}
