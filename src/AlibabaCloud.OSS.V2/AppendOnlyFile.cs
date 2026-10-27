using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AlibabaCloud.OSS.V2.Models;

namespace AlibabaCloud.OSS.V2
{
    /// <summary>
    /// Options for opening an object as an <see cref="AppendOnlyFile"/>.
    /// </summary>
    public sealed class AppendFileOptions
    {
        /// <summary>
        /// Parameters applied to the object on the first append that creates it
        /// (Acl, StorageClass, ContentType, Metadata, Tagging, ...). Ignored once the object exists.
        /// </summary>
        public AppendObjectRequest? CreateParameter { get; set; }
    }

    /// <summary>
    /// Exposes an OSS appendable object as a write-only <see cref="Stream"/> backed by AppendObject.
    /// Ported from the Go SDK's AppendOnlyFile (filelike.go).
    /// </summary>
    public sealed class AppendOnlyFile : Stream
    {
        private readonly Client _client;
        private readonly string _bucket;
        private readonly string _key;
        private readonly AppendObjectRequest? _createParam;

        private long _offset;
        private string? _hashCrc64;
        private bool _created;

        private OssFileInfo? _info;
        private bool _closed;

        private AppendOnlyFile(Client client, string bucket, string key, AppendFileOptions options)
        {
            _client = client;
            _bucket = bucket;
            _key = key;
            _createParam = options.CreateParameter;
        }

        /// <summary>
        /// Opens or creates the named object for appending. Performs a HeadObject to resolve the current
        /// append position; a 404 means the object does not exist yet and will be created on first write.
        /// </summary>
        public static async Task<AppendOnlyFile> OpenAsync(
            Client client,
            string bucket,
            string key,
            Action<AppendFileOptions>? optFn = null,
            CancellationToken cancellationToken = default)
        {
            if (client == null) throw new ArgumentNullException(nameof(client));
            if (string.IsNullOrEmpty(bucket)) throw new ArgumentException("bucket is required", nameof(bucket));
            if (string.IsNullOrEmpty(key)) throw new ArgumentException("key is required", nameof(key));

            var options = new AppendFileOptions();
            optFn?.Invoke(options);

            var f = new AppendOnlyFile(client, bucket, key, options);

            try
            {
                var result = await client.HeadObjectAsync(new HeadObjectRequest
                {
                    Bucket = bucket,
                    Key = key
                }, cancellationToken: cancellationToken).ConfigureAwait(false);

                if (!string.Equals(result.ObjectType, "Appendable", StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException("Not a appendable file");

                f._offset = result.ContentLength ?? 0;
                f._hashCrc64 = result.HashCrc64;
                f._created = true;
            }
            catch (Exception ex) when (FindServiceException(ex)?.StatusCode == 404)
            {
                // Object does not exist yet; it will be created on the first append.
            }

            return f;
        }

        /// <summary>The object name in the form <c>oss://bucket/key</c>.</summary>
        public string Name => $"oss://{_bucket}/{_key}";

        /// <summary>Returns metadata describing the object, refreshing it from the server when stale.</summary>
        public OssFileInfo Stat()
            => StatAsync().ConfigureAwait(false).GetAwaiter().GetResult();

        /// <summary>Returns metadata describing the object, refreshing it from the server when stale.</summary>
        public async Task<OssFileInfo> StatAsync(CancellationToken cancellationToken = default)
        {
            CheckValid();
            if (_info == null || _info.Size != _offset)
            {
                var result = await _client.HeadObjectAsync(new HeadObjectRequest
                {
                    Bucket = _bucket,
                    Key = _key
                }, cancellationToken: cancellationToken).ConfigureAwait(false);

                DateTime? mtime = null;
                if (result.Headers.TryGetValue("Last-Modified", out var modTime) &&
                    !string.IsNullOrEmpty(modTime) &&
                    DateTime.TryParse(modTime, CultureInfo.InvariantCulture,
                        DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out var dt))
                    mtime = dt;

                _info = new OssFileInfo(Name, result.ContentLength ?? 0, mtime, result.Headers);
            }
            return _info;
        }

        public override bool CanRead => false;

        public override bool CanSeek => false;

        public override bool CanWrite => !_closed;

        public override long Length
        {
            get
            {
                CheckValid();
                return _offset;
            }
        }

        public override long Position
        {
            get => _offset;
            set => throw new NotSupportedException();
        }

        public override void Flush()
        {
        }

        public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();

        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

        public override void SetLength(long value) => throw new NotSupportedException();

        public override void Write(byte[] buffer, int offset, int count)
            => WriteAsync(buffer, offset, count, CancellationToken.None).ConfigureAwait(false).GetAwaiter().GetResult();

        public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            CheckValid();
            ValidateBufferArgs(buffer, offset, count);
            if (count == 0)
                return;

            using var body = new MemoryStream(buffer, offset, count, writable: false);
            var written = await AppendBodyAsync(body, count, cancellationToken).ConfigureAwait(false);
            if (written != count)
                throw new IOException($"short write to {Name}, expect {count}, wrote {written}");
        }

        /// <summary>Appends the entire content of <paramref name="stream"/>, returning the number of bytes written.</summary>
        public long WriteFrom(Stream stream)
            => WriteFromAsync(stream).ConfigureAwait(false).GetAwaiter().GetResult();

        /// <summary>Appends the entire content of <paramref name="stream"/>, returning the number of bytes written.</summary>
        public async Task<long> WriteFromAsync(Stream stream, CancellationToken cancellationToken = default)
        {
            CheckValid();
            if (stream == null) throw new ArgumentNullException(nameof(stream));
            return await AppendBodyAsync(stream, null, cancellationToken).ConfigureAwait(false);
        }

        // Issues a single AppendObject for the given body and returns the number of bytes appended.
        // On PositionNotEqualToLength, refreshes the real position and self-recovers when it matches
        // the expected position (start offset plus the amount the body advanced).
        private async Task<long> AppendBodyAsync(Stream body, int? knownCount, CancellationToken ct)
        {
            var offset = _offset;

            long bodyStart = 0;
            var canSeek = body.CanSeek;
            if (canSeek)
                bodyStart = body.Position;

            var request = new AppendObjectRequest
            {
                Bucket = _bucket,
                Key = _key,
                Position = _offset,
                Body = body,
                InitHashCrc64 = _hashCrc64
            };

            ApplyCreateParamIfNeed(request);

            if (_offset == 0)
                request.InitHashCrc64 = "0";

            try
            {
                var result = await _client.AppendObjectAsync(request, cancellationToken: ct).ConfigureAwait(false);
                _offset = result.NextAppendPosition ?? _offset;
                _hashCrc64 = result.HashCrc64;
                _created = true;
            }
            catch (Exception ex) when (FindServiceException(ex)?.ErrorCode == "PositionNotEqualToLength")
            {
                var (position, hashcrc, ok) = await NextAppendPositionAsync(ct).ConfigureAwait(false);
                if (ok)
                {
                    long advanced;
                    if (knownCount.HasValue)
                        advanced = knownCount.Value;
                    else if (canSeek)
                        advanced = body.Position - bodyStart;
                    else
                        throw;

                    if (offset + advanced == position)
                    {
                        _offset = position;
                        _hashCrc64 = hashcrc;
                        _created = true;
                    }
                    else
                    {
                        throw;
                    }
                }
                else
                {
                    throw;
                }
            }

            return _offset - offset;
        }

        private async Task<(long position, string? hashcrc, bool ok)> NextAppendPositionAsync(CancellationToken ct)
        {
            try
            {
                var result = await _client.HeadObjectAsync(new HeadObjectRequest
                {
                    Bucket = _bucket,
                    Key = _key
                }, cancellationToken: ct).ConfigureAwait(false);
                return (result.ContentLength ?? 0, result.HashCrc64, true);
            }
            catch
            {
                return (0, null, false);
            }
        }

        private void ApplyCreateParamIfNeed(AppendObjectRequest request)
        {
            if (_created || _createParam == null)
                return;

            if (!string.IsNullOrEmpty(_createParam.Acl))
                request.Acl = _createParam.Acl;
            if (!string.IsNullOrEmpty(_createParam.StorageClass))
                request.StorageClass = _createParam.StorageClass;
            request.CacheControl = _createParam.CacheControl;
            request.ContentDisposition = _createParam.ContentDisposition;
            request.ContentEncoding = _createParam.ContentEncoding;
            request.Expires = _createParam.Expires;
            request.ContentType = _createParam.ContentType;
            request.ServerSideEncryption = _createParam.ServerSideEncryption;
            request.ServerSideDataEncryption = _createParam.ServerSideDataEncryption;
            request.ServerSideEncryptionKeyId = _createParam.ServerSideEncryptionKeyId;
            request.Metadata = _createParam.Metadata;
            request.Tagging = _createParam.Tagging;
        }

        private static ServiceException? FindServiceException(Exception? ex)
        {
            while (ex != null)
            {
                if (ex is ServiceException se)
                    return se;
                ex = ex.InnerException;
            }
            return null;
        }

        private void CheckValid()
        {
            if (_closed)
                throw new ObjectDisposedException(nameof(AppendOnlyFile));
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
            _closed = true;
            base.Dispose(disposing);
        }
    }
}
