#nullable enable
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AlibabaCloud.OSS.V2.Internal;

namespace AlibabaCloud.OSS.V2.UnitTests.FileLike;

public class AsyncRangeReaderTest
{
    private static byte[] MakeData(int size)
    {
        var data = new byte[size];
        for (var i = 0; i < size; i++)
            data[i] = (byte)(i % 251);
        return data;
    }

    // Serves an in-memory object over the range-get delegate, honoring offset/count and
    // returning the appropriate Content-Range/ETag headers.
    private static Func<HttpRange, CancellationToken, Task<AsyncRangeReader.RangeGetOutput>> MakeGetter(
        byte[] data, string etag = "\"abc\"", Action<HttpRange>? onGet = null)
    {
        return (range, ct) =>
        {
            onGet?.Invoke(range);
            var start = (int)range.Offset;
            var count = range.Count > 0 ? (int)range.Count : data.Length - start;
            if (start + count > data.Length)
                count = data.Length - start;

            var slice = new byte[count];
            Array.Copy(data, start, slice, 0, count);

            var last = start + count - 1;
            var output = new AsyncRangeReader.RangeGetOutput
            {
                Body = new MemoryStream(slice, writable: false),
                ContentLength = count,
                ContentRange = $"bytes {start}-{last}/{data.Length}",
                ETag = etag
            };
            return Task.FromResult(output);
        };
    }

    private static byte[] ReadAll(AsyncRangeReader reader, int chunk)
    {
        using var ms = new MemoryStream();
        var buf = new byte[chunk];
        while (true)
        {
            var n = reader.Read(buf, 0, buf.Length);
            if (n == 0) break;
            ms.Write(buf, 0, n);
        }
        return ms.ToArray();
    }

    [Fact]
    public void Ctor_ZeroBuffers_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            new AsyncRangeReader(MakeGetter(MakeData(16)), new HttpRange(0, 0), null, 0));
    }

    [Fact]
    public void Ctor_NullGetter_Throws()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new AsyncRangeReader(null!, new HttpRange(0, 0), null, 2));
    }

    [Fact]
    public void SequentialRead_WholeObject()
    {
        var data = MakeData(3 * 1024 * 1024 + 12345);
        using var reader = new AsyncRangeReader(MakeGetter(data), new HttpRange(0, 0), null, 3);
        var got = ReadAll(reader, 64 * 1024);
        Assert.Equal(data, got);
        Assert.Equal(data.Length, reader.Offset);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(7)]
    [InlineData(1024)]
    [InlineData(700 * 1024)]
    [InlineData(4 * 1024 * 1024)]
    public void ChunkedRead_VaryingChunkSizes(int chunk)
    {
        var data = MakeData(2 * 1024 * 1024 + 99);
        using var reader = new AsyncRangeReader(MakeGetter(data), new HttpRange(0, 0), null, 2);
        var got = ReadAll(reader, chunk);
        Assert.Equal(data, got);
    }

    [Fact]
    public void SoftStart_CrossesBufferBoundaries()
    {
        // Larger than SoftStartInitial + one full buffer so multiple producer buffers are used.
        var data = MakeData(Defaults.SoftStartInitial + Defaults.AsyncReadBufferSize + 4096);
        using var reader = new AsyncRangeReader(MakeGetter(data), new HttpRange(0, 0), null, 4);
        var got = ReadAll(reader, 333);
        Assert.Equal(data, got);
    }

    [Fact]
    public void BoundedRange_ReadsOnlyCount()
    {
        var data = MakeData(1024 * 1024);
        const int offset = 1000;
        const int count = 500 * 1024;
        using var reader = new AsyncRangeReader(MakeGetter(data), new HttpRange(offset, count), null, 3);
        var got = ReadAll(reader, 7777);

        var expected = new byte[count];
        Array.Copy(data, offset, expected, 0, count);
        Assert.Equal(expected, got);
    }

    [Fact]
    public void FromOffset_ReadsToEnd()
    {
        var data = MakeData(1024 * 1024 + 5);
        const int offset = 4096;
        using var reader = new AsyncRangeReader(MakeGetter(data), new HttpRange(offset, 0), null, 2);
        var got = ReadAll(reader, 8192);

        var expected = new byte[data.Length - offset];
        Array.Copy(data, offset, expected, 0, expected.Length);
        Assert.Equal(expected, got);
    }

    [Fact]
    public void EmptyObject_ReturnsZeroImmediately()
    {
        var data = MakeData(0);
        using var reader = new AsyncRangeReader(MakeGetter(data), new HttpRange(0, 0), null, 2);
        var buf = new byte[16];
        Assert.Equal(0, reader.Read(buf, 0, buf.Length));
    }

    [Fact]
    public void RepeatedReadAfterEof_KeepsReturningZero()
    {
        var data = MakeData(1024);
        using var reader = new AsyncRangeReader(MakeGetter(data), new HttpRange(0, 0), null, 2);
        ReadAll(reader, 256);
        var buf = new byte[16];
        Assert.Equal(0, reader.Read(buf, 0, buf.Length));
        Assert.Equal(0, reader.Read(buf, 0, buf.Length));
    }

    [Fact]
    public void EtagChanged_Throws()
    {
        var data = MakeData(512 * 1024);
        // Getter always returns a different etag than the expected one.
        var reader = new AsyncRangeReader(MakeGetter(data, "\"changed\""), new HttpRange(0, 0), "\"expected\"", 2);
        var buf = new byte[64 * 1024];
        var ex = Assert.Throws<InvalidOperationException>(() =>
        {
            while (reader.Read(buf, 0, buf.Length) > 0) { }
        });
        Assert.Contains("Source file is changed", ex.Message);
        reader.Dispose();
    }

    [Fact]
    public void ContentRangeOffsetMismatch_Throws()
    {
        // Getter reports a start offset that does not match the requested one.
        Func<HttpRange, CancellationToken, Task<AsyncRangeReader.RangeGetOutput>> badGetter = (range, ct) =>
        {
            var output = new AsyncRangeReader.RangeGetOutput
            {
                Body = new MemoryStream(new byte[16]),
                ContentLength = 16,
                ContentRange = "bytes 999-1014/2000",
                ETag = "\"abc\""
            };
            return Task.FromResult(output);
        };

        var reader = new AsyncRangeReader(badGetter, new HttpRange(0, 0), null, 2);
        var buf = new byte[64];
        var ex = Assert.Throws<InvalidOperationException>(() =>
        {
            while (reader.Read(buf, 0, buf.Length) > 0) { }
        });
        Assert.Contains("Range get fail", ex.Message);
        reader.Dispose();
    }

    [Fact]
    public void GetterThrows_PropagatesToConsumer()
    {
        Func<HttpRange, CancellationToken, Task<AsyncRangeReader.RangeGetOutput>> throwingGetter =
            (range, ct) => throw new IOException("network down");

        var reader = new AsyncRangeReader(throwingGetter, new HttpRange(0, 0), null, 2);
        var buf = new byte[64];
        Assert.Throws<IOException>(() =>
        {
            while (reader.Read(buf, 0, buf.Length) > 0) { }
        });
        reader.Dispose();
    }

    [Fact]
    public void FirstEtagAdopted_WhenNoneExpected()
    {
        var data = MakeData(256 * 1024);
        // No expected etag supplied; the reader should adopt the first response etag and succeed.
        using var reader = new AsyncRangeReader(MakeGetter(data, "\"srv\""), new HttpRange(0, 0), null, 2);
        var got = ReadAll(reader, 4096);
        Assert.Equal(data, got);
    }

    [Fact]
    public void Dispose_BeforeFullyRead_DoesNotThrow()
    {
        var data = MakeData(5 * 1024 * 1024);
        var reader = new AsyncRangeReader(MakeGetter(data), new HttpRange(0, 0), null, 3);
        var buf = new byte[1024];
        reader.Read(buf, 0, buf.Length);
        reader.Dispose();
    }

    [Fact]
    public void Ctor_NegativeBuffers_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            new AsyncRangeReader(MakeGetter(MakeData(16)), new HttpRange(0, 0), null, -1));
    }

    [Fact]
    public void Dispose_Twice_NoOp()
    {
        var data = MakeData(1024);
        var reader = new AsyncRangeReader(MakeGetter(data), new HttpRange(0, 0), null, 2);
        ReadAll(reader, 256);
        reader.Dispose();
        reader.Dispose();
    }

    [Fact]
    public void MatchingEtag_Succeeds()
    {
        var data = MakeData(256 * 1024);
        using var reader = new AsyncRangeReader(MakeGetter(data, "\"same\""), new HttpRange(0, 0), "\"same\"", 2);
        var got = ReadAll(reader, 4096);
        Assert.Equal(data, got);
    }

    [Fact]
    public void NegativeCount_ReadsToEnd()
    {
        var data = MakeData(512 * 1024 + 33);
        const int offset = 3;
        // A negative count means "read to the end of the object" (Go's Count == -1).
        using var reader = new AsyncRangeReader(MakeGetter(data), new HttpRange(offset, -1), null, 3);
        var got = ReadAll(reader, 8192);

        var expected = new byte[data.Length - offset];
        Array.Copy(data, offset, expected, 0, expected.Length);
        Assert.Equal(expected, got);
    }

    // Mirrors Go's TestAsyncRangeReaderSizes: tiny inputs (length 0..30) crossed with source-read
    // behavior (full / one-byte / half) and consumer read chunk sizes (1,2,3,4,5,7). Go's fourth
    // "data+err" reader (returns bytes and io.EOF in the same call) has no Stream analogue because
    // Stream.Read always signals EOF with a separate 0-return; Go's "timeout" reader is exempted
    // from the assertion there too. Go's bufsizes dimension controls a bufio wrapper on the source,
    // which maps to the source-read helpers here rather than to any AsyncRangeReader knob (its buffer
    // size is fixed with a soft-start ramp), so it is folded in rather than multiplied out.
    [Theory]
    [InlineData("full")]
    [InlineData("byte")]
    [InlineData("half")]
    public void SizeMatrix_TinyInputs_AllReadPatterns(string readMaker)
    {
        var readSizes = new[] { 1, 2, 3, 4, 5, 7 };
        for (var textLen = 0; textLen <= 30; textLen++)
        {
            var data = MakeData(textLen);
            foreach (var m in readSizes)
            {
                Func<HttpRange, CancellationToken, Task<AsyncRangeReader.RangeGetOutput>> getter = (range, ct) =>
                {
                    var start = (int)range.Offset;
                    var count = data.Length - start;
                    var slice = new byte[count];
                    Array.Copy(data, start, slice, 0, count);
                    Stream body = readMaker switch
                    {
                        "byte" => new OneByteStream(slice),
                        "half" => new HalfReadStream(slice),
                        _ => new MemoryStream(slice, writable: false)
                    };
                    var last = start + count - 1;
                    return Task.FromResult(new AsyncRangeReader.RangeGetOutput
                    {
                        Body = body,
                        ContentLength = count,
                        ContentRange = $"bytes {start}-{last}/{data.Length}",
                        ETag = "\"abc\""
                    });
                };
                using var reader = new AsyncRangeReader(getter, new HttpRange(0, 0), null, 1);
                var got = ReadInChunks(reader, m);
                Assert.Equal(data, got);
            }
        }
    }

    // Reads exactly m bytes per call, accumulating until EOF (mirrors Go's reads(buf, m) helper).
    private static byte[] ReadInChunks(AsyncRangeReader reader, int m)
    {
        using var ms = new MemoryStream();
        var buf = new byte[m];
        while (true)
        {
            var n = reader.Read(buf, 0, m);
            if (n == 0) break;
            ms.Write(buf, 0, n);
        }
        return ms.ToArray();
    }

    // Returns ceil(count/2) bytes per Read (mirrors iotest.HalfReader).
    private sealed class HalfReadStream : Stream
    {
        private readonly byte[] _data;
        private int _pos;
        public HalfReadStream(byte[] data) => _data = data;
        public override int Read(byte[] buffer, int offset, int count)
        {
            if (_pos >= _data.Length || count == 0) return 0;
            var want = (count + 1) / 2;
            var n = Math.Min(want, _data.Length - _pos);
            Array.Copy(_data, _pos, buffer, offset, n);
            _pos += n;
            return n;
        }
        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken ct)
            => Task.FromResult(Read(buffer, offset, count));
        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => throw new NotSupportedException();
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public override void Flush() { }
        public override long Seek(long o, SeekOrigin s) => throw new NotSupportedException();
        public override void SetLength(long v) => throw new NotSupportedException();
        public override void Write(byte[] b, int o, int c) => throw new NotSupportedException();
    }

    // Wraps a stream so it returns at most one byte per Read, exercising the producer's
    // fill loop against a slow/partial underlying stream.
    private sealed class OneByteStream : Stream
    {
        private readonly byte[] _data;
        private int _pos;
        public OneByteStream(byte[] data) => _data = data;
        public override int Read(byte[] buffer, int offset, int count)
        {
            if (_pos >= _data.Length || count == 0) return 0;
            buffer[offset] = _data[_pos++];
            return 1;
        }
        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken ct)
            => Task.FromResult(Read(buffer, offset, count));
        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => throw new NotSupportedException();
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public override void Flush() { }
        public override long Seek(long o, SeekOrigin s) => throw new NotSupportedException();
        public override void SetLength(long v) => throw new NotSupportedException();
        public override void Write(byte[] b, int o, int c) => throw new NotSupportedException();
    }

    [Fact]
    public void PartialReadStream_ReadsFullData()
    {
        var data = MakeData(Defaults.SoftStartInitial + 5000);
        Func<HttpRange, CancellationToken, Task<AsyncRangeReader.RangeGetOutput>> getter = (range, ct) =>
        {
            var start = (int)range.Offset;
            var count = range.Count > 0 ? (int)range.Count : data.Length - start;
            var slice = new byte[count];
            Array.Copy(data, start, slice, 0, count);
            var last = start + count - 1;
            return Task.FromResult(new AsyncRangeReader.RangeGetOutput
            {
                Body = new OneByteStream(slice),
                ContentLength = count,
                ContentRange = $"bytes {start}-{last}/{data.Length}",
                ETag = "\"abc\""
            });
        };
        using var reader = new AsyncRangeReader(getter, new HttpRange(0, 0), null, 3);
        var got = ReadAll(reader, 4096);
        Assert.Equal(data, got);
    }

    // A stream that serves bytes then throws once, forcing the producer to re-fetch and resume.
    private sealed class BreakOnceStream : Stream
    {
        private readonly byte[] _data;
        private int _pos;
        private readonly int _breakAt;
        public BreakOnceStream(byte[] data, int breakAt) { _data = data; _breakAt = breakAt; }
        public override int Read(byte[] buffer, int offset, int count)
        {
            if (_pos >= _breakAt) throw new IOException("broken mid-stream");
            var n = Math.Min(count, _breakAt - _pos);
            Array.Copy(_data, _pos, buffer, offset, n);
            _pos += n;
            return n;
        }
        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken ct)
            => Task.FromResult(Read(buffer, offset, count));
        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => throw new NotSupportedException();
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public override void Flush() { }
        public override long Seek(long o, SeekOrigin s) => throw new NotSupportedException();
        public override void SetLength(long v) => throw new NotSupportedException();
        public override void Write(byte[] b, int o, int c) => throw new NotSupportedException();
    }

    [Fact]
    public void TransientReadError_ResumesAndCompletes()
    {
        var data = MakeData(300 * 1024);
        var calls = 0;
        Func<HttpRange, CancellationToken, Task<AsyncRangeReader.RangeGetOutput>> getter = (range, ct) =>
        {
            var start = (int)range.Offset;
            var count = range.Count > 0 ? (int)range.Count : data.Length - start;
            var slice = new byte[count];
            Array.Copy(data, start, slice, 0, count);
            var last = start + count - 1;
            var cr = $"bytes {start}-{last}/{data.Length}";

            var mine = Interlocked.Increment(ref calls);
            Stream body = mine == 1
                ? new BreakOnceStream(slice, Math.Max(1, count / 2))
                : new MemoryStream(slice, writable: false);
            return Task.FromResult(new AsyncRangeReader.RangeGetOutput
            {
                Body = body,
                ContentLength = count,
                ContentRange = cr,
                ETag = "\"abc\""
            });
        };
        using var reader = new AsyncRangeReader(getter, new HttpRange(0, 0), null, 2);
        var got = ReadAll(reader, 4096);
        Assert.Equal(data, got);
        Assert.True(calls >= 2);
    }

    [Fact]
    public void OffsetMismatchOnResume_Throws()
    {
        var data = MakeData(200 * 1024);
        var calls = 0;
        Func<HttpRange, CancellationToken, Task<AsyncRangeReader.RangeGetOutput>> getter = (range, ct) =>
        {
            var start = (int)range.Offset;
            var count = range.Count > 0 ? (int)range.Count : data.Length - start;
            var slice = new byte[count];
            Array.Copy(data, start, slice, 0, count);

            var mine = Interlocked.Increment(ref calls);
            if (mine == 1)
            {
                var last = start + count - 1;
                return Task.FromResult(new AsyncRangeReader.RangeGetOutput
                {
                    Body = new BreakOnceStream(slice, Math.Max(1, count / 2)),
                    ContentLength = count,
                    ContentRange = $"bytes {start}-{last}/{data.Length}",
                    ETag = "\"abc\""
                });
            }
            // On resume, report a start offset that does not match the requested one.
            return Task.FromResult(new AsyncRangeReader.RangeGetOutput
            {
                Body = new MemoryStream(slice, writable: false),
                ContentLength = count,
                ContentRange = $"bytes 7-{6 + count}/{data.Length}",
                ETag = "\"abc\""
            });
        };
        var reader = new AsyncRangeReader(getter, new HttpRange(0, 0), null, 2);
        var buf = new byte[8192];
        var ex = Assert.Throws<InvalidOperationException>(() =>
        {
            while (reader.Read(buf, 0, buf.Length) > 0) { }
        });
        Assert.Contains("Range get fail", ex.Message);
        reader.Dispose();
    }

    [Fact]
    public void ErrorOnResume_PropagatesAfterPartialData()
    {
        var data = MakeData(200 * 1024);
        var calls = 0;
        Func<HttpRange, CancellationToken, Task<AsyncRangeReader.RangeGetOutput>> getter = (range, ct) =>
        {
            var mine = Interlocked.Increment(ref calls);
            if (mine == 1)
            {
                var start = (int)range.Offset;
                var count = range.Count > 0 ? (int)range.Count : data.Length - start;
                var slice = new byte[count];
                Array.Copy(data, start, slice, 0, count);
                var last = start + count - 1;
                return Task.FromResult(new AsyncRangeReader.RangeGetOutput
                {
                    Body = new BreakOnceStream(slice, Math.Max(1, count / 2)),
                    ContentLength = count,
                    ContentRange = $"bytes {start}-{last}/{data.Length}",
                    ETag = "\"abc\""
                });
            }
            throw new IOException("range get fail on resume");
        };
        var reader = new AsyncRangeReader(getter, new HttpRange(0, 0), null, 2);
        var buf = new byte[8192];
        Assert.Throws<IOException>(() =>
        {
            while (reader.Read(buf, 0, buf.Length) > 0) { }
        });
        reader.Dispose();
    }
}
