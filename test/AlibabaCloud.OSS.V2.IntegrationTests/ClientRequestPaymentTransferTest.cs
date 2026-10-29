using System.Text;
using AlibabaCloud.OSS.V2.Models;

namespace AlibabaCloud.OSS.V2.IntegrationTests;

public class ClientRequestPaymentTransferTest : IDisposable
{
    private readonly string BucketNamePrefix;

    public void Dispose()
    {
        Utils.CleanBuckets(BucketNamePrefix);
    }

    public ClientRequestPaymentTransferTest()
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

    private static async Task SetupRequestPaymentBucketAsync(Client ownerClient, string bucketName)
    {
        // Allow the payer account to access the bucket and its objects.
        var policy = $$"""
            {
                "Version": "1",
                "Statement": [
                    {
                        "Action": ["oss:*"],
                        "Effect": "Allow",
                        "Principal": ["{{Utils.PayerUid}}"],
                        "Resource": [
                            "acs:oss:*:*:{{bucketName}}",
                            "acs:oss:*:*:{{bucketName}}/*"
                        ]
                    }
                ]
            }
            """;

        var putPolicyOutput = await ownerClient.InvokeOperationAsync(new OperationInput
        {
            OperationName = "PutBucketPolicy",
            Method = "PUT",
            Bucket = bucketName,
            Parameters = new Dictionary<string, string> { { "policy", "" } },
            Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "Content-Type", "application/json" }
            },
            Body = new MemoryStream(Encoding.UTF8.GetBytes(policy))
        });
        Assert.Equal(200, putPolicyOutput.StatusCode);

        // Enable request payment.
        var paymentXml = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
                         "<RequestPaymentConfiguration><Payer>Requester</Payer></RequestPaymentConfiguration>";
        var putPaymentOutput = await ownerClient.InvokeOperationAsync(new OperationInput
        {
            OperationName = "PutBucketRequestPayment",
            Method = "PUT",
            Bucket = bucketName,
            Parameters = new Dictionary<string, string> { { "requestPayment", "" } },
            Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "Content-Type", "application/xml" }
            },
            Body = new MemoryStream(Encoding.UTF8.GetBytes(paymentXml))
        });
        Assert.Equal(200, putPaymentOutput.StatusCode);
    }

    [Fact]
    public async Task TestDownloaderWithRequestPayer()
    {
        var ownerClient = Utils.GetDefaultClient();
        var bucketName = await NewBucketAsync(ownerClient);
        var objectName = Utils.RandomObjectName();

        var content = Utils.GetRandomString(2000);
        await ownerClient.PutObjectAsync(new PutObjectRequest
        {
            Bucket = bucketName,
            Key = objectName,
            Body = new MemoryStream(Encoding.UTF8.GetBytes(content))
        });

        await SetupRequestPaymentBucketAsync(ownerClient, bucketName);

        var dstPath = Utils.GetTempFileName();
        try
        {
            var downloader = new Downloader(ownerClient);
            var result = await downloader.DownloadFileAsync(
                new GetObjectRequest
                {
                    Bucket = bucketName,
                    Key = objectName,
                    Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                    {
                        { "x-oss-request-payer", "requester" }
                    }
                }, dstPath);

            Assert.Equal(content.Length, result.Written);
            Assert.Equal(content, File.ReadAllText(dstPath));
        }
        finally
        {
            Utils.CleanPath(dstPath);
        }
    }

    [Fact]
    public async Task TestUploaderWithRequestPayer()
    {
        var ownerClient = Utils.GetDefaultClient();
        var bucketName = await NewBucketAsync(ownerClient);
        var objectName = Utils.RandomObjectName();

        await SetupRequestPaymentBucketAsync(ownerClient, bucketName);

        var srcPath = Utils.GetTempFileName();
        Utils.PrepareSampleFile(srcPath, 200);
        try
        {
            var uploader = new Uploader(ownerClient, o =>
            {
                o.PartSize = 100 * 1024;
                o.ParallelNum = 3;
            });
            var result = await uploader.UploadFileAsync(
                new PutObjectRequest
                {
                    Bucket = bucketName,
                    Key = objectName,
                    Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                    {
                        { "x-oss-request-payer", "requester" }
                    }
                }, srcPath);

            Assert.Equal(200, result.StatusCode);

            var head = await ownerClient.HeadObjectAsync(new HeadObjectRequest { Bucket = bucketName, Key = objectName });
            Assert.Equal(new FileInfo(srcPath).Length, head.ContentLength);
        }
        finally
        {
            Utils.CleanPath(srcPath);
        }
    }

    [Fact]
    public async Task TestCopierWithRequestPayer()
    {
        var ownerClient = Utils.GetDefaultClient();
        var bucketName = await NewBucketAsync(ownerClient);
        var srcKey = Utils.RandomObjectName();
        var dstKey = Utils.RandomObjectName();

        var data = Encoding.UTF8.GetBytes(Utils.GetRandomString(300 * 1024));
        await ownerClient.PutObjectAsync(new PutObjectRequest
        {
            Bucket = bucketName,
            Key = srcKey,
            Body = new MemoryStream(data)
        });

        await SetupRequestPaymentBucketAsync(ownerClient, bucketName);

        var copier = new Copier(ownerClient, o =>
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
            SourceKey = srcKey,
            Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "x-oss-request-payer", "requester" }
            }
        });

        Assert.Equal(200, result.StatusCode);

        var head = await ownerClient.HeadObjectAsync(new HeadObjectRequest { Bucket = bucketName, Key = dstKey });
        Assert.Equal(data.Length, head.ContentLength);
    }
}
