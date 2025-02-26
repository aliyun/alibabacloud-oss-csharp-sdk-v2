namespace AlibabaCloud.OSS.v2.UnitTests;

public class MiscTest {
    [Fact]
    public void TestDictionary() {
        var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        Assert.Empty(headers);
        headers["test"] = "value1";
        Assert.Single(headers);
        Assert.Equal("value1", headers["test"]);

        headers["test"] = "value2";
        Assert.Single(headers);
        Assert.Equal("value2", headers["test"]);
    }
}