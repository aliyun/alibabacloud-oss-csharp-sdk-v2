using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AlibabaCloud.OSS.V2.Internal;
using AlibabaCloud.OSS.V2.Models;

namespace AlibabaCloud.OSS.V2
{
    /// <summary>
    /// Options for creating a <see cref="WriteOnlyFile"/>.
    /// </summary>
    public sealed class WriteOnlyOptions
    {
        /// <summary>Multipart part size in bytes. Defaults to <see cref="Defaults.DefaultUploadPartSize"/>.</summary>
        public long PartSize { get; set; } = Defaults.DefaultUploadPartSize;

        /// <summary>Number of concurrent UploadPart workers. Defaults to <see cref="Defaults.DefaultUploadParallel"/>.</summary>
        public int ParallelNum { get; set; } = Defaults.DefaultUploadParallel;

        /// <summary>
        /// Object attributes applied to the created object (ContentType, StorageClass, SSE, Metadata,
        /// Tagging, Acl, ...). Fields with no multipart equivalent apply only on the small-object PutObject path.
        /// </summary>
        public PutObjectRequest? CreateParameter { get; set; }
    }

    /// <summary>
    /// A self-describing resume token for a <see cref="WriteOnlyFile"/> session.
    /// </summary>
    public sealed class WriteCheckpoint
    {
        /// <summary>Resume token; null or empty before the multipart upload is initiated.</summary>
        public string? UploadId { get; set; }

        /// <summary>Contiguous durable prefix [0, Offset); always a multiple of <see cref="PartSize"/>.</summary>
        public long Offset { get; set; }

        /// <summary>CRC64-ECMA of [0, Offset), or 0 when unknown.</summary>
        [CLSCompliant(false)]
        public ulong Crc64 { get; set; }

        /// <summary>Part size bound to <see cref="UploadId"/>.</summary>
        public long PartSize { get; set; }

        /// <summary>The most recent unrecoverable background error, or null when healthy.</summary>
        public Exception? LastError { get; set; }
    }

    /// <summary>
    /// Reports that a <see cref="WriteCheckpoint"/> no longer matches server state. The handle refuses to
    /// resume rather than silently restarting from zero.
    /// </summary>
    public sealed class CheckpointInvalidException : Exception
    {
        public CheckpointInvalidException(string message, Exception? innerException = null)
            : base("write checkpoint no longer valid: " + message, innerException)
        {
        }
    }

    /// <summary>
    /// A buffered, background-concurrent, sequential write handle that produces a normal object, with
    /// checkpoint/resume support. Ported from the Go SDK's WriteOnlyFile (filelike_writeonly.go).
    /// </summary>
    public sealed class WriteOnlyFile : Stream
    {
        private sealed class DonePart
        {
            public string? ETag;
            public ulong Crc;
            public bool CrcKnown;
            public long Size;
        }

        private sealed class BufferPool : IDisposable
        {
            private readonly int _partSize;
            private readonly ConcurrentBag<byte[]> _free = new ConcurrentBag<byte[]>();
            private readonly SemaphoreSlim _slots;
            private volatile bool _disposed;

            public BufferPool(int partSize, int capacity)
            {
                _partSize = partSize;
                _slots = new SemaphoreSlim(capacity, capacity);
            }

            public async Task<byte[]> AcquireAsync(CancellationToken ct)
            {
                await _slots.WaitAsync(ct).ConfigureAwait(false);
                return Take();
            }

            public byte[] Acquire()
            {
                _slots.Wait();
                return Take();
            }

            private byte[] Take()
                => _free.TryTake(out var buf) ? buf : new byte[_partSize];

            public void Release(byte[] buffer)
            {
                // The pool may be torn down while uploads are still finishing (e.g. a close whose drain
                // wait was canceled). Returning a buffer then is a no-op, never a failure.
                if (_disposed)
                    return;
                try
                {
                    _free.Add(buffer);
                    _slots.Release();
                }
                catch (ObjectDisposedException)
                {
                }
            }

            public void Dispose()
            {
                _disposed = true;
                _slots.Dispose();
            }
        }

        private readonly Client _client;
        private readonly string _bucket;
        private readonly string _key;
        private readonly PutObjectRequest? _createParam;
        private readonly int _parallelNum;

        private readonly object _mu = new object();
        private int _partSize;

        private string? _uploadId;
        private bool _initiated;
        private bool _closed;

        private readonly Dictionary<long, DonePart> _doneParts = new Dictionary<long, DonePart>();
        private long _nextContigPart = 1;
        private long _committedOffset;
        private ulong _committedCrc;
        private bool _committedCrcKnown = true;

        // Producer-only state (single-threaded per Stream contract).
        private long _writeCursor;
        private long _assignedParts;
        private byte[]? _curBuf;
        private int _curFilled;

        private BufferPool? _pool;
        private readonly List<Task> _uploadTasks = new List<Task>();
        private Exception? _stickyError;

        private WriteOnlyFile(Client client, string bucket, string key, WriteOnlyOptions options)
        {
            _client = client;
            _bucket = bucket;
            _key = key;
            _createParam = options.CreateParameter;

            var partSize = options.PartSize;
            if (partSize <= 0)
                partSize = Defaults.DefaultUploadPartSize;
            if (partSize > int.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(options), "PartSize must not exceed int.MaxValue");
            _partSize = (int)partSize;

            _parallelNum = options.ParallelNum > 0 ? options.ParallelNum : Defaults.DefaultUploadParallel;

            Crc64.InitECMA();
        }

        /// <summary>
        /// Creates a handle for a new object. No network call is made until the first full part is written.
        /// </summary>
        public static WriteOnlyFile Create(
            Client client,
            string bucket,
            string key,
            Action<WriteOnlyOptions>? optFn = null)
        {
            if (client == null) throw new ArgumentNullException(nameof(client));
            if (string.IsNullOrEmpty(bucket)) throw new ArgumentException("bucket is required", nameof(bucket));
            if (string.IsNullOrEmpty(key)) throw new ArgumentException("key is required", nameof(key));

            var options = new WriteOnlyOptions();
            optFn?.Invoke(options);
            return new WriteOnlyFile(client, bucket, key, options);
        }

        /// <summary>
        /// Resumes a write session from a checkpoint snapshot. An empty <see cref="WriteCheckpoint.UploadId"/>
        /// yields a fresh handle. A checkpoint that no longer matches server state is refused with
        /// <see cref="CheckpointInvalidException"/> rather than silently restarting from zero.
        /// </summary>
        public static async Task<WriteOnlyFile> OpenAsync(
            Client client,
            string bucket,
            string key,
            WriteCheckpoint checkpoint,
            Action<WriteOnlyOptions>? optFn = null,
            CancellationToken cancellationToken = default)
        {
            if (checkpoint == null) throw new ArgumentNullException(nameof(checkpoint));

            var f = Create(client, bucket, key, optFn);

            if (checkpoint.PartSize > 0)
            {
                if (checkpoint.PartSize > int.MaxValue)
                    throw new CheckpointInvalidException("PartSize is out of range");
                f._partSize = (int)checkpoint.PartSize;
            }

            if (string.IsNullOrEmpty(checkpoint.UploadId))
                return f; // no checkpoint to resume; behaves like a fresh handle

            if (checkpoint.PartSize <= 0)
                throw new CheckpointInvalidException("resume requires a positive PartSize");

            List<Part> parts;
            try
            {
                parts = await f.ListAllPartsAsync(checkpoint.UploadId!, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex) when (FindServiceException(ex)?.ErrorCode == "NoSuchUpload")
            {
                // The upload vanished; refuse instead of silently restarting from zero.
                throw new CheckpointInvalidException($"upload {checkpoint.UploadId} not found", ex);
            }

            parts.Sort((a, b) => (a.PartNumber ?? 0).CompareTo(b.PartNumber ?? 0));
            for (var i = 0; i < parts.Count; i++)
            {
                var size = parts[i].Size ?? 0;
                if (size != f._partSize && i != parts.Count - 1)
                    throw new CheckpointInvalidException(
                        $"part {parts[i].PartNumber} is short ({size} < {f._partSize})");
            }

            // Seed the contiguous full-part prefix; a trailing short part stops the walk.
            long expected = 1;
            long offset = 0;
            ulong crc = 0;
            var crcKnown = true;
            foreach (var p in parts)
            {
                if ((p.PartNumber ?? 0) != expected || (p.Size ?? 0) != f._partSize)
                    break;

                var pcrc = ParseCrc(p.HashCrc64);
                if (pcrc.HasValue && crcKnown)
                    crc = Crc64.Combine(crc, pcrc.Value, f._partSize);
                else
                    crcKnown = false;

                f._doneParts[expected] = new DonePart
                {
                    ETag = p.ETag,
                    Crc = pcrc ?? 0,
                    CrcKnown = pcrc.HasValue,
                    Size = f._partSize,
                };
                offset += f._partSize;
                expected++;
            }

            // Cross-check the caller-supplied durable prefix against server truth.
            if (checkpoint.Offset != offset)
                throw new CheckpointInvalidException(
                    $"offset mismatch (checkpoint {checkpoint.Offset}, server {offset})");

            if (checkpoint.Crc64 != 0 && crcKnown && checkpoint.Crc64 != crc)
                throw new CheckpointInvalidException(
                    $"crc64 mismatch (checkpoint {checkpoint.Crc64}, server {crc})");

            f._uploadId = checkpoint.UploadId;
            f._initiated = true;
            f._assignedParts = expected - 1;
            f._nextContigPart = expected;
            f._committedOffset = offset;
            f._committedCrc = crc;
            f._committedCrcKnown = crcKnown;
            return f;
        }

        /// <summary>The object name in the form <c>oss://bucket/key</c>.</summary>
        public string Name => $"oss://{_bucket}/{_key}";

        /// <summary>Returns a consistent snapshot of the current durable state.</summary>
        public WriteCheckpoint StatCheckpoint()
        {
            lock (_mu)
            {
                return new WriteCheckpoint
                {
                    UploadId = _uploadId,
                    Offset = _committedOffset,
                    // An unknown prefix CRC must be reported as 0; a partial-prefix CRC paired with a
                    // longer Offset would be rejected by the resume validation as a mismatch.
                    Crc64 = _committedCrcKnown ? _committedCrc : 0,
                    PartSize = _partSize,
                    LastError = _stickyError,
                };
            }
        }

        public override bool CanRead => false;

        public override bool CanSeek => true;

        public override bool CanWrite => !_closed;

        public override long Length => throw new NotSupportedException("WriteOnlyFile length is unknown until closed");

        public override long Position
        {
            get => _writeCursor;
            set => throw new NotSupportedException();
        }

        /// <summary>Supports only Seek(0, SeekOrigin.Current), which returns the write cursor.</summary>
        public override long Seek(long offset, SeekOrigin origin)
        {
            CheckValid();
            if (origin != SeekOrigin.Current || offset != 0)
                throw new NotSupportedException(
                    "WriteOnlyFile only supports Seek(0, SeekOrigin.Current) to query the write position");
            return _writeCursor;
        }

        public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();

        public override void SetLength(long value) => throw new NotSupportedException();

        public override void Write(byte[] buffer, int offset, int count)
            => WriteAsync(buffer, offset, count, CancellationToken.None).ConfigureAwait(false).GetAwaiter().GetResult();

        public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            CheckValid();
            ValidateBufferArgs(buffer, offset, count);
            ThrowIfSticky();

            if (count == 0)
                return;

            var total = 0;
            while (total < count)
            {
                _pool ??= new BufferPool(_partSize, _parallelNum + 1);

                if (_curBuf == null)
                {
                    _curBuf = await _pool.AcquireAsync(cancellationToken).ConfigureAwait(false);
                    _curFilled = 0;
                }

                var n = Math.Min(count - total, _partSize - _curFilled);
                Buffer.BlockCopy(buffer, offset + total, _curBuf, _curFilled, n);
                _curFilled += n;
                total += n;
                _writeCursor += n;

                if (_curFilled == _partSize)
                {
                    try
                    {
                        await SealFullPartAsync(cancellationToken).ConfigureAwait(false);
                    }
                    catch (Exception ex) when (!(ex is IOException || ex is OperationCanceledException))
                    {
                        throw WrapError("write", ex);
                    }
                }
            }
        }

        /// <summary>Writes the entire content of <paramref name="source"/>, returning the number of bytes written.</summary>
        public long WriteFrom(Stream source)
            => WriteFromAsync(source).ConfigureAwait(false).GetAwaiter().GetResult();

        /// <summary>Writes the entire content of <paramref name="source"/>, returning the number of bytes written.</summary>
        public async Task<long> WriteFromAsync(Stream source, CancellationToken cancellationToken = default)
        {
            CheckValid();
            ThrowIfSticky("writefrom");
            if (source == null) throw new ArgumentNullException(nameof(source));

            var written = 0L;
            var buf = new byte[32 * 1024];
            while (true)
            {
                var nr = await source.ReadAsync(buf, 0, buf.Length, cancellationToken).ConfigureAwait(false);
                if (nr > 0)
                {
                    await WriteAsync(buf, 0, nr, cancellationToken).ConfigureAwait(false);
                    written += nr;
                }
                else
                {
                    break;
                }
            }
            return written;
        }

        public override void Flush()
            => FlushAsync(CancellationToken.None).ConfigureAwait(false).GetAwaiter().GetResult();

        /// <summary>
        /// Uploads the current partial tail as a provisional part (a later full seal overwrites it) without
        /// advancing the durable offset.
        /// </summary>
        public override async Task FlushAsync(CancellationToken cancellationToken)
        {
            CheckValid();
            ThrowIfSticky();

            // Drain in-flight parts so their errors surface and state settles.
            await DrainAsync(cancellationToken).ConfigureAwait(false);
            ThrowIfSticky();

            if (_curFilled == 0)
                return;

            await EnsureInitiatedAsync(cancellationToken).ConfigureAwait(false);

            try
            {
                var partNumber = _assignedParts + 1;
                using var body = new MemoryStream(_curBuf!, 0, _curFilled, writable: false);
                await _client.UploadPartAsync(new UploadPartRequest
                {
                    Bucket = _bucket,
                    Key = _key,
                    UploadId = _uploadId,
                    PartNumber = partNumber,
                    Body = body,
                    ContentLength = _curFilled,
                }, cancellationToken: cancellationToken).ConfigureAwait(false);
                // Partial part: not recorded in doneParts, offset not advanced, buffer retained.
            }
            catch (Exception ex)
            {
                lock (_mu)
                {
                    _stickyError ??= ex;
                }
                throw WrapError("flush", ex);
            }
        }

        /// <summary>
        /// Commits the object via CompleteMultipartUpload, or PutObject for a small object. On failure parts
        /// are kept for resume; it does not auto-abort. Calling Close on an already-closed handle is a no-op.
        /// </summary>
        public async Task CloseAsync(CancellationToken cancellationToken = default)
        {
            if (_closed)
                return;
            _closed = true;

            try
            {
                var preexisting = StickyError;
                if (preexisting != null)
                {
                    await DrainAsync(cancellationToken).ConfigureAwait(false);
                    throw WrapError("close", preexisting);
                }

                if (!_initiated)
                {
                    try
                    {
                        await PutSingleObjectAsync(cancellationToken).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        throw WrapError("close", ex);
                    }
                    return;
                }

                // Dispatch the remaining tail as the last (possibly short) part.
                if (_curFilled > 0)
                {
                    Dispatch(_assignedParts + 1, _curBuf!, _curFilled);
                    _curBuf = null;
                    _curFilled = 0;
                }

                await DrainAsync(cancellationToken).ConfigureAwait(false);

                var afterDrain = StickyError;
                if (afterDrain != null)
                    throw WrapError("close", afterDrain);

                try
                {
                    await CompleteAsync(cancellationToken).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    throw WrapError("close", ex);
                }
            }
            finally
            {
                // Must run even when the drain wait is canceled: a closed handle rejects every later
                // close/abort/dispose, so this is the last chance to tear the pool down. Uploads still
                // in flight simply drop their buffers instead of returning them.
                ReleasePool();
            }
        }

        /// <summary>
        /// Discards the upload via AbortMultipartUpload and releases the handle. Mutually exclusive with
        /// <see cref="CloseAsync"/>: whichever runs first wins, and any later call is a no-op.
        /// </summary>
        public async Task AbortCloseAsync(CancellationToken cancellationToken = default)
        {
            if (_closed)
                return;
            _closed = true;

            try
            {
                await DrainAsync(cancellationToken).ConfigureAwait(false);

                if (_initiated)
                {
                    try
                    {
                        var abortRequest = new AbortMultipartUploadRequest
                        {
                            Bucket = _bucket,
                            Key = _key,
                            UploadId = _uploadId,
                        };
                        var abortPayer = RequestPayer;
                        if (abortPayer != null)
                            abortRequest.Headers["x-oss-request-payer"] = abortPayer;
                        await _client.AbortMultipartUploadAsync(abortRequest, cancellationToken: cancellationToken).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        throw WrapError("abortclose", ex);
                    }
                }
            }
            finally
            {
                // Same reasoning as CloseAsync: the handle is already closed, so release here or never.
                ReleasePool();
            }
        }

        private async Task SealFullPartAsync(CancellationToken cancellationToken)
        {
            await EnsureInitiatedAsync(cancellationToken).ConfigureAwait(false);

            var partNumber = _assignedParts + 1;
            var buf = _curBuf!;
            _curBuf = null;
            _curFilled = 0;
            Dispatch(partNumber, buf, _partSize);
        }

        private void Dispatch(long partNumber, byte[] buffer, int size)
        {
            var uploadId = _uploadId!;
            _assignedParts = partNumber;

            Task task;
            lock (_mu)
            {
                task = UploadPartBufferedAsync(uploadId, partNumber, buffer, size);
                _uploadTasks.Add(task);
            }
        }

        private async Task UploadPartBufferedAsync(string uploadId, long partNumber, byte[] buffer, int size)
        {
            try
            {
                if (StickyError != null)
                    return;

                using var body = new MemoryStream(buffer, 0, size, writable: false);
                var payer = RequestPayer;
                var request = new UploadPartRequest
                {
                    Bucket = _bucket,
                    Key = _key,
                    UploadId = uploadId,
                    PartNumber = partNumber,
                    Body = body,
                    ContentLength = size,
                };
                if (payer != null)
                    request.Headers["x-oss-request-payer"] = payer;
                var result = await _client.UploadPartAsync(request).ConfigureAwait(false);

                var crc = ParseCrc(result.HashCrc64);
                lock (_mu)
                {
                    _doneParts[partNumber] = new DonePart
                    {
                        ETag = result.ETag,
                        Crc = crc ?? 0,
                        CrcKnown = crc.HasValue,
                        Size = size,
                    };
                    AdvanceContigLocked();
                }
            }
            catch (Exception ex)
            {
                lock (_mu)
                {
                    _stickyError ??= ex;
                }
            }
            finally
            {
                _pool?.Release(buffer);
            }
        }

        // Advances the durable prefix over gap-free full parts. Callers must hold _mu.
        private void AdvanceContigLocked()
        {
            while (true)
            {
                if (!_doneParts.TryGetValue(_nextContigPart, out var dp) || dp.Size != _partSize)
                    break;

                if (_committedCrcKnown && dp.CrcKnown)
                    _committedCrc = Crc64.Combine(_committedCrc, dp.Crc, dp.Size);
                else
                    _committedCrcKnown = false;

                _committedOffset += dp.Size;
                _nextContigPart++;
            }
        }

        private Exception? StickyError
        {
            get
            {
                lock (_mu)
                    return _stickyError;
            }
        }

        private async Task EnsureInitiatedAsync(CancellationToken cancellationToken)
        {
            if (_initiated)
                return;

            InitiateMultipartUploadResult result;
            try
            {
                result = await _client.InitiateMultipartUploadAsync(BuildInitiateRequest(), cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                lock (_mu)
                {
                    _stickyError ??= ex;
                }
                throw;
            }

            _uploadId = result.UploadId;
            if (string.IsNullOrEmpty(_uploadId))
                throw new IOException("initiate multipart upload returned no upload id");
            _initiated = true;
        }

        private InitiateMultipartUploadRequest BuildInitiateRequest()
        {
            var request = new InitiateMultipartUploadRequest
            {
                Bucket = _bucket,
                Key = _key,
            };

            var cp = _createParam;
            if (cp != null)
            {
                request.ForbidOverwrite = cp.ForbidOverwrite;
                request.StorageClass = cp.StorageClass;
                request.Tagging = cp.Tagging;
                request.ServerSideEncryption = cp.ServerSideEncryption;
                request.ServerSideDataEncryption = cp.ServerSideDataEncryption;
                request.ServerSideEncryptionKeyId = cp.ServerSideEncryptionKeyId;
                request.CacheControl = cp.CacheControl;
                request.ContentDisposition = cp.ContentDisposition;
                request.ContentEncoding = cp.ContentEncoding;
                request.Expires = cp.Expires;
                request.ContentType = cp.ContentType;
                request.Metadata = cp.Metadata;
            }

            var payer = RequestPayer;
            if (payer != null)
                request.Headers["x-oss-request-payer"] = payer;

            return request;
        }

        private async Task CompleteAsync(CancellationToken cancellationToken)
        {
            List<UploadPart> parts;
            lock (_mu)
            {
                parts = _doneParts
                    .OrderBy(kv => kv.Key)
                    .Select(kv => new UploadPart { PartNumber = kv.Key, ETag = kv.Value.ETag })
                    .ToList();
            }

            var completeRequest = new CompleteMultipartUploadRequest
            {
                Bucket = _bucket,
                Key = _key,
                UploadId = _uploadId,
                Acl = _createParam?.Acl,
                CompleteMultipartUpload = new CompleteMultipartUpload { Parts = parts },
            };
            var completePayer = RequestPayer;
            if (completePayer != null)
                completeRequest.Headers["x-oss-request-payer"] = completePayer;
            await _client.CompleteMultipartUploadAsync(completeRequest, cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        private async Task PutSingleObjectAsync(CancellationToken cancellationToken)
        {
            var count = _curFilled;
            var request = new PutObjectRequest
            {
                Bucket = _bucket,
                Key = _key,
                Body = new MemoryStream(_curBuf ?? Array.Empty<byte>(), 0, count, writable: false),
                // The handle owns the body; the copied progress callback (if any) does not apply.
                ProgressFn = null,
            };

            var cp = _createParam;
            if (cp != null)
            {
                // Mirror Go's request copy: carry over every header/parameter (including user-supplied
                // custom ones), not just the well-known properties exposed on PutObjectRequest.
                foreach (var header in cp.Headers)
                    request.Headers[header.Key] = header.Value;
                foreach (var parameter in cp.Parameters)
                    request.Parameters[parameter.Key] = parameter.Value;

                request.Metadata = cp.Metadata;

                // The body is owned here; a stale length from CreateParameter must not leak through.
                request.Headers.Remove("Content-Length");
            }

            await _client.PutObjectAsync(request, cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        private async Task<List<Part>> ListAllPartsAsync(string uploadId, CancellationToken cancellationToken)
        {
            var all = new List<Part>();
            long marker = 0;
            var listPayer = RequestPayer;
            while (true)
            {
                var listRequest = new ListPartsRequest
                {
                    Bucket = _bucket,
                    Key = _key,
                    UploadId = uploadId,
                    MaxParts = 1000,
                    PartNumberMarker = marker,
                };
                if (listPayer != null)
                    listRequest.Headers["x-oss-request-payer"] = listPayer;
                var result = await _client.ListPartsAsync(listRequest, cancellationToken: cancellationToken).ConfigureAwait(false);

                if (result.Parts != null)
                    all.AddRange(result.Parts);

                if (result.IsTruncated != true)
                    break;

                marker = result.NextPartNumberMarker ?? 0;
            }
            return all;
        }

        private async Task DrainAsync(CancellationToken cancellationToken = default)
        {
            Task[] snapshot;
            lock (_mu)
            {
                snapshot = _uploadTasks.ToArray();
            }

            // In-flight UploadPart calls have already been issued and are intentionally not abortable;
            // cancellation stops us waiting for them, it does not cancel the transfers themselves.
            if (snapshot.Length == 0)
            {
                cancellationToken.ThrowIfCancellationRequested();
                return;
            }

            var all = Task.WhenAll(snapshot);
            if (!cancellationToken.CanBeCanceled)
            {
                await all.ConfigureAwait(false);
                return;
            }

            var canceled = new TaskCompletionSource<bool>();
            using (cancellationToken.Register(() => canceled.TrySetResult(true)))
            {
                var completed = await Task.WhenAny(all, canceled.Task).ConfigureAwait(false);
                if (completed != all)
                    throw new OperationCanceledException(cancellationToken);
            }
            await all.ConfigureAwait(false);
        }

        private void ReleasePool()
        {
            _pool?.Dispose();
            _pool = null;
        }

        /// <summary>
        /// The requester-pays header value to forward from <see cref="WriteOnlyOptions.CreateParameter"/>, if any.
        /// </summary>
        private string? RequestPayer
        {
            get
            {
                var cp = _createParam;
                return cp != null && cp.Headers.TryGetValue("x-oss-request-payer", out var payer) ? payer : null;
            }
        }

        private void ThrowIfSticky(string op = "write")
        {
            var error = StickyError;
            if (error != null)
                throw WrapError(op, error);
        }

        /// <summary>
        /// Wraps a non-IO failure as <see cref="IOException"/> while preserving cancellation tokens.
        /// </summary>
        private Exception WrapError(string op, Exception ex)
        {
            if (ex is OperationCanceledException)
                return ex;
            // The SDK wraps HTTP-level cancellation in an OperationException; preserve the user token.
            if (ex is OperationException { InnerException: OperationCanceledException oce })
                return oce;
            return new IOException($"{op} {Name} failed: {ex.Message}", ex);
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

        private static ulong? ParseCrc(string? value)
        {
            if (string.IsNullOrEmpty(value))
                return null;
            return ulong.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var crc)
                ? crc
                : (ulong?)null;
        }

        private void CheckValid()
        {
            if (_closed)
                throw new ObjectDisposedException(nameof(WriteOnlyFile));
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
            if (disposing)
            {
                try
                {
                    if (!_closed)
                        CloseAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                }
                finally
                {
                    // Belt and braces: the pool must not outlive the handle whatever happened in Close.
                    ReleasePool();
                }
            }
            base.Dispose(disposing);
        }
    }
}
