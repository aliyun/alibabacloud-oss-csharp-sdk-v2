namespace AlibabaCloud.OSS.V2.UnitTests;

public class MimeUtilsTest {
    [Fact]
    public void TestGetMimeType() {
        Assert.Equal("text/html", MimeUtils.GetMimeType("demo.html"));
        Assert.Equal("text/html", MimeUtils.GetMimeType("demo.htm"));
        Assert.Equal("text/plain", MimeUtils.GetMimeType("demo.txt"));
        Assert.Equal("", MimeUtils.GetMimeType(""));
        Assert.Equal("application/octet-stream", MimeUtils.GetMimeType("bin", "application/octet-stream"));
    }

    [Fact]
    public void TestGetUserDefindedMimeType() {
        Assert.Equal("", MimeUtils.GetMimeType("demo.my-html"));

        MimeUtils.AddMimeType(".my-html", "text/my-html");
        Assert.Equal("text/my-html", MimeUtils.GetMimeType("demo.my-html"));

        MimeUtils.ClearMimeType();
        Assert.Equal("", MimeUtils.GetMimeType("demo.my-html"));
    }
}