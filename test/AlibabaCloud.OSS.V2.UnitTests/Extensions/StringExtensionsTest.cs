using AlibabaCloud.OSS.V2.Extensions;

namespace AlibabaCloud.OSS.V2.UnitTests.Extensions;

public class StringExtensionsTest
{
    [Fact]
    public void TestUrlDecode()
    {
        var val = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789.-_~";
        Assert.Equal("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789.-_~", val.UrlDecode());

        val = "%60%21%40%23%24%25%5E%26%2A%28%29%2B%3D%7B%7D%5B%5D%3A%3B%27%5C%7C%3C%3E%2C%3F%2F%20%22";
        Assert.Equal("`!@#$%^&*()+={}[]:;'\\|<>,?/ \"", val.UrlDecode());

        val = "hello%20world%21";
        Assert.Equal("hello world!", val.UrlDecode());
    }

    [Fact]
    public void TestUrlEncode()
    {
        var val = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789.-_!()*";
        Assert.Equal("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789.-_!()*", val.UrlEncode());

        val = "`@#$%^&+={}[]:;'\\|<>,?/ \"~";
        Assert.Equal("%60%40%23%24%25%5E%26%2B%3D%7B%7D%5B%5D%3A%3B%27%5C%7C%3C%3E%2C%3F%2F%20%22%7E", val.UrlEncode());

        val = "hello world!";
        Assert.Equal("hello%20world!", val.UrlEncode());
    }

    [Fact]
    public void TestUrlEncodePath()
    {
        var val = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789.-_/";
        Assert.Equal("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789.-_/", val.UrlEncodePath());

        val = "`@#$%^&+={}[]:;'\\|<>,? \"!()*";

        Assert.Equal(
            "%60%40%23%24%25%5E%26%2B%3D%7B%7D%5B%5D%3A%3B%27%5C%7C%3C%3E%2C%3F%20%22%21%28%29%2A",
            val.UrlEncodePath()
        );

        val = "hello world!";
        Assert.Equal("hello%20world%21", val.UrlEncodePath());

        val = "";
        Assert.Equal("", val.UrlEncodePath());
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

    [Fact]
    public void TestIsValidObjectName()
    {
        string[] keys = [
            "123",
            "#ADfa",
            "#ADfa?fasdk#ja"
        ];
        foreach (var key in keys) Assert.True(key.IsValidObjectName());

        keys = [
            ""
        ];
        foreach (var key in keys) Assert.False(key.IsValidObjectName());
    }
}
