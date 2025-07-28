using AlibabaCloud.OSS.V2.Extensions;

namespace AlibabaCloud.OSS.V2.UnitTests.Extensions;

public class StringExtensionsTest
{
    [Theory]
    [InlineData("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789.-_!()*", "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789.-_!()*")]
    [InlineData("%60%21%40%23%24%25%5E%26%2A%28%29%2B%3D%7B%7D%5B%5D%3A%3B%27%5C%7C%3C%3E%2C%3F%2F%20%22", "`!@#$%^&*()+={}[]:;'\\|<>,?/ \"")]
    [InlineData("hello%20world%21", "hello world!")]
    public void TestUrlDecode(string input, string expected)
    {
        var actual = input.UrlDecode();
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789.-_!()*", "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789.-_!()*")]
    [InlineData("`@#$%^&+={}[]:;'\\|<>,?/ \"~", "%60%40%23%24%25%5E%26%2B%3D%7B%7D%5B%5D%3A%3B%27%5C%7C%3C%3E%2C%3F%2F%20%22%7E")]
    [InlineData("hello world!", "hello%20world!")]
    public void TestUrlEncode(string input, string expected)
    {
        var actual = input.UrlEncode();
        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789.-_/", "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789.-_/")]
    [InlineData("`@#$%^&+={}[]:;'\\|<>,? \"!()*", "%60%40%23%24%25%5E%26%2B%3D%7B%7D%5B%5D%3A%3B%27%5C%7C%3C%3E%2C%3F%20%22%21%28%29%2A")]
    [InlineData("hello world!", "hello%20world%21")]
    [InlineData("", "")]
    public void TestUrlEncodePath(string input, string expected)
    {
        var actual = input.UrlEncodePath();
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void TestIsEmpty() { }

    [Fact]
    public void TestIsNotEmpty() { }

    [Fact]
    public void TestJoinToString() { }

    [Fact]
    public void TestSafeString() { }

    [Fact]
    public void TestAddScheme()
    {
        var input = "";
        Assert.NotNull(input);
        Assert.Equal("", input.AddScheme(true));
        Assert.Equal("", input.AddScheme(false));

        input = "oss://bucket/key";
        Assert.Equal("oss://bucket/key", input.AddScheme(true));
        Assert.Equal("oss://bucket/key", input.AddScheme(false));

        input = "http://bucket/key";
        Assert.Equal("http://bucket/key", input.AddScheme(true));
        Assert.Equal("http://bucket/key", input.AddScheme(false));

        input = "bucket/key";
        Assert.Equal("http://bucket/key", input.AddScheme(true));
        Assert.Equal("https://bucket/key", input.AddScheme(false));
    }

    [Fact]
    public void TestIsValidRegion()
    {
        string[] regions = ["cn-hangzhou", "us-east-1"];
        foreach (var region in regions) Assert.True(region.IsValidRegion());

        regions = ["CN-hangzhou", "#ad,ad", ""];
        foreach (var region in regions) Assert.False(region.IsValidRegion());
    }

    [Fact]
    public void TestToEndpoint()
    {
        const string region = "cn-hangzhou";
        Assert.Equal("https://oss-cn-hangzhou-internal.aliyuncs.com", region.ToEndpoint(false, "internal"));
        Assert.Equal("http://oss-cn-hangzhou-internal.aliyuncs.com", region.ToEndpoint(true, "internal"));

        Assert.Equal("https://cn-hangzhou.oss.aliyuncs.com", region.ToEndpoint(false, "dual-stack"));
        Assert.Equal("http://cn-hangzhou.oss.aliyuncs.com", region.ToEndpoint(true, "dual-stack"));

        Assert.Equal("https://oss-accelerate.aliyuncs.com", region.ToEndpoint(false, "accelerate"));
        Assert.Equal("http://oss-accelerate.aliyuncs.com", region.ToEndpoint(true, "accelerate"));

        Assert.Equal("https://oss-accelerate-overseas.aliyuncs.com", region.ToEndpoint(false, "overseas"));
        Assert.Equal("http://oss-accelerate-overseas.aliyuncs.com", region.ToEndpoint(true, "overseas"));

        Assert.Equal("https://oss-cn-hangzhou.aliyuncs.com", region.ToEndpoint(false, ""));
        Assert.Equal("http://oss-cn-hangzhou.aliyuncs.com", region.ToEndpoint(true, ""));
    }

    [Fact]
    public void TestToUri()
    {
        string url = null;
        Assert.Null(url.ToUri());

        url = "";
        Assert.Null(url.ToUri());

        url = "#?-invalid";
        Assert.Null(url.ToUri());

        url = "http://bucket";
        var uri = url.ToUri();
        Assert.NotNull(uri);
        Assert.IsAssignableFrom<Uri>(uri);
        Assert.Equal("http", uri.Scheme);
    }

    [Fact]
    public void TestIsValidBucketName()
    {
        string[] buckets = [
            "123",
            "test",
            "test-123",
            "123-test",
            "123test"
        ];
        foreach (var bucket in buckets) Assert.True(bucket.IsValidBucketName());

        buckets = [
            "12",
            "abcdefghij-abcdefghij-abcdefghij-abcdefghij-abcdefghij-abcdefghij",
            "-test",
            "test-",
            "test_123",
            "TEst",
            "#?123",
            ""
        ];
        foreach (var bucket in buckets) Assert.False(bucket.IsValidBucketName());
    }

    [Theory]
    [InlineData("123", false)]
    [InlineData("#ADfa", false)]
    [InlineData("#ADfa?fasdk#ja", false)]
    [InlineData("", true)]
    public void TestIsValidObjectName(string key, bool shouldThrow)
    {
        if (shouldThrow)
        {
            Assert.ThrowsAny<ArgumentException>(() => key.EnsureObjectNameValid(paramName: nameof(key)));
        }
        else
        {
            key.EnsureObjectNameValid();
        }
    }
}
