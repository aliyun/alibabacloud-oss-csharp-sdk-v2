using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace AlibabaCloud.OSS.V2.Internal {
    // this is to work around the HttpClient behavior that it will dispose the request's content after to send.
    // While this behavior does not work with current code base as the caller suppose the stream can be re-used.
    internal class NopCloseRequestContent : WrapperStream {
        // timeout
        private CancellationTokenSource? _cancellationTokenSource;
        private TimeSpan _readTimeout;
        private bool _setDeadline = false;

        public NopCloseRequestContent(
            Stream stream,
            CancellationTokenSource? cancellationTokenSource = null,
            TimeSpan? readTimeout = null
        ) : base(stream) {
            _cancellationTokenSource = cancellationTokenSource;
            _readTimeout = readTimeout ?? TimeSpan.Zero;
            _setDeadline = cancellationTokenSource != null && readTimeout != null;
        }

        public override int Read(byte[] buffer, int offset, int count) {
            var n = base.Read(buffer, offset, count);
            if (n > 0 && _setDeadline) {
                nudgeDeadline();
            }
            return n;
        }

        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) {
            var n = await base.ReadAsync(buffer, offset, count, cancellationToken).ConfigureAwait(false);
            if (n > 0 && _setDeadline) {
                nudgeDeadline();
            }
            return n;
        }

        /// <summary>
        /// The Close implementation for this wrapper stream
        /// does not close the underlying stream.
        /// </summary>
        public override void Close() {
            //IGNORE
        }

        /// <summary>
        /// The Dispose implementation for this wrapper stream
        /// does not close the underlying stream.
        /// </summary>
        protected override void Dispose(bool disposing) {
            //IGNORE
        }

        private void nudgeDeadline() {
            _cancellationTokenSource?.CancelAfter(_readTimeout);
        }

        internal static NopCloseRequestContent? StreamFor(
            Stream? stream,
            CancellationTokenSource? cancellationTokenSource,
            TimeSpan? readTimeout
        ) {
            if (stream == null) {
                return null;
            }

            if (stream.CanSeek && stream.Length < 256 * 1024) {
                readTimeout = null;
                cancellationTokenSource = null;
            }
            return new NopCloseRequestContent(stream, cancellationTokenSource, readTimeout);
        }
    }
}
