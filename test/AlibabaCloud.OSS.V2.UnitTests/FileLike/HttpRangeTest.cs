using AlibabaCloud.OSS.V2.Internal;

namespace AlibabaCloud.OSS.V2.UnitTests.FileLike;

public class HttpRangeTest
{
    [Fact]
    public void FormatHttpRange_ZeroZero_ReturnsNull()
    {
        Assert.Null(new HttpRange(0, 0).FormatHttpRange());
    }

    [Fact]
    public void FormatHttpRange_OffsetOnly_OpenEnded()
    {
        Assert.Equal("bytes=100-", new HttpRange(100, 0).FormatHttpRange());
    }

    [Fact]
    public void FormatHttpRange_OffsetAndCount_ClosedRange()
    {
        // last byte = offset + count - 1
        Assert.Equal("bytes=100-149", new HttpRange(100, 50).FormatHttpRange());
    }

    [Fact]
    public void FormatHttpRange_CountFromZeroOffset()
    {
        Assert.Equal("bytes=0-15", new HttpRange(0, 16).FormatHttpRange());
    }
}
