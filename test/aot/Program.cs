#nullable enable

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AlibabaCloud.OSS.V2;
using AlibabaCloud.OSS.V2.Credentials;
using AlibabaCloud.OSS.V2.Models;
using AlibabaCloud.OSS.V2.Transport;

int failures = 0;
const string bucket = "test-bucket";
const string key = "test-key";

var mockHandler = new MockHttpMessageHandler();
var config = new Configuration()
{
    Region = "cn-hangzhou",
    CredentialsProvider = new AnonymousCredentialsProvider(),
    HttpTransport = new HttpTransport(mockHandler),
};
using var client = new Client(config);

// --- Multipart Operations ---

// InitiateMultipartUpload (deserialize: XmlInitiateMultipartUploadResult)
failures += await TestApi("InitiateMultipartUpload", async () =>
{
    mockHandler.SetResponse(HttpStatusCode.OK,
        "<InitiateMultipartUploadResult><Bucket>b</Bucket><Key>k</Key><UploadId>uid-123</UploadId></InitiateMultipartUploadResult>");
    var result = await client.InitiateMultipartUploadAsync(new() { Bucket = bucket, Key = key });
    Assert(result.UploadId == "uid-123", $"UploadId={result.UploadId}");
});

// CompleteMultipartUpload (serialize: CompleteMultipartUpload XML, deserialize: XmlCompleteMultipartUploadResult)
failures += await TestApi("CompleteMultipartUpload", async () =>
{
    mockHandler.SetResponse(HttpStatusCode.OK,
        "<CompleteMultipartUploadResult><Bucket>b</Bucket><Key>k</Key><ETag>\"etag\"</ETag><Location>http://b.oss.example.com/k</Location></CompleteMultipartUploadResult>");
    var result = await client.CompleteMultipartUploadAsync(new()
    {
        Bucket = bucket,
        Key = key,
        UploadId = "uid-123",
        CompleteMultipartUpload = new CompleteMultipartUpload
        {
            Parts = new List<UploadPart> { new() { PartNumber = 1, ETag = "\"etag\"" } }
        }
    });
    Assert(result.ETag == "\"etag\"", $"ETag={result.ETag}");
});

// UploadPartCopy (deserialize: XmlCopyPartResult)
failures += await TestApi("UploadPartCopy", async () =>
{
    mockHandler.SetResponse(HttpStatusCode.OK,
        "<CopyPartResult><ETag>\"etag\"</ETag><LastModified>2024-01-01T00:00:00.000Z</LastModified></CopyPartResult>");
    var result = await client.UploadPartCopyAsync(new()
    {
        Bucket = bucket,
        Key = key,
        SourceKey = "source-key",
        PartNumber = 1,
        UploadId = "uid-123",
    });
    Assert(result.ETag == "\"etag\"", $"ETag={result.ETag}");
});

// ListMultipartUploads (deserialize: XmlListMultipartUploadsResult)
failures += await TestApi("ListMultipartUploads", async () =>
{
    mockHandler.SetResponse(HttpStatusCode.OK,
        "<ListMultipartUploadsResult><Bucket>b</Bucket><MaxUploads>10</MaxUploads></ListMultipartUploadsResult>");
    var result = await client.ListMultipartUploadsAsync(new() { Bucket = bucket });
    Assert(result.MaxUploads == 10, $"MaxUploads={result.MaxUploads}");
});

// ListParts (deserialize: XmlListPartsResult)
failures += await TestApi("ListParts", async () =>
{
    mockHandler.SetResponse(HttpStatusCode.OK,
        "<ListPartsResult><Bucket>b</Bucket><Key>k</Key><UploadId>uid</UploadId><MaxParts>100</MaxParts></ListPartsResult>");
    var result = await client.ListPartsAsync(new() { Bucket = bucket, Key = key, UploadId = "uid" });
    Assert(result.MaxParts == 100, $"MaxParts={result.MaxParts}");
});

// --- Object Basic Operations ---

// CopyObject (deserialize: XmlCopyObjectResult)
failures += await TestApi("CopyObject", async () =>
{
    mockHandler.SetResponse(HttpStatusCode.OK,
        "<CopyObjectResult><ETag>\"etag\"</ETag><LastModified>2024-01-01T00:00:00.000Z</LastModified></CopyObjectResult>");
    var result = await client.CopyObjectAsync(new() { Bucket = bucket, Key = key, SourceKey = "src-key" });
    Assert(result.ETag == "\"etag\"", $"ETag={result.ETag}");
});

// DeleteMultipleObjects (serialize: Delete XML, deserialize: XmlDeleteResult)
failures += await TestApi("DeleteMultipleObjects", async () =>
{
    mockHandler.SetResponse(HttpStatusCode.OK,
        "<DeleteResult><Deleted><Key>k1</Key></Deleted></DeleteResult>");
    var result = await client.DeleteMultipleObjectsAsync(new()
    {
        Bucket = bucket,
        Objects = new List<DeleteObject> { new() { Key = "k1" } }
    });
    Assert(result.DeletedObjects?.Count == 1, $"DeletedObjects.Count={result.DeletedObjects?.Count}");
});

// --- Bucket Basic Operations ---

// PutBucket (serialize: CreateBucketConfiguration)
failures += await TestApi("PutBucket", async () =>
{
    mockHandler.SetResponse(HttpStatusCode.OK, "");
    var result = await client.PutBucketAsync(new()
    {
        Bucket = bucket,
        CreateBucketConfiguration = new CreateBucketConfiguration { StorageClass = "Standard" }
    });
    Assert(result.Status == "OK", $"Status={result.Status}");
});

// ListObjects (deserialize: XmlListBucketResult)
failures += await TestApi("ListObjects", async () =>
{
    mockHandler.SetResponse(HttpStatusCode.OK,
        "<ListBucketResult><Name>b</Name><MaxKeys>100</MaxKeys><IsTruncated>false</IsTruncated></ListBucketResult>");
    var result = await client.ListObjectsAsync(new() { Bucket = bucket });
    Assert(result.MaxKeys == 100, $"MaxKeys={result.MaxKeys}");
});

// ListObjectsV2 (deserialize: XmlListBucketResult)
failures += await TestApi("ListObjectsV2", async () =>
{
    mockHandler.SetResponse(HttpStatusCode.OK,
        "<ListBucketResult><Name>b</Name><MaxKeys>50</MaxKeys><IsTruncated>false</IsTruncated></ListBucketResult>");
    var result = await client.ListObjectsV2Async(new() { Bucket = bucket });
    Assert(result.MaxKeys == 50, $"MaxKeys={result.MaxKeys}");
});

// GetBucketStat (deserialize via DeserializerXmlBody: BucketStat)
failures += await TestApi("GetBucketStat", async () =>
{
    mockHandler.SetResponse(HttpStatusCode.OK,
        "<BucketStat><Storage>1024</Storage><ObjectCount>5</ObjectCount></BucketStat>");
    var result = await client.GetBucketStatAsync(new() { Bucket = bucket });
    Assert(result.BucketStat?.Storage == 1024, $"Storage={result.BucketStat?.Storage}");
});

// GetBucketInfo (deserialize via DeserializerXmlBody: XmlBucketInfo)
failures += await TestApi("GetBucketInfo", async () =>
{
    mockHandler.SetResponse(HttpStatusCode.OK,
        "<BucketInfo><Bucket><Name>b</Name><CreationDate>2024-01-01T00:00:00.000Z</CreationDate></Bucket></BucketInfo>");
    var result = await client.GetBucketInfoAsync(new() { Bucket = bucket });
    Assert(result.BucketInfo != null, $"BucketInfo is null");
});

// GetBucketLocation (deserialize via DeserializerXmlBody: XmlLocationConstraint)
failures += await TestApi("GetBucketLocation", async () =>
{
    mockHandler.SetResponse(HttpStatusCode.OK,
        "<LocationConstraint>cn-hangzhou</LocationConstraint>");
    var result = await client.GetBucketLocationAsync(new() { Bucket = bucket });
    Assert(result.LocationConstraint == "cn-hangzhou", $"LocationConstraint={result.LocationConstraint}");
});

// --- Bucket Versioning Operations ---

// PutBucketVersioning (serialize: VersioningConfiguration)
failures += await TestApi("PutBucketVersioning", async () =>
{
    mockHandler.SetResponse(HttpStatusCode.OK, "");
    var result = await client.PutBucketVersioningAsync(new()
    {
        Bucket = bucket,
        VersioningConfiguration = new VersioningConfiguration { Status = "Enabled" }
    });
    Assert(result.Status == "OK", $"Status={result.Status}");
    Assert(mockHandler.LastRequestBody != null && mockHandler.LastRequestBody.Contains("<Status>Enabled</Status>"),
        $"RequestBody missing VersioningConfiguration: {mockHandler.LastRequestBody}");
});

// GetBucketVersioning (deserialize: XmlVersioningConfiguration)
failures += await TestApi("GetBucketVersioning", async () =>
{
    mockHandler.SetResponse(HttpStatusCode.OK,
        "<VersioningConfiguration><Status>Enabled</Status></VersioningConfiguration>");
    var result = await client.GetBucketVersioningAsync(new() { Bucket = bucket });
    Assert(result.VersioningConfiguration?.Status == "Enabled", $"Status={result.VersioningConfiguration?.Status}");
});

// ListObjectVersions (deserialize: XmlListVersionsResult)
failures += await TestApi("ListObjectVersions", async () =>
{
    mockHandler.SetResponse(HttpStatusCode.OK,
        "<ListVersionsResult><Name>b</Name><MaxKeys>100</MaxKeys></ListVersionsResult>");
    var result = await client.ListObjectVersionsAsync(new() { Bucket = bucket });
    Assert(result.MaxKeys == 100, $"MaxKeys={result.MaxKeys}");
});

// --- Bucket ACL ---

// GetBucketAcl (deserialize via DeserializerXmlBody: AccessControlPolicy)
failures += await TestApi("GetBucketAcl", async () =>
{
    mockHandler.SetResponse(HttpStatusCode.OK,
        "<AccessControlPolicy><Owner><ID>owner-id</ID><DisplayName>owner</DisplayName></Owner><AccessControlList><Grant>public-read</Grant></AccessControlList></AccessControlPolicy>");
    var result = await client.GetBucketAclAsync(new() { Bucket = bucket });
    Assert(result.AccessControlPolicy?.AccessControlList?.Grant == "public-read",
        $"Grant={result.AccessControlPolicy?.AccessControlList?.Grant}");
});

// --- Object ACL ---

// GetObjectAcl (deserialize via DeserializerAnyBody: AccessControlPolicy)
failures += await TestApi("GetObjectAcl", async () =>
{
    mockHandler.SetResponse(HttpStatusCode.OK,
        "<AccessControlPolicy><Owner><ID>owner-id</ID><DisplayName>owner</DisplayName></Owner><AccessControlList><Grant>private</Grant></AccessControlList></AccessControlPolicy>");
    var result = await client.GetObjectAclAsync(new() { Bucket = bucket, Key = key });
    Assert(result.Acl == "private", $"Acl={result.Acl}");
});

// --- Object Tagging ---

// PutObjectTagging (serialize: Tagging)
failures += await TestApi("PutObjectTagging", async () =>
{
    mockHandler.SetResponse(HttpStatusCode.OK, "");
    var result = await client.PutObjectTaggingAsync(new()
    {
        Bucket = bucket,
        Key = key,
        Tagging = new Tagging
        {
            TagSet = new TagSet { Tags = new List<Tag> { new() { Key = "env", Value = "test" } } }
        }
    });
    Assert(result.Status == "OK", $"Status={result.Status}");
    Assert(mockHandler.LastRequestBody != null && mockHandler.LastRequestBody.Contains("<Key>env</Key>"),
        $"RequestBody missing Tagging: {mockHandler.LastRequestBody}");
});

// GetObjectTagging (deserialize via DeserializerAnyBody: Tagging)
failures += await TestApi("GetObjectTagging", async () =>
{
    mockHandler.SetResponse(HttpStatusCode.OK,
        "<Tagging><TagSet><Tag><Key>env</Key><Value>test</Value></Tag></TagSet></Tagging>");
    var result = await client.GetObjectTaggingAsync(new() { Bucket = bucket, Key = key });
    Assert(result.Tagging?.TagSet?.Tags?.Count == 1, $"Tags.Count={result.Tagging?.TagSet?.Tags?.Count}");
});

// RestoreObject (serialize: RestoreRequest with nested JobParameters)
failures += await TestApi("RestoreObject", async () =>
{
    mockHandler.SetResponse(HttpStatusCode.OK, "");
    var result = await client.RestoreObjectAsync(new()
    {
        Bucket = bucket,
        Key = key,
        RestoreRequest = new RestoreRequest
        {
            Days = 3,
            JobParameters = new JobParameters { Tier = "Standard" }
        }
    });
    Assert(result.Status == "OK", $"Status={result.Status}");
    Assert(mockHandler.LastRequestBody != null && mockHandler.LastRequestBody.Contains("<Days>3</Days>"),
        $"RequestBody missing RestoreRequest: {mockHandler.LastRequestBody}");
});

// --- Service ---

// ListBuckets (deserialize: XmlListAllMyBucketsResult)
failures += await TestApi("ListBuckets", async () =>
{
    mockHandler.SetResponse(HttpStatusCode.OK,
        "<ListAllMyBucketsResult><Prefix>p</Prefix><MaxKeys>50</MaxKeys><Buckets><Bucket><Name>b1</Name><CreationDate>2024-01-01T00:00:00.000Z</CreationDate></Bucket></Buckets></ListAllMyBucketsResult>");
    var result = await client.ListBucketsAsync(new());
    Assert(result.MaxKeys == 50, $"MaxKeys={result.MaxKeys}");
});

// --- Region ---

// DescribeRegions (deserialize via DeserializerAnyBody: RegionInfoList)
failures += await TestApi("DescribeRegions", async () =>
{
    mockHandler.SetResponse(HttpStatusCode.OK,
        "<RegionInfoList><RegionInfo><Region>cn-hangzhou</Region><InternetEndpoint>oss-cn-hangzhou.aliyuncs.com</InternetEndpoint><InternalEndpoint>oss-cn-hangzhou-internal.aliyuncs.com</InternalEndpoint><AccelerateEndpoint>oss-accelerate.aliyuncs.com</AccelerateEndpoint></RegionInfo></RegionInfoList>");
    var result = await client.DescribeRegionsAsync(new());
    Assert(result.RegionInfoList?.RegionInfos?.Count == 1, $"RegionInfos.Count={result.RegionInfoList?.RegionInfos?.Count}");
});

// --- Summary ---
if (failures > 0)
{
    Console.WriteLine($"FAILED: {failures} API(s) failed AOT verification.");
    return 1;
}

Console.WriteLine("AOT verification passed: all API calls work correctly.");
return 0;

// --- Helpers ---

static void Assert(bool condition, string detail)
{
    if (!condition) throw new Exception($"Assertion failed: {detail}");
}

static async Task<int> TestApi(string name, Func<Task> action)
{
    try
    {
        await action();
        Console.WriteLine($"  OK   [{name}]");
        return 0;
    }
    catch (Exception ex)
    {
        var msg = ex.Message;
        for (var inner = ex.InnerException; inner != null; inner = inner.InnerException)
            msg = inner.Message;
        Console.WriteLine($"  FAIL [{name}]: {ex.GetType().Name}: {msg}");
        return 1;
    }
}

class MockHttpMessageHandler : HttpMessageHandler
{
    private HttpResponseMessage? _response;
    public string? LastRequestBody { get; private set; }

    public void SetResponse(HttpStatusCode statusCode, string body)
    {
        _response = new HttpResponseMessage(statusCode)
        {
            Content = string.IsNullOrEmpty(body) ? new StringContent("") : new StringContent(body, Encoding.UTF8, "application/xml")
        };
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (_response == null) throw new InvalidOperationException("No mock response configured");
        LastRequestBody = request.Content != null ? await request.Content.ReadAsStringAsync() : null;
        var ret = _response;
        _response = null;
        return ret;
    }
}
