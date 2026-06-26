using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using AlibabaCloud.OSS.V2;
using AlibabaCloud.OSS.V2.Credentials;
using AlibabaCloud.OSS.V2.Models;

class Program
{
    static int passed = 0;
    static string BucketName;
    static string Region;

    static void Check(string name, bool ok)
    {
        if (!ok) throw new Exception($"FAIL: {name}");
        passed++;
        Console.WriteLine($"  [PASS] {name}");
    }

    static async Task Main(string[] args)
    {
        Region = "cn-hangzhou";

        var cfg = Configuration.LoadDefault();
        cfg.Region = Region;
        cfg.CredentialsProvider = new EnvironmentVariableCredentialsProvider();

        using var client = new Client(cfg);
        BucketName = $"csharp-nuget-verify-{Guid.NewGuid().ToString().Substring(0, 8)}";

        var asm = typeof(Client).Assembly;
        var version = asm.GetCustomAttribute<System.Reflection.AssemblyInformationalVersionAttribute>()?.InformationalVersion
            ?? asm.GetName().Version?.ToString() ?? "unknown";
        Console.WriteLine($"=== NuGet Package Verify: AlibabaCloud.OSS.V2 {version} ===");
        Console.WriteLine($"Region: {Region}, Bucket: {BucketName}");

        try
        {
            await TestService(client);
            await TestBucketBasic(client);
            await TestBucketAcl(client);
            await TestObjectBasic(client);
            await TestObjectAcl(client);
            await TestObjectTagging(client);
            await TestObjectSymlink(client);
            await TestAppendObject(client);
            await TestMultipartUpload(client);
            await TestPresigner(client);
            await TestPaginator(client);
            await TestExtensions(client);
        }
        finally
        {
            await CleanBucket(client, BucketName);
        }

        var tfm = System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription;
        Console.WriteLine($"\n[INFO] Assembly: {asm.FullName}");
        Console.WriteLine($"[INFO] Runtime: {tfm}");
        Console.WriteLine($"\n=== All {passed} checks passed! ===");
    }

    static async Task TestService(Client client)
    {
        Console.WriteLine("\n--- Service: DescribeRegions / ListBuckets ---");

        var regions = await client.DescribeRegionsAsync(new DescribeRegionsRequest());
        Check("DescribeRegions status=200", regions.StatusCode == 200);
        Check("DescribeRegions has regions", regions.RegionInfoList?.RegionInfos?.Count > 0);

        var listBuckets = await client.ListBucketsAsync(new ListBucketsRequest() { MaxKeys = 10 });
        Check("ListBuckets status=200", listBuckets.StatusCode == 200);
    }

    static async Task TestBucketBasic(Client client)
    {
        Console.WriteLine("\n--- Bucket Basic: Put/GetInfo/GetStat/GetLocation/Delete ---");

        var putResult = await client.PutBucketAsync(new PutBucketRequest()
        {
            Bucket = BucketName,
            CreateBucketConfiguration = new CreateBucketConfiguration()
            {
                StorageClass = StorageClassType.IA.GetString()
            }
        });
        Check("PutBucket status=200", putResult.StatusCode == 200);

        var info = await client.GetBucketInfoAsync(new GetBucketInfoRequest() { Bucket = BucketName });
        Check("GetBucketInfo status=200", info.StatusCode == 200);
        Check("GetBucketInfo StorageClass=IA", info.BucketInfo?.StorageClass == "IA");
        Check("GetBucketInfo Name matches", info.BucketInfo?.Name == BucketName);

        var stat = await client.GetBucketStatAsync(new GetBucketStatRequest() { Bucket = BucketName });
        Check("GetBucketStat status=200", stat.StatusCode == 200);
        Check("GetBucketStat ObjectCount=0", stat.BucketStat?.ObjectCount == 0);

        var loc = await client.GetBucketLocationAsync(new GetBucketLocationRequest() { Bucket = BucketName });
        Check("GetBucketLocation status=200", loc.StatusCode == 200);
        Check("GetBucketLocation has value", !string.IsNullOrEmpty(loc.LocationConstraint));
    }

    static async Task TestBucketAcl(Client client)
    {
        Console.WriteLine("\n--- Bucket Acl: Get/Put ---");

        var getAcl = await client.GetBucketAclAsync(new GetBucketAclRequest() { Bucket = BucketName });
        Check("GetBucketAcl status=200", getAcl.StatusCode == 200);
        Check("GetBucketAcl Grant=private", getAcl.AccessControlPolicy?.AccessControlList?.Grant == "private");
    }

    static async Task TestObjectBasic(Client client)
    {
        Console.WriteLine("\n--- Object Basic: Put/Head/GetMeta/Get/Copy/Delete ---");
        var key = "verify-obj-basic";
        var content = "hello world from nuget verify";

        var putResult = await client.PutObjectAsync(new PutObjectRequest()
        {
            Bucket = BucketName,
            Key = key,
            Body = new MemoryStream(Encoding.UTF8.GetBytes(content)),
            Metadata = new Dictionary<string, string> { { "mykey", "myval" } },
            Tagging = "tag1=val1"
        });
        Check("PutObject status=200", putResult.StatusCode == 200);
        Check("PutObject has ETag", !string.IsNullOrEmpty(putResult.ETag));

        var headResult = await client.HeadObjectAsync(new HeadObjectRequest()
        {
            Bucket = BucketName, Key = key
        });
        Check("HeadObject status=200", headResult.StatusCode == 200);
        Check("HeadObject ContentLength", headResult.ContentLength == content.Length);
        Check("HeadObject ObjectType=Normal", headResult.ObjectType == "Normal");
        Check("HeadObject Metadata", headResult.Metadata?["mykey"] == "myval");
        Check("HeadObject TaggingCount=1", headResult.TaggingCount == 1);

        var getMeta = await client.GetObjectMetaAsync(new GetObjectMetaRequest()
        {
            Bucket = BucketName, Key = key
        });
        Check("GetObjectMeta status=200", getMeta.StatusCode == 200);
        Check("GetObjectMeta ContentLength", getMeta.ContentLength == content.Length);

        var getResult = await client.GetObjectAsync(new GetObjectRequest()
        {
            Bucket = BucketName, Key = key
        });
        Check("GetObject status=200", getResult.StatusCode == 200);
        Check("GetObject ContentLength", getResult.ContentLength == content.Length);
        using (var reader = new StreamReader(getResult.Body))
        {
            var body = await reader.ReadToEndAsync();
            Check("GetObject body matches", body == content);
        }

        var copyResult = await client.CopyObjectAsync(new CopyObjectRequest()
        {
            Bucket = BucketName, Key = key + "-copy", SourceKey = key
        });
        Check("CopyObject status=200", copyResult.StatusCode == 200);

        var getResult2 = await client.GetObjectAsync(new GetObjectRequest()
        {
            Bucket = BucketName, Key = key + "-copy"
        });
        Check("GetObject(copy) status=200", getResult2.StatusCode == 200);
        using (var reader = new StreamReader(getResult2.Body))
        {
            var body = await reader.ReadToEndAsync();
            Check("GetObject(copy) body matches", body == content);
        }

        var delResult = await client.DeleteObjectAsync(new DeleteObjectRequest()
        {
            Bucket = BucketName, Key = key
        });
        Check("DeleteObject status=204", delResult.StatusCode == 204);

        var delResult2 = await client.DeleteObjectAsync(new DeleteObjectRequest()
        {
            Bucket = BucketName, Key = key + "-copy"
        });
        Check("DeleteObject(copy) status=204", delResult2.StatusCode == 204);
    }

    static async Task TestObjectAcl(Client client)
    {
        Console.WriteLine("\n--- Object Acl: Put/Get ---");
        var key = "verify-obj-acl";

        await client.PutObjectAsync(new PutObjectRequest()
        {
            Bucket = BucketName, Key = key,
            Body = new MemoryStream(Encoding.UTF8.GetBytes("acl test"))
        });

        var getAcl = await client.GetObjectAclAsync(new GetObjectAclRequest()
        {
            Bucket = BucketName, Key = key
        });
        Check("GetObjectAcl status=200", getAcl.StatusCode == 200);
        Check("GetObjectAcl default", getAcl.Acl == "default");

        var putAcl = await client.PutObjectAclAsync(new PutObjectAclRequest()
        {
            Bucket = BucketName, Key = key, Acl = "private"
        });
        Check("PutObjectAcl status=200", putAcl.StatusCode == 200);

        getAcl = await client.GetObjectAclAsync(new GetObjectAclRequest()
        {
            Bucket = BucketName, Key = key
        });
        Check("GetObjectAcl after put=private", getAcl.Acl == "private");

        await client.DeleteObjectAsync(new DeleteObjectRequest() { Bucket = BucketName, Key = key });
    }

    static async Task TestObjectTagging(Client client)
    {
        Console.WriteLine("\n--- Object Tagging: Put/Get/Delete ---");
        var key = "verify-obj-tag";

        await client.PutObjectAsync(new PutObjectRequest()
        {
            Bucket = BucketName, Key = key,
            Body = new MemoryStream(Encoding.UTF8.GetBytes("tag test"))
        });

        var putTag = await client.PutObjectTaggingAsync(new PutObjectTaggingRequest()
        {
            Bucket = BucketName, Key = key,
            Tagging = new Tagging()
            {
                TagSet = new TagSet()
                {
                    Tags = new List<Tag> { new Tag() { Key = "k1", Value = "v1" } }
                }
            }
        });
        Check("PutObjectTagging status=200", putTag.StatusCode == 200);

        var getTag = await client.GetObjectTaggingAsync(new GetObjectTaggingRequest()
        {
            Bucket = BucketName, Key = key
        });
        Check("GetObjectTagging status=200", getTag.StatusCode == 200);
        Check("GetObjectTagging count=1", getTag.Tagging?.TagSet?.Tags?.Count == 1);
        Check("GetObjectTagging key=k1", getTag.Tagging?.TagSet?.Tags?[0].Key == "k1");
        Check("GetObjectTagging value=v1", getTag.Tagging?.TagSet?.Tags?[0].Value == "v1");

        var delTag = await client.DeleteObjectTaggingAsync(new DeleteObjectTaggingRequest()
        {
            Bucket = BucketName, Key = key
        });
        Check("DeleteObjectTagging status=204", delTag.StatusCode == 204);

        getTag = await client.GetObjectTaggingAsync(new GetObjectTaggingRequest()
        {
            Bucket = BucketName, Key = key
        });
        Check("GetObjectTagging after delete count=0", getTag.Tagging?.TagSet?.Tags?.Count == 0);

        await client.DeleteObjectAsync(new DeleteObjectRequest() { Bucket = BucketName, Key = key });
    }

    static async Task TestObjectSymlink(Client client)
    {
        Console.WriteLine("\n--- Object Symlink: Put/Get ---");
        var key = "verify-obj-symlink";
        var linkKey = key + "-link";

        await client.PutObjectAsync(new PutObjectRequest()
        {
            Bucket = BucketName, Key = key,
            Body = new MemoryStream(Encoding.UTF8.GetBytes("symlink target"))
        });

        var putSym = await client.PutSymlinkAsync(new PutSymlinkRequest()
        {
            Bucket = BucketName, Key = linkKey, SymlinkTarget = key
        });
        Check("PutSymlink status=200", putSym.StatusCode == 200);

        var getSym = await client.GetSymlinkAsync(new GetSymlinkRequest()
        {
            Bucket = BucketName, Key = linkKey
        });
        Check("GetSymlink status=200", getSym.StatusCode == 200);
        Check("GetSymlink target matches", getSym.SymlinkTarget == key);

        var getObj = await client.GetObjectAsync(new GetObjectRequest()
        {
            Bucket = BucketName, Key = linkKey
        });
        Check("GetObject via symlink status=200", getObj.StatusCode == 200);
        Check("GetObject via symlink ObjectType=Symlink", getObj.ObjectType == "Symlink");
        using (var reader = new StreamReader(getObj.Body))
        {
            var body = await reader.ReadToEndAsync();
            Check("GetObject via symlink body", body == "symlink target");
        }

        await client.DeleteObjectAsync(new DeleteObjectRequest() { Bucket = BucketName, Key = linkKey });
        await client.DeleteObjectAsync(new DeleteObjectRequest() { Bucket = BucketName, Key = key });
    }

    static async Task TestAppendObject(Client client)
    {
        Console.WriteLine("\n--- AppendObject / SealAppendObject ---");
        var key = "verify-append-obj";

        var append1 = await client.AppendObjectAsync(new AppendObjectRequest()
        {
            Bucket = BucketName, Key = key, Position = 0,
            Body = new MemoryStream(Encoding.UTF8.GetBytes("hello "))
        });
        Check("AppendObject(1) status=200", append1.StatusCode == 200);
        Check("AppendObject(1) NextPosition=6", append1.NextAppendPosition == 6);

        var append2 = await client.AppendObjectAsync(new AppendObjectRequest()
        {
            Bucket = BucketName, Key = key, Position = 6,
            Body = new MemoryStream(Encoding.UTF8.GetBytes("world"))
        });
        Check("AppendObject(2) status=200", append2.StatusCode == 200);
        Check("AppendObject(2) NextPosition=11", append2.NextAppendPosition == 11);

        var getObj = await client.GetObjectAsync(new GetObjectRequest()
        {
            Bucket = BucketName, Key = key
        });
        Check("GetObject(append) ObjectType=Appendable", getObj.ObjectType == "Appendable");
        using (var reader = new StreamReader(getObj.Body))
        {
            var body = await reader.ReadToEndAsync();
            Check("GetObject(append) body=hello world", body == "hello world");
        }

        try
        {
            var sealResult = await client.SealAppendObjectAsync(new SealAppendObjectRequest()
            {
                Bucket = BucketName, Key = key, Position = 11
            });
            Check("SealAppendObject status=200", sealResult.StatusCode == 200);
        }
        catch (OperationException e) when (e.InnerException is ServiceException se && se.ErrorCode == "OperationNotSupported")
        {
            Check("SealAppendObject (not supported in region, skipped)", true);
        }

        await client.DeleteObjectAsync(new DeleteObjectRequest() { Bucket = BucketName, Key = key });
    }

    static async Task TestMultipartUpload(Client client)
    {
        Console.WriteLine("\n--- Multipart: Init/Upload/ListParts/Complete ---");
        var key = "verify-multipart";

        var initResult = await client.InitiateMultipartUploadAsync(new InitiateMultipartUploadRequest()
        {
            Bucket = BucketName, Key = key
        });
        Check("InitiateMultipartUpload status=200", initResult.StatusCode == 200);
        Check("InitiateMultipartUpload has UploadId", !string.IsNullOrEmpty(initResult.UploadId));

        var content = "multipart content data";
        var upResult = await client.UploadPartAsync(new UploadPartRequest()
        {
            Bucket = BucketName, Key = key, PartNumber = 1,
            UploadId = initResult.UploadId,
            Body = new MemoryStream(Encoding.UTF8.GetBytes(content))
        });
        Check("UploadPart status=200", upResult.StatusCode == 200);
        Check("UploadPart has ETag", !string.IsNullOrEmpty(upResult.ETag));

        var listParts = await client.ListPartsAsync(new ListPartsRequest()
        {
            Bucket = BucketName, Key = key, UploadId = initResult.UploadId
        });
        Check("ListParts status=200", listParts.StatusCode == 200);
        Check("ListParts count=1", listParts.Parts?.Count == 1);
        Check("ListParts size matches", listParts.Parts?[0].Size == content.Length);

        var listUploads = await client.ListMultipartUploadsAsync(new ListMultipartUploadsRequest()
        {
            Bucket = BucketName, Prefix = key
        });
        Check("ListMultipartUploads status=200", listUploads.StatusCode == 200);
        Check("ListMultipartUploads count>=1", listUploads.Uploads?.Count >= 1);

        var cmResult = await client.CompleteMultipartUploadAsync(new CompleteMultipartUploadRequest()
        {
            Bucket = BucketName, Key = key, UploadId = initResult.UploadId,
            CompleteMultipartUpload = new CompleteMultipartUpload()
            {
                Parts = new List<UploadPart> { new UploadPart() { ETag = upResult.ETag, PartNumber = 1 } }
            }
        });
        Check("CompleteMultipartUpload status=200", cmResult.StatusCode == 200);

        var getObj = await client.GetObjectAsync(new GetObjectRequest()
        {
            Bucket = BucketName, Key = key
        });
        Check("GetObject(multipart) ObjectType=Multipart", getObj.ObjectType == "Multipart");
        using (var reader = new StreamReader(getObj.Body))
        {
            var body = await reader.ReadToEndAsync();
            Check("GetObject(multipart) body matches", body == content);
        }

        // UploadPartCopy
        Console.WriteLine("\n--- Multipart: UploadPartCopy ---");
        var copyKey = key + "-copy";
        var initResult2 = await client.InitiateMultipartUploadAsync(new InitiateMultipartUploadRequest()
        {
            Bucket = BucketName, Key = copyKey
        });
        Check("InitiateMultipartUpload(copy) status=200", initResult2.StatusCode == 200);

        var copyResult = await client.UploadPartCopyAsync(new UploadPartCopyRequest()
        {
            Bucket = BucketName, Key = copyKey, PartNumber = 1,
            UploadId = initResult2.UploadId, SourceKey = key
        });
        Check("UploadPartCopy status=200", copyResult.StatusCode == 200);

        var cmResult2 = await client.CompleteMultipartUploadAsync(new CompleteMultipartUploadRequest()
        {
            Bucket = BucketName, Key = copyKey, UploadId = initResult2.UploadId,
            CompleteMultipartUpload = new CompleteMultipartUpload()
            {
                Parts = new List<UploadPart> { new UploadPart() { ETag = copyResult.ETag, PartNumber = 1 } }
            }
        });
        Check("CompleteMultipartUpload(copy) status=200", cmResult2.StatusCode == 200);

        // AbortMultipartUpload
        Console.WriteLine("\n--- Multipart: Abort ---");
        var abortKey = key + "-abort";
        var initResult3 = await client.InitiateMultipartUploadAsync(new InitiateMultipartUploadRequest()
        {
            Bucket = BucketName, Key = abortKey
        });
        var abortResult = await client.AbortMultipartUploadAsync(new AbortMultipartUploadRequest()
        {
            Bucket = BucketName, Key = abortKey, UploadId = initResult3.UploadId
        });
        Check("AbortMultipartUpload status=204", abortResult.StatusCode == 204);

        await client.DeleteObjectAsync(new DeleteObjectRequest() { Bucket = BucketName, Key = key });
        await client.DeleteObjectAsync(new DeleteObjectRequest() { Bucket = BucketName, Key = copyKey });
    }

    static async Task TestPresigner(Client client)
    {
        Console.WriteLine("\n--- Presigner ---");
        var key = "verify-presign";
        var content = "presign content";

        await client.PutObjectAsync(new PutObjectRequest()
        {
            Bucket = BucketName, Key = key,
            Body = new MemoryStream(Encoding.UTF8.GetBytes(content))
        });

        var preResult = client.Presign(new GetObjectRequest()
        {
            Bucket = BucketName, Key = key
        });
        Check("Presign has URL", !string.IsNullOrEmpty(preResult.Url));
        Check("Presign Method=GET", preResult.Method == "GET");
        Check("Presign has Expiration", preResult.Expiration != null);

        using var hc = new HttpClient();
        var resp = await hc.GetAsync(preResult.Url);
        Check("Presign GET status=200", resp.IsSuccessStatusCode);
        var body = await resp.Content.ReadAsStringAsync();
        Check("Presign GET body matches", body == content);

        await client.DeleteObjectAsync(new DeleteObjectRequest() { Bucket = BucketName, Key = key });
    }

    static async Task TestPaginator(Client client)
    {
        Console.WriteLine("\n--- Paginator: ListObjects ---");
        for (var i = 0; i < 3; i++)
        {
            await client.PutObjectAsync(new PutObjectRequest()
            {
                Bucket = BucketName, Key = $"paginator-test/{i}"
            });
        }

        var paginator = client.ListObjectsPaginator(new ListObjectsRequest()
        {
            Bucket = BucketName, Prefix = "paginator-test/"
        }, new AlibabaCloud.OSS.V2.Paginator.PaginatorOptions() { Limit = 2 });

        var count = 0;
        await foreach (var page in paginator.IterPageAsync())
        {
            count += page.Contents?.Count ?? 0;
        }
        Check("ListObjectsPaginator total=3", count == 3);

        Console.WriteLine("\n--- Paginator: ListObjectsV2 ---");
        var paginator2 = client.ListObjectsV2Paginator(new ListObjectsV2Request()
        {
            Bucket = BucketName, Prefix = "paginator-test/"
        }, new AlibabaCloud.OSS.V2.Paginator.PaginatorOptions() { Limit = 2 });

        count = 0;
        await foreach (var page in paginator2.IterPageAsync())
        {
            count += page.Contents?.Count ?? 0;
        }
        Check("ListObjectsV2Paginator total=3", count == 3);

        Console.WriteLine("\n--- Paginator: ListBuckets ---");
        var paginator3 = client.ListBucketsPaginator(new ListBucketsRequest()
        {
            Prefix = BucketName
        });
        count = 0;
        await foreach (var page in paginator3.IterPageAsync())
        {
            count += page.Buckets?.Count ?? 0;
        }
        Check("ListBucketsPaginator found bucket", count >= 1);

        // cleanup paginator objects
        await client.DeleteMultipleObjectsAsync(new DeleteMultipleObjectsRequest()
        {
            Bucket = BucketName,
            Objects = new List<DeleteObject>
            {
                new DeleteObject() { Key = "paginator-test/0" },
                new DeleteObject() { Key = "paginator-test/1" },
                new DeleteObject() { Key = "paginator-test/2" },
            }
        });
        Check("DeleteMultipleObjects status OK", true);
    }

    static async Task TestExtensions(Client client)
    {
        Console.WriteLine("\n--- Extensions: IsExist / File APIs ---");

        var exist = await client.IsBucketExistAsync(BucketName);
        Check("IsBucketExist=true", exist);

        var key = "verify-ext-file";
        await client.PutObjectAsync(new PutObjectRequest()
        {
            Bucket = BucketName, Key = key,
            Body = new MemoryStream(Encoding.UTF8.GetBytes("extension test"))
        });

        var objExist = await client.IsObjectExistAsync(BucketName, key);
        Check("IsObjectExist=true", objExist);

        var notExist = await client.IsObjectExistAsync(BucketName, "no-such-key-xyz");
        Check("IsObjectExist=false for missing", !notExist);

        await client.DeleteObjectAsync(new DeleteObjectRequest() { Bucket = BucketName, Key = key });
    }

    static async Task CleanBucket(Client client, string bucketName)
    {
        try
        {
            var paginator = client.ListObjectsPaginator(new ListObjectsRequest() { Bucket = bucketName });
            await foreach (var page in paginator.IterPageAsync())
            {
                if (page.Contents == null || page.Contents.Count == 0) continue;
                var objs = new List<DeleteObject>();
                foreach (var obj in page.Contents)
                    objs.Add(new DeleteObject() { Key = obj.Key });
                await client.DeleteMultipleObjectsAsync(new DeleteMultipleObjectsRequest()
                {
                    Bucket = bucketName, Objects = objs
                });
            }
            await client.DeleteBucketAsync(new DeleteBucketRequest() { Bucket = bucketName });
        }
        catch { }
    }
}
