using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AlibabaCloud.OSS.V2.Internal;
using AlibabaCloud.OSS.V2.Models;

namespace AlibabaCloud.OSS.V2
{
    /// <summary>
    /// Options for opening an object as a <see cref="ReadOnlyFile"/>.
    /// </summary>
    public sealed class OpenFileOptions
    {
        /// <summary>The initial read offset within the object.</summary>
        public long Offset { get; set; }

        /// <summary>The version id of the object to read.</summary>
        public string? VersionId { get; set; }

        /// <summary>Whether to enable background prefetch to speed up sequential reads.</summary>
        public bool EnablePrefetch { get; set; }

        /// <summary>Number of chunks to prefetch concurrently.</summary>
        public int PrefetchNum { get; set; } = Defaults.DefaultPrefetchNum;

        /// <summary>Size of each prefetch chunk in bytes.</summary>
        public long ChunkSize { get; set; } = Defaults.DefaultPrefetchChunkSize;

        /// <summary>Sequential read amount, in bytes, before prefetch kicks in.</summary>
        public long PrefetchThreshold { get; set; } = Defaults.DefaultPrefetchThreshold;

        /// <summary>Number of out-of-order reads tolerated before prefetch is disabled.</summary>
        public long OutOfOrderReadThreshold { get; set; } = Defaults.DefaultOutOfOrderReadThreshold;
    }

    /// <summary>
    /// Metadata describing an object opened as a file.
    /// </summary>
    public sealed class OssFileInfo
    {
        internal OssFileInfo(string name, long size, DateTime? lastModified, IDictionary<string, string> headers)
        {
            Name = name;
            Size = size;
            LastModified = lastModified;
            Headers = headers;
        }

        /// <summary>The object name in the form <c>oss://bucket/key[?versionId=...]</c>.</summary>
        public string Name { get; }

        /// <summary>The object size in bytes.</summary>
        public long Size { get; }

        /// <summary>The last-modified time, if available.</summary>
        public DateTime? LastModified { get; }

        /// <summary>The full set of response headers from the HeadObject call.</summary>
        public IDictionary<string, string> Headers { get; }
    }

    /// <summary>
    /// Exposes an OSS object as a read-only, seekable <see cref="Stream"/> with optional background
    /// prefetch. Ported from the Go SDK's ReadOnlyFile (filelike.go).
    /// </summary>
    public sealed class ReadOnlyFile : Stream
    {
        private const int MaxReadRetries = 3;

        private readonly Client _client;
        private readonly string _bucket;
        private readonly string _key;
        private readonly string? _versionId;

        private readonly bool _enablePrefetch;
        private readonly int _prefetchNum;
        private readonly long _chunkSize;
        private readonly long _prefetchThreshold;
        private readonly long _oooReadThreshold;

        // File info, populated by OpenAsync.
        private long _sizeInBytes;
        private string? _modTime;
        private string? _etag;
        private IDictionary<string, string> _headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        // Read state.
        private long _offset;
        private Stream? _reader;
        private long _readBufOffset;

        // Prefetch state.
        private List<AsyncRangeReader>? _asyncReaders;
        private long _seqReadAmount;
        private long _numOOORead;

        private bool _closed;

        private ReadOnlyFile(Client client, string bucket, string key, OpenFileOptions options)
        {
            _client = client;
            _bucket = bucket;
            _key = key;
            _versionId = options.VersionId;
            _offset = options.Offset;
            _enablePrefetch = options.EnablePrefetch;
            _prefetchNum = options.PrefetchNum;
            _chunkSize = options.ChunkSize;
            _prefetchThreshold = options.PrefetchThreshold;
            _oooReadThreshold = options.OutOfOrderReadThreshold;
        }

        /// <summary>
        /// Opens the named object for reading. Performs a HeadObject to resolve size and validity.
        /// </summary>
        public static async Task<ReadOnlyFile> OpenAsync(
            Client client,
            string bucket,
            string key,
            Action<OpenFileOptions>? optFn = null,
            CancellationToken cancellationToken = default)
        {
            if (client == null) throw new ArgumentNullException(nameof(client));
            if (string.IsNullOrEmpty(bucket)) throw new ArgumentException("bucket is required", nameof(bucket));
            if (string.IsNullOrEmpty(key)) throw new ArgumentException("key is required", nameof(key));

            var options = new OpenFileOptions();
            optFn?.Invoke(options);

            if (options.EnablePrefetch)
            {
                options.ChunkSize = options.ChunkSize > 0
                    ? (options.ChunkSize + Defaults.AsyncReadBufferSize - 1)
                        / Defaults.AsyncReadBufferSize * Defaults.AsyncReadBufferSize
                    : Defaults.DefaultPrefetchChunkSize;

                if (options.PrefetchNum <= 0)
                    options.PrefetchNum = Defaults.DefaultPrefetchNum;

                if (options.OutOfOrderReadThreshold <= 0)
                    options.OutOfOrderReadThreshold = Defaults.DefaultOutOfOrderReadThreshold;
            }

            var f = new ReadOnlyFile(client, bucket, key, options);

            var result = await client.HeadObjectAsync(new HeadObjectRequest
            {
                Bucket = bucket,
                Key = key,
                VersionId = options.VersionId
            }, cancellationToken: cancellationToken).ConfigureAwait(false);

            f._sizeInBytes = result.ContentLength ?? -1;
            result.Headers.TryGetValue("Last-Modified", out var modTime);
            result.Headers.TryGetValue("ETag", out var etag);
            f._modTime = modTime;
            f._etag = etag;
            f._headers = result.Headers;

            if (f._sizeInBytes < 0)
                throw new ArgumentException($"file size is invalid, got {f._sizeInBytes}");

            if (f._offset > f._sizeInBytes)
                throw new ArgumentException(
                    $"offset is unavailable, offset:{f._offset}, file size:{f._sizeInBytes}");

            return f;
        }

        /// <summary>The object name in the form <c>oss://bucket/key[?versionId=...]</c>.</summary>
        public string Name => _versionId != null
            ? $"oss://{_bucket}/{_key}?versionId={_versionId}"
            : $"oss://{_bucket}/{_key}";

        /// <summary>Returns metadata describing the object.</summary>
        public OssFileInfo Stat()
        {
            CheckValid();
            DateTime? mtime = null;
            if (!string.IsNullOrEmpty(_modTime) &&
                DateTime.TryParse(_modTime, CultureInfo.InvariantCulture,
                    DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out var dt))
                mtime = dt;
            return new OssFileInfo(Name, _sizeInBytes, mtime, _headers);
        }

        public override bool CanRead => !_closed;

        public override bool CanSeek => !_closed;

        public override bool CanWrite => false;

        public override long Length
        {
            get
            {
                CheckValid();
                return _sizeInBytes;
            }
        }

        public override long Position
        {
            get => _offset;
            set => Seek(value, SeekOrigin.Begin);
        }

        public override void Flush()
        {
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            CheckValid();
            long abs;
            switch (origin)
            {
                case SeekOrigin.Begin:
                    abs = offset;
                    break;
                case SeekOrigin.Current:
                    abs = _offset + offset;
                    break;
                case SeekOrigin.End:
                    abs = _sizeInBytes + offset;
                    break;
                default:
                    throw new ArgumentException("invalid seek origin", nameof(origin));
            }

            if (abs < 0)
                throw new IOException("negative position");
            if (abs > _sizeInBytes)
                throw new IOException("offset is unavailable");

            _offset = abs;
            return abs;
        }

        public override int Read(byte[] buffer, int offset, int count)
            => ReadAsync(buffer, offset, count, CancellationToken.None).ConfigureAwait(false).GetAwaiter().GetResult();

        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            CheckValid();
            ValidateBufferArgs(buffer, offset, count);
            if (count == 0)
                return 0;

            var bytesRead = 0;
            var zeroProgress = 0;
            while (bytesRead == 0)
            {
                var (nread, eof) = await ReadInternalAsync(_offset, buffer, offset, count, cancellationToken)
                    .ConfigureAwait(false);
                if (nread > 0)
                {
                    bytesRead = nread;
                    break;
                }
                if (eof)
                    break;
                if (++zeroProgress > MaxReadRetries)
                    throw new IOException($"Failed to read from {Name} at offset {_offset}");
            }

            _offset += bytesRead;
            return bytesRead;
        }

        public override void SetLength(long value) => throw new NotSupportedException();

        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

        private async Task<(int nread, bool eof)> ReadInternalAsync(
            long offset, byte[] p, int poff, int pcount, CancellationToken ct)
        {
            if (offset >= _sizeInBytes)
                return (0, true);

            if (_readBufOffset != offset)
            {
                _readBufOffset = offset;
                _seqReadAmount = 0;

                if (_reader != null)
                {
                    _reader.Dispose();
                    _reader = null;
                }

                if (_asyncReaders != null)
                    _numOOORead++;

                CloseAsyncReaders();
            }

            if (_enablePrefetch && _seqReadAmount >= _prefetchThreshold && _numOOORead < _oooReadThreshold)
            {
                if (_reader != null)
                {
                    _reader.Dispose();
                    _reader = null;
                }

                Prefetch(offset);
                var (n, err) = ReadFromPrefetcher(offset, p, poff, pcount);
                if (err == null)
                {
                    if (n > 0)
                    {
                        _readBufOffset += n;
                        _seqReadAmount += n;
                    }
                    return (n, false);
                }

                // Fall back to serial reads.
                _seqReadAmount = 0;
                CloseAsyncReaders();
            }

            var (nd, eof) = await ReadDirectAsync(offset, p, poff, pcount, ct).ConfigureAwait(false);
            if (nd > 0)
            {
                _readBufOffset += nd;
                _seqReadAmount += nd;
            }
            return (nd, eof);
        }

        private async Task<(int nread, bool eof)> ReadDirectAsync(
            long offset, byte[] buf, int poff, int pcount, CancellationToken ct)
        {
            if (offset >= _sizeInBytes)
                return (0, true);

            if (_reader == null)
            {
                var result = await _client.GetObjectAsync(new GetObjectRequest
                {
                    Bucket = _bucket,
                    Key = _key,
                    VersionId = _versionId,
                    Range = $"bytes={offset.ToString(CultureInfo.InvariantCulture)}-",
                    RangeBehavior = "standard"
                }, cancellationToken: ct).ConfigureAwait(false);

                var err = CheckResultValid(offset, result.Headers);
                if (err != null || result.Body == null)
                {
                    result.Body?.Dispose();
                    throw err ?? new IOException("empty response body");
                }

                _reader = result.Body;
            }

            var reader = _reader;
            int n;
            try
            {
                n = await reader.ReadAsync(buf, poff, pcount, ct).ConfigureAwait(false);
            }
            catch
            {
                // Drop the body and retry from the same offset via a fresh GET (matches Go).
                reader.Dispose();
                _reader = null;
                return (0, false);
            }

            if (n == 0)
            {
                reader.Dispose();
                _reader = null;
                return offset >= _sizeInBytes ? (0, true) : (0, false);
            }

            return (n, false);
        }

        private Exception? CheckResultValid(long offset, IDictionary<string, string> headers)
        {
            headers.TryGetValue("Last-Modified", out var modTime);
            headers.TryGetValue("ETag", out var etag);
            var (gotOffset, _) = RangeParser.ParseOffsetAndSizeFromHeaders(headers);

            if (gotOffset != offset)
                return new IOException($"Range get fail, expect offset:{offset}, got offset:{gotOffset}");

            if ((!string.IsNullOrEmpty(modTime) && !string.IsNullOrEmpty(_modTime) && modTime != _modTime) ||
                (!string.IsNullOrEmpty(etag) && !string.IsNullOrEmpty(_etag) && etag != _etag))
                return new IOException(
                    $"Source file is changed, origin info [{_modTime},{_etag}], new info [{modTime},{etag}]");

            return null;
        }

        private (int nread, Exception? error) ReadFromPrefetcher(
            long offset, byte[] buf, int poff, int pcount)
        {
            var bytesRead = 0;
            while (_asyncReaders != null && _asyncReaders.Count != 0)
            {
                var asyncReader = _asyncReaders[0];
                if (offset != asyncReader.Offset)
                    return (0, new IOException("out of order"));

                int nread;
                try
                {
                    nread = asyncReader.Read(buf, poff, pcount);
                }
                catch (Exception ex)
                {
                    return (0, ex);
                }

                bytesRead += nread;
                poff += nread;
                pcount -= nread;

                if (nread == 0)
                {
                    // This reader is exhausted (EOF).
                    asyncReader.Dispose();
                    _asyncReaders.RemoveAt(0);
                }

                if (pcount == 0)
                    return (bytesRead, null);

                offset += nread;
            }

            return (bytesRead, null);
        }

        private void Prefetch(long offset)
        {
            var off = offset;
            if (_asyncReaders != null)
                foreach (var ar in _asyncReaders)
                    off = ar.OriginalRange.Offset + ar.OriginalRange.Count;

            _asyncReaders ??= new List<AsyncRangeReader>();

            while (_asyncReaders.Count < _prefetchNum)
            {
                var remaining = _sizeInBytes - off;
                var size = Math.Min(remaining, _chunkSize);
                var cnt = (size + (Defaults.AsyncReadBufferSize - 1)) / Defaults.AsyncReadBufferSize;

                // Add extra buffers so soft-start ramps do not stall connection release.
                if (size > Defaults.SoftStartInitial)
                {
                    var acnt = (Defaults.AsyncReadBufferSize + (Defaults.SoftStartInitial - 1))
                        / Defaults.SoftStartInitial - 1;
                    cnt += acnt;
                }

                if (size != 0)
                {
                    var ar = new AsyncRangeReader(
                        PrefetchGetAsync,
                        new HttpRange(off, size),
                        _etag,
                        (int)cnt);
                    _asyncReaders.Add(ar);
                    off += size;
                }

                if (size != _chunkSize)
                    break;
            }
        }

        private async Task<AsyncRangeReader.RangeGetOutput> PrefetchGetAsync(HttpRange range, CancellationToken ct)
        {
            var request = new GetObjectRequest
            {
                Bucket = _bucket,
                Key = _key,
                VersionId = _versionId
            };

            var rangeStr = range.FormatHttpRange();
            if (rangeStr != null)
            {
                request.Range = rangeStr;
                request.RangeBehavior = "standard";
            }

            var result = await _client.GetObjectAsync(request, cancellationToken: ct).ConfigureAwait(false);
            return new AsyncRangeReader.RangeGetOutput
            {
                Body = result.Body,
                ETag = result.ETag,
                ContentLength = result.ContentLength ?? -1,
                ContentRange = result.ContentRange
            };
        }

        private void CloseAsyncReaders()
        {
            if (_asyncReaders == null)
                return;
            foreach (var ar in _asyncReaders)
                ar.Dispose();
            _asyncReaders = null;
        }

        private void CheckValid()
        {
            if (_closed)
                throw new ObjectDisposedException(nameof(ReadOnlyFile));
        }

        private static void ValidateBufferArgs(byte[] buffer, int offset, int count)
        {
            if (buffer == null) throw new ArgumentNullException(nameof(buffer));
            if (offset < 0) throw new ArgumentOutOfRangeException(nameof(offset));
            if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
            if (offset + count > buffer.Length) throw new ArgumentException("buffer too small");
        }

        protected override void Dispose(bool disposing)
        {
            if (!_closed && disposing)
            {
                if (_reader != null)
                {
                    _reader.Dispose();
                    _reader = null;
                }
                CloseAsyncReaders();
            }
            _closed = true;
            base.Dispose(disposing);
        }
    }
}
