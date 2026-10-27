#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AlibabaCloud.OSS.V2;
using AlibabaCloud.OSS.V2.Credentials;
using AlibabaCloud.OSS.V2.Internal;
using AlibabaCloud.OSS.V2.Models;
using AlibabaCloud.OSS.V2.Transport;

namespace AlibabaCloud.OSS.V2.UnitTests.FileLike;

public class WriteOnlyFileTest
{
    private static Client MakeClient(MultipartUploadServerHandler handler)
    {
        var config = new Configuration
        {
            Region = "cn-hangzhou",
            CredentialsProvider = new AnonymousCredentialsProvider(),
            HttpTransport = new HttpTransport(handler)
        };
        return new Client(config, x =>
        {
            x.FeatureFlags &= ~(FeatureFlagsType.EnableCrc64CheckDownload | FeatureFlagsType.EnableCrc64CheckUpload);
        });
    }

    private static byte[] MakeData(int size)
    {
        var data = new byte[size];
        for (var i = 0; i < size; i++)
            data[i] = (byte)(i % 251);
        return data;
    }

    private static ulong Crc(byte[] data)
    {
        Crc64.InitECMA();
        return Crc64.Compute(data, 0, data.Length);
    }

    private static async Task WriteAllAsync(WriteOnlyFile f, byte[] data)
    {
        await f.WriteAsync(data, 0, data.Length);
    }

    [Fact]
    public void Scaffolding_DefaultsAndSeek()
    {
        var client = MakeClient(new MultipartUploadServerHandler());

        var f = client.NewWriteOnlyFile("bucket", "key", o =>
        {
            o.PartSize = 8;
            o.ParallelNum = 2;
        });

        // No network yet: nothing initiated.
        var cp = f.StatCheckpoint();
        Assert.True(string.IsNullOrEmpty(cp.UploadId));
        Assert.Equal(0, cp.Offset);
        Assert.Equal(8, cp.PartSize);
        Assert.Null(cp.LastError);

        Assert.Equal(0, f.Position);
        Assert.Equal(0, f.Seek(0, SeekOrigin.Current));

        // Illegal seeks.
        Assert.Throws<NotSupportedException>(() => f.Seek(1, SeekOrigin.Current));
        Assert.Throws<NotSupportedException>(() => f.Seek(0, SeekOrigin.Begin));

        // Defaults applied when unset.
        var f2 = client.NewWriteOnlyFile("bucket", "key");
        Assert.Equal(Defaults.DefaultUploadPartSize, f2.StatCheckpoint().PartSize);
    }

    [Fact]
    public async Task MultiPartComplete_AssemblesObject()
    {
        var handler = new MultipartUploadServerHandler();
        var client = MakeClient(handler);

        var f = client.NewWriteOnlyFile("bucket", "key", o =>
        {
            o.PartSize = 8;
            o.ParallelNum = 3;
        });

        var data = System.Text.Encoding.ASCII.GetBytes("0123456789ABCDEFGHIJ"); // 20 bytes -> parts 8,8,4
        await WriteAllAsync(f, data);
        await f.CloseAsync();

        Assert.Equal(data, handler.Completed);

        // Terminal: further calls rejected.
        await Assert.ThrowsAsync<ObjectDisposedException>(() => f.WriteAsync(new byte[] { 1 }, 0, 1));
    }

    [Fact]
    public async Task ContiguousOffset_TracksFullPrefix()
    {
        var handler = new MultipartUploadServerHandler();
        var client = MakeClient(handler);

        var f = client.NewWriteOnlyFile("bucket", "key", o =>
        {
            o.PartSize = 8;
            o.ParallelNum = 4;
        });

        var data = System.Text.Encoding.ASCII.GetBytes("AAAAAAAABBBBBBBBCCCCCCCC"); // 24 bytes -> 3 parts
        await WriteAllAsync(f, data);
        await f.CloseAsync();

        var cp = f.StatCheckpoint();
        Assert.Equal(24, cp.Offset);
        Assert.Equal(handler.UploadId, cp.UploadId);
        Assert.Equal(Crc(data), cp.Crc64);
    }

    [Fact]
    public async Task SmallObject_PutObjectFallback()
    {
        var handler = new MultipartUploadServerHandler();
        var client = MakeClient(handler);

        var f = client.NewWriteOnlyFile("bucket", "key", o =>
        {
            o.PartSize = 16;
            o.ParallelNum = 2;
        });

        var data = System.Text.Encoding.ASCII.GetBytes("hello");
        await WriteAllAsync(f, data); // < 1 part, never flushed
        await f.CloseAsync();

        Assert.Equal(data, handler.PutObjectBody);
        Assert.Null(handler.Completed); // no multipart
        Assert.True(string.IsNullOrEmpty(f.StatCheckpoint().UploadId)); // never initiated
        Assert.Equal(0, handler.InitiateCount);
    }

    [Fact]
    public async Task Flush_DoesNotAdvanceOffset()
    {
        var handler = new MultipartUploadServerHandler();
        var client = MakeClient(handler);

        var f = client.NewWriteOnlyFile("bucket", "key", o =>
        {
            o.PartSize = 8;
            o.ParallelNum = 2;
        });

        // Write 5 bytes (< 1 part), then flush.
        await WriteAllAsync(f, System.Text.Encoding.ASCII.GetBytes("HELLO"));
        await f.FlushAsync();

        // Flush uploaded part 1 as a short (5-byte) part; offset NOT advanced.
        Assert.Equal(new[] { 5 }, handler.PartSizes(1));
        Assert.Equal(0, f.StatCheckpoint().Offset);

        // Write cursor reflects flushed bytes.
        Assert.Equal(5, f.Seek(0, SeekOrigin.Current));

        // Continue writing to fill part 1 (needs 3 more bytes) plus part 2.
        await WriteAllAsync(f, System.Text.Encoding.ASCII.GetBytes("WORLD!!")); // 7 bytes
        await f.CloseAsync();

        // Part 1 re-uploaded as a full 8-byte version (overwrote the flush short part).
        Assert.Equal(new[] { 5, 8 }, handler.PartSizes(1));
        Assert.Equal(System.Text.Encoding.ASCII.GetBytes("HELLOWORLD!!"), handler.Completed);

        // After Close, part 1 is full and part 2 is the last part -> offset covers part 1.
        Assert.Equal(8, f.StatCheckpoint().Offset);
    }

    [Fact]
    public async Task AbortClose_DiscardsUpload()
    {
        var handler = new MultipartUploadServerHandler();
        var client = MakeClient(handler);

        var f = client.NewWriteOnlyFile("bucket", "key", o =>
        {
            o.PartSize = 8;
            o.ParallelNum = 2;
        });
        await WriteAllAsync(f, System.Text.Encoding.ASCII.GetBytes("AAAAAAAABBBB")); // seals part 1, part 2 partial

        await f.AbortCloseAsync();
        Assert.Equal(1, handler.AbortCount);
        Assert.Null(handler.Completed);

        // Idempotent: repeated AbortClose and a later Close are no-ops, writes still rejected.
        await f.AbortCloseAsync();
        await f.CloseAsync();
        Assert.Equal(1, handler.AbortCount);
        Assert.Null(handler.Completed);
        await Assert.ThrowsAsync<ObjectDisposedException>(() => f.WriteAsync(new byte[] { 1 }, 0, 1));
    }

    [Fact]
    public async Task Close_Idempotent()
    {
        var handler = new MultipartUploadServerHandler();
        var client = MakeClient(handler);

        var f = client.NewWriteOnlyFile("bucket", "key", o =>
        {
            o.PartSize = 8;
            o.ParallelNum = 2;
        });
        await WriteAllAsync(f, System.Text.Encoding.ASCII.GetBytes("AAAAAAAABBBB")); // seals part 1, part 2 partial

        await f.CloseAsync();
        Assert.Equal(System.Text.Encoding.ASCII.GetBytes("AAAAAAAABBBB"), handler.Completed);

        // A second Close is a no-op; AbortClose does not abort a committed object.
        await f.CloseAsync();
        await f.AbortCloseAsync();
        Assert.Equal(System.Text.Encoding.ASCII.GetBytes("AAAAAAAABBBB"), handler.Completed);
        Assert.Equal(0, handler.AbortCount);
        await Assert.ThrowsAsync<ObjectDisposedException>(() => f.WriteAsync(new byte[] { 1 }, 0, 1));
    }

    [Fact]
    public async Task StickyError_NoAutoAbort()
    {
        var handler = new MultipartUploadServerHandler { FailUploadPart = true };
        var client = MakeClient(handler);

        var f = client.NewWriteOnlyFile("bucket", "key", o =>
        {
            o.PartSize = 4;
            o.ParallelNum = 1;
        });

        // Write enough to seal at least one part and trigger the failing UploadPart.
        await WriteAllAsync(f, System.Text.Encoding.ASCII.GetBytes("AAAABBBBCCCC"));

        // Flush drains in-flight parts, so the sticky error surfaces.
        var flushError = await Assert.ThrowsAsync<IOException>(() => f.FlushAsync());
        Assert.Contains("AccessDenied", flushError.Message);
        Assert.NotNull(f.StatCheckpoint().LastError);

        // Close returns the sticky error and does NOT auto-abort (parts kept for resume).
        var closeError = await Assert.ThrowsAsync<IOException>(() => f.CloseAsync());
        Assert.Contains("AccessDenied", closeError.Message);
        Assert.Equal(0, handler.AbortCount);
        Assert.Equal(0, handler.CompleteCount);
    }

    [Fact]
    public async Task Resume_ValidCheckpoint()
    {
        var handler = new MultipartUploadServerHandler();
        // Simulate a prior interrupted session: two full 8-byte parts + a trailing 3-byte flush snapshot.
        handler.SeedPart(1, System.Text.Encoding.ASCII.GetBytes("AAAAAAAA"));
        handler.SeedPart(2, System.Text.Encoding.ASCII.GetBytes("BBBBBBBB"));
        handler.SeedPart(3, System.Text.Encoding.ASCII.GetBytes("CCC"));
        var client = MakeClient(handler);

        var prefix = System.Text.Encoding.ASCII.GetBytes("AAAAAAAABBBBBBBB");
        var cp = new WriteCheckpoint
        {
            UploadId = handler.UploadId,
            Offset = 16,
            Crc64 = Crc(prefix),
            PartSize = 8,
        };

        var f = await client.OpenWriteOnlyFileAsync("bucket", "key", cp);
        // Seeded from the full prefix [1,2]; trailing short part 3 is ignored.
        var got = f.StatCheckpoint();
        Assert.Equal(16, got.Offset);
        Assert.Equal(handler.UploadId, got.UploadId);

        // Replay from offset 16: write the real part-3 payload and finish.
        await WriteAllAsync(f, System.Text.Encoding.ASCII.GetBytes("CCCCCCCCDD")); // fills part 3 (8), part 4 (2)
        await f.CloseAsync();

        Assert.Equal(
            System.Text.Encoding.ASCII.GetBytes("AAAAAAAABBBBBBBBCCCCCCCCDD"),
            handler.Completed);
    }

    [Fact]
    public async Task Resume_InvalidMiddleShortPart_Refused()
    {
        var handler = new MultipartUploadServerHandler();
        handler.SeedPart(1, System.Text.Encoding.ASCII.GetBytes("AAAAAAAA"));
        handler.SeedPart(2, System.Text.Encoding.ASCII.GetBytes("BB")); // middle short part -> invalid
        handler.SeedPart(3, System.Text.Encoding.ASCII.GetBytes("CCCCCCCC"));
        var client = MakeClient(handler);

        var cp = new WriteCheckpoint { UploadId = handler.UploadId, Offset = 16, PartSize = 8 };

        await Assert.ThrowsAsync<CheckpointInvalidException>(
            () => client.OpenWriteOnlyFileAsync("bucket", "key", cp));
    }

    [Fact]
    public async Task Resume_OffsetMismatch_Refused()
    {
        var handler = new MultipartUploadServerHandler();
        // Server holds one full part (durable prefix = 8) ...
        handler.SeedPart(1, System.Text.Encoding.ASCII.GetBytes("AAAAAAAA"));
        var client = MakeClient(handler);

        // ... but the checkpoint claims 16 -> mismatch, refused.
        var cp = new WriteCheckpoint { UploadId = handler.UploadId, Offset = 16, PartSize = 8 };

        await Assert.ThrowsAsync<CheckpointInvalidException>(
            () => client.OpenWriteOnlyFileAsync("bucket", "key", cp));
    }

    [Fact]
    public async Task Resume_MissingPartSize_Refused()
    {
        var client = MakeClient(new MultipartUploadServerHandler());

        // A resume token carrying an UploadId but no PartSize is malformed.
        var cp = new WriteCheckpoint { UploadId = "upload-id-xyz", Offset = 16 };

        await Assert.ThrowsAsync<CheckpointInvalidException>(
            () => client.OpenWriteOnlyFileAsync("bucket", "key", cp));
    }

    [Fact]
    public async Task Resume_NoSuchUpload_Refused()
    {
        var handler = new MultipartUploadServerHandler { NoSuchUpload = true };
        var client = MakeClient(handler);

        var cp = new WriteCheckpoint { UploadId = "gone", Offset = 24, PartSize = 8 };

        // A vanished upload is refused, not silently restarted.
        await Assert.ThrowsAsync<CheckpointInvalidException>(
            () => client.OpenWriteOnlyFileAsync("bucket", "key", cp));
    }

    [Fact]
    public async Task CreateParameter_Multipart()
    {
        var handler = new MultipartUploadServerHandler();
        var client = MakeClient(handler);

        var f = client.NewWriteOnlyFile("bucket", "key", o =>
        {
            o.PartSize = 8;
            o.ParallelNum = 2;
            o.CreateParameter = new PutObjectRequest
            {
                ContentType = "text/plain",
                StorageClass = "IA",
                Metadata = new Dictionary<string, string> { ["foo"] = "bar" },
                Tagging = "a=b",
                Acl = "private", // no multipart equivalent -> must be dropped from Initiate
            };
        });

        await WriteAllAsync(f, System.Text.Encoding.ASCII.GetBytes("0123456789")); // seals part 1, triggers Initiate
        await f.CloseAsync();

        var hdr = handler.InitiateHeaders;
        Assert.NotNull(hdr); // multipart path was taken

        // Common object attributes mapped onto InitiateMultipartUpload.
        Assert.Equal("text/plain", hdr!["Content-Type"]);
        Assert.Equal("IA", hdr["x-oss-storage-class"]);
        Assert.Equal("bar", hdr["x-oss-meta-foo"]);
        Assert.Equal("a=b", hdr["x-oss-tagging"]);
        // Acl has no Initiate equivalent: it must NOT be on Initiate ...
        Assert.False(hdr.ContainsKey("x-oss-object-acl"));
        // ... but IS applied at CompleteMultipartUpload.
        var completeHdr = handler.CompleteHeaders;
        Assert.NotNull(completeHdr);
        Assert.Equal("private", completeHdr!["x-oss-object-acl"]);
    }

    [Fact]
    public async Task CreateParameter_SmallObject()
    {
        var handler = new MultipartUploadServerHandler();
        var client = MakeClient(handler);

        var f = client.NewWriteOnlyFile("bucket", "key", o =>
        {
            o.PartSize = 16;
            o.ParallelNum = 2;
            o.CreateParameter = new PutObjectRequest
            {
                ContentType = "application/json",
                Metadata = new Dictionary<string, string> { ["k"] = "v" },
                Acl = "public-read", // takes effect on the PutObject path
            };
        });

        await WriteAllAsync(f, System.Text.Encoding.ASCII.GetBytes("small")); // < 1 part -> single PutObject
        await f.CloseAsync();

        var hdr = handler.PutObjectHeaders;
        Assert.NotNull(hdr);
        Assert.Equal(System.Text.Encoding.ASCII.GetBytes("small"), handler.PutObjectBody);

        Assert.Equal("application/json", hdr!["Content-Type"]);
        Assert.Equal("v", hdr["x-oss-meta-k"]);
        // Acl does apply on the small-object PutObject path.
        Assert.Equal("public-read", hdr["x-oss-object-acl"]);
        // Never initiated a multipart upload.
        Assert.Null(handler.InitiateHeaders);
        Assert.True(string.IsNullOrEmpty(f.StatCheckpoint().UploadId));
    }

    [Fact]
    public async Task WriteFrom_WritesAllBytes()
    {
        var handler = new MultipartUploadServerHandler();
        var client = MakeClient(handler);

        var f = client.NewWriteOnlyFile("bucket", "key", o =>
        {
            o.PartSize = 8;
            o.ParallelNum = 2;
        });

        var data = MakeData(20); // parts 8, 8, 4
        using var src = new MemoryStream(data);
        var written = await f.WriteFromAsync(src);
        Assert.Equal(data.Length, written);
        Assert.Equal(data.Length, f.Seek(0, SeekOrigin.Current));

        await f.CloseAsync();
        Assert.Equal(data, handler.Completed);
    }

    [Fact]
    public async Task Close_Cancellation_StopsDrainWait()
    {
        var handler = new MultipartUploadServerHandler { UploadPartDelayMs = 3000 };
        var client = MakeClient(handler);

        var f = client.NewWriteOnlyFile("bucket", "key", o =>
        {
            o.PartSize = 4;
            o.ParallelNum = 1;
        });

        // Seal a part; its upload is held by the mock, so Close would block on the drain.
        await WriteAllAsync(f, System.Text.Encoding.ASCII.GetBytes("AAAA"));

        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(150));
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => f.CloseAsync(cts.Token));
        stopwatch.Stop();
        Assert.True(stopwatch.ElapsedMilliseconds < 2000,
            $"close waited {stopwatch.ElapsedMilliseconds}ms despite cancellation");
    }

    [Fact]
    public async Task Checkpoint_UnknownCrc_ReportedAsZero_AndResumable()
    {
        var handler = new MultipartUploadServerHandler();
        handler.OmitPartCrc.Add(2); // part 1 carries a CRC, part 2 does not
        var client = MakeClient(handler);

        var f = client.NewWriteOnlyFile("bucket", "key", o =>
        {
            o.PartSize = 8;
            o.ParallelNum = 2;
        });

        await WriteAllAsync(f, System.Text.Encoding.ASCII.GetBytes("AAAAAAAABBBBBBBB")); // two full parts
        await f.FlushAsync(); // drain, so the durable offset reaches 16

        var cp = f.StatCheckpoint();
        Assert.Equal(16, cp.Offset);
        // An unknown prefix CRC must be reported as 0, not as a stale CRC covering only part 1.
        Assert.Equal(0UL, cp.Crc64);

        // ListParts serves correct CRCs; a checkpoint that says "CRC unknown" must still resume.
        var resumed = await client.OpenWriteOnlyFileAsync("bucket", "key", cp);
        Assert.Equal(16, resumed.StatCheckpoint().Offset);
        await resumed.AbortCloseAsync();
    }

    [Fact]
    public async Task CreateParameter_SmallObject_PreservesCustomHeadersAndParameters()
    {
        var handler = new MultipartUploadServerHandler();
        var client = MakeClient(handler);

        var createParameter = new PutObjectRequest
        {
            ContentType = "text/plain",
            Metadata = new Dictionary<string, string> { ["k"] = "v" },
            Acl = "private",
            ContentLength = 99, // stale: the handle owns the body
        };
        createParameter.Headers["x-oss-custom-header"] = "custom-value";
        createParameter.Parameters["custom-param"] = "p1";

        var f = client.NewWriteOnlyFile("bucket", "key", o =>
        {
            o.PartSize = 16;
            o.CreateParameter = createParameter;
        });

        await WriteAllAsync(f, System.Text.Encoding.ASCII.GetBytes("small"));
        await f.CloseAsync();

        var hdr = handler.PutObjectHeaders;
        Assert.NotNull(hdr);
        // Well-known properties still map through ...
        Assert.Equal("text/plain", hdr!["Content-Type"]);
        Assert.Equal("private", hdr["x-oss-object-acl"]);
        Assert.Equal("v", hdr["x-oss-meta-k"]);
        // ... and custom headers/parameters from CreateParameter survive the copy.
        Assert.Equal("custom-value", hdr["x-oss-custom-header"]);
        Assert.Contains("custom-param=p1", handler.PutObjectQuery);
        // The stale Content-Length must not leak through.
        Assert.True(!hdr.TryGetValue("Content-Length", out var cl) || cl == "5", $"Content-Length={cl}");
    }

    [Fact]
    public async Task WriteFrom_StickyError_CheckedBeforeReadingSource()
    {
        var handler = new MultipartUploadServerHandler { FailUploadPart = true };
        var client = MakeClient(handler);

        var f = client.NewWriteOnlyFile("bucket", "key", o =>
        {
            o.PartSize = 4;
            o.ParallelNum = 1;
        });

        await WriteAllAsync(f, System.Text.Encoding.ASCII.GetBytes("AAAA")); // seals part 1 -> upload fails
        await Assert.ThrowsAsync<IOException>(() => f.FlushAsync()); // surface the sticky error

        // Empty source: must report the existing error instead of returning success.
        var empty = new MemoryStream(Array.Empty<byte>());
        var emptyError = await Assert.ThrowsAsync<IOException>(() => f.WriteFromAsync(empty));
        Assert.Contains("writefrom", emptyError.Message);
        Assert.Equal(0, empty.Position);

        // Non-empty source: must fail before consuming any data.
        var payload = new MemoryStream(System.Text.Encoding.ASCII.GetBytes("BBBBBB"));
        var payloadError = await Assert.ThrowsAsync<IOException>(() => f.WriteFromAsync(payload));
        Assert.Contains("writefrom", payloadError.Message);
        Assert.Equal(0, payload.Position);

        // The synchronous variant must reject as well.
        Assert.Throws<IOException>(() => f.WriteFrom(new MemoryStream(System.Text.Encoding.ASCII.GetBytes("CC"))));

        await Assert.ThrowsAsync<IOException>(() => f.CloseAsync());
    }

    private static object? PrivateField(object target, string name)
        => typeof(WriteOnlyFile)
            .GetField(name, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
            .GetValue(target);

    private static async Task<WriteOnlyFile> OpenWithPartInFlight(string key)
    {
        var handler = new MultipartUploadServerHandler { UploadPartDelayMs = 3000 };
        var client = MakeClient(handler);

        var f = client.NewWriteOnlyFile("bucket", key, o =>
        {
            o.PartSize = 4;
            o.ParallelNum = 1;
        });

        // Seal one part so the pool is alive and an upload is held by the mock.
        await WriteAllAsync(f, System.Text.Encoding.ASCII.GetBytes("AAAA"));
        Assert.NotNull(PrivateField(f, "_pool"));

        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(150));
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => f.CloseAsync(cts.Token));
        return f;
    }

    [Fact]
    public async Task Close_Cancellation_ReleasesBufferPool()
    {
        var f = await OpenWithPartInFlight("close-cancel");

        // The handle is already closed, so a later close/abort/dispose must not be the only way
        // to get rid of the pool: it has to be gone as soon as the canceled close returns.
        Assert.Null(PrivateField(f, "_pool"));
        await f.CloseAsync(); // no-op on a closed handle
        await f.AbortCloseAsync(); // no-op
        f.Dispose(); // must not throw
        Assert.Null(PrivateField(f, "_pool"));
    }

    [Fact]
    public async Task AbortClose_Cancellation_ReleasesBufferPool()
    {
        var handler = new MultipartUploadServerHandler { UploadPartDelayMs = 3000 };
        var client = MakeClient(handler);

        var f = client.NewWriteOnlyFile("bucket", "abort-cancel", o =>
        {
            o.PartSize = 4;
            o.ParallelNum = 1;
        });
        await WriteAllAsync(f, System.Text.Encoding.ASCII.GetBytes("AAAA"));
        Assert.NotNull(PrivateField(f, "_pool"));

        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(150));
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => f.AbortCloseAsync(cts.Token));

        Assert.Null(PrivateField(f, "_pool"));
        await f.AbortCloseAsync(); // no-op
        f.Dispose();
        Assert.Null(PrivateField(f, "_pool"));
    }

    [Fact]
    public async Task OperationCanceledException_Is_Not_Wrapped_As_IOException()
    {
        var handler = new MultipartUploadServerHandler { CompleteException = new OperationCanceledException() };
        var client = MakeClient(handler);

        var f = client.NewWriteOnlyFile("bucket", "key", o => { o.PartSize = 4; o.ParallelNum = 1; });
        await WriteAllAsync(f, System.Text.Encoding.ASCII.GetBytes("AAAA"));

        var ex = await Assert.ThrowsAnyAsync<OperationCanceledException>(() => f.CloseAsync());
        Assert.Null(ex.InnerException);
    }

    [Fact]
    public async Task RequestPayer_Header_Forwarded_To_Multipart_Requests()
    {
        var handler = new MultipartUploadServerHandler();
        var client = MakeClient(handler);

        var f = client.NewWriteOnlyFile("bucket", "key", o =>
        {
            o.PartSize = 4;
            o.ParallelNum = 1;
            o.CreateParameter = new PutObjectRequest
            {
                Headers = { ["x-oss-request-payer"] = "requester" }
            };
        });

        await WriteAllAsync(f, System.Text.Encoding.ASCII.GetBytes("AAAA"));
        await f.FlushAsync();
        Assert.Equal("requester", handler.InitiateHeaders!["x-oss-request-payer"]);
        Assert.Equal("requester", handler.UploadPartHeaders!["x-oss-request-payer"]);

        await f.CloseAsync();
        Assert.Equal("requester", handler.CompleteHeaders!["x-oss-request-payer"]);
    }

    [Fact]
    public async Task RequestPayer_Header_Forwarded_To_Abort()
    {
        var handler = new MultipartUploadServerHandler();
        var client = MakeClient(handler);

        var f = client.NewWriteOnlyFile("bucket", "key", o =>
        {
            o.PartSize = 4;
            o.ParallelNum = 1;
            o.CreateParameter = new PutObjectRequest
            {
                Headers = { ["x-oss-request-payer"] = "requester" }
            };
        });

        await WriteAllAsync(f, System.Text.Encoding.ASCII.GetBytes("AAAA"));
        await f.FlushAsync();
        await f.AbortCloseAsync();
        Assert.Equal("requester", handler.AbortHeaders!["x-oss-request-payer"]);
    }
}
