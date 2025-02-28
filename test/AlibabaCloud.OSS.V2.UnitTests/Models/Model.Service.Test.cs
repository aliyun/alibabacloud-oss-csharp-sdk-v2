using System.Text;
using AlibabaCloud.OSS.V2.Models;
using AlibabaCloud.OSS.V2.Transform;

namespace AlibabaCloud.OSS.V2.UnitTests.Models;

public class ModelServiceTest {
    [Fact]
    public void TestListBucketsRequest() {
        var request = new ListBucketsRequest();
        Assert.Empty(request.Headers);
        Assert.Empty(request.Parameters);
        Assert.Null(request.InnerBody);
        Assert.Equal("", request.BodyFormat);
        Assert.Null(request.ResourceGroupId);
        Assert.Null(request.Prefix);
        Assert.Null(request.Marker);
        Assert.Null(request.MaxKeys);

        var input = new OperationInput();
        Serde.SerializeInput(request, ref input);

        Assert.Null(input.Headers);
        Assert.Null(input.Parameters);
        Assert.Null(input.Body);

        request = new ListBucketsRequest {
            ResourceGroupId = "rg-id-123",
            Prefix = "prefix-01",
            MaxKeys = 10001,
            Marker = "bucket-123",
        };
        Assert.NotEmpty(request.Headers);
        Assert.Equal("rg-id-123", request.ResourceGroupId);
        Assert.Equal("prefix-01", request.Prefix);
        Assert.Equal(10001, request.MaxKeys);
        Assert.Equal("bucket-123", request.Marker);

        input = new OperationInput();
        Serde.SerializeInput(request, ref input);

        Assert.Null(input.Body);

        Assert.NotNull(input.Headers);
        Assert.Single(input.Headers);
        Assert.Equal("rg-id-123", input.Headers["x-oss-resource-group-id"]);
        Assert.NotNull(input.Parameters);
        Assert.Equal(3, input.Parameters.Count);
        Assert.Equal("bucket-123", input.Parameters["marker"]);
        Assert.Equal("10001", input.Parameters["max-keys"]);
        Assert.Equal("prefix-01", input.Parameters["prefix"]);
    }

    [Fact]
    public void TestListBucketsResult() {
        var result = new ListBucketsResult();
        Assert.Equal(0, result.StatusCode);
        Assert.Equal("", result.Status);
        Assert.Equal("", result.RequestId);
        Assert.Empty(result.Headers);
        Assert.Equal("", result.BodyFormat);
        Assert.Null(result.BodyType);
        Assert.Null(result.Prefix);
        Assert.Null(result.Marker);
        Assert.Null(result.MaxKeys);
        Assert.Null(result.IsTruncated);
        Assert.Null(result.NextMarker);
        Assert.Null(result.Buckets);

        //empty xml
        var xml = """
<?xml version="1.0" encoding="utf-8"?>
<ListAllMyBucketsResult>
</ListAllMyBucketsResult>
""";

        var output = new OperationOutput {
            StatusCode = 200,
            Status = "OK",
            Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
                { "x-oss-request-id", "123-id" },
                { "Content-Type", "application/xml" }
            },
            Body = new MemoryStream(Encoding.UTF8.GetBytes(xml))
        };
        ResultModel baseResult = result;
        Serde.DeserializeOutput(ref baseResult, ref output, Serde.DeserializeListBuckets);

        Assert.Equal(200, result.StatusCode);
        Assert.Equal("OK", result.Status);
        Assert.Equal("123-id", result.RequestId);
        Assert.Equal(2, result.Headers.Count);
        Assert.Equal("application/xml", result.Headers["content-type"]);
        Assert.Null(result.Marker);
        Assert.Null(result.MaxKeys);
        Assert.Null(result.Prefix);
        Assert.Null(result.IsTruncated);
        Assert.Null(result.NextMarker);
        Assert.NotNull(result.Buckets);
        Assert.Empty(result.Buckets);

        //full xml
        xml = """
<?xml version="1.0" encoding="utf-8"?>
<ListAllMyBucketsResult>
  <Prefix>my</Prefix>
  <Marker>mybucket</Marker>
  <MaxKeys>10</MaxKeys>
  <IsTruncated>true</IsTruncated>
  <NextMarker>mybucket10</NextMarker>
  <Owner>
    <ID>512**</ID>
    <DisplayName>51264</DisplayName>
  </Owner>
  <Buckets>
    <Bucket>
      <CreationDate>2014-02-17T18:12:43.000Z</CreationDate>
      <ExtranetEndpoint>oss-cn-shanghai.aliyuncs.com</ExtranetEndpoint>
      <IntranetEndpoint>oss-cn-shanghai-internal.aliyuncs.com</IntranetEndpoint>
      <Location>oss-cn-shanghai</Location>
      <Name>app-base-oss</Name>
      <Region>cn-shanghai</Region>
      <StorageClass>Standard</StorageClass>
      <ResourceGroupId>rg-aek27tc********</ResourceGroupId>
    </Bucket>
    <Bucket>
      <CreationDate>2014-02-25T11:21:04.000Z</CreationDate>
      <ExtranetEndpoint>oss-cn-hangzhou.aliyuncs.com</ExtranetEndpoint>
      <IntranetEndpoint>oss-cn-hangzhou-internal.aliyuncs.com</IntranetEndpoint>
      <Location>oss-cn-hangzhou</Location>
      <Name>mybucket</Name>
      <Region>cn-hangzhou</Region>
      <StorageClass>IA</StorageClass>
    </Bucket>
  </Buckets>
</ListAllMyBucketsResult>
""";
        output = new OperationOutput {
            StatusCode = 200,
            Status = "OK",
            Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
                { "x-oss-request-id", "123-id" },
                { "Content-Type", "application/xml" }
            },
            Body = new MemoryStream(Encoding.UTF8.GetBytes(xml))
        };
        baseResult = result; Serde.DeserializeOutput(ref baseResult, ref output, Serde.DeserializeListBuckets);

        Assert.Equal(200, result.StatusCode);
        Assert.Equal("OK", result.Status);
        Assert.Equal("123-id", result.RequestId);
        Assert.Equal(2, result.Headers.Count);
        Assert.Equal("application/xml", result.Headers["content-type"]);
        Assert.Equal("mybucket", result.Marker);
        Assert.Equal("my", result.Prefix);
        Assert.Equal(10, result.MaxKeys);
        Assert.Equal("mybucket10", result.NextMarker);
        Assert.Equal(true, result.IsTruncated);
        Assert.NotNull(result.Owner);
        Assert.Equal("512**", result.Owner.Id);
        Assert.Equal("51264", result.Owner.DisplayName);

        Assert.NotNull(result.Buckets);
        Assert.Equal(2, result.Buckets.Count);
        Assert.Equal("app-base-oss", result.Buckets[0].Name);
        Assert.Equal("2014/2/17 18:12:43", result.Buckets[0].CreationDate.ToString());
        Assert.Equal("Standard", result.Buckets[0].StorageClass);
        Assert.Equal("oss-cn-shanghai.aliyuncs.com", result.Buckets[0].ExtranetEndpoint);
        Assert.Equal("oss-cn-shanghai-internal.aliyuncs.com", result.Buckets[0].IntranetEndpoint);
        Assert.Equal("cn-shanghai", result.Buckets[0].Region);
        Assert.Equal("rg-aek27tc********", result.Buckets[0].ResourceGroupId);
        Assert.Equal("oss-cn-shanghai", result.Buckets[0].Location);

        Assert.Equal("mybucket", result.Buckets[1].Name);
        Assert.Equal("2014/2/25 11:21:04", result.Buckets[1].CreationDate.ToString());
        Assert.Equal("IA", result.Buckets[1].StorageClass);
        Assert.Equal("oss-cn-hangzhou.aliyuncs.com", result.Buckets[1].ExtranetEndpoint);
        Assert.Equal("oss-cn-hangzhou-internal.aliyuncs.com", result.Buckets[1].IntranetEndpoint);
        Assert.Equal("cn-hangzhou", result.Buckets[1].Region);
        Assert.Null(result.Buckets[1].ResourceGroupId);
        Assert.Equal("oss-cn-hangzhou", result.Buckets[1].Location);
    }
}