using System.Text;
using AlibabaCloud.OSS.V2.Agentic;
using AlibabaCloud.OSS.V2.Agentic.Models;
using static AlibabaCloud.OSS.V2.IntegrationTests.Agentic.AgenticTestSupport;

namespace AlibabaCloud.OSS.V2.IntegrationTests.Agentic;

// Miscellaneous agentic integration scenarios that do not belong to the Basic,
// Lifecycle, or Space suites. Add future one-offs here.
public class ClientAgenticMiscTest
{
    // Path-style addressing across both the agentic bucket client and the bucket
    // space client. Path-style may be disabled on the endpoint; the probe detects
    // SecondLevelDomainForbidden and skips (returns) rather than fails, since that
    // is an endpoint capability and not an SDK defect.
    [Fact]
    public async Task TestPathStyle()
    {
        if (!Configured) return;

        // Create the bucket with the default (virtual-hosted) client so the fixture
        // stands regardless of whether path-style turns out to be allowed.
        using var client = GetAgenticClient();
        var bucket = GenBucketName();
        var createResult = await client.CreateAgenticBucketAsync(new CreateAgenticBucketRequest { Bucket = bucket });
        Assert.Equal(200, createResult.StatusCode);

        try
        {
            using var psClient = GetAgenticClientPathStyle();

            // Probe: a path-style GET carrying the bucket. ListAgenticBuckets is
            // service-level (no bucket label) so it cannot probe path-style;
            // GetAgenticBucket carries the bucket and does.
            try
            {
                var getResult = await psClient.GetAgenticBucketAsync(new GetAgenticBucketRequest { Bucket = bucket });
                Assert.Equal(200, getResult.StatusCode);
                Assert.NotNull(getResult.AgenticBucketInfo);
            }
            catch (Exception e)
            {
                var se = e as ServiceException ?? e.InnerException as ServiceException;
                if (se != null && se.ErrorCode == "SecondLevelDomainForbidden") return;
                throw;
            }

            // Agentic bucket client over path-style.
            var listResult = await psClient.ListBucketSpacesAsync(new ListBucketSpacesRequest { Bucket = bucket });
            Assert.Equal(200, listResult.StatusCode);

            // Create one bucket space (via the default client) shared by the checks below.
            using var bsClient = GetBucketSpaceClient();
            var putBucketResult = await bsClient.PutBucketAsync(new Models.PutBucketRequest
            {
                Bucket = bucket,
                AgenticBucket = BuildFullName(bucket, "ab-apsr")
            });
            Assert.Equal(200, putBucketResult.StatusCode);

            try
            {
                // Bucket space client over path-style.
                using var psBsClient = GetBucketSpaceClientPathStyle();

                var key = "csharp-sdk-test-object-" + GenBucketName();
                const string content = "hello path-style";

                // Path-style may be forbidden on the bucket space endpoint independently
                // of the agentic bucket endpoint (different domain), so guard the first
                // bucket-space path-style call separately. A pass here implies the
                // GetBucketAcl check below is fine too.
                Models.PutObjectResult putObjectResult;
                try
                {
                    putObjectResult = await psBsClient.PutObjectAsync(new Models.PutObjectRequest
                    {
                        Bucket = bucket,
                        Key = key,
                        Body = new MemoryStream(Encoding.UTF8.GetBytes(content))
                    });
                }
                catch (Exception e)
                {
                    var se = e as ServiceException ?? e.InnerException as ServiceException;
                    if (se != null && se.ErrorCode == "SecondLevelDomainForbidden") return;
                    throw;
                }
                Assert.Equal(200, putObjectResult.StatusCode);

                var getObjectResult = await psBsClient.GetObjectAsync(new Models.GetObjectRequest
                {
                    Bucket = bucket,
                    Key = key
                });
                Assert.Equal(200, getObjectResult.StatusCode);
                using (var reader = new StreamReader(getObjectResult.Body!))
                {
                    Assert.Equal(content, await reader.ReadToEndAsync());
                }

                try { await psBsClient.DeleteObjectAsync(new Models.DeleteObjectRequest { Bucket = bucket, Key = key }); }
                catch { /* best effort */ }

                var getAclResult = await psBsClient.GetBucketAclAsync(new Models.GetBucketAclRequest { Bucket = bucket });
                Assert.Equal(200, getAclResult.StatusCode);
            }
            finally
            {
                try { await bsClient.DeleteBucketAsync(new Models.DeleteBucketRequest { Bucket = bucket }); }
                catch { /* best effort */ }
            }
        }
        finally
        {
            await DisableAndReapAsync(client, bucket);
        }
    }
}
