using AlibabaCloud.OSS.V2.Retry;

namespace AlibabaCloud.OSS.V2.UnitTests.Retry;

public class RetryerTest
{
    [Fact]
    public void TestNopRetryer()
    {
        var retryer = new NopRetryer();
        Assert.Equal(1, retryer.MaxAttempts());
        Assert.False(retryer.IsErrorRetryable(new("")));

        try
        {
            retryer.RetryDelay(1, new());
            Assert.Fail("should not here");
        }
        catch (NotImplementedException e)
        {
            Assert.Contains("not retrying any attempt errors", e.ToString());
        }
    }

    [Fact]
    public void TestStandardRetryerDefault()
    {
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

    [Fact]
    public void TestStandardRetryerBackoffParametersNotSwapped()
    {
        var retryer = new StandardRetryer();

        // With default settings the max backoff should be Defaults.MaxBackOff (20 s),
        // not Defaults.BaseDelay (200 ms). Sample enough delays at a high attempt
        // count to make it virtually certain that at least one exceeds BaseDelay.
        var maxObserved = TimeSpan.Zero;
        for (var i = 0; i < 200; i++)
        {
            var delay = retryer.RetryDelay(10, new Exception());
            Assert.True(delay >= TimeSpan.Zero);
            Assert.True(delay <= Defaults.MaxBackOff);
            if (delay > maxObserved)
            {
                maxObserved = delay;
            }
        }

        Assert.True(maxObserved > Defaults.BaseDelay,
            $"expected some delay > {Defaults.BaseDelay} but max was {maxObserved}");
    }
}
