using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace AlibabaCloud.OSS.V2.Internal
{
    /// <summary>
    /// Asynchronously prefetches an object byte range into a bounded set of buffers using a
    /// background producer, so a sequential consumer can read without waiting on the network.
    /// Ported from the Go SDK's AsyncRangeReader (io_utils.go).
    /// </summary>
    internal sealed class AsyncRangeReader : IDisposable
    {
        /// <summary>
        /// Result of a single range GET issued by the producer.
        /// </summary>
        internal sealed class RangeGetOutput
        {
            public Stream? Body { get; set; }

            public long ContentLength { get; set; }

            public string? ContentRange { get; set; }

            public string? ETag { get; set; }
        }

        private sealed class Buf
        {
            public byte[] Data = Array.Empty<byte>();

            public int Length;

            public int Offset;

            public Exception? Error;

            public bool Eof;

            public bool IsEmpty => Length - Offset <= 0;
        }

        private readonly Func<HttpRange, CancellationToken, Task<RangeGetOutput>> _rangeGet;
        private readonly BlockingCollection<Buf> _ready = new BlockingCollection<Buf>();
        private readonly SemaphoreSlim _token;
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private readonly Task _producer;
        private readonly HttpRange _oriHttpRange;

        // Producer-only state.
        private HttpRange _httpRange;
        private long _gotsize;
        private int _size = Defaults.SoftStartInitial;
        private Stream? _in;
        private string? _etag;

        // Consumer-only state.
        private Buf? _cur;
        private long _offset;
        private Exception? _err;
        private bool _eof;

        private volatile bool _disposed;

        /// <summary>
        /// Creates a reader that prefetches <paramref name="httpRange"/> from the object using
        /// <paramref name="rangeGet"/>, filling up to <paramref name="buffers"/> buffers ahead of the consumer.
        /// </summary>
        /// <param name="rangeGet">Issues a range GET for the requested sub-range.</param>
        /// <param name="httpRange">The overall range to read. A <see cref="HttpRange.Count"/> of 0 means "to end".</param>
        /// <param name="etag">Expected object ETag; if empty, the first response ETag is adopted.</param>
        /// <param name="buffers">Number of read-ahead buffers (&gt; 0).</param>
        public AsyncRangeReader(
            Func<HttpRange, CancellationToken, Task<RangeGetOutput>> rangeGet,
            HttpRange httpRange,
            string? etag,
            int buffers)
        {
            if (buffers <= 0)
                throw new ArgumentException("number of buffers too small", nameof(buffers));

            _rangeGet = rangeGet ?? throw new ArgumentNullException(nameof(rangeGet));
            _httpRange = httpRange;
            _oriHttpRange = httpRange;
            _offset = httpRange.Offset;
            _etag = etag;
            _token = new SemaphoreSlim(buffers, buffers);

            _producer = Task.Run(() => ProducerLoopAsync(_cts.Token));
        }

        /// <summary>Total number of bytes consumed so far (absolute object offset).</summary>
        public long Offset => _offset;

        /// <summary>The original range this reader was created to fetch.</summary>
        public HttpRange OriginalRange => _oriHttpRange;

        /// <summary>
        /// Reads the next available prefetched bytes into <paramref name="p"/>. Returns 0 at end of range.
        /// Throws if the producer reported an error (source changed, range mismatch, or GET failure).
        /// </summary>
        public int Read(byte[] p, int off, int count)
        {
            if (p == null) throw new ArgumentNullException(nameof(p));
            if (count <= 0) return 0;

            if (_cur == null || _cur.IsEmpty)
            {
                if (!AdvanceBuffer())
                    return 0;
            }

            var cur = _cur!;
            var n = Math.Min(count, cur.Length - cur.Offset);
            Array.Copy(cur.Data, cur.Offset, p, off, n);
            cur.Offset += n;
            _offset += n;
            return n;
        }

        // Moves to the next buffer with data, or resolves the terminal state.
        // Returns true if _cur now holds readable data, false at clean end.
        private bool AdvanceBuffer()
        {
            while (true)
            {
                if (_cur != null)
                {
                    var prev = _cur;
                    _cur = null;
                    _token.Release();
                    if (prev.Error != null)
                    {
                        _err = prev.Error;
                        throw _err;
                    }
                    if (prev.Eof)
                    {
                        _eof = true;
                    }
                }

                if (_eof) return false;
                if (_err != null) throw _err;

                Buf b;
                try
                {
                    b = _ready.Take();
                }
                catch (InvalidOperationException)
                {
                    // Producer completed and the queue is drained.
                    if (_err != null) throw _err;
                    return false;
                }

                _cur = b;
                if (!_cur.IsEmpty)
                    return true;

                // Empty buffer is a terminal marker (EOF or error); loop to resolve it.
            }
        }

        private async Task ProducerLoopAsync(CancellationToken ct)
        {
            try
            {
                while (true)
                {
                    try
                    {
                        await _token.WaitAsync(ct).ConfigureAwait(false);
                    }
                    catch (OperationCanceledException)
                    {
                        return;
                    }

                    var b = new Buf();

                    if (_httpRange.Count > 0 && _gotsize > _httpRange.Count)
                    {
                        b.Eof = true;
                        _ready.Add(b);
                        return;
                    }

                    var bufSize = Defaults.AsyncReadBufferSize;
                    if (_size < Defaults.AsyncReadBufferSize)
                    {
                        bufSize = _size;
                        _size <<= 1;
                    }
                    b.Data = new byte[bufSize];

                    if (_in == null)
                    {
                        var err = await OpenBodyAsync(ct).ConfigureAwait(false);
                        if (err != null)
                        {
                            if (ct.IsCancellationRequested)
                                return;
                            b.Error = err;
                            _ready.Add(b);
                            return;
                        }
                    }

                    int n;
                    bool eof;
                    Exception? readErr = null;
                    try
                    {
                        (n, eof) = await ReadFillAsync(_in!, b.Data, ct).ConfigureAwait(false);
                    }
                    catch (OperationCanceledException)
                    {
                        return;
                    }
                    catch (Exception ex)
                    {
                        n = 0;
                        eof = false;
                        readErr = ex;
                    }

                    b.Length = n;
                    b.Offset = 0;
                    _httpRange = new HttpRange(_httpRange.Offset + n, _httpRange.Count);
                    _gotsize += n;
                    if (eof)
                        b.Eof = true;

                    // Non-EOF read errors are not surfaced to the consumer; instead the body is
                    // dropped and the range is re-fetched from the current offset (matches Go).
                    _ready.Add(b);

                    if (eof || readErr != null)
                    {
                        _in?.Dispose();
                        _in = null;
                        if (eof)
                            return;
                    }
                }
            }
            finally
            {
                _ready.CompleteAdding();
            }
        }

        // Issues the range GET, validates etag and start offset, and installs _in.
        // Returns null on success, or the error to hand to the consumer.
        private async Task<Exception?> OpenBodyAsync(CancellationToken ct)
        {
            var remains = _httpRange;
            if (_httpRange.Count > 0)
            {
                var gotNum = _httpRange.Offset - _oriHttpRange.Offset;
                if (gotNum > 0 && _httpRange.Count > gotNum)
                    remains = new HttpRange(_httpRange.Offset, _httpRange.Count - gotNum);
            }

            RangeGetOutput? output = null;
            try
            {
                output = await _rangeGet(remains, ct).ConfigureAwait(false);

                var etag = output.ETag ?? "";
                if (string.IsNullOrEmpty(_etag))
                    _etag = etag;
                if (!string.Equals(etag, _etag, StringComparison.Ordinal))
                    throw new InvalidOperationException(
                        $"Source file is changed, expect etag:{_etag} ,got etag:{etag}");

                long off = 0;
                if (output.ContentRange != null)
                    RangeParser.TryParseContentRange(output.ContentRange, out off, out _, out _);
                if (off != remains.Offset)
                    throw new InvalidOperationException(
                        $"Range get fail, expect offset:{remains.Offset}, got offset:{off}");
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                output?.Body?.Dispose();
                return null;
            }
            catch (Exception ex)
            {
                output?.Body?.Dispose();
                return ex;
            }

            var body = output!.Body!;
            if (remains.Count > 0)
                body = new LimitedReadStream(body, remains.Count);
            _in = body;
            return null;
        }

        private static async Task<(int n, bool eof)> ReadFillAsync(Stream s, byte[] buf, CancellationToken ct)
        {
            var n = 0;
            var eof = false;
            while (n < buf.Length)
            {
                var nn = await s.ReadAsync(buf, n, buf.Length - n, ct).ConfigureAwait(false);
                if (nn == 0)
                {
                    eof = true;
                    break;
                }
                n += nn;
            }
            return (n, eof);
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            _cts.Cancel();
            try
            {
                _producer.Wait(TimeSpan.FromSeconds(30));
            }
            catch
            {
                // Ignore: shutting down.
            }

            while (_ready.TryTake(out _))
            {
            }

            _in?.Dispose();
            _in = null;

            _cts.Dispose();
            _token.Dispose();
            _ready.Dispose();
        }

        /// <summary>
        /// Wraps a stream so at most a fixed number of bytes can be read from it.
        /// </summary>
        private sealed class LimitedReadStream : Stream
        {
            private readonly Stream _inner;
            private long _remaining;

            public LimitedReadStream(Stream inner, long limit)
            {
                _inner = inner;
                _remaining = limit;
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                if (_remaining <= 0) return 0;
                if (count > _remaining) count = (int)_remaining;
                var n = _inner.Read(buffer, offset, count);
                _remaining -= n;
                return n;
            }

            public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            {
                if (_remaining <= 0) return 0;
                if (count > _remaining) count = (int)_remaining;
                var n = await _inner.ReadAsync(buffer, offset, count, cancellationToken).ConfigureAwait(false);
                _remaining -= n;
                return n;
            }

            public override bool CanRead => true;

            public override bool CanSeek => false;

            public override bool CanWrite => false;

            public override long Length => throw new NotSupportedException();

            public override long Position
            {
                get => throw new NotSupportedException();
                set => throw new NotSupportedException();
            }

            public override void Flush()
            {
            }

            public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

            public override void SetLength(long value) => throw new NotSupportedException();

            public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                    _inner.Dispose();
                base.Dispose(disposing);
            }
        }
    }
}
