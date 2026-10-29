using System;
using System.IO;
using System.Threading.Tasks;
using AlibabaCloud.OSS.V2.Credentials;
using AlibabaCloud.OSS.V2.Internal;
using AlibabaCloud.OSS.V2.Models;

namespace AlibabaCloud.OSS.V2.UnitTests;

public class UploaderTest
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
        var uploader = new Uploader(client);
        Assert.NotNull(uploader);

        var opt = new UploaderOptions();
        Assert.Equal(Defaults.DefaultUploadPartSize, opt.PartSize);
        Assert.Equal(Defaults.DefaultUploadParallel, opt.ParallelNum);
        Assert.False(opt.LeavePartsOnError);
        Assert.False(opt.EnableCheckpoint);
    }

    [Fact]
    public void TestOptionsOverride()
    {
        var client = NewClient();
        var uploader = new Uploader(client, o =>
        {
            o.PartSize = 12 * 1024 * 1024;
            o.ParallelNum = 8;
            o.LeavePartsOnError = true;
            o.EnableCheckpoint = true;
            o.CheckpointDir = "/tmp/cp";
        });
        Assert.NotNull(uploader);
    }

    [Fact]
    public void TestCtorNullClient()
    {
        Assert.Throws<ArgumentNullException>(() => new Uploader((Client)null!));
    }

    [Fact]
    public async Task TestUploadFileNullRequest()
    {
        var uploader = new Uploader(NewClient());
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            uploader.UploadFileAsync(null!, "any.txt"));
    }

    [Fact]
    public async Task TestUploadFileEmptyBucket()
    {
        var uploader = new Uploader(NewClient());
        var req = new PutObjectRequest { Bucket = "", Key = "key" };
        await Assert.ThrowsAsync<ArgumentException>(() =>
            uploader.UploadFileAsync(req, "any.txt"));
    }

    [Fact]
    public async Task TestUploadFileEmptyKey()
    {
        var uploader = new Uploader(NewClient());
        var req = new PutObjectRequest { Bucket = "bucket", Key = "" };
        await Assert.ThrowsAsync<ArgumentException>(() =>
            uploader.UploadFileAsync(req, "any.txt"));
    }

    [Fact]
    public async Task TestUploadFileNotExists()
    {
        var uploader = new Uploader(NewClient());
        var req = new PutObjectRequest { Bucket = "bucket", Key = "key" };
        await Assert.ThrowsAsync<FileNotFoundException>(() =>
            uploader.UploadFileAsync(req, Path.Combine(Path.GetTempPath(), "no-such-file-" + Guid.NewGuid())));
    }

    [Fact]
    public async Task TestUploadStreamNullBody()
    {
        var uploader = new Uploader(NewClient());
        var req = new PutObjectRequest { Bucket = "bucket", Key = "key" };
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            uploader.UploadStreamAsync(req, null!));
    }

    [Fact]
    public void TestUploadCheckpointRoundTrip()
    {
        var dir = Path.Combine(Path.GetTempPath(), "ossucp-" + Guid.NewGuid());
        Directory.CreateDirectory(dir);
        try
        {
            var cp = UploadCheckpoint.Create(
                "bucket", "key", "/data/file.bin", dir,
                123456, "2026-01-01T00:00:00Z", Defaults.DefaultUploadPartSize);
            cp.UploadId = "upload-id-abc";
            cp.Dump();
            Assert.True(File.Exists(cp.CpFilePath));

            var cp2 = UploadCheckpoint.Create(
                "bucket", "key", "/data/file.bin", dir,
                123456, "2026-01-01T00:00:00Z", Defaults.DefaultUploadPartSize);
            cp2.Load();
            Assert.True(cp2.Loaded);
            Assert.Equal("upload-id-abc", cp2.UploadId);

            cp2.Remove();
            Assert.False(File.Exists(cp.CpFilePath));
        }
        finally
        {
            Directory.Delete(dir, true);
        }
    }

    [Fact]
    public void TestUploadCheckpointInvalidWhenSizeChanged()
    {
        var dir = Path.Combine(Path.GetTempPath(), "ossucp-" + Guid.NewGuid());
        Directory.CreateDirectory(dir);
        try
        {
            var cp = UploadCheckpoint.Create(
                "bucket", "key", "/data/file.bin", dir,
                123456, "2026-01-01T00:00:00Z", Defaults.DefaultUploadPartSize);
            cp.UploadId = "upload-id-abc";
            cp.Dump();

            // A checkpoint created for a different file size must not load.
            var cp2 = UploadCheckpoint.Create(
                "bucket", "key", "/data/file.bin", dir,
                999999, "2026-01-01T00:00:00Z", Defaults.DefaultUploadPartSize);
            cp2.Load();
            Assert.False(cp2.Loaded);
        }
        finally
        {
            Directory.Delete(dir, true);
        }
    }

    [Fact]
    public void TestUploadCheckpointCorruptRemoved()
    {
        var dir = Path.Combine(Path.GetTempPath(), "ossucp-" + Guid.NewGuid());
        Directory.CreateDirectory(dir);
        try
        {
            var cp = UploadCheckpoint.Create(
                "bucket", "key", "/data/file.bin", dir,
                123456, "2026-01-01T00:00:00Z", Defaults.DefaultUploadPartSize);
            cp.UploadId = "upload-id-abc";
            cp.Dump();

            // Corrupt the MD5 line so validation fails.
            var text = File.ReadAllText(cp.CpFilePath);
            File.WriteAllText(cp.CpFilePath, text.Replace("MD5=", "MD5=deadbeef"));

            var cp2 = UploadCheckpoint.Create(
                "bucket", "key", "/data/file.bin", dir,
                123456, "2026-01-01T00:00:00Z", Defaults.DefaultUploadPartSize);
            cp2.Load();
            Assert.False(cp2.Loaded);
            Assert.False(File.Exists(cp.CpFilePath));
        }
        finally
        {
            Directory.Delete(dir, true);
        }
    }

    [Fact]
    public void TestCrcCombineMatchesFullCompute()
    {
        Crc64.InitECMA();
        var rnd = new Random(1234);
        var data = new byte[10000];
        rnd.NextBytes(data);

        var full = Crc64.Compute(data, 0, data.Length);

        const int split = 3333;
        var crc1 = Crc64.Compute(data, 0, split);
        var crc2 = Crc64.Compute(data, split, data.Length - split);
        var combined = Crc64.Combine(crc1, crc2, data.Length - split);

        Assert.Equal(full, combined);
    }
}
