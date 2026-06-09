using System.Net;
using AlibabaCloud.OSS.V2.Credentials;
using AlibabaCloud.OSS.V2.Models;

namespace AlibabaCloud.OSS.V2.UnitTests;

public class PaginatorTest
{
    private static readonly string ListBucketsXml =
        "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" +
        "<ListAllMyBucketsResult>" +
        "<Prefix></Prefix>" +
        "<Marker></Marker>" +
        "<MaxKeys>100</MaxKeys>" +
        "<IsTruncated>false</IsTruncated>" +
        "<Buckets>" +
        "<Bucket><Name>bucket-1</Name></Bucket>" +
        "</Buckets>" +
        "</ListAllMyBucketsResult>";

    private static readonly string ListBucketsXmlPage1 =
        "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" +
        "<ListAllMyBucketsResult>" +
        "<Prefix></Prefix>" +
        "<Marker></Marker>" +
        "<MaxKeys>1</MaxKeys>" +
        "<IsTruncated>true</IsTruncated>" +
        "<NextMarker>bucket-1</NextMarker>" +
        "<Buckets>" +
        "<Bucket><Name>bucket-1</Name></Bucket>" +
        "</Buckets>" +
        "</ListAllMyBucketsResult>";

    private static readonly string ListBucketsXmlPage2 =
        "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" +
        "<ListAllMyBucketsResult>" +
        "<Prefix></Prefix>" +
        "<Marker>bucket-1</Marker>" +
        "<MaxKeys>1</MaxKeys>" +
        "<IsTruncated>false</IsTruncated>" +
        "<Buckets>" +
        "<Bucket><Name>bucket-2</Name></Bucket>" +
        "</Buckets>" +
        "</ListAllMyBucketsResult>";

    private static Client CreateMockClient(MockHttpMessageHandler mockHandler)
    {
        var config = new Configuration()
        {
            Region = "cn-hangzhou",
            CredentialsProvider = new AnonymousCredentialsProvider(),
            HttpTransport = new Transport.HttpTransport(mockHandler),
        };
        return new Client(config);
    }

    private static HttpResponseMessage MakeListBucketsResponse(string xml)
    {
        return new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(xml, System.Text.Encoding.UTF8, "application/xml")
        };
    }

    [Fact]
    public async Task TestListBucketsPaginator_IterPageAsync()
    {
        var mockHandler = new MockHttpMessageHandler();
        var client = CreateMockClient(mockHandler);

        mockHandler.Responses = new List<HttpResponseMessage>
        {
            MakeListBucketsResponse(ListBucketsXml)
        };

        var paginator = client.ListBucketsPaginator(new ListBucketsRequest());
        var count = 0;
        await foreach (var result in paginator.IterPageAsync())
        {
            Assert.NotNull(result);
            Assert.NotNull(result.Buckets);
            Assert.Single(result.Buckets);
            Assert.Equal("bucket-1", result.Buckets[0].Name);
            count++;
        }
        Assert.Equal(1, count);
    }

    [Fact]
    public async Task TestListBucketsPaginator_IterPageAsync_MultiplePages()
    {
        var mockHandler = new MockHttpMessageHandler();
        var client = CreateMockClient(mockHandler);

        mockHandler.Responses = new List<HttpResponseMessage>
        {
            MakeListBucketsResponse(ListBucketsXmlPage1),
            MakeListBucketsResponse(ListBucketsXmlPage2)
        };

        var paginator = client.ListBucketsPaginator(new ListBucketsRequest());
        var count = 0;
        await foreach (var result in paginator.IterPageAsync())
        {
            Assert.NotNull(result);
            Assert.NotNull(result.Buckets);
            Assert.Single(result.Buckets);
            count++;
        }
        Assert.Equal(2, count);
    }

    [Fact]
    public void TestListBucketsPaginator_IterPage()
    {
        var mockHandler = new MockHttpMessageHandler();
        var client = CreateMockClient(mockHandler);

        mockHandler.Responses = new List<HttpResponseMessage>
        {
            MakeListBucketsResponse(ListBucketsXmlPage1),
            MakeListBucketsResponse(ListBucketsXmlPage2)
        };

        var paginator = client.ListBucketsPaginator(new ListBucketsRequest());
        var count = 0;
        foreach (var result in paginator.IterPage())
        {
            Assert.NotNull(result);
            Assert.NotNull(result.Buckets);
            Assert.Single(result.Buckets);
            count++;
        }
        Assert.Equal(2, count);
    }
}
