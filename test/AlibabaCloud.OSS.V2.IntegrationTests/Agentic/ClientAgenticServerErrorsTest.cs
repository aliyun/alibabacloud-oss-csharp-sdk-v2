using AlibabaCloud.OSS.V2.Agentic.Models;
using static AlibabaCloud.OSS.V2.IntegrationTests.Agentic.AgenticTestSupport;

namespace AlibabaCloud.OSS.V2.IntegrationTests.Agentic;

// Error propagation with invalid credentials. Create returns 403; Get/List are
// relaxed because the real service returns 404 for Get under invalid AK.
public class ClientAgenticServerErrorsTest
{
    [Fact]
    public async Task TestServerErrors()
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
            Assert.NotEqual(0, se!.StatusCode);
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
            Assert.NotEqual(0, se!.StatusCode);
        }
    }
}
