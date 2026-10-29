using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using AlibabaCloud.OSS.V2.Internal;
using AlibabaCloud.OSS.V2.Models;

namespace AlibabaCloud.OSS.V2
{
    /// <summary>
    /// Options that control how the <see cref="Uploader"/> transfers data.
    /// </summary>
    public sealed class UploaderOptions
    {
        /// <summary>The part size, in bytes. Default is 6 MiB.</summary>
        public long PartSize { get; set; } = Defaults.DefaultUploadPartSize;

        /// <summary>The number of parts uploaded concurrently. Default is 3.</summary>
        public int ParallelNum { get; set; } = Defaults.DefaultUploadParallel;

        /// <summary>Whether to keep already-uploaded parts when the upload fails. Default is false.</summary>
        public bool LeavePartsOnError { get; set; }

        /// <summary>Whether to enable resumable upload via a checkpoint file (file uploads only).</summary>
        public bool EnableCheckpoint { get; set; }

        /// <summary>The directory that stores the checkpoint file. Empty means the system temp dir.</summary>
        public string? CheckpointDir { get; set; }

        internal UploaderOptions Clone() => new()
        {
            PartSize = PartSize,
            ParallelNum = ParallelNum,
            LeavePartsOnError = LeavePartsOnError,
            EnableCheckpoint = EnableCheckpoint,
            CheckpointDir = CheckpointDir
        };
    }

    /// <summary>
    /// The result of an upload performed by the <see cref="Uploader"/>.
    /// </summary>
    public sealed class UploadResult : ResultModel
    {
        /// <summary>The multipart upload id, when a multipart upload was used.</summary>
        public string? UploadId { get; internal set; }

        /// <summary>The ETag of the uploaded object.</summary>
        public string? ETag { get; internal set; }

        /// <summary>The version id of the uploaded object.</summary>
        public string? VersionId { get; internal set; }

        /// <summary>The 64-bit ECMA-182 CRC of the uploaded object.</summary>
        public string? HashCrc64 { get; internal set; }
    }

    /// <summary>
    /// A high-level component that uploads objects to OSS. Small objects are uploaded with a single
    /// PutObject request; larger objects are uploaded concurrently as a multipart upload, with
    /// optional CRC64 integrity checking and, for file uploads, resumable checkpoints.
    /// Ported from the Go V2 SDK (oss/uploader.go).
    /// </summary>
    public sealed class Uploader
    {
        private readonly Transfer.ITransferClient _client;
        private readonly UploaderOptions _options;
        private readonly bool _featureEnableCrc64;

        public Uploader(Client client, params Action<UploaderOptions>[] optFns)
            : this(new Transfer.ClientAdapter(client ?? throw new ArgumentNullException(nameof(client))), optFns)
        {
        }

        internal Uploader(Transfer.ITransferClient client, params Action<UploaderOptions>[] optFns)
        {
            _client = client;
            _options = new UploaderOptions
            {
                PartSize = Defaults.DefaultUploadPartSize,
                ParallelNum = Defaults.DefaultUploadParallel,
                LeavePartsOnError = false
            };

            if (optFns != null)
                foreach (var fn in optFns)
                    fn?.Invoke(_options);

            _featureEnableCrc64 = _client.GetClientOptions().FeatureFlags.HasFlag(FeatureFlagsType.EnableCrc64CheckUpload);
        }

        /// <summary>
        /// Uploads a local file to OSS. Supports concurrent multipart upload, CRC64 checking,
        /// progress reporting and resumable checkpoints.
        /// </summary>
        public async Task<UploadResult> UploadFileAsync(
            PutObjectRequest request,
            string filePath,
            Action<UploaderOptions>? optFn = null,
            CancellationToken cancellationToken = default)
        {
            var d = NewDelegate(request, optFn);
            d.CheckSource(filePath);

#if NET5_0_OR_GREATER
            await using var file = File.OpenRead(filePath);
#else
            using var file = File.OpenRead(filePath);
#endif
            d.Body = file;
            d.ApplySource();
            d.CheckCheckpoint();
            await d.AdjustSourceAsync(cancellationToken).ConfigureAwait(false);

            var result = await d.UploadAsync(cancellationToken).ConfigureAwait(false);
            d.RemoveCheckpoint();
            return result;
        }

        /// <summary>
        /// Uploads data from a stream to OSS. Checkpoints and resume are not available for stream
        /// uploads because the source cannot be reliably re-read.
        /// </summary>
        public async Task<UploadResult> UploadStreamAsync(
            PutObjectRequest request,
            Stream body,
            Action<UploaderOptions>? optFn = null,
            CancellationToken cancellationToken = default)
        {
            if (body == null)
                throw new ArgumentNullException(nameof(body));

            var d = NewDelegate(request, optFn);
            d.Body = body;
            d.ApplySource();
            return await d.UploadAsync(cancellationToken).ConfigureAwait(false);
        }

        private UploadDelegate NewDelegate(PutObjectRequest request, Action<UploaderOptions>? optFn)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));
            if (string.IsNullOrEmpty(request.Bucket))
                throw new ArgumentException("request.Bucket is required.", nameof(request));
            if (string.IsNullOrEmpty(request.Key))
                throw new ArgumentException("request.Key is required.", nameof(request));

            var options = _options.Clone();
            optFn?.Invoke(options);

            if (options.ParallelNum <= 0)
                options.ParallelNum = Defaults.DefaultUploadParallel;
            if (options.PartSize <= 0)
                options.PartSize = Defaults.DefaultUploadPartSize;

            return new UploadDelegate(_client, options, request, _featureEnableCrc64);
        }

        private sealed class UploadDelegate
        {
            private static readonly List<Part> EmptyParts = new();

            private readonly Transfer.ITransferClient _client;
            private readonly UploaderOptions _options;
            private readonly PutObjectRequest _request;
            private readonly bool _enableCrc64;

            private long _totalSize = -1;
            private long _readerPos;
            private ulong _baseCrc64;
            private long _transferred;

            private string _filePath = "";
            private long _fileSize;
            private string _fileLastModified = "";

            private string _uploadId = "";
            private int _startPartNumber;
            private List<Part> _uploadedParts = EmptyParts;

            private UploadCheckpoint? _checkpoint;

            public UploadDelegate(Transfer.ITransferClient client, UploaderOptions options, PutObjectRequest request, bool enableCrc64)
            {
                _client = client;
                _options = options;
                _request = request;
                _enableCrc64 = enableCrc64;
            }

            public Stream? Body { get; set; }

            public void CheckSource(string filePath)
            {
                if (string.IsNullOrEmpty(filePath))
                    throw new ArgumentException("filePath is required.", nameof(filePath));
                if (!File.Exists(filePath))
                    throw new FileNotFoundException($"File not exists, {filePath}", filePath);

                var info = new FileInfo(filePath);
                _filePath = filePath;
                _fileSize = info.Length;
                _fileLastModified = info.LastWriteTimeUtc.ToString("O", CultureInfo.InvariantCulture);
            }

            public void ApplySource()
            {
                if (Body == null)
                    throw new ArgumentNullException(nameof(Body), "the body is null");

                var totalSize = Body.CanSeek ? Body.Length - Body.Position : -1;

                var partSize = _options.PartSize;
                if (totalSize > 0)
                    while (totalSize / partSize >= Defaults.MaxUploadParts)
                        partSize += _options.PartSize;

                _totalSize = totalSize;
                _options.PartSize = partSize;
            }

            public void CheckCheckpoint()
            {
                if (!_options.EnableCheckpoint)
                    return;

                _checkpoint = UploadCheckpoint.Create(
                    _request.Bucket!, _request.Key!, _filePath, _options.CheckpointDir,
                    _fileSize, _fileLastModified, _options.PartSize);
                _checkpoint.Load();
                if (_checkpoint.Loaded)
                    _uploadId = _checkpoint.UploadId;
                _options.LeavePartsOnError = true;
            }

            public void RemoveCheckpoint() => _checkpoint?.Remove();

            public async Task AdjustSourceAsync(CancellationToken ct)
            {
                if (string.IsNullOrEmpty(_uploadId))
                    return;
                if (Body == null || !Body.CanSeek)
                {
                    _uploadId = "";
                    return;
                }

                var checkPartNumber = 1;
                ulong crc = 0;
                var uploaded = new List<Part>();
                long? marker = null;
                var stop = false;

                try
                {
                    do
                    {
                        var page = await _client.ListPartsAsync(new ListPartsRequest
                        {
                            Bucket = _request.Bucket,
                            Key = _request.Key,
                            UploadId = _uploadId,
                            PartNumberMarker = marker
                        }, null, ct).ConfigureAwait(false);

                        marker = page.NextPartNumberMarker;
                        var truncated = page.IsTruncated ?? false;

                        foreach (var p in page.Parts ?? EmptyParts)
                        {
                            if (p.PartNumber != checkPartNumber || p.Size != _options.PartSize)
                            {
                                stop = true;
                                break;
                            }

                            checkPartNumber++;
                            uploaded.Add(p);
                            if (_enableCrc64 && p.HashCrc64 != null &&
                                ulong.TryParse(p.HashCrc64, NumberStyles.Integer, CultureInfo.InvariantCulture, out var v))
                                crc = Crc64.Combine(crc, v, p.Size ?? 0);
                        }

                        if (stop || !truncated)
                            break;
                    } while (true);
                }
                catch
                {
                    _uploadId = "";
                    return;
                }

                var partNumber = checkPartNumber - 1;
                var newOffset = (long)partNumber * _options.PartSize;
                try
                {
                    Body.Seek(newOffset, SeekOrigin.Begin);
                }
                catch
                {
                    _uploadId = "";
                    return;
                }

                _startPartNumber = partNumber;
                _readerPos = newOffset;
                _baseCrc64 = crc;
                _uploadedParts = uploaded;
            }

            public Task<UploadResult> UploadAsync(CancellationToken ct)
            {
                if (_totalSize >= 0 && _totalSize < _options.PartSize)
                    return SinglePartAsync(ct);
                return MultiPartAsync(ct);
            }

            private async Task<UploadResult> SinglePartAsync(CancellationToken ct)
            {
                _request.Body = Body;
                if (_request.ContentType == null)
                {
                    var contentType = GetContentType();
                    if (contentType != null)
                        _request.ContentType = contentType;
                }

                var r = await _client.PutObjectAsync(_request, null, ct).ConfigureAwait(false);
                return new UploadResult
                {
                    ETag = r.ETag,
                    VersionId = r.VersionId,
                    HashCrc64 = r.HashCrc64,
                    Headers = r.Headers,
                    Status = r.Status,
                    StatusCode = r.StatusCode
                };
            }

            private async Task<UploadResult> MultiPartAsync(CancellationToken ct)
            {
                var (uploadId, startPartNum) = await GetUploadIdAsync(ct).ConfigureAwait(false);

                if (_checkpoint != null)
                {
                    _checkpoint.UploadId = uploadId;
                    _checkpoint.Dump();
                }

                var sem = new SemaphoreSlim(_options.ParallelNum);
                var tasks = new List<Task>();
                var gate = new object();
                var parts = new List<UploadPart>();
                var crcParts = new List<PartCrc>();
                Exception? error = null;

                void SetError(Exception e) => Interlocked.CompareExchange(ref error, e, null);
                bool HasError() => Volatile.Read(ref error) != null;

                void OnPart(int partNumber, UploadPartResult up, long size)
                {
                    lock (gate)
                    {
                        parts.Add(new UploadPart { ETag = up.ETag, PartNumber = partNumber });
                        if (_enableCrc64)
                            crcParts.Add(new PartCrc(partNumber, up.HashCrc64, size));
                        if (_request.ProgressFn != null)
                        {
                            _transferred += size;
                            _request.ProgressFn(size, _transferred, _totalSize);
                        }
                    }
                }

                // Restore already-uploaded parts from a resumed session.
                if (_readerPos > 0)
                {
                    foreach (var p in _uploadedParts)
                        parts.Add(new UploadPart { ETag = p.ETag, PartNumber = p.PartNumber });
                    if (_request.ProgressFn != null)
                    {
                        _transferred = _readerPos;
                        _request.ProgressFn(_readerPos, _transferred, _totalSize);
                    }
                }

                try
                {
                    if (!string.IsNullOrEmpty(_filePath))
                        await ProduceFilePartsAsync(uploadId, sem, tasks, OnPart, SetError, HasError, ct).ConfigureAwait(false);
                    else
                        await ProduceStreamPartsAsync(uploadId, startPartNum, sem, tasks, OnPart, SetError, HasError, ct).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    SetError(ex);
                }

                await Task.WhenAll(tasks).ConfigureAwait(false);

                CompleteMultipartUploadResult? cmResult = null;
                if (!HasError())
                {
                    parts.Sort((a, b) => (a.PartNumber ?? 0).CompareTo(b.PartNumber ?? 0));
                    try
                    {
                        cmResult = await _client.CompleteMultipartUploadAsync(
                            BuildCompleteRequest(uploadId, parts), null, ct).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        SetError(ex);
                    }
                }

                if (HasError())
                {
                    if (!_options.LeavePartsOnError)
                    {
                        try
                        {
                            await _client.AbortMultipartUploadAsync(
                                BuildAbortRequest(uploadId), null, CancellationToken.None).ConfigureAwait(false);
                        }
                        catch
                        {
                            // best effort
                        }
                    }

                    ExceptionDispatchInfo.Capture(error!).Throw();
                }

                if (_enableCrc64)
                {
                    var ccrc = CombineCrc(crcParts);
                    if (cmResult!.Headers.TryGetValue("x-oss-hash-crc64ecma", out var scrc) && !string.IsNullOrEmpty(scrc))
                    {
                        var ccrcStr = ccrc.ToString(CultureInfo.InvariantCulture);
                        if (!string.Equals(ccrcStr, scrc, StringComparison.Ordinal))
                        {
                            cmResult.Headers.TryGetValue("x-oss-request-id", out var rid);
                            throw new InconsistentException(ccrcStr, scrc, rid ?? "");
                        }
                    }
                }

                return new UploadResult
                {
                    UploadId = uploadId,
                    ETag = cmResult!.ETag,
                    VersionId = cmResult.VersionId,
                    HashCrc64 = cmResult.HashCrc64,
                    Headers = cmResult.Headers,
                    Status = cmResult.Status,
                    StatusCode = cmResult.StatusCode
                };
            }

            private async Task ProduceFilePartsAsync(
                string uploadId, SemaphoreSlim sem, List<Task> tasks,
                Action<int, UploadPartResult, long> onPart, Action<Exception> setError, Func<bool> hasError,
                CancellationToken ct)
            {
                var offset = _readerPos;
                var partNumber = _startPartNumber;

                while (offset < _totalSize && !hasError())
                {
                    var size = Math.Min(_options.PartSize, _totalSize - offset);
                    partNumber++;

                    var capOffset = offset;
                    var capSize = size;
                    var capNum = partNumber;

                    await sem.WaitAsync(ct).ConfigureAwait(false);
                    if (hasError())
                    {
                        sem.Release();
                        break;
                    }

                    tasks.Add(Task.Run(async () =>
                    {
                        try
                        {
                            var fs = File.OpenRead(_filePath);
                            using var section = new SectionReadStream(fs, capOffset, capSize);
                            var up = await _client.UploadPartAsync(
                                BuildUploadPartRequest(uploadId, capNum, section), null, ct).ConfigureAwait(false);
                            onPart(capNum, up, capSize);
                        }
                        catch (Exception ex)
                        {
                            setError(ex);
                        }
                        finally
                        {
                            sem.Release();
                        }
                    }, ct));

                    offset += size;
                }
            }

            private async Task ProduceStreamPartsAsync(
                string uploadId, int startPartNum, SemaphoreSlim sem, List<Task> tasks,
                Action<int, UploadPartResult, long> onPart, Action<Exception> setError, Func<bool> hasError,
                CancellationToken ct)
            {
                if (_options.PartSize > int.MaxValue)
                    throw new NotSupportedException("PartSize larger than 2 GiB is not supported for stream uploads.");

                var partSize = (int)_options.PartSize;
                var partNumber = startPartNum;

                while (!hasError())
                {
                    await sem.WaitAsync(ct).ConfigureAwait(false);
                    if (hasError())
                    {
                        sem.Release();
                        break;
                    }

                    var buffer = new byte[partSize];
                    int n;
                    try
                    {
                        n = await ReadFullAsync(Body!, buffer, partSize, ct).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        sem.Release();
                        setError(ex);
                        break;
                    }

                    if (n == 0)
                    {
                        sem.Release();
                        break;
                    }

                    partNumber++;
                    var capNum = partNumber;
                    var capSize = n;
                    var capBuf = buffer;

                    tasks.Add(Task.Run(async () =>
                    {
                        try
                        {
                            using var ms = new MemoryStream(capBuf, 0, capSize, writable: false);
                            var up = await _client.UploadPartAsync(
                                BuildUploadPartRequest(uploadId, capNum, ms), null, ct).ConfigureAwait(false);
                            onPart(capNum, up, capSize);
                        }
                        catch (Exception ex)
                        {
                            setError(ex);
                        }
                        finally
                        {
                            sem.Release();
                        }
                    }, ct));
                }
            }

            private async Task<(string UploadId, int StartPartNum)> GetUploadIdAsync(CancellationToken ct)
            {
                if (!string.IsNullOrEmpty(_uploadId))
                    return (_uploadId, _startPartNumber);

                var r = await _client.InitiateMultipartUploadAsync(BuildInitiateRequest(), null, ct).ConfigureAwait(false);
                return (r.UploadId ?? "", 0);
            }

            private ulong CombineCrc(List<PartCrc> crcs)
            {
                if (crcs.Count == 0)
                    return _baseCrc64;

                crcs.Sort((a, b) => a.PartNumber.CompareTo(b.PartNumber));
                var crc = _baseCrc64;
                foreach (var c in crcs)
                {
                    if (c.Crc == null)
                        return 0;
                    if (!ulong.TryParse(c.Crc, NumberStyles.Integer, CultureInfo.InvariantCulture, out var v))
                        break;
                    crc = Crc64.Combine(crc, v, c.Size);
                }
                return crc;
            }

            private string? GetContentType()
            {
                var name = !string.IsNullOrEmpty(_filePath) ? _filePath : _request.Key;
                if (string.IsNullOrEmpty(name))
                    return null;
                var contentType = MimeUtils.GetMimeType(name!, "");
                return string.IsNullOrEmpty(contentType) ? null : contentType;
            }

            private InitiateMultipartUploadRequest BuildInitiateRequest()
            {
                var r = new InitiateMultipartUploadRequest { Bucket = _request.Bucket, Key = _request.Key };
                foreach (var kv in _request.Headers)
                {
                    if (string.Equals(kv.Key, "Content-Length", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(kv.Key, "Content-MD5", StringComparison.OrdinalIgnoreCase))
                        continue;
                    r.Headers[kv.Key] = kv.Value;
                }
                foreach (var kv in _request.Parameters)
                    r.Parameters[kv.Key] = kv.Value;
                if (_request.Metadata != null)
                    r.Metadata = new Dictionary<string, string>(_request.Metadata);
                if (r.ContentType == null)
                {
                    var contentType = GetContentType();
                    if (contentType != null)
                        r.ContentType = contentType;
                }
                return r;
            }

            private UploadPartRequest BuildUploadPartRequest(string uploadId, int partNumber, Stream body)
            {
                var r = new UploadPartRequest
                {
                    Bucket = _request.Bucket,
                    Key = _request.Key,
                    UploadId = uploadId,
                    PartNumber = partNumber,
                    Body = body
                };
                CopyHeader(r, "x-oss-request-payer");
                CopyHeader(r, "x-oss-traffic-limit");
                return r;
            }

            private CompleteMultipartUploadRequest BuildCompleteRequest(string uploadId, List<UploadPart> parts)
            {
                var r = new CompleteMultipartUploadRequest
                {
                    Bucket = _request.Bucket,
                    Key = _request.Key,
                    UploadId = uploadId,
                    CompleteMultipartUpload = new CompleteMultipartUpload { Parts = parts }
                };
                CopyHeader(r, "x-oss-forbid-overwrite");
                CopyHeader(r, "x-oss-object-acl");
                CopyHeader(r, "x-oss-callback");
                CopyHeader(r, "x-oss-callback-var");
                CopyHeader(r, "x-oss-request-payer");
                if (_request.Parameters.TryGetValue("encoding-type", out var et))
                    r.Parameters["encoding-type"] = et;
                return r;
            }

            private AbortMultipartUploadRequest BuildAbortRequest(string uploadId)
            {
                var r = new AbortMultipartUploadRequest
                {
                    Bucket = _request.Bucket,
                    Key = _request.Key,
                    UploadId = uploadId
                };
                CopyHeader(r, "x-oss-request-payer");
                return r;
            }

            private void CopyHeader(RequestModel dst, string key)
            {
                if (_request.Headers.TryGetValue(key, out var value))
                    dst.Headers[key] = value;
            }

            private static async Task<int> ReadFullAsync(Stream stream, byte[] buffer, int count, CancellationToken ct)
            {
                var total = 0;
                while (total < count)
                {
                    var n = await stream.ReadAsync(buffer, total, count - total, ct).ConfigureAwait(false);
                    if (n == 0)
                        break;
                    total += n;
                }
                return total;
            }

            private readonly struct PartCrc
            {
                public PartCrc(int partNumber, string? crc, long size)
                {
                    PartNumber = partNumber;
                    Crc = crc;
                    Size = size;
                }

                public int PartNumber { get; }
                public string? Crc { get; }
                public long Size { get; }
            }
        }
    }
}
