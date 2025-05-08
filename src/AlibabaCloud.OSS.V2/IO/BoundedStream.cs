using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace AlibabaCloud.OSS.V2.IO
{
    /// <summary>
    /// Represents read-only view over the portion of underlying stream.
    /// </summary>
    public sealed class BoundedStream : Stream
    {
        private long _length;
        private long _offset;

        /// <summary>
        /// Creates a new BoundedStream that wraps the given stream.
        /// </summary>
        /// <param name="stream">The wrapped input stream.</param>
        public BoundedStream(Stream stream)
        {
            this._length = stream.Length;
            BaseStream = stream;
        }

        /// <summary>
        /// Creates a new BoundedStream that wraps the given stream and limits it to a certain size.
        /// </summary>
        /// <param name="stream">The wrapped input stream.</param>
        /// <param name="offset">The offset in the underlying stream.</param>
        /// <param name="length">Total number of bytes to allow to be read from the stream. </param>
        public BoundedStream(Stream stream, long offset, long length)
        {
            BaseStream = stream;
            Adjust(offset, length);
        }

        /// <summary>
        /// Gets underlying stream.
        /// </summary>
        public Stream BaseStream { get; private set; }

        /// <summary>
        /// Adjust the stream bounds.
        /// </summary>
        /// <remarks>
        /// This method modifies <see cref="Stream.Position"/> property of the underlying stream.
        /// </remarks>
        /// <param name="offset">The offset in the underlying stream.</param>
        /// <param name="length">Total number of bytes to allow to be read from the stream. </param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="length"/> is larger than the remaining length of the underlying stream; or <paramref name="offset"/> if greater than the length of the underlying stream.</exception>
        public void Adjust(long offset, long length)
        {
            ThrowIfGreaterThan((ulong)offset, (ulong)BaseStream.Length, nameof(offset));
            ThrowIfGreaterThan((ulong)length, (ulong)(BaseStream.Length - offset), nameof(length));
            this._length = length;
            this._offset = offset;
            BaseStream.Position = offset;
        }

        /// <summary>
        /// Gets a value indicating whether the current stream supports reading.
        /// </summary>
        /// <value><see langword="true"/> if the stream supports reading; otherwise, <see langword="false"/>.</value>
        public override bool CanRead => BaseStream.CanRead;

        /// <summary>
        /// Gets a value indicating whether the current stream supports seeking.
        /// </summary>
        /// <value><see langword="true"/> if the stream supports seeking; otherwise, <see langword="false"/>.</value>
        public override bool CanSeek => BaseStream.CanSeek;

        /// <summary>
        /// Gets a value indicating whether the current stream supports writing.
        /// </summary>
        /// <value>Always <see langword="false"/>.</value>
        public override bool CanWrite => false;

        /// <inheritdoc/>
        public override long Length => _length;

        /// <inheritdoc/>
        public override long Position
        {
            get => BaseStream.Position - _offset;
            set
            {
                ThrowIfGreaterThan(value, _length, nameof(value));
                BaseStream.Position = _offset + value;
            }
        }

        private long RemainingBytes => _length - Position;

        /// <inheritdoc/>
        public override void Flush() => BaseStream.Flush();

        /// <inheritdoc/>
        public override Task FlushAsync(CancellationToken token = default) => BaseStream.FlushAsync(token);

        /// <inheritdoc/>
        public override bool CanTimeout => BaseStream.CanTimeout;

        /// <inheritdoc/>
        public override int ReadByte()
            => Position < _length ? BaseStream.ReadByte() : -1;

        /// <inheritdoc/>
        public override void WriteByte(byte value) => throw new NotSupportedException();

        /// <inheritdoc/>
        public override int Read(byte[] buffer, int offset, int count)
        {
            return BaseStream.Read(buffer, offset, (int)Math.Min(count, RemainingBytes));
        }

        /// <inheritdoc/>
        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback? callback, object? state)
        {
            count = (int)Math.Min(count, RemainingBytes);
            return BaseStream.BeginRead(buffer, offset, count, callback, state);
        }

        /// <inheritdoc/>
        public override int EndRead(IAsyncResult asyncResult) => BaseStream.EndRead(asyncResult);

        /// <inheritdoc/>
        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken token = default)
            => BaseStream.ReadAsync(buffer, offset, (int)Math.Min(count, RemainingBytes), token);

        /// <inheritdoc/>
        public override long Seek(long offset, SeekOrigin origin)
        {
            var newPosition = origin switch
            {
                SeekOrigin.Begin => offset,
                SeekOrigin.Current => Position + offset,
                SeekOrigin.End => _length + offset,
                _ => throw new ArgumentOutOfRangeException(nameof(origin))
            };

            if (newPosition < 0L)
                throw new IOException();

            ThrowIfGreaterThan(newPosition, _length, nameof(offset));

            Position = newPosition;
            return newPosition;
        }

        /// <inheritdoc/>
        public override void SetLength(long value)
        {
            ThrowIfGreaterThan((ulong)value, (ulong)(BaseStream.Length - BaseStream.Position), nameof(value));
            _length = value;
        }

        /// <inheritdoc/>
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

        /// <inheritdoc/>
        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken token = default) => Task.FromException(new NotSupportedException());

        /// <inheritdoc/>
        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback? callback, object? state)
            => throw new NotSupportedException();

        /// <inheritdoc/>
        public override void EndWrite(IAsyncResult asyncResult) => throw new InvalidOperationException();

        /// <inheritdoc/>
        public override int ReadTimeout
        {
            get => BaseStream.ReadTimeout;
            set => BaseStream.ReadTimeout = value;
        }

        /// <inheritdoc/>
        public override int WriteTimeout
        {
            get => BaseStream.WriteTimeout;
            set => BaseStream.WriteTimeout = value;
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            BaseStream.Dispose();
        }

        private static void ThrowIfGreaterThan<T>(T value, T other, string? paramName = null) where T : IComparable<T>
        {
            if (value.CompareTo(other) > 0)
            {
                throw new ArgumentOutOfRangeException(paramName, value, $"{paramName} ('{value}')must be less than or equal to  '{other}'");
            }
        }
    }
}
