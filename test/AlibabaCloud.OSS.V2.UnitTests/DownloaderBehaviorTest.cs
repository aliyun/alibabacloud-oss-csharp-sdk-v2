using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using AlibabaCloud.OSS.V2;
using AlibabaCloud.OSS.V2.Models;
using AlibabaCloud.OSS.V2.UnitTests.Fakes;

namespace AlibabaCloud.OSS.V2.UnitTests;

public class DownloaderBehaviorTest
{
    private const string Bucket = "bucket";
    private const string Key = "key";

    private static FeatureFlagsType NoCrc =>
        FeatureFlagsType.CorrectClockSkew | FeatureFlagsType.AutoDetectMimeType;

    private static byte[] RandomBytes(int size)
    {
        var data = new byte[size];
        new Random(size + 7).NextBytes(data);
        return data;
    }

    private static string NewDestPath() =>
        Path.Combine(Path.GetTempPath(), "ossdl-" + Guid.NewGuid());

    private static void CleanUp(string path)
    {
        foreach (var p in new[] { path, path + ".temp" })
            if (File.Exists(p))
                File.Delete(p);
    }

    // --- Basic downloads ---

    [Fact]
    public async Task TestDownloadSinglePart()
    {
        var fake = new FakeTransferClient();
        var data = RandomBytes(4096);
        fake.SeedObject(Bucket, Key, data);
        var downloader = new Downloader(fake, o => o.PartSize = 5 * 1024 * 1024);
        var path = NewDestPath();
        try
        {
            var req = new GetObjectRequest { Bucket = Bucket, Key = Key };
            var result = await downloader.DownloadFileAsync(req, path);

            Assert.Equal(data.Length, result.Written);
            Assert.Equal(data, File.ReadAllBytes(path));
            Assert.Equal(1, fake.HeadObjectCount);
            Assert.Equal(1, fake.GetObjectCount);
        }
        finally
        {
            CleanUp(path);
        }
    }

    [Fact]
    public async Task TestDownloadMultiPartParallel()
    {
        var fake = new FakeTransferClient();
        var data = RandomBytes(500 * 1024);
        fake.SeedObject(Bucket, Key, data);
        var downloader = new Downloader(fake, o => { o.PartSize = 100 * 1024; o.ParallelNum = 4; });
        var path = NewDestPath();
        try
        {
            var req = new GetObjectRequest { Bucket = Bucket, Key = Key };
            var result = await downloader.DownloadFileAsync(req, path);

            Assert.Equal(data.Length, result.Written);
            Assert.Equal(data, File.ReadAllBytes(path));
            Assert.Equal(5, fake.GetObjectCount);
        }
        finally
        {
            CleanUp(path);
        }
    }

    [Fact]
    public async Task TestDownloadEmptyObject()
    {
        var fake = new FakeTransferClient();
        fake.SeedObject(Bucket, Key, Array.Empty<byte>());
        var downloader = new Downloader(fake);
        var path = NewDestPath();
        try
        {
            var req = new GetObjectRequest { Bucket = Bucket, Key = Key };
            var result = await downloader.DownloadFileAsync(req, path);

            Assert.Equal(0, result.Written);
            Assert.True(File.Exists(path));
            Assert.Empty(File.ReadAllBytes(path));
        }
        finally
        {
            CleanUp(path);
        }
    }

    [Fact]
    public async Task TestDownloadWithRange()
    {
        var fake = new FakeTransferClient();
        var data = RandomBytes(500 * 1024);
        fake.SeedObject(Bucket, Key, data);
        var downloader = new Downloader(fake, o => o.PartSize = 100 * 1024);
        var path = NewDestPath();
        try
        {
            var req = new GetObjectRequest { Bucket = Bucket, Key = Key, Range = "bytes=100-199" };
            var result = await downloader.DownloadFileAsync(req, path);

            Assert.Equal(100, result.Written);
            Assert.Equal(data.Skip(100).Take(100).ToArray(), File.ReadAllBytes(path));
        }
        finally
        {
            CleanUp(path);
        }
    }

    [Fact]
    public async Task TestDownloadWithSuffixRange()
    {
        var fake = new FakeTransferClient();
        var data = RandomBytes(500 * 1024);
        fake.SeedObject(Bucket, Key, data);
        var downloader = new Downloader(fake, o => o.PartSize = 100 * 1024);
        var path = NewDestPath();
        try
        {
            var req = new GetObjectRequest { Bucket = Bucket, Key = Key, Range = "bytes=-100" };
            var result = await downloader.DownloadFileAsync(req, path);

            Assert.Equal(100, result.Written);
            Assert.Equal(data.Skip(data.Length - 100).Take(100).ToArray(), File.ReadAllBytes(path));
        }
        finally
        {
            CleanUp(path);
        }
    }

    [Fact]
    public async Task TestDownloadInvalidReversedRangeThrows()
    {
        var fake = new FakeTransferClient();
        fake.SeedObject(Bucket, Key, RandomBytes(1024));
        var downloader = new Downloader(fake);
        var path = NewDestPath();
        try
        {
            var req = new GetObjectRequest { Bucket = Bucket, Key = Key, Range = "bytes=100-50" };
            await Assert.ThrowsAsync<ArgumentException>(() => downloader.DownloadFileAsync(req, path));
        }
        finally
        {
            CleanUp(path);
        }
    }

    [Fact]
    public async Task TestDownloadInvalidOverflowRangeThrows()
    {
        var fake = new FakeTransferClient();
        fake.SeedObject(Bucket, Key, RandomBytes(1024));
        var downloader = new Downloader(fake);
        var path = NewDestPath();
        try
        {
            var req = new GetObjectRequest { Bucket = Bucket, Key = Key, Range = $"bytes=0-{long.MaxValue}" };
            await Assert.ThrowsAsync<ArgumentException>(() => downloader.DownloadFileAsync(req, path));
        }
        finally
        {
            CleanUp(path);
        }
    }

    [Fact]
    public async Task TestDownloadEmptyObjectForwardsConditionHeaders()
    {
        var fake = new FakeTransferClient();
        fake.SeedObject(Bucket, Key, Array.Empty<byte>());
        var downloader = new Downloader(fake);
        var path = NewDestPath();
        try
        {
            var req = new GetObjectRequest
            {
                Bucket = Bucket,
                Key = Key,
                IfMatch = "\"etag\"",
                IfNoneMatch = "\"none\"",
                IfModifiedSince = "Mon, 01 Jan 2024 00:00:00 GMT",
                IfUnmodifiedSince = "Mon, 01 Jan 2024 00:00:00 GMT"
            };
            var result = await downloader.DownloadFileAsync(req, path);

            Assert.Equal(0, result.Written);
            Assert.Single(fake.HeadObjectRequests);
            Assert.Equal("\"etag\"", fake.HeadObjectRequests[0].IfMatch);
            Assert.Equal("\"none\"", fake.HeadObjectRequests[0].IfNoneMatch);
            Assert.Equal("Mon, 01 Jan 2024 00:00:00 GMT", fake.HeadObjectRequests[0].IfModifiedSince);
            Assert.Equal("Mon, 01 Jan 2024 00:00:00 GMT", fake.HeadObjectRequests[0].IfUnmodifiedSince);
            Assert.Equal(0, fake.GetObjectCount);
        }
        finally
        {
            CleanUp(path);
        }
    }

    [Fact]
    public async Task TestDownloadForwardsConditionHeadersToGetObject()
    {
        var fake = new FakeTransferClient();
        var data = RandomBytes(300 * 1024);
        fake.SeedObject(Bucket, Key, data);
        var downloader = new Downloader(fake, o => o.PartSize = 100 * 1024);
        var path = NewDestPath();
        try
        {
            var req = new GetObjectRequest
            {
                Bucket = Bucket,
                Key = Key,
                IfMatch = "\"etag\"",
                IfNoneMatch = "\"none\"",
                IfModifiedSince = "Mon, 01 Jan 2024 00:00:00 GMT",
                IfUnmodifiedSince = "Mon, 01 Jan 2024 00:00:00 GMT"
            };
            await downloader.DownloadFileAsync(req, path);

            Assert.Single(fake.HeadObjectRequests);
            Assert.Equal("\"etag\"", fake.HeadObjectRequests[0].IfMatch);

            Assert.True(fake.GetObjectRequests.Count >= 1);
            foreach (var getReq in fake.GetObjectRequests)
            {
                Assert.Equal("\"etag\"", getReq.IfMatch);
                Assert.Equal("\"none\"", getReq.IfNoneMatch);
                Assert.Equal("Mon, 01 Jan 2024 00:00:00 GMT", getReq.IfModifiedSince);
                Assert.Equal("Mon, 01 Jan 2024 00:00:00 GMT", getReq.IfUnmodifiedSince);
            }
        }
        finally
        {
            CleanUp(path);
        }
    }

    [Fact]
    public async Task TestDownloadEtagMismatchThrows()
    {
        var fake = new FakeTransferClient();
        var data = RandomBytes(300 * 1024);
        fake.SeedObject(Bucket, Key, data);
        var mutated = false;
        fake.MutateGetObjectResult = (_, result) =>
        {
            if (!mutated)
            {
                mutated = true;
                result.Headers["ETag"] = "\"other\"";
            }
        };
        var downloader = new Downloader(fake, o => { o.PartSize = 100 * 1024; o.ParallelNum = 1; });
        var path = NewDestPath();
        try
        {
            var req = new GetObjectRequest { Bucket = Bucket, Key = Key };
            await Assert.ThrowsAsync<InvalidOperationException>(() => downloader.DownloadFileAsync(req, path));
        }
        finally
        {
            CleanUp(path);
        }
    }

    [Fact]
    public async Task TestDownloadContentRangeMismatchThrows()
    {
        var fake = new FakeTransferClient();
        var data = RandomBytes(300 * 1024);
        fake.SeedObject(Bucket, Key, data);
        var mutated = false;
        fake.MutateGetObjectResult = (_, result) =>
        {
            if (!mutated && result.StatusCode == 206)
            {
                mutated = true;
                // Report a Content-Range whose start offset does not match the request.
                result.Headers["Content-Range"] = $"bytes 1-1/{data.Length}";
            }
        };
        var downloader = new Downloader(fake, o => { o.PartSize = 100 * 1024; o.ParallelNum = 1; });
        var path = NewDestPath();
        try
        {
            var req = new GetObjectRequest { Bucket = Bucket, Key = Key };
            await Assert.ThrowsAsync<InvalidOperationException>(() => downloader.DownloadFileAsync(req, path));
        }
        finally
        {
            CleanUp(path);
        }
    }

    [Fact]
    public async Task TestDownloadTruncatedResponseThrows()
    {
        var fake = new FakeTransferClient();
        var data = RandomBytes(300 * 1024);
        fake.SeedObject(Bucket, Key, data);
        fake.MutateGetObjectResult = (_, result) =>
        {
            // Return a body that is shorter than the requested range.
            result.InnerBody = new MemoryStream(new byte[10], writable: false);
        };
        var downloader = new Downloader(fake, o => { o.PartSize = 100 * 1024; o.ParallelNum = 1; });
        var path = NewDestPath();
        try
        {
            var req = new GetObjectRequest { Bucket = Bucket, Key = Key };
            await Assert.ThrowsAsync<InvalidOperationException>(() => downloader.DownloadFileAsync(req, path));
        }
        finally
        {
            CleanUp(path);
        }
    }

    // --- Temp file behavior ---

    [Fact]
    public async Task TestDownloadUseTempFileRenamesOnSuccess()
    {
        var fake = new FakeTransferClient();
        var data = RandomBytes(2048);
        fake.SeedObject(Bucket, Key, data);
        var downloader = new Downloader(fake); // UseTempFile default true
        var path = NewDestPath();
        try
        {
            var req = new GetObjectRequest { Bucket = Bucket, Key = Key };
            await downloader.DownloadFileAsync(req, path);

            Assert.True(File.Exists(path));
            Assert.False(File.Exists(path + ".temp"));
        }
        finally
        {
            CleanUp(path);
        }
    }

    [Fact]
    public async Task TestDownloadNoTempFileWritesDirectly()
    {
        var fake = new FakeTransferClient();
        var data = RandomBytes(2048);
        fake.SeedObject(Bucket, Key, data);
        var downloader = new Downloader(fake, o => o.UseTempFile = false);
        var path = NewDestPath();
        try
        {
            var req = new GetObjectRequest { Bucket = Bucket, Key = Key };
            await downloader.DownloadFileAsync(req, path);

            Assert.True(File.Exists(path));
            Assert.Equal(data, File.ReadAllBytes(path));
        }
        finally
        {
            CleanUp(path);
        }
    }

    // --- CRC64 ---

    [Fact]
    public async Task TestDownloadCrcMatches()
    {
        var fake = new FakeTransferClient(); // CRC enabled
        var data = RandomBytes(300 * 1024);
        fake.SeedObject(Bucket, Key, data);
        var downloader = new Downloader(fake, o => o.PartSize = 100 * 1024);
        var path = NewDestPath();
        try
        {
            var req = new GetObjectRequest { Bucket = Bucket, Key = Key };
            var result = await downloader.DownloadFileAsync(req, path);
            Assert.Equal(data.Length, result.Written);
        }
        finally
        {
            CleanUp(path);
        }
    }

    [Fact]
    public async Task TestDownloadCrcMismatchThrows()
    {
        var fake = new FakeTransferClient { HeadCrcOverride = "999999" };
        var data = RandomBytes(300 * 1024);
        fake.SeedObject(Bucket, Key, data);
        var downloader = new Downloader(fake, o => o.PartSize = 100 * 1024);
        var path = NewDestPath();
        try
        {
            var req = new GetObjectRequest { Bucket = Bucket, Key = Key };
            await Assert.ThrowsAsync<InconsistentException>(() =>
                downloader.DownloadFileAsync(req, path));
        }
        finally
        {
            CleanUp(path);
        }
    }

    [Fact]
    public async Task TestDownloadCrcSkippedForRange()
    {
        var fake = new FakeTransferClient { HeadCrcOverride = "999999" };
        var data = RandomBytes(300 * 1024);
        fake.SeedObject(Bucket, Key, data);
        var downloader = new Downloader(fake, o => o.PartSize = 100 * 1024);
        var path = NewDestPath();
        try
        {
            var req = new GetObjectRequest { Bucket = Bucket, Key = Key, Range = "bytes=0-99" };
            var result = await downloader.DownloadFileAsync(req, path); // no CRC check for ranged
            Assert.Equal(100, result.Written);
        }
        finally
        {
            CleanUp(path);
        }
    }

    [Fact]
    public async Task TestDownloadCrcDisabledSkipsCheck()
    {
        var fake = new FakeTransferClient(NoCrc) { HeadCrcOverride = "999999" };
        var data = RandomBytes(300 * 1024);
        fake.SeedObject(Bucket, Key, data);
        var downloader = new Downloader(fake, o => o.PartSize = 100 * 1024);
        var path = NewDestPath();
        try
        {
            var req = new GetObjectRequest { Bucket = Bucket, Key = Key };
            var result = await downloader.DownloadFileAsync(req, path);
            Assert.Equal(data.Length, result.Written);
        }
        finally
        {
            CleanUp(path);
        }
    }

    // --- Progress ---

    [Fact]
    public async Task TestDownloadProgress()
    {
        var fake = new FakeTransferClient();
        var data = RandomBytes(350 * 1024);
        fake.SeedObject(Bucket, Key, data);
        var downloader = new Downloader(fake, o => { o.PartSize = 100 * 1024; o.ParallelNum = 1; });
        var path = NewDestPath();
        try
        {
            long lastTransferred = 0;
            long total = 0;
            var req = new GetObjectRequest
            {
                Bucket = Bucket,
                Key = Key,
                ProgressFn = (increment, transferred, t) => { lastTransferred = transferred; total = t; }
            };
            await downloader.DownloadFileAsync(req, path);

            Assert.Equal(data.Length, lastTransferred);
            Assert.Equal(data.Length, total);
        }
        finally
        {
            CleanUp(path);
        }
    }

    // --- Failure handling ---

    [Fact]
    public async Task TestDownloadHeadObjectFailure()
    {
        var fake = new FakeTransferClient { OnHeadObject = _ => new InvalidOperationException("boom") };
        var downloader = new Downloader(fake);
        var path = NewDestPath();
        try
        {
            var req = new GetObjectRequest { Bucket = Bucket, Key = Key };
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                downloader.DownloadFileAsync(req, path));
        }
        finally
        {
            CleanUp(path);
        }
    }

    [Fact]
    public async Task TestDownloadGetObjectFailureDeletesTempFile()
    {
        var fake = new FakeTransferClient { OnGetObject = _ => new InvalidOperationException("boom") };
        var data = RandomBytes(300 * 1024);
        fake.SeedObject(Bucket, Key, data);
        var downloader = new Downloader(fake, o => o.PartSize = 100 * 1024);
        var path = NewDestPath();
        try
        {
            var req = new GetObjectRequest { Bucket = Bucket, Key = Key };
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                downloader.DownloadFileAsync(req, path));
            Assert.False(File.Exists(path + ".temp"));
            Assert.False(File.Exists(path));
        }
        finally
        {
            CleanUp(path);
        }
    }

    [Fact]
    public async Task TestDownloadCancellationWaitsForStartedTasks()
    {
        var fake = new FakeTransferClient();
        var data = RandomBytes(300 * 1024);
        fake.SeedObject(Bucket, Key, data);
        var downloader = new Downloader(fake, o =>
        {
            o.PartSize = 100 * 1024;
            o.ParallelNum = 1;
        });

        var entered = new TaskCompletionSource<bool>();
        var release = new TaskCompletionSource<bool>();
        fake.GetObjectHook = _ =>
        {
            entered.SetResult(true);
            return release.Task;
        };

        var path = NewDestPath();
        try
        {
            var cts = new CancellationTokenSource();
            var req = new GetObjectRequest { Bucket = Bucket, Key = Key };
            var downloadTask = downloader.DownloadFileAsync(req, path, null, cts.Token);

            // Wait until the first (and only concurrent) GetObject has started.
            await entered.Task;

            // Cancel while the scheduler is waiting for the next semaphore slot.
            cts.Cancel();

            // Release the in-flight chunk so it can finish.
            release.SetResult(true);

            var ex = await Record.ExceptionAsync(() => downloadTask);
            Assert.IsAssignableFrom<OperationCanceledException>(ex);
            // At least the in-flight chunk must have completed before the call returned.
            Assert.True(fake.GetObjectCount >= 1);
        }
        finally
        {
            CleanUp(path);
        }
    }

    // --- Checkpoint ---

    [Fact]
    public async Task TestDownloadWithCheckpointRemovedOnSuccess()
    {
        var fake = new FakeTransferClient();
        var data = RandomBytes(300 * 1024);
        fake.SeedObject(Bucket, Key, data);
        var cpDir = Path.Combine(Path.GetTempPath(), "ossdlcp-" + Guid.NewGuid());
        Directory.CreateDirectory(cpDir);
        var downloader = new Downloader(fake, o =>
        {
            o.PartSize = 100 * 1024;
            o.EnableCheckpoint = true;
            o.CheckpointDir = cpDir;
        });
        var path = NewDestPath();
        try
        {
            var req = new GetObjectRequest { Bucket = Bucket, Key = Key };
            var result = await downloader.DownloadFileAsync(req, path);

            Assert.Equal(data.Length, result.Written);
            Assert.Equal(data, File.ReadAllBytes(path));
            Assert.Empty(Directory.GetFiles(cpDir, "*.dcp"));
        }
        finally
        {
            CleanUp(path);
            Directory.Delete(cpDir, true);
        }
    }

    [Fact]
    public async Task TestDownloadCheckpointResume()
    {
        var fake = new FakeTransferClient();
        var data = RandomBytes(350 * 1024); // 4 parts at 100 KiB
        fake.SeedObject(Bucket, Key, data);
        var cpDir = Path.Combine(Path.GetTempPath(), "ossdlcp-" + Guid.NewGuid());
        Directory.CreateDirectory(cpDir);
        var path = NewDestPath();
        try
        {
            // First pass: cancel midway so a checkpoint and temp file remain.
            var cts = new CancellationTokenSource();
            long transferred = 0;
            var downloader = new Downloader(fake, o =>
            {
                o.PartSize = 100 * 1024;
                o.ParallelNum = 1;
                o.EnableCheckpoint = true;
                o.CheckpointDir = cpDir;
            });

            var req = new GetObjectRequest
            {
                Bucket = Bucket,
                Key = Key,
                ProgressFn = (increment, current, total) =>
                {
                    if (Interlocked.Add(ref transferred, increment) >= 150 * 1024)
                        cts.Cancel();
                }
            };

            await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
                downloader.DownloadFileAsync(req, path, null, cts.Token));

            Assert.NotEmpty(Directory.GetFiles(cpDir, "*.dcp"));
            Assert.True(File.Exists(path + ".temp")); // temp file kept for resume

            // Second pass: resume to completion.
            var req2 = new GetObjectRequest { Bucket = Bucket, Key = Key };
            var result = await downloader.DownloadFileAsync(req2, path);

            Assert.Equal(data.Length, result.Written);
            Assert.Equal(data, File.ReadAllBytes(path));
            Assert.Empty(Directory.GetFiles(cpDir, "*.dcp"));
        }
        finally
        {
            CleanUp(path);
            Directory.Delete(cpDir, true);
        }
    }

    [Fact]
    public async Task TestDownloadCheckpointResumeVerifyData()
    {
        var fake = new FakeTransferClient();
        var data = RandomBytes(350 * 1024);
        fake.SeedObject(Bucket, Key, data);
        var cpDir = Path.Combine(Path.GetTempPath(), "ossdlvd-" + Guid.NewGuid());
        Directory.CreateDirectory(cpDir);
        var path = NewDestPath();
        try
        {
            var cts = new CancellationTokenSource();
            long transferred = 0;
            var downloader = new Downloader(fake, o =>
            {
                o.PartSize = 100 * 1024;
                o.ParallelNum = 1;
                o.EnableCheckpoint = true;
                o.VerifyData = true;
                o.CheckpointDir = cpDir;
            });

            var req = new GetObjectRequest
            {
                Bucket = Bucket,
                Key = Key,
                ProgressFn = (increment, current, total) =>
                {
                    if (Interlocked.Add(ref transferred, increment) >= 150 * 1024)
                        cts.Cancel();
                }
            };

            await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
                downloader.DownloadFileAsync(req, path, null, cts.Token));

            Assert.NotEmpty(Directory.GetFiles(cpDir, "*.dcp"));

            // Resume: the checkpoint verifies the already-downloaded bytes before continuing.
            var req2 = new GetObjectRequest { Bucket = Bucket, Key = Key };
            var result = await downloader.DownloadFileAsync(req2, path);

            Assert.Equal(data.Length, result.Written);
            Assert.Equal(data, File.ReadAllBytes(path));
            Assert.Empty(Directory.GetFiles(cpDir, "*.dcp"));
        }
        finally
        {
            CleanUp(path);
            Directory.Delete(cpDir, true);
        }
    }

    // --- Regression tests for recent fixes ---

    [Fact]
    public async Task TestDownloadRangeWithHugeEndPositionNoOverflow()
    {
        var fake = new FakeTransferClient();
        var data = RandomBytes(500 * 1024);
        fake.SeedObject(Bucket, Key, data);
        var downloader = new Downloader(fake, o => o.PartSize = 100 * 1024);
        var path = NewDestPath();
        try
        {
            // A non-zero start with end == long.MaxValue used to overflow _pos + hr.Count.
            var req = new GetObjectRequest { Bucket = Bucket, Key = Key, Range = $"bytes=100-{long.MaxValue}" };
            var result = await downloader.DownloadFileAsync(req, path);

            Assert.Equal(data.Length - 100, result.Written);
            Assert.Equal(data.Skip(100).ToArray(), File.ReadAllBytes(path));
        }
        finally
        {
            CleanUp(path);
        }
    }

    [Fact]
    public async Task TestDownloadValidationFailureDisposesResponseStream()
    {
        var fake = new FakeTransferClient();
        var data = RandomBytes(300 * 1024);
        fake.SeedObject(Bucket, Key, data);

        TrackingStream tracking = null;
        fake.MutateGetObjectResult = (_, result) =>
        {
            if (result.StatusCode != 206)
                return;

            tracking = new TrackingStream(new byte[100 * 1024]);
            result.InnerBody = tracking;
            result.Headers["ETag"] = "\"other\"";
        };

        var downloader = new Downloader(fake, o => { o.PartSize = 100 * 1024; o.ParallelNum = 1; });
        var path = NewDestPath();
        try
        {
            var req = new GetObjectRequest { Bucket = Bucket, Key = Key };
            await Assert.ThrowsAsync<InvalidOperationException>(() => downloader.DownloadFileAsync(req, path));

            Assert.NotNull(tracking);
            Assert.True(tracking.Disposed, "The response stream must be disposed when validation fails.");
        }
        finally
        {
            CleanUp(path);
        }
    }

    [Fact]
    public async Task TestDownloadSuffixRangeCheckpointResume()
    {
        var fake = new FakeTransferClient();
        var data = RandomBytes(500 * 1024);
        fake.SeedObject(Bucket, Key, data);
        var cpDir = Path.Combine(Path.GetTempPath(), "ossdl-suffix-" + Guid.NewGuid());
        Directory.CreateDirectory(cpDir);
        var path = NewDestPath();
        try
        {
            var cts = new CancellationTokenSource();
            long transferred = 0;
            var downloader = new Downloader(fake, o =>
            {
                o.PartSize = 100;
                o.ParallelNum = 1;
                o.EnableCheckpoint = true;
                o.CheckpointDir = cpDir;
            });

            var req = new GetObjectRequest
            {
                Bucket = Bucket,
                Key = Key,
                Range = "bytes=-200",
                ProgressFn = (increment, current, total) =>
                {
                    if (Interlocked.Add(ref transferred, increment) >= 100)
                        cts.Cancel();
                }
            };

            await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
                downloader.DownloadFileAsync(req, path, null, cts.Token));

            Assert.NotEmpty(Directory.GetFiles(cpDir, "*.dcp"));

            var req2 = new GetObjectRequest { Bucket = Bucket, Key = Key, Range = "bytes=-200" };
            var result = await downloader.DownloadFileAsync(req2, path);

            Assert.Equal(200, result.Written);
            Assert.Equal(data.Skip(data.Length - 200).Take(200).ToArray(), File.ReadAllBytes(path));
            Assert.Empty(Directory.GetFiles(cpDir, "*.dcp"));
        }
        finally
        {
            CleanUp(path);
            Directory.Delete(cpDir, true);
        }
    }

    private sealed class TrackingStream : MemoryStream
    {
        public TrackingStream(byte[] data) : base(data, writable: false) { }

        public bool Disposed { get; private set; }

        protected override void Dispose(bool disposing)
        {
            Disposed = true;
            base.Dispose(disposing);
        }
    }

    // --- Symbolic link safety ---

    [Fact]
    public async Task TestDownloadRemovesTempSymbolicLink()
    {
        var target = Path.Combine(Path.GetTempPath(), "ossdl-target-" + Guid.NewGuid());
        var path = NewDestPath();
        var tempPath = path + ".temp";
        File.WriteAllText(target, "original");
        if (!TryCreateFileSymlink(target, tempPath))
        {
            File.Delete(target);
            return; // symbolic links are not supported on this platform/account
        }

        var fake = new FakeTransferClient();
        var data = RandomBytes(4096);
        fake.SeedObject(Bucket, Key, data);
        var downloader = new Downloader(fake);
        try
        {
            var req = new GetObjectRequest { Bucket = Bucket, Key = Key };
            var result = await downloader.DownloadFileAsync(req, path);

            Assert.Equal(data.Length, result.Written);
            Assert.Equal(data, File.ReadAllBytes(path));
            Assert.Equal("original", File.ReadAllText(target)); // link target untouched
            Assert.False(IsSymbolicLink(tempPath));
        }
        finally
        {
            CleanUp(path);
            TryDelete(target);
        }
    }

    [Fact]
    public async Task TestDownloadReplacesFinalSymbolicLink()
    {
        var target = Path.Combine(Path.GetTempPath(), "ossdl-target-" + Guid.NewGuid());
        var path = NewDestPath();
        File.WriteAllText(target, "original");
        if (!TryCreateFileSymlink(target, path))
        {
            File.Delete(target);
            return; // symbolic links are not supported on this platform/account
        }

        var fake = new FakeTransferClient();
        var data = RandomBytes(4096);
        fake.SeedObject(Bucket, Key, data);
        var downloader = new Downloader(fake);
        try
        {
            var req = new GetObjectRequest { Bucket = Bucket, Key = Key };
            var result = await downloader.DownloadFileAsync(req, path);

            Assert.Equal(data.Length, result.Written);
            Assert.Equal(data, File.ReadAllBytes(path));
            Assert.Equal("original", File.ReadAllText(target)); // link target untouched
            Assert.False(IsSymbolicLink(path));
        }
        finally
        {
            CleanUp(path);
            TryDelete(target);
        }
    }

    [Fact]
    public async Task TestDownloadFailureRemovesTempSymbolicLink()
    {
        var target = Path.Combine(Path.GetTempPath(), "ossdl-target-" + Guid.NewGuid());
        var path = NewDestPath();
        var tempPath = path + ".temp";
        File.WriteAllText(target, "original");
        if (!TryCreateFileSymlink(target, tempPath))
        {
            File.Delete(target);
            return;
        }

        var fake = new FakeTransferClient { OnGetObject = _ => new InvalidOperationException("boom") };
        fake.SeedObject(Bucket, Key, RandomBytes(4096));
        var downloader = new Downloader(fake);
        try
        {
            var req = new GetObjectRequest { Bucket = Bucket, Key = Key };
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                downloader.DownloadFileAsync(req, path));

            Assert.False(IsSymbolicLink(tempPath));
            Assert.Equal("original", File.ReadAllText(target));
        }
        finally
        {
            CleanUp(path);
            TryDelete(target);
        }
    }

    private static bool TryCreateFileSymlink(string targetPath, string linkPath)
    {
        try
        {
#if NET6_0_OR_GREATER
            File.CreateSymbolicLink(linkPath, targetPath);
#else
            if (CreateSymbolicLink(linkPath, targetPath, 0) == 0)
                return false;
#endif
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static bool IsSymbolicLink(string path)
    {
        try
        {
            return (File.GetAttributes(path) & FileAttributes.ReparsePoint) == FileAttributes.ReparsePoint;
        }
        catch
        {
            return false;
        }
    }

    private static void TryDelete(string path)
    {
        try
        {
            if (File.Exists(path) || IsSymbolicLink(path))
                File.Delete(path);
        }
        catch { /* best effort */ }
    }

#if !NET6_0_OR_GREATER
    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern byte CreateSymbolicLink(string lpSymlinkFileName, string lpTargetFileName, int dwFlags);
#endif
}
