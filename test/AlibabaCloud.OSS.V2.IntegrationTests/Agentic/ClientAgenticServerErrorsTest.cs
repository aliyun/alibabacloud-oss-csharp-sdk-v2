using AlibabaCloud.OSS.V2.Agentic.Models;
using static AlibabaCloud.OSS.V2.IntegrationTests.Agentic.AgenticTestSupport;

namespace AlibabaCloud.OSS.V2.IntegrationTests.Agentic;

// Error propagation with invalid credentials. Create and List return 403 InvalidAccessKeyId,
// while Get returns 404 NoSuchAgenticBucket: the real service resolves bucket existence before
// it validates the credentials. Ec is only checked for presence, it is a server-internal
// diagnostic and not part of the contract.
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
            Assert.Equal("InvalidAccessKeyId", se.ErrorCode);
            Assert.NotEmpty(se.Ec);
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
            Assert.Equal(404, se!.StatusCode);
            Assert.Equal("NoSuchAgenticBucket", se.ErrorCode);
            Assert.NotEmpty(se.Ec);
            Assert.NotEmpty(se.RequestId);
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
            Assert.Equal("InvalidAccessKeyId", se.ErrorCode);
            Assert.NotEmpty(se.Ec);
            Assert.NotEmpty(se.RequestId);
        }
    }
}
