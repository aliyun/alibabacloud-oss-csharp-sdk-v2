using System.Text;
using AlibabaCloud.OSS.V2.Models;

namespace AlibabaCloud.OSS.V2.IntegrationTests;


public class ClientBucketVersioningTest : IDisposable
{
    private readonly string BucketNamePrefix;

    public void Dispose()
    {
        Utils.CleanBuckets(BucketNamePrefix);
    }

    public ClientBucketVersioningTest()
    {
        BucketNamePrefix = Utils.RandomBucketNamePrefix();
    }

    [Fact]
    public async Task TestPutAndGetBucketVersioning()
    {
        var client = Utils.GetDefaultClient();

        //default
        var bucketName = Utils.RandomBucketName(BucketNamePrefix);

        var result = await client.PutBucketAsync(new()
        {
            Bucket = bucketName
        });
        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.RequestId);

        // get bucket versioning status
        var getResult = await client.GetBucketVersioningAsync(new()
        {
            Bucket = bucketName
        });
        Assert.NotNull(getResult);
        Assert.Equal(200, getResult.StatusCode);
        Assert.NotNull(getResult.RequestId);
        Assert.NotNull(getResult.VersioningConfiguration);
        Assert.Null(getResult.VersioningConfiguration.Status);

        // put bucket versioning status
        var putResult = await client.PutBucketVersioningAsync(new()
        {
            Bucket = bucketName,
            VersioningConfiguration = new VersioningConfiguration
            {
                Status = BucketVersioningStatusType.Enabled.GetString()
            }
        });
        Assert.NotNull(putResult);
        Assert.Equal(200, putResult.StatusCode);
        Assert.NotNull(putResult.RequestId);

        // get bucket versioning status
        getResult = await client.GetBucketVersioningAsync(new()
        {
            Bucket = bucketName
        });
        Assert.NotNull(getResult);
        Assert.Equal(200, getResult.StatusCode);
        Assert.NotNull(getResult.RequestId);
        Assert.NotNull(getResult.VersioningConfiguration);
        Assert.Equal("Enabled", getResult.VersioningConfiguration.Status);
    }

    [Fact]
    public async Task TestPutAndGetBucketVersioningFail()
    {
        var client = Utils.GetDefaultClient();

        //default
        var bucketName = Utils.RandomBucketName(BucketNamePrefix);

        var result = await client.PutBucketAsync(new()
        {
            Bucket = bucketName,
        });

        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.RequestId);

        var invClient = Utils.GetInvalidAkClient();

        try
        {
            await invClient.PutBucketVersioningAsync(new()
            {
                Bucket = bucketName,
                VersioningConfiguration = new VersioningConfiguration
                {
                    Status = BucketVersioningStatusType.Enabled.GetString()
                }
            });
        }
        catch (Exception e)
        {
            Assert.IsAssignableFrom<OperationException>(e);
            Assert.StartsWith("operation error PutBucketVersioning", e.Message);
            Assert.IsAssignableFrom<ServiceException>(e.InnerException);
            var se = e.InnerException as ServiceException;
            Assert.NotNull(se);
            Assert.Equal(403, se.StatusCode);
            Assert.Equal("InvalidAccessKeyId", se.ErrorCode);
            Assert.Equal("The OSS Access Key Id you provided does not exist in our records.", se.ErrorMessage);
            Assert.Equal("0002-00000902", se.Ec);
            Assert.Contains("PUT", se.RequestTarget);
            Assert.Contains("?versioning", se.RequestTarget);
            Assert.Contains("GMT", se.TimeStamp);
        }

        try
        {
            await invClient.GetBucketVersioningAsync(new()
            {
                Bucket = bucketName
            });
        }
        catch (Exception e)
        {
            Assert.IsAssignableFrom<OperationException>(e);
            Assert.StartsWith("operation error GetBucketVersioning", e.Message);
            Assert.IsAssignableFrom<ServiceException>(e.InnerException);
            var se = e.InnerException as ServiceException;
            Assert.NotNull(se);
            Assert.Equal(403, se.StatusCode);
            Assert.Equal("InvalidAccessKeyId", se.ErrorCode);
            Assert.Equal("The OSS Access Key Id you provided does not exist in our records.", se.ErrorMessage);
            Assert.Equal("0002-00000902", se.Ec);
            Assert.Contains("GET", se.RequestTarget);
            Assert.Contains("?versioning", se.RequestTarget);
            Assert.Contains("GMT", se.TimeStamp);
        }

        try
        {
            await invClient.ListObjectVersionsAsync(new()
            {
                Bucket = bucketName
            });
        }
        catch (Exception e)
        {
            Assert.IsAssignableFrom<OperationException>(e);
            Assert.StartsWith("operation error ListObjectVersions", e.Message);
            Assert.IsAssignableFrom<ServiceException>(e.InnerException);
            var se = e.InnerException as ServiceException;
            Assert.NotNull(se);
            Assert.Equal(403, se.StatusCode);
            Assert.Equal("InvalidAccessKeyId", se.ErrorCode);
            Assert.Equal("The OSS Access Key Id you provided does not exist in our records.", se.ErrorMessage);
            Assert.Equal("0002-00000902", se.Ec);
            Assert.Contains("GET", se.RequestTarget);
            Assert.Contains("?versions", se.RequestTarget);
            Assert.Contains("GMT", se.TimeStamp);
        }
    }

    [Fact]
    public async Task TestListObjectVersionsByPaginators()
    {
        var client = Utils.GetDefaultClient();

        //default
        var bucketName = Utils.RandomBucketName(BucketNamePrefix);

        var result = await client.PutBucketAsync(new()
        {
            Bucket = bucketName
        });
        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.RequestId);

        // put bucket versioning status
        var putResult = await client.PutBucketVersioningAsync(new()
        {
            Bucket = bucketName,
            VersioningConfiguration = new VersioningConfiguration
            {
                Status = BucketVersioningStatusType.Enabled.GetString()
            }
        });

        // paginator
        var paginators = client.ListObjectVersionsPaginator(new ListObjectVersionsRequest()
        {
            Bucket = bucketName
        });
        await foreach (var page in paginators.IterPageAsync())
        {
            Assert.NotNull(page.Versions);
            Assert.Empty(page.Versions);
        }

        // put object
        var normalKeyPrefix = "normal/key-";
        for (var i = 0; i < 10; i++)
        {
            await client.PutObjectAsync(new() { Bucket = bucketName, Key = $"{normalKeyPrefix}{i}" });
        }
        var specialKeyPrefix = "special/key-";
        var chars = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0a };
        var charStr = Encoding.UTF8.GetString(chars);
        await client.PutObjectAsync(new() { Bucket = bucketName, Key = $"{specialKeyPrefix}#?+ 123" });
        await client.PutObjectAsync(new() { Bucket = bucketName, Key = $"{specialKeyPrefix}{charStr}123" });

        // list default
        paginators = client.ListObjectVersionsPaginator(new ListObjectVersionsRequest()
        {
            Bucket = bucketName
        });
        var count = 0;
        await foreach (var page in paginators.IterPageAsync())
        {
            Assert.NotNull(page.Versions);
            Assert.NotEmpty(page.Versions);
            count += page.Versions.Count;
        }
        Assert.Equal(12, count);

        // list with prefix and Limit
        paginators = client.ListObjectVersionsPaginator(
            new ListObjectVersionsRequest()
            {
                Bucket = bucketName,
                Prefix = specialKeyPrefix,
            },
            new Paginator.PaginatorOptions()
            {
                Limit = 1
            });
        count = 0;
        await foreach (var page in paginators.IterPageAsync())
        {
            Assert.NotNull(page.Versions);
            Assert.Single(page.Versions);
            Assert.StartsWith(specialKeyPrefix, page.Versions[0].Key);
            Assert.Equal("url", page.EncodingType);
            count += page.Versions.Count;
        }
        Assert.Equal(2, count);

        // list sync
        paginators = client.ListObjectVersionsPaginator(
            new ListObjectVersionsRequest()
            {
                Bucket = bucketName,
                Prefix = normalKeyPrefix,
            },
            new Paginator.PaginatorOptions()
            {
                Limit = 100
            });
        count = 0;
        foreach (var page in paginators.IterPage())
        {
            Assert.NotNull(page.Versions);
            Assert.Equal(10, page.Versions.Count);
            Assert.StartsWith(normalKeyPrefix, page.Versions[0].Key);
            Assert.Equal("url", page.EncodingType);
            count += page.Versions.Count;
        }
        Assert.Equal(10, count);
    }
}
