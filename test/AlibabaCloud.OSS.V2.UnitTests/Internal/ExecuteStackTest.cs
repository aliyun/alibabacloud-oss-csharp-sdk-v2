using AlibabaCloud.OSS.V2.Internal;

namespace AlibabaCloud.OSS.V2.UnitTests.Internal;

public class ExecuteStackTest
{
    [Fact]
    public void TestExecuteStackNullArg()
    {
        var stack = new ExecuteStack(null);

        try
        {
            stack.Resolve();
            Assert.Fail("should not here");
        }
        catch (Exception e)
        {
            Assert.Contains("HttpTransport is null", e.ToString());
        }
    }
}
