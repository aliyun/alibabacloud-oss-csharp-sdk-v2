using System.Collections.Generic;
using AlibabaCloud.OSS.V2.Internal;

namespace AlibabaCloud.OSS.V2.UnitTests.FileLike;

public class RangeParserTest
{
    private static Dictionary<string, string> Headers(params (string, string)[] kvs)
    {
        var d = new Dictionary<string, string>(System.StringComparer.OrdinalIgnoreCase);
        foreach (var (k, v) in kvs)
            d[k] = v;
        return d;
    }

    [Fact]
    public void OffsetAndSize_ContentLengthOnly()
    {
        var (offset, size) = RangeParser.ParseOffsetAndSizeFromHeaders(Headers(("Content-Length", "1024")));
        Assert.Equal(0, offset);
        Assert.Equal(1024, size);
    }

    [Fact]
    public void OffsetAndSize_NoHeaders_SizeIsMinusOne()
    {
        var (offset, size) = RangeParser.ParseOffsetAndSizeFromHeaders(Headers());
        Assert.Equal(0, offset);
        Assert.Equal(-1, size);
    }

    [Fact]
    public void OffsetAndSize_ContentRange_TotalKnown()
    {
        var (offset, size) = RangeParser.ParseOffsetAndSizeFromHeaders(Headers(
            ("Content-Length", "400"),
            ("Content-Range", "bytes 100-499/2000")));
        Assert.Equal(100, offset);
        Assert.Equal(2000, size);
    }

    [Fact]
    public void OffsetAndSize_ContentRange_StarTotal_KeepsContentLength()
    {
        var (offset, size) = RangeParser.ParseOffsetAndSizeFromHeaders(Headers(
            ("Content-Length", "400"),
            ("Content-Range", "bytes 100-499/*")));
        Assert.Equal(100, offset);
        Assert.Equal(400, size);
    }

    [Fact]
    public void OffsetAndSize_MalformedContentRange_Fails()
    {
        var (offset, size) = RangeParser.ParseOffsetAndSizeFromHeaders(Headers(
            ("Content-Range", "items 100-499/2000")));
        Assert.Equal(0, offset);
        Assert.Equal(-1, size);
    }

    [Fact]
    public void OffsetAndSize_InvalidContentLength_Fails()
    {
        var (offset, size) = RangeParser.ParseOffsetAndSizeFromHeaders(Headers(("Content-Length", "abc")));
        Assert.Equal(0, offset);
        Assert.Equal(-1, size);
    }

    [Fact]
    public void ContentRange_Valid()
    {
        Assert.True(RangeParser.TryParseContentRange("bytes 100-499/2000", out var from, out var to, out var total));
        Assert.Equal(100, from);
        Assert.Equal(499, to);
        Assert.Equal(2000, total);
    }

    [Fact]
    public void ContentRange_StarTotal_MinusOne()
    {
        Assert.True(RangeParser.TryParseContentRange("bytes 0-15/*", out var from, out var to, out var total));
        Assert.Equal(0, from);
        Assert.Equal(15, to);
        Assert.Equal(-1, total);
    }

    [Theory]
    [InlineData("")]
    [InlineData("items 0-15/16")]
    [InlineData("bytes 0-15")]           // no slash
    [InlineData("bytes 016/16")]         // no dash
    [InlineData("bytes a-15/16")]        // bad from
    [InlineData("bytes 0-b/16")]         // bad to
    public void ContentRange_Invalid_ReturnsFalse(string input)
    {
        Assert.False(RangeParser.TryParseContentRange(input, out _, out _, out _));
    }
}
