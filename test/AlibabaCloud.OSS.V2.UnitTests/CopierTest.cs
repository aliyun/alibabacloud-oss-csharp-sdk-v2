using System;
using System.Threading.Tasks;
using AlibabaCloud.OSS.V2.Credentials;
using AlibabaCloud.OSS.V2.Internal;
using AlibabaCloud.OSS.V2.Models;

namespace AlibabaCloud.OSS.V2.UnitTests;

public class CopierTest
{
    private static Client NewClient() => new(new Configuration
    {
        Region = "cn-hangzhou",
        CredentialsProvider = new AnonymousCredentialsProvider(),
    });

    [Fact]
    public void TestOptionsDefaults()
    {
        var client = NewClient();
        var copier = new Copier(client);
        Assert.NotNull(copier);

        var opt = new CopierOptions();
        Assert.Equal(Defaults.DefaultCopyPartSize, opt.PartSize);
        Assert.Equal(Defaults.DefaultCopyParallel, opt.ParallelNum);
        Assert.Equal(Defaults.DefaultCopyThreshold, opt.MultipartCopyThreshold);
        Assert.False(opt.LeavePartsOnError);
        Assert.False(opt.DisableShallowCopy);
        Assert.False(opt.NoCheckSse);
        Assert.False(opt.NoCheckCrossBucket);
    }

    [Fact]
    public void TestOptionsOverride()
    {
        var client = NewClient();
        var copier = new Copier(client, o =>
        {
            o.PartSize = 100 * 1024 * 1024;
            o.ParallelNum = 4;
            o.MultipartCopyThreshold = 500 * 1024 * 1024;
            o.LeavePartsOnError = true;
            o.DisableShallowCopy = true;
            o.NoCheckSse = true;
            o.NoCheckCrossBucket = true;
        });
        Assert.NotNull(copier);
    }

    [Fact]
    public void TestCtorNullClient()
    {
        Assert.Throws<ArgumentNullException>(() => new Copier((Client)null!));
    }

    [Fact]
    public async Task TestCopyNullRequest()
    {
        var copier = new Copier(NewClient());
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            copier.CopyAsync(null!));
    }

    [Fact]
    public async Task TestCopyEmptyBucket()
    {
        var copier = new Copier(NewClient());
        var req = new CopyObjectRequest { Bucket = "", Key = "key", SourceKey = "src" };
        await Assert.ThrowsAsync<ArgumentException>(() =>
            copier.CopyAsync(req));
    }

    [Fact]
    public async Task TestCopyEmptyKey()
    {
        var copier = new Copier(NewClient());
        var req = new CopyObjectRequest { Bucket = "bucket", Key = "", SourceKey = "src" };
        await Assert.ThrowsAsync<ArgumentException>(() =>
            copier.CopyAsync(req));
    }

    [Fact]
    public async Task TestCopyEmptySourceKey()
    {
        var copier = new Copier(NewClient());
        var req = new CopyObjectRequest { Bucket = "bucket", Key = "key", SourceKey = "" };
        await Assert.ThrowsAsync<ArgumentException>(() =>
            copier.CopyAsync(req));
    }
}
