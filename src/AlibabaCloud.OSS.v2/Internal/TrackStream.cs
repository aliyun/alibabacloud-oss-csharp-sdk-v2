using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace AlibabaCloud.OSS.v2.Internal {
    /// <summary>
    /// Read-only that writes to w what it reads from stream.
    /// </summary>
    internal class TrackStream : WrapperStream {
        private readonly Stream[] _trackers;
        private readonly long _position;
        private readonly bool _hasTrackers;

        public TrackStream(Stream reader, params Stream[] trackers) : base(reader) {
            _trackers = trackers;
            _position = reader.CanSeek? reader.Position : 0;
            _hasTrackers = trackers != null && trackers.Length > 0;
        }

        public override int Read(byte[] buffer, int offset, int count) {
            var n = base.Read(buffer, offset, count);
            if (_hasTrackers) {
                foreach (var w in _trackers) {
                    w.Write(buffer, offset, n);
                }
            }
            return n;
        }

        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) {
            var n= await base.ReadAsync(buffer, offset, count, cancellationToken).ConfigureAwait(false);
            if (_hasTrackers) {
                foreach (var w in _trackers) {
                    await w.WriteAsync(buffer, offset, n, cancellationToken).ConfigureAwait(false);
                }
            }
            return n;
        }

        public override long Seek(long offset, SeekOrigin origin) {
            var off = base.Seek(offset, origin);
            if (_hasTrackers) {
                foreach (var w in _trackers) {
                    w.Seek(off - _position, SeekOrigin.Begin);
                }
            }
            return off;
        }
    }
}
