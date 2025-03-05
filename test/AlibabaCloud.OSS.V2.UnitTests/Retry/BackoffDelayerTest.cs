namespace AlibabaCloud.OSS.V2.UnitTests.Retry;

public class BackoffDelayerTest
{
    [Fact]
    public void TestEqualJitterBackoff()
    {
        var delayer = new V2.Retry.EqualJitterBackoff(
            TimeSpan.FromMilliseconds(200),
            TimeSpan.FromSeconds(10)
        );

        Assert.True(delayer.BackofDelay(0, new Exception()) > TimeSpan.FromSeconds(0));
        for (var i = 0; i < 128; i++)
        {
            Assert.True(delayer.BackofDelay(1, new Exception()) < TimeSpan.FromSeconds(10));
        }
    }

    [Fact]
    public void TestFixedDelayBackoff()
    {
        var delayer = new V2.Retry.FixedDelayBackoff(
            TimeSpan.FromSeconds(10)
        );

        for (var i = 0; i < 128; i++)
        {
            Assert.Equal(TimeSpan.FromSeconds(10), delayer.BackofDelay(1, new Exception()));
        }
    }
}
