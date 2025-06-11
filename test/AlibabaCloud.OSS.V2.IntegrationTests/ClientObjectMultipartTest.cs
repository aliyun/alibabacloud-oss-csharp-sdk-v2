using System.Text;
using AlibabaCloud.OSS.V2.Models;

namespace AlibabaCloud.OSS.V2.IntegrationTests;


public class ClientObjectMultipartTest : IDisposable
{
    private readonly string BucketNamePrefix;

    public void Dispose()
    {
        Utils.CleanBuckets(BucketNamePrefix);
    }

    public ClientObjectMultipartTest()
    {
        BucketNamePrefix = Utils.RandomBucketNamePrefix();
    }

    [Fact]
    public async Task TestUploadPart()
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
        var content = "hello world";
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

        // list parts
        var listResult = await client.ListPartsAsync(new ListPartsRequest()
        {
            Bucket = bucketName,
            Key = objectName,
            UploadId = initResult.UploadId,
        });
        Assert.NotNull(listResult);
        Assert.Equal(200, listResult.StatusCode);
        Assert.NotNull(listResult.RequestId);
        Assert.Equal("url", listResult.EncodingType);
        Assert.Equal(objectName, listResult.Key);
        Assert.NotNull(listResult.Parts);
        Assert.Equal(11, listResult.Parts[0].Size);

        // complete
        var cmResult = await client.CompleteMultipartUploadAsync(new()
        {
            Bucket = bucketName,
            Key = objectName,
            UploadId = initResult.UploadId,
            CompleteMultipartUpload = new CompleteMultipartUpload()
            {
                Parts = [
                  new UploadPart(){ ETag = upResult.ETag, PartNumber =1}
                ],
            }
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
        Assert.Equal(content.Length, getObjectResult.ContentLength);
        Assert.Equal("Multipart", getObjectResult.ObjectType);
        Assert.Null(getObjectResult.TaggingCount);
        Assert.Null(getObjectResult.ContentMd5);
        Assert.NotNull(getObjectResult.StorageClass);
        Assert.NotNull(getObjectResult.Metadata);
        Assert.Empty(getObjectResult.Metadata);
        Assert.NotNull(getObjectResult.Body);
        Assert.False(getObjectResult.Body.CanSeek);
        using var reader = new StreamReader(getObjectResult.Body);
        var got = await reader.ReadToEndAsync();
        Assert.Equal(content, got);
    }

    [Fact]
    public async Task TestUploadPartCopy()
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

        var specialKeyPrefix = "special/key-";
        var chars = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0a };
        var charStr = Encoding.UTF8.GetString(chars);
        var srcObjectName = $"{specialKeyPrefix}#?+ 123";
        var dstObjectName = $"{specialKeyPrefix}{charStr}123";
        var content = "hello world";

        var putResult = await client.PutObjectAsync(
            new()
            {
                Bucket = bucketName,
                Key = srcObjectName,
                Body = new MemoryStream(Encoding.UTF8.GetBytes(content))
            }
        );
        Assert.NotNull(putResult);
        Assert.Equal(200, putResult.StatusCode);
        Assert.NotNull(putResult.RequestId);

        // init
        var initResult = await client.InitiateMultipartUploadAsync(new()
        {
            Bucket = bucketName,
            Key = dstObjectName
        }
        );
        Assert.NotNull(initResult);
        Assert.Equal(200, initResult.StatusCode);
        Assert.NotNull(initResult.RequestId);
        Assert.Equal("url", initResult.EncodingType);
        Assert.Equal(dstObjectName, initResult.Key);

        // upload part copy
        var upResult = await client.UploadPartCopyAsync(new()
        {
            Bucket = bucketName,
            Key = dstObjectName,
            PartNumber = 1,
            UploadId = initResult.UploadId,
            SourceKey = srcObjectName,
        });

        Assert.NotNull(upResult);
        Assert.Equal(200, upResult.StatusCode);
        Assert.NotNull(upResult.RequestId);

        // list parts
        var listResult = await client.ListPartsAsync(new ListPartsRequest()
        {
            Bucket = bucketName,
            Key = dstObjectName,
            UploadId = initResult.UploadId,
        });
        Assert.NotNull(listResult);
        Assert.Equal(200, listResult.StatusCode);
        Assert.NotNull(listResult.RequestId);
        Assert.Equal("url", listResult.EncodingType);
        Assert.Equal(dstObjectName, listResult.Key);
        Assert.NotNull(listResult.Parts);
        Assert.Equal(11, listResult.Parts[0].Size);

        // complete
        var cmResult = await client.CompleteMultipartUploadAsync(new()
        {
            Bucket = bucketName,
            Key = dstObjectName,
            UploadId = initResult.UploadId,
            CompleteMultipartUpload = new CompleteMultipartUpload()
            {
                Parts = [
                  new UploadPart(){ ETag = upResult.ETag, PartNumber =1}
                ],
            }
        });
        Assert.NotNull(cmResult);
        Assert.Equal(200, cmResult.StatusCode);
        Assert.NotNull(cmResult.RequestId);
        Assert.Equal("url", cmResult.EncodingType);
        Assert.Equal(dstObjectName, cmResult.Key);

        // get object
        var getObjectResult = await client.GetObjectAsync(
            new()
            {
                Bucket = bucketName,
                Key = dstObjectName
            }
        );
        Assert.NotNull(getObjectResult);
        Assert.Equal(200, getObjectResult.StatusCode);
        Assert.Equal(content.Length, getObjectResult.ContentLength);
        Assert.Equal("Multipart", getObjectResult.ObjectType);
        Assert.Null(getObjectResult.TaggingCount);
        Assert.Null(getObjectResult.ContentMd5);
        Assert.NotNull(getObjectResult.StorageClass);
        Assert.NotNull(getObjectResult.Metadata);
        Assert.Empty(getObjectResult.Metadata);
        Assert.NotNull(getObjectResult.Body);
        Assert.False(getObjectResult.Body.CanSeek);
        using var reader = new StreamReader(getObjectResult.Body);
        var got = await reader.ReadToEndAsync();
        Assert.Equal(content, got);
    }

    [Fact]
    public async Task TestAbortMultipartUpload()
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

        // abort
        var abortResult = await client.AbortMultipartUploadAsync(new()
        {
            Bucket = bucketName,
            Key = objectName,
            UploadId = initResult.UploadId
        });
        Assert.NotNull(abortResult);
        Assert.Equal(204, abortResult.StatusCode);
        Assert.NotNull(abortResult.RequestId);
    }

    [Fact]
    public async Task TestListMultipartUploads()
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

        // init
        for (var i = 0; i < 10; i++)
        {
            for (var j = 0; j < 2; j++)
            {
                var initResult = await client.InitiateMultipartUploadAsync(new()
                {
                    Bucket = bucketName,
                    Key = $"folder-1/{i}"
                });
                Assert.NotNull(initResult);
                Assert.Equal(200, initResult.StatusCode);
                Assert.NotNull(initResult.RequestId);
                Assert.Equal("url", initResult.EncodingType);
            }
        }

        for (var i = 0; i < 10; i++)
        {
            var initResult = await client.InitiateMultipartUploadAsync(new()
            {
                Bucket = bucketName,
                Key = $"folder-2/{i}"
            });
            Assert.NotNull(initResult);
            Assert.Equal(200, initResult.StatusCode);
            Assert.NotNull(initResult.RequestId);
            Assert.Equal("url", initResult.EncodingType);
        }

        // list default
        var paginators = client.ListMultipartUploadsPaginator(new ListMultipartUploadsRequest()
        {
            Bucket = bucketName
        });
        var count = 0;
        await foreach (var page in paginators.IterPageAsync())
        {
            Assert.NotNull(page.Uploads);
            Assert.NotEmpty(page.Uploads);
            count += page.Uploads.Count;
        }
        Assert.Equal(30, count);

        // list with prefix and Limit
        paginators = client.ListMultipartUploadsPaginator(
            new ListMultipartUploadsRequest()
            {
                Bucket = bucketName,
                Prefix = "folder-1/",
            },
            new Paginator.PaginatorOptions()
            {
                Limit = 1
            });
        count = 0;
        await foreach (var page in paginators.IterPageAsync())
        {
            Assert.NotNull(page.Uploads);
            Assert.Single(page.Uploads);
            Assert.StartsWith("folder-1/", page.Uploads[0].Key);
            Assert.Equal("url", page.EncodingType);
            count += page.Uploads.Count;
        }
        Assert.Equal(20, count);

        // list sync
        paginators = client.ListMultipartUploadsPaginator(
            new ListMultipartUploadsRequest()
            {
                Bucket = bucketName,
                Prefix = "folder-2/",
            });
        count = 0;
        foreach (var page in paginators.IterPage())
        {
            Assert.NotNull(page.Uploads);
            Assert.StartsWith("folder-2/", page.Uploads[0].Key);
            Assert.Equal("url", page.EncodingType);
            count += page.Uploads.Count;
        }
        Assert.Equal(10, count);

        //abort multipart upload
        paginators = client.ListMultipartUploadsPaginator(
            new ListMultipartUploadsRequest()
            {
                Bucket = bucketName
            },
            new Paginator.PaginatorOptions()
            {
                Limit = 1
            });
        foreach (var page in paginators.IterPage())
        {
            Assert.NotNull(page.Uploads);
            Assert.Single(page.Uploads);
            var abortResult = await client.AbortMultipartUploadAsync(new()
            {
                Bucket = page.Bucket,
                Key = page.Uploads[0].Key,
                UploadId = page.Uploads[0].UploadId
            });
            Assert.NotNull(abortResult);
            Assert.Equal(204, abortResult.StatusCode);
            Assert.NotNull(abortResult.RequestId);
        }

        var delResult = await client.DeleteBucketAsync(new()
        {
            Bucket = bucketName,
        });

        Assert.Equal(204, delResult.StatusCode);
        Assert.NotNull(delResult.RequestId);
    }

    [Fact]
    public async Task TestListParts()
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

        var content = "hello world";
        // uploadpart
        for (var i = 0; i < 10; i++)
        {
            var upResult = await client.UploadPartAsync(new()
            {
                Bucket = bucketName,
                Key = objectName,
                PartNumber = i + 1,
                UploadId = initResult.UploadId,
                Body = new MemoryStream(Encoding.UTF8.GetBytes(content))
            });
            Assert.NotNull(initResult);
            Assert.Equal(200, initResult.StatusCode);
            Assert.NotNull(initResult.RequestId);
            Assert.Equal("url", initResult.EncodingType);
        }

        // list default
        var paginators = client.ListPartsPaginator(new ListPartsRequest()
        {
            Bucket = bucketName,
            Key = objectName,
            UploadId = initResult.UploadId
        });
        var count = 0;
        await foreach (var page in paginators.IterPageAsync())
        {
            Assert.NotNull(page.Parts);
            Assert.NotEmpty(page.Parts);
            count += page.Parts.Count;
        }
        Assert.Equal(10, count);

        // list Limit
        paginators = client.ListPartsPaginator(
            new ListPartsRequest()
            {
                Bucket = bucketName,
                Key = objectName,
                UploadId = initResult.UploadId
            },
            new Paginator.PaginatorOptions()
            {
                Limit = 1
            });
        count = 0;
        await foreach (var page in paginators.IterPageAsync())
        {
            Assert.NotNull(page.Parts);
            Assert.Single(page.Parts);
            Assert.Equal("url", page.EncodingType);
            Assert.Equal(11, page.Parts[0].Size);
            count += page.Parts.Count;
        }
        Assert.Equal(10, count);

        // list sync
        paginators = client.ListPartsPaginator(
            new ListPartsRequest()
            {
                Bucket = bucketName,
                Key = objectName,
                UploadId = initResult.UploadId
            });
        count = 0;
        foreach (var page in paginators.IterPage())
        {
            Assert.NotNull(page.Parts);
            Assert.Equal("url", page.EncodingType);
            count += page.Parts.Count;
        }
        Assert.Equal(10, count);

        paginators = client.ListPartsPaginator(
        new ListPartsRequest()
        {
            Bucket = bucketName,
            Key = objectName,
            UploadId = initResult.UploadId
        },
        new Paginator.PaginatorOptions()
        {
            Limit = 1
        });
        count = 0;
        foreach (var page in paginators.IterPage())
        {
            Assert.NotNull(page.Parts);
            Assert.Single(page.Parts);
            Assert.Equal("url", page.EncodingType);
            count += page.Parts.Count;
        }
        Assert.Equal(10, count);

        // abort
        var abortResult = await client.AbortMultipartUploadAsync(new()
        {
            Bucket = bucketName,
            Key = objectName,
            UploadId = initResult.UploadId
        });
        Assert.NotNull(abortResult);
        Assert.Equal(204, abortResult.StatusCode);
        Assert.NotNull(abortResult.RequestId);
    }

    [Fact]
    public async Task TestMultipartUploadFail()
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
        var objectName = Utils.RandomObjectName();

        // InitiateMultipartUpload
        try
        {
            await invClient.InitiateMultipartUploadAsync(
                new()
                {
                    Bucket = bucketName,
                    Key = objectName,
                }
            );
        }
        catch (Exception e)
        {
            Assert.IsAssignableFrom<OperationException>(e);
            Assert.StartsWith("operation error InitiateMultipartUpload", e.Message);
            Assert.IsAssignableFrom<ServiceException>(e.InnerException);
            var se = e.InnerException as ServiceException;
            Assert.NotNull(se);
            Assert.Equal(403, se.StatusCode);
            Assert.Equal("InvalidAccessKeyId", se.ErrorCode);
            Assert.Equal("The OSS Access Key Id you provided does not exist in our records.", se.ErrorMessage);
            Assert.Equal("0002-00000902", se.Ec);
            Assert.Contains("POST", se.RequestTarget);
            Assert.Contains("uploads", se.RequestTarget);
            Assert.Contains("encoding-type=url", se.RequestTarget);
            Assert.Contains("GMT", se.TimeStamp);
        }

        // UploadPart
        try
        {
            await invClient.UploadPartAsync(
                new()
                {
                    Bucket = bucketName,
                    Key = objectName,
                    PartNumber = 1,
                    UploadId = "id-123"
                }
            );
        }
        catch (Exception e)
        {
            Assert.IsAssignableFrom<OperationException>(e);
            Assert.StartsWith("operation error UploadPart", e.Message);
            Assert.IsAssignableFrom<ServiceException>(e.InnerException);
            var se = e.InnerException as ServiceException;
            Assert.NotNull(se);
            Assert.Equal(403, se.StatusCode);
            Assert.Equal("InvalidAccessKeyId", se.ErrorCode);
            Assert.Equal("The OSS Access Key Id you provided does not exist in our records.", se.ErrorMessage);
            Assert.Equal("0002-00000902", se.Ec);
            Assert.Contains("PUT", se.RequestTarget);
            Assert.Contains("uploadId=id-123", se.RequestTarget);
            Assert.Contains("partNumber=1", se.RequestTarget);
            Assert.Contains("GMT", se.TimeStamp);
        }

        // UploadPartCopy
        try
        {
            await invClient.UploadPartCopyAsync(
                new()
                {
                    Bucket = bucketName,
                    Key = objectName,
                    PartNumber = 1,
                    UploadId = "id-123",
                    SourceKey = $"{objectName}-src",
                }
            );
        }
        catch (Exception e)
        {
            Assert.IsAssignableFrom<OperationException>(e);
            Assert.StartsWith("operation error UploadPartCopy", e.Message);
            Assert.IsAssignableFrom<ServiceException>(e.InnerException);
            var se = e.InnerException as ServiceException;
            Assert.NotNull(se);
            Assert.Equal(403, se.StatusCode);
            Assert.Equal("InvalidAccessKeyId", se.ErrorCode);
            Assert.Equal("The OSS Access Key Id you provided does not exist in our records.", se.ErrorMessage);
            Assert.Equal("0002-00000902", se.Ec);
            Assert.Contains("PUT", se.RequestTarget);
            Assert.Contains("uploadId=id-123", se.RequestTarget);
            Assert.Contains("partNumber=1", se.RequestTarget);
            Assert.Contains("GMT", se.TimeStamp);
        }

        // CompleteMultipartUpload
        try
        {
            await invClient.CompleteMultipartUploadAsync(
                new()
                {
                    Bucket = bucketName,
                    Key = objectName,
                    UploadId = "id-123"
                }
            );
        }
        catch (Exception e)
        {
            Assert.IsAssignableFrom<OperationException>(e);
            Assert.StartsWith("operation error CompleteMultipartUpload", e.Message);
            Assert.IsAssignableFrom<ServiceException>(e.InnerException);
            var se = e.InnerException as ServiceException;
            Assert.NotNull(se);
            Assert.Equal(403, se.StatusCode);
            Assert.Equal("InvalidAccessKeyId", se.ErrorCode);
            Assert.Equal("The OSS Access Key Id you provided does not exist in our records.", se.ErrorMessage);
            Assert.Equal("0002-00000902", se.Ec);
            Assert.Contains("POST", se.RequestTarget);
            Assert.Contains("uploadId=id-123", se.RequestTarget);
            Assert.Contains("encoding-type=url", se.RequestTarget);
            Assert.Contains("GMT", se.TimeStamp);
        }

        // AbortMultipartUpload
        try
        {
            await invClient.AbortMultipartUploadAsync(
                new()
                {
                    Bucket = bucketName,
                    Key = objectName,
                    UploadId = "id-123"
                }
            );
        }
        catch (Exception e)
        {
            Assert.IsAssignableFrom<OperationException>(e);
            Assert.StartsWith("operation error AbortMultipartUpload", e.Message);
            Assert.IsAssignableFrom<ServiceException>(e.InnerException);
            var se = e.InnerException as ServiceException;
            Assert.NotNull(se);
            Assert.Equal(403, se.StatusCode);
            Assert.Equal("InvalidAccessKeyId", se.ErrorCode);
            Assert.Equal("The OSS Access Key Id you provided does not exist in our records.", se.ErrorMessage);
            Assert.Equal("0002-00000902", se.Ec);
            Assert.Contains("DELETE", se.RequestTarget);
            Assert.Contains("uploadId=id-123", se.RequestTarget);
            Assert.Contains("GMT", se.TimeStamp);
        }

        // ListParts
        try
        {
            await invClient.ListPartsAsync(
                new()
                {
                    Bucket = bucketName,
                    Key = objectName,
                    UploadId = "id-123"
                }
            );
        }
        catch (Exception e)
        {
            Assert.IsAssignableFrom<OperationException>(e);
            Assert.StartsWith("operation error ListParts", e.Message);
            Assert.IsAssignableFrom<ServiceException>(e.InnerException);
            var se = e.InnerException as ServiceException;
            Assert.NotNull(se);
            Assert.Equal(403, se.StatusCode);
            Assert.Equal("InvalidAccessKeyId", se.ErrorCode);
            Assert.Equal("The OSS Access Key Id you provided does not exist in our records.", se.ErrorMessage);
            Assert.Equal("0002-00000902", se.Ec);
            Assert.Contains("GET", se.RequestTarget);
            Assert.Contains("uploadId=id-123", se.RequestTarget);
            Assert.Contains("encoding-type=url", se.RequestTarget);
            Assert.Contains("GMT", se.TimeStamp);
        }

        // ListMultipartUploads
        try
        {
            await invClient.ListMultipartUploadsAsync(
                new()
                {
                    Bucket = bucketName
                }
            );
        }
        catch (Exception e)
        {
            Assert.IsAssignableFrom<OperationException>(e);
            Assert.StartsWith("operation error ListMultipartUploads", e.Message);
            Assert.IsAssignableFrom<ServiceException>(e.InnerException);
            var se = e.InnerException as ServiceException;
            Assert.NotNull(se);
            Assert.Equal(403, se.StatusCode);
            Assert.Equal("InvalidAccessKeyId", se.ErrorCode);
            Assert.Equal("The OSS Access Key Id you provided does not exist in our records.", se.ErrorMessage);
            Assert.Equal("0002-00000902", se.Ec);
            Assert.Contains("GET", se.RequestTarget);
            Assert.Contains("uploads", se.RequestTarget);
            Assert.Contains("encoding-type=url", se.RequestTarget);
            Assert.Contains("GMT", se.TimeStamp);
        }
    }

    [Fact]
    public async Task TestUploadPartWithCrcCheckDisable()
    {
        var cfg = Configuration.LoadDefault();
        cfg.CredentialsProvider = new Credentials.StaticCredentialsProvider(Utils.AccessKeyId, Utils.AccessKeySecret);
        cfg.Region = Utils.Region;
        cfg.Endpoint = Utils.Endpoint;
        cfg.DisableUploadCrc64Check = true;

        using var client = new Client(cfg);

        //default
        var bucketName = Utils.RandomBucketName(BucketNamePrefix);

        var result = await client.PutBucketAsync(new()
        {
            Bucket = bucketName
        });

        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.RequestId);

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
        var content = "hello world";
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
            CompleteMultipartUpload = new CompleteMultipartUpload()
            {
                Parts = [
                  new UploadPart(){ ETag = upResult.ETag, PartNumber =1}
                ],
            }
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
        Assert.Equal(content.Length, getObjectResult.ContentLength);
        Assert.Equal("Multipart", getObjectResult.ObjectType);
        Assert.Null(getObjectResult.TaggingCount);
        Assert.Null(getObjectResult.ContentMd5);
        Assert.NotNull(getObjectResult.StorageClass);
        Assert.NotNull(getObjectResult.Metadata);
        Assert.Empty(getObjectResult.Metadata);
        Assert.NotNull(getObjectResult.Body);
        Assert.False(getObjectResult.Body.CanSeek);
        using var reader = new StreamReader(getObjectResult.Body);
        var got = await reader.ReadToEndAsync();
        Assert.Equal(content, got);
    }

    [Fact]
    public async Task TestUploadPartWithProgress()
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

        long increment = 0;
        long transferred = 0;
        long total = 0;

        ProgressFunc func = (x, y, z) =>
        {
            increment += x;
            transferred = y;
            total = z;
        };

        // upload
        var lenght = 1024 * 100 + 123;
        var content = Utils.GetRandomString(lenght);
        var upResult = await client.UploadPartAsync(new()
        {
            Bucket = bucketName,
            Key = objectName,
            PartNumber = 1,
            UploadId = initResult.UploadId,
            Body = new MemoryStream(Encoding.UTF8.GetBytes(content)),
            ProgressFn = func
        });

        Assert.NotNull(upResult);
        Assert.Equal(200, upResult.StatusCode);
        Assert.NotNull(upResult.RequestId);
        Assert.Equal(lenght, total);
        Assert.Equal(lenght, transferred);
        Assert.Equal(lenght, increment);

        // complete
        var cmResult = await client.CompleteMultipartUploadAsync(new()
        {
            Bucket = bucketName,
            Key = objectName,
            UploadId = initResult.UploadId,
            CompleteMultipartUpload = new CompleteMultipartUpload()
            {
                Parts = [
                  new UploadPart(){ ETag = upResult.ETag, PartNumber =1}
                ],
            }
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
        Assert.Equal(content.Length, getObjectResult.ContentLength);
        Assert.Equal("Multipart", getObjectResult.ObjectType);
        Assert.Null(getObjectResult.TaggingCount);
        Assert.Null(getObjectResult.ContentMd5);
        Assert.NotNull(getObjectResult.StorageClass);
        Assert.NotNull(getObjectResult.Metadata);
        Assert.Empty(getObjectResult.Metadata);
        Assert.NotNull(getObjectResult.Body);
        Assert.False(getObjectResult.Body.CanSeek);
        using var reader = new StreamReader(getObjectResult.Body);
        var got = await reader.ReadToEndAsync();
        Assert.Equal(content, got);
    }
}
