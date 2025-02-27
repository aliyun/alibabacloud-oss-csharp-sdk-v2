using AlibabaCloud.OSS.V2.Retry;

namespace AlibabaCloud.OSS.V2.UnitTests.Retry;

public class ErrorRetryableTest {
    [Fact]
    public void TestHttpStatusCodeRetryable() {
        var retry = new HttpStatusCodeRetryable();
        Assert.False(retry.IsErrorRetryable(new Exception("")));
        Assert.False(retry.IsErrorRetryable(new V2.ServiceException(404, null)));

        Assert.True(retry.IsErrorRetryable(new V2.ServiceException(401, null)));
        Assert.True(retry.IsErrorRetryable(new V2.ServiceException(408, null)));
        Assert.True(retry.IsErrorRetryable(new V2.ServiceException(429, null)));
        Assert.True(retry.IsErrorRetryable(new V2.ServiceException(500, null)));
        Assert.True(retry.IsErrorRetryable(new V2.ServiceException(599, null)));
    }


    [Fact]
    public void TestServiceErrorCodeRetryable() {
        var retry = new ServiceErrorCodeRetryable();
        var details = new Dictionary<string, string>();
        Assert.False(retry.IsErrorRetryable(new Exception("")));
        Assert.False(retry.IsErrorRetryable(new V2.ServiceException(404, null)));
        Assert.False(retry.IsErrorRetryable(new V2.ServiceException(404, details)));
        details = new Dictionary<string, string>() {
            { "Code", "UnSupportCode" }
        };
        Assert.False(retry.IsErrorRetryable(new V2.ServiceException(401, details)));


        details = new Dictionary<string, string>() {
            { "Code", "RequestTimeTooSkewed" }
        };
        Assert.True(retry.IsErrorRetryable(new V2.ServiceException(401, details)));
        details = new Dictionary<string, string>() {
            { "Code", "BadRequest" }
        };
        Assert.True(retry.IsErrorRetryable(new V2.ServiceException(408, details)));
    }

    [Fact]
    public void TestClientExceptionRetryable() {
        var retry = new ClientExceptionRetryable();
        Assert.False(retry.IsErrorRetryable(new Exception("")));

        Assert.True(retry.IsErrorRetryable(new V2.InconsistentException("", "", "")));
        Assert.True(retry.IsErrorRetryable(new V2.RequestFailedException("fake")));
    }
}