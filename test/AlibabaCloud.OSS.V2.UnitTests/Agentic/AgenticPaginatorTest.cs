#nullable enable
using System.Net;
using AlibabaCloud.OSS.V2.Agentic;
using AlibabaCloud.OSS.V2.Agentic.Models;
using AlibabaCloud.OSS.V2.Credentials;
using AlibabaCloud.OSS.V2.Transport;

namespace AlibabaCloud.OSS.V2.UnitTests.Agentic;

public class AgenticPaginatorTest
{
    private static Configuration NewConfig(MockHttpMessageHandler mock)
    {
        return new Configuration
        {
            Region = "cn-hangzhou",
            AccountId = "123456",
            CredentialsProvider = new AnonymousCredentialsProvider(),
            HttpTransport = new HttpTransport(mock)
        };
    }

    private static HttpResponseMessage OkXml(string body)
    {
        return new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(body)
        };
    }

    private static string ListBucketsPage(string name, string? nextToken, bool truncated)
    {
        var next = nextToken == null ? "" : $"<NextContinuationToken>{nextToken}</NextContinuationToken>";
        return "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" +
            "<ListAgenticBucketsResult>" +
            "<Region>cn-hangzhou</Region>" +
            next +
            $"<IsTruncated>{(truncated ? "true" : "false")}</IsTruncated>" +
            $"<AgenticBuckets><AgenticBucket><Name>{name}</Name></AgenticBucket></AgenticBuckets>" +
            "</ListAgenticBucketsResult>";
    }

    [Fact]
    public async Task TestListAgenticBucketsPaginatorAsync()
    {
        var mock = new MockHttpMessageHandler();
        using var client = new AgenticBucketClient(NewConfig(mock));

        mock.Clear();
        mock.Responses = [
            OkXml(ListBucketsPage("bucket-a", "token-1", true)),
            OkXml(ListBucketsPage("bucket-b", null, false))
        ];

        var paginator = client.ListAgenticBucketsPaginator(new ListAgenticBucketsRequest());
        var names = new List<string>();
        await foreach (var page in paginator.IterPageAsync())
        {
            foreach (var b in page.AgenticBuckets!) names.Add(b.Name!);
        }

        Assert.Equal(new[] { "bucket-a", "bucket-b" }, names);
        Assert.Equal(2, mock.Requests.Count);
        // second request carries the continuation token from the first page
        Assert.Contains("continuation-token=token-1", mock.Requests[1].RequestUri!.Query);
    }

    [Fact]
    public void TestListAgenticBucketsPaginatorSync()
    {
        var mock = new MockHttpMessageHandler();
        using var client = new AgenticBucketClient(NewConfig(mock));

        mock.Clear();
        mock.Responses = [
            OkXml(ListBucketsPage("bucket-a", "token-1", true)),
            OkXml(ListBucketsPage("bucket-b", null, false))
        ];

        var paginator = client.ListAgenticBucketsPaginator(new ListAgenticBucketsRequest());
        var names = new List<string>();
        foreach (var page in paginator.IterPage())
        {
            foreach (var b in page.AgenticBuckets!) names.Add(b.Name!);
        }

        Assert.Equal(new[] { "bucket-a", "bucket-b" }, names);
    }

    [Fact]
    public void TestPaginatorCannotBeReused()
    {
        var mock = new MockHttpMessageHandler();
        using var client = new AgenticBucketClient(NewConfig(mock));

        mock.Clear();
        mock.Responses = [OkXml(ListBucketsPage("bucket-a", null, false))];

        var paginator = client.ListAgenticBucketsPaginator(new ListAgenticBucketsRequest());
        foreach (var _ in paginator.IterPage()) { }

        Assert.Throws<InvalidOperationException>(() =>
        {
            foreach (var _ in paginator.IterPage()) { }
        });
    }

    [Fact]
    public async Task TestListBucketSpacesPaginatorAsync()
    {
        var mock = new MockHttpMessageHandler();
        using var client = new AgenticBucketClient(NewConfig(mock));

        string Page(string name, string? next, bool truncated)
        {
            var nextTok = next == null ? "" : $"<NextContinuationToken>{next}</NextContinuationToken>";
            return "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" +
                "<ListBucketSpacesResult>" +
                nextTok +
                $"<IsTruncated>{(truncated ? "true" : "false")}</IsTruncated>" +
                $"<BucketSpaces><BucketSpace><Name>{name}</Name></BucketSpace></BucketSpaces>" +
                "</ListBucketSpacesResult>";
        }

        mock.Clear();
        mock.Responses = [
            OkXml(Page("space-a", "tok", true)),
            OkXml(Page("space-b", null, false))
        ];

        var paginator = client.ListBucketSpacesPaginator(new ListBucketSpacesRequest { Bucket = "my-agentic" });
        var names = new List<string>();
        await foreach (var page in paginator.IterPageAsync())
        {
            foreach (var s in page.BucketSpaces!) names.Add(s.Name!);
        }

        Assert.Equal(new[] { "space-a", "space-b" }, names);
        Assert.Equal(2, mock.Requests.Count);
        Assert.Contains("continuation-token=tok", mock.Requests[1].RequestUri!.Query);
    }
}
