using System.Text;
using AlibabaCloud.OSS.V2.Models;
using AlibabaCloud.OSS.V2.Transform;

namespace AlibabaCloud.OSS.V2.UnitTests.Models;

public class ModelBucketBasicTest
{
    [Fact]
    public void TestPutBucketRequest()
    {
        var request = new PutBucketRequest();
        Assert.Empty(request.Headers);
        Assert.Empty(request.Parameters);
        Assert.Null(request.InnerBody);
        Assert.Equal("xml", request.BodyFormat);
        Assert.Null(request.Bucket);
        Assert.Null(request.Acl);
        Assert.Null(request.ResourceGroupId);
        Assert.Null(request.BucketTagging);
        Assert.Null(request.CreateBucketConfiguration);

        var input = new OperationInput();
        Serde.SerializeInput(request, ref input);

        Assert.Null(input.Headers);
        Assert.Null(input.Parameters);
        Assert.Null(input.Body);

        request = new PutBucketRequest
        {
            Bucket = "bucket",
            Acl = "private",
            ResourceGroupId = "rg-123",
            BucketTagging = "k1=v1&k2=v2",
            CreateBucketConfiguration = new CreateBucketConfiguration
            {
                DataRedundancyType = "LZR",
                StorageClass = "IA"
            }
        };
        Assert.Equal(3, request.Headers.Count);
        Assert.Equal("bucket", request.Bucket);
        Assert.Equal("private", request.Headers["x-oss-acl"]);
        Assert.Equal("private", request.Acl);
        Assert.Equal("rg-123", request.Headers["x-oss-resource-group-id"]);
        Assert.Equal("rg-123", request.ResourceGroupId);
        Assert.Equal("k1=v1&k2=v2", request.Headers["x-oss-bucket-tagging"]);
        Assert.Equal("k1=v1&k2=v2", request.BucketTagging);
        Assert.NotNull(request.InnerBody);
        Assert.Equal("LZR", request.CreateBucketConfiguration.DataRedundancyType);
        Assert.Equal("IA", request.CreateBucketConfiguration.StorageClass);

        input = new OperationInput();
        Serde.SerializeInput(request, ref input);

        Assert.NotNull(input.Headers);
        Assert.Equal(3, input.Headers.Count);
        Assert.Equal("private", input.Headers["x-oss-acl"]);
        Assert.Equal("rg-123", input.Headers["x-oss-resource-group-id"]);
        Assert.Equal("k1=v1&k2=v2", input.Headers["x-oss-bucket-tagging"]);
        Assert.NotNull(input.Body);
        using var reader = new StreamReader(input.Body);
        var xml = """
<?xml version="1.0" encoding="utf-8"?>
<CreateBucketConfiguration>
  <StorageClass>IA</StorageClass>
  <DataRedundancyType>LZR</DataRedundancyType>
</CreateBucketConfiguration>
""";
        Assert.Equal(xml, reader.ReadToEnd());
    }

    [Fact]
    public void TestPutBucketResult()
    {
        var result = new PutBucketResult();
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
    public void TestDeleteBucketRequest()
    {
        var request = new DeleteBucketRequest();
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

        request = new DeleteBucketRequest
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
    public void TestDeleteBucketResult()
    {
        var result = new DeleteBucketResult();
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
    public void TestGetBucketStatRequest()
    {
        var request = new GetBucketStatRequest();
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

        request = new GetBucketStatRequest
        {
            Bucket = "bucket",
        };
        Assert.Empty(request.Headers);
        Assert.Equal("bucket", request.Bucket);

        input = new OperationInput();
        Serde.SerializeInput(request, ref input);

        Assert.Null(input.Headers);
        Assert.Null(input.Parameters);
        Assert.Null(input.Body);
    }

    [Fact]
    public void TestGetBucketStatResult()
    {
        var result = new GetBucketStatResult();
        Assert.Equal(0, result.StatusCode);
        Assert.Equal("", result.Status);
        Assert.Equal("", result.RequestId);
        Assert.Empty(result.Headers);
        Assert.Equal("xml", result.BodyFormat);
        Assert.Equal(typeof(BucketStat), result.BodyType);
        Assert.Null(result.BucketStat);

        //empty xml
        var xml = """
                  <?xml version="1.0" encoding="utf-8"?>
                  <BucketStat>
                  </BucketStat>
                  """;

        var output = new OperationOutput
        {
            StatusCode = 200,
            Status = "OK",
            Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
                {"x-oss-request-id", "123-id"},
                {"Content-Type","application/xml"}
            },
            Body = new MemoryStream(Encoding.UTF8.GetBytes(xml))
        };
        ResultModel baseResult = result;
        Serde.DeserializeOutput(ref baseResult, ref output, Serde.DeserializerXmlBody);

        Assert.Equal(200, result.StatusCode);
        Assert.Equal("OK", result.Status);
        Assert.Equal("123-id", result.RequestId);
        Assert.Equal(2, result.Headers.Count);
        Assert.Equal("application/xml", result.Headers["content-type"]);
        Assert.NotNull(result.BucketStat);
        Assert.Null(result.BucketStat.ArchiveObjectCount);
        Assert.Null(result.BucketStat.ArchiveRealStorage);
        Assert.Null(result.BucketStat.ArchiveStorage);
        Assert.Null(result.BucketStat.ColdArchiveObjectCount);
        Assert.Null(result.BucketStat.ColdArchiveRealStorage);
        Assert.Null(result.BucketStat.ColdArchiveStorage);
        Assert.Null(result.BucketStat.DeepColdArchiveObjectCount);
        Assert.Null(result.BucketStat.DeepColdArchiveRealStorage);
        Assert.Null(result.BucketStat.DeepColdArchiveStorage);
        Assert.Null(result.BucketStat.DeleteMarkerCount);
        Assert.Null(result.BucketStat.InfrequentAccessObjectCount);
        Assert.Null(result.BucketStat.InfrequentAccessRealStorage);
        Assert.Null(result.BucketStat.InfrequentAccessStorage);
        Assert.Null(result.BucketStat.LastModifiedTime);
        Assert.Null(result.BucketStat.LiveChannelCount);
        Assert.Null(result.BucketStat.MultipartPartCount);
        Assert.Null(result.BucketStat.ObjectCount);
        Assert.Null(result.BucketStat.StandardObjectCount);
        Assert.Null(result.BucketStat.StandardStorage);
        Assert.Null(result.BucketStat.Storage);

        //full xml
        xml = """
                  <?xml version="1.0" encoding="utf-8"?>
                  <BucketStat>
                    <Storage>1600</Storage>
                    <ObjectCount>230</ObjectCount>
                    <MultipartUploadCount>40</MultipartUploadCount>
                    <MultipartPartCount>41</MultipartPartCount>
                    <LiveChannelCount>4</LiveChannelCount>
                    <LastModifiedTime>1643341269</LastModifiedTime>
                    <StandardStorage>430</StandardStorage>
                    <StandardObjectCount>66</StandardObjectCount>
                    <InfrequentAccessStorage>2359296</InfrequentAccessStorage>
                    <InfrequentAccessRealStorage>360</InfrequentAccessRealStorage>
                    <InfrequentAccessObjectCount>54</InfrequentAccessObjectCount>
                    <ArchiveStorage>2949120</ArchiveStorage>
                    <ArchiveRealStorage>450</ArchiveRealStorage>
                    <ArchiveObjectCount>74</ArchiveObjectCount>
                    <ColdArchiveStorage>2359296</ColdArchiveStorage>
                    <ColdArchiveRealStorage>3610</ColdArchiveRealStorage>
                    <ColdArchiveObjectCount>36</ColdArchiveObjectCount>
                    <ColdArchiveStorage>2359296</ColdArchiveStorage>
                    <DeepColdArchiveStorage>23594961</DeepColdArchiveStorage>
                    <DeepColdArchiveRealStorage>10</DeepColdArchiveRealStorage>
                    <DeepColdArchiveObjectCount>16</DeepColdArchiveObjectCount>
                    <DeleteMarkerCount>1234355467575856878</DeleteMarkerCount>
                  </BucketStat>
                  """;

        output = new OperationOutput
        {
            StatusCode = 200,
            Status = "OK",
            Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
                {"x-oss-request-id", "123-id"},
                {"Content-Type","application/xml"}
            },
            Body = new MemoryStream(Encoding.UTF8.GetBytes(xml))
        };
        baseResult = result;
        Serde.DeserializeOutput(ref baseResult, ref output, Serde.DeserializerXmlBody);

        Assert.Equal(200, result.StatusCode);
        Assert.Equal("OK", result.Status);
        Assert.Equal("123-id", result.RequestId);
        Assert.Equal(2, result.Headers.Count);
        Assert.Equal("application/xml", result.Headers["content-type"]);
        Assert.NotNull(result.BucketStat);
        Assert.Equal(74, result.BucketStat.ArchiveObjectCount);
        Assert.Equal(450, result.BucketStat.ArchiveRealStorage);
        Assert.Equal(2949120, result.BucketStat.ArchiveStorage);
        Assert.Equal(36, result.BucketStat.ColdArchiveObjectCount);
        Assert.Equal(3610, result.BucketStat.ColdArchiveRealStorage);
        Assert.Equal(2359296, result.BucketStat.ColdArchiveStorage);
        Assert.Equal(16, result.BucketStat.DeepColdArchiveObjectCount);
        Assert.Equal(10, result.BucketStat.DeepColdArchiveRealStorage);
        Assert.Equal(23594961, result.BucketStat.DeepColdArchiveStorage);
        Assert.Equal(1234355467575856878, result.BucketStat.DeleteMarkerCount);
        Assert.Equal(54, result.BucketStat.InfrequentAccessObjectCount);
        Assert.Equal(360, result.BucketStat.InfrequentAccessRealStorage);
        Assert.Equal(2359296, result.BucketStat.InfrequentAccessStorage);
        Assert.Equal(1643341269, result.BucketStat.LastModifiedTime);
        Assert.Equal(4, result.BucketStat.LiveChannelCount);
        Assert.Equal(40, result.BucketStat.MultipartUploadCount);
        Assert.Equal(41, result.BucketStat.MultipartPartCount);
        Assert.Equal(230, result.BucketStat.ObjectCount);
        Assert.Equal(66, result.BucketStat.StandardObjectCount);
        Assert.Equal(430, result.BucketStat.StandardStorage);
        Assert.Equal(1600, result.BucketStat.Storage);
    }

    [Fact]
    public void TestGetBucketInfoRequest()
    {
        var request = new GetBucketInfoRequest();
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

        request = new GetBucketInfoRequest
        {
            Bucket = "bucket",
        };
        Assert.Empty(request.Headers);
        Assert.Equal("bucket", request.Bucket);

        input = new OperationInput();
        Serde.SerializeInput(request, ref input);

        Assert.Null(input.Headers);
        Assert.Null(input.Parameters);
        Assert.Null(input.Body);
    }

    [Fact]
    public void TestGetBucketInfoResult()
    {
        var result = new GetBucketInfoResult();
        Assert.Equal(0, result.StatusCode);
        Assert.Equal("", result.Status);
        Assert.Equal("", result.RequestId);
        Assert.Empty(result.Headers);
        Assert.Equal("xml", result.BodyFormat);
        Assert.Equal(typeof(XmlBucketInfo), result.BodyType);
        Assert.Null(result.BucketInfo);

        //empty xml
        var xml = """
                  <?xml version="1.0" encoding="utf-8"?>
                  <BucketInfo>
                  </BucketInfo>
                  """;

        var output = new OperationOutput
        {
            StatusCode = 200,
            Status = "OK",
            Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
                {"x-oss-request-id", "123-id"},
                {"Content-Type","application/xml"}
            },
            Body = new MemoryStream(Encoding.UTF8.GetBytes(xml))
        };
        ResultModel baseResult = result;
        Serde.DeserializeOutput(ref baseResult, ref output, Serde.DeserializerXmlBody);

        Assert.Equal(200, result.StatusCode);
        Assert.Equal("OK", result.Status);
        Assert.Equal("123-id", result.RequestId);
        Assert.Equal(2, result.Headers.Count);
        Assert.Equal("application/xml", result.Headers["content-type"]);
        Assert.Null(result.BucketInfo);

        //full xml
        xml = """
              <?xml version="1.0" encoding="utf-8"?>
              <BucketInfo>
                <Bucket>
                  <AccessMonitor>Enabled</AccessMonitor>
                  <CreationDate>2013-07-31T10:56:21.000Z</CreationDate>
                  <ExtranetEndpoint>oss-cn-hangzhou.aliyuncs.com</ExtranetEndpoint>
                  <IntranetEndpoint>oss-cn-hangzhou-internal.aliyuncs.com</IntranetEndpoint>
                  <Location>oss-cn-hangzhou</Location>
                  <StorageClass>Standard</StorageClass>
                  <TransferAcceleration>Disabled</TransferAcceleration>
                  <CrossRegionReplication>Disabled</CrossRegionReplication>
                  <DataRedundancyType>LRS</DataRedundancyType>
                  <Name>oss-example</Name>
                  <ResourceGroupId>rg-aek27tc********</ResourceGroupId>
                  <Owner>
                    <DisplayName>username</DisplayName>
                    <ID>27183473914****</ID>
                  </Owner>
                  <AccessControlList>
                    <Grant>private</Grant>
                  </AccessControlList> 
                  <ServerSideEncryptionRule>
                      <SSEAlgorithm>KMS</SSEAlgorithm>
                      <KMSMasterKeyID>shUhih687675***32edghadg</KMSMasterKeyID>
                      <KMSDataEncryption>SM4</KMSDataEncryption>
                  </ServerSideEncryptionRule>
                  <BucketPolicy>
                    <LogBucket>examplebucket</LogBucket>
                    <LogPrefix>log/</LogPrefix>
                  </BucketPolicy>
                  <Comment>test</Comment>
                  <Versioning>Enabled</Versioning>
                  <BlockPublicAccess>true</BlockPublicAccess>
                </Bucket>
              </BucketInfo>
              """;

        output = new OperationOutput
        {
            StatusCode = 200,
            Status = "OK",
            Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
                {"x-oss-request-id", "123-id"},
                {"Content-Type","application/xml"}
            },
            Body = new MemoryStream(Encoding.UTF8.GetBytes(xml))
        };
        baseResult = result;
        Serde.DeserializeOutput(ref baseResult, ref output, Serde.DeserializerXmlBody);

        Assert.Equal(200, result.StatusCode);
        Assert.Equal("OK", result.Status);
        Assert.Equal("123-id", result.RequestId);
        Assert.Equal(2, result.Headers.Count);
        Assert.Equal("application/xml", result.Headers["content-type"]);
        Assert.NotNull(result.BucketInfo);
        Assert.Equal("oss-example", result.BucketInfo.Name);
        Assert.Equal("Enabled", result.BucketInfo.AccessMonitor);
        Assert.Equal("oss-cn-hangzhou", result.BucketInfo.Location);
        //Assert.Equal("Enabled", result.BucketInfo.CreationDate);
        Assert.Equal("oss-cn-hangzhou.aliyuncs.com", result.BucketInfo.ExtranetEndpoint);
        Assert.Equal("oss-cn-hangzhou-internal.aliyuncs.com", result.BucketInfo.IntranetEndpoint);
        Assert.Equal("private", result.BucketInfo.AccessControlList!.Grant);
        Assert.Equal("LRS", result.BucketInfo.DataRedundancyType);
        Assert.Equal("27183473914****", result.BucketInfo.Owner!.Id);
        Assert.Equal("username", result.BucketInfo.Owner!.DisplayName);
        Assert.Equal("Standard", result.BucketInfo.StorageClass);
        Assert.Equal("rg-aek27tc********", result.BucketInfo.ResourceGroupId);
        Assert.Equal("shUhih687675***32edghadg", result.BucketInfo.ServerSideEncryptionRule!.KMSMasterKeyID);
        Assert.Equal("KMS", result.BucketInfo.ServerSideEncryptionRule!.SSEAlgorithm);
        Assert.Equal("SM4", result.BucketInfo.ServerSideEncryptionRule!.KMSDataEncryption);
        Assert.Equal("Enabled", result.BucketInfo.Versioning);
        Assert.Equal("Disabled", result.BucketInfo.TransferAcceleration);
        Assert.Equal("Disabled", result.BucketInfo.CrossRegionReplication);
        Assert.Equal("examplebucket", result.BucketInfo.BucketPolicy!.LogBucket);
        Assert.Equal("log/", result.BucketInfo.BucketPolicy!.LogPrefix);
        Assert.Equal(true, result.BucketInfo.BlockPublicAccess);
    }

    [Fact]
    public void TestGetBucketLocationRequest()
    {
        var request = new GetBucketLocationRequest();
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

        request = new GetBucketLocationRequest
        {
            Bucket = "bucket",
        };
        Assert.Empty(request.Headers);
        Assert.Equal("bucket", request.Bucket);

        input = new OperationInput();
        Serde.SerializeInput(request, ref input);

        Assert.Null(input.Headers);
        Assert.Null(input.Parameters);
        Assert.Null(input.Body);
    }

    [Fact]
    public void TestGetBucketLocationResult()
    {
        var result = new GetBucketLocationResult();
        Assert.Equal(0, result.StatusCode);
        Assert.Equal("", result.Status);
        Assert.Equal("", result.RequestId);
        Assert.Empty(result.Headers);
        Assert.Equal("xml", result.BodyFormat);
        Assert.Equal(typeof(XmlLocationConstraint), result.BodyType);
        Assert.Null(result.LocationConstraint);

        //empty xml
        var xml = """
                  <?xml version="1.0" encoding="utf-8"?>
                  <LocationConstraint>
                  </LocationConstraint>
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
        Serde.DeserializeOutput(ref baseResult, ref output, Serde.DeserializerXmlBody);

        Assert.Equal(200, result.StatusCode);
        Assert.Equal("OK", result.Status);
        Assert.Equal("123-id", result.RequestId);
        Assert.Equal(2, result.Headers.Count);
        Assert.Equal("application/xml", result.Headers["content-type"]);
        Assert.Null(result.LocationConstraint);

        //full xml
        xml = """
                  <?xml version="1.0" encoding="utf-8"?>
                  <LocationConstraint>oss-cn-hangzhou</LocationConstraint>
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
        baseResult = result;
        Serde.DeserializeOutput(ref baseResult, ref output, Serde.DeserializerXmlBody);

        Assert.Equal(200, result.StatusCode);
        Assert.Equal("OK", result.Status);
        Assert.Equal("123-id", result.RequestId);
        Assert.Equal(2, result.Headers.Count);
        Assert.Equal("application/xml", result.Headers["content-type"]);
        Assert.Equal("oss-cn-hangzhou", result.LocationConstraint);
    }

    [Fact]
    public void TestListObjectsRequest()
    {
        var request = new ListObjectsRequest();
        Assert.Empty(request.Headers);
        Assert.Empty(request.Parameters);
        Assert.Null(request.InnerBody);
        Assert.Equal("", request.BodyFormat);
        Assert.Null(request.Bucket);
        Assert.Null(request.Delimiter);
        Assert.Null(request.EncodingType);
        Assert.Null(request.Marker);
        Assert.Null(request.MaxKeys);
        Assert.Null(request.Prefix);

        var input = new OperationInput();
        Serde.SerializeInput(request, ref input);

        Assert.Null(input.Headers);
        Assert.Null(input.Parameters);
        Assert.Null(input.Body);

        request = new ListObjectsRequest
        {
            Bucket = "bucket",
            Delimiter = "/",
            EncodingType = "url",
            Marker = "key-01",
            MaxKeys = 10001,
            Prefix = "prefix-01",
        };
        Assert.Empty(request.Headers);
        Assert.Equal("bucket", request.Bucket);
        Assert.Equal("/", request.Delimiter);
        Assert.Equal("url", request.EncodingType);
        Assert.Equal("key-01", request.Marker);
        Assert.Equal(10001, request.MaxKeys);
        Assert.Equal("prefix-01", request.Prefix);

        input = new OperationInput();
        Serde.SerializeInput(request, ref input);

        Assert.Null(input.Headers);
        Assert.Null(input.Body);
        Assert.NotNull(input.Parameters);
        Assert.Equal(5, input.Parameters.Count);
        Assert.Equal("/", input.Parameters["delimiter"]);
        Assert.Equal("url", input.Parameters["encoding-type"]);
        Assert.Equal("key-01", input.Parameters["marker"]);
        Assert.Equal("10001", input.Parameters["max-keys"]);
        Assert.Equal("prefix-01", input.Parameters["prefix"]);
    }

    [Fact]
    public void TestListObjectsResult()
    {
        var result = new ListObjectsResult();
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
        Assert.Null(result.Marker);
        Assert.Null(result.NextMarker);
        Assert.Null(result.Contents);
        Assert.Null(result.Prefix);
        Assert.Null(result.IsTruncated);
        Assert.Null(result.CommonPrefixes);

        //empty xml
        var xml = """
                  <?xml version="1.0" encoding="utf-8"?>
                  <ListBucketResult>
                  </ListBucketResult>
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
        Serde.DeserializeOutput(ref baseResult, ref output, Serde.DeserializeListObjects);

        Assert.Equal(200, result.StatusCode);
        Assert.Equal("OK", result.Status);
        Assert.Equal("123-id", result.RequestId);
        Assert.Equal(2, result.Headers.Count);
        Assert.Equal("application/xml", result.Headers["content-type"]);
        Assert.Null(result.Name);
        Assert.Null(result.MaxKeys);
        Assert.Null(result.Delimiter);
        Assert.Null(result.EncodingType);
        Assert.Null(result.Marker);
        Assert.Null(result.NextMarker);
        Assert.NotNull(result.Contents);
        Assert.Empty(result.Contents);
        Assert.Null(result.Prefix);
        Assert.Null(result.IsTruncated);
        Assert.NotNull(result.CommonPrefixes);
        Assert.Empty(result.CommonPrefixes);

        //full xml without url encode
        xml = """
              <ListBucketResult>
                <Name>example-bucket</Name>
                <Prefix>aaa</Prefix>
                <Marker>CgJiYw--</Marker>
                <MaxKeys>100</MaxKeys>
                <Delimiter>/</Delimiter>
                <EncodingType></EncodingType>
                <IsTruncated>false</IsTruncated>
                <NextMarker>NextChR1c2V</NextMarker>
                <Contents>
                      <Key>example-object11.txt</Key>
                      <LastModified>2020-06-22T11:42:32.000Z</LastModified>
                      <ETag>"5B3C1A2E053D763E1B002CC607C5A0FE1****"</ETag>
                      <Type>Normal</Type>
                      <Size>344606</Size>
                      <StorageClass>ColdArchive</StorageClass>
                      <Owner>
                          <ID>0022012****</ID>
                          <DisplayName>user-example</DisplayName>
                      </Owner>
                      <RestoreInfo>ongoing-request="true"</RestoreInfo>
                </Contents>
                <Contents>
                      <Key>example-object2.txt</Key>
                      <LastModified>2023-12-08T08:12:20.000Z</LastModified>
                      <ETag>"5B3C1A2E053D763E1B002CC607C5A0FE1****"</ETag>
                      <Type>Normal2</Type>
                      <Size>344607</Size>
                      <StorageClass>DeepColdArchive</StorageClass>
                      <Owner>
                          <ID>0022012****22</ID>
                          <DisplayName>user-example22</DisplayName>
                      </Owner>
                      <RestoreInfo>ongoing-request="false", expiry-date="Sat, 05 Nov 2022 07:38:08 GMT"</RestoreInfo>
                      <TransitionTime>2023-12-08T08:12:21.000Z</TransitionTime>
                </Contents>
                <CommonPrefixes>
                      <Prefix>a/b/</Prefix>
                </CommonPrefixes>
              </ListBucketResult>
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
        baseResult = result; Serde.DeserializeOutput(ref baseResult, ref output, Serde.DeserializeListObjects);

        Assert.Equal(200, result.StatusCode);
        Assert.Equal("OK", result.Status);
        Assert.Equal("123-id", result.RequestId);
        Assert.Equal(2, result.Headers.Count);
        Assert.Equal("application/xml", result.Headers["content-type"]);
        Assert.Equal("example-bucket", result.Name);
        Assert.Equal(100, result.MaxKeys);
        Assert.Equal("/", result.Delimiter);
        Assert.Equal("", result.EncodingType);
        Assert.Equal("CgJiYw--", result.Marker);
        Assert.Equal("NextChR1c2V", result.NextMarker);
        Assert.NotNull(result.Contents);
        Assert.Equal(2, result.Contents.Count);
        Assert.Equal("example-object11.txt", result.Contents[0].Key);
        Assert.Equal(new DateTime(2020, 6, 22, 11, 42, 32), result.Contents[0].LastModified);
        Assert.Equal("\"5B3C1A2E053D763E1B002CC607C5A0FE1****\"", result.Contents[0].ETag);
        Assert.Equal("Normal", result.Contents[0].Type);
        Assert.Equal(344606, result.Contents[0].Size);
        Assert.Equal("ColdArchive", result.Contents[0].StorageClass);
        Assert.NotNull(result.Contents[0].Owner);
        Assert.Equal("0022012****", result.Contents[0].Owner.Id);
        Assert.Equal("user-example", result.Contents[0].Owner.DisplayName);
        Assert.Equal("ongoing-request=\"true\"", result.Contents[0].RestoreInfo);

        Assert.Equal("example-object2.txt", result.Contents[1].Key);
        Assert.Equal(new DateTime(2023, 12, 8, 8, 12, 20), result.Contents[1].LastModified);
        Assert.Equal(new DateTime(2023, 12, 8, 8, 12, 21), result.Contents[1].TransitionTime);

        Assert.Equal("aaa", result.Prefix);
        Assert.Equal(false, result.IsTruncated);
        Assert.NotNull(result.CommonPrefixes);
        Assert.Single(result.CommonPrefixes);
        Assert.Equal("a/b/", result.CommonPrefixes[0].Prefix);

        //full xml with url encode
        xml = """
              <ListBucketResult>
                <Name>example-bucket</Name>
                <Prefix>aaa%2F%2B%23%3F%2F12</Prefix>
                <Marker>marker%2F123%2F</Marker>
                <MaxKeys>100</MaxKeys>
                <Delimiter>%2F</Delimiter>
                <EncodingType>url</EncodingType>
                <IsTruncated>false</IsTruncated>
                <NextMarker>next-marker%2F123%2F</NextMarker>
                <Contents>
                      <Key>key%2F123%2F1.txt</Key>
                      <LastModified>2020-06-22T11:42:32.000Z</LastModified>
                      <ETag>"5B3C1A2E053D763E1B002CC607C5A0FE1****"</ETag>
                      <Type>Normal</Type>
                      <Size>344606</Size>
                      <StorageClass>ColdArchive</StorageClass>
                      <Owner>
                          <ID>0022012****</ID>
                          <DisplayName>user-example</DisplayName>
                      </Owner>
                      <RestoreInfo>ongoing-request="true"</RestoreInfo>
                </Contents>
                <CommonPrefixes>
                      <Prefix>a%2Fb%2F</Prefix>
                </CommonPrefixes>
              </ListBucketResult>
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
        baseResult = result; Serde.DeserializeOutput(ref baseResult, ref output, Serde.DeserializeListObjects);

        Assert.Equal(200, result.StatusCode);
        Assert.Equal("OK", result.Status);
        Assert.Equal("123-id", result.RequestId);
        Assert.Equal(2, result.Headers.Count);
        Assert.Equal("application/xml", result.Headers["content-type"]);
        Assert.Equal("example-bucket", result.Name);
        Assert.Equal(100, result.MaxKeys);
        Assert.Equal("/", result.Delimiter);
        Assert.Equal("url", result.EncodingType);
        Assert.Equal("marker/123/", result.Marker);
        Assert.Equal("next-marker/123/", result.NextMarker);
        Assert.NotNull(result.Contents);
        Assert.Single(result.Contents);
        Assert.Equal("key/123/1.txt", result.Contents[0].Key);
        Assert.Equal(new DateTime(2020, 6, 22, 11, 42, 32), result.Contents[0].LastModified);
        Assert.Equal("\"5B3C1A2E053D763E1B002CC607C5A0FE1****\"", result.Contents[0].ETag);
        Assert.Equal("Normal", result.Contents[0].Type);
        Assert.Equal(344606, result.Contents[0].Size);
        Assert.Equal("ColdArchive", result.Contents[0].StorageClass);
        Assert.NotNull(result.Contents[0].Owner);
        Assert.Equal("0022012****", result.Contents[0].Owner.Id);
        Assert.Equal("user-example", result.Contents[0].Owner.DisplayName);
        Assert.Equal("ongoing-request=\"true\"", result.Contents[0].RestoreInfo);
        Assert.Equal("aaa/+#?/12", result.Prefix);
        Assert.Equal(false, result.IsTruncated);
        Assert.NotNull(result.CommonPrefixes);
        Assert.Single(result.CommonPrefixes);
        Assert.Equal("a/b/", result.CommonPrefixes[0].Prefix);
    }

    [Fact]
    public void TestListObjectsV2Request()
    {
        var request = new ListObjectsV2Request();
        Assert.Empty(request.Headers);
        Assert.Empty(request.Parameters);
        Assert.Null(request.InnerBody);
        Assert.Equal("", request.BodyFormat);
        Assert.Null(request.Bucket);
        Assert.Null(request.Delimiter);
        Assert.Null(request.EncodingType);
        Assert.Null(request.StartAfter);
        Assert.Null(request.MaxKeys);
        Assert.Null(request.Prefix);
        Assert.Null(request.ContinuationToken);
        Assert.Null(request.FetchOwner);

        var input = new OperationInput();
        Serde.SerializeInput(request, ref input);

        Assert.Null(input.Headers);
        Assert.Null(input.Parameters);
        Assert.Null(input.Body);

        request = new ListObjectsV2Request
        {
            Bucket = "bucket",
            Delimiter = "/",
            EncodingType = "url",
            StartAfter = "key-01",
            ContinuationToken = "token-01",
            MaxKeys = 10001,
            Prefix = "prefix-01",
            FetchOwner = true
        };
        Assert.Empty(request.Headers);
        Assert.Equal("bucket", request.Bucket);
        Assert.Equal("/", request.Delimiter);
        Assert.Equal("url", request.EncodingType);
        Assert.Equal("key-01", request.StartAfter);
        Assert.Equal(10001, request.MaxKeys);
        Assert.Equal("prefix-01", request.Prefix);
        Assert.Equal("token-01", request.ContinuationToken);
        Assert.Equal(true, request.FetchOwner);

        input = new OperationInput();
        Serde.SerializeInput(request, ref input);

        Assert.Null(input.Headers);
        Assert.Null(input.Body);
        Assert.NotNull(input.Parameters);
        Assert.Equal(7, input.Parameters.Count);
        Assert.Equal("/", input.Parameters["delimiter"]);
        Assert.Equal("url", input.Parameters["encoding-type"]);
        Assert.Equal("key-01", input.Parameters["start-after"]);
        Assert.Equal("10001", input.Parameters["max-keys"]);
        Assert.Equal("prefix-01", input.Parameters["prefix"]);
        Assert.Equal("token-01", input.Parameters["continuation-token"]);
        Assert.Equal("true", input.Parameters["fetch-owner"]);
    }

    [Fact]
    public void TestListObjectsV2Result()
    {
        var result = new ListObjectsV2Result();
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
        Assert.Null(result.StartAfter);
        Assert.Null(result.ContinuationToken);
        Assert.Null(result.NextContinuationToken);
        Assert.Null(result.Contents);
        Assert.Null(result.KeyCount);
        Assert.Null(result.Prefix);
        Assert.Null(result.IsTruncated);
        Assert.Null(result.CommonPrefixes);

        //empty xml
        var xml = """
                  <?xml version="1.0" encoding="utf-8"?>
                  <ListBucketResult>
                  </ListBucketResult>
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
        Serde.DeserializeOutput(ref baseResult, ref output, Serde.DeserializeListObjectsV2);

        Assert.Equal(200, result.StatusCode);
        Assert.Equal("OK", result.Status);
        Assert.Equal("123-id", result.RequestId);
        Assert.Equal(2, result.Headers.Count);
        Assert.Equal("application/xml", result.Headers["content-type"]);
        Assert.Null(result.Name);
        Assert.Null(result.MaxKeys);
        Assert.Null(result.Delimiter);
        Assert.Null(result.EncodingType);
        Assert.Null(result.StartAfter);
        Assert.Null(result.ContinuationToken);
        Assert.Null(result.NextContinuationToken);
        Assert.NotNull(result.Contents);
        Assert.Empty(result.Contents);
        Assert.Null(result.KeyCount);
        Assert.Null(result.Prefix);
        Assert.Null(result.IsTruncated);
        Assert.NotNull(result.CommonPrefixes);
        Assert.Empty(result.CommonPrefixes);

        //full xml without url encode
        xml = """
              <ListBucketResult>
                <Name>example-bucket</Name>
                <Prefix>aaa</Prefix>
                <ContinuationToken>CgJiYw--</ContinuationToken>
                <MaxKeys>100</MaxKeys>
                <Delimiter>/</Delimiter>
                <StartAfter>b</StartAfter>
                <EncodingType></EncodingType>
                <IsTruncated>false</IsTruncated>
                <NextContinuationToken>NextChR1c2V</NextContinuationToken>
                <Contents>
                      <Key>example-object11.txt</Key>
                      <LastModified>2020-06-22T11:42:32.000Z</LastModified>
                      <ETag>"5B3C1A2E053D763E1B002CC607C5A0FE1****"</ETag>
                      <Type>Normal</Type>
                      <Size>344606</Size>
                      <StorageClass>ColdArchive</StorageClass>
                      <Owner>
                          <ID>0022012****</ID>
                          <DisplayName>user-example</DisplayName>
                      </Owner>
                      <RestoreInfo>ongoing-request="true"</RestoreInfo>
                </Contents>
                <Contents>
                      <Key>example-object2.txt</Key>
                      <LastModified>2023-12-08T08:12:20.000Z</LastModified>
                      <ETag>"5B3C1A2E053D763E1B002CC607C5A0FE1****"</ETag>
                      <Type>Normal2</Type>
                      <Size>344607</Size>
                      <StorageClass>DeepColdArchive</StorageClass>
                      <Owner>
                          <ID>0022012****22</ID>
                          <DisplayName>user-example22</DisplayName>
                      </Owner>
                      <RestoreInfo>ongoing-request="false", expiry-date="Sat, 05 Nov 2022 07:38:08 GMT"</RestoreInfo>
                </Contents>
                <CommonPrefixes>
                      <Prefix>a/b/</Prefix>
                </CommonPrefixes>
                <KeyCount>3</KeyCount>
              </ListBucketResult>
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
        baseResult = result; Serde.DeserializeOutput(ref baseResult, ref output, Serde.DeserializeListObjectsV2);

        Assert.Equal(200, result.StatusCode);
        Assert.Equal("OK", result.Status);
        Assert.Equal("123-id", result.RequestId);
        Assert.Equal(2, result.Headers.Count);
        Assert.Equal("application/xml", result.Headers["content-type"]);
        Assert.Equal("example-bucket", result.Name);
        Assert.Equal(100, result.MaxKeys);
        Assert.Equal("/", result.Delimiter);
        Assert.Equal("", result.EncodingType);
        Assert.Equal("b", result.StartAfter);
        Assert.Equal("CgJiYw--", result.ContinuationToken);
        Assert.Equal("NextChR1c2V", result.NextContinuationToken);
        Assert.NotNull(result.Contents);
        Assert.Equal(2, result.Contents.Count);
        Assert.Equal("example-object11.txt", result.Contents[0].Key);
        Assert.Equal(new DateTime(2020, 6, 22, 11, 42, 32), result.Contents[0].LastModified);
        Assert.Equal("\"5B3C1A2E053D763E1B002CC607C5A0FE1****\"", result.Contents[0].ETag);
        Assert.Equal("Normal", result.Contents[0].Type);
        Assert.Equal(344606, result.Contents[0].Size);
        Assert.Equal("ColdArchive", result.Contents[0].StorageClass);
        Assert.NotNull(result.Contents[0].Owner);
        Assert.Equal("0022012****", result.Contents[0].Owner.Id);
        Assert.Equal("user-example", result.Contents[0].Owner.DisplayName);
        Assert.Equal("ongoing-request=\"true\"", result.Contents[0].RestoreInfo);
        Assert.Equal("aaa", result.Prefix);
        Assert.Equal(false, result.IsTruncated);
        Assert.NotNull(result.CommonPrefixes);
        Assert.Single(result.CommonPrefixes);
        Assert.Equal("a/b/", result.CommonPrefixes[0].Prefix);
        Assert.Equal(3, result.KeyCount);

        //full xml with url encode
        xml = """
              <ListBucketResult>
                <Name>example-bucket</Name>
                <Prefix>aaa%2F%2B%23%3F%2F12</Prefix>
                <StartAfter>start%2F123%2F</StartAfter>
                <MaxKeys>100</MaxKeys>
                <Delimiter>%2F</Delimiter>
                <EncodingType>url</EncodingType>
                <IsTruncated>false</IsTruncated>
                <ContinuationToken>marker%2F123%2F</ContinuationToken>
                <NextContinuationToken>next-marker%2F123%2F</NextContinuationToken>
                <Contents>
                      <Key>key%2F123%2F1.txt</Key>
                      <LastModified>2020-06-22T11:42:32.000Z</LastModified>
                      <ETag>"5B3C1A2E053D763E1B002CC607C5A0FE1****"</ETag>
                      <Type>Normal</Type>
                      <Size>344606</Size>
                      <StorageClass>ColdArchive</StorageClass>
                      <Owner>
                          <ID>0022012****</ID>
                          <DisplayName>user-example</DisplayName>
                      </Owner>
                      <RestoreInfo>ongoing-request="true"</RestoreInfo>
                </Contents>
                <CommonPrefixes>
                      <Prefix>a%2Fb%2F</Prefix>
                </CommonPrefixes>
                <KeyCount>3</KeyCount>
              </ListBucketResult>
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
        baseResult = result; Serde.DeserializeOutput(ref baseResult, ref output, Serde.DeserializeListObjectsV2);

        Assert.Equal(200, result.StatusCode);
        Assert.Equal("OK", result.Status);
        Assert.Equal("123-id", result.RequestId);
        Assert.Equal(2, result.Headers.Count);
        Assert.Equal("application/xml", result.Headers["content-type"]);
        Assert.Equal("example-bucket", result.Name);
        Assert.Equal(100, result.MaxKeys);
        Assert.Equal("/", result.Delimiter);
        Assert.Equal("url", result.EncodingType);
        Assert.Equal("start/123/", result.StartAfter);
        Assert.Equal("marker/123/", result.ContinuationToken);
        Assert.Equal("next-marker/123/", result.NextContinuationToken);
        Assert.NotNull(result.Contents);
        Assert.Single(result.Contents);
        Assert.Equal("key/123/1.txt", result.Contents[0].Key);
        Assert.Equal(new DateTime(2020, 6, 22, 11, 42, 32), result.Contents[0].LastModified);
        Assert.Equal("\"5B3C1A2E053D763E1B002CC607C5A0FE1****\"", result.Contents[0].ETag);
        Assert.Equal("Normal", result.Contents[0].Type);
        Assert.Equal(344606, result.Contents[0].Size);
        Assert.Equal("ColdArchive", result.Contents[0].StorageClass);
        Assert.NotNull(result.Contents[0].Owner);
        Assert.Equal("0022012****", result.Contents[0].Owner.Id);
        Assert.Equal("user-example", result.Contents[0].Owner.DisplayName);
        Assert.Equal("ongoing-request=\"true\"", result.Contents[0].RestoreInfo);
        Assert.Equal("aaa/+#?/12", result.Prefix);
        Assert.Equal(false, result.IsTruncated);
        Assert.NotNull(result.CommonPrefixes);
        Assert.Single(result.CommonPrefixes);
        Assert.Equal("a/b/", result.CommonPrefixes[0].Prefix);
        Assert.Equal(3, result.KeyCount);
    }
}
