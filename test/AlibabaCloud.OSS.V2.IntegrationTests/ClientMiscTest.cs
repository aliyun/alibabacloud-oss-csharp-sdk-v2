using System.Text;
using AlibabaCloud.OSS.V2.Models;
using AlibabaCloud.OSS.V2.IO;

namespace AlibabaCloud.OSS.V2.IntegrationTests;


public class ClientMiscTest : IDisposable
{
    private readonly string BucketNamePrefix;
    private readonly string RootPath;

    public void Dispose()
    {
        Utils.CleanBuckets(BucketNamePrefix);
        Utils.CleanPath(RootPath);
    }

    public ClientMiscTest()
    {
        BucketNamePrefix = Utils.RandomBucketNamePrefix();
        RootPath = $"{Utils.GetTempPath()}{Path.DirectorySeparatorChar}";
        Directory.CreateDirectory(RootPath);
    }

    [Fact]
    public async Task TestClientDispose()
    {
        using var client = Utils.GetClient(Utils.Region, Utils.Endpoint);

        var result = await client.DescribeRegionsAsync(new DescribeRegionsRequest());
        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.RequestId);
        Assert.NotNull(result.RegionInfoList);
        Assert.NotNull(result.RegionInfoList.RegionInfos);
        Assert.NotEmpty(result.RegionInfoList.RegionInfos);
        var found = false;
        foreach (var region in result.RegionInfoList.RegionInfos)
        {
            if (region.Region == "oss-cn-hangzhou")
            {
                found = true;
                Assert.Equal("oss-cn-hangzhou.aliyuncs.com", region.InternetEndpoint);
                Assert.Equal("oss-cn-hangzhou-internal.aliyuncs.com", region.InternalEndpoint);
                Assert.Equal("oss-accelerate.aliyuncs.com", region.AccelerateEndpoint);
            }
        }
        Assert.True(found);
    }

    [Fact]
    public async Task TestPaginatorReused()
    {
        var client = Utils.GetDefaultClient();

        //default
        var bucketName = Utils.RandomBucketName(BucketNamePrefix);
        var objectName = Utils.RandomObjectName();

        var result = await client.PutBucketAsync(
            new()
            {
                Bucket = bucketName
            }
        );

        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.RequestId);

        //ListBucketsPaginator
        var paginators = client.ListBucketsPaginator(
            new ListBucketsRequest()
            {
                Prefix = BucketNamePrefix
            });
        await foreach (var bucket in paginators.IterPageAsync())
        {
            Assert.NotNull(bucket);
        }

        // reuse and fail
        try
        {
            await foreach (var bucket in paginators.IterPageAsync())
            {
                Assert.NotNull(bucket);
            }
            Assert.Fail("should not here");
        }
        catch (Exception ex)
        {
            Assert.Contains("Paginator has already been consumed and cannot be reused. Please create a new instance.", ex.ToString());
        }

        try
        {
            foreach (var bucket in paginators.IterPage())
            {
                Assert.NotNull(bucket);
            }
            Assert.Fail("should not here");
        }
        catch (Exception ex)
        {
            Assert.Contains("Paginator has already been consumed and cannot be reused. Please create a new instance.", ex.ToString());
        }

        //ListMultipartUploadsPaginator
        var paginators1 = client.ListMultipartUploadsPaginator(
            new ListMultipartUploadsRequest()
            {
                Bucket = bucketName
            });
        await foreach (var bucket in paginators1.IterPageAsync())
        {
            Assert.NotNull(bucket);
        }

        // reuse and fail
        try
        {
            await foreach (var bucket in paginators1.IterPageAsync())
            {
                Assert.NotNull(bucket);
            }
            Assert.Fail("should not here");
        }
        catch (Exception ex)
        {
            Assert.Contains("Paginator has already been consumed and cannot be reused. Please create a new instance.", ex.ToString());
        }

        try
        {
            foreach (var bucket in paginators1.IterPage())
            {
                Assert.NotNull(bucket);
            }
            Assert.Fail("should not here");
        }
        catch (Exception ex)
        {
            Assert.Contains("Paginator has already been consumed and cannot be reused. Please create a new instance.", ex.ToString());
        }

        //ListObjectsPaginator
        var paginators2 = client.ListObjectsPaginator(
            new ListObjectsRequest()
            {
                Bucket = bucketName
            });
        await foreach (var page in paginators2.IterPageAsync())
        {
            Assert.NotNull(page);
        }

        // reuse and fail
        try
        {
            await foreach (var page in paginators2.IterPageAsync())
            {
                Assert.NotNull(page);
            }
            Assert.Fail("should not here");
        }
        catch (Exception ex)
        {
            Assert.Contains("Paginator has already been consumed and cannot be reused. Please create a new instance.", ex.ToString());
        }

        try
        {
            foreach (var page in paginators2.IterPage())
            {
                Assert.NotNull(page);
            }
            Assert.Fail("should not here");
        }
        catch (Exception ex)
        {
            Assert.Contains("Paginator has already been consumed and cannot be reused. Please create a new instance.", ex.ToString());
        }

        //ListObjectsV2Paginator
        var paginators3 = client.ListObjectsV2Paginator(
            new ListObjectsV2Request()
            {
                Bucket = bucketName
            });
        await foreach (var page in paginators3.IterPageAsync())
        {
            Assert.NotNull(page);
        }

        // reuse and fail
        try
        {
            await foreach (var page in paginators3.IterPageAsync())
            {
                Assert.NotNull(page);
            }
            Assert.Fail("should not here");
        }
        catch (Exception ex)
        {
            Assert.Contains("Paginator has already been consumed and cannot be reused. Please create a new instance.", ex.ToString());
        }

        try
        {
            foreach (var page in paginators3.IterPage())
            {
                Assert.NotNull(page);
            }
            Assert.Fail("should not here");
        }
        catch (Exception ex)
        {
            Assert.Contains("Paginator has already been consumed and cannot be reused. Please create a new instance.", ex.ToString());
        }

        //ListObjectVersionsPaginator
        var paginators4 = client.ListObjectVersionsPaginator(
            new ListObjectVersionsRequest()
            {
                Bucket = bucketName
            });
        await foreach (var page in paginators4.IterPageAsync())
        {
            Assert.NotNull(page);
        }

        // reuse and fail
        try
        {
            await foreach (var page in paginators4.IterPageAsync())
            {
                Assert.NotNull(page);
            }
            Assert.Fail("should not here");
        }
        catch (Exception ex)
        {
            Assert.Contains("Paginator has already been consumed and cannot be reused. Please create a new instance.", ex.ToString());
        }

        try
        {
            foreach (var page in paginators4.IterPage())
            {
                Assert.NotNull(page);
            }
            Assert.Fail("should not here");
        }
        catch (Exception ex)
        {
            Assert.Contains("Paginator has already been consumed and cannot be reused. Please create a new instance.", ex.ToString());
        }
    }

    [Fact]
    public async Task TestListPartsPaginatorReused()
    {
        var client = Utils.GetDefaultClient();

        //default
        var bucketName = Utils.RandomBucketName(BucketNamePrefix);
        var objectName = Utils.RandomObjectName();

        var result = await client.PutBucketAsync(
            new()
            {
                Bucket = bucketName
            }
        );

        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.RequestId);

        //ListPartsPaginator
        var initResult = await client.InitiateMultipartUploadAsync(new()
        {
            Bucket = bucketName,
            Key = objectName
        });
        Assert.NotNull(initResult);
        Assert.Equal(200, initResult.StatusCode);
        Assert.NotNull(initResult.RequestId);
        Assert.Equal("url", initResult.EncodingType);

        var paginators5 = client.ListPartsPaginator(
            new ListPartsRequest()
            {
                Bucket = bucketName,
                Key = objectName,
                UploadId = initResult.UploadId
            });
        await foreach (var page in paginators5.IterPageAsync())
        {
            Assert.NotNull(page);
        }

        // reuse and fail
        try
        {
            await foreach (var page in paginators5.IterPageAsync())
            {
                Assert.NotNull(page);
            }
            Assert.Fail("should not here");
        }
        catch (Exception ex)
        {
            Assert.Contains("Paginator has already been consumed and cannot be reused. Please create a new instance.", ex.ToString());
        }

        try
        {
            foreach (var page in paginators5.IterPage())
            {
                Assert.NotNull(page);
            }
            Assert.Fail("should not here");
        }
        catch (Exception ex)
        {
            Assert.Contains("Paginator has already been consumed and cannot be reused. Please create a new instance.", ex.ToString());
        }
    }

    [Fact]
    public async Task TestInvokeOperation()
    {
        var client = Utils.GetDefaultClient();

        //default
        var bucketName = Utils.RandomBucketName(BucketNamePrefix);
        var objectName = Utils.RandomObjectName();

        var result = await client.PutBucketAsync(
            new()
            {
                Bucket = bucketName
            }
        );

        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.RequestId);

        // get bucket acl
        var res = await client.InvokeOperationAsync(
            new OperationInput
            {
                OperationName = "GetBucketAcl",
                Method = "GET",
                Parameters = new Dictionary<string, string> {
                    { "acl", "" }
                },
                Bucket = bucketName
            }
        );
        Assert.NotNull(res);
        Assert.Equal(200, res.StatusCode);
        Assert.NotNull(res.Body);
        Assert.True(res.Body.CanSeek);
        res.Body.Close();
    }

    [Fact]
    public async Task TestInvokeOperationAsStream()
    {
        var client = Utils.GetDefaultClient();

        //default
        var bucketName = Utils.RandomBucketName(BucketNamePrefix);
        var objectName = Utils.RandomObjectName();

        var result = await client.PutBucketAsync(
            new()
            {
                Bucket = bucketName
            }
        );

        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.RequestId);

        // get bucket acl
        var res = await client.InvokeOperationAsync(
            new OperationInput
            {
                OperationName = "GetBucketAcl",
                Method = "GET",
                Parameters = new Dictionary<string, string> {
                    { "acl", "" }
                },
                Bucket = bucketName,
                OperationMetadata = new Dictionary<string, object> {
                    { "http-completion-option", HttpCompletionOption.ResponseHeadersRead}
                }
            }
        );
        Assert.NotNull(res);
        Assert.Equal(200, res.StatusCode);
        Assert.NotNull(res.Body);
        Assert.False(res.Body.CanSeek);
    }

    [Fact]
    public async Task TestIsObjectExistFail()
    {
        var client = Utils.GetDefaultClient();

        var bucketName = Utils.RandomBucketName(BucketNamePrefix);

        try
        {
            await client.IsObjectExistAsync("", "key");
            Assert.Fail("should not here");
        }
        catch (Exception e)
        {
            Assert.Contains("input.Bucket name is invalid", e.ToString());
        }

        try
        {
            await client.IsObjectExistAsync(bucketName, "key");
            Assert.Fail("should not here");
        }
        catch (Exception e)
        {
            Assert.Contains("NoSuchBucket", e.ToString());
        }
    }

    [Fact]
    public async Task TestIsBucketExistFail()
    {
        var client = Utils.GetDefaultClient();

        var bucketName = Utils.RandomBucketName(BucketNamePrefix);

        try
        {
            await client.IsBucketExistAsync("");
            Assert.Fail("should not here");
        }
        catch (Exception e)
        {
            Assert.Contains("input.Bucket name is invalid", e.ToString());
        }

        try
        {
            await client.IsBucketExistAsync("oss-bucket");
            Assert.Fail("should not here");
        }
        catch (Exception e)
        {
            Assert.Contains("The bucket you access does not belong to you.", e.ToString());
        }
    }

    [Fact]
    public async Task TestPutGetObjectByFilePath()
    {
        var client = Utils.GetDefaultClient();

        //default
        var bucketName = Utils.RandomBucketName(BucketNamePrefix);
        var objectName = Utils.RandomObjectName();
        var filepath = Utils.RandomFilePath(RootPath);
        var saveFilepath = Utils.RandomFilePath(RootPath);

        var result = await client.PutBucketAsync(
            new()
            {
                Bucket = bucketName
            }
        );
        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        Utils.PrepareSampleFile(filepath, 100);
        Assert.True(File.Exists(filepath));

        var putResult = await client.PutObjectFromFileAsync(new PutObjectRequest()
        {
            Bucket = bucketName,
            Key = objectName
        }, filepath);

        Assert.NotNull(putResult);
        Assert.Equal(200, putResult.StatusCode);

        var getResult = await client.GetObjectToFileAsync(new GetObjectRequest()
        {
            Bucket = bucketName,
            Key = objectName
        }, saveFilepath);

        Assert.NotNull(putResult);
        Assert.Equal(200, putResult.StatusCode);

        var srcMd5 = Utils.ComputeContentMd5(filepath);
        var dstMd5 = Utils.ComputeContentMd5(saveFilepath);
        Assert.NotEmpty(srcMd5);
        Assert.Equal(srcMd5, dstMd5);
    }

    [Fact]
    public async Task TestInsecureSkipVerifyTrue()
    {
        var cfg = Configuration.LoadDefault();
        cfg.CredentialsProvider = new Credentials.StaticCredentialsProvide(Utils.AccessKeyId, Utils.AccessKeySecret);
        cfg.Region = Utils.Region;
        cfg.InsecureSkipVerify = true;

        using var client = new Client(cfg);

        // without region
        var result = await client.DescribeRegionsAsync(new DescribeRegionsRequest());
        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.RequestId);
        Assert.NotNull(result.RegionInfoList);
        Assert.NotNull(result.RegionInfoList.RegionInfos);
        Assert.NotEmpty(result.RegionInfoList.RegionInfos);
        var found = false;
        foreach (var region in result.RegionInfoList.RegionInfos)
        {
            if (region.Region == "oss-cn-hangzhou")
            {
                found = true;
                Assert.Equal("oss-cn-hangzhou.aliyuncs.com", region.InternetEndpoint);
                Assert.Equal("oss-cn-hangzhou-internal.aliyuncs.com", region.InternalEndpoint);
                Assert.Equal("oss-accelerate.aliyuncs.com", region.AccelerateEndpoint);
            }
        }
        Assert.True(found);
    }

    [Fact]
    public async Task TestAutoDetectMimeType()
    {

        var client = Utils.GetDefaultClient();

        //default
        var bucketName = Utils.RandomBucketName(BucketNamePrefix);

        var result = await client.PutBucketAsync(
            new()
            {
                Bucket = bucketName
            }
        );
        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);

        // put .txt
        var objectName = "test.txt";
        const string content = "hello world";

        var putResult = await client.PutObjectAsync(
            new()
            {
                Bucket = bucketName,
                Key = objectName,
                Body = new MemoryStream(Encoding.UTF8.GetBytes(content))
            }
        );

        // head object
        var headResult = await client.HeadObjectAsync(
            new()
            {
                Bucket = bucketName,
                Key = objectName
            }
        );
        Assert.NotNull(headResult);
        Assert.Equal(200, headResult.StatusCode);
        Assert.NotNull(headResult.RequestId);
        Assert.Equal(content.Length, headResult.ContentLength);
        Assert.Equal("Normal", headResult.ObjectType);
        Assert.Equal("text/plain", headResult.ContentType);

        // append .json
        objectName = "test.json";
        var appendResult = await client.AppendObjectAsync(
            new()
            {
                Bucket = bucketName,
                Key = objectName,
                Position = 0,
                Body = new MemoryStream(Encoding.UTF8.GetBytes(content))
            }
        );

        // head object
        headResult = await client.HeadObjectAsync(
            new()
            {
                Bucket = bucketName,
                Key = objectName
            }
        );
        Assert.NotNull(headResult);
        Assert.Equal(200, headResult.StatusCode);
        Assert.NotNull(headResult.RequestId);
        Assert.Equal(content.Length, headResult.ContentLength);
        Assert.Equal("Appendable", headResult.ObjectType);
        Assert.Equal("application/json", headResult.ContentType);

        // init
        objectName = "test.jpg";
        var initResult = await client.InitiateMultipartUploadAsync(new()
        {
            Bucket = bucketName,
            Key = objectName
        }
        );
        Assert.NotNull(initResult);
        Assert.Equal(200, initResult.StatusCode);

        // upload
        var upResult = await client.UploadPartAsync(new()
        {
            Bucket = bucketName,
            Key = objectName,
            PartNumber = 1,
            UploadId = initResult.UploadId,
            Body = new MemoryStream(Encoding.UTF8.GetBytes(content))
        });

        Assert.NotNull(upResult);
        Assert.Equal(200, upResult.StatusCode);
        Assert.NotNull(upResult.RequestId);

        // complete
        var cmResult = await client.CompleteMultipartUploadAsync(new()
        {
            Bucket = bucketName,
            Key = objectName,
            UploadId = initResult.UploadId,
            CompleteAll = "yes"
        });
        Assert.NotNull(cmResult);
        Assert.Equal(200, cmResult.StatusCode);
        Assert.NotNull(cmResult.RequestId);
        Assert.Equal("url", cmResult.EncodingType);

        // head object
        headResult = await client.HeadObjectAsync(
            new()
            {
                Bucket = bucketName,
                Key = objectName
            }
        );
        Assert.NotNull(headResult);
        Assert.Equal(200, headResult.StatusCode);
        Assert.NotNull(headResult.RequestId);
        Assert.Equal(content.Length, headResult.ContentLength);
        Assert.Equal("Multipart", headResult.ObjectType);
        Assert.Equal("image/jpeg", headResult.ContentType);
    }

    [Fact]
    public async Task TestGetObjectAsStreamOk()
    {
        var client = Utils.GetDefaultClient();

        //default
        var bucketName = Utils.RandomBucketName(BucketNamePrefix);
        var objectName = Utils.RandomObjectName();
        var filepath = Utils.RandomFilePath(RootPath);
        var saveFilepath1 = Utils.RandomFilePath(RootPath);
        var saveFilepath2 = Utils.RandomFilePath(RootPath);

        var result = await client.PutBucketAsync(
            new()
            {
                Bucket = bucketName
            }
        );
        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);

        // 100k
        Utils.PrepareSampleFile(filepath, 100);
        Assert.True(File.Exists(filepath));

        var putResult = await client.PutObjectFromFileAsync(new PutObjectRequest()
        {
            Bucket = bucketName,
            Key = objectName
        }, filepath);

        Assert.NotNull(putResult);
        Assert.Equal(200, putResult.StatusCode);

        var getResult = await client.GetObjectAsync(new GetObjectRequest()
        {
            Bucket = bucketName,
            Key = objectName
        });

        Assert.NotNull(getResult);
        Assert.Equal(200, getResult.StatusCode);
        Assert.NotNull(getResult.Body);
        Assert.False(getResult.Body.CanSeek);

        using var saveStream1 = File.OpenWrite(saveFilepath1);
        await getResult.Body.CopyToAsync(saveStream1);
        saveStream1.Close();

        getResult = await client.GetObjectAsync(new GetObjectRequest()
        {
            Bucket = bucketName,
            Key = objectName
        }, HttpCompletionOption.ResponseContentRead);

        Assert.NotNull(getResult);
        Assert.Equal(200, getResult.StatusCode);
        Assert.NotNull(getResult.Body);
        Assert.True(getResult.Body.CanSeek);

        using var saveStream2 = File.OpenWrite(saveFilepath2);
        await getResult.Body.CopyToAsync(saveStream2);
        saveStream2.Close();

        var srcMd5 = Utils.ComputeContentMd5(filepath);
        var dstMd51 = Utils.ComputeContentMd5(saveFilepath1);
        var dstMd52 = Utils.ComputeContentMd5(saveFilepath2);
        Assert.NotEmpty(srcMd5);
        Assert.Equal(srcMd5, dstMd51);
        Assert.Equal(srcMd5, dstMd52);
    }

    [Fact]
    public async Task TestGetObjectAsStreamFail()
    {
        var client = Utils.GetDefaultClient();

        //default
        var bucketName = Utils.RandomBucketName(BucketNamePrefix);

        try
        {
            var getResult = await client.GetObjectAsync(new GetObjectRequest()
            {
                Bucket = bucketName,
                Key = "key-1"
            });
        }
        catch (Exception e)
        {
            Assert.IsAssignableFrom<OperationException>(e);
            Assert.StartsWith("operation error GetObject", e.Message);
            Assert.IsAssignableFrom<ServiceException>(e.InnerException);
            var se = e.InnerException as ServiceException;
            Assert.NotNull(se);
            Assert.Equal(404, se.StatusCode);
            Assert.Equal("NoSuchBucket", se.ErrorCode);
        }

        try
        {
            var getResult = await client.GetObjectAsync(new GetObjectRequest()
            {
                Bucket = bucketName,
                Key = "key-2"
            });
        }
        catch (Exception e)
        {
            Assert.IsAssignableFrom<OperationException>(e);
            Assert.StartsWith("operation error GetObject", e.Message);
            Assert.IsAssignableFrom<ServiceException>(e.InnerException);
            var se = e.InnerException as ServiceException;
            Assert.NotNull(se);
            Assert.Equal(404, se.StatusCode);
            Assert.Equal("NoSuchBucket", se.ErrorCode);
        }
    }

    [Fact]
    public async Task TestSendRequestWithTimeout()
    {
        var client = Utils.GetDefaultClient();

        //default
        var bucketName = Utils.RandomBucketName(BucketNamePrefix);

        var result = await client.PutBucketAsync(
            new()
            {
                Bucket = bucketName
            }
        );
        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);

        try
        {
            await client.PutObjectAsync(new PutObjectRequest()
            {
                Bucket = bucketName,
                Key = "timeout-2-1",
                Body = new TimeoutReadStream(new MemoryStream(Encoding.UTF8.GetBytes("hello world")), TimeSpan.FromSeconds(2))
            }, new OperationOptions() { ReadWriteTimeout = TimeSpan.FromSeconds(1) });
        }
        catch (OperationException e)
        {
            Assert.StartsWith("operation error PutObject", e.Message);
            Assert.IsAssignableFrom<RequestTimeoutException>(e.InnerException);
            Assert.Contains("The operation was cancelled because it exceeded the configured timeout of 0:00:01", e.Message);
        }

        var putResult = await client.PutObjectAsync(new PutObjectRequest()
        {
            Bucket = bucketName,
            Key = "timeout-2-5",
            Body = new TimeoutReadStream(new MemoryStream(Encoding.UTF8.GetBytes("hello world")), TimeSpan.FromSeconds(2))
        }, new OperationOptions() { ReadWriteTimeout = TimeSpan.FromSeconds(5) });
        Assert.NotNull(putResult);
        Assert.Equal(200, putResult.StatusCode);
    }

    [Fact]
    public async Task TestPutGetObjectByFilePathWithProgress()
    {
        var client = Utils.GetDefaultClient();

        //default
        var bucketName = Utils.RandomBucketName(BucketNamePrefix);
        var objectName = Utils.RandomObjectName();
        var filepath = Utils.RandomFilePath(RootPath);
        var saveFilepath = Utils.RandomFilePath(RootPath);

        var result = await client.PutBucketAsync(
            new()
            {
                Bucket = bucketName
            }
        );
        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        Utils.PrepareSampleFile(filepath, 100);
        Assert.True(File.Exists(filepath));
        var fileInfo = new FileInfo(filepath);

        var putResult = await client.PutObjectFromFileAsync(new PutObjectRequest()
        {
            Bucket = bucketName,
            Key = objectName
        }, filepath);

        Assert.NotNull(putResult);
        Assert.Equal(200, putResult.StatusCode);

        long increment = 0;
        long transferred = 0;
        long total = 0;

        ProgressFunc func = (x, y, z) =>
        {
            increment += x;
            transferred = y;
            total = z;
        };

        var getResult = await client.GetObjectToFileAsync(new GetObjectRequest()
        {
            Bucket = bucketName,
            Key = objectName,
            ProgressFn = func
        }, saveFilepath);

        Assert.NotNull(putResult);
        Assert.Equal(200, putResult.StatusCode);

        var srcMd5 = Utils.ComputeContentMd5(filepath);
        var dstMd5 = Utils.ComputeContentMd5(saveFilepath);
        Assert.NotEmpty(srcMd5);
        Assert.Equal(srcMd5, dstMd5);

        Assert.NotNull(putResult);
        Assert.Equal(200, putResult.StatusCode);
        Assert.NotNull(putResult.RequestId);
        Assert.Equal(fileInfo.Length, total);
        Assert.Equal(fileInfo.Length, transferred);
        Assert.Equal(fileInfo.Length, increment);

        // disable crc
        var cfg = Configuration.LoadDefault();
        cfg.CredentialsProvider = new Credentials.StaticCredentialsProvide(Utils.AccessKeyId, Utils.AccessKeySecret);
        cfg.Region = Utils.Region;
        cfg.Endpoint = Utils.Endpoint;
        cfg.DisableDownloadCrc64Check = true;

        using var client1 = new Client(cfg);

        increment = 0;
        transferred = 0;
        total = 0;

        getResult = await client1.GetObjectToFileAsync(new GetObjectRequest()
        {
            Bucket = bucketName,
            Key = objectName,
            ProgressFn = func
        }, saveFilepath);

        Assert.NotNull(putResult);
        Assert.Equal(200, putResult.StatusCode);
        Assert.Equal(fileInfo.Length, total);
        Assert.Equal(fileInfo.Length, transferred);
        Assert.Equal(fileInfo.Length, increment);
    }

    [Fact]
    public async Task TestQueryWithSpecialChar()
    {
        var client = Utils.GetDefaultClient();

        //default
        var bucketName = Utils.RandomBucketName(BucketNamePrefix);

        var result = await client.PutBucketAsync(
            new()
            {
                Bucket = bucketName
            }
        );

        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.RequestId);

        // only body
        var objectName = "special/char-123+ 456.txt";
        const string content = "hello world, hi oss!";

        var putRequest = new PutObjectRequest()
        {
            Bucket = bucketName,
            Key = objectName,
        };
        putRequest.Parameters.Add("with-plus", "123+456");
        putRequest.Parameters.Add("with-space", "123 456");
        putRequest.Parameters.Add("key-plus+", "value-key1");
        putRequest.Parameters.Add("key space", "value-key2");

        var preResult = client.Presign(putRequest);

        Assert.NotNull(preResult);
        Assert.Empty(preResult.SignedHeaders);
        Assert.Equal("PUT", preResult.Method);
        Assert.NotNull(preResult.Expiration);
        Assert.Contains("with-plus=123%2B456", preResult.Url);
        Assert.Contains("with-space=123%20456", preResult.Url);
        Assert.Contains("special/char-123%2B%20456.txt", preResult.Url);
        Assert.Contains("key-plus%2B=value-key1", preResult.Url);
        Assert.Contains("key%20space=value-key2", preResult.Url);

        using var hc = new HttpClient();
        var httpResult = await hc.PutAsync(preResult.Url, new ByteArrayContent(Encoding.UTF8.GetBytes(content)));
        Assert.NotNull(httpResult);
        Assert.True(httpResult.IsSuccessStatusCode);

        var getRequest = new GetObjectRequest()
        {
            Bucket = bucketName,
            Key = objectName,
        };
        getRequest.Parameters.Add("with-plus", "123+456");
        getRequest.Parameters.Add("with-space", "123 456");
        getRequest.Parameters.Add("key-plus+", "value-key1");
        getRequest.Parameters.Add("key space", "value-key2");

        preResult = client.Presign(getRequest);
        Assert.NotNull(preResult);
        Assert.Empty(preResult.SignedHeaders);
        Assert.Equal("GET", preResult.Method);
        Assert.NotNull(preResult.Expiration);

        httpResult = await hc.GetAsync(preResult.Url);
        Assert.NotNull(httpResult);
        Assert.True(httpResult.IsSuccessStatusCode);
        var stream = await httpResult.Content.ReadAsStreamAsync();
        using var reader = new StreamReader(stream);
        var got = await reader.ReadToEndAsync();
        Assert.Equal(content, got);

        var getResult = await client.GetObjectAsync(getRequest);
        Assert.NotNull(getResult);
        using var reader1 = new StreamReader(getResult.Body);
        var got1 = await reader1.ReadToEndAsync();
        Assert.Equal(content, got1);
    }

    [Fact]
    public async Task TestUploadPartFromFile()
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

        var partSize = 120 * 1024;
        var filepath = Utils.RandomFilePath(RootPath);
        var saveFilepath = Utils.RandomFilePath(RootPath);
        Utils.PrepareSampleFile(filepath, 512);
        Assert.True(File.Exists(filepath));

        // init
        var objectName = Utils.RandomObjectName();
        var initResult = await client.InitiateMultipartUploadAsync(new()
        {
            Bucket = bucketName,
            Key = objectName
        }
        );
        Assert.NotNull(initResult);
        Assert.Equal(200, initResult.StatusCode);
        Assert.NotNull(initResult.RequestId);
        Assert.Equal("url", initResult.EncodingType);

        // upload
        using var file = File.OpenRead(filepath);
        var fileSize = file.Length;
        long partNumber = 1;
        for (int offset = 0; offset < fileSize; offset += partSize)
        {
            var size = (long)Math.Min(partSize, fileSize - offset);
            var upResult = await client.UploadPartAsync(new()
            {
                Bucket = bucketName,
                Key = objectName,
                PartNumber = partNumber,
                UploadId = initResult.UploadId,
                Body = new BoundedStream(file, (long)offset, size)
            });

            partNumber++;

            Assert.NotNull(upResult);
            Assert.Equal(200, upResult.StatusCode);
            Assert.NotNull(upResult.RequestId);
        }

        // complete
        var cmResult = await client.CompleteMultipartUploadAsync(new()
        {
            Bucket = bucketName,
            Key = objectName,
            UploadId = initResult.UploadId,
            CompleteAll = "yes"
        });
        Assert.NotNull(cmResult);
        Assert.Equal(200, cmResult.StatusCode);
        Assert.NotNull(cmResult.RequestId);
        Assert.Equal("url", cmResult.EncodingType);

        // get object
        var getObjectResult = await client.GetObjectAsync(
            new()
            {
                Bucket = bucketName,
                Key = objectName
            }
        );
        Assert.NotNull(getObjectResult);
        Assert.Equal(200, getObjectResult.StatusCode);
        Assert.Equal((long)fileSize, getObjectResult.ContentLength);
        Assert.Equal("Multipart", getObjectResult.ObjectType);
        Assert.Null(getObjectResult.TaggingCount);
        Assert.Null(getObjectResult.ContentMd5);
        Assert.NotNull(getObjectResult.StorageClass);
        Assert.NotNull(getObjectResult.Metadata);
        Assert.Empty(getObjectResult.Metadata);
        Assert.NotNull(getObjectResult.Body);
        Assert.False(getObjectResult.Body.CanSeek);

        using var saveStream = File.OpenWrite(saveFilepath);
        await getObjectResult.Body.CopyToAsync(saveStream);
        saveStream.Close();

        var srcMd5 = Utils.ComputeContentMd5(filepath);
        var dstMd5 = Utils.ComputeContentMd5(saveFilepath);
        Assert.NotEmpty(srcMd5);
        Assert.Equal(srcMd5, dstMd5);
    }
}
