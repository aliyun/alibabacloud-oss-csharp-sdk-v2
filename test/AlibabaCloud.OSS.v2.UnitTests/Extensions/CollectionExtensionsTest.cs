using AlibabaCloud.OSS.v2.Extensions;

namespace AlibabaCloud.OSS.v2.UnitTests.Extensions;
public class CollectionExtensionsTest {

    [Fact]
    public void TestForEach() {
        var headers = new Dictionary<string, string> {
            {"X-Oss-Meta-Key-1", "value1"},
            {"X-Oss-Meta-Key-2", "value2"},
        };

        var newHeaders = new Dictionary<string, string> { };
        Assert.Empty(newHeaders);
        headers.ForEach(x => { newHeaders.Add(x.Key.ToLowerInvariant(), x.Value);});

        Assert.NotEmpty(newHeaders);
        Assert.Equal("value1", newHeaders["x-oss-meta-key-1"]);
        Assert.Equal("value2", newHeaders["x-oss-meta-key-2"]);
    }

}

