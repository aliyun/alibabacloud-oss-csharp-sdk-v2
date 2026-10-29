using AlibabaCloud.OSS.V2.Internal;

namespace AlibabaCloud.OSS.V2.UnitTests.FileLike;

public class RangeUtilsTest
{
    [Fact]
    public void ParseRange_OpenEnded()
    {
        var r = RangeUtils.ParseRange("bytes=100-");
        Assert.NotNull(r);
        Assert.False(r.Value.IsSuffix);
        Assert.Equal(100, r.Value.Start);
        Assert.Equal(0, r.Value.Count);
    }

    [Fact]
    public void ParseRange_Closed()
    {
        var r = RangeUtils.ParseRange("bytes=100-199");
        Assert.NotNull(r);
        Assert.False(r.Value.IsSuffix);
        Assert.Equal(100, r.Value.Start);
        Assert.Equal(100, r.Value.Count);
    }

    [Fact]
    public void ParseRange_SingleByte()
    {
        var r = RangeUtils.ParseRange("bytes=100-100");
        Assert.NotNull(r);
        Assert.Equal(100, r.Value.Start);
        Assert.Equal(1, r.Value.Count);
    }

    [Fact]
    public void ParseRange_Suffix()
    {
        var r = RangeUtils.ParseRange("bytes=-500");
        Assert.NotNull(r);
        Assert.True(r.Value.IsSuffix);
        Assert.Equal(500, r.Value.SuffixLength);
    }

    [Fact]
    public void ParseRange_SuffixZero()
    {
        var r = RangeUtils.ParseRange("bytes=-0");
        Assert.NotNull(r);
        Assert.True(r.Value.IsSuffix);
        Assert.Equal(0, r.Value.SuffixLength);
    }

    [Fact]
    public void ParseRange_ReversedIsInvalid()
    {
        Assert.Null(RangeUtils.ParseRange("bytes=100-50"));
    }

    [Fact]
    public void ParseRange_EmptyIsInvalid()
    {
        Assert.Null(RangeUtils.ParseRange("bytes=100-99"));
    }

    [Fact]
    public void ParseRange_MaxEndOverflowIsInvalid()
    {
        Assert.Null(RangeUtils.ParseRange($"bytes=0-{long.MaxValue}"));
    }

    [Fact]
    public void ParseRange_NegativeStartIsInvalid()
    {
        Assert.Null(RangeUtils.ParseRange("bytes=-1-10"));
    }

    [Fact]
    public void ParseRange_NegativeEndIsInvalid()
    {
        Assert.Null(RangeUtils.ParseRange("bytes=0--1"));
    }

    [Fact]
    public void ParseRange_MultipleRangesIsInvalid()
    {
        Assert.Null(RangeUtils.ParseRange("bytes=0-10,20-30"));
    }

    [Fact]
    public void ParseRange_MissingDashIsInvalid()
    {
        Assert.Null(RangeUtils.ParseRange("bytes=010"));
    }

    [Fact]
    public void ParseRange_WhitespaceIsTolerated()
    {
        var r = RangeUtils.ParseRange("bytes= 100 - 199 ");
        Assert.NotNull(r);
        Assert.Equal(100, r.Value.Start);
        Assert.Equal(100, r.Value.Count);
    }
}
