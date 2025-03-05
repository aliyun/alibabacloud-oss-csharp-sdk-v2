using AlibabaCloud.OSS.V2.Models;

namespace AlibabaCloud.OSS.V2.IntegrationTests;


public class ClientServiceTest : IDisposable
{
    private readonly string BucketNamePrefix;

    public void Dispose()
    {
        Utils.CleanBuckets(BucketNamePrefix);
    }

    public ClientServiceTest()
    {
        BucketNamePrefix = Utils.RandomBucketNamePrefix();
    }

    [Fact]
    public async Task TestListBuckets()
    {
        var client = Utils.GetDefaultClient();

        var bucketNamePrefix = Utils.RandomBucketName(BucketNamePrefix) + "list-b-";

        for (var i = 0; i < 10; i++)
        {
            var result = await client.PutBucketAsync(new()
            {
                Bucket = $"{bucketNamePrefix}{i}",
            });
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.NotNull(result.RequestId);
        }
        Utils.WaitFor(1);

        //default
        var listResult = await client.ListBucketsAsync(new ListBucketsRequest() { MaxKeys = 50 });
        Assert.NotNull(listResult);
        Assert.Equal(200, listResult.StatusCode);
        Assert.NotNull(listResult.RequestId);
        Assert.Equal(50, listResult.MaxKeys);
        Assert.True(listResult.Buckets.Count > 10);

        //list with prefix
        listResult = await client.ListBucketsAsync(new ListBucketsRequest()
        {
            MaxKeys = 50,
            Prefix = bucketNamePrefix
        });
        Assert.NotNull(listResult);
        Assert.Equal(200, listResult.StatusCode);
        Assert.NotNull(listResult.RequestId);
        Assert.True(listResult.Buckets.Count == 10);
        Assert.Equal($"{bucketNamePrefix}0", listResult.Buckets[0].Name);
    }

    [Fact]
    public async Task TestListBucketsByPaginators()
    {
        var client = Utils.GetDefaultClient();

        var bucketNamePrefix = Utils.RandomBucketName(BucketNamePrefix) + "list-c-";

        for (var i = 0; i < 10; i++)
        {
            var result = await client.PutBucketAsync(new()
            {
                Bucket = $"{bucketNamePrefix}{i}",
            });
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
            Assert.NotNull(result.RequestId);
        }
        Utils.WaitFor(1);

        //default
        var count = 0;
        var paginators = client.ListBucketsPaginator(new ListBucketsRequest());
        await foreach (var bucket in paginators.IterPageAsync())
        {
            Assert.NotNull(bucket);
            Assert.NotNull(bucket.Buckets);
            Assert.NotEmpty(bucket.Buckets);
            count += bucket.Buckets.Count;
        }
        Assert.True(count > 10);

        //list with prefix one by one async
        paginators = client.ListBucketsPaginator(
            new ListBucketsRequest() { Prefix = bucketNamePrefix },
            new Paginator.PaginatorOptions() { Limit = 1 });
        count = 0;
        await foreach (var bucket in paginators.IterPageAsync())
        {
            Assert.NotNull(bucket);
            Assert.NotNull(bucket.Buckets);
            Assert.Single(bucket.Buckets);
            count++;
        }
        Assert.Equal(10, count);

        //default
        paginators = client.ListBucketsPaginator(new ListBucketsRequest());
        count = 0;
        foreach (var bucket in paginators.IterPage())
        {
            Assert.NotNull(bucket);
            Assert.NotNull(bucket.Buckets);
            Assert.NotEmpty(bucket.Buckets);
            count += bucket.Buckets.Count;
        }
        Assert.True(count > 10);

        //list with prefix one by one
        paginators = client.ListBucketsPaginator(
            new ListBucketsRequest() { Prefix = bucketNamePrefix },
            new Paginator.PaginatorOptions() { Limit = 1 });
        count = 0;
        foreach (var bucket in paginators.IterPage())
        {
            Assert.NotNull(bucket);
            Assert.NotNull(bucket.Buckets);
            Assert.Single(bucket.Buckets);
            count++;
        }
        Assert.Equal(10, count);
    }

    [Fact]
    public async Task TestListBucketsFail()
    {
        var invClient = Utils.GetInvalidAkClient();

        try
        {
            await invClient.ListBucketsAsync(new ListBucketsRequest());
        }
        catch (Exception e)
        {
            Assert.IsAssignableFrom<OperationException>(e);
            Assert.StartsWith("operation error ListBuckets", e.Message);
            Assert.IsAssignableFrom<ServiceException>(e.InnerException);
            var se = e.InnerException as ServiceException;
            Assert.NotNull(se);
            Assert.Equal(403, se.StatusCode);
            Assert.Equal("InvalidAccessKeyId", se.ErrorCode);
            Assert.Equal("The OSS Access Key Id you provided does not exist in our records.", se.ErrorMessage);
            Assert.Equal("0002-00000902", se.Ec);
            Assert.Contains("GET", se.RequestTarget);
            Assert.Contains("GMT", se.TimeStamp);
        }
    }

}
