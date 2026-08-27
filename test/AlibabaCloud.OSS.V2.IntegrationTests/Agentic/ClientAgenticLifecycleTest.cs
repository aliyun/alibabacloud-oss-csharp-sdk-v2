using AlibabaCloud.OSS.V2.Agentic;
using AlibabaCloud.OSS.V2.Agentic.Models;
using static AlibabaCloud.OSS.V2.IntegrationTests.Agentic.AgenticTestSupport;

namespace AlibabaCloud.OSS.V2.IntegrationTests.Agentic;

// Lifecycle connectivity: on its own bucket (so disabling does not break other
// tests), PutStatus(Disabled) succeeds, then Delete is not yet ready (409).
public class ClientAgenticLifecycleTest
{
    [Fact]
    public async Task TestDisableThenDelete()
    {
        if (!Configured) return;
        using var client = GetAgenticClient();
        var bucket = GenBucketName();

        var createResult = await client.CreateAgenticBucketAsync(new CreateAgenticBucketRequest { Bucket = bucket });
        Assert.Equal(200, createResult.StatusCode);

        try
        {
            var putResult = await client.PutAgenticBucketStatusAsync(new PutAgenticBucketStatusRequest
            {
                Bucket = bucket,
                AgenticBucketStatus = new AgenticBucketStatus { Status = "Disabled" }
            });
            Assert.Equal(200, putResult.StatusCode);

            var threw = false;
            try
            {
                await client.DeleteAgenticBucketAsync(new DeleteAgenticBucketRequest { Bucket = bucket });
            }
            catch (Exception e)
            {
                threw = true;
                var se = e as ServiceException ?? e.InnerException as ServiceException;
                Assert.NotNull(se);
                Assert.True(se!.StatusCode == 409 || se.ErrorCode == "AgenticBucketNotReady",
                    $"expected AgenticBucketNotReady/409, got code={se.ErrorCode} status={se.StatusCode}");
            }
            Assert.True(threw, "immediate delete after disable should fail (not ready)");
        }
        finally
        {
            await ReapDisabledAsync(client);
        }
    }
}
