using System.IO;
using System.Threading.Tasks;
using AlibabaCloud.OSS.V2.Internal;

namespace AlibabaCloud.OSS.V2.UnitTests.Internal;

public class LimitedReadStreamTest
{
    [Fact]
    public void Read_LimitsTotalBytes()
    {
        var inner = new MemoryStream(new byte[100], writable: false);
        using var limited = new LimitedReadStream(inner, 50);

        var buffer = new byte[100];
        Assert.Equal(50, limited.Read(buffer, 0, buffer.Length));
        Assert.Equal(0, limited.Read(buffer, 0, buffer.Length));
    }

    [Fact]
    public void Read_LimitsToBufferSize()
    {
        var inner = new MemoryStream(new byte[100], writable: false);
        using var limited = new LimitedReadStream(inner, 100);

        var buffer = new byte[10];
        Assert.Equal(10, limited.Read(buffer, 0, buffer.Length));
        Assert.Equal(10, limited.Read(buffer, 0, buffer.Length));
    }

    [Fact]
    public void Read_StopsAtLimitAcrossMultipleCalls()
    {
        var inner = new MemoryStream(new byte[100], writable: false);
        using var limited = new LimitedReadStream(inner, 23);

        var buffer = new byte[10];
        Assert.Equal(10, limited.Read(buffer, 0, buffer.Length));
        Assert.Equal(10, limited.Read(buffer, 0, buffer.Length));
        Assert.Equal(3, limited.Read(buffer, 0, buffer.Length));
        Assert.Equal(0, limited.Read(buffer, 0, buffer.Length));
    }

    [Fact]
    public void Read_DoesNotOverflowWhenLimitExceedsIntMaxValue()
    {
        // The limit is larger than any single int Read call can request.
        // The wrapper must not cast _remaining to int incorrectly and must
        // still honor the underlying stream's actual end.
        var inner = new MemoryStream(new byte[5], writable: false);
        using var limited = new LimitedReadStream(inner, (long)int.MaxValue + 1);

        var buffer = new byte[5];
        Assert.Equal(5, limited.Read(buffer, 0, buffer.Length));
        Assert.Equal(0, limited.Read(buffer, 0, buffer.Length));
    }

    [Fact]
    public async Task ReadAsync_DoesNotOverflowWhenLimitExceedsIntMaxValue()
    {
        var inner = new MemoryStream(new byte[5], writable: false);
        using var limited = new LimitedReadStream(inner, (long)int.MaxValue + 1);

        var buffer = new byte[5];
        Assert.Equal(5, await limited.ReadAsync(buffer, 0, buffer.Length));
        Assert.Equal(0, await limited.ReadAsync(buffer, 0, buffer.Length));
    }
}
