#nullable enable
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using AlibabaCloud.OSS.V2;
using AlibabaCloud.OSS.V2.Internal;
using AlibabaCloud.OSS.V2.Models;
using AlibabaCloud.OSS.V2.Transfer;

namespace AlibabaCloud.OSS.V2.UnitTests.Fakes
{
    /// <summary>
    /// An in-memory <see cref="ITransferClient"/> test double. It simulates the OSS data plane
    /// (put/get/head/copy and the full multipart lifecycle) over an in-memory object store so that
    /// upload/download/copy round-trips can be verified for data integrity and CRC64. Every
    /// operation increments a counter and consults an optional failure hook, so tests can assert
    /// call behavior and inject errors at any step.
    /// </summary>
    internal sealed class FakeTransferClient : ITransferClient
    {
        static FakeTransferClient() => Crc64.InitECMA();

        private readonly object _gate = new();
        private readonly Dictionary<string, StoredObject> _objects = new(StringComparer.Ordinal);
        private readonly Dictionary<string, MultipartUpload> _uploads = new(StringComparer.Ordinal);

        private readonly ClientOptions _options;

        public FakeTransferClient(FeatureFlagsType? featureFlags = null)
        {
            _options = new ClientOptions();
            if (featureFlags.HasValue)
                _options.FeatureFlags = featureFlags.Value;
        }

        // --- Call counters ---
        public int PutObjectCount;
        public int GetObjectCount;
        public int HeadObjectCount;
        public int CopyObjectCount;
        public int GetObjectTaggingCount;
        public int InitiateCount;
        public int UploadPartCount;
        public int UploadPartCopyCount;
        public int CompleteCount;
        public int AbortCount;
        public int ListPartsCount;

        // --- Failure hooks (return non-null to throw) ---
        public Func<PutObjectRequest, Exception?>? OnPutObject;
        public Func<GetObjectRequest, Exception?>? OnGetObject;
        public Func<GetObjectRequest, Task>? GetObjectHook;
        public Action<GetObjectRequest, GetObjectResult>? MutateGetObjectResult;
        public Func<HeadObjectRequest, Exception?>? OnHeadObject;
        public Func<CopyObjectRequest, Exception?>? OnCopyObject;
        public Func<GetObjectTaggingRequest, Exception?>? OnGetObjectTagging;
        public Func<InitiateMultipartUploadRequest, Exception?>? OnInitiate;
        public Func<UploadPartRequest, Exception?>? OnUploadPart;
        public Func<UploadPartCopyRequest, Exception?>? OnUploadPartCopy;
        public Func<UploadPartCopyRequest, Task>? UploadPartCopyHook;
        public Func<CompleteMultipartUploadRequest, Exception?>? OnComplete;
        public Func<AbortMultipartUploadRequest, Exception?>? OnAbort;

        // --- Captured requests (for assertions) ---
        public readonly List<HeadObjectRequest> HeadObjectRequests = new();
        public readonly List<GetObjectRequest> GetObjectRequests = new();
        public readonly List<GetObjectTaggingRequest> GetObjectTaggingRequests = new();
        public readonly List<InitiateMultipartUploadRequest> InitiateRequests = new();
        public readonly List<CompleteMultipartUploadRequest> CompleteRequests = new();
        public readonly List<AbortMultipartUploadRequest> AbortRequests = new();
        public readonly List<UploadPartRequest> UploadPartRequests = new();
        public readonly List<UploadPartCopyRequest> UploadPartCopyRequests = new();
        public readonly List<PutObjectRequest> PutObjectRequests = new();
        public readonly List<CopyObjectRequest> CopyObjectRequests = new();

        /// <summary>Forces the CompleteMultipartUpload response CRC to this value (to simulate a mismatch).</summary>
        public string? CompleteCrcOverride;

        /// <summary>Forces the CopyObject response CRC to this value.</summary>
        public string? CopyObjectCrcOverride;

        /// <summary>Forces the HeadObject response CRC to this value (to simulate a download mismatch).</summary>
        public string? HeadCrcOverride;

        public ClientOptions GetClientOptions() => _options;

        // --- Seeding helpers ---

        /// <summary>Places an object into the store and returns its CRC64 as a decimal string.</summary>
        public string SeedObject(string bucket, string key, byte[] data,
            IDictionary<string, string>? extraHeaders = null, List<Tag>? tags = null)
        {
            var obj = new StoredObject(data);
            if (extraHeaders != null)
                foreach (var kv in extraHeaders)
                    obj.Headers[kv.Key] = kv.Value;
            if (tags != null)
                obj.Tags.AddRange(tags);
            lock (_gate)
                _objects[Id(bucket, key)] = obj;
            return obj.Crc.ToString(CultureInfo.InvariantCulture);
        }

        public byte[]? GetStored(string bucket, string key)
        {
            lock (_gate)
                return _objects.TryGetValue(Id(bucket, key), out var o) ? o.Data : null;
        }

        // --- ITransferClient ---

        public Task<PutObjectResult> PutObjectAsync(
            PutObjectRequest request, OperationOptions? options = null, CancellationToken cancellationToken = default)
        {
            Interlocked.Increment(ref PutObjectCount);
            lock (_gate) PutObjectRequests.Add(request);
            ThrowIf(OnPutObject?.Invoke(request));
            cancellationToken.ThrowIfCancellationRequested();

            var data = ReadAll(request.Body);
            var obj = new StoredObject(data);
            if (request.ContentType != null)
                obj.Headers["Content-Type"] = request.ContentType;
            lock (_gate) _objects[Id(request.Bucket!, request.Key!)] = obj;

            var result = new PutObjectResult { StatusCode = 200, Status = "OK" };
            result.Headers["ETag"] = Etag(data);
            result.Headers["x-oss-hash-crc64ecma"] = obj.Crc.ToString(CultureInfo.InvariantCulture);
            result.Headers["x-oss-request-id"] = NewRequestId();
            return Task.FromResult(result);
        }

        public async Task<GetObjectResult> GetObjectAsync(
            GetObjectRequest request, OperationOptions? options = null, CancellationToken cancellationToken = default)
        {
            Interlocked.Increment(ref GetObjectCount);
            lock (_gate) GetObjectRequests.Add(request);
            ThrowIf(OnGetObject?.Invoke(request));
            cancellationToken.ThrowIfCancellationRequested();

            if (GetObjectHook != null)
                await GetObjectHook(request).ConfigureAwait(false);

            StoredObject obj;
            lock (_gate)
            {
                if (!_objects.TryGetValue(Id(request.Bucket!, request.Key!), out obj!))
                    throw new InvalidOperationException($"NoSuchKey: {request.Key}");
            }

            var start = 0L;
            var end = (long)obj.Data.Length; // exclusive
            if (!string.IsNullOrEmpty(request.Range))
            {
                var (s, e) = ParseByteRange(request.Range!, obj.Data.Length);
                start = s;
                end = e;
            }

            var len = (int)(end - start);
            var slice = new byte[len];
            Array.Copy(obj.Data, start, slice, 0, len);

            var result = new GetObjectResult { StatusCode = string.IsNullOrEmpty(request.Range) ? 200 : 206, Status = "OK" };
            result.InnerBody = new MemoryStream(slice, writable: false);
            foreach (var kv in obj.Headers)
                result.Headers[kv.Key] = kv.Value;
            result.Headers["Content-Length"] = len.ToString(CultureInfo.InvariantCulture);
            result.Headers["ETag"] = obj.Headers.TryGetValue("ETag", out var etag) ? etag : Etag(obj.Data);
            result.Headers["x-oss-hash-crc64ecma"] = obj.Crc.ToString(CultureInfo.InvariantCulture);
            result.Headers["x-oss-request-id"] = NewRequestId();
            if (!string.IsNullOrEmpty(request.Range))
            {
                result.Headers["Content-Range"] = $"bytes {start}-{end - 1}/{obj.Data.Length}";
            }

            MutateGetObjectResult?.Invoke(request, result);
            return result;
        }

        public Task<HeadObjectResult> HeadObjectAsync(
            HeadObjectRequest request, OperationOptions? options = null, CancellationToken cancellationToken = default)
        {
            Interlocked.Increment(ref HeadObjectCount);
            lock (_gate) HeadObjectRequests.Add(request);
            ThrowIf(OnHeadObject?.Invoke(request));
            cancellationToken.ThrowIfCancellationRequested();

            StoredObject obj;
            lock (_gate)
            {
                if (!_objects.TryGetValue(Id(request.Bucket!, request.Key!), out obj!))
                    throw new InvalidOperationException($"NoSuchKey: {request.Key}");
            }

            var result = new HeadObjectResult { StatusCode = 200, Status = "OK" };
            foreach (var kv in obj.Headers)
                result.Headers[kv.Key] = kv.Value;
            result.Headers["Content-Length"] = obj.Data.Length.ToString(CultureInfo.InvariantCulture);
            result.Headers["ETag"] = Etag(obj.Data);
            result.Headers["Last-Modified"] = "Mon, 01 Jan 2024 00:00:00 GMT";
            result.Headers["x-oss-hash-crc64ecma"] = HeadCrcOverride ?? obj.Crc.ToString(CultureInfo.InvariantCulture);
            if (obj.Tags.Count > 0)
                result.Headers["x-oss-tagging-count"] = obj.Tags.Count.ToString(CultureInfo.InvariantCulture);
            result.Headers["x-oss-request-id"] = NewRequestId();
            return Task.FromResult(result);
        }

        public Task<CopyObjectResult> CopyObjectAsync(
            CopyObjectRequest request, OperationOptions? options = null, CancellationToken cancellationToken = default)
        {
            Interlocked.Increment(ref CopyObjectCount);
            lock (_gate) CopyObjectRequests.Add(request);
            ThrowIf(OnCopyObject?.Invoke(request));
            cancellationToken.ThrowIfCancellationRequested();

            var srcBucket = request.SourceBucket ?? request.Bucket;
            StoredObject src;
            lock (_gate)
            {
                if (!_objects.TryGetValue(Id(srcBucket!, request.SourceKey!), out src!))
                    throw new InvalidOperationException($"NoSuchKey: {request.SourceKey}");
                _objects[Id(request.Bucket!, request.Key!)] = new StoredObject((byte[])src.Data.Clone());
            }

            var result = new CopyObjectResult { StatusCode = 200, Status = "OK", ETag = Etag(src.Data) };
            result.Headers["ETag"] = Etag(src.Data);
            result.Headers["x-oss-hash-crc64ecma"] = CopyObjectCrcOverride ?? src.Crc.ToString(CultureInfo.InvariantCulture);
            result.Headers["x-oss-request-id"] = NewRequestId();
            return Task.FromResult(result);
        }

        public Task<GetObjectTaggingResult> GetObjectTaggingAsync(
            GetObjectTaggingRequest request, OperationOptions? options = null, CancellationToken cancellationToken = default)
        {
            Interlocked.Increment(ref GetObjectTaggingCount);
            lock (_gate) GetObjectTaggingRequests.Add(request);
            ThrowIf(OnGetObjectTagging?.Invoke(request));
            cancellationToken.ThrowIfCancellationRequested();

            List<Tag> tags;
            lock (_gate)
                tags = _objects.TryGetValue(Id(request.Bucket!, request.Key!), out var o) ? o.Tags.ToList() : new List<Tag>();

            var result = new GetObjectTaggingResult { StatusCode = 200, Status = "OK" };
            result.InnerBody = new Tagging { TagSet = new TagSet { Tags = tags } };
            return Task.FromResult(result);
        }

        public Task<InitiateMultipartUploadResult> InitiateMultipartUploadAsync(
            InitiateMultipartUploadRequest request, OperationOptions? options = null, CancellationToken cancellationToken = default)
        {
            Interlocked.Increment(ref InitiateCount);
            lock (_gate) InitiateRequests.Add(request);
            ThrowIf(OnInitiate?.Invoke(request));
            cancellationToken.ThrowIfCancellationRequested();

            var uploadId = Guid.NewGuid().ToString("N");
            lock (_gate)
                _uploads[uploadId] = new MultipartUpload(request.Bucket!, request.Key!);

            var result = new InitiateMultipartUploadResult
            {
                StatusCode = 200,
                Status = "OK",
                Bucket = request.Bucket,
                Key = request.Key,
                UploadId = uploadId
            };
            result.Headers["x-oss-request-id"] = NewRequestId();
            return Task.FromResult(result);
        }

        public Task<UploadPartResult> UploadPartAsync(
            UploadPartRequest request, OperationOptions? options = null, CancellationToken cancellationToken = default)
        {
            Interlocked.Increment(ref UploadPartCount);
            lock (_gate) UploadPartRequests.Add(request);
            ThrowIf(OnUploadPart?.Invoke(request));
            cancellationToken.ThrowIfCancellationRequested();

            var data = ReadAll(request.Body);
            lock (_gate)
            {
                if (_uploads.TryGetValue(request.UploadId!, out var up))
                    up.Parts[(int)request.PartNumber!] = data;
            }

            var result = new UploadPartResult { StatusCode = 200, Status = "OK" };
            result.Headers["ETag"] = Etag(data);
            result.Headers["x-oss-hash-crc64ecma"] = Crc64.Compute(data, 0, data.Length).ToString(CultureInfo.InvariantCulture);
            result.Headers["x-oss-request-id"] = NewRequestId();
            return Task.FromResult(result);
        }

        public async Task<UploadPartCopyResult> UploadPartCopyAsync(
            UploadPartCopyRequest request, OperationOptions? options = null, CancellationToken cancellationToken = default)
        {
            Interlocked.Increment(ref UploadPartCopyCount);
            lock (_gate) UploadPartCopyRequests.Add(request);
            ThrowIf(OnUploadPartCopy?.Invoke(request));
            cancellationToken.ThrowIfCancellationRequested();

            if (UploadPartCopyHook != null)
                await UploadPartCopyHook(request).ConfigureAwait(false);

            var srcBucket = request.SourceBucket ?? request.Bucket;
            byte[] data;
            lock (_gate)
            {
                if (!_objects.TryGetValue(Id(srcBucket!, request.SourceKey!), out var src))
                    throw new InvalidOperationException($"NoSuchKey: {request.SourceKey}");
                var (start, end) = ParseByteRange(request.SourceRange!, src.Data.Length);
                var len = (int)(end - start);
                data = new byte[len];
                Array.Copy(src.Data, start, data, 0, len);
                if (_uploads.TryGetValue(request.UploadId!, out var up))
                    up.Parts[(int)request.PartNumber!] = data;
            }

            var result = new UploadPartCopyResult { StatusCode = 200, Status = "OK", ETag = Etag(data) };
            result.Headers["x-oss-request-id"] = NewRequestId();
            return result;
        }

        public Task<CompleteMultipartUploadResult> CompleteMultipartUploadAsync(
            CompleteMultipartUploadRequest request, OperationOptions? options = null, CancellationToken cancellationToken = default)
        {
            Interlocked.Increment(ref CompleteCount);
            lock (_gate) CompleteRequests.Add(request);
            ThrowIf(OnComplete?.Invoke(request));
            cancellationToken.ThrowIfCancellationRequested();

            byte[] assembled;
            lock (_gate)
            {
                if (!_uploads.TryGetValue(request.UploadId!, out var up))
                    throw new InvalidOperationException($"NoSuchUpload: {request.UploadId}");
                var ordered = up.Parts.OrderBy(p => p.Key).Select(p => p.Value).ToList();
                assembled = ordered.SelectMany(b => b).ToArray();
                _objects[Id(request.Bucket!, request.Key!)] = new StoredObject(assembled);
                _uploads.Remove(request.UploadId!);
            }

            var crc = Crc64.Compute(assembled, 0, assembled.Length).ToString(CultureInfo.InvariantCulture);
            var result = new CompleteMultipartUploadResult
            {
                StatusCode = 200,
                Status = "OK",
                Bucket = request.Bucket,
                Key = request.Key,
                ETag = Etag(assembled)
            };
            result.Headers["x-oss-hash-crc64ecma"] = CompleteCrcOverride ?? crc;
            result.Headers["x-oss-request-id"] = NewRequestId();
            return Task.FromResult(result);
        }

        public Task<AbortMultipartUploadResult> AbortMultipartUploadAsync(
            AbortMultipartUploadRequest request, OperationOptions? options = null, CancellationToken cancellationToken = default)
        {
            Interlocked.Increment(ref AbortCount);
            lock (_gate) AbortRequests.Add(request);
            ThrowIf(OnAbort?.Invoke(request));
            lock (_gate) _uploads.Remove(request.UploadId!);

            var result = new AbortMultipartUploadResult { StatusCode = 204, Status = "No Content" };
            result.Headers["x-oss-request-id"] = NewRequestId();
            return Task.FromResult(result);
        }

        public Task<ListPartsResult> ListPartsAsync(
            ListPartsRequest request, OperationOptions? options = null, CancellationToken cancellationToken = default)
        {
            Interlocked.Increment(ref ListPartsCount);
            cancellationToken.ThrowIfCancellationRequested();

            var parts = new List<Part>();
            lock (_gate)
            {
                if (_uploads.TryGetValue(request.UploadId!, out var up))
                    foreach (var p in up.Parts.OrderBy(x => x.Key))
                        parts.Add(new Part
                        {
                            PartNumber = p.Key,
                            Size = p.Value.Length,
                            ETag = Etag(p.Value),
                            HashCrc64 = Crc64.Compute(p.Value, 0, p.Value.Length).ToString(CultureInfo.InvariantCulture)
                        });
            }

            var result = new ListPartsResult
            {
                StatusCode = 200,
                Status = "OK",
                Bucket = request.Bucket,
                Key = request.Key,
                UploadId = request.UploadId,
                IsTruncated = false,
                Parts = parts
            };
            return Task.FromResult(result);
        }

        // --- Helpers ---

        private static void ThrowIf(Exception? ex)
        {
            if (ex != null)
                throw ex;
        }

        private static string Id(string bucket, string key) => bucket + "/" + key;

        private static string NewRequestId() => Guid.NewGuid().ToString("N").ToUpperInvariant();

        private static byte[] ReadAll(Stream? body)
        {
            if (body == null)
                return Array.Empty<byte>();
            if (body is MemoryStream ms && ms.CanSeek)
                return ms.ToArray();
            using var buffer = new MemoryStream();
            body.CopyTo(buffer);
            return buffer.ToArray();
        }

        private static string Etag(byte[] data)
        {
            using var md5 = MD5.Create();
            var hash = md5.ComputeHash(data);
            return "\"" + BitConverter.ToString(hash).Replace("-", "") + "\"";
        }

        private static (long Start, long End) ParseByteRange(string range, long size)
        {
            // "bytes=start-end" (end inclusive; end may be omitted).
            var spec = range.StartsWith("bytes=", StringComparison.OrdinalIgnoreCase) ? range.Substring(6) : range;
            var dash = spec.IndexOf('-');
            var startStr = spec.Substring(0, dash);
            var endStr = spec.Substring(dash + 1);
            var start = long.Parse(startStr, CultureInfo.InvariantCulture);
            var end = string.IsNullOrEmpty(endStr)
                ? size
                : Math.Min(long.Parse(endStr, CultureInfo.InvariantCulture) + 1, size); // exclusive
            return (start, end);
        }

        private sealed class StoredObject
        {
            public StoredObject(byte[] data)
            {
                Data = data;
                Crc = Crc64.Compute(data, 0, data.Length);
            }

            public byte[] Data { get; }
            public ulong Crc { get; }
            public Dictionary<string, string> Headers { get; } = new(StringComparer.OrdinalIgnoreCase);
            public List<Tag> Tags { get; } = new();
        }

        private sealed class MultipartUpload
        {
            public MultipartUpload(string bucket, string key)
            {
                Bucket = bucket;
                Key = key;
            }

            public string Bucket { get; }
            public string Key { get; }
            public SortedDictionary<int, byte[]> Parts { get; } = new();
        }
    }
}
