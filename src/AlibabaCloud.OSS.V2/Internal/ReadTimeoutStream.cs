﻿using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace AlibabaCloud.OSS.V2.Internal
{
    /// <summary>
    /// Read-timeout Stream that will throw a <see cref="OperationCanceledException"/> if it has to wait longer than a configurable timeout to read more data
    /// </summary>
    internal class ReadTimeoutStream : Stream
    {
        private readonly Stream _stream;
        private TimeSpan _readTimeout;
        private CancellationTokenSource _cancellationTokenSource = null!;

        public ReadTimeoutStream(Stream stream, TimeSpan readTimeout)
        {
            _stream = stream;
            _readTimeout = readTimeout;
            UpdateReadTimeout();
            InitializeTokenSource();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var source = StartTimeout(default, out var dispose);
            try
            {
                return _stream.Read(buffer, offset, count);
            }
            // We dispose stream on timeout so catch and check if cancellation token was cancelled
            catch (IOException ex)
            {
                CancellationHelper.ThrowIfCancellationRequestedOrTimeout(default, source.Token, ex, _readTimeout);
                throw;
            }
            // We dispose stream on timeout so catch and check if cancellation token was cancelled
            catch (ObjectDisposedException ex)
            {
                CancellationHelper.ThrowIfCancellationRequestedOrTimeout(default, source.Token, ex, _readTimeout);
                throw;
            }
            catch (OperationCanceledException ex)
            {
                CancellationHelper.ThrowIfCancellationRequestedOrTimeout(default, source.Token, ex, _readTimeout);
                throw;
            }
            finally
            {
                StopTimeout(source, dispose);
            }
        }

        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            var source = StartTimeout(cancellationToken, out bool dispose);
            try
            {
                return await _stream.ReadAsync(buffer, offset, count, source.Token).ConfigureAwait(false);
            }
            // We dispose stream on timeout so catch and check if cancellation token was cancelled
            catch (IOException ex)
            {
                CancellationHelper.ThrowIfCancellationRequestedOrTimeout(cancellationToken, source.Token, ex, _readTimeout);
                throw;
            }
            // We dispose stream on timeout so catch and check if cancellation token was cancelled
            catch (ObjectDisposedException ex)
            {
                CancellationHelper.ThrowIfCancellationRequestedOrTimeout(cancellationToken, source.Token, ex, _readTimeout);
                throw;
            }
            catch (OperationCanceledException ex)
            {
                CancellationHelper.ThrowIfCancellationRequestedOrTimeout(cancellationToken, source.Token, ex, _readTimeout);
                throw;
            }
            finally
            {
                StopTimeout(source, dispose);
            }
        }

        private CancellationTokenSource StartTimeout(CancellationToken additionalToken, out bool dispose)
        {
            if (_cancellationTokenSource.IsCancellationRequested)
            {
                InitializeTokenSource();
            }

            CancellationTokenSource source;
            if (additionalToken.CanBeCanceled)
            {
                source = CancellationTokenSource.CreateLinkedTokenSource(additionalToken, _cancellationTokenSource.Token);
                dispose = true;
            }
            else
            {
                source = _cancellationTokenSource;
                dispose = false;
            }

            _cancellationTokenSource.CancelAfter(_readTimeout);

            return source;
        }

        public override bool CanWrite => false;

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Flush()
        {
            // Flush is allowed on read-only stream
        }

        private void InitializeTokenSource()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _cancellationTokenSource.Token.Register(static state => ((ReadTimeoutStream)state!).DisposeStream(), this);
        }

        private void DisposeStream()
        {
            _stream.Dispose();
        }

        private void StopTimeout(CancellationTokenSource source, bool dispose)
        {
            _cancellationTokenSource.CancelAfter(Timeout.InfiniteTimeSpan);
            if (dispose)
            {
                source.Dispose();
            }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return _stream.Seek(offset, origin);
        }

        public override bool CanRead => _stream.CanRead;
        public override bool CanSeek => _stream.CanSeek;
        public override long Length => _stream.Length;

        public override long Position
        {
            get => _stream.Position;
            set => _stream.Position = value;
        }

        public override int ReadTimeout
        {
            get => (int)_readTimeout.TotalMilliseconds;
            set
            {
                _readTimeout = TimeSpan.FromMilliseconds(value);
                UpdateReadTimeout();
            }
        }

        private void UpdateReadTimeout()
        {
            try
            {
                if (_stream.CanTimeout)
                {
                    _stream.ReadTimeout = (int)_readTimeout.TotalMilliseconds;
                }
            }
            catch
            {
                // ignore
            }
        }

        public override void Close()
        {
            _stream.Close();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            _stream.Dispose();
            _cancellationTokenSource.Dispose();
        }
    }
}
