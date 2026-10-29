using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AlibabaCloud.OSS.V2;
using AlibabaCloud.OSS.V2.Models;
using AlibabaCloud.OSS.V2.UnitTests.Fakes;

namespace AlibabaCloud.OSS.V2.UnitTests;

public class CopierBehaviorTest
{
    private const string Bucket = "bucket";
    private const string DstKey = "dst";
    private const string SrcKey = "src";

    private static byte[] RandomBytes(int size)
    {
        var data = new byte[size];
        new Random(size + 13).NextBytes(data);
        return data;
    }

    private static CopyObjectRequest NewCopyReq() => new()
    {
        Bucket = Bucket,
        Key = DstKey,
        SourceKey = SrcKey
    };

    // --- Single copy ---

    [Fact]
    public async Task TestSingleCopy()
    {
        var fake = new FakeTransferClient();
        var data = RandomBytes(4096);
        fake.SeedObject(Bucket, SrcKey, data);
        var copier = new Copier(fake); // default 200MiB threshold → single copy

        var result = await copier.CopyAsync(NewCopyReq());

        Assert.Equal(200, result.StatusCode);
        Assert.Null(result.UploadId);
        Assert.Equal(1, fake.HeadObjectCount);
        Assert.Equal(1, fake.CopyObjectCount);
        Assert.Equal(0, fake.InitiateCount);
        Assert.Equal(data, fake.GetStored(Bucket, DstKey));
    }

    // --- Shallow copy ---

    [Fact]
    public async Task TestShallowCopyUsedForLargeSameBucket()
    {
        var fake = new FakeTransferClient();
        var data = RandomBytes(300 * 1024);
        fake.SeedObject(Bucket, SrcKey, data);
        var copier = new Copier(fake, o => o.MultipartCopyThreshold = 100 * 1024);

        var result = await copier.CopyAsync(NewCopyReq());

        Assert.Equal(200, result.StatusCode);
        Assert.Equal(1, fake.CopyObjectCount);
        Assert.Equal(0, fake.InitiateCount);
    }

    [Fact]
    public async Task TestShallowCopyDisabledByOption()
    {
        var fake = new FakeTransferClient();
        var data = RandomBytes(300 * 1024);
        fake.SeedObject(Bucket, SrcKey, data);
        var copier = new Copier(fake, o =>
        {
            o.MultipartCopyThreshold = 100 * 1024;
            o.PartSize = 100 * 1024;
            o.DisableShallowCopy = true;
        });

        var result = await copier.CopyAsync(NewCopyReq());

        Assert.Equal(1, fake.InitiateCount);
        Assert.Equal(1, fake.CompleteCount);
        Assert.Equal(0, fake.CopyObjectCount);
        Assert.Equal(data, fake.GetStored(Bucket, DstKey));
    }

    [Fact]
    public async Task TestShallowCopyDisabledBySse()
    {
        var fake = new FakeTransferClient();
        var data = RandomBytes(300 * 1024);
        fake.SeedObject(Bucket, SrcKey, data,
            new Dictionary<string, string> { ["x-oss-server-side-encryption"] = "AES256" });
        var copier = new Copier(fake, o => { o.MultipartCopyThreshold = 100 * 1024; o.PartSize = 100 * 1024; });

        await copier.CopyAsync(NewCopyReq());

        Assert.Equal(1, fake.InitiateCount); // fell back to multipart copy
        Assert.Equal(0, fake.CopyObjectCount);
    }

    [Fact]
    public async Task TestShallowCopyWithSseAllowedByNoCheckSse()
    {
        var fake = new FakeTransferClient();
        var data = RandomBytes(300 * 1024);
        fake.SeedObject(Bucket, SrcKey, data,
            new Dictionary<string, string> { ["x-oss-server-side-encryption"] = "AES256" });
        var copier = new Copier(fake, o =>
        {
            o.MultipartCopyThreshold = 100 * 1024;
            o.NoCheckSse = true;
        });

        await copier.CopyAsync(NewCopyReq());

        Assert.Equal(1, fake.CopyObjectCount); // shallow copy used
        Assert.Equal(0, fake.InitiateCount);
    }

    [Fact]
    public async Task TestShallowCopyDisabledByCrossBucket()
    {
        var fake = new FakeTransferClient();
        var data = RandomBytes(300 * 1024);
        fake.SeedObject("srcbucket", SrcKey, data);
        var copier = new Copier(fake, o => { o.MultipartCopyThreshold = 100 * 1024; o.PartSize = 100 * 1024; });

        var req = new CopyObjectRequest
        {
            Bucket = Bucket,
            Key = DstKey,
            SourceBucket = "srcbucket",
            SourceKey = SrcKey
        };
        await copier.CopyAsync(req);

        Assert.Equal(1, fake.InitiateCount); // multipart copy across buckets
        Assert.Equal(data, fake.GetStored(Bucket, DstKey));
    }

    [Fact]
    public async Task TestShallowCopyDisabledByStorageClass()
    {
        var fake = new FakeTransferClient();
        var data = RandomBytes(300 * 1024);
        fake.SeedObject(Bucket, SrcKey, data);
        var copier = new Copier(fake, o => { o.MultipartCopyThreshold = 100 * 1024; o.PartSize = 100 * 1024; });

        var req = NewCopyReq();
        req.StorageClass = "IA";
        await copier.CopyAsync(req);

        Assert.Equal(1, fake.InitiateCount);
        Assert.Equal(0, fake.CopyObjectCount);
    }

    // --- Multipart copy ---

    [Fact]
    public async Task TestMultipartCopyParallel()
    {
        var fake = new FakeTransferClient();
        var data = RandomBytes(450 * 1024);
        fake.SeedObject(Bucket, SrcKey, data);
        var copier = new Copier(fake, o =>
        {
            o.MultipartCopyThreshold = 100 * 1024;
            o.PartSize = 100 * 1024;
            o.ParallelNum = 3;
            o.DisableShallowCopy = true;
        });

        var result = await copier.CopyAsync(NewCopyReq());

        Assert.Equal(1, fake.InitiateCount);
        Assert.Equal(5, fake.UploadPartCopyCount);
        Assert.Equal(1, fake.CompleteCount);
        Assert.NotNull(result.UploadId);
        Assert.Equal(data, fake.GetStored(Bucket, DstKey));
    }

    [Fact]
    public async Task TestMultipartCopyCrcMismatchThrows()
    {
        var fake = new FakeTransferClient { CompleteCrcOverride = "12345" };
        var data = RandomBytes(300 * 1024);
        fake.SeedObject(Bucket, SrcKey, data);
        var copier = new Copier(fake, o =>
        {
            o.MultipartCopyThreshold = 100 * 1024;
            o.PartSize = 100 * 1024;
            o.DisableShallowCopy = true;
        });

        await Assert.ThrowsAsync<InconsistentException>(() => copier.CopyAsync(NewCopyReq()));
    }

    // --- Metadata / tagging directives ---

    [Fact]
    public async Task TestMultipartCopyCopiesSourceMetadata()
    {
        var fake = new FakeTransferClient();
        var data = RandomBytes(300 * 1024);
        fake.SeedObject(Bucket, SrcKey, data,
            new Dictionary<string, string> { ["x-oss-meta-foo"] = "bar" });
        var copier = new Copier(fake, o =>
        {
            o.MultipartCopyThreshold = 100 * 1024;
            o.PartSize = 100 * 1024;
            o.DisableShallowCopy = true;
        });

        await copier.CopyAsync(NewCopyReq());

        Assert.Single(fake.InitiateRequests);
        Assert.NotNull(fake.InitiateRequests[0].Metadata);
        Assert.Equal("bar", fake.InitiateRequests[0].Metadata!["foo"]);
    }

    [Fact]
    public async Task TestMultipartCopyCopiesSourceTagging()
    {
        var fake = new FakeTransferClient();
        var data = RandomBytes(300 * 1024);
        fake.SeedObject(Bucket, SrcKey, data, tags: new List<Tag> { new() { Key = "k", Value = "v" } });
        var copier = new Copier(fake, o =>
        {
            o.MultipartCopyThreshold = 100 * 1024;
            o.PartSize = 100 * 1024;
            o.DisableShallowCopy = true;
        });

        await copier.CopyAsync(NewCopyReq());

        Assert.Equal(1, fake.GetObjectTaggingCount);
        Assert.Single(fake.InitiateRequests);
        Assert.Contains("k=v", fake.InitiateRequests[0].Tagging);
    }

    [Fact]
    public async Task TestMultipartCopyEncodesSourceTagging()
    {
        var fake = new FakeTransferClient();
        var data = RandomBytes(300 * 1024);
        fake.SeedObject(Bucket, SrcKey, data, tags: new List<Tag>
        {
            new() { Key = "k=", Value = "v+" },
            new() { Key = "a&b", Value = "c d" }
        });
        var copier = new Copier(fake, o =>
        {
            o.MultipartCopyThreshold = 100 * 1024;
            o.PartSize = 100 * 1024;
            o.DisableShallowCopy = true;
        });

        await copier.CopyAsync(NewCopyReq());

        Assert.Single(fake.InitiateRequests);
        var tagging = fake.InitiateRequests[0].Tagging;
        Assert.Contains("k%3D=v%2B", tagging);
        Assert.Contains("a%26b=c%20d", tagging);
    }

    // --- Header / parameter propagation ---

    [Fact]
    public async Task TestMultipartCopyForwardsAclToInitiateAndComplete()
    {
        var fake = new FakeTransferClient();
        var data = RandomBytes(300 * 1024);
        fake.SeedObject(Bucket, SrcKey, data);
        var copier = new Copier(fake, o =>
        {
            o.MultipartCopyThreshold = 100 * 1024;
            o.PartSize = 100 * 1024;
            o.DisableShallowCopy = true;
        });

        var req = NewCopyReq();
        req.Acl = "public-read";
        await copier.CopyAsync(req);

        Assert.Single(fake.InitiateRequests);
        Assert.Equal("public-read", fake.InitiateRequests[0].Headers["x-oss-object-acl"]);
        Assert.Single(fake.CompleteRequests);
        Assert.Equal("public-read", fake.CompleteRequests[0].Acl);
    }

    [Fact]
    public async Task TestMultipartCopyForwardsCallbackToComplete()
    {
        var fake = new FakeTransferClient();
        var data = RandomBytes(300 * 1024);
        fake.SeedObject(Bucket, SrcKey, data);
        var copier = new Copier(fake, o =>
        {
            o.MultipartCopyThreshold = 100 * 1024;
            o.PartSize = 100 * 1024;
            o.DisableShallowCopy = true;
        });

        var req = NewCopyReq();
        req.Headers["x-oss-callback"] = "base64callback";
        req.Headers["x-oss-callback-var"] = "base64var";
        await copier.CopyAsync(req);

        Assert.Single(fake.CompleteRequests);
        Assert.Equal("base64callback", fake.CompleteRequests[0].Callback);
        Assert.Equal("base64var", fake.CompleteRequests[0].CallbackVar);
    }

    [Fact]
    public async Task TestMultipartCopyForwardsCustomHeadersAndParameters()
    {
        var fake = new FakeTransferClient();
        var data = RandomBytes(300 * 1024);
        fake.SeedObject(Bucket, SrcKey, data);
        var copier = new Copier(fake, o =>
        {
            o.MultipartCopyThreshold = 100 * 1024;
            o.PartSize = 100 * 1024;
            o.DisableShallowCopy = true;
        });

        var req = NewCopyReq();
        req.Headers["x-oss-custom"] = "custom-value";
        req.Parameters["custom-param"] = "param-value";
        await copier.CopyAsync(req);

        Assert.Single(fake.InitiateRequests);
        Assert.Equal("custom-value", fake.InitiateRequests[0].Headers["x-oss-custom"]);
        Assert.Equal("param-value", fake.InitiateRequests[0].Parameters["custom-param"]);
        Assert.Single(fake.CompleteRequests);
        Assert.Equal("custom-value", fake.CompleteRequests[0].Headers["x-oss-custom"]);
        Assert.Equal("param-value", fake.CompleteRequests[0].Parameters["custom-param"]);
    }

    [Fact]
    public async Task TestMultipartCopyDoesNotForwardCopySpecificHeaders()
    {
        var fake = new FakeTransferClient();
        var data = RandomBytes(300 * 1024);
        fake.SeedObject(Bucket, SrcKey, data);
        var copier = new Copier(fake, o =>
        {
            o.MultipartCopyThreshold = 100 * 1024;
            o.PartSize = 100 * 1024;
            o.DisableShallowCopy = true;
        });

        var req = NewCopyReq();
        req.IfMatch = "\"etag\"";
        req.IfNoneMatch = "\"none\"";
        req.IfModifiedSince = "Mon, 01 Jan 2024 00:00:00 GMT";
        req.IfUnmodifiedSince = "Mon, 01 Jan 2024 00:00:00 GMT";
        req.MetadataDirective = "COPY";
        req.TaggingDirective = "COPY";
        await copier.CopyAsync(req);

        Assert.Single(fake.InitiateRequests);
        Assert.False(fake.InitiateRequests[0].Headers.ContainsKey("x-oss-copy-source-if-match"));
        Assert.False(fake.InitiateRequests[0].Headers.ContainsKey("x-oss-copy-source-if-none-match"));
        Assert.False(fake.InitiateRequests[0].Headers.ContainsKey("x-oss-copy-source-if-modified-since"));
        Assert.False(fake.InitiateRequests[0].Headers.ContainsKey("x-oss-copy-source-if-unmodified-since"));
        Assert.False(fake.InitiateRequests[0].Headers.ContainsKey("x-oss-metadata-directive"));
        Assert.False(fake.InitiateRequests[0].Headers.ContainsKey("x-oss-tagging-directive"));
    }

    [Fact]
    public async Task TestMultipartCopyForwardsSourceConditionsToHeadAndPartCopy()
    {
        var fake = new FakeTransferClient();
        var data = RandomBytes(300 * 1024);
        fake.SeedObject(Bucket, SrcKey, data,
            new Dictionary<string, string> { ["ETag"] = "\"etag\"" });
        var copier = new Copier(fake, o =>
        {
            o.MultipartCopyThreshold = 100 * 1024;
            o.PartSize = 100 * 1024;
            o.DisableShallowCopy = true;
        });

        var req = NewCopyReq();
        req.IfMatch = "\"etag\"";
        req.IfNoneMatch = "\"none\"";
        req.IfModifiedSince = "Mon, 01 Jan 2024 00:00:00 GMT";
        req.IfUnmodifiedSince = "Mon, 01 Jan 2024 00:00:00 GMT";
        await copier.CopyAsync(req);

        Assert.Single(fake.HeadObjectRequests);
        Assert.Equal("\"etag\"", fake.HeadObjectRequests[0].IfMatch);
        Assert.Equal("\"none\"", fake.HeadObjectRequests[0].IfNoneMatch);
        Assert.Equal("Mon, 01 Jan 2024 00:00:00 GMT", fake.HeadObjectRequests[0].IfModifiedSince);
        Assert.Equal("Mon, 01 Jan 2024 00:00:00 GMT", fake.HeadObjectRequests[0].IfUnmodifiedSince);

        Assert.Equal(3, fake.UploadPartCopyRequests.Count);
        foreach (var up in fake.UploadPartCopyRequests)
        {
            Assert.Equal("\"etag\"", up.IfMatch);
            Assert.Equal("\"none\"", up.IfNoneMatch);
            Assert.Equal("Mon, 01 Jan 2024 00:00:00 GMT", up.IfModifiedSince);
            Assert.Equal("Mon, 01 Jan 2024 00:00:00 GMT", up.IfUnmodifiedSince);
        }
    }

    [Fact]
    public async Task TestMultipartCopyCopyModeRemovesRequestSideUserMetadata()
    {
        var fake = new FakeTransferClient();
        var data = RandomBytes(300 * 1024);
        fake.SeedObject(Bucket, SrcKey, data,
            new Dictionary<string, string> { ["x-oss-meta-source"] = "source-value" });
        var copier = new Copier(fake, o =>
        {
            o.MultipartCopyThreshold = 100 * 1024;
            o.PartSize = 100 * 1024;
            o.DisableShallowCopy = true;
        });

        var req = NewCopyReq();
        req.Headers["x-oss-meta-request"] = "request-value";
        req.MetadataDirective = "COPY";
        await copier.CopyAsync(req);

        Assert.Single(fake.InitiateRequests);
        Assert.False(fake.InitiateRequests[0].Headers.ContainsKey("x-oss-meta-request"));
        Assert.NotNull(fake.InitiateRequests[0].Metadata);
        Assert.Equal("source-value", fake.InitiateRequests[0].Metadata!["source"]);
    }

    [Fact]
    public async Task TestMultipartCopyReplaceModeKeepsRequestSideUserMetadata()
    {
        var fake = new FakeTransferClient();
        var data = RandomBytes(300 * 1024);
        fake.SeedObject(Bucket, SrcKey, data);
        var copier = new Copier(fake, o =>
        {
            o.MultipartCopyThreshold = 100 * 1024;
            o.PartSize = 100 * 1024;
            o.DisableShallowCopy = true;
        });

        var req = NewCopyReq();
        req.Headers["x-oss-meta-request"] = "request-value";
        req.Metadata = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["request"] = "request-value"
        };
        req.MetadataDirective = "REPLACE";
        await copier.CopyAsync(req);

        Assert.Single(fake.InitiateRequests);
        Assert.Equal("request-value", fake.InitiateRequests[0].Headers["x-oss-meta-request"]);
        Assert.NotNull(fake.InitiateRequests[0].Metadata);
        Assert.Equal("request-value", fake.InitiateRequests[0].Metadata!["request"]);
    }

    [Fact]
    public async Task TestMultipartCopyAbortForwardsRequestPayer()
    {
        var fake = new FakeTransferClient { OnUploadPartCopy = _ => new InvalidOperationException("boom") };
        fake.SeedObject(Bucket, SrcKey, RandomBytes(300 * 1024));
        var copier = new Copier(fake, o =>
        {
            o.MultipartCopyThreshold = 100 * 1024;
            o.PartSize = 100 * 1024;
            o.DisableShallowCopy = true;
        });

        var req = NewCopyReq();
        req.Headers["x-oss-request-payer"] = "requester";
        await Assert.ThrowsAsync<InvalidOperationException>(() => copier.CopyAsync(req));

        Assert.Single(fake.AbortRequests);
        Assert.Equal("requester", fake.AbortRequests[0].Headers["x-oss-request-payer"]);
    }

    [Fact]
    public async Task TestMultipartCopyForwardsRequestPayerToGetObjectTagging()
    {
        var fake = new FakeTransferClient();
        var data = RandomBytes(300 * 1024);
        fake.SeedObject(Bucket, SrcKey, data, tags: new List<Tag> { new() { Key = "k", Value = "v" } });
        var copier = new Copier(fake, o =>
        {
            o.MultipartCopyThreshold = 100 * 1024;
            o.PartSize = 100 * 1024;
            o.DisableShallowCopy = true;
        });

        var req = NewCopyReq();
        req.Headers["x-oss-request-payer"] = "requester";
        await copier.CopyAsync(req);

        Assert.Single(fake.HeadObjectRequests);
        Assert.Equal("requester", fake.HeadObjectRequests[0].Headers["x-oss-request-payer"]);

        Assert.Single(fake.GetObjectTaggingRequests);
        Assert.Equal("requester", fake.GetObjectTaggingRequests[0].Headers["x-oss-request-payer"]);

        Assert.Equal(3, fake.UploadPartCopyRequests.Count);
        foreach (var up in fake.UploadPartCopyRequests)
            Assert.Equal("requester", up.Headers["x-oss-request-payer"]);

        Assert.Single(fake.InitiateRequests);
        Assert.Contains("k=v", fake.InitiateRequests[0].Tagging);
    }

    // --- Failure handling ---

    [Fact]
    public async Task TestCopyHeadObjectFailure()
    {
        var fake = new FakeTransferClient { OnHeadObject = _ => new InvalidOperationException("boom") };
        var copier = new Copier(fake);

        await Assert.ThrowsAsync<InvalidOperationException>(() => copier.CopyAsync(NewCopyReq()));
    }

    [Fact]
    public async Task TestSingleCopyFailure()
    {
        var fake = new FakeTransferClient { OnCopyObject = _ => new InvalidOperationException("boom") };
        fake.SeedObject(Bucket, SrcKey, RandomBytes(4096));
        var copier = new Copier(fake);

        await Assert.ThrowsAsync<InvalidOperationException>(() => copier.CopyAsync(NewCopyReq()));
    }

    [Fact]
    public async Task TestMultipartCopyInitiateFailure()
    {
        var fake = new FakeTransferClient { OnInitiate = _ => new InvalidOperationException("boom") };
        fake.SeedObject(Bucket, SrcKey, RandomBytes(300 * 1024));
        var copier = new Copier(fake, o =>
        {
            o.MultipartCopyThreshold = 100 * 1024;
            o.PartSize = 100 * 1024;
            o.DisableShallowCopy = true;
        });

        await Assert.ThrowsAsync<InvalidOperationException>(() => copier.CopyAsync(NewCopyReq()));
        Assert.Equal(0, fake.CompleteCount);
    }

    [Fact]
    public async Task TestMultipartCopyUploadPartCopyFailureAborts()
    {
        var fake = new FakeTransferClient { OnUploadPartCopy = _ => new InvalidOperationException("boom") };
        fake.SeedObject(Bucket, SrcKey, RandomBytes(300 * 1024));
        var copier = new Copier(fake, o =>
        {
            o.MultipartCopyThreshold = 100 * 1024;
            o.PartSize = 100 * 1024;
            o.DisableShallowCopy = true;
        });

        await Assert.ThrowsAsync<InvalidOperationException>(() => copier.CopyAsync(NewCopyReq()));
        Assert.Equal(1, fake.AbortCount);
        Assert.Equal(0, fake.CompleteCount);
    }

    [Fact]
    public async Task TestMultipartCopyCancellationWaitsForStartedTasks()
    {
        var fake = new FakeTransferClient();
        fake.SeedObject(Bucket, SrcKey, RandomBytes(300 * 1024));
        var copier = new Copier(fake, o =>
        {
            o.MultipartCopyThreshold = 100 * 1024;
            o.PartSize = 100 * 1024;
            o.ParallelNum = 1;
            o.DisableShallowCopy = true;
        });

        var entered = new TaskCompletionSource<bool>();
        var release = new TaskCompletionSource<bool>();
        fake.UploadPartCopyHook = _ =>
        {
            entered.SetResult(true);
            return release.Task;
        };

        var cts = new CancellationTokenSource();
        var copyTask = copier.CopyAsync(NewCopyReq(), null, cts.Token);

        // Wait until the first (and only concurrent) UploadPartCopy has started.
        await entered.Task;

        // Cancel while the scheduler is waiting for the next semaphore slot.
        cts.Cancel();

        // Release the in-flight part so it can finish.
        release.SetResult(true);

        var ex = await Record.ExceptionAsync(() => copyTask);
        Assert.IsAssignableFrom<OperationCanceledException>(ex);
        // At least the in-flight part must have completed before the call returned.
        Assert.True(fake.UploadPartCopyCount >= 1);
    }

    [Fact]
    public async Task TestMultipartCopyLeavePartsNoAbort()
    {
        var fake = new FakeTransferClient { OnUploadPartCopy = _ => new InvalidOperationException("boom") };
        fake.SeedObject(Bucket, SrcKey, RandomBytes(300 * 1024));
        var copier = new Copier(fake, o =>
        {
            o.MultipartCopyThreshold = 100 * 1024;
            o.PartSize = 100 * 1024;
            o.DisableShallowCopy = true;
            o.LeavePartsOnError = true;
        });

        await Assert.ThrowsAsync<InvalidOperationException>(() => copier.CopyAsync(NewCopyReq()));
        Assert.Equal(0, fake.AbortCount);
    }

    [Fact]
    public async Task TestShallowCopyTimeoutFallsBackToMultipart()
    {
        var fake = new FakeTransferClient { OnCopyObject = _ => new OperationCanceledException() };
        var data = RandomBytes(300 * 1024);
        fake.SeedObject(Bucket, SrcKey, data);
        var copier = new Copier(fake, o =>
        {
            o.MultipartCopyThreshold = 100 * 1024;
            o.PartSize = 100 * 1024;
        });

        var result = await copier.CopyAsync(NewCopyReq());

        Assert.Equal(1, fake.CopyObjectCount); // shallow attempt
        Assert.Equal(1, fake.InitiateCount);   // fallback multipart copy
        Assert.Equal(data, fake.GetStored(Bucket, DstKey));
    }

    [Fact]
    public async Task TestShallowCopyWrappedTimeoutFallsBackToMultipart()
    {
        var fake = new FakeTransferClient
        {
            OnCopyObject = _ => new OperationException(
                "CopyObject",
                new RequestTimeoutException("request timeout", new OperationCanceledException()))
        };
        var data = RandomBytes(300 * 1024);
        fake.SeedObject(Bucket, SrcKey, data);
        var copier = new Copier(fake, o =>
        {
            o.MultipartCopyThreshold = 100 * 1024;
            o.PartSize = 100 * 1024;
        });

        var result = await copier.CopyAsync(NewCopyReq());

        Assert.Equal(1, fake.CopyObjectCount);
        Assert.Equal(1, fake.InitiateCount);
        Assert.Equal(data, fake.GetStored(Bucket, DstKey));
    }

    [Fact]
    public async Task TestShallowCopyEntityTooLargeFallsBackToMultipart()
    {
        var serviceEx = new ServiceException(400, new Dictionary<string, string> { ["Code"] = "EntityTooLarge" });
        var fake = new FakeTransferClient { OnCopyObject = _ => new OperationException("CopyObject", serviceEx) };
        var data = RandomBytes(300 * 1024);
        fake.SeedObject(Bucket, SrcKey, data);
        var copier = new Copier(fake, o =>
        {
            o.MultipartCopyThreshold = 100 * 1024;
            o.PartSize = 100 * 1024;
        });

        var result = await copier.CopyAsync(NewCopyReq());

        Assert.Equal(1, fake.CopyObjectCount);
        Assert.Equal(1, fake.InitiateCount);
        Assert.Equal(data, fake.GetStored(Bucket, DstKey));
    }

    [Fact]
    public async Task TestShallowCopyDoesNotFallbackOnUserCancellation()
    {
        var cts = new CancellationTokenSource();
        var fake = new FakeTransferClient
        {
            OnCopyObject = _ =>
            {
                // Simulate the user cancelling while the shallow CopyObject is in flight.
                cts.Cancel();
                return null;
            }
        };
        fake.SeedObject(Bucket, SrcKey, RandomBytes(300 * 1024));
        var copier = new Copier(fake, o =>
        {
            o.MultipartCopyThreshold = 100 * 1024;
            o.PartSize = 100 * 1024;
        });

        await Assert.ThrowsAsync<OperationCanceledException>(() => copier.CopyAsync(NewCopyReq(), null, cts.Token));
        Assert.Equal(1, fake.CopyObjectCount);
        Assert.Equal(0, fake.InitiateCount);
    }
}
