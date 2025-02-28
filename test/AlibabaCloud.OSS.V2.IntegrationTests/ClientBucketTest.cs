using AlibabaCloud.OSS.V2.Models;
using System.Text;

namespace AlibabaCloud.OSS.V2.IntegrationTests;

public class ClientBucketTest : IDisposable {
    private readonly string BucketNamePrefix;

    public void Dispose() {
        Utils.CleanBuckets(BucketNamePrefix);
    }

    public ClientBucketTest() {
        BucketNamePrefix = Utils.RandomBucketNamePrefix();
    }

    [Fact]
    public async Task TestBucketBasic() {
        var client = Utils.GetDefaultClient();

        //default
        var bucketName = Utils.RandomBucketName(BucketNamePrefix);

        var result = await client.PutBucketAsync(new() {
            Bucket = bucketName,
            CreateBucketConfiguration = new CreateBucketConfiguration() {
                StorageClass = Models.StorageClassType.IA.GetString(),
            }
        });

        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.RequestId);

        var exist = await client.IsBucketExistAsync(bucketName);
        Assert.True(exist);

        // bucket stat
        var stat = await client.GetBucketStatAsync(
            new GetBucketStatRequest() {
                Bucket = bucketName
            }
        );
        Assert.NotNull(stat);
        Assert.Equal(200, stat.StatusCode);
        Assert.NotNull(stat.RequestId);
        Assert.NotNull(stat.BucketStat);
        Assert.Equal(0, stat.BucketStat.ObjectCount);

        // bucket info
        var info = await client.GetBucketInfoAsync(
            new GetBucketInfoRequest() {
                Bucket = bucketName
            }
        );
        Assert.NotNull(info);
        Assert.Equal(200, info.StatusCode);
        Assert.NotNull(info.RequestId);
        Assert.NotNull(info.BucketInfo);
        Assert.Equal("IA", info.BucketInfo.StorageClass);
        Assert.Equal(bucketName, info.BucketInfo.Name);
        Assert.Equal("private", info.BucketInfo.AccessControlList!.Grant);
        Assert.Contains("rg-", info.BucketInfo.ResourceGroupId);
        Assert.Contains($"oss-{Utils.Region}.aliyuncs.com", info.BucketInfo.ExtranetEndpoint);
        Assert.Contains("rg-", info.BucketInfo.ResourceGroupId);

        // bucket location
        var location = await client.GetBucketLocationAsync(
            new GetBucketLocationRequest() {
                Bucket = bucketName
            }
        );
        Assert.NotNull(location);
        Assert.Equal(200, info.StatusCode);
        Assert.NotNull(location.RequestId);
        Assert.NotNull(location.LocationConstraint);
        Assert.Equal($"oss-{Utils.Region}", location.LocationConstraint);

        var result1 = await client.DeleteBucketAsync(new() {
            Bucket = bucketName,
        });

        Assert.Equal(204, result1.StatusCode);
        Assert.NotNull(result1.RequestId);

        Utils.WaitFor(2);

        exist = await client.IsBucketExistAsync(bucketName);
        Assert.False(exist);
    }

    [Fact]
    public async Task TestBucketBasicFail() {
        var client = Utils.GetDefaultClient();

        //default
        var bucketName = Utils.RandomBucketName(BucketNamePrefix);

        var result = await client.PutBucketAsync(new() {
            Bucket = bucketName,
        });

        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.RequestId);

        var invClient = Utils.GetInvalidAkClient();

        // put bucket
        try {
            await invClient.PutBucketAsync(
                new() {
                    Bucket = bucketName,
                }
            );
        }
        catch (Exception e) {
            Assert.IsAssignableFrom<OperationException>(e);
            Assert.StartsWith("operation error PutBucket", e.Message);
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

        // delete bucket
        try {
            await invClient.DeleteBucketAsync(
                new() {
                    Bucket = bucketName,
                }
            );
        }
        catch (Exception e) {
            Assert.IsAssignableFrom<OperationException>(e);
            Assert.StartsWith("operation error DeleteBucket", e.Message);
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

        // bucket stat
        try {
            await invClient.GetBucketStatAsync(
                new() {
                    Bucket = bucketName,
                }
            );
        }
        catch (Exception e) {
            Assert.IsAssignableFrom<OperationException>(e);
            Assert.StartsWith("operation error GetBucketStat", e.Message);
            Assert.IsAssignableFrom<ServiceException>(e.InnerException);
            var se = e.InnerException as ServiceException;
            Assert.NotNull(se);
            Assert.Equal(403, se.StatusCode);
            Assert.Equal("InvalidAccessKeyId", se.ErrorCode);
            Assert.Equal("The OSS Access Key Id you provided does not exist in our records.", se.ErrorMessage);
            Assert.Equal("0002-00000902", se.Ec);
            Assert.Contains("GET", se.RequestTarget);
            Assert.Contains("?stat", se.RequestTarget);
            Assert.Contains("GMT", se.TimeStamp);
        }

        // bucket info
        try {
            await invClient.GetBucketInfoAsync(
                new() {
                    Bucket = bucketName,
                }
            );
        }
        catch (Exception e) {
            Assert.IsAssignableFrom<OperationException>(e);
            Assert.StartsWith("operation error GetBucketInfo", e.Message);
            Assert.IsAssignableFrom<ServiceException>(e.InnerException);
            var se = e.InnerException as ServiceException;
            Assert.NotNull(se);
            Assert.Equal(403, se.StatusCode);
            Assert.Equal("InvalidAccessKeyId", se.ErrorCode);
            Assert.Equal("The OSS Access Key Id you provided does not exist in our records.", se.ErrorMessage);
            Assert.Equal("0002-00000902", se.Ec);
            Assert.Contains("GET", se.RequestTarget);
            Assert.Contains("?bucketInfo", se.RequestTarget);
            Assert.Contains("GMT", se.TimeStamp);
        }

        // bucket location
        try {
            await invClient.GetBucketLocationAsync(
                new() {
                    Bucket = bucketName,
                }
            );
        }
        catch (Exception e) {
            Assert.IsAssignableFrom<OperationException>(e);
            Assert.StartsWith("operation error GetBucketLocation", e.Message);
            Assert.IsAssignableFrom<ServiceException>(e.InnerException);
            var se = e.InnerException as ServiceException;
            Assert.NotNull(se);
            Assert.Equal(403, se.StatusCode);
            Assert.Equal("InvalidAccessKeyId", se.ErrorCode);
            Assert.Equal("The OSS Access Key Id you provided does not exist in our records.", se.ErrorMessage);
            Assert.Equal("0002-00000902", se.Ec);
            Assert.Contains("GET", se.RequestTarget);
            Assert.Contains("?location", se.RequestTarget);
            Assert.Contains("GMT", se.TimeStamp);
        }

        // list objects
        try {
            await invClient.ListObjectsAsync(
                new() {
                    Bucket = bucketName,
                }
            );
        }
        catch (Exception e) {
            Assert.IsAssignableFrom<OperationException>(e);
            Assert.StartsWith("operation error ListObjects", e.Message);
            Assert.IsAssignableFrom<ServiceException>(e.InnerException);
            var se = e.InnerException as ServiceException;
            Assert.NotNull(se);
            Assert.Equal(403, se.StatusCode);
            Assert.Equal("InvalidAccessKeyId", se.ErrorCode);
            Assert.Equal("The OSS Access Key Id you provided does not exist in our records.", se.ErrorMessage);
            Assert.Equal("0002-00000902", se.Ec);
            Assert.Contains("GET", se.RequestTarget);
            Assert.Contains("encoding-type=url", se.RequestTarget);
            Assert.Contains("GMT", se.TimeStamp);
        }

        // list objects v2
        try {
            await invClient.ListObjectsV2Async(
                new() {
                    Bucket = bucketName,
                }
            );
        }
        catch (Exception e) {
            Assert.IsAssignableFrom<OperationException>(e);
            Assert.StartsWith("operation error ListObjectsV2", e.Message);
            Assert.IsAssignableFrom<ServiceException>(e.InnerException);
            var se = e.InnerException as ServiceException;
            Assert.NotNull(se);
            Assert.Equal(403, se.StatusCode);
            Assert.Equal("InvalidAccessKeyId", se.ErrorCode);
            Assert.Equal("The OSS Access Key Id you provided does not exist in our records.", se.ErrorMessage);
            Assert.Equal("0002-00000902", se.Ec);
            Assert.Contains("GET", se.RequestTarget);
            Assert.Contains("encoding-type=url", se.RequestTarget);
            Assert.Contains("list-type=2", se.RequestTarget);
            Assert.Contains("GMT", se.TimeStamp);
        }
    }

    [Fact]
    public async Task TestBucketAcl() {
        var client = Utils.GetDefaultClient();

        //default
        var bucketName = Utils.RandomBucketName(BucketNamePrefix);

        var result = await client.PutBucketAsync(new() {
            Bucket = bucketName,
        });

        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.RequestId);

        var result1 = await client.GetBucketAclAsync(
            new() {
                Bucket = bucketName
            }
        );
        Assert.NotNull(result1);
        Assert.Equal(200, result1.StatusCode);
        Assert.NotNull(result1.RequestId);
        Assert.NotNull(result1.AccessControlPolicy);
        Assert.NotNull(result1.AccessControlPolicy.AccessControlList);
        Assert.Equal("private", result1.AccessControlPolicy.AccessControlList.Grant);

        var result2 = await client.PutBucketAclAsync(
            new() {
                Bucket = bucketName,
                Acl = "public-read"
            }
        );
        Assert.NotNull(result2);
        Assert.Equal(200, result2.StatusCode);
        Assert.NotNull(result2.RequestId);

        result1 = await client.GetBucketAclAsync(
            new() {
                Bucket = bucketName
            }
        );
        Assert.NotNull(result1);
        Assert.NotNull(result1.AccessControlPolicy);
        Assert.NotNull(result1.AccessControlPolicy.AccessControlList);
        Assert.Equal("public-read", result1.AccessControlPolicy.AccessControlList.Grant);
    }


    [Fact]
    public async Task TestBucketAclFail() {
        var client = Utils.GetDefaultClient();

        //default
        var bucketName = Utils.RandomBucketName(BucketNamePrefix);

        var result = await client.PutBucketAsync(new() {
            Bucket = bucketName,
        });

        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.RequestId);

        var invClient = Utils.GetInvalidAkClient();

        // put bucket acl
        try {
            await invClient.PutBucketAclAsync(
                new() {
                    Bucket = bucketName,
                    Acl = "private"
                }
            );
        }
        catch (Exception e) {
            Assert.IsAssignableFrom<OperationException>(e);
            Assert.StartsWith("operation error PutBucketAcl", e.Message);
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

        // get bucket acl
        try {
            await invClient.GetBucketAclAsync(
                new() {
                    Bucket = bucketName
                }
            );
        }
        catch (Exception e) {
            Assert.IsAssignableFrom<OperationException>(e);
            Assert.StartsWith("operation error GetBucketAcl", e.Message);
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
    public async Task TestListObjectsByPaginators() {
        var client = Utils.GetDefaultClient();

        //default
        var bucketName = Utils.RandomBucketName(BucketNamePrefix);

        var result = await client.PutBucketAsync(new() {
            Bucket = bucketName
        });
        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.RequestId);

        // paginator
        var paginators = client.ListObjectsPaginator(new ListObjectsRequest() {
            Bucket = bucketName
        });
        await foreach (var page in paginators.IterPageAsync()) {
            Assert.NotNull(page.Contents);
            Assert.Empty(page.Contents);
        }

        // put object
        var normalKeyPrefix = "normal/key-";
        for (var i = 0; i < 10; i++) {
            await client.PutObjectAsync(new() { Bucket = bucketName, Key = $"{normalKeyPrefix}{i}" });
        }
        var specialKeyPrefix = "special/key-";
        var chars = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0a };
        var charStr = Encoding.UTF8.GetString(chars);
        await client.PutObjectAsync(new() { Bucket = bucketName, Key = $"{specialKeyPrefix}#?+ 123" });
        await client.PutObjectAsync(new() { Bucket = bucketName, Key = $"{specialKeyPrefix}{charStr}123" });

        // list default
        paginators = client.ListObjectsPaginator(new ListObjectsRequest() {
            Bucket = bucketName
        });
        var count = 0;
        await foreach (var page in paginators.IterPageAsync()) {
            Assert.NotNull(page.Contents);
            Assert.NotEmpty(page.Contents);
            count += page.Contents.Count;
        }
        Assert.Equal(12, count);

        // list with prefix and Limit
        paginators = client.ListObjectsPaginator(
            new ListObjectsRequest() {
                Bucket = bucketName,
                Prefix = specialKeyPrefix,
            },
            new Paginator.PaginatorOptions() {
                Limit = 1
            });
        count = 0;
        await foreach (var page in paginators.IterPageAsync()) {
            Assert.NotNull(page.Contents);
            Assert.Single(page.Contents);
            Assert.StartsWith(specialKeyPrefix, page.Contents[0].Key);
            Assert.Equal("url", page.EncodingType);
            count += page.Contents.Count;
        }
        Assert.Equal(2, count);

        // list sync
        paginators = client.ListObjectsPaginator(
            new ListObjectsRequest() {
                Bucket = bucketName,
                Prefix = normalKeyPrefix,
            });
        count = 0;
        foreach (var page in paginators.IterPage()) {
            Assert.NotNull(page.Contents);
            Assert.Equal(10, page.Contents.Count);
            Assert.StartsWith(normalKeyPrefix, page.Contents[0].Key);
            Assert.Equal("url", page.EncodingType);
            count += page.Contents.Count;
        }
        Assert.Equal(10, count);

        // common prefix
        paginators = client.ListObjectsPaginator(
            new ListObjectsRequest() {
                Bucket = bucketName,
                Delimiter = "/"
            },
            new Paginator.PaginatorOptions() {
                Limit = 100
            });
        count = 0;
        foreach (var page in paginators.IterPage()) {
            Assert.NotNull(page.Contents);
            Assert.Empty(page.Contents);
            Assert.Equal("url", page.EncodingType);
            Assert.NotNull(page.CommonPrefixes);
            Assert.Equal(2, page.CommonPrefixes.Count);
            Assert.Equal("normal/", page.CommonPrefixes[0].Prefix);
            Assert.Equal("special/", page.CommonPrefixes[1].Prefix);
            count++;
        }
        Assert.Equal(1, count);

        // delete all objects
        paginators = client.ListObjectsPaginator(new ListObjectsRequest() { Bucket = bucketName });
        await foreach (var page in paginators.IterPageAsync()) {
            var obj = new List<Models.DeleteObject>();
            foreach (var version in page.Contents ?? []) {
                obj.Add(new DeleteObject() {
                    Key = version.Key
                });
            }
            if (obj.Count > 0) {
                await client.DeleteMultipleObjects(
                    new Models.DeleteMultipleObjectsRequest() {
                        Bucket = bucketName,
                        Objects = obj
                    }
                );
            }
        }

        var delResult = await client.DeleteBucketAsync(new() {
            Bucket = bucketName,
        });

        Assert.Equal(204, delResult.StatusCode);
        Assert.NotNull(delResult.RequestId);
    }

    [Fact]
    public async Task TestListObjectsV2ByPaginators() {
        var client = Utils.GetDefaultClient();

        //default
        var bucketName = Utils.RandomBucketName(BucketNamePrefix);

        var result = await client.PutBucketAsync(new() {
            Bucket = bucketName
        });
        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.RequestId);

        // paginator
        var paginators = client.ListObjectsV2Paginator(new ListObjectsV2Request() {
            Bucket = bucketName
        });
        await foreach (var page in paginators.IterPageAsync()) {
            Assert.NotNull(page.Contents);
            Assert.Empty(page.Contents);
        }

        // put object
        var normalKeyPrefix = "normal/key-";
        for (var i = 0; i < 10; i++) {
            await client.PutObjectAsync(new() { Bucket = bucketName, Key = $"{normalKeyPrefix}{i}" });
        }
        var specialKeyPrefix = "special/key-";
        var chars = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0a };
        var charStr = Encoding.UTF8.GetString(chars);
        await client.PutObjectAsync(new() { Bucket = bucketName, Key = $"{specialKeyPrefix}#?+ 123" });
        await client.PutObjectAsync(new() { Bucket = bucketName, Key = $"{specialKeyPrefix}{charStr}123" });

        // list default
        paginators = client.ListObjectsV2Paginator(new ListObjectsV2Request() {
            Bucket = bucketName
        });
        var count = 0;
        await foreach (var page in paginators.IterPageAsync()) {
            Assert.NotNull(page.Contents);
            Assert.NotEmpty(page.Contents);
            count += page.Contents.Count;
        }
        Assert.Equal(12, count);

        // list with prefix and Limit
        paginators = client.ListObjectsV2Paginator(
            new ListObjectsV2Request() {
                Bucket = bucketName,
                Prefix = specialKeyPrefix,
            },
            new Paginator.PaginatorOptions() {
                Limit = 1
            });
        count = 0;
        await foreach (var page in paginators.IterPageAsync()) {
            Assert.NotNull(page.Contents);
            Assert.Single(page.Contents);
            Assert.StartsWith(specialKeyPrefix, page.Contents[0].Key);
            Assert.Equal("url", page.EncodingType);
            count += page.Contents.Count;
        }
        Assert.Equal(2, count);

        // list sync
        paginators = client.ListObjectsV2Paginator(
            new ListObjectsV2Request() {
                Bucket = bucketName,
                Prefix = normalKeyPrefix,
            });
        count = 0;
        foreach (var page in paginators.IterPage()) {
            Assert.NotNull(page.Contents);
            Assert.Equal(10, page.Contents.Count);
            Assert.StartsWith(normalKeyPrefix, page.Contents[0].Key);
            Assert.Equal("url", page.EncodingType);
            count += page.Contents.Count;
        }
        Assert.Equal(10, count);

        // common prefix
        paginators = client.ListObjectsV2Paginator(
            new ListObjectsV2Request() {
                Bucket = bucketName,
                Delimiter = "/"
            },
            new Paginator.PaginatorOptions() {
                Limit = 100
            });
        count = 0;
        foreach (var page in paginators.IterPage()) {
            Assert.NotNull(page.Contents);
            Assert.Empty(page.Contents);
            Assert.Equal("url", page.EncodingType);
            Assert.NotNull(page.CommonPrefixes);
            Assert.Equal(2, page.CommonPrefixes.Count);
            Assert.Equal("normal/", page.CommonPrefixes[0].Prefix);
            Assert.Equal("special/", page.CommonPrefixes[1].Prefix);
            count++;
        }
        Assert.Equal(1, count);

        // delete all objects
        paginators = client.ListObjectsV2Paginator(new ListObjectsV2Request() { Bucket = bucketName });
        await foreach (var page in paginators.IterPageAsync()) {
            var obj = new List<Models.DeleteObject>();
            foreach (var version in page.Contents ?? []) {
                obj.Add(new DeleteObject() {
                    Key = version.Key
                });
            }
            if (obj.Count > 0) {
                await client.DeleteMultipleObjects(
                    new Models.DeleteMultipleObjectsRequest() {
                        Bucket = bucketName,
                        Objects = obj
                    }
                );
            }
        }

        var delResult = await client.DeleteBucketAsync(new() {
            Bucket = bucketName,
        });

        Assert.Equal(204, delResult.StatusCode);
        Assert.NotNull(delResult.RequestId);
    }
}
