using AlibabaCloud.OSS.v2.Retry;

namespace AlibabaCloud.OSS.v2.UnitTests.Retry;

public class RetryerTest {
    [Fact]
    public void TestNopRetryer() {
        var retryer = new NopRetryer();
        Assert.Equal(1, retryer.MaxAttempts());
        Assert.False(retryer.IsErrorRetryable(new("")));

        try {
            retryer.RetryDelay(1, new());
            Assert.Fail("should not here");
        }
        catch (NotImplementedException e) {
            Assert.Contains("not retrying any attempt errors", e.ToString());
        }
    }

    [Fact]
    public void TestStandardRetryerDefault() {
        var retryer = new StandardRetryer();
        Assert.Equal(Defaults.MaxAttpempts, retryer.MaxAttempts());

        // Retryable error
        //status code
        Assert.False(retryer.IsErrorRetryable(new("")));
        Assert.False(retryer.IsErrorRetryable(new ServiceException(404, null)));
        Assert.False(retryer.IsErrorRetryable(new ServiceException(403, null)));

        Assert.True(retryer.IsErrorRetryable(new ServiceException(401, null)));
        Assert.True(retryer.IsErrorRetryable(new ServiceException(408, null)));
        Assert.True(retryer.IsErrorRetryable(new ServiceException(429, null)));
        Assert.True(retryer.IsErrorRetryable(new ServiceException(500, null)));
        Assert.True(retryer.IsErrorRetryable(new ServiceException(599, null)));

        // error code
        var details = new Dictionary<string, string>();
        Assert.False(retryer.IsErrorRetryable(new("")));
        Assert.False(retryer.IsErrorRetryable(new ServiceException(403, null)));
        Assert.False(retryer.IsErrorRetryable(new ServiceException(403, details)));

        details = new() {
            { "Code", "UnSupportCode" }
        };
        Assert.False(retryer.IsErrorRetryable(new ServiceException(403, details)));

        details = new() {
            { "Code", "RequestTimeTooSkewed" }
        };
        Assert.True(retryer.IsErrorRetryable(new ServiceException(403, details)));

        details = new() {
            { "Code", "BadRequest" }
        };
        Assert.True(retryer.IsErrorRetryable(new ServiceException(403, details)));

        Assert.False(retryer.IsErrorRetryable(new("")));
        Assert.True(retryer.IsErrorRetryable(new InconsistentException("", "", "")));
        Assert.True(retryer.IsErrorRetryable(new RequestFailedException("fake")));

        // Delay
        Assert.True(retryer.RetryDelay(0, new()) > TimeSpan.FromSeconds(0));
        for (var i = 0; i < 128; i++) Assert.True(retryer.RetryDelay(1, new()) < Defaults.MaxBackOff);
    }
}