using System.Text;
using AlibabaCloud.OSS.V2.Models;
using AlibabaCloud.OSS.V2.Transform;

namespace AlibabaCloud.OSS.V2.UnitTests.Models;

public class ModelBucketVersioningTest
{
    [Fact]
    public void TestPutBucketVersioningRequest()
    {
        var request = new PutBucketVersioningRequest();
        Assert.Empty(request.Headers);
        Assert.Empty(request.Parameters);
        Assert.Null(request.InnerBody);
        Assert.Equal("xml", request.BodyFormat);
        Assert.Null(request.Bucket);

        var input = new OperationInput();
        Serde.SerializeInput(request, ref input);

        Assert.Null(input.Headers);
        Assert.Null(input.Parameters);
        Assert.Null(input.Body);

        request = new PutBucketVersioningRequest
        {
            Bucket = "bucket",
            VersioningConfiguration = new VersioningConfiguration()
            {
                Status = BucketVersioningStatusType.Enabled.GetString()
            }
        };
        Serde.SerializeInput(request, ref input);

        Assert.Empty(request.Headers);
        Assert.Equal("bucket", request.Bucket);
        Assert.Equal("Enabled", request.VersioningConfiguration.Status);

        Assert.Null(input.Headers);
        Assert.Null(input.Parameters);
        Assert.NotNull(input.Body);
        using var reader = new StreamReader(input.Body);
        var xml = """
<?xml version="1.0" encoding="utf-8"?>
<VersioningConfiguration>
  <Status>Enabled</Status>
</VersioningConfiguration>
""";
        Assert.Equal(xml, reader.ReadToEnd());
    }

    [Fact]
    public void TestPutBucketVersioningResult()
    {
        var result = new PutBucketVersioningResult();
        Assert.Equal(0, result.StatusCode);
        Assert.Equal("", result.Status);
        Assert.Equal("", result.RequestId);
        Assert.Empty(result.Headers);

        var output = new OperationOutput
        {
            StatusCode = 200,
            Status = "OK",
            Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
                {"x-oss-request-id", "123-id"},
                {"Content-Type","txt"}
            }
        };
        ResultModel baseResult = result;
        Serde.DeserializeOutput(ref baseResult, ref output);

        Assert.Equal(200, result.StatusCode);
        Assert.Equal("OK", result.Status);
        Assert.Equal("123-id", result.RequestId);
        Assert.Equal(2, result.Headers.Count);
        Assert.Equal("txt", result.Headers["content-type"]);
    }

    [Fact]
    public void TestGetBucketVersioningRequest()
    {
        var request = new GetBucketVersioningRequest();
        Assert.Empty(request.Headers);
        Assert.Empty(request.Parameters);
        Assert.Null(request.InnerBody);
        Assert.Equal("", request.BodyFormat);
        Assert.Null(request.Bucket);

        var input = new OperationInput();
        Serde.SerializeInput(request, ref input);

        Assert.Null(input.Headers);
        Assert.Null(input.Parameters);
        Assert.Null(input.Body);

        request = new GetBucketVersioningRequest
        {
            Bucket = "bucket",
        };
        Assert.Empty(request.Headers);
        Assert.Equal("bucket", request.Bucket);
        Serde.SerializeInput(request, ref input);

        Assert.Null(input.Headers);
        Assert.Null(input.Parameters);
        Assert.Null(input.Body);
    }

    [Fact]
    public void TestGetBucketVersioningResult()
    {
        var result = new GetBucketVersioningResult();
        Assert.Equal(0, result.StatusCode);
        Assert.Equal("", result.Status);
        Assert.Equal("", result.RequestId);
        Assert.Empty(result.Headers);

        var xml = """
                  <?xml version="1.0" encoding="utf-8"?>
                  <VersioningConfiguration>
                  </VersioningConfiguration>
                  """;

        var output = new OperationOutput
        {
            StatusCode = 200,
            Status = "OK",
            Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
                {"x-oss-request-id", "123-id"},
                {"Content-Type","txt"}
            },
            Body = new MemoryStream(Encoding.UTF8.GetBytes(xml))
        };
        ResultModel baseResult = result;
        Serde.DeserializeOutput(ref baseResult, ref output, Serde.DeserializeGetBucketVersioning);

        Assert.Equal(200, result.StatusCode);
        Assert.Equal("OK", result.Status);
        Assert.Equal("123-id", result.RequestId);
        Assert.Equal(2, result.Headers.Count);
        Assert.Equal("txt", result.Headers["content-type"]);
        Assert.NotNull(result.VersioningConfiguration);
        Assert.Null(result.VersioningConfiguration.Status);

        xml = """
                <?xml version="1.0" encoding="utf-8"?>
                <VersioningConfiguration>
                    <Status>Enabled</Status>
                </VersioningConfiguration>
                """;

        output = new OperationOutput
        {
            StatusCode = 200,
            Status = "OK",
            Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
                {"x-oss-request-id", "123-id"},
                {"Content-Type","txt"}
            },
            Body = new MemoryStream(Encoding.UTF8.GetBytes(xml))
        };
        baseResult = result;
        Serde.DeserializeOutput(ref baseResult, ref output, Serde.DeserializeGetBucketVersioning);

        Assert.Equal(200, result.StatusCode);
        Assert.Equal("OK", result.Status);
        Assert.Equal("123-id", result.RequestId);
        Assert.Equal(2, result.Headers.Count);
        Assert.Equal("txt", result.Headers["content-type"]);
        Assert.NotNull(result.VersioningConfiguration);
        Assert.Equal("Enabled", result.VersioningConfiguration.Status);

        xml = "<VersioningConfiguration xmlns=\"http://doc.oss-cn-hangzhou.aliyuncs.com\"/>";
        output = new OperationOutput
        {
            StatusCode = 200,
            Status = "OK",
            Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
                {"x-oss-request-id", "123-id"},
                {"Content-Type","txt"}
            },
            Body = new MemoryStream(Encoding.UTF8.GetBytes(xml))
        };
        baseResult = result;
        Serde.DeserializeOutput(ref baseResult, ref output, Serde.DeserializeGetBucketVersioning);
        Assert.Equal(200, result.StatusCode);
        Assert.Equal("OK", result.Status);
        Assert.Equal("123-id", result.RequestId);
        Assert.Equal(2, result.Headers.Count);
        Assert.Equal("txt", result.Headers["content-type"]);
        Assert.NotNull(result.VersioningConfiguration);
        Assert.Null(result.VersioningConfiguration.Status);
    }

    [Fact]
    public void TestListObjectVersionsRequest()
    {
        var request = new ListObjectVersionsRequest();
        Assert.Empty(request.Headers);
        Assert.Empty(request.Parameters);
        Assert.Null(request.InnerBody);
        Assert.Equal("", request.BodyFormat);
        Assert.Null(request.Bucket);
        Assert.Null(request.Delimiter);
        Assert.Null(request.EncodingType);
        Assert.Null(request.KeyMarker);
        Assert.Null(request.VersionIdMarker);
        Assert.Null(request.MaxKeys);
        Assert.Null(request.Prefix);

        var input = new OperationInput();
        Serde.SerializeInput(request, ref input);

        Assert.Null(input.Headers);
        Assert.Null(input.Parameters);
        Assert.Null(input.Body);

        request = new ListObjectVersionsRequest
        {
            Bucket = "bucket",
            Delimiter = "/",
            EncodingType = "url",
            KeyMarker = "key-01",
            VersionIdMarker = "version-id-01",
            MaxKeys = 10001,
            Prefix = "prefix-01",
        };
        Assert.Empty(request.Headers);
        Assert.Equal("bucket", request.Bucket);
        Assert.Equal("/", request.Delimiter);
        Assert.Equal("url", request.EncodingType);
        Assert.Equal("key-01", request.KeyMarker);
        Assert.Equal(10001, request.MaxKeys);
        Assert.Equal("prefix-01", request.Prefix);
        Assert.Equal("version-id-01", request.VersionIdMarker);

        input = new OperationInput();
        Serde.SerializeInput(request, ref input);

        Assert.Null(input.Headers);
        Assert.Null(input.Body);
        Assert.NotNull(input.Parameters);
        Assert.Equal(6, input.Parameters.Count);
        Assert.Equal("/", input.Parameters["delimiter"]);
        Assert.Equal("url", input.Parameters["encoding-type"]);
        Assert.Equal("key-01", input.Parameters["key-marker"]);
        Assert.Equal("version-id-01", input.Parameters["version-id-marker"]);
        Assert.Equal("10001", input.Parameters["max-keys"]);
        Assert.Equal("prefix-01", input.Parameters["prefix"]);
    }

    [Fact]
    public void TestListObjectVersionsResult()
    {
        var result = new ListObjectVersionsResult();
        Assert.Equal(0, result.StatusCode);
        Assert.Equal("", result.Status);
        Assert.Equal("", result.RequestId);
        Assert.Empty(result.Headers);
        Assert.Equal("", result.BodyFormat);
        Assert.Null(result.BodyType);
        Assert.Null(result.Name);
        Assert.Null(result.MaxKeys);
        Assert.Null(result.Delimiter);
        Assert.Null(result.EncodingType);
        Assert.Null(result.Prefix);
        Assert.Null(result.IsTruncated);
        Assert.Null(result.KeyMarker);
        Assert.Null(result.NextKeyMarker);
        Assert.Null(result.VersionIdMarker);
        Assert.Null(result.NextVersionIdMarker);
        Assert.Null(result.Versions);
        Assert.Null(result.DeleteMarkers);
        Assert.Null(result.CommonPrefixes);

        //empty xml
        var xml = """
                  <?xml version="1.0" encoding="utf-8"?>
                  <ListVersionsResult>
                  </ListVersionsResult>
                  """;

        var output = new OperationOutput
        {
            StatusCode = 200,
            Status = "OK",
            Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
                { "x-oss-request-id", "123-id" },
                { "Content-Type", "application/xml" }
            },
            Body = new MemoryStream(Encoding.UTF8.GetBytes(xml))
        };
        ResultModel baseResult = result;
        Serde.DeserializeOutput(ref baseResult, ref output, Serde.DeserializeListObjectVersions);

        Assert.Equal(200, result.StatusCode);
        Assert.Equal("OK", result.Status);
        Assert.Equal("123-id", result.RequestId);
        Assert.Equal(2, result.Headers.Count);
        Assert.Equal("application/xml", result.Headers["content-type"]);
        Assert.Null(result.Name);
        Assert.Null(result.MaxKeys);
        Assert.Null(result.Delimiter);
        Assert.Null(result.EncodingType);
        Assert.Null(result.Prefix);
        Assert.Null(result.IsTruncated);
        Assert.Null(result.KeyMarker);
        Assert.Null(result.NextKeyMarker);
        Assert.Null(result.VersionIdMarker);
        Assert.Null(result.NextVersionIdMarker);
        Assert.NotNull(result.Versions);
        Assert.Empty(result.Versions);
        Assert.NotNull(result.DeleteMarkers);
        Assert.Empty(result.DeleteMarkers);
        Assert.NotNull(result.CommonPrefixes);
        Assert.Empty(result.CommonPrefixes);

        //full xml without url encode
        xml = """
<?xml version="1.0" encoding="utf-8"?>
<ListVersionsResult>
    <Name>demo-bucket</Name>
    <Prefix>demo/</Prefix>
    <KeyMarker>demo/1</KeyMarker>
    <VersionIdMarker>version-id</VersionIdMarker>
    <MaxKeys>20</MaxKeys>
    <Delimiter>/</Delimiter>
    <EncodingType/>
    <IsTruncated>true</IsTruncated>
    <NextKeyMarker>demo/README-CN.md</NextKeyMarker>
    <NextVersionIdMarker>CAEQEhiBgICDzK6NnBgiIGRlZWJhYmNlMGUxZDQ4YTZhNTU2MzM4Mzk5NDBl****</NextVersionIdMarker>
    <Version>
        <Key>demo/README-CN.md</Key>
        <VersionId>CAEQEhiBgICDzK6NnBgiIGRlZWJhYmNlMGUxZDQ4YTZhNTU2MzM4Mzk5NDBl****</VersionId>
        <IsLatest>false</IsLatest>
        <LastModified>2022-09-28T09:04:39.000Z</LastModified>
        <ETag>"E317049B40462DE37C422CE4FC1B****"</ETag>
        <Type>Normal</Type>
        <Size>2943</Size>
        <StorageClass>Standard</StorageClass>
        <Owner>
            <ID>150692521021****</ID>
            <DisplayName>160692521021****</DisplayName>
        </Owner>
        <RestoreInfo>ongoing-request="false", expiry-date="Thu, 24 Sep 2020 12:40:33 GMT"</RestoreInfo>
    </Version>
    <Version>
        <Key>example-object-2.jpg</Key>
        <VersionId/>
        <IsLatest>true</IsLatest>
        <LastModified>2019-08-09T12:03:09.000Z</LastModified>
        <ETag>5B3C1A2E053D763E1B002CC607C5A0FE1****</ETag>
        <Size>20</Size>
        <StorageClass>STANDARD</StorageClass>
        <Owner>
            <ID>1250000000</ID>
            <DisplayName>1250000000</DisplayName>
        </Owner>
        <RestoreInfo>ongoing-request="true"</RestoreInfo>
        <TransitionTime>2022-09-28T09:04:40.000Z</TransitionTime>
    </Version>
    <DeleteMarker>
        <Key>demo/README-CN.md</Key>
        <VersionId>CAEQFBiCgID3.86GohgiIDc4ZTE0NTNhZTc5MDQxYzBhYTU5MjY1ZDFjNGJm****</VersionId>
        <IsLatest>true</IsLatest>
        <LastModified>2022-11-04T08:00:06.000Z</LastModified>
        <Owner>
            <ID>150692521021****</ID>
            <DisplayName>350692521021****</DisplayName>
        </Owner>
    </DeleteMarker>
    <DeleteMarker>
        <Key>demo/LICENSE</Key>
        <VersionId>CAEQFBiBgMD0.86GohgiIGZmMmFlM2UwNjdlMzRiMGFhYjk4MjM1ZGUyZDY0****</VersionId>
        <IsLatest>true</IsLatest>
        <LastModified>2022-11-04T08:00:06.000Z</LastModified>
        <Owner>
            <ID>150692521021****</ID>
            <DisplayName>250692521021****</DisplayName>
        </Owner>
    </DeleteMarker>
    <CommonPrefixes>
        <Prefix>demo/.git/</Prefix>
    </CommonPrefixes>
    <CommonPrefixes>
        <Prefix>demo/.idea/</Prefix>
    </CommonPrefixes>
</ListVersionsResult>
""";
        output = new OperationOutput
        {
            StatusCode = 200,
            Status = "OK",
            Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
                { "x-oss-request-id", "123-id" },
                { "Content-Type", "application/xml" }
            },
            Body = new MemoryStream(Encoding.UTF8.GetBytes(xml))
        };
        baseResult = result; Serde.DeserializeOutput(ref baseResult, ref output, Serde.DeserializeListObjectVersions);

        Assert.Equal(200, result.StatusCode);
        Assert.Equal("OK", result.Status);
        Assert.Equal("123-id", result.RequestId);
        Assert.Equal(2, result.Headers.Count);
        Assert.Equal("application/xml", result.Headers["content-type"]);
        Assert.Equal("demo-bucket", result.Name);
        Assert.Equal(20, result.MaxKeys);
        Assert.Equal("/", result.Delimiter);
        Assert.Equal("", result.EncodingType);
        Assert.Equal("demo/1", result.KeyMarker);
        Assert.Equal("version-id", result.VersionIdMarker);
        Assert.Equal("demo/README-CN.md", result.NextKeyMarker);
        Assert.Equal("CAEQEhiBgICDzK6NnBgiIGRlZWJhYmNlMGUxZDQ4YTZhNTU2MzM4Mzk5NDBl****", result.NextVersionIdMarker);
        Assert.Equal("demo/", result.Prefix);
        Assert.Equal(true, result.IsTruncated);

        Assert.NotNull(result.Versions);
        Assert.Equal(2, result.Versions.Count);
        Assert.Equal("demo/README-CN.md", result.Versions[0].Key);
        Assert.Equal("CAEQEhiBgICDzK6NnBgiIGRlZWJhYmNlMGUxZDQ4YTZhNTU2MzM4Mzk5NDBl****", result.Versions[0].VersionId);
        Assert.Equal(false, result.Versions[0].IsLatest);
        Assert.Equal("2022/9/28 9:04:39", result.Versions[0].LastModified.ToString());
        Assert.Equal("\"E317049B40462DE37C422CE4FC1B****\"", result.Versions[0].ETag);
        Assert.Equal("Normal", result.Versions[0].Type);
        Assert.Equal(2943, result.Versions[0].Size);
        Assert.Equal("Standard", result.Versions[0].StorageClass);
        Assert.NotNull(result.Versions[0].Owner);
        Assert.Equal("150692521021****", result.Versions[0].Owner.Id);
        Assert.Equal("160692521021****", result.Versions[0].Owner.DisplayName);
        Assert.Equal("ongoing-request=\"false\", expiry-date=\"Thu, 24 Sep 2020 12:40:33 GMT\"", result.Versions[0].RestoreInfo);

        Assert.Equal("example-object-2.jpg", result.Versions[1].Key);
        Assert.Equal("", result.Versions[1].VersionId);
        Assert.Equal("2022/9/28 9:04:40", result.Versions[1].TransitionTime.ToString());

        Assert.NotNull(result.DeleteMarkers);
        Assert.Equal(2, result.DeleteMarkers.Count);
        Assert.Equal("demo/README-CN.md", result.DeleteMarkers[0].Key);
        Assert.Equal("CAEQFBiCgID3.86GohgiIDc4ZTE0NTNhZTc5MDQxYzBhYTU5MjY1ZDFjNGJm****", result.DeleteMarkers[0].VersionId);
        Assert.Equal(true, result.DeleteMarkers[0].IsLatest);
        Assert.Equal("2022/11/4 8:00:06", result.DeleteMarkers[0].LastModified.ToString());
        Assert.NotNull(result.DeleteMarkers[0].Owner);
        Assert.Equal("150692521021****", result.DeleteMarkers[0].Owner.Id);
        Assert.Equal("350692521021****", result.DeleteMarkers[0].Owner.DisplayName);

        Assert.Equal("demo/LICENSE", result.DeleteMarkers[1].Key);
        Assert.Equal("CAEQFBiBgMD0.86GohgiIGZmMmFlM2UwNjdlMzRiMGFhYjk4MjM1ZGUyZDY0****", result.DeleteMarkers[1].VersionId);

        Assert.NotNull(result.CommonPrefixes);
        Assert.Equal(2, result.CommonPrefixes.Count);
        Assert.Equal("demo/.git/", result.CommonPrefixes[0].Prefix);
        Assert.Equal("demo/.idea/", result.CommonPrefixes[1].Prefix);

        //full xml with url encode
        xml = """
              <ListVersionsResult>
                <Name>demo-bucket</Name>
                <Prefix>demo%2F</Prefix>
                <KeyMarker>demo%2F1</KeyMarker>
                <VersionIdMarker>version-id</VersionIdMarker>
                <MaxKeys>20</MaxKeys>
                <Delimiter>%2F</Delimiter>
                <EncodingType>url</EncodingType>
                <IsTruncated>true</IsTruncated>
                <NextKeyMarker>demo%2FREADME-CN.md</NextKeyMarker>
                <NextVersionIdMarker>CAEQEhiBgICDzK6NnBgiIGRlZWJhYmNlMGUxZDQ4YTZhNTU2MzM4Mzk5NDBl****</NextVersionIdMarker>
                  <Version>
                  <Key>demo%2FREADME-CN.md</Key>
                  <VersionId>CAEQEhiBgICDzK6NnBgiIGRlZWJhYmNlMGUxZDQ4YTZhNTU2MzM4Mzk5NDBl****</VersionId>
                  <IsLatest>false</IsLatest>
                  <LastModified>2022-09-28T09:04:39.000Z</LastModified>
                  <ETag>"E317049B40462DE37C422CE4FC1B****"</ETag>
                  <Type>Normal</Type>
                  <Size>2943</Size>
                  <StorageClass>Standard</StorageClass>
                  <Owner>
                    <ID>150692521021****</ID>
                    <DisplayName>160692521021****</DisplayName>
                  </Owner>
                  <RestoreInfo>ongoing-request="false", expiry-date="Thu, 24 Sep 2020 12:40:33 GMT"</RestoreInfo>
                  </Version>
                  <Version>
                      <Key>example-object-2.jpg</Key>
                      <VersionId/>
                      <IsLatest>true</IsLatest>
                      <LastModified>2019-08-09T12:03:09.000Z</LastModified>
                      <ETag>5B3C1A2E053D763E1B002CC607C5A0FE1****</ETag>
                      <Size>20</Size>
                      <StorageClass>STANDARD</StorageClass>
                      <Owner>
                          <ID>1250000000</ID>
                          <DisplayName>1250000000</DisplayName>
                      </Owner>
                      <RestoreInfo>ongoing-request="true"</RestoreInfo>
                  </Version>
                <DeleteMarker>
                  <Key>demo%2FREADME-CN.md</Key>
                  <VersionId>CAEQFBiCgID3.86GohgiIDc4ZTE0NTNhZTc5MDQxYzBhYTU5MjY1ZDFjNGJm****</VersionId>
                  <IsLatest>true</IsLatest>
                  <LastModified>2022-11-04T08:00:06.000Z</LastModified>
                  <Owner>
                    <ID>150692521021****</ID>
                    <DisplayName>350692521021****</DisplayName>
                  </Owner>
                </DeleteMarker>
                <DeleteMarker>
                    <Key>demo%2FLICENSE</Key>
                    <VersionId>CAEQFBiBgMD0.86GohgiIGZmMmFlM2UwNjdlMzRiMGFhYjk4MjM1ZGUyZDY0****</VersionId>
                    <IsLatest>true</IsLatest>
                    <LastModified>2022-11-04T08:00:06.000Z</LastModified>
                    <Owner>
                      <ID>150692521021****</ID>
                      <DisplayName>250692521021****</DisplayName>
                    </Owner>
                </DeleteMarker>
                <CommonPrefixes>
                  <Prefix>demo%2F.git%2F</Prefix>
                </CommonPrefixes>
                <CommonPrefixes>
                  <Prefix>demo%2F.idea%2F</Prefix>
                </CommonPrefixes>
            </ListVersionsResult>
""";
        output = new OperationOutput
        {
            StatusCode = 200,
            Status = "OK",
            Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
                { "x-oss-request-id", "123-id" },
                { "Content-Type", "application/xml" }
            },
            Body = new MemoryStream(Encoding.UTF8.GetBytes(xml))
        };
        baseResult = result; Serde.DeserializeOutput(ref baseResult, ref output, Serde.DeserializeListObjectVersions);

        Assert.Equal(200, result.StatusCode);
        Assert.Equal("OK", result.Status);
        Assert.Equal("123-id", result.RequestId);
        Assert.Equal(2, result.Headers.Count);
        Assert.Equal("application/xml", result.Headers["content-type"]);
        Assert.Equal("demo-bucket", result.Name);
        Assert.Equal(20, result.MaxKeys);
        Assert.Equal("/", result.Delimiter);
        Assert.Equal("url", result.EncodingType);
        Assert.Equal("demo/1", result.KeyMarker);
        Assert.Equal("version-id", result.VersionIdMarker);
        Assert.Equal("demo/README-CN.md", result.NextKeyMarker);
        Assert.Equal("CAEQEhiBgICDzK6NnBgiIGRlZWJhYmNlMGUxZDQ4YTZhNTU2MzM4Mzk5NDBl****", result.NextVersionIdMarker);
        Assert.Equal("demo/", result.Prefix);
        Assert.Equal(true, result.IsTruncated);

        Assert.NotNull(result.Versions);
        Assert.Equal(2, result.Versions.Count);
        Assert.Equal("demo/README-CN.md", result.Versions[0].Key);
        Assert.Equal("CAEQEhiBgICDzK6NnBgiIGRlZWJhYmNlMGUxZDQ4YTZhNTU2MzM4Mzk5NDBl****", result.Versions[0].VersionId);
        Assert.Equal(false, result.Versions[0].IsLatest);
        Assert.Equal("2022/9/28 9:04:39", result.Versions[0].LastModified.ToString());
        Assert.Equal("\"E317049B40462DE37C422CE4FC1B****\"", result.Versions[0].ETag);
        Assert.Equal("Normal", result.Versions[0].Type);
        Assert.Equal(2943, result.Versions[0].Size);
        Assert.Equal("Standard", result.Versions[0].StorageClass);
        Assert.NotNull(result.Versions[0].Owner);
        Assert.Equal("150692521021****", result.Versions[0].Owner.Id);
        Assert.Equal("160692521021****", result.Versions[0].Owner.DisplayName);
        Assert.Equal("ongoing-request=\"false\", expiry-date=\"Thu, 24 Sep 2020 12:40:33 GMT\"", result.Versions[0].RestoreInfo);

        Assert.Equal("example-object-2.jpg", result.Versions[1].Key);
        Assert.Equal("", result.Versions[1].VersionId);

        Assert.NotNull(result.DeleteMarkers);
        Assert.Equal(2, result.DeleteMarkers.Count);
        Assert.Equal("demo/README-CN.md", result.DeleteMarkers[0].Key);
        Assert.Equal("CAEQFBiCgID3.86GohgiIDc4ZTE0NTNhZTc5MDQxYzBhYTU5MjY1ZDFjNGJm****", result.DeleteMarkers[0].VersionId);
        Assert.Equal(true, result.DeleteMarkers[0].IsLatest);
        Assert.Equal("2022/11/4 8:00:06", result.DeleteMarkers[0].LastModified.ToString());
        Assert.NotNull(result.DeleteMarkers[0].Owner);
        Assert.Equal("150692521021****", result.DeleteMarkers[0].Owner.Id);
        Assert.Equal("350692521021****", result.DeleteMarkers[0].Owner.DisplayName);

        Assert.Equal("demo/LICENSE", result.DeleteMarkers[1].Key);
        Assert.Equal("CAEQFBiBgMD0.86GohgiIGZmMmFlM2UwNjdlMzRiMGFhYjk4MjM1ZGUyZDY0****", result.DeleteMarkers[1].VersionId);

        Assert.NotNull(result.CommonPrefixes);
        Assert.Equal(2, result.CommonPrefixes.Count);
        Assert.Equal("demo/.git/", result.CommonPrefixes[0].Prefix);
        Assert.Equal("demo/.idea/", result.CommonPrefixes[1].Prefix);
    }
}
