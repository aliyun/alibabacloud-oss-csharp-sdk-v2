using System.Linq;
using System.Text;
using AlibabaCloud.OSS.V2.Models;

namespace AlibabaCloud.OSS.V2.IntegrationTests;

public class ClientCopierTest : IDisposable
{
    private readonly string BucketNamePrefix;

    public void Dispose()
    {
        Utils.CleanBuckets(BucketNamePrefix);
    }

    public ClientCopierTest()
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

    private static async Task PutContentAsync(Client client, string bucket, string key, byte[] data)
    {
        var result = await client.PutObjectAsync(new PutObjectRequest
        {
            Bucket = bucket,
            Key = key,
            Body = new MemoryStream(data)
        });
        Assert.Equal(200, result.StatusCode);
    }

    [Fact]
    public async Task TestCopySmallSameBucket()
    {
        var client = Utils.GetDefaultClient();
        var bucketName = await NewBucketAsync(client);
        var srcKey = Utils.RandomObjectName();
        var dstKey = Utils.RandomObjectName();

        var data = Encoding.UTF8.GetBytes(Utils.GetRandomString(2048));
        await PutContentAsync(client, bucketName, srcKey, data);

        var copier = new Copier(client);
        var result = await copier.CopyAsync(new CopyObjectRequest
        {
            Bucket = bucketName,
            Key = dstKey,
            SourceKey = srcKey
        });

        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.ETag);

        var head = await client.HeadObjectAsync(new HeadObjectRequest { Bucket = bucketName, Key = dstKey });
        Assert.Equal(data.Length, head.ContentLength);

        var getResult = await client.GetObjectAsync(new GetObjectRequest { Bucket = bucketName, Key = dstKey });
        Assert.Equal(Utils.ComputeContentMd5(new MemoryStream(data)), Utils.ComputeContentMd5(getResult.Body!));
    }

    [Fact]
    public async Task TestCopyMultipart()
    {
        var client = Utils.GetDefaultClient();
        var bucketName = await NewBucketAsync(client);
        var srcKey = Utils.RandomObjectName();
        var dstKey = Utils.RandomObjectName();

        var srcPath = Utils.GetTempFileName();
        Utils.PrepareSampleFile(srcPath, 500);
        try
        {
            using (var fs = File.OpenRead(srcPath))
                await client.PutObjectAsync(new PutObjectRequest { Bucket = bucketName, Key = srcKey, Body = fs });
            var fileSize = new FileInfo(srcPath).Length;

            // Force multipart copy by lowering the threshold and part size.
            var copier = new Copier(client, o =>
            {
                o.MultipartCopyThreshold = 100 * 1024;
                o.PartSize = 100 * 1024;
                o.ParallelNum = 3;
                o.DisableShallowCopy = true;
            });

            var result = await copier.CopyAsync(new CopyObjectRequest
            {
                Bucket = bucketName,
                Key = dstKey,
                SourceKey = srcKey
            });

            Assert.Equal(200, result.StatusCode);
            Assert.NotNull(result.UploadId); // multipart copy path

            var head = await client.HeadObjectAsync(new HeadObjectRequest { Bucket = bucketName, Key = dstKey });
            Assert.Equal(fileSize, head.ContentLength);

            var getResult = await client.GetObjectAsync(new GetObjectRequest { Bucket = bucketName, Key = dstKey });
            Assert.Equal(Utils.ComputeContentMd5(srcPath), Utils.ComputeContentMd5(getResult.Body!));
        }
        finally
        {
            Utils.CleanPath(srcPath);
        }
    }

    [Fact]
    public async Task TestCopyCrossBucket()
    {
        var client = Utils.GetDefaultClient();
        var srcBucket = await NewBucketAsync(client);
        var dstBucket = await NewBucketAsync(client);
        var srcKey = Utils.RandomObjectName();
        var dstKey = Utils.RandomObjectName();

        var data = Encoding.UTF8.GetBytes(Utils.GetRandomString(4096));
        await PutContentAsync(client, srcBucket, srcKey, data);

        var copier = new Copier(client);
        var result = await copier.CopyAsync(new CopyObjectRequest
        {
            Bucket = dstBucket,
            Key = dstKey,
            SourceBucket = srcBucket,
            SourceKey = srcKey
        });

        Assert.Equal(200, result.StatusCode);

        var head = await client.HeadObjectAsync(new HeadObjectRequest { Bucket = dstBucket, Key = dstKey });
        Assert.Equal(data.Length, head.ContentLength);
    }

    [Fact]
    public async Task TestCopyMetadataReplace()
    {
        var client = Utils.GetDefaultClient();
        var bucketName = await NewBucketAsync(client);
        var srcKey = Utils.RandomObjectName();
        var dstKey = Utils.RandomObjectName();

        var data = Encoding.UTF8.GetBytes(Utils.GetRandomString(1024));
        await client.PutObjectAsync(new PutObjectRequest
        {
            Bucket = bucketName,
            Key = srcKey,
            Body = new MemoryStream(data),
            Metadata = new Dictionary<string, string> { { "author", "src" } }
        });

        var copier = new Copier(client);
        var result = await copier.CopyAsync(new CopyObjectRequest
        {
            Bucket = bucketName,
            Key = dstKey,
            SourceKey = srcKey,
            MetadataDirective = "REPLACE",
            Metadata = new Dictionary<string, string> { { "author", "dst" }, { "team", "oss" } }
        });

        Assert.Equal(200, result.StatusCode);

        var head = await client.HeadObjectAsync(new HeadObjectRequest { Bucket = bucketName, Key = dstKey });
        Assert.NotNull(head.Metadata);
        Assert.Equal("dst", head.Metadata["author"]);
        Assert.Equal("oss", head.Metadata["team"]);
    }

    [Fact]
    public async Task TestCopyShallowLargeSameBucket()
    {
        var client = Utils.GetDefaultClient();
        var bucketName = await NewBucketAsync(client);
        var srcKey = Utils.RandomObjectName();
        var dstKey = Utils.RandomObjectName();

        var srcPath = Utils.GetTempFileName();
        Utils.PrepareSampleFile(srcPath, 500);
        try
        {
            using (var fs = File.OpenRead(srcPath))
                await client.PutObjectAsync(new PutObjectRequest { Bucket = bucketName, Key = srcKey, Body = fs });
            var fileSize = new FileInfo(srcPath).Length;

            // Lower the threshold so a large object takes the shallow-copy path (same bucket, no changes).
            var copier = new Copier(client, o => o.MultipartCopyThreshold = 100 * 1024);
            var result = await copier.CopyAsync(new CopyObjectRequest
            {
                Bucket = bucketName,
                Key = dstKey,
                SourceKey = srcKey
            });

            Assert.Equal(200, result.StatusCode);
            Assert.Null(result.UploadId); // shallow copy uses CopyObject, not multipart

            var head = await client.HeadObjectAsync(new HeadObjectRequest { Bucket = bucketName, Key = dstKey });
            Assert.Equal(fileSize, head.ContentLength);

            var getResult = await client.GetObjectAsync(new GetObjectRequest { Bucket = bucketName, Key = dstKey });
            Assert.Equal(Utils.ComputeContentMd5(srcPath), Utils.ComputeContentMd5(getResult.Body!));
        }
        finally
        {
            Utils.CleanPath(srcPath);
        }
    }

    [Fact]
    public async Task TestCopyMetadataCopyDirective()
    {
        var client = Utils.GetDefaultClient();
        var bucketName = await NewBucketAsync(client);
        var srcKey = Utils.RandomObjectName();
        var dstKey = Utils.RandomObjectName();

        var data = Encoding.UTF8.GetBytes(Utils.GetRandomString(300 * 1024));
        await client.PutObjectAsync(new PutObjectRequest
        {
            Bucket = bucketName,
            Key = srcKey,
            Body = new MemoryStream(data),
            Metadata = new Dictionary<string, string> { { "author", "src" } }
        });

        // Force multipart copy; source metadata should be carried over by default (COPY directive).
        var copier = new Copier(client, o =>
        {
            o.MultipartCopyThreshold = 100 * 1024;
            o.PartSize = 100 * 1024;
            o.DisableShallowCopy = true;
        });
        var result = await copier.CopyAsync(new CopyObjectRequest
        {
            Bucket = bucketName,
            Key = dstKey,
            SourceKey = srcKey
        });

        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.UploadId);

        var head = await client.HeadObjectAsync(new HeadObjectRequest { Bucket = bucketName, Key = dstKey });
        Assert.NotNull(head.Metadata);
        Assert.Equal("src", head.Metadata["author"]);
    }

    [Fact]
    public async Task TestCopyTaggingCopyDirective()
    {
        var client = Utils.GetDefaultClient();
        var bucketName = await NewBucketAsync(client);
        var srcKey = Utils.RandomObjectName();
        var dstKey = Utils.RandomObjectName();

        var data = Encoding.UTF8.GetBytes(Utils.GetRandomString(300 * 1024));
        await client.PutObjectAsync(new PutObjectRequest
        {
            Bucket = bucketName,
            Key = srcKey,
            Body = new MemoryStream(data),
            Tagging = "k1=v1&k2=v2"
        });

        // Force multipart copy; source tagging should be carried over by default.
        var copier = new Copier(client, o =>
        {
            o.MultipartCopyThreshold = 100 * 1024;
            o.PartSize = 100 * 1024;
            o.DisableShallowCopy = true;
        });
        var result = await copier.CopyAsync(new CopyObjectRequest
        {
            Bucket = bucketName,
            Key = dstKey,
            SourceKey = srcKey
        });

        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.UploadId);

        var tagging = await client.GetObjectTaggingAsync(
            new GetObjectTaggingRequest { Bucket = bucketName, Key = dstKey });
        Assert.NotNull(tagging.Tagging?.TagSet?.Tags);
        var tags = tagging.Tagging!.TagSet!.Tags!;
        Assert.Equal(2, tags.Count);
        Assert.Equal("v1", tags.Single(t => t.Key == "k1").Value);
        Assert.Equal("v2", tags.Single(t => t.Key == "k2").Value);
    }

    [Fact]
    public async Task TestCopyTaggingCopyDirectiveWithSpecialChars()
    {
        var client = Utils.GetDefaultClient();
        var bucketName = await NewBucketAsync(client);
        var srcKey = Utils.RandomObjectName();
        var dstKey = Utils.RandomObjectName();

        var data = Encoding.UTF8.GetBytes(Utils.GetRandomString(300 * 1024));
        await PutContentAsync(client, bucketName, srcKey, data);

        // Use PutObjectTagging XML body to store raw '=' and '+' in key/value.
        await client.PutObjectTaggingAsync(new PutObjectTaggingRequest
        {
            Bucket = bucketName,
            Key = srcKey,
            Tagging = new Tagging
            {
                TagSet = new TagSet
                {
                    Tags =
                    [
                        new Tag { Key = "a=b", Value = "c+d" },
                        new Tag { Key = "k1", Value = "v1" }
                    ]
                }
            }
        });

        // Force multipart copy; source tagging with special chars must be carried over.
        var copier = new Copier(client, o =>
        {
            o.MultipartCopyThreshold = 100 * 1024;
            o.PartSize = 100 * 1024;
            o.DisableShallowCopy = true;
        });
        var result = await copier.CopyAsync(new CopyObjectRequest
        {
            Bucket = bucketName,
            Key = dstKey,
            SourceKey = srcKey
        });

        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.UploadId);

        var tagging = await client.GetObjectTaggingAsync(
            new GetObjectTaggingRequest { Bucket = bucketName, Key = dstKey });
        Assert.NotNull(tagging.Tagging?.TagSet?.Tags);
        var tags = tagging.Tagging!.TagSet!.Tags!;
        Assert.Equal(2, tags.Count);
        Assert.Equal("c+d", tags.Single(t => t.Key == "a=b").Value);
        Assert.Equal("v1", tags.Single(t => t.Key == "k1").Value);
    }

    [Fact]
    public async Task TestCopySourceNotExist()
    {
        var client = Utils.GetDefaultClient();
        var bucketName = await NewBucketAsync(client);

        var copier = new Copier(client);
        var ex = await Assert.ThrowsAsync<OperationException>(() =>
            copier.CopyAsync(new CopyObjectRequest
            {
                Bucket = bucketName,
                Key = Utils.RandomObjectName(),
                SourceKey = Utils.RandomObjectName()
            }));
        var se = Assert.IsAssignableFrom<ServiceException>(ex.InnerException);
        Assert.Equal(404, se.StatusCode);
        Assert.Equal("NoSuchKey", se.ErrorCode);
    }

    [Fact]
    public async Task TestCopyInvalidAk()
    {
        // Create the bucket with a valid client so routing succeeds and the AK check runs.
        var bucketName = await NewBucketAsync(Utils.GetDefaultClient());

        var client = Utils.GetInvalidAkClient();
        var copier = new Copier(client);
        var ex = await Assert.ThrowsAsync<OperationException>(() =>
            copier.CopyAsync(new CopyObjectRequest
            {
                Bucket = bucketName,
                Key = "dst",
                SourceKey = "src"
            }));
        var se = Assert.IsAssignableFrom<ServiceException>(ex.InnerException);
        Assert.Equal(403, se.StatusCode);
        Assert.Equal("InvalidAccessKeyId", se.ErrorCode);
    }
}
