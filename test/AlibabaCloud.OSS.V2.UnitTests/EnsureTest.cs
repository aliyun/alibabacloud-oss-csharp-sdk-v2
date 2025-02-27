namespace AlibabaCloud.OSS.V2.UnitTests;

public class EnsureTest {
    [Fact]
    public void TestNotNull() {
        Assert.Equal("", Ensure.NotNull("", "filed.string"));
        try {
            string val = null;
            Ensure.NotNull(val, "filed.string");
            Assert.Fail("should not here");
        }
        catch (ArgumentNullException e) {
            Assert.Contains("filed.string", e.ToString());
        }
    }

    [Fact]
    public void TestNotEmptyString() {
        Assert.Equal("123", Ensure.NotEmptyString("123", "filed.string"));
        Assert.Equal("System.Exception: 123", Ensure.NotEmptyString(new Exception("123"), "filed.string"));

        // null
        try {
            Ensure.NotEmptyString(null, "filed.string");
            Assert.Fail("should not here");
        }
        catch (ArgumentNullException e) {
            Assert.Contains("filed.string", e.ToString());
        }

        // empty
        try {
            Ensure.NotEmptyString("", "filed.string");
            Assert.Fail("should not here");
        }
        catch (ArgumentException e) {
            Assert.Contains("filed.string", e.ToString());
        }

        // WhiteSpace
        try {
            Ensure.NotEmptyString("   ", "filed.string");
            Assert.Fail("should not here");
        }
        catch (ArgumentException e) {
            Assert.Contains("filed.string", e.ToString());
        }
    }
}