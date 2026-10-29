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
    /// Options that control how the <see cref="Downloader"/> transfers data.
    /// </summary>
    public sealed class DownloaderOptions
    {
        /// <summary>The part size, in bytes. Default is 6 MiB.</summary>
        public long PartSize { get; set; } = Defaults.DefaultDownloadPartSize;

        /// <summary>The number of parts downloaded concurrently. Default is 3.</summary>
        public int ParallelNum { get; set; } = Defaults.DefaultDownloadParallel;

        /// <summary>Whether to enable resumable download via a checkpoint file.</summary>
        public bool EnableCheckpoint { get; set; }

        /// <summary>The directory that stores the checkpoint file. Empty means the system temp dir.</summary>
        public string? CheckpointDir { get; set; }

        /// <summary>Whether to verify the CRC-64 of the already-downloaded data when a download is resumed.</summary>
        public bool VerifyData { get; set; }

        /// <summary>Whether to download to a temporary file that is renamed on success. Default is true.</summary>
        public bool UseTempFile { get; set; } = true;

        internal DownloaderOptions Clone() => new()
        {
            PartSize = PartSize,
            ParallelNum = ParallelNum,
            EnableCheckpoint = EnableCheckpoint,
            CheckpointDir = CheckpointDir,
            VerifyData = VerifyData,
            UseTempFile = UseTempFile
        };
    }

    /// <summary>
    /// The result of a download performed by the <see cref="Downloader"/>.
    /// </summary>
    public sealed class DownloadResult
    {
        /// <summary>The number of bytes written to the destination file.</summary>
        public long Written { get; internal set; }
    }

    /// <summary>
    /// A high-level component that downloads objects from OSS to a local file. The object is
    /// downloaded concurrently as ranged parts written to their target offsets, with optional
    /// CRC64 integrity checking and resumable checkpoints. Ported from the Go V2 SDK
    /// (oss/downloader.go).
    /// </summary>
    public sealed class Downloader
    {
        private readonly Transfer.ITransferClient _client;
        private readonly DownloaderOptions _options;
        private readonly bool _featureEnableCrc64;

        public Downloader(Client client, params Action<DownloaderOptions>[] optFns)
            : this(new Transfer.ClientAdapter(client ?? throw new ArgumentNullException(nameof(client))), optFns)
        {
        }

        internal Downloader(Transfer.ITransferClient client, params Action<DownloaderOptions>[] optFns)
        {
            _client = client;
            _options = new DownloaderOptions
            {
                PartSize = Defaults.DefaultDownloadPartSize,
                ParallelNum = Defaults.DefaultDownloadParallel,
                UseTempFile = true
            };

            if (optFns != null)
                foreach (var fn in optFns)
                    fn?.Invoke(_options);

            _featureEnableCrc64 = _client.GetClientOptions().FeatureFlags.HasFlag(FeatureFlagsType.EnableCrc64CheckDownload);
        }

        /// <summary>
        /// Downloads an object to a local file. Supports concurrent ranged download, CRC64 checking,
        /// progress reporting and resumable checkpoints.
        /// </summary>
        public async Task<DownloadResult> DownloadFileAsync(
            GetObjectRequest request,
            string filePath,
            Action<DownloaderOptions>? optFn = null,
            CancellationToken cancellationToken = default)
        {
            var d = NewDelegate(request, filePath, optFn);
            await d.CheckSourceAsync(cancellationToken).ConfigureAwait(false);

            var file = d.CheckDestination();
            try
            {
                d.AdjustRange();
                d.CheckCheckpoint();
                d.AdjustWriter(file);
                d.UpdateCrcFlag();
                var result = await d.DownloadAsync(file, cancellationToken).ConfigureAwait(false);
                d.CloseWriter(file, null);
                return result;
            }
            catch (Exception ex)
            {
                d.CloseWriter(file, ex);
                throw;
            }
        }

        private DownloadDelegate NewDelegate(GetObjectRequest request, string filePath, Action<DownloaderOptions>? optFn)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));
            if (string.IsNullOrEmpty(request.Bucket))
                throw new ArgumentException("request.Bucket is required.", nameof(request));
            if (string.IsNullOrEmpty(request.Key))
                throw new ArgumentException("request.Key is required.", nameof(request));
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentException("filePath is required.", nameof(filePath));

            var options = _options.Clone();
            optFn?.Invoke(options);

            if (options.ParallelNum <= 0)
                options.ParallelNum = Defaults.DefaultDownloadParallel;
            if (options.PartSize <= 0)
                options.PartSize = Defaults.DefaultDownloadPartSize;

            return new DownloadDelegate(_client, options, request, filePath, _featureEnableCrc64);
        }

        private sealed class DownloadDelegate
        {
            private readonly Transfer.ITransferClient _client;
            private readonly DownloaderOptions _options;
            private readonly GetObjectRequest _request;
            private readonly bool _featureEnableCrc64;

            private long _sizeInBytes;
            private string _etag = "";
            private string _modTime = "";
            private IDictionary<string, string> _headers = new Dictionary<string, string>();

            private readonly string _filePath;
            private string _tempFilePath = "";
            private long _existingFileSize;

            private long _rstart;
            private long _pos;
            private long _epos;
            private long _written;

            private bool _calcCrc;
            private bool _checkCrc;

            private DownloadCheckpoint? _checkpoint;

            private SectionWriter? _writer;
            private readonly object _progressGate = new();

            public DownloadDelegate(Transfer.ITransferClient client, DownloaderOptions options, GetObjectRequest request,
                string filePath, bool featureEnableCrc64)
            {
                _client = client;
                _options = options;
                _request = request;
                _filePath = Path.GetFullPath(filePath);
                _featureEnableCrc64 = featureEnableCrc64;
            }

            public async Task CheckSourceAsync(CancellationToken ct)
            {
                var head = new HeadObjectRequest { Bucket = _request.Bucket, Key = _request.Key };
                if (_request.Parameters.TryGetValue("versionId", out var vid))
                    head.VersionId = vid;

                // Forward user-supplied headers (preconditions, request payer, etc.) to HEAD.
                // Range headers are not meaningful for HEAD and must be excluded.
                foreach (var kv in _request.Headers)
                {
                    if (string.Equals(kv.Key, "Range", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(kv.Key, "x-oss-range-behavior", StringComparison.OrdinalIgnoreCase))
                        continue;
                    head.Headers[kv.Key] = kv.Value;
                }

                var result = await _client.HeadObjectAsync(head, null, ct).ConfigureAwait(false);
                _sizeInBytes = result.ContentLength ?? 0;
                _modTime = result.LastModified ?? "";
                _etag = result.ETag ?? "";
                _headers = result.Headers;
            }

            public FileStream CheckDestination()
            {
                _tempFilePath = _options.UseTempFile ? _filePath + Defaults.TempFileSuffix : _filePath;

                // Remove an existing symbolic link at the temporary path before opening it,
                // so that the download does not write through the link to an unexpected target.
                if (_options.UseTempFile && IsSymbolicLink(_tempFilePath))
                    File.Delete(_tempFilePath);

                var file = new FileStream(_tempFilePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite);
                _existingFileSize = file.Length;
                return file;
            }

            public void AdjustRange()
            {
                _pos = 0;
                _rstart = 0;
                _epos = _sizeInBytes;

                if (!string.IsNullOrEmpty(_request.Range))
                {
                    var r = RangeUtils.ParseRange(_request.Range);
                    if (r == null)
                        throw new ArgumentException($"Invalid range, {_request.Range}", nameof(_request));
                    var hr = r.Value;
                    if (hr.IsSuffix)
                    {
                        _pos = Math.Max(0, _sizeInBytes - hr.SuffixLength);
                        _rstart = _pos;
                        _epos = _sizeInBytes;
                    }
                    else
                    {
                        if (hr.Start >= _sizeInBytes)
                            throw new ArgumentException(
                                $"Invalid range, object size {_sizeInBytes}, range {_request.Range}", nameof(_request));
                        _pos = hr.Start;
                        _rstart = _pos;
                        if (hr.Count > 0)
                        {
                            // Compare counts directly to avoid overflow in hr.Start + hr.Count.
                            var remaining = _sizeInBytes - _pos;
                            _epos = hr.Count < remaining ? _pos + hr.Count : _sizeInBytes;
                        }
                    }
                }
            }

            public void CheckCheckpoint()
            {
                if (!_options.EnableCheckpoint)
                    return;

                _checkpoint = DownloadCheckpoint.Create(
                    _request.Bucket!, _request.Key!,
                    _request.Parameters.TryGetValue("versionId", out var vid) ? vid : null,
                    _request.Range, _tempFilePath, _options.CheckpointDir, _options.VerifyData,
                    _sizeInBytes, _modTime, _etag, _options.PartSize);
                _checkpoint.Load();

                if (_checkpoint.Loaded)
                {
                    _pos = _checkpoint.Offset;
                    _written = _pos - _rstart;
                }
                else
                {
                    _checkpoint.Offset = _pos;
                }
            }

            public void AdjustWriter(FileStream file)
            {
                var expectSize = _epos - _rstart;
                if (_existingFileSize > expectSize)
                    file.SetLength(_pos - _rstart);
                _writer = new SectionWriter(file);
            }

            public void UpdateCrcFlag()
            {
                if (!_featureEnableCrc64)
                    return;
                _checkCrc = string.IsNullOrEmpty(_request.Range);
                _calcCrc = (_checkpoint != null && _checkpoint.VerifyData) || _checkCrc;
            }

            public async Task<DownloadResult> DownloadAsync(FileStream file, CancellationToken ct)
            {
                var tracks = _calcCrc || _checkpoint != null;

                var trackerGate = new object();
                var pending = new List<DownloadedChunk>();
                var tOffset = _checkpoint?.Offset ?? _pos;
                var tCrc = _checkpoint?.Crc64 ?? 0UL;

                void OnChunkDone(DownloadedChunk c)
                {
                    if (!tracks)
                        return;
                    lock (trackerGate)
                    {
                        pending.Add(c);
                        pending.Sort((a, b) => a.Start.CompareTo(b.Start));
                        var i = 0;
                        var newOffset = tOffset;
                        foreach (var pc in pending)
                        {
                            if (pc.Start == newOffset)
                            {
                                newOffset += pc.Size;
                                i++;
                            }
                            else
                            {
                                break;
                            }
                        }

                        if (newOffset != tOffset)
                        {
                            if (_calcCrc)
                                for (var j = 0; j < i; j++)
                                    tCrc = Crc64.Combine(tCrc, pending[j].Crc64, pending[j].Size);
                            tOffset = newOffset;
                            pending.RemoveRange(0, i);
                            if (_checkpoint != null)
                            {
                                _checkpoint.Offset = tOffset;
                                _checkpoint.Crc64 = tCrc;
                                _checkpoint.Dump();
                            }
                        }
                    }
                }

                if (_request.ProgressFn != null && _written > 0)
                    _request.ProgressFn(_written, _written, _sizeInBytes);

                var sem = new SemaphoreSlim(_options.ParallelNum);
                var tasks = new List<Task>();
                Exception? error = null;
                Exception? cancelError = null;
                void SetError(Exception e) => Interlocked.CompareExchange(ref error, e, null);
                bool HasError() => Volatile.Read(ref error) != null;

                var pos = _pos;
                while (pos < _epos && !HasError())
                {
                    var size = Math.Min(_epos - pos, _options.PartSize);
                    var capStart = pos;
                    var capSize = size;

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
                            var chunk = await DownloadChunkAsync(capStart, capSize, ct).ConfigureAwait(false);
                            OnChunkDone(chunk);
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

                    pos += size;
                }

                await Task.WhenAll(tasks).ConfigureAwait(false);

                if (cancelError != null)
                    ExceptionDispatchInfo.Capture(cancelError).Throw();

                if (HasError())
                    ExceptionDispatchInfo.Capture(error!).Throw();

                if (_checkCrc)
                {
                    var finalCrc = tCrc;
                    if (pending.Count > 0)
                    {
                        pending.Sort((a, b) => a.Start.CompareTo(b.Start));
                        foreach (var c in pending)
                            finalCrc = Crc64.Combine(finalCrc, c.Crc64, c.Size);
                    }

                    if (_headers.TryGetValue("x-oss-hash-crc64ecma", out var scrc) && !string.IsNullOrEmpty(scrc))
                    {
                        var ccrcStr = finalCrc.ToString(CultureInfo.InvariantCulture);
                        if (!string.Equals(ccrcStr, scrc, StringComparison.Ordinal))
                        {
                            _headers.TryGetValue("x-oss-request-id", out var rid);
                            throw new InconsistentException(ccrcStr, scrc, rid ?? "");
                        }
                    }
                }

                return new DownloadResult { Written = _written };
            }

            private async Task<DownloadedChunk> DownloadChunkAsync(long start, long size, CancellationToken ct)
            {
                var req = BuildGetRequest(start, size);
                var result = await _client.GetObjectAsync(req, null, ct).ConfigureAwait(false);

                var body = result.Body ?? throw new InvalidOperationException("GetObject returned an empty body.");
                if (size > 0)
                    body = new LimitedReadStream(body, size);

#if NET5_0_OR_GREATER
                await using (body)
#else
                using (body)
#endif
                {
                    ValidateChunkResponse(result, start, size);

                    var hash = _calcCrc ? new HashCrc64(0UL) : null;
                    long written = 0;
                    var buffer = new byte[Defaults.DefaultCopyBufferSize];

                    int n;
                    while ((n = await body.ReadAsync(buffer, 0, buffer.Length, ct).ConfigureAwait(false)) > 0)
                    {
                        _writer!.WriteAt(buffer, 0, n, start + written - _rstart);
                        hash?.Update(buffer, 0, n);
                        written += n;
                        IncrWritten(n);
                    }

                    if (written != size)
                        throw new InvalidOperationException($"Downloaded size mismatch, expect {size}, got {written}");

                    var crc = hash != null ? BitConverter.ToUInt64(hash.Final(), 0) : 0UL;
                    return new DownloadedChunk(start, written, crc);
                }
            }

            private void ValidateChunkResponse(GetObjectResult result, long start, long size)
            {
                // ETag consistency: every part must come from the same object version.
                if (!string.IsNullOrEmpty(_etag) && !string.IsNullOrEmpty(result.ETag) &&
                    !string.Equals(result.ETag, _etag, StringComparison.Ordinal))
                {
                    throw new InvalidOperationException(
                        $"Source file is changed, expect etag:{_etag}, got etag:{result.ETag}");
                }

                var contentRange = result.ContentRange;
                if (string.IsNullOrEmpty(contentRange))
                {
                    // A ranged GET for an offset > 0 must return Content-Range.
                    if (start != 0)
                        throw new InvalidOperationException(
                            $"Range get fail, expect offset:{start}, got no Content-Range");
                    return;
                }

                if (!RangeParser.TryParseContentRange(contentRange!, out var respStart, out var respEnd, out var respTotal))
                    throw new InvalidOperationException($"Invalid Content-Range, {contentRange}");

                if (respStart != start)
                {
                    throw new InvalidOperationException(
                        $"Range get fail, expect offset:{start}, got offset:{respStart}");
                }

                if (respTotal != -1 && respTotal != _sizeInBytes)
                {
                    throw new InvalidOperationException(
                        $"Source file size changed, expect {_sizeInBytes}, got {respTotal}");
                }

                var respLength = respEnd - respStart + 1;
                if (respLength > size)
                {
                    throw new InvalidOperationException(
                        $"Range get fail, expect size <= {size}, got {respLength}");
                }
            }

            private void IncrWritten(long n)
            {
                lock (_progressGate)
                {
                    _written += n;
                    if (_request.ProgressFn != null && n > 0)
                        _request.ProgressFn(n, _written, _sizeInBytes);
                }
            }

            public void CloseWriter(FileStream? file, Exception? error)
            {
                file?.Dispose();
                _writer = null;

                if (error != null)
                {
                    if (_checkpoint == null)
                        TryDelete(_tempFilePath);
                    return;
                }

                if (!string.Equals(_tempFilePath, _filePath, StringComparison.Ordinal))
                {
                    if (File.Exists(_filePath) || IsSymbolicLink(_filePath))
                        File.Delete(_filePath);
                    File.Move(_tempFilePath, _filePath);
                }

                _checkpoint?.Remove();
                _checkpoint = null;
            }

            private static bool IsSymbolicLink(string path)
            {
                try
                {
                    return (File.GetAttributes(path) & FileAttributes.ReparsePoint) == FileAttributes.ReparsePoint;
                }
                catch (FileNotFoundException)
                {
                    return false;
                }
                catch (DirectoryNotFoundException)
                {
                    return false;
                }
            }

            private static void TryDelete(string path)
            {
                try { if (File.Exists(path) || IsSymbolicLink(path)) File.Delete(path); }
                catch { /* best effort */ }
            }

            private GetObjectRequest BuildGetRequest(long start, long size)
            {
                var r = new GetObjectRequest { Bucket = _request.Bucket, Key = _request.Key };
                foreach (var kv in _request.Headers)
                {
                    if (string.Equals(kv.Key, "Range", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(kv.Key, "x-oss-range-behavior", StringComparison.OrdinalIgnoreCase))
                        continue;
                    r.Headers[kv.Key] = kv.Value;
                }
                foreach (var kv in _request.Parameters)
                    r.Parameters[kv.Key] = kv.Value;

                var rangeStr = new HttpRange(start, size).Format();
                if (rangeStr != null)
                {
                    r.Range = rangeStr;
                    r.RangeBehavior = "standard";
                }
                return r;
            }

            private readonly struct DownloadedChunk
            {
                public DownloadedChunk(long start, long size, ulong crc64)
                {
                    Start = start;
                    Size = size;
                    Crc64 = crc64;
                }

                public long Start { get; }
                public long Size { get; }
                public ulong Crc64 { get; }
            }
        }
    }
}
