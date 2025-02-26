using AlibabaCloud.OSS.v2.Models;

namespace AlibabaCloud.OSS.v2.UnitTests.Models;

public class ModelEnumsTest {
    [Fact]
    public void TestBucketAclType() {
        Assert.Equal("private", BucketAclType.Private.GetString());
        Assert.Equal("public-read", BucketAclType.PublicRead.GetString());
        Assert.Equal("public-read-write", BucketAclType.PublicReadWrite.GetString());
    }

    [Fact]
    public void TestAccessMonitorStatusType() {
        Assert.Equal("Enabled", AccessMonitorStatusType.Enabled.GetString());
        Assert.Equal("Disabled", AccessMonitorStatusType.Disabled.GetString());
    }

    [Fact]
    public void TestStorageClassType() {
        Assert.Equal("Standard", StorageClassType.Standard.GetString());
        Assert.Equal("IA", StorageClassType.IA.GetString());
        Assert.Equal("Archive", StorageClassType.Archive.GetString());
        Assert.Equal("ColdArchive", StorageClassType.ColdArchive.GetString());
        Assert.Equal("DeepColdArchive", StorageClassType.DeepColdArchive.GetString());
    }

    [Fact]
    public void TestDataRedundancyType() {
        Assert.Equal("LRS", DataRedundancyType.LRS.GetString());
        Assert.Equal("ZRS", DataRedundancyType.ZRS.GetString());
    }

    [Fact]
    public void TestObjectAclType() {
        Assert.Equal("private", ObjectAclType.Private.GetString());
        Assert.Equal("public-read", ObjectAclType.PublicRead.GetString());
        Assert.Equal("public-read-write", ObjectAclType.PublicReadWrite.GetString());
        Assert.Equal("default", ObjectAclType.Default.GetString());
    }

    [Fact]
    public void TestEncodingType() {
        Assert.Equal("url", EncodingType.Url.GetString());
    }

    [Fact]
    public void TestBucketVersioningStatusType() {
        Assert.Equal("Enabled", BucketVersioningStatusType.Enabled.GetString());
        Assert.Equal("Suspended", BucketVersioningStatusType.Suspended.GetString());
    }
}
