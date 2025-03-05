using AlibabaCloud.OSS.V2.Extensions;

namespace AlibabaCloud.OSS.V2.UnitTests.Extensions;

public class UriExtensionsTest
{

    [Fact]
    public void TestIsHostIp()
    {
        var uri = new Uri("http://www.test.com");
        Assert.False(uri.IsHostIp());

        uri = new Uri("http://192.168.1.1");
        Assert.True(uri.IsHostIp());

        uri = new Uri("http://192.168.1.1:8080");
        Assert.True(uri.IsHostIp());

        uri = new Uri("http://127.0.0.1");
        Assert.True(uri.IsHostIp());
    }

    [Fact]
    public void TestAppendToQuery()
    {
        var uri = new Uri("http://www.test.com");
        var uri1 = uri.AppendToQuery("param=1");
        Assert.Equal("http://www.test.com/", uri.AbsoluteUri);
        Assert.Equal("http://www.test.com/?param=1", uri1.AbsoluteUri);

        uri = new Uri("http://www.test.com/?key=val");
        uri1 = uri.AppendToQuery("param=1");
        Assert.Equal("http://www.test.com/?key=val", uri.AbsoluteUri);
        Assert.Equal("http://www.test.com/?key=val&param=1", uri1.AbsoluteUri);

        uri = new Uri("http://www.test.com/?key=val");
        uri1 = uri.AppendToQuery(null);
        Assert.Equal("http://www.test.com/?key=val", uri.AbsoluteUri);
        Assert.Equal("http://www.test.com/?key=val", uri1.AbsoluteUri);
    }

    [Fact]
    public void TestAppendToPath()
    {
        var uri = new Uri("http://www.test.com");
        var uri1 = uri.AppendToPath("objectname");
        Assert.Equal("http://www.test.com/", uri.AbsoluteUri);
        Assert.Equal("http://www.test.com/objectname", uri1.AbsoluteUri);

        uri = new Uri("http://www.test.com/key1");
        uri1 = uri.AppendToPath("objectname");
        Assert.Equal("http://www.test.com/key1", uri.AbsoluteUri);
        Assert.Equal("http://www.test.com/key1/objectname", uri1.AbsoluteUri);
    }
    [Fact]
    public void TestReplaceQuery()
    {
        var uri = new Uri("http://www.test.com");
        var uri1 = uri.ReplaceQuery("param=1");
        Assert.Equal("http://www.test.com/", uri.AbsoluteUri);
        Assert.Equal("http://www.test.com/?param=1", uri1.AbsoluteUri);

        uri = new Uri("http://www.test.com/?key=val");
        uri1 = uri.ReplaceQuery("param=1");
        Assert.Equal("http://www.test.com/?key=val", uri.AbsoluteUri);
        Assert.Equal("http://www.test.com/?param=1", uri1.AbsoluteUri);

        uri = new Uri("http://www.test.com/?key=val");
        uri1 = uri.ReplaceQuery(null);
        Assert.Equal("http://www.test.com/?key=val", uri.AbsoluteUri);
        Assert.Equal("http://www.test.com/?key=val", uri1.AbsoluteUri);
    }

    [Fact]
    public void TestGetPath()
    {
        var uri = new Uri("http://www.test.com/key1");
        Assert.Equal("key1", uri.GetPath());

        uri = new Uri("http://www.test.com");
        Assert.Equal("", uri.GetPath());
    }
    [Fact]
    public void TestGetQueryParameters()
    {
        var uri = new Uri("http://www.test.com/?%2Bparam1=value3&%2Bparam2&%7Cparam1=value4&%7Cparam2&param1=value1&param2");
        var queries = uri.GetQueryParameters();

        Assert.Equal("value3", queries["+param1"]);
        Assert.Equal("", queries["+param2"]);
        Assert.Equal("value4", queries["|param1"]);
        Assert.Equal("", queries["|param2"]);
        Assert.Equal("value1", queries["param1"]);
        Assert.Equal("", queries["param2"]);

        uri = new Uri("http://www.test.com/?");
        queries = uri.GetQueryParameters();
        Assert.Empty(queries);

        uri = new Uri("http://www.test.com/");
        queries = uri.GetQueryParameters();
        Assert.Empty(queries);
    }
}
