using System.Text;
using AlibabaCloud.OSS.V2.Models;

namespace AlibabaCloud.OSS.V2.IntegrationTests;

public class ClientObjectTest : IDisposable
{
    private readonly string BucketNamePrefix;

    public void Dispose()
    {
        Utils.CleanBuckets(BucketNamePrefix);
    }

    public ClientObjectTest()
    {
        BucketNamePrefix = Utils.RandomBucketNamePrefix();
    }

    [Fact]
    public async Task TestObjectBasic()
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

        // put object only body
        var objectName = Utils.RandomObjectName();
        const string content = "hello world";

        var putResult = await client.PutObjectAsync(
            new()
            {
                Bucket = bucketName,
                Key = objectName,
                Body = new MemoryStream(Encoding.UTF8.GetBytes("hello world"))
            }
        );

        var exist = await client.IsObjectExistAsync(bucketName, objectName);
        Assert.True(exist);

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
        Assert.Null(headResult.TaggingCount);
        Assert.NotNull(headResult.ContentMd5);
        Assert.NotNull(headResult.StorageClass);
        Assert.NotNull(headResult.Metadata);
        Assert.Empty(headResult.Metadata);
        // default value set by oss server
        Assert.Equal("application/octet-stream", headResult.ContentType);

        // get object metadata
        var getMetaResult = await client.GetObjectMetaAsync(
            new()
            {
                Bucket = bucketName,
                Key = objectName
            }
        );
        Assert.NotNull(getMetaResult);
        Assert.Equal(200, getMetaResult.StatusCode);
        Assert.NotNull(getMetaResult.RequestId);
        Assert.Equal(content.Length, getMetaResult.ContentLength);

        // get object acl
        var getAclResult = await client.GetObjectAclAsync(
            new()
            {
                Bucket = bucketName,
                Key = objectName
            }
        );
        Assert.NotNull(getAclResult);
        Assert.Equal(200, getAclResult.StatusCode);
        Assert.Equal("default", getAclResult.AccessControlPolicy!.AccessControlList!.Grant);

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
        Assert.Equal("Normal", getObjectResult.ObjectType);
        Assert.Null(getObjectResult.TaggingCount);
        Assert.NotNull(getObjectResult.ContentMd5);
        Assert.NotNull(getObjectResult.StorageClass);
        Assert.NotNull(getObjectResult.Metadata);
        Assert.Empty(getObjectResult.Metadata);
        Assert.NotNull(getObjectResult.Body);
        Assert.False(getObjectResult.Body.CanSeek);
        using var reader = new StreamReader(getObjectResult.Body);
        var got = await reader.ReadToEndAsync();
        Assert.Equal(content, got);

        // delete object
        var delObjectResult = await client.DeleteObjectAsync(
            new()
            {
                Bucket = bucketName,
                Key = objectName
            }
        );
        Assert.NotNull(delObjectResult);
        Assert.Equal(204, delObjectResult.StatusCode);

        exist = await client.IsObjectExistAsync(bucketName, objectName);
        Assert.False(exist);
    }

    [Fact]
    public async Task TestPutObjectWithAllProps()
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

        // put object with storage-class, acl, metadata, tagging
        var objectName = Utils.RandomObjectName();
        const string content = "hello world";

        var putResult = await client.PutObjectAsync(
            new()
            {
                Bucket = bucketName,
                Key = objectName,
                StorageClass = "IA",
                Acl = "private",
                ContentDisposition = "1.txt",
                CacheControl = "no-cache",
                ContentEncoding = "deflate",
                Expires = "Wed, 21 Oct 2015 07:28:00 GMT",
                ContentType = "text/txt",
                ContentMd5 = "XrY7u+Ae7tCTyyK7j1rNww==",
                Metadata = new Dictionary<string, string>() {
                    { "key1", "value1" },
                    { "key2", "value2" }
                },
                Tagging = "tag-key1=val1",
                Body = new MemoryStream(Encoding.UTF8.GetBytes("hello world"))
            }
        );

        Assert.NotNull(putResult);
        Assert.Equal(200, putResult.StatusCode);
        Assert.NotNull(putResult.RequestId);

        var exist = await client.IsObjectExistAsync(bucketName, objectName);
        Assert.True(exist);

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
        Assert.Equal("IA", headResult.StorageClass);
        Assert.Equal("Normal", headResult.ObjectType);
        Assert.Equal("text/txt", headResult.ContentType);
        Assert.Equal("1.txt", headResult.ContentDisposition);
        Assert.Equal("deflate", headResult.ContentEncoding);
        Assert.Equal("no-cache", headResult.CacheControl);
        Assert.Equal("Wed, 21 Oct 2015 07:28:00 GMT", headResult.Expires);
        Assert.Equal("XrY7u+Ae7tCTyyK7j1rNww==", headResult.ContentMd5);
        Assert.Equal(1, headResult.TaggingCount);
        Assert.NotNull(headResult.Metadata);
        var metadata = headResult.Metadata;
        Assert.Equal("value1", metadata["key1"]);
        Assert.Equal("value2", metadata["key2"]);

        // get object acl
        var getAclResult = await client.GetObjectAclAsync(
            new()
            {
                Bucket = bucketName,
                Key = objectName
            }
        );
        Assert.NotNull(getAclResult);
        Assert.Equal(200, getAclResult.StatusCode);
        Assert.Equal("private", getAclResult.Acl);

        // get object tagging
        var getTagResult = await client.GetObjectTaggingAsync(
            new()
            {
                Bucket = bucketName,
                Key = objectName
            }
        );
        Assert.NotNull(getTagResult);
        Assert.Equal(200, getTagResult.StatusCode);
        Assert.NotNull(getTagResult.Tagging);
        Assert.NotNull(getTagResult.Tagging.TagSet);
        Assert.NotNull(getTagResult.Tagging.TagSet.Tags);
        Assert.Single(getTagResult.Tagging.TagSet.Tags);
        Assert.Equal("tag-key1", getTagResult.Tagging.TagSet.Tags[0].Key);
        Assert.Equal("val1", getTagResult.Tagging.TagSet.Tags[0].Value);

        // delete object
        var delObjectResult = await client.DeleteObjectAsync(
            new()
            {
                Bucket = bucketName,
                Key = objectName
            }
        );
        Assert.NotNull(delObjectResult);
        Assert.Equal(204, delObjectResult.StatusCode);
    }

    [Fact]
    public async Task TestObjectBasicFail()
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

        var invClient = Utils.GetInvalidAkClient();

        // put object
        try
        {
            await invClient.PutObjectAsync(
                new()
                {
                    Bucket = bucketName,
                    Key = "key"
                }
            );
        }
        catch (Exception e)
        {
            Assert.IsAssignableFrom<OperationException>(e);
            Assert.StartsWith("operation error PutObject", e.Message);
            Assert.IsAssignableFrom<ServiceException>(e.InnerException);
            var se = e.InnerException as ServiceException;
            Assert.NotNull(se);
            Assert.Equal(403, se.StatusCode);
            Assert.Equal("InvalidAccessKeyId", se.ErrorCode);
            Assert.Equal("The OSS Access Key Id you provided does not exist in our records.", se.ErrorMessage);
            Assert.Equal("0002-00000902", se.Ec);
            Assert.Contains("PUT", se.RequestTarget);
            Assert.Contains("GMT", se.TimeStamp);
        }

        // get object
        try
        {
            await invClient.GetObjectAsync(
                new()
                {
                    Bucket = bucketName,
                    Key = "key"
                }
            );
        }
        catch (Exception e)
        {
            Assert.IsAssignableFrom<OperationException>(e);
            Assert.StartsWith("operation error GetObject", e.Message);
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

        // head object
        try
        {
            await invClient.HeadObjectAsync(
                new()
                {
                    Bucket = bucketName,
                    Key = "key"
                }
            );
        }
        catch (Exception e)
        {
            Assert.IsAssignableFrom<OperationException>(e);
            Assert.StartsWith("operation error HeadObject", e.Message);
            Assert.IsAssignableFrom<ServiceException>(e.InnerException);
            var se = e.InnerException as ServiceException;
            Assert.NotNull(se);
            Assert.Equal(403, se.StatusCode);
            Assert.Equal("InvalidAccessKeyId", se.ErrorCode);
            Assert.Equal("The OSS Access Key Id you provided does not exist in our records.", se.ErrorMessage);
            Assert.Equal("0002-00000902", se.Ec);
            Assert.Contains("HEAD", se.RequestTarget);
            Assert.Contains("GMT", se.TimeStamp);
        }

        // get object meta
        try
        {
            await invClient.GetObjectMetaAsync(
                new()
                {
                    Bucket = bucketName,
                    Key = "key"
                }
            );
        }
        catch (Exception e)
        {
            Assert.IsAssignableFrom<OperationException>(e);
            Assert.StartsWith("operation error GetObjectMeta", e.Message);
            Assert.IsAssignableFrom<ServiceException>(e.InnerException);
            var se = e.InnerException as ServiceException;
            Assert.NotNull(se);
            Assert.Equal(403, se.StatusCode);
            Assert.Equal("InvalidAccessKeyId", se.ErrorCode);
            Assert.Equal("The OSS Access Key Id you provided does not exist in our records.", se.ErrorMessage);
            Assert.Equal("0002-00000902", se.Ec);
            Assert.Contains("HEAD", se.RequestTarget);
            Assert.Contains("GMT", se.TimeStamp);
        }

        // delete object
        try
        {
            await invClient.DeleteObjectAsync(
                new()
                {
                    Bucket = bucketName,
                    Key = "key"
                }
            );
        }
        catch (Exception e)
        {
            Assert.IsAssignableFrom<OperationException>(e);
            Assert.StartsWith("operation error DeleteObject", e.Message);
            Assert.IsAssignableFrom<ServiceException>(e.InnerException);
            var se = e.InnerException as ServiceException;
            Assert.NotNull(se);
            Assert.Equal(403, se.StatusCode);
            Assert.Equal("InvalidAccessKeyId", se.ErrorCode);
            Assert.Equal("The OSS Access Key Id you provided does not exist in our records.", se.ErrorMessage);
            Assert.Equal("0002-00000902", se.Ec);
            Assert.Contains("DELETE", se.RequestTarget);
            Assert.Contains("GMT", se.TimeStamp);
        }
    }

    [Fact]
    public async Task TestObjectAcl()
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

        // put object only body
        var objectName = Utils.RandomObjectName();
        const string content = "hello world";

        var putResult = await client.PutObjectAsync(
            new()
            {
                Bucket = bucketName,
                Key = objectName,
                Body = new MemoryStream(Encoding.UTF8.GetBytes(content))
            }
        );
        Assert.NotNull(putResult);
        Assert.Equal(200, putResult.StatusCode);
        Assert.NotNull(putResult.RequestId);

        // get object acl
        var aclResult = await client.GetObjectAclAsync(
            new()
            {
                Bucket = bucketName,
                Key = objectName
            }
        );
        Assert.NotNull(aclResult);
        Assert.Equal(200, aclResult.StatusCode);
        Assert.NotNull(aclResult.RequestId);
        Assert.Equal("default", aclResult.Acl);

        // put object acl
        var putAclResult = await client.PutObjectAclAsync(
            new()
            {
                Bucket = bucketName,
                Key = objectName,
                Acl = "private"
            }
        );
        Assert.NotNull(putAclResult);
        Assert.Equal(200, putAclResult.StatusCode);
        Assert.NotNull(putAclResult.RequestId);

        aclResult = await client.GetObjectAclAsync(
            new()
            {
                Bucket = bucketName,
                Key = objectName
            }
        );
        Assert.NotNull(aclResult);
        Assert.Equal(200, aclResult.StatusCode);
        Assert.NotNull(aclResult.RequestId);
        Assert.Equal("private", aclResult.Acl);
    }

    [Fact]
    public async Task TestObjectAclFail()
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

        var invClient = Utils.GetInvalidAkClient();

        // put object acl
        try
        {
            await invClient.PutObjectAclAsync(
                new()
                {
                    Bucket = bucketName,
                    Key = "key",
                    Acl = "private"
                }
            );
        }
        catch (Exception e)
        {
            Assert.IsAssignableFrom<OperationException>(e);
            Assert.StartsWith("operation error PutObjectAcl", e.Message);
            Assert.IsAssignableFrom<ServiceException>(e.InnerException);
            var se = e.InnerException as ServiceException;
            Assert.NotNull(se);
            Assert.Equal(403, se.StatusCode);
            Assert.Equal("InvalidAccessKeyId", se.ErrorCode);
            Assert.Equal("The OSS Access Key Id you provided does not exist in our records.", se.ErrorMessage);
            Assert.Equal("0002-00000902", se.Ec);
            Assert.Contains("PUT", se.RequestTarget);
            Assert.Contains("?acl", se.RequestTarget);
            Assert.Contains("GMT", se.TimeStamp);
        }

        // get object acl
        try
        {
            await invClient.GetObjectAclAsync(
                new()
                {
                    Bucket = bucketName,
                    Key = "key"
                }
            );
        }
        catch (Exception e)
        {
            Assert.IsAssignableFrom<OperationException>(e);
            Assert.StartsWith("operation error GetObjectAcl", e.Message);
            Assert.IsAssignableFrom<ServiceException>(e.InnerException);
            var se = e.InnerException as ServiceException;
            Assert.NotNull(se);
            Assert.Equal(403, se.StatusCode);
            Assert.Equal("InvalidAccessKeyId", se.ErrorCode);
            Assert.Equal("The OSS Access Key Id you provided does not exist in our records.", se.ErrorMessage);
            Assert.Equal("0002-00000902", se.Ec);
            Assert.Contains("GET", se.RequestTarget);
            Assert.Contains("?acl", se.RequestTarget);
            Assert.Contains("GMT", se.TimeStamp);
        }
    }

    [Fact]
    public async Task TestObjectTagging()
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

        // put object only body
        var objectName = Utils.RandomObjectName();
        const string content = "hello world";

        var putResult = await client.PutObjectAsync(
            new()
            {
                Bucket = bucketName,
                Key = objectName,
                Body = new MemoryStream(Encoding.UTF8.GetBytes(content))
            }
        );
        Assert.NotNull(putResult);
        Assert.Equal(200, putResult.StatusCode);
        Assert.NotNull(putResult.RequestId);

        // get object tagging
        var getTagResult = await client.GetObjectTaggingAsync(
            new()
            {
                Bucket = bucketName,
                Key = objectName
            }
        );
        Assert.NotNull(getTagResult);
        Assert.NotNull(getTagResult.Tagging);
        Assert.NotNull(getTagResult.Tagging.TagSet);
        Assert.NotNull(getTagResult.Tagging.TagSet.Tags);
        Assert.Empty(getTagResult.Tagging.TagSet.Tags);

        // put object tagging
        var putTagResult = await client.PutObjectTaggingAsync(
            new()
            {
                Bucket = bucketName,
                Key = objectName,
                Tagging = new()
                {
                    TagSet = new()
                    {
                        Tags = [new Models.Tag() { Key = "tagK", Value = "tagV" }]
                    }
                }
            }
        );
        Assert.NotNull(putTagResult);
        Assert.Equal(200, putTagResult.StatusCode);
        Assert.NotNull(putTagResult.RequestId);

        // get object tagging again
        getTagResult = await client.GetObjectTaggingAsync(
            new()
            {
                Bucket = bucketName,
                Key = objectName
            }
        );
        Assert.NotNull(getTagResult);
        Assert.Equal(200, getTagResult.StatusCode);
        Assert.NotNull(getTagResult.RequestId);
        Assert.NotNull(getTagResult.Tagging);
        Assert.NotNull(getTagResult.Tagging.TagSet);
        Assert.NotNull(getTagResult.Tagging.TagSet.Tags);
        Assert.Single(getTagResult.Tagging.TagSet.Tags);
        Assert.Equal("tagK", getTagResult.Tagging.TagSet.Tags[0].Key);
        Assert.Equal("tagV", getTagResult.Tagging.TagSet.Tags[0].Value);

        // delete object tagging
        var delTagResult = await client.DeleteObjectTaggingAsync(
            new()
            {
                Bucket = bucketName,
                Key = objectName
            }
        );
        Assert.NotNull(delTagResult);
        Assert.Equal(204, delTagResult.StatusCode);
        Assert.NotNull(delTagResult.RequestId);

        // get object tagging again
        getTagResult = await client.GetObjectTaggingAsync(
            new()
            {
                Bucket = bucketName,
                Key = objectName
            }
        );
        Assert.NotNull(getTagResult);
        Assert.NotNull(getTagResult.Tagging);
        Assert.NotNull(getTagResult.Tagging.TagSet);
        Assert.NotNull(getTagResult.Tagging.TagSet.Tags);
        Assert.Empty(getTagResult.Tagging.TagSet.Tags);
    }

    [Fact]
    public async Task TestObjectTaggingFail()
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

        var invClient = Utils.GetInvalidAkClient();

        // put object tagging
        try
        {
            await invClient.PutObjectTaggingAsync(
                new()
                {
                    Bucket = bucketName,
                    Key = objectName,
                    Tagging = new()
                    {
                        TagSet = new()
                        {
                            Tags = [new Models.Tag() { Key = "tagK", Value = "tagV" }]
                        }
                    }
                }
            );
        }
        catch (Exception e)
        {
            Assert.IsAssignableFrom<OperationException>(e);
            Assert.StartsWith("operation error PutObjectTagging", e.Message);
            Assert.IsAssignableFrom<ServiceException>(e.InnerException);
            var se = e.InnerException as ServiceException;
            Assert.NotNull(se);
            Assert.Equal(403, se.StatusCode);
            Assert.Equal("InvalidAccessKeyId", se.ErrorCode);
            Assert.Equal("The OSS Access Key Id you provided does not exist in our records.", se.ErrorMessage);
            Assert.Equal("0002-00000902", se.Ec);
            Assert.Contains("PUT", se.RequestTarget);
            Assert.Contains("?tagging", se.RequestTarget);
            Assert.Contains("GMT", se.TimeStamp);
        }

        // get object tagging
        try
        {
            await invClient.GetObjectTaggingAsync(
                new()
                {
                    Bucket = bucketName,
                    Key = objectName
                }
            );
        }
        catch (Exception e)
        {
            Assert.IsAssignableFrom<OperationException>(e);
            Assert.StartsWith("operation error GetObjectTagging", e.Message);
            Assert.IsAssignableFrom<ServiceException>(e.InnerException);
            var se = e.InnerException as ServiceException;
            Assert.NotNull(se);
            Assert.Equal(403, se.StatusCode);
            Assert.Equal("InvalidAccessKeyId", se.ErrorCode);
            Assert.Equal("The OSS Access Key Id you provided does not exist in our records.", se.ErrorMessage);
            Assert.Equal("0002-00000902", se.Ec);
            Assert.Contains("GET", se.RequestTarget);
            Assert.Contains("?tagging", se.RequestTarget);
            Assert.Contains("GMT", se.TimeStamp);
        }

        // delete object tagging
        try
        {
            await invClient.DeleteObjectTaggingAsync(
                new()
                {
                    Bucket = bucketName,
                    Key = objectName
                }
            );
        }
        catch (Exception e)
        {
            Assert.IsAssignableFrom<OperationException>(e);
            Assert.StartsWith("operation error DeleteObjectTagging", e.Message);
            Assert.IsAssignableFrom<ServiceException>(e.InnerException);
            var se = e.InnerException as ServiceException;
            Assert.NotNull(se);
            Assert.Equal(403, se.StatusCode);
            Assert.Equal("InvalidAccessKeyId", se.ErrorCode);
            Assert.Equal("The OSS Access Key Id you provided does not exist in our records.", se.ErrorMessage);
            Assert.Equal("0002-00000902", se.Ec);
            Assert.Contains("DELETE", se.RequestTarget);
            Assert.Contains("?tagging", se.RequestTarget);
            Assert.Contains("GMT", se.TimeStamp);
        }
    }

    [Fact]
    public async Task TestObjectSymlink()
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

        // put object only body
        var objectName = Utils.RandomObjectName();
        const string content = "hello world";

        var putResult = await client.PutObjectAsync(
            new()
            {
                Bucket = bucketName,
                Key = objectName,
                Body = new MemoryStream(Encoding.UTF8.GetBytes("hello world"))
            }
        );
        Assert.NotNull(putResult);
        Assert.Equal(200, putResult.StatusCode);
        Assert.NotNull(putResult.RequestId);

        // put object symlink
        var putSymResult = await client.PutSymlinkAsync(
            new()
            {
                Bucket = bucketName,
                Key = $"{objectName}-link",
                SymlinkTarget = objectName
            }
        );
        Assert.NotNull(putSymResult);
        Assert.Equal(200, putSymResult.StatusCode);
        Assert.NotNull(putSymResult.RequestId);

        // get object symlink
        var getSymResult = await client.GetSymlinkAsync(
            new()
            {
                Bucket = bucketName,
                Key = $"{objectName}-link"
            }
        );
        Assert.NotNull(getSymResult);
        Assert.Equal(200, getSymResult.StatusCode);
        Assert.NotNull(getSymResult.RequestId);
        Assert.Equal(objectName, getSymResult.SymlinkTarget);

        // get object by symlink
        var getObjectResult = await client.GetObjectAsync(
            new()
            {
                Bucket = bucketName,
                Key = $"{objectName}-link"
            }
        );
        Assert.NotNull(getObjectResult);
        Assert.Equal(200, getObjectResult.StatusCode);
        Assert.Equal(content.Length, getObjectResult.ContentLength);
        Assert.Equal("Symlink", getObjectResult.ObjectType);
        Assert.Null(getObjectResult.TaggingCount);
        Assert.NotNull(getObjectResult.ContentMd5);
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
    public async Task TestObjectSymlinkFail()
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

        var invClient = Utils.GetInvalidAkClient();

        // get object symlink
        try
        {
            await invClient.GetSymlinkAsync(
                new()
                {
                    Bucket = bucketName,
                    Key = "key"
                }
            );
        }
        catch (Exception e)
        {
            Assert.IsAssignableFrom<OperationException>(e);
            Assert.StartsWith("operation error GetSymlink", e.Message);
            Assert.IsAssignableFrom<ServiceException>(e.InnerException);
            var se = e.InnerException as ServiceException;
            Assert.NotNull(se);
            Assert.Equal(403, se.StatusCode);
            Assert.Equal("InvalidAccessKeyId", se.ErrorCode);
            Assert.Equal("The OSS Access Key Id you provided does not exist in our records.", se.ErrorMessage);
            Assert.Equal("0002-00000902", se.Ec);
            Assert.Contains("GET", se.RequestTarget);
            Assert.Contains("?symlink", se.RequestTarget);
            Assert.Contains("GMT", se.TimeStamp);
        }

        // put object symlink
        try
        {
            await invClient.PutSymlinkAsync(
                new()
                {
                    Bucket = bucketName,
                    Key = "key-link",
                    SymlinkTarget = "key"
                }
            );
        }
        catch (Exception e)
        {
            Assert.IsAssignableFrom<OperationException>(e);
            Assert.StartsWith("operation error PutSymlink", e.Message);
            Assert.IsAssignableFrom<ServiceException>(e.InnerException);
            var se = e.InnerException as ServiceException;
            Assert.NotNull(se);
            Assert.Equal(403, se.StatusCode);
            Assert.Equal("InvalidAccessKeyId", se.ErrorCode);
            Assert.Equal("The OSS Access Key Id you provided does not exist in our records.", se.ErrorMessage);
            Assert.Equal("0002-00000902", se.Ec);
            Assert.Contains("PUT", se.RequestTarget);
            Assert.Contains("?symlink", se.RequestTarget);
            Assert.Contains("GMT", se.TimeStamp);
        }
    }

    [Fact]
    public async Task TestAppendObject()
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

        var objectName = Utils.RandomObjectName();

        // append 1
        var appendResult = await client.AppendObjectAsync(
            new(
            )
            {
                Bucket = bucketName,
                Key = objectName,
                Position = 0,
                Body = new MemoryStream(Encoding.UTF8.GetBytes("hello "))
            }
        );

        Assert.NotNull(appendResult);
        Assert.Equal(200, appendResult.StatusCode);
        Assert.NotNull(appendResult.RequestId);
        Assert.Equal(6, appendResult.NextAppendPosition);

        // append 2
        appendResult = await client.AppendObjectAsync(
            new(
            )
            {
                Bucket = bucketName,
                Key = objectName,
                Position = 6,
                Body = new MemoryStream(Encoding.UTF8.GetBytes("world"))
            }
        );

        Assert.NotNull(appendResult);
        Assert.Equal(200, appendResult.StatusCode);
        Assert.NotNull(appendResult.RequestId);
        Assert.Equal(11, appendResult.NextAppendPosition);

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
        Assert.Equal(11, getObjectResult.ContentLength);
        Assert.Equal("Appendable", getObjectResult.ObjectType);
        Assert.Equal("Standard", getObjectResult.StorageClass);
        Assert.Null(getObjectResult.TaggingCount);
        Assert.Null(getObjectResult.ContentMd5);
        Assert.NotNull(getObjectResult.Metadata);
        Assert.Empty(getObjectResult.Metadata);
        Assert.NotNull(getObjectResult.Body);
        Assert.False(getObjectResult.Body.CanSeek);
        using var reader = new StreamReader(getObjectResult.Body);
        var got = await reader.ReadToEndAsync();
        Assert.Equal("hello world", got);
    }

    [Fact]
    public async Task TestAppendObjectWithAllProps()
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

        var objectName = Utils.RandomObjectName();

        // append 1
        var appendResult = await client.AppendObjectAsync(
            new(
            )
            {
                Bucket = bucketName,
                Key = objectName,
                Position = 0,
                StorageClass = "IA",
                Acl = "private",
                ContentDisposition = "1.txt",
                CacheControl = "no-cache",
                ContentEncoding = "deflate",
                Expires = "Wed, 21 Oct 2015 07:28:00 GMT",
                ContentType = "text/txt",
                ContentMd5 = "XrY7u+Ae7tCTyyK7j1rNww==",
                Metadata = new Dictionary<string, string>() {
                    { "key1", "value1" },
                    { "key2", "value2" }
                },
                Tagging = "tag-key1=val1",
                Body = new MemoryStream(Encoding.UTF8.GetBytes("hello world"))
            }
        );

        Assert.NotNull(appendResult);
        Assert.Equal(200, appendResult.StatusCode);
        Assert.NotNull(appendResult.RequestId);
        Assert.Equal(11, appendResult.NextAppendPosition);

        // append 2
        appendResult = await client.AppendObjectAsync(
            new(
            )
            {
                Bucket = bucketName,
                Key = objectName,
                Position = 11,
                Body = new MemoryStream(Encoding.UTF8.GetBytes(" 123"))
            }
        );

        Assert.NotNull(appendResult);
        Assert.Equal(200, appendResult.StatusCode);
        Assert.NotNull(appendResult.RequestId);
        Assert.Equal(15, appendResult.NextAppendPosition);

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
        Assert.Equal(15, getObjectResult.ContentLength);
        Assert.Equal("Appendable", getObjectResult.ObjectType);
        Assert.Equal("IA", getObjectResult.StorageClass);
        Assert.Equal("text/txt", getObjectResult.ContentType);
        Assert.Equal("1.txt", getObjectResult.ContentDisposition);
        Assert.Equal("deflate", getObjectResult.ContentEncoding);
        Assert.Equal("no-cache", getObjectResult.CacheControl);
        Assert.Equal("Wed, 21 Oct 2015 07:28:00 GMT", getObjectResult.Expires);
        Assert.Null(getObjectResult.ContentMd5);
        Assert.Equal(1, getObjectResult.TaggingCount);
        Assert.NotNull(getObjectResult.Metadata);
        var metadata = getObjectResult.Metadata;
        Assert.Equal("value1", metadata["key1"]);
        Assert.Equal("value2", metadata["key2"]);
        Assert.NotNull(getObjectResult.Body);
        Assert.False(getObjectResult.Body.CanSeek);
        using var reader = new StreamReader(getObjectResult.Body);
        var got = await reader.ReadToEndAsync();
        Assert.Equal("hello world 123", got);
    }

    [Fact]
    public async Task TestAppendObjectFail()
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

        var invClient = Utils.GetInvalidAkClient();

        // append object
        try
        {
            await invClient.AppendObjectAsync(
                new()
                {
                    Bucket = bucketName,
                    Key = "key",
                    Position = 0,
                    Body = new MemoryStream(Encoding.UTF8.GetBytes(" 123"))
                }
            );
        }
        catch (Exception e)
        {
            Assert.IsAssignableFrom<OperationException>(e);
            Assert.StartsWith("operation error AppendObject", e.Message);
            Assert.IsAssignableFrom<ServiceException>(e.InnerException);
            var se = e.InnerException as ServiceException;
            Assert.NotNull(se);
            Assert.Equal(403, se.StatusCode);
            Assert.Equal("InvalidAccessKeyId", se.ErrorCode);
            Assert.Equal("The OSS Access Key Id you provided does not exist in our records.", se.ErrorMessage);
            Assert.Equal("0002-00000902", se.Ec);
            Assert.Contains("POST", se.RequestTarget);
            Assert.Contains("?append", se.RequestTarget);
            Assert.Contains("GMT", se.TimeStamp);
        }
    }

    [Fact]
    public async Task TestCopyObject()
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

        // put object only body
        var objectName = Utils.RandomObjectName();
        const string content = "hello world";

        var putResult = await client.PutObjectAsync(
            new()
            {
                Bucket = bucketName,
                Key = objectName,
                Body = new MemoryStream(Encoding.UTF8.GetBytes("hello world"))
            }
        );
        Assert.NotNull(putResult);
        Assert.Equal(200, putResult.StatusCode);
        Assert.NotNull(putResult.RequestId);

        // copy object
        var copyResult = await client.CopyObjectAsync(
            new()
            {
                Bucket = bucketName,
                Key = $"{objectName}-copy",
                SourceKey = objectName
            }
        );
        Assert.NotNull(copyResult);
        Assert.Equal(200, copyResult.StatusCode);
        Assert.NotNull(copyResult.RequestId);

        // get object by symlink
        var getObjectResult = await client.GetObjectAsync(
            new()
            {
                Bucket = bucketName,
                Key = $"{objectName}-copy"
            }
        );
        Assert.NotNull(getObjectResult);
        Assert.Equal(200, getObjectResult.StatusCode);
        Assert.Equal(content.Length, getObjectResult.ContentLength);
        Assert.Equal("Normal", getObjectResult.ObjectType);
        Assert.Null(getObjectResult.TaggingCount);
        Assert.NotNull(getObjectResult.ContentMd5);
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
    public async Task TestCopyObjectFail()
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

        var invClient = Utils.GetInvalidAkClient();

        // copy object
        try
        {
            await invClient.CopyObjectAsync(
                new()
                {
                    Bucket = bucketName,
                    Key = "key-copy",
                    SourceKey = "key"
                }
            );
        }
        catch (Exception e)
        {
            Assert.IsAssignableFrom<OperationException>(e);
            Assert.StartsWith("operation error CopyObject", e.Message);
            Assert.IsAssignableFrom<ServiceException>(e.InnerException);
            var se = e.InnerException as ServiceException;
            Assert.NotNull(se);
            Assert.Equal(403, se.StatusCode);
            Assert.Equal("InvalidAccessKeyId", se.ErrorCode);
            Assert.Equal("The OSS Access Key Id you provided does not exist in our records.", se.ErrorMessage);
            Assert.Equal("0002-00000902", se.Ec);
            Assert.Contains("PUT", se.RequestTarget);
            Assert.Contains("GMT", se.TimeStamp);
        }
    }

    [Fact]
    public async Task TestRestoreObject()
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

        // put object only body
        var objectName = Utils.RandomObjectName();
        var objectName1 = objectName + "-1";

        const string content = "hello world";

        var putResult = await client.PutObjectAsync(
            new()
            {
                Bucket = bucketName,
                Key = objectName,
                StorageClass = StorageClassType.ColdArchive.GetString(),
                Body = new MemoryStream(Encoding.UTF8.GetBytes(content))
            }
        );
        Assert.NotNull(putResult);
        Assert.Equal(200, putResult.StatusCode);
        Assert.NotNull(putResult.RequestId);

        putResult = await client.PutObjectAsync(
            new()
            {
                Bucket = bucketName,
                Key = objectName1,
                StorageClass = StorageClassType.ColdArchive.GetString(),
                Body = new MemoryStream(Encoding.UTF8.GetBytes("hello world 123"))
            }
        );
        Assert.NotNull(putResult);
        Assert.Equal(200, putResult.StatusCode);
        Assert.NotNull(putResult.RequestId);

        // restore object
        var resResult = await client.RestoreObjectAsync(
            new()
            {
                Bucket = bucketName,
                Key = objectName,
            }
        );
        Assert.NotNull(resResult);
        Assert.Equal(202, resResult.StatusCode);
        Assert.NotNull(resResult.RequestId);
        Assert.Equal("Standard", resResult.RestorePriority);

        // restore object
        resResult = await client.RestoreObjectAsync(
            new()
            {
                Bucket = bucketName,
                Key = objectName1,
                RestoreRequest = new RestoreRequest()
                {
                    Days = 7,
                    JobParameters = new JobParameters()
                    {
                        Tier = "Bulk"
                    }
                }
            }
        );
        Assert.NotNull(resResult);
        Assert.Equal(202, resResult.StatusCode);
        Assert.NotNull(resResult.RequestId);
        Assert.Equal("Bulk", resResult.RestorePriority);

        // restore  not finished object
        try
        {
            await client.RestoreObjectAsync(
                new()
                {
                    Bucket = bucketName,
                    Key = objectName,
                }
            );
            Assert.Fail("should not here");
        }
        catch (Exception e)
        {
            Assert.IsAssignableFrom<OperationException>(e);
            Assert.StartsWith("operation error RestoreObject", e.Message);
            Assert.IsAssignableFrom<ServiceException>(e.InnerException);
            var se = e.InnerException as ServiceException;
            Assert.NotNull(se);
            Assert.Equal(409, se.StatusCode);
            Assert.Equal("RestoreAlreadyInProgress", se.ErrorCode);
            Assert.Equal("The restore operation is in progress.", se.ErrorMessage);
            Assert.Equal("0016-00000701", se.Ec);
            Assert.Contains("POST", se.RequestTarget);
            Assert.Contains("?restore", se.RequestTarget);
            Assert.Contains("GMT", se.TimeStamp);
        }


        // clean restored object
        try
        {
            await client.CleanRestoredObjectAsync(
                new()
                {
                    Bucket = bucketName,
                    Key = objectName,
                }
            );
            Assert.Fail("should not here");
        }
        catch (Exception e)
        {
            Assert.IsAssignableFrom<OperationException>(e);
            Assert.StartsWith("operation error CleanRestoredObject", e.Message);
            Assert.IsAssignableFrom<ServiceException>(e.InnerException);
            var se = e.InnerException as ServiceException;
            Assert.NotNull(se);
            Assert.Equal(409, se.StatusCode);
            Assert.Equal("ArchiveRestoreNotFinished", se.ErrorCode);
            Assert.Equal("The archive file's restore is not finished.", se.ErrorMessage);
            Assert.Equal("0016-00000719", se.Ec);
            Assert.Contains("POST", se.RequestTarget);
            Assert.Contains("?cleanRestoredObject", se.RequestTarget);
            Assert.Contains("GMT", se.TimeStamp);
        }
    }


    [Fact]
    public async Task TestAppendObjectWithCrcCheckEnable()
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

        var objectName = Utils.RandomObjectName();

        // crc OK
        // append 1
        var appendResult = await client.AppendObjectAsync(
            new(
            )
            {
                Bucket = bucketName,
                Key = objectName,
                Position = 0,
                Body = new MemoryStream(Encoding.UTF8.GetBytes("hello ")),
                InitHashCrc64 = "0",
            }
        );

        Assert.NotNull(appendResult);
        Assert.Equal(200, appendResult.StatusCode);
        Assert.NotNull(appendResult.RequestId);
        Assert.Equal(6, appendResult.NextAppendPosition);

        // append 2
        appendResult = await client.AppendObjectAsync(
            new(
            )
            {
                Bucket = bucketName,
                Key = objectName,
                Position = 6,
                Body = new MemoryStream(Encoding.UTF8.GetBytes("world")),
                InitHashCrc64 = appendResult.HashCrc64
            }
        );

        Assert.NotNull(appendResult);
        Assert.Equal(200, appendResult.StatusCode);
        Assert.NotNull(appendResult.RequestId);
        Assert.Equal(11, appendResult.NextAppendPosition);

        // crc Fail
        objectName = Utils.RandomObjectName();
        try
        {
            appendResult = await client.AppendObjectAsync(
                new(
                )
                {
                    Bucket = bucketName,
                    Key = objectName,
                    Position = 0,
                    Body = new MemoryStream(Encoding.UTF8.GetBytes("hello ")),
                    InitHashCrc64 = "1",
                }
            );
            Assert.Fail("should not here");
        }
        catch (Exception e)
        {
            Assert.IsAssignableFrom<OperationException>(e);
            Assert.StartsWith("operation error AppendObject", e.Message);
            Assert.IsAssignableFrom<NoRetryableInconsistentException>(e.InnerException);
            Assert.Contains("crc is inconsistent, client", e.ToString());
        }
    }

    [Fact]
    public async Task TestAppendObjectWithCrcCheckDisable()
    {
        var cfg = Configuration.LoadDefault();
        cfg.CredentialsProvider = new Credentials.StaticCredentialsProvide(Utils.AccessKeyId, Utils.AccessKeySecret);
        cfg.Region = Utils.Region;
        cfg.Endpoint = Utils.Endpoint;
        cfg.DisableUploadCrc64Check = true;

        using var client = new Client(cfg);

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

        var objectName = Utils.RandomObjectName();

        // invalid crc, but ok
        var appendResult = await client.AppendObjectAsync(
            new(
            )
            {
                Bucket = bucketName,
                Key = objectName,
                Position = 0,
                Body = new MemoryStream(Encoding.UTF8.GetBytes("hello ")),
                InitHashCrc64 = "1",
            }
        );

        Assert.NotNull(appendResult);
        Assert.Equal(200, appendResult.StatusCode);
        Assert.NotNull(appendResult.RequestId);
        Assert.Equal(6, appendResult.NextAppendPosition);
    }

    [Fact]
    public async Task TestPutObjectWithCrcCheckDisable()
    {
        var cfg = Configuration.LoadDefault();
        cfg.CredentialsProvider = new Credentials.StaticCredentialsProvide(Utils.AccessKeyId, Utils.AccessKeySecret);
        cfg.Region = Utils.Region;
        cfg.Endpoint = Utils.Endpoint;
        cfg.DisableUploadCrc64Check = true;

        using var client = new Client(cfg);

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

        var objectName = Utils.RandomObjectName();

        var putResult = await client.PutObjectAsync(
            new(
            )
            {
                Bucket = bucketName,
                Key = objectName,
                Body = new MemoryStream(Encoding.UTF8.GetBytes("hello ")),
            }
        );

        Assert.NotNull(putResult);
        Assert.Equal(200, putResult.StatusCode);
        Assert.NotNull(putResult.RequestId);
    }

    [Fact]
    public async Task TestPutObjectWithProgress()
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

        var objectName = Utils.RandomObjectName();
        long increment = 0;
        long transferred = 0;
        long total = 0;

        ProgressFunc func = (x, y, z) =>
        {
            increment += x;
            transferred = y;
            total = z;
        };

        var lenght = 1024 * 100 + 123;
        var content = Utils.GetRandomString(lenght);

        var putResult = await client.PutObjectAsync(
            new(
            )
            {
                Bucket = bucketName,
                Key = objectName,
                Body = new MemoryStream(Encoding.UTF8.GetBytes(content)),
                ProgressFn = func
            }
        );

        Assert.NotNull(putResult);
        Assert.Equal(200, putResult.StatusCode);
        Assert.NotNull(putResult.RequestId);
        Assert.Equal(lenght, total);
        Assert.Equal(lenght, transferred);
        Assert.Equal(lenght, increment);

        // append 1
        increment = 0;
        transferred = 0;
        total = 0;
        objectName = Utils.RandomObjectName();
        var appendResult = await client.AppendObjectAsync(
            new(
            )
            {
                Bucket = bucketName,
                Key = objectName,
                Position = 0,
                Body = new MemoryStream(Encoding.UTF8.GetBytes(content)),
                ProgressFn = func
            }
        );

        Assert.NotNull(appendResult);
        Assert.Equal(200, appendResult.StatusCode);
        Assert.NotNull(appendResult.RequestId);
        Assert.Equal(lenght, appendResult.NextAppendPosition);
        Assert.Equal(lenght, total);
        Assert.Equal(lenght, transferred);
        Assert.Equal(lenght, increment);
    }

    [Fact]
    public async Task TestGetObjectWithCrcCheckEnable()
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

        //null body
        var objectName = Utils.RandomObjectName();
        var putResult = await client.PutObjectAsync(
            new(
            )
            {
                Bucket = bucketName,
                Key = objectName
            }
        );

        Assert.NotNull(putResult);
        Assert.Equal(200, putResult.StatusCode);
        Assert.NotNull(putResult.RequestId);

        var getResult = await client.GetObjectAsync(
            new(
            )
            {
                Bucket = bucketName,
                Key = objectName
            }
        );

        Assert.NotNull(getResult);
        Assert.Equal(200, getResult.StatusCode);
        Assert.NotNull(getResult.RequestId);
        Assert.Equal("0", getResult.HashCrc64);

        //emtpy body
        objectName = Utils.RandomObjectName();
        putResult = await client.PutObjectAsync(
            new(
            )
            {
                Bucket = bucketName,
                Key = objectName,
                Body = new MemoryStream()
            }
        );

        Assert.NotNull(putResult);
        Assert.Equal(200, putResult.StatusCode);
        Assert.NotNull(putResult.RequestId);

        getResult = await client.GetObjectAsync(
            new(
            )
            {
                Bucket = bucketName,
                Key = objectName
            }
        );

        Assert.NotNull(getResult);
        Assert.Equal(200, getResult.StatusCode);
        Assert.NotNull(getResult.RequestId);
        Assert.Equal("0", getResult.HashCrc64);

        //non emtpy body
        objectName = Utils.RandomObjectName();
        putResult = await client.PutObjectAsync(
            new(
            )
            {
                Bucket = bucketName,
                Key = objectName,
                Body = new MemoryStream(Encoding.UTF8.GetBytes("hello "))
            }
        );

        Assert.NotNull(putResult);
        Assert.Equal(200, putResult.StatusCode);
        Assert.NotNull(putResult.RequestId);

        getResult = await client.GetObjectAsync(
            new(
            )
            {
                Bucket = bucketName,
                Key = objectName
            }
        );

        Assert.NotNull(getResult);
        Assert.Equal(200, getResult.StatusCode);
        Assert.NotNull(getResult.RequestId);
        Assert.Equal(6, getResult.ContentLength);
        Assert.Equal("1796661072844795914", getResult.HashCrc64);

        //range get
        getResult = await client.GetObjectAsync(
            new(
            )
            {
                Bucket = bucketName,
                Key = objectName,
                Range = "bytes=1-5"
            }
        );

        Assert.NotNull(getResult);
        Assert.Equal(206, getResult.StatusCode);
        Assert.NotNull(getResult.RequestId);
        Assert.Equal(5, getResult.ContentLength);
        Assert.Equal("1796661072844795914", getResult.HashCrc64);

        //range get
        getResult = await client.GetObjectAsync(
            new(
            )
            {
                Bucket = bucketName,
                Key = objectName,
                Range = "bytes=0-"
            }
        );

        Assert.NotNull(getResult);
        Assert.Equal(206, getResult.StatusCode);
        Assert.NotNull(getResult.RequestId);
        Assert.Equal(6, getResult.ContentLength);
        Assert.Equal("1796661072844795914", getResult.HashCrc64);
    }

    [Fact]
    public async Task TestGetObjectWithCrcCheckDisable()
    {
        var cfg = Configuration.LoadDefault();
        cfg.CredentialsProvider = new Credentials.StaticCredentialsProvide(Utils.AccessKeyId, Utils.AccessKeySecret);
        cfg.Region = Utils.Region;
        cfg.Endpoint = Utils.Endpoint;
        cfg.DisableDownloadCrc64Check = true;

        using var client = new Client(cfg);

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

        //non emtpy body
        var objectName = Utils.RandomObjectName();
        var putResult = await client.PutObjectAsync(
            new(
            )
            {
                Bucket = bucketName,
                Key = objectName,
                Body = new MemoryStream(Encoding.UTF8.GetBytes("hello "))
            }
        );

        Assert.NotNull(putResult);
        Assert.Equal(200, putResult.StatusCode);
        Assert.NotNull(putResult.RequestId);

        var getResult = await client.GetObjectAsync(
            new(
            )
            {
                Bucket = bucketName,
                Key = objectName
            }
        );

        Assert.NotNull(getResult);
        Assert.Equal(200, getResult.StatusCode);
        Assert.NotNull(getResult.RequestId);
        Assert.Equal("1796661072844795914", getResult.HashCrc64);
    }
}
