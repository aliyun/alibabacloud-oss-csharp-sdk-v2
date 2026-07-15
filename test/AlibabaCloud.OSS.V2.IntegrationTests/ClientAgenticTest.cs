using System.Text;
using AlibabaCloud.OSS.V2.Agentic;
using AlibabaCloud.OSS.V2.Agentic.Models;
using AlibabaCloud.OSS.V2.Credentials;

namespace AlibabaCloud.OSS.V2.IntegrationTests;

public class ClientAgenticTest
{
    private const string BucketNamePrefix = "csharp-sdk-test-ab-";
    private const string Letters = "abcdefghijklmnopqrstuvwxyz";

    private static string AccountId => Environment.GetEnvironmentVariable("OSS_TEST_ACCOUNT_ID");

    private static bool Configured =>
        !string.IsNullOrEmpty(AccountId) && !string.IsNullOrEmpty(Utils.Region);

    private static AgenticBucketClient GetAgenticClient()
    {
        var cfg = Configuration.LoadDefault();
        cfg.CredentialsProvider = new StaticCredentialsProvider(Utils.AccessKeyId, Utils.AccessKeySecret);
        cfg.Region = Utils.Region;
        cfg.Endpoint = Utils.Endpoint;
        cfg.AccountId = AccountId;
        return new AgenticBucketClient(cfg);
    }

    private static AgenticBucketClient GetInvalidAkClient()
    {
        var cfg = Configuration.LoadDefault();
        cfg.CredentialsProvider = new StaticCredentialsProvider("invalid-ak", "invalid-sk");
        cfg.Region = Utils.Region;
        cfg.Endpoint = Utils.Endpoint;
        cfg.AccountId = AccountId;
        return new AgenticBucketClient(cfg);
    }

    private static Client GetBucketSpaceClient()
    {
        var cfg = Configuration.LoadDefault();
        cfg.CredentialsProvider = new StaticCredentialsProvider(Utils.AccessKeyId, Utils.AccessKeySecret);
        cfg.Region = Utils.Region;
        cfg.Endpoint = Utils.Endpoint;
        cfg.AccountId = AccountId;
        return AgenticBucketClient.NewBucketSpaceClient(cfg);
    }

    private static string GenBucketName()
    {
        var rnd = new Random();
        var sb = new StringBuilder(6);
        for (var i = 0; i < 6; i++) sb.Append(Letters[rnd.Next(Letters.Length)]);
        return BucketNamePrefix + sb;
    }

    private static async Task CleanAgenticBucket(AgenticBucketClient client, string bucket)
    {
        try { await client.DeleteAgenticBucketAsync(new DeleteAgenticBucketRequest { Bucket = bucket }); }
        catch { /* best effort */ }
    }

    [Fact]
    public async Task TestAgenticBucketLifecycle()
    {
        if (!Configured) return;
        using var client = GetAgenticClient();
        var bucket = GenBucketName();

        try
        {
            var createResult = await client.CreateAgenticBucketAsync(new CreateAgenticBucketRequest
            {
                Bucket = bucket,
                CreateAgenticBucketConfiguration = new CreateAgenticBucketConfiguration
                {
                    StorageClass = "Standard",
                    DataRedundancyType = "LRS"
                }
            });
            Assert.Equal(200, createResult.StatusCode);

            var getResult = await client.GetAgenticBucketAsync(new GetAgenticBucketRequest { Bucket = bucket });
            Assert.Equal(200, getResult.StatusCode);
            Assert.NotNull(getResult.AgenticBucketInfo);
            Assert.Contains(bucket, getResult.AgenticBucketInfo!.Name);

            // list, ensure created bucket appears
            var found = false;
            var paginator = client.ListAgenticBucketsPaginator(new ListAgenticBucketsRequest());
            await foreach (var page in paginator.IterPageAsync())
            {
                if (page.AgenticBuckets == null) continue;
                foreach (var b in page.AgenticBuckets)
                {
                    if (b.Name != null && b.Name.Contains(bucket)) found = true;
                }
            }
            Assert.True(found, "created agentic bucket should appear in list");

            var deleteResult = await client.DeleteAgenticBucketAsync(new DeleteAgenticBucketRequest { Bucket = bucket });
            Assert.True(deleteResult.StatusCode is 200 or 204);
        }
        finally
        {
            await CleanAgenticBucket(client, bucket);
        }
    }

    [Fact]
    public async Task TestAgenticBucketStatus()
    {
        if (!Configured) return;
        using var client = GetAgenticClient();
        var bucket = GenBucketName();

        try
        {
            var createResult = await client.CreateAgenticBucketAsync(new CreateAgenticBucketRequest { Bucket = bucket });
            Assert.Equal(200, createResult.StatusCode);

            var putResult = await client.PutAgenticBucketStatusAsync(new PutAgenticBucketStatusRequest
            {
                Bucket = bucket,
                AgenticBucketStatus = new AgenticBucketStatus { Status = "Enabled" }
            });
            Assert.Equal(200, putResult.StatusCode);
        }
        finally
        {
            await CleanAgenticBucket(client, bucket);
        }
    }

    [Fact]
    public async Task TestListBucketSpaces()
    {
        if (!Configured) return;
        using var client = GetAgenticClient();
        var bucket = GenBucketName();

        try
        {
            var createResult = await client.CreateAgenticBucketAsync(new CreateAgenticBucketRequest { Bucket = bucket });
            Assert.Equal(200, createResult.StatusCode);

            var listResult = await client.ListBucketSpacesAsync(new ListBucketSpacesRequest { Bucket = bucket });
            Assert.Equal(200, listResult.StatusCode);
        }
        finally
        {
            await CleanAgenticBucket(client, bucket);
        }
    }

    [Fact]
    public async Task TestBucketSpaceObjectLifecycle()
    {
        if (!Configured) return;
        using var client = GetAgenticClient();
        using var bsClient = GetBucketSpaceClient();
        var bucket = GenBucketName();
        const string content = "hello world";

        try
        {
            var createResult = await client.CreateAgenticBucketAsync(new CreateAgenticBucketRequest { Bucket = bucket });
            Assert.Equal(200, createResult.StatusCode);

            var putBucketResult = await bsClient.PutBucketAsync(new Models.PutBucketRequest { Bucket = bucket });
            Assert.Equal(200, putBucketResult.StatusCode);

            var key = "csharp-sdk-test-object-" + GenBucketName();
            try
            {
                var putObjectResult = await bsClient.PutObjectAsync(new Models.PutObjectRequest
                {
                    Bucket = bucket,
                    Key = key,
                    Body = new MemoryStream(Encoding.UTF8.GetBytes(content))
                });
                Assert.Equal(200, putObjectResult.StatusCode);

                var getObjectResult = await bsClient.GetObjectAsync(new Models.GetObjectRequest
                {
                    Bucket = bucket,
                    Key = key
                });
                Assert.Equal(200, getObjectResult.StatusCode);
                using var reader = new StreamReader(getObjectResult.Body!);
                Assert.Equal(content, await reader.ReadToEndAsync());
            }
            finally
            {
                try { await bsClient.DeleteObjectAsync(new Models.DeleteObjectRequest { Bucket = bucket, Key = key }); }
                catch { /* best effort */ }
                try { await bsClient.DeleteBucketAsync(new Models.DeleteBucketRequest { Bucket = bucket }); }
                catch { /* best effort */ }
            }
        }
        finally
        {
            await CleanAgenticBucket(client, bucket);
        }
    }

    [Fact]
    public async Task TestAgenticBucketServerErrors()
    {
        if (!Configured) return;
        using var client = GetInvalidAkClient();
        var bucket = GenBucketName();

        // create with invalid AK
        try
        {
            await client.CreateAgenticBucketAsync(new CreateAgenticBucketRequest { Bucket = bucket });
            Assert.Fail("expected an exception");
        }
        catch (Exception e)
        {
            var se = e as ServiceException ?? e.InnerException as ServiceException;
            Assert.NotNull(se);
            Assert.Equal(403, se!.StatusCode);
            Assert.NotEmpty(se.RequestId);
        }

        // get with invalid AK
        try
        {
            await client.GetAgenticBucketAsync(new GetAgenticBucketRequest { Bucket = bucket });
            Assert.Fail("expected an exception");
        }
        catch (Exception e)
        {
            var se = e as ServiceException ?? e.InnerException as ServiceException;
            Assert.NotNull(se);
            Assert.Equal(403, se!.StatusCode);
        }

        // list with invalid AK
        try
        {
            await client.ListAgenticBucketsAsync(new ListAgenticBucketsRequest());
            Assert.Fail("expected an exception");
        }
        catch (Exception e)
        {
            var se = e as ServiceException ?? e.InnerException as ServiceException;
            Assert.NotNull(se);
            Assert.Equal(403, se!.StatusCode);
        }
    }
}
