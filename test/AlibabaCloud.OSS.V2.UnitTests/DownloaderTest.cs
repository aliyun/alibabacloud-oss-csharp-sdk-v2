using System;
using System.IO;
using System.Threading.Tasks;
using AlibabaCloud.OSS.V2.Credentials;
using AlibabaCloud.OSS.V2.Internal;
using AlibabaCloud.OSS.V2.Models;

namespace AlibabaCloud.OSS.V2.UnitTests;

public class DownloaderTest
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
        var downloader = new Downloader(client);
        Assert.NotNull(downloader);

        var opt = new DownloaderOptions();
        Assert.Equal(Defaults.DefaultDownloadPartSize, opt.PartSize);
        Assert.Equal(Defaults.DefaultDownloadParallel, opt.ParallelNum);
        Assert.False(opt.EnableCheckpoint);
        Assert.False(opt.VerifyData);
        Assert.True(opt.UseTempFile);
    }

    [Fact]
    public void TestOptionsOverride()
    {
        var client = NewClient();
        var downloader = new Downloader(client, o =>
        {
            o.PartSize = 8 * 1024 * 1024;
            o.ParallelNum = 5;
            o.EnableCheckpoint = true;
            o.CheckpointDir = "/tmp/cp";
            o.VerifyData = true;
            o.UseTempFile = false;
        });
        Assert.NotNull(downloader);
    }

    [Fact]
    public void TestCtorNullClient()
    {
        Assert.Throws<ArgumentNullException>(() => new Downloader((Client)null!));
    }

    [Fact]
    public async Task TestDownloadFileNullRequest()
    {
        var downloader = new Downloader(NewClient());
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            downloader.DownloadFileAsync(null!, "any.txt"));
    }

    [Fact]
    public async Task TestDownloadFileEmptyBucket()
    {
        var downloader = new Downloader(NewClient());
        var req = new GetObjectRequest { Bucket = "", Key = "key" };
        await Assert.ThrowsAsync<ArgumentException>(() =>
            downloader.DownloadFileAsync(req, "any.txt"));
    }

    [Fact]
    public async Task TestDownloadFileEmptyKey()
    {
        var downloader = new Downloader(NewClient());
        var req = new GetObjectRequest { Bucket = "bucket", Key = "" };
        await Assert.ThrowsAsync<ArgumentException>(() =>
            downloader.DownloadFileAsync(req, "any.txt"));
    }

    [Fact]
    public async Task TestDownloadFileEmptyPath()
    {
        var downloader = new Downloader(NewClient());
        var req = new GetObjectRequest { Bucket = "bucket", Key = "key" };
        await Assert.ThrowsAsync<ArgumentException>(() =>
            downloader.DownloadFileAsync(req, ""));
    }

    [Fact]
    public void TestParseRangeValid()
    {
        var r = RangeUtils.ParseRange("bytes=100-199");
        Assert.NotNull(r);
        Assert.Equal(100, r!.Value.Start);
        Assert.Equal(100, r.Value.Count);

        var open = RangeUtils.ParseRange("bytes=100-");
        Assert.NotNull(open);
        Assert.Equal(100, open!.Value.Start);
        Assert.Equal(0, open.Value.Count);
    }

    [Fact]
    public void TestParseRangeInvalid()
    {
        Assert.Null(RangeUtils.ParseRange(null));
        Assert.Null(RangeUtils.ParseRange(""));
        Assert.Null(RangeUtils.ParseRange("100-199"));
        Assert.Null(RangeUtils.ParseRange("bytes=1-2,3-4"));
        Assert.Null(RangeUtils.ParseRange("bytes=abc"));
    }

    [Fact]
    public void TestHttpRangeFormat()
    {
        Assert.Null(new HttpRange(0, 0).Format());
        Assert.Equal("bytes=100-199", new HttpRange(100, 100).Format());
        Assert.Equal("bytes=100-", new HttpRange(100, 0).Format());
    }

    [Fact]
    public void TestSectionWriterWriteAt()
    {
        var path = Path.Combine(Path.GetTempPath(), "osssw-" + Guid.NewGuid());
        try
        {
            using (var fs = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite))
            {
                var writer = new SectionWriter(fs);
                writer.WriteAt(new byte[] { 3, 4 }, 0, 2, 2);
                writer.WriteAt(new byte[] { 1, 2 }, 0, 2, 0);
            }

            var bytes = File.ReadAllBytes(path);
            Assert.Equal(new byte[] { 1, 2, 3, 4 }, bytes);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void TestDownloadCheckpointRoundTrip()
    {
        var dir = Path.Combine(Path.GetTempPath(), "ossdcp-" + Guid.NewGuid());
        Directory.CreateDirectory(dir);
        var filePath = Path.Combine(dir, "out.bin");
        try
        {
            // Offset > 0 requires the destination file to exist and be large enough.
            File.WriteAllBytes(filePath, new byte[Defaults.DefaultDownloadPartSize]);

            var cp = DownloadCheckpoint.Create(
                "bucket", "key", null, null, filePath, dir, false,
                123456, "2026-01-01T00:00:00Z", "\"etag\"", Defaults.DefaultDownloadPartSize);
            cp.Offset = Defaults.DefaultDownloadPartSize;
            cp.Crc64 = 42UL;
            cp.Dump();
            Assert.True(File.Exists(cp.CpFilePath));

            var cp2 = DownloadCheckpoint.Create(
                "bucket", "key", null, null, filePath, dir, false,
                123456, "2026-01-01T00:00:00Z", "\"etag\"", Defaults.DefaultDownloadPartSize);
            cp2.Load();
            Assert.True(cp2.Loaded);
            Assert.Equal(Defaults.DefaultDownloadPartSize, cp2.Offset);
            Assert.Equal(42UL, cp2.Crc64);

            cp2.Remove();
            Assert.False(File.Exists(cp.CpFilePath));
        }
        finally
        {
            Directory.Delete(dir, true);
        }
    }

    [Fact]
    public void TestDownloadCheckpointInvalidWhenFileTooSmall()
    {
        var dir = Path.Combine(Path.GetTempPath(), "ossdcp-" + Guid.NewGuid());
        Directory.CreateDirectory(dir);
        var filePath = Path.Combine(dir, "out.bin");
        try
        {
            var cp = DownloadCheckpoint.Create(
                "bucket", "key", null, null, filePath, dir, false,
                123456, "2026-01-01T00:00:00Z", "\"etag\"", Defaults.DefaultDownloadPartSize);
            cp.Offset = Defaults.DefaultDownloadPartSize;
            cp.Dump();

            // File does not exist yet.
            var cp2 = DownloadCheckpoint.Create(
                "bucket", "key", null, null, filePath, dir, false,
                123456, "2026-01-01T00:00:00Z", "\"etag\"", Defaults.DefaultDownloadPartSize);
            cp2.Load();
            Assert.False(cp2.Loaded);

            // File exists but is smaller than the downloaded length.
            File.WriteAllBytes(filePath, new byte[Defaults.DefaultDownloadPartSize - 1]);
            var cp3 = DownloadCheckpoint.Create(
                "bucket", "key", null, null, filePath, dir, false,
                123456, "2026-01-01T00:00:00Z", "\"etag\"", Defaults.DefaultDownloadPartSize);
            cp3.Load();
            Assert.False(cp3.Loaded);
        }
        finally
        {
            Directory.Delete(dir, true);
        }
    }

    [Fact]
    public void TestDownloadCheckpointInvalidWhenEtagChanged()
    {
        var dir = Path.Combine(Path.GetTempPath(), "ossdcp-" + Guid.NewGuid());
        Directory.CreateDirectory(dir);
        try
        {
            var cp = DownloadCheckpoint.Create(
                "bucket", "key", null, null, "/data/out.bin", dir, false,
                123456, "2026-01-01T00:00:00Z", "\"etag-1\"", Defaults.DefaultDownloadPartSize);
            cp.Offset = Defaults.DefaultDownloadPartSize;
            cp.Dump();

            var cp2 = DownloadCheckpoint.Create(
                "bucket", "key", null, null, "/data/out.bin", dir, false,
                123456, "2026-01-01T00:00:00Z", "\"etag-2\"", Defaults.DefaultDownloadPartSize);
            cp2.Load();
            Assert.False(cp2.Loaded);
        }
        finally
        {
            Directory.Delete(dir, true);
        }
    }

    [Fact]
    public void TestDownloadCheckpointCorruptRemoved()
    {
        var dir = Path.Combine(Path.GetTempPath(), "ossdcp-" + Guid.NewGuid());
        Directory.CreateDirectory(dir);
        try
        {
            var cp = DownloadCheckpoint.Create(
                "bucket", "key", null, null, "/data/out.bin", dir, false,
                123456, "2026-01-01T00:00:00Z", "\"etag\"", Defaults.DefaultDownloadPartSize);
            cp.Offset = Defaults.DefaultDownloadPartSize;
            cp.Dump();

            var text = File.ReadAllText(cp.CpFilePath);
            File.WriteAllText(cp.CpFilePath, text.Replace("MD5=", "MD5=deadbeef"));

            var cp2 = DownloadCheckpoint.Create(
                "bucket", "key", null, null, "/data/out.bin", dir, false,
                123456, "2026-01-01T00:00:00Z", "\"etag\"", Defaults.DefaultDownloadPartSize);
            cp2.Load();
            Assert.False(cp2.Loaded);
            Assert.False(File.Exists(cp.CpFilePath));
        }
        finally
        {
            Directory.Delete(dir, true);
        }
    }
}
