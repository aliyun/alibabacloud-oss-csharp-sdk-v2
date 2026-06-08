using AlibabaCloud.OSS.V2.Extensions;

namespace AlibabaCloud.OSS.V2.UnitTests.Extensions;

public class StringExtensionsTest
{
    [Theory]
    // basic decode
    [InlineData("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789.-_!()*", "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789.-_!()*")]
    [InlineData("%60%21%40%23%24%25%5E%26%2A%28%29%2B%3D%7B%7D%5B%5D%3A%3B%27%5C%7C%3C%3E%2C%3F%2F%20%22", "`!@#$%^&*()+={}[]:;'\\|<>,?/ \"")]
    [InlineData("hello%20world%21", "hello world!")]
    // space encoding
    [InlineData("%20", " ")]
    [InlineData("hello%20world", "hello world")]
    // multiple encoded chars
    [InlineData("%48%65%6C%6C%6F", "Hello")]
    // lowercase hex
    [InlineData("%2f", "/")]
    // mixed case hex
    [InlineData("%2F%3a%3A", "/::" )]
    // special chars
    [InlineData("%21%40%23%24%25", "!@#$%")]
    // tilde
    [InlineData("%7E", "~")]
    [InlineData("~", "~")]
    // empty string
    [InlineData("", "")]
    // no encoding
    [InlineData("plain-text", "plain-text")]
    // plus sign (Uri.UnescapeDataString does NOT treat + as space, consistent with C++/Java)
    [InlineData("hello+world", "hello+world")]
    [InlineData("+", "+")]
    // unicode
    [InlineData("%E4%B8%AD%E6%96%87", "中文")]
    // control characters
    [InlineData("%09", "\t")]
    [InlineData("%0A", "\n")]
    [InlineData("%0D", "\r")]
    // malformed percent encoding (pass through as-is)
    [InlineData("hello%", "hello%")]
    [InlineData("hello%A", "hello%A")]
    [InlineData("%", "%")]
    [InlineData("x%", "x%")]
    [InlineData("%%", "%%")]
    [InlineData("a%b", "a%b")]
    [InlineData("%G", "%G")]
    [InlineData("%GG", "%GG")]
    [InlineData("a%20b%c", "a b%c")]
    [InlineData("%%20", "% ")]
    public void TestUrlDecode(string input, string expected)
    {
        var actual = input.UrlDecode();
        Assert.Equal(expected, actual);
    }

    [Theory]
    // basic encode - safe chars: a-zA-Z0-9 - _ . ~
    [InlineData("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789.-_~", "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789.-_~")]
    [InlineData("`@#$%^&+={}[]:;'\\|<>,?/ \"", "%60%40%23%24%25%5E%26%2B%3D%7B%7D%5B%5D%3A%3B%27%5C%7C%3C%3E%2C%3F%2F%20%22")]
    [InlineData("hello world!", "hello%20world%21")]
    // tilde is safe (consistent with C++/Java)
    [InlineData("~", "~")]
    [InlineData("file~name", "file~name")]
    // empty string
    [InlineData("", "")]
    // unicode
    [InlineData("中文", "%E4%B8%AD%E6%96%87")]
    // slash is encoded
    [InlineData("/", "%2F")]
    // parentheses and asterisk are encoded by Uri.EscapeDataString
    [InlineData("(", "%28")]
    [InlineData(")", "%29")]
    [InlineData("*", "%2A")]
    [InlineData("!", "%21")]
    // control characters
    [InlineData("\t", "%09")]
    [InlineData("\n", "%0A")]
    [InlineData("\r", "%0D")]
    // plus sign is encoded
    [InlineData("+", "%2B")]
    public void TestUrlEncode(string input, string expected)
    {
        var actual = input.UrlEncode();
        Assert.Equal(expected, actual);
    }

    [Theory]
    // basic encode path
    [InlineData("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789.-_/", "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789.-_/")]
    [InlineData("`@#$%^&+={}[]:;'\\|<>,? \"!()*", "%60%40%23%24%25%5E%26%2B%3D%7B%7D%5B%5D%3A%3B%27%5C%7C%3C%3E%2C%3F%20%22%21%28%29%2A")]
    [InlineData("hello world!", "hello%20world%21")]
    [InlineData("", "")]
    // tilde is safe in path
    [InlineData("~", "~")]
    [InlineData("file~name", "file~name")]
    // slash is safe in path (unlike UrlEncode)
    [InlineData("path/to/file", "path/to/file")]
    // unicode
    [InlineData("中文", "%E4%B8%AD%E6%96%87")]
    // mixed safe and unsafe
    [InlineData("dir/file name.txt", "dir/file%20name.txt")]
    public void TestUrlEncodePath(string input, string expected)
    {
        var actual = input.UrlEncodePath();
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void TestUrlEncodeDecodeRoundTrip()
    {
        string[] inputs = [
            "hello world",
            "path/to/file name",
            "中文测试",
            "special!@#$%^&*()",
            "~tilde~test~",
            "\t\n\r",
            "key=value&foo=bar",
        ];
        foreach (var input in inputs)
        {
            Assert.Equal(input, input.UrlEncode().UrlDecode());
        }
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

    [Theory]
    [InlineData("cn-hangzhou", true)]
    [InlineData("us-east-1", true)]
    [InlineData("CN-hangzhou", false)]
    [InlineData("#ad,ad", false)]
    [InlineData("", false)]
    public void TestIsValidRegion(string region, bool expectValid)
    {
        Assert.Equal(expectValid, region.IsValidRegion());
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

    [Theory]
    [InlineData(default(string), true)]
    [InlineData("", true)]
    [InlineData("#?-invalid", true)]
    [InlineData("http://bucket", false)]
    public void TestToUri(string url, bool exceptNull)
    {
        var uri = url.ToUri();
        if (exceptNull)
        {
            Assert.Null(uri);
        }
        else
        {
            Assert.NotNull(uri);
            Assert.IsAssignableFrom<Uri>(uri);
            Assert.Equal("http", uri.Scheme);
        }
    }

    [Theory]
    [InlineData("123", false)]
    [InlineData("test", false)]
    [InlineData("test-123", false)]
    [InlineData("123-test", false)]
    [InlineData("123test", false)]
    [InlineData("12", true)]
    [InlineData("abcdefghij-abcdefghij-abcdefghij-abcdefghij-abcdefghij-abcdefghij", true)]
    [InlineData("-test", true)]
    [InlineData("test-", true)]
    [InlineData("test_123", true)]
    [InlineData("TEst", true)]
    [InlineData("#?123", true)]
    [InlineData("", true)]
    public void TestIsValidBucketName(string bucket, bool shouldThrow)
    {
        if (shouldThrow)
        {
            Assert.ThrowsAny<ArgumentException>(() => bucket.EnsureBucketNameValid(paramName: nameof(bucket)));
        }
        else
        {
            bucket.EnsureBucketNameValid();
        }
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
