using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AlibabaCloud.OSS.V2;
using AlibabaCloud.OSS.V2.Models;
using AlibabaCloud.OSS.V2.UnitTests.Fakes;

namespace AlibabaCloud.OSS.V2.UnitTests;

public class UploaderBehaviorTest
{
    private const string Bucket = "bucket";
    private const string Key = "key";

    private static FeatureFlagsType NoCrc =>
        FeatureFlagsType.CorrectClockSkew | FeatureFlagsType.AutoDetectMimeType;

    private static byte[] RandomBytes(int size)
    {
        var data = new byte[size];
        new Random(size).NextBytes(data);
        return data;
    }

    private static string NewTempFile(byte[] data)
    {
        var path = Path.Combine(Path.GetTempPath(), "ossup-" + Guid.NewGuid());
        File.WriteAllBytes(path, data);
        return path;
    }

    // --- Single-part uploads ---

    [Fact]
    public async Task TestUploadStreamSinglePart()
    {
        var fake = new FakeTransferClient();
        var uploader = new Uploader(fake);
        var data = RandomBytes(1024);

        var req = new PutObjectRequest { Bucket = Bucket, Key = Key };
        var result = await uploader.UploadStreamAsync(req, new MemoryStream(data), o => o.PartSize = 5 * 1024 * 1024);

        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.ETag);
        Assert.Null(result.UploadId);
        Assert.Equal(1, fake.PutObjectCount);
        Assert.Equal(0, fake.InitiateCount);
        Assert.Equal(data, fake.GetStored(Bucket, Key));
    }

    [Fact]
    public async Task TestUploadFileSinglePart()
    {
        var fake = new FakeTransferClient();
        var uploader = new Uploader(fake);
        var data = RandomBytes(2048);
        var path = NewTempFile(data);
        try
        {
            var req = new PutObjectRequest { Bucket = Bucket, Key = Key };
            var result = await uploader.UploadFileAsync(req, path, o => o.PartSize = 5 * 1024 * 1024);

            Assert.Equal(200, result.StatusCode);
            Assert.Equal(1, fake.PutObjectCount);
            Assert.Equal(data, fake.GetStored(Bucket, Key));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task TestUploadEmptyStreamSinglePart()
    {
        var fake = new FakeTransferClient();
        var uploader = new Uploader(fake);

        var req = new PutObjectRequest { Bucket = Bucket, Key = Key };
        var result = await uploader.UploadStreamAsync(req, new MemoryStream(Array.Empty<byte>()));

        Assert.Equal(200, result.StatusCode);
        Assert.Equal(1, fake.PutObjectCount);
        Assert.Equal(Array.Empty<byte>(), fake.GetStored(Bucket, Key));
    }

    [Fact]
    public async Task TestUploadContentTypeInferredFromKey()
    {
        var fake = new FakeTransferClient();
        var uploader = new Uploader(fake);

        var req = new PutObjectRequest { Bucket = Bucket, Key = "photo.jpg" };
        await uploader.UploadStreamAsync(req, new MemoryStream(RandomBytes(64)));

        Assert.Single(fake.PutObjectRequests);
        Assert.Equal("image/jpeg", fake.PutObjectRequests[0].ContentType);
    }

    [Fact]
    public async Task TestUploadContentTypeNotOverridden()
    {
        var fake = new FakeTransferClient();
        var uploader = new Uploader(fake);

        var req = new PutObjectRequest { Bucket = Bucket, Key = "photo.jpg", ContentType = "application/octet-stream" };
        await uploader.UploadStreamAsync(req, new MemoryStream(RandomBytes(64)));

        Assert.Equal("application/octet-stream", fake.PutObjectRequests[0].ContentType);
    }

    // --- Multipart uploads ---

    [Fact]
    public async Task TestUploadStreamMultiPartSequential()
    {
        var fake = new FakeTransferClient();
        var uploader = new Uploader(fake, o => { o.PartSize = 100 * 1024; o.ParallelNum = 1; });
        var data = RandomBytes(350 * 1024); // 4 parts (100,100,100,50)

        var req = new PutObjectRequest { Bucket = Bucket, Key = Key };
        var result = await uploader.UploadStreamAsync(req, new MemoryStream(data));

        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.UploadId);
        Assert.Equal(1, fake.InitiateCount);
        Assert.Equal(4, fake.UploadPartCount);
        Assert.Equal(1, fake.CompleteCount);
        Assert.Equal(0, fake.PutObjectCount);
        Assert.Equal(data, fake.GetStored(Bucket, Key));
    }

    [Fact]
    public async Task TestUploadStreamMultiPartParallel()
    {
        var fake = new FakeTransferClient();
        var uploader = new Uploader(fake, o => { o.PartSize = 64 * 1024; o.ParallelNum = 4; });
        var data = RandomBytes(500 * 1024);

        var req = new PutObjectRequest { Bucket = Bucket, Key = Key };
        var result = await uploader.UploadStreamAsync(req, new MemoryStream(data));

        Assert.Equal(200, result.StatusCode);
        Assert.Equal(1, fake.InitiateCount);
        Assert.Equal(1, fake.CompleteCount);
        Assert.Equal(data, fake.GetStored(Bucket, Key));
    }

    [Fact]
    public async Task TestUploadFileMultiPartParallel()
    {
        var fake = new FakeTransferClient();
        var uploader = new Uploader(fake, o => { o.PartSize = 100 * 1024; o.ParallelNum = 3; });
        var data = RandomBytes(450 * 1024);
        var path = NewTempFile(data);
        try
        {
            var req = new PutObjectRequest { Bucket = Bucket, Key = Key };
            var result = await uploader.UploadFileAsync(req, path);

            Assert.Equal(200, result.StatusCode);
            Assert.Equal(1, fake.InitiateCount);
            Assert.Equal(5, fake.UploadPartCount);
            Assert.Equal(1, fake.CompleteCount);
            Assert.Equal(data, fake.GetStored(Bucket, Key));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task TestUploadMultiPartPropagatesMetadataAndHeaders()
    {
        var fake = new FakeTransferClient();
        var uploader = new Uploader(fake, o => o.PartSize = 100 * 1024);
        var req = new PutObjectRequest
        {
            Bucket = Bucket,
            Key = Key,
            Metadata = new Dictionary<string, string> { ["foo"] = "bar" }
        };
        req.Headers["x-oss-object-acl"] = "private";

        await uploader.UploadStreamAsync(req, new MemoryStream(RandomBytes(250 * 1024)));

        Assert.Single(fake.InitiateRequests);
        Assert.NotNull(fake.InitiateRequests[0].Metadata);
        Assert.Equal("bar", fake.InitiateRequests[0].Metadata!["foo"]);
    }

    // --- CRC64 ---

    [Fact]
    public async Task TestUploadMultiPartCrcMatches()
    {
        var fake = new FakeTransferClient(); // CRC enabled by default
        var uploader = new Uploader(fake, o => o.PartSize = 100 * 1024);

        var req = new PutObjectRequest { Bucket = Bucket, Key = Key };
        var result = await uploader.UploadStreamAsync(req, new MemoryStream(RandomBytes(300 * 1024)));

        Assert.Equal(200, result.StatusCode);
    }

    [Fact]
    public async Task TestUploadMultiPartCrcMismatchThrows()
    {
        var fake = new FakeTransferClient { CompleteCrcOverride = "12345" };
        var uploader = new Uploader(fake, o => o.PartSize = 100 * 1024);

        var req = new PutObjectRequest { Bucket = Bucket, Key = Key };
        await Assert.ThrowsAsync<InconsistentException>(() =>
            uploader.UploadStreamAsync(req, new MemoryStream(RandomBytes(300 * 1024))));
    }

    [Fact]
    public async Task TestUploadMultiPartCrcDisabledSkipsCheck()
    {
        var fake = new FakeTransferClient(NoCrc) { CompleteCrcOverride = "12345" };
        var uploader = new Uploader(fake, o => o.PartSize = 100 * 1024);

        var req = new PutObjectRequest { Bucket = Bucket, Key = Key };
        var result = await uploader.UploadStreamAsync(req, new MemoryStream(RandomBytes(300 * 1024)));

        Assert.Equal(200, result.StatusCode);
    }

    // --- Progress ---

    [Fact]
    public async Task TestUploadMultiPartProgress()
    {
        var fake = new FakeTransferClient();
        var uploader = new Uploader(fake, o => { o.PartSize = 100 * 1024; o.ParallelNum = 1; });
        var data = RandomBytes(350 * 1024);

        long lastTransferred = 0;
        long total = 0;
        var req = new PutObjectRequest
        {
            Bucket = Bucket,
            Key = Key,
            ProgressFn = (increment, transferred, t) =>
            {
                lastTransferred = transferred;
                total = t;
            }
        };
        await uploader.UploadStreamAsync(req, new MemoryStream(data));

        Assert.Equal(data.Length, lastTransferred);
        Assert.Equal(data.Length, total);
    }

    // --- Failure handling ---

    [Fact]
    public async Task TestUploadSinglePartPutFailure()
    {
        var fake = new FakeTransferClient { OnPutObject = _ => new InvalidOperationException("boom") };
        var uploader = new Uploader(fake);

        var req = new PutObjectRequest { Bucket = Bucket, Key = Key };
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            uploader.UploadStreamAsync(req, new MemoryStream(RandomBytes(1024))));
    }

    [Fact]
    public async Task TestUploadInitiateFailure()
    {
        var fake = new FakeTransferClient { OnInitiate = _ => new InvalidOperationException("boom") };
        var uploader = new Uploader(fake, o => o.PartSize = 100 * 1024);

        var req = new PutObjectRequest { Bucket = Bucket, Key = Key };
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            uploader.UploadStreamAsync(req, new MemoryStream(RandomBytes(300 * 1024))));
        Assert.Equal(0, fake.CompleteCount);
    }

    [Fact]
    public async Task TestUploadPartFailureAborts()
    {
        var fake = new FakeTransferClient { OnUploadPart = _ => new InvalidOperationException("boom") };
        var uploader = new Uploader(fake, o => o.PartSize = 100 * 1024);

        var req = new PutObjectRequest { Bucket = Bucket, Key = Key };
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            uploader.UploadStreamAsync(req, new MemoryStream(RandomBytes(300 * 1024))));
        Assert.Equal(0, fake.CompleteCount);
        Assert.Equal(1, fake.AbortCount);
    }

    [Fact]
    public async Task TestUploadCompleteFailureAborts()
    {
        var fake = new FakeTransferClient { OnComplete = _ => new InvalidOperationException("boom") };
        var uploader = new Uploader(fake, o => o.PartSize = 100 * 1024);

        var req = new PutObjectRequest { Bucket = Bucket, Key = Key };
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            uploader.UploadStreamAsync(req, new MemoryStream(RandomBytes(300 * 1024))));
        Assert.Equal(1, fake.AbortCount);
    }

    [Fact]
    public async Task TestUploadPartFailureLeavePartsNoAbort()
    {
        var fake = new FakeTransferClient { OnUploadPart = _ => new InvalidOperationException("boom") };
        var uploader = new Uploader(fake, o => { o.PartSize = 100 * 1024; o.LeavePartsOnError = true; });

        var req = new PutObjectRequest { Bucket = Bucket, Key = Key };
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            uploader.UploadStreamAsync(req, new MemoryStream(RandomBytes(300 * 1024))));
        Assert.Equal(0, fake.AbortCount);
    }

    // --- Checkpoint resume ---

    [Fact]
    public async Task TestUploadFileCheckpointResume()
    {
        var fake = new FakeTransferClient();
        var data = RandomBytes(350 * 1024); // 4 parts at 100 KiB (100,100,100,50)
        var path = NewTempFile(data);
        var cpDir = Path.Combine(Path.GetTempPath(), "ossupcp-" + Guid.NewGuid());
        Directory.CreateDirectory(cpDir);
        try
        {
            // First pass: cancel after ~two parts so a checkpoint is left on disk.
            var cts = new CancellationTokenSource();
            long transferred = 0;
            var uploader = new Uploader(fake, o =>
            {
                o.PartSize = 100 * 1024;
                o.ParallelNum = 1;
                o.EnableCheckpoint = true;
                o.CheckpointDir = cpDir;
            });

            var req = new PutObjectRequest
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
                uploader.UploadFileAsync(req, path, null, cts.Token));

            Assert.NotEmpty(Directory.GetFiles(cpDir, "*.ucp"));

            // Second pass: reuse the same fake (which retains the multipart upload) and resume.
            var req2 = new PutObjectRequest { Bucket = Bucket, Key = Key };
            var result = await uploader.UploadFileAsync(req2, path);

            Assert.Equal(200, result.StatusCode);
            Assert.Equal(1, fake.CompleteCount); // only the resumed pass completes
            Assert.True(fake.ListPartsCount >= 1); // resume listed the existing parts
            Assert.Equal(data, fake.GetStored(Bucket, Key));
            Assert.Empty(Directory.GetFiles(cpDir, "*.ucp")); // checkpoint removed on success
        }
        finally
        {
            File.Delete(path);
            Directory.Delete(cpDir, true);
        }
    }

    // --- Part-size auto scaling ---

    [Fact]
    public async Task TestUploadPartSizeAutoScaled()
    {
        var fake = new FakeTransferClient();
        // 10000 bytes with a 1-byte part would need 10000 parts (== MaxUploadParts),
        // so ApplySource must grow the part size to 2 bytes → 5000 parts.
        var data = RandomBytes(10000);
        var uploader = new Uploader(fake, o => { o.PartSize = 1; o.ParallelNum = 4; });

        var req = new PutObjectRequest { Bucket = Bucket, Key = Key };
        var result = await uploader.UploadStreamAsync(req, new MemoryStream(data));

        Assert.Equal(200, result.StatusCode);
        Assert.Equal(5000, fake.UploadPartCount);
        Assert.True(fake.UploadPartCount <= 10000); // never exceeds MaxUploadParts
        Assert.Equal(data, fake.GetStored(Bucket, Key));
    }
}
