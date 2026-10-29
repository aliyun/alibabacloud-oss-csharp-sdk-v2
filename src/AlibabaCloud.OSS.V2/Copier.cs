using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using AlibabaCloud.OSS.V2.Extensions;
using AlibabaCloud.OSS.V2.Models;

namespace AlibabaCloud.OSS.V2
{
    /// <summary>
    /// Options that control how the <see cref="Copier"/> copies objects.
    /// </summary>
    public sealed class CopierOptions
    {
        /// <summary>The part size, in bytes, for multipart copy. Default is 64 MiB.</summary>
        public long PartSize { get; set; } = Defaults.DefaultCopyPartSize;

        /// <summary>The number of parts copied concurrently. Default is 3.</summary>
        public int ParallelNum { get; set; } = Defaults.DefaultCopyParallel;

        /// <summary>Objects larger than this threshold are copied as a multipart copy. Default is 200 MiB.</summary>
        public long MultipartCopyThreshold { get; set; } = Defaults.DefaultCopyThreshold;

        /// <summary>Whether to keep already-copied parts when the copy fails. Default is false.</summary>
        public bool LeavePartsOnError { get; set; }

        /// <summary>Whether to disable the server-side shallow copy fast path. Default is false.</summary>
        public bool DisableShallowCopy { get; set; }

        /// <summary>Whether to skip the source server-side-encryption check for shallow copy.</summary>
        public bool NoCheckSse { get; set; }

        /// <summary>Whether to skip the cross-bucket check for shallow copy.</summary>
        public bool NoCheckCrossBucket { get; set; }

        internal CopierOptions Clone() => new()
        {
            PartSize = PartSize,
            ParallelNum = ParallelNum,
            MultipartCopyThreshold = MultipartCopyThreshold,
            LeavePartsOnError = LeavePartsOnError,
            DisableShallowCopy = DisableShallowCopy,
            NoCheckSse = NoCheckSse,
            NoCheckCrossBucket = NoCheckCrossBucket
        };
    }

    /// <summary>
    /// The result of a copy performed by the <see cref="Copier"/>.
    /// </summary>
    public sealed class CopyResult : ResultModel
    {
        /// <summary>The multipart upload id, when a multipart copy was used.</summary>
        public string? UploadId { get; internal set; }

        /// <summary>The ETag of the copied object.</summary>
        public string? ETag { get; internal set; }

        /// <summary>The version id of the copied object.</summary>
        public string? VersionId { get; internal set; }

        /// <summary>The 64-bit ECMA-182 CRC of the copied object.</summary>
        public string? HashCrc64 { get; internal set; }
    }

    /// <summary>
    /// A high-level component that copies objects within OSS. Small objects are copied with a single
    /// CopyObject request (optionally a server-side shallow copy); larger objects are copied
    /// concurrently as a multipart copy. Ported from the Go V2 SDK (oss/copier.go).
    /// </summary>
    public sealed class Copier
    {
        private static readonly HashSet<string> MetadataCopied = new(StringComparer.OrdinalIgnoreCase)
        {
            "content-type",
            "content-language",
            "content-encoding",
            "content-disposition",
            "cache-control",
            "expires"
        };

        private static readonly HashSet<string> CopySpecificHeaders = new(StringComparer.OrdinalIgnoreCase)
        {
            "x-oss-copy-source-if-match",
            "x-oss-copy-source-if-none-match",
            "x-oss-copy-source-if-modified-since",
            "x-oss-copy-source-if-unmodified-since",
            "x-oss-metadata-directive",
            "x-oss-tagging-directive",
            "Content-Length",
            "Content-MD5"
        };

        private readonly Transfer.ITransferClient _client;
        private readonly CopierOptions _options;

        public Copier(Client client, params Action<CopierOptions>[] optFns)
            : this(new Transfer.ClientAdapter(client ?? throw new ArgumentNullException(nameof(client))), optFns)
        {
        }

        internal Copier(Transfer.ITransferClient client, params Action<CopierOptions>[] optFns)
        {
            _client = client;
            _options = new CopierOptions
            {
                PartSize = Defaults.DefaultCopyPartSize,
                ParallelNum = Defaults.DefaultCopyParallel,
                MultipartCopyThreshold = Defaults.DefaultCopyThreshold,
                LeavePartsOnError = false,
                DisableShallowCopy = false
            };

            if (optFns != null)
                foreach (var fn in optFns)
                    fn?.Invoke(_options);
        }

        /// <summary>
        /// Copies an object. Chooses a single copy, server-side shallow copy or multipart copy based
        /// on the source size and the configured thresholds.
        /// </summary>
        public async Task<CopyResult> CopyAsync(
            CopyObjectRequest request,
            Action<CopierOptions>? optFn = null,
            CancellationToken cancellationToken = default)
        {
            var d = NewDelegate(request, optFn);
            await d.CheckSourceAsync(cancellationToken).ConfigureAwait(false);
            d.ApplySource();
            return await d.CopyAsync(cancellationToken).ConfigureAwait(false);
        }

        private CopyDelegate NewDelegate(CopyObjectRequest request, Action<CopierOptions>? optFn)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));
            if (string.IsNullOrEmpty(request.Bucket))
                throw new ArgumentException("request.Bucket is required.", nameof(request));
            if (string.IsNullOrEmpty(request.Key))
                throw new ArgumentException("request.Key is required.", nameof(request));
            if (string.IsNullOrEmpty(request.SourceKey))
                throw new ArgumentException("request.SourceKey is required.", nameof(request));

            var options = _options.Clone();
            optFn?.Invoke(options);

            if (options.ParallelNum <= 0)
                options.ParallelNum = Defaults.DefaultCopyParallel;
            if (options.PartSize <= 0)
                options.PartSize = Defaults.DefaultCopyPartSize;
            if (options.MultipartCopyThreshold < 0)
                options.MultipartCopyThreshold = Defaults.DefaultCopyThreshold;

            return new CopyDelegate(_client, options, request);
        }

        private sealed class CopyDelegate
        {
            private readonly Transfer.ITransferClient _client;
            private readonly CopierOptions _options;
            private readonly CopyObjectRequest _request;

            private HeadObjectResult? _metaProp;
            private long _sizeInBytes;

            public CopyDelegate(Transfer.ITransferClient client, CopierOptions options, CopyObjectRequest request)
            {
                _client = client;
                _options = options;
                _request = request;
            }

            public async Task CheckSourceAsync(CancellationToken ct)
            {
                var head = new HeadObjectRequest
                {
                    Bucket = _request.SourceBucket ?? _request.Bucket,
                    Key = _request.SourceKey,
                    VersionId = _request.SourceVersionId
                };
                CopySourceConditionHeaders(head);
                if (_request.Headers.TryGetValue("x-oss-request-payer", out var payer))
                    head.Headers["x-oss-request-payer"] = payer;

                _metaProp = await _client.HeadObjectAsync(head, null, ct).ConfigureAwait(false);
            }

            public void ApplySource()
            {
                _sizeInBytes = _metaProp!.ContentLength ?? 0;

                if (_sizeInBytes <= _options.MultipartCopyThreshold)
                    return;

                var partSize = _options.PartSize;
                if (_sizeInBytes > 0)
                    while (_sizeInBytes / partSize >= Defaults.MaxUploadParts)
                        partSize += _options.PartSize;
                _options.PartSize = partSize;
            }

            public Task<CopyResult> CopyAsync(CancellationToken ct)
            {
                if (_sizeInBytes <= _options.MultipartCopyThreshold)
                    return SingleCopyAsync(ct);
                if (CanUseShallowCopy())
                    return ShallowCopyAsync(ct);
                return MultiCopyAsync(ct);
            }

            private async Task<CopyResult> SingleCopyAsync(CancellationToken ct)
            {
                var r = await _client.CopyObjectAsync(_request, null, ct).ConfigureAwait(false);
                return new CopyResult
                {
                    ETag = r.ETag,
                    VersionId = r.VersionId,
                    HashCrc64 = r.HashCrc64,
                    Headers = r.Headers,
                    Status = r.Status,
                    StatusCode = r.StatusCode
                };
            }

            private async Task<CopyResult> ShallowCopyAsync(CancellationToken ct)
            {
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                cts.CancelAfter(TimeSpan.FromSeconds(30));

                CopyObjectResult r;
                try
                {
                    r = await _client.CopyObjectAsync(_request, null, cts.Token).ConfigureAwait(false);
                }
                catch (Exception ex) when (ShouldFallbackToMultipart(ex, ct))
                {
                    return await MultiCopyAsync(ct).ConfigureAwait(false);
                }

                return new CopyResult
                {
                    ETag = r.ETag,
                    VersionId = r.VersionId,
                    HashCrc64 = r.HashCrc64,
                    Headers = r.Headers,
                    Status = r.Status,
                    StatusCode = r.StatusCode
                };
            }

            private static bool ShouldFallbackToMultipart(Exception ex, CancellationToken externalCt)
            {
                var inner = UnwrapOperationException(ex);

                // Timeout/cancellation that was not requested by the caller (e.g. the internal 30 s
                // shallow-copy window expiring) should fall back to multipart copy.
                if (!externalCt.IsCancellationRequested)
                {
                    if (inner is OperationCanceledException || inner is RequestTimeoutException)
                        return true;
                }

                // Server-side EntityTooLarge should fall back to multipart copy.
                if (inner is ServiceException se && string.Equals(se.ErrorCode, "EntityTooLarge", StringComparison.Ordinal))
                    return true;

                return false;
            }

            private static Exception? UnwrapOperationException(Exception ex)
            {
                if (ex is OperationException opEx)
                    return opEx.InnerException ?? ex;
                return ex;
            }

            private bool CanUseShallowCopy()
            {
                if (_options.DisableShallowCopy)
                    return false;

                if (!string.IsNullOrEmpty(_request.StorageClass))
                    return false;

                if (!_options.NoCheckCrossBucket)
                {
                    var srcBucket = _request.SourceBucket ?? _request.Bucket;
                    if (!string.Equals(srcBucket, _request.Bucket, StringComparison.Ordinal))
                        return false;
                }

                if (!_options.NoCheckSse)
                {
                    if (_metaProp!.Headers.TryGetValue("x-oss-server-side-encryption", out var sse) &&
                        !string.IsNullOrEmpty(sse))
                        return false;
                }

                return true;
            }

            private async Task<CopyResult> MultiCopyAsync(CancellationToken ct)
            {
                var initReq = await BuildInitiateRequestAsync(ct).ConfigureAwait(false);
                var initResult = await _client.InitiateMultipartUploadAsync(initReq, null, ct).ConfigureAwait(false);
                var uploadId = initResult.UploadId ?? "";

                var sem = new SemaphoreSlim(_options.ParallelNum);
                var tasks = new List<Task>();
                var gate = new object();
                var parts = new List<UploadPart>();
                Exception? error = null;
                Exception? cancelError = null;

                void SetError(Exception e) => Interlocked.CompareExchange(ref error, e, null);
                bool HasError() => Volatile.Read(ref error) != null;

                var readerPos = 0L;
                var partNumber = 0;
                while (readerPos < _sizeInBytes && !HasError())
                {
                    var n = Math.Min(_options.PartSize, _sizeInBytes - readerPos);
                    partNumber++;
                    var capNum = partNumber;
                    var capRange = $"bytes={readerPos.ToString(CultureInfo.InvariantCulture)}-{(readerPos + n - 1).ToString(CultureInfo.InvariantCulture)}";

                    bool entered;
                    try
                    {
                        await sem.WaitAsync(ct).ConfigureAwait(false);
                        entered = true;
                    }
                    catch (OperationCanceledException ex)
                    {
                        entered = false;
                        cancelError = ex;
                    }

                    if (!entered || HasError())
                    {
                        if (entered) sem.Release();
                        break;
                    }

                    tasks.Add(Task.Run(async () =>
                    {
                        try
                        {
                            var up = await _client.UploadPartCopyAsync(
                                BuildUploadPartCopyRequest(uploadId, capNum, capRange), null, ct).ConfigureAwait(false);
                            lock (gate)
                                parts.Add(new UploadPart { ETag = up.ETag, PartNumber = capNum });
                        }
                        catch (Exception ex)
                        {
                            SetError(ex);
                        }
                        finally
                        {
                            sem.Release();
                        }
                    }, ct));

                    readerPos += n;
                }

                await Task.WhenAll(tasks).ConfigureAwait(false);

                if (cancelError != null)
                    ExceptionDispatchInfo.Capture(cancelError).Throw();

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

                if (!string.IsNullOrEmpty(cmResult!.HashCrc64) &&
                    _metaProp!.Headers.TryGetValue("x-oss-hash-crc64ecma", out var srcCrc) &&
                    !string.IsNullOrEmpty(srcCrc))
                {
                    if (!string.Equals(cmResult.HashCrc64, srcCrc, StringComparison.Ordinal))
                    {
                        cmResult.Headers.TryGetValue("x-oss-request-id", out var rid);
                        throw new InconsistentException(srcCrc, cmResult.HashCrc64!, rid ?? "");
                    }
                }

                return new CopyResult
                {
                    UploadId = uploadId,
                    ETag = cmResult.ETag,
                    VersionId = cmResult.VersionId,
                    HashCrc64 = cmResult.HashCrc64,
                    Headers = cmResult.Headers,
                    Status = cmResult.Status,
                    StatusCode = cmResult.StatusCode
                };
            }

            private async Task<InitiateMultipartUploadRequest> BuildInitiateRequestAsync(CancellationToken ct)
            {
                var r = new InitiateMultipartUploadRequest
                {
                    Bucket = _request.Bucket,
                    Key = _request.Key,
                    DisableAutoDetectMimeType = true
                };

                // Preserve custom headers/parameters from the original CopyObjectRequest,
                // excluding headers that are specific to the single CopyObject/UploadPartCopy operations.
                CopyRequestHeadersAndParameters(r);

                await OverwriteMetadataAsync(r).ConfigureAwait(false);
                await OverwriteTaggingAsync(r, ct).ConfigureAwait(false);
                return r;
            }

            private Task OverwriteMetadataAsync(InitiateMultipartUploadRequest r)
            {
                var directive = (_request.MetadataDirective ?? "").ToLowerInvariant();
                switch (directive)
                {
                    case "":
                    case "copy":
                        // Remove metadata-related values copied from the request; they are replaced
                        // with the source object's metadata.
                        foreach (var key in MetadataCopied)
                            r.Headers.Remove(key);
                        r.Metadata = null;

                        // Remove any request-side x-oss-meta-* headers so they do not leak into
                        // the destination object; only source object user metadata is copied below.
                        var metaKeysToRemove = new List<string>();
                        foreach (var key in r.Headers.Keys)
                        {
                            if (key.StartsWith("x-oss-meta-", StringComparison.OrdinalIgnoreCase))
                                metaKeysToRemove.Add(key);
                        }
                        foreach (var key in metaKeysToRemove)
                            r.Headers.Remove(key);

                        // Copy metadata from the source object.
                        foreach (var kv in _metaProp!.Headers)
                        {
                            if (kv.Key.StartsWith("x-oss-meta-", StringComparison.OrdinalIgnoreCase))
                            {
                                r.Metadata ??= new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                                r.Metadata[kv.Key.Substring("x-oss-meta-".Length)] = kv.Value;
                            }
                            else if (MetadataCopied.Contains(kv.Key))
                            {
                                r.Headers[kv.Key] = kv.Value;
                            }
                        }
                        break;
                    case "replace":
                        // Metadata has already been copied from the original request.
                        if (_request.Metadata != null)
                            r.Metadata = new Dictionary<string, string>(_request.Metadata);
                        break;
                    default:
                        throw new ArgumentException(
                            $"Unsupported MetadataDirective, {_request.MetadataDirective}", nameof(_request));
                }
                return Task.CompletedTask;
            }

            private async Task OverwriteTaggingAsync(InitiateMultipartUploadRequest r, CancellationToken ct)
            {
                var directive = (_request.TaggingDirective ?? "").ToLowerInvariant();
                switch (directive)
                {
                    case "":
                    case "copy":
                        // Remove any tagging copied from the request; it is replaced with the source object's tags.
                        r.Headers.Remove("x-oss-tagging");
                        if ((_metaProp!.TaggingCount ?? 0) > 0)
                        {
                            var tagReq = new GetObjectTaggingRequest
                            {
                                Bucket = _request.SourceBucket ?? _request.Bucket,
                                Key = _request.SourceKey,
                                VersionId = _request.SourceVersionId
                            };
                            CopyHeader(tagReq, "x-oss-request-payer");
                            var tagResult = await _client.GetObjectTaggingAsync(tagReq, null, ct).ConfigureAwait(false);
                            var tags = new List<string>();
                            foreach (var t in tagResult.Tagging?.TagSet?.Tags ?? new List<Tag>())
                                tags.Add($"{(t.Key ?? string.Empty).UrlEncode()}={(t.Value ?? string.Empty).UrlEncode()}");
                            if (tags.Count > 0)
                                r.Tagging = string.Join("&", tags);
                        }
                        break;
                    case "replace":
                        // Tagging has already been copied from the original request.
                        break;
                    default:
                        throw new ArgumentException(
                            $"Unsupported TaggingDirective, {_request.TaggingDirective}", nameof(_request));
                }
            }

            private UploadPartCopyRequest BuildUploadPartCopyRequest(string uploadId, int partNumber, string sourceRange)
            {
                var r = new UploadPartCopyRequest
                {
                    Bucket = _request.Bucket,
                    Key = _request.Key,
                    SourceBucket = _request.SourceBucket ?? _request.Bucket,
                    SourceKey = _request.SourceKey,
                    SourceVersionId = _request.SourceVersionId,
                    UploadId = uploadId,
                    PartNumber = partNumber,
                    SourceRange = sourceRange
                };
                r.IfMatch = _request.IfMatch;
                r.IfNoneMatch = _request.IfNoneMatch;
                r.IfModifiedSince = _request.IfModifiedSince;
                r.IfUnmodifiedSince = _request.IfUnmodifiedSince;
                CopyHeader(r, "x-oss-request-payer");
                CopyHeader(r, "x-oss-traffic-limit");
                return r;
            }

            private void CopySourceConditionHeaders(HeadObjectRequest head)
            {
                head.IfMatch = _request.IfMatch;
                head.IfNoneMatch = _request.IfNoneMatch;
                head.IfModifiedSince = _request.IfModifiedSince;
                head.IfUnmodifiedSince = _request.IfUnmodifiedSince;
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
                CopyRequestHeadersAndParameters(r);
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
                CopyRequestHeadersAndParameters(r);
                return r;
            }

            private void CopyRequestHeadersAndParameters(RequestModel dst)
            {
                foreach (var kv in _request.Headers)
                {
                    if (CopySpecificHeaders.Contains(kv.Key))
                        continue;
                    dst.Headers[kv.Key] = kv.Value;
                }
                foreach (var kv in _request.Parameters)
                    dst.Parameters[kv.Key] = kv.Value;
            }

            private void CopyHeader(RequestModel dst, string key)
            {
                if (_request.Headers.TryGetValue(key, out var value))
                    dst.Headers[key] = value;
            }
        }
    }
}
