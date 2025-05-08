using System.Text;
using AlibabaCloud.OSS.V2.IO;

namespace AlibabaCloud.OSS.V2.UnitTests.Models;

public class BoundedStreamTest
{
    [Theory]
    [InlineData(0, 4, "This")]
    [InlineData(5, 2, "is")]
    [InlineData(10, 4, "test")]
    public static void AdjustSetsBoundedOfStream(int offset, int length, string expected)
    {
        using var ms = new MemoryStream(Encoding.UTF8.GetBytes("This is a test"));
        using var bounded = new BoundedStream(ms);
        bounded.Adjust(offset, length);
        using StreamReader reader = new(bounded);
        Assert.Equal(expected, reader.ReadToEnd());
    }

    [Theory]
    [InlineData(0, 4, "This")]
    [InlineData(5, 2, "is")]
    [InlineData(10, 4, "test")]
    public static void CreatesBoundedOfStream(int offset, int length, string expected)
    {
        using var ms = new MemoryStream(Encoding.UTF8.GetBytes("This is a test"));
        using var bounded = new BoundedStream(ms, offset, length);
        using StreamReader reader = new(bounded);
        Assert.Equal(expected, reader.ReadToEnd());
    }

    [Fact]
    public static void ReadByteSequentially()
    {
        using var ms = new MemoryStream([1, 3, 5, 8, 12]);
        using var bounded = new BoundedStream(ms);
        Assert.Same(ms, bounded.BaseStream);
        Assert.Equal(0, bounded.Position);
        bounded.Adjust(0, 2);
        Assert.Equal(1, bounded.ReadByte());
        Assert.Equal(1, bounded.Position);

        Assert.Equal(3, bounded.ReadByte());
        Assert.Equal(2, bounded.Position);

        Assert.Equal(-1, bounded.ReadByte());
        Assert.Equal(2, bounded.Position);
    }

    [Fact]
    public static void CreatesAndReadByteSequentially()
    {
        using var ms = new MemoryStream([1, 3, 5, 8, 12]);
        using var bounded = new BoundedStream(ms, 0, 2);
        Assert.Same(ms, bounded.BaseStream);
        Assert.Equal(0, bounded.Position);

        Assert.Equal(1, bounded.ReadByte());
        Assert.Equal(1, bounded.Position);

        Assert.Equal(3, bounded.ReadByte());
        Assert.Equal(2, bounded.Position);

        Assert.Equal(-1, bounded.ReadByte());
        Assert.Equal(2, bounded.Position);
    }

    [Fact]
    public static void SetPosition()
    {
        using var ms = new MemoryStream([1, 3, 5, 8, 12]);
        using var bounded = new BoundedStream(ms);
        bounded.Adjust(1, 3);
        bounded.Position = 1;
        Assert.Equal(5, bounded.ReadByte());
        Assert.Equal(2, bounded.Position);
        bounded.Position = 0;
        Assert.Equal(3, bounded.ReadByte());
        Assert.Equal(1, bounded.Position);
    }

    [Fact]
    public static void CreatesAndSetPosition()
    {
        using var ms = new MemoryStream([1, 3, 5, 8, 12]);
        using var bounded = new BoundedStream(ms, 1, 3);
        bounded.Position = 1;
        Assert.Equal(5, bounded.ReadByte());
        Assert.Equal(2, bounded.Position);
        bounded.Position = 0;
        Assert.Equal(3, bounded.ReadByte());
        Assert.Equal(1, bounded.Position);
    }

    [Fact]
    public static void ReadRange()
    {
        using var ms = new MemoryStream([1, 3, 5, 8, 12]);
        using var bounded = new BoundedStream(ms);
        bounded.Adjust(1L, 2L);
        var buffer = new byte[4];
        Assert.Equal(2, bounded.Read(buffer, 0, buffer.Length));
        Assert.Equal(3, buffer[0]);
        Assert.Equal(5, buffer[1]);
        Assert.Equal(0, buffer[2]);
        Assert.Equal(0, buffer[3]);
        //read from the end of the stream
        Assert.Equal(-1, bounded.ReadByte());
    }

    [Fact]
    public static void CreatesAndReadRange()
    {
        using var ms = new MemoryStream([1, 3, 5, 8, 12]);
        using var bounded = new BoundedStream(ms, 1L, 2L);
        var buffer = new byte[4];
        Assert.Equal(2, bounded.Read(buffer, 0, buffer.Length));
        Assert.Equal(3, buffer[0]);
        Assert.Equal(5, buffer[1]);
        Assert.Equal(0, buffer[2]);
        Assert.Equal(0, buffer[3]);
        //read from the end of the stream
        Assert.Equal(-1, bounded.ReadByte());
    }

    [Fact]
    public static async Task ReadRangeAsync()
    {
        using var ms = new MemoryStream([1, 3, 5, 8, 12]);
        using var bounded = new BoundedStream(ms);
        bounded.Adjust(1L, 2L);
        var buffer = new byte[4];
        Assert.Equal(2, await bounded.ReadAsync(buffer, 0, buffer.Length));
        Assert.Equal(3, buffer[0]);
        Assert.Equal(5, buffer[1]);
        Assert.Equal(0, buffer[2]);
        Assert.Equal(0, buffer[3]);
        //read from the end of the stream
        Assert.Equal(-1, bounded.ReadByte());
    }

    [Fact]
    public static async Task CreatesAndReadRangeAsync()
    {
        using var ms = new MemoryStream([1, 3, 5, 8, 12]);
        using var bounded = new BoundedStream(ms, 1L, 2L);
        var buffer = new byte[4];
        Assert.Equal(2, await bounded.ReadAsync(buffer, 0, buffer.Length));
        Assert.Equal(3, buffer[0]);
        Assert.Equal(5, buffer[1]);
        Assert.Equal(0, buffer[2]);
        Assert.Equal(0, buffer[3]);
        //read from the end of the stream
        Assert.Equal(-1, bounded.ReadByte());
    }

    [Fact]
    public static void ReadApm()
    {
        using var ms = new MemoryStream([1, 3, 5, 8, 12]);
        using var bounded = new BoundedStream(ms);
        bounded.Adjust(1L, 2L);
        var buffer = new byte[4];
        var ar = bounded.BeginRead(buffer, 0, 2, null, null);
        Assert.Equal(2, bounded.EndRead(ar));
        Assert.Equal(3, buffer[0]);
        Assert.Equal(5, buffer[1]);
        Assert.Equal(0, buffer[2]);
    }

    [Fact]
    public static void CreatesAndReadApm()
    {
        using var ms = new MemoryStream([1, 3, 5, 8, 12]);
        using var bounded = new BoundedStream(ms, 1L, 2L);
        var buffer = new byte[4];
        var ar = bounded.BeginRead(buffer, 0, 2, null, null);
        Assert.Equal(2, bounded.EndRead(ar));
        Assert.Equal(3, buffer[0]);
        Assert.Equal(5, buffer[1]);
        Assert.Equal(0, buffer[2]);
    }

    [Fact]
    public static async Task ExceptionCheck()
    {
        using var ms = new MemoryStream([1, 3, 5, 8, 12]);
        using var bounded = new BoundedStream(ms);
        Assert.True(bounded.CanRead);
        Assert.True(bounded.CanSeek);
        Assert.False(bounded.CanWrite);
        Assert.Equal(ms.CanTimeout, bounded.CanTimeout);
        Assert.Throws<NotSupportedException>(() => bounded.WriteByte(2));
        Assert.Throws<NotSupportedException>(() => bounded.Write(new byte[3], 0, 3));
        Assert.Throws<NotSupportedException>(() => bounded.BeginWrite(new byte[2], 0, 2, null, null));
        Assert.Throws<InvalidOperationException>(() => bounded.EndWrite(Task.CompletedTask));
        await Assert.ThrowsAsync<NotSupportedException>(() => bounded.WriteAsync(new byte[3], 0, 3));
    }
}
