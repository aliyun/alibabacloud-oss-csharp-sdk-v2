using System.Text;
using AlibabaCloud.OSS.V2.Models;
using AlibabaCloud.OSS.V2.Transform;

namespace AlibabaCloud.OSS.V2.UnitTests.Models;

public class ModelObjectTaggingTest {
    [Fact]
    public void TestPutObjectTaggingRequest() {
        var request = new PutObjectTaggingRequest();
        Assert.Empty(request.Headers);
        Assert.Empty(request.Parameters);
        Assert.Null(request.InnerBody);
        Assert.Equal("xml", request.BodyFormat);
        Assert.Null(request.Bucket);
        Assert.Null(request.Key);
        Assert.Null(request.VersionId);
        Assert.Null(request.Tagging);

        var input = new OperationInput();
        Serde.SerializeInput(request, ref input);

        Assert.Null(input.Headers);
        Assert.Null(input.Parameters);
        Assert.Null(input.Body);

        request = new PutObjectTaggingRequest {
            Bucket = "bucket",
            Key = "key",
            VersionId = "version-id",
            Tagging = new Tagging() {
                TagSet = new TagSet() {
                    Tags = [
                        new Tag() {
                            Key   = "key1",
                            Value = "value1"
                        },
                        new Tag() {
                            Key   = "key2",
                            Value = "value2"
                        }
                    ]
                }
            }
        };

        Assert.Equal("bucket", request.Bucket);
        Assert.Equal("key", request.Key);
        Assert.Empty(request.Headers);

        Assert.Single(request.Parameters);
        Assert.Equal("version-id", request.Parameters["versionId"]);
        Assert.Equal("version-id", request.VersionId);
        Assert.NotNull(request.Tagging);
        Assert.NotNull(request.Tagging.TagSet);
        Assert.Equal("key1", request.Tagging.TagSet.Tags[0].Key);
        Assert.Equal("value1", request.Tagging.TagSet.Tags[0].Value);
        Assert.Equal("key2", request.Tagging.TagSet.Tags[1].Key);
        Assert.Equal("value2", request.Tagging.TagSet.Tags[1].Value);

        Serde.SerializeInput(request, ref input);

        Assert.Null(input.Headers);
        Assert.NotNull(input.Parameters);
        Assert.Equal("version-id", input.Parameters["versionId"]);

        Assert.NotNull(input.Body);
        using var reader = new StreamReader(input.Body);
        var xml = """
<?xml version="1.0" encoding="utf-8"?>
<Tagging>
  <TagSet>
    <Tag>
      <Key>key1</Key>
      <Value>value1</Value>
    </Tag>
    <Tag>
      <Key>key2</Key>
      <Value>value2</Value>
    </Tag>
  </TagSet>
</Tagging>
""";
        Assert.Equal(xml, reader.ReadToEnd());
    }

    [Fact]
    public void TestPutObjectTaggingResult() {
        var result = new PutObjectTaggingResult();
        Assert.Equal(0, result.StatusCode);
        Assert.Equal("", result.Status);
        Assert.Equal("", result.RequestId);
        Assert.Empty(result.Headers);

        var output = new OperationOutput {
            StatusCode = 200,
            Status = "OK",
            Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
                {"x-oss-request-id", "123-id"},
                {"x-oss-version-id", "version-id-123"},
                {"Content-Type","txt"}
            }
        };
        ResultModel baseResult = result;
        Serde.DeserializeOutput(ref baseResult, ref output);

        Assert.Equal(200, result.StatusCode);
        Assert.Equal("OK", result.Status);
        Assert.Equal("123-id", result.RequestId);
        Assert.Equal(3, result.Headers.Count);
        Assert.Equal("txt", result.Headers["content-type"]);
        Assert.Equal("version-id-123", result.Headers["x-oss-version-id"]);
        Assert.Equal("version-id-123", result.VersionId);
    }

    [Fact]
    public void TestGetObjectTaggingRequest() {
        var request = new GetObjectTaggingRequest();
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

        request = new GetObjectTaggingRequest {
            Bucket = "bucket",
            Key = "key",
            VersionId = "version-id"
        };
        Assert.Empty(request.Headers);
        Assert.Equal("bucket", request.Bucket);
        Assert.Equal("key", request.Key);
        Assert.Single(request.Parameters);
        Assert.Equal("version-id", request.Parameters["versionId"]);
        Assert.Equal("version-id", request.VersionId);

        Serde.SerializeInput(request, ref input);

        Assert.Null(input.Headers);
        Assert.Null(input.Body);
        Assert.NotNull(input.Parameters);
        Assert.Equal("version-id", input.Parameters["versionId"]);
    }

    [Fact]
    public void TestGetObjectTaggingResult() {
        var result = new GetObjectTaggingResult();
        Assert.Equal(0, result.StatusCode);
        Assert.Equal("", result.Status);
        Assert.Equal("", result.RequestId);
        Assert.Empty(result.Headers);
        Assert.Equal("xml", result.BodyFormat);
        Assert.Null(result.Tagging);

        var xml = """
<?xml version="1.0" encoding="utf-8"?>
<Tagging>
  <TagSet>
    <Tag>
      <Key>a</Key>
      <Value>1</Value>
    </Tag>
    <Tag>
      <Key>b</Key>
      <Value>2</Value>
    </Tag>
  </TagSet>
</Tagging>
""";

        var output = new OperationOutput {
            StatusCode = 200,
            Status = "OK",
            Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
                {"x-oss-request-id", "123-id"},
                {"Content-Type","txt"}
            },
            Body = new MemoryStream(Encoding.UTF8.GetBytes(xml))
        };
        ResultModel baseResult = result;
        Serde.DeserializeOutput(ref baseResult, ref output, Serde.DeserializerXmlBody);

        Assert.Equal(200, result.StatusCode);
        Assert.Equal("OK", result.Status);
        Assert.Equal("123-id", result.RequestId);
        Assert.Equal(2, result.Headers.Count);
        Assert.Equal("txt", result.Headers["content-type"]);
        Assert.NotNull(result.Tagging);
        Assert.NotNull(result.Tagging.TagSet);
        Assert.NotNull(result.Tagging.TagSet.Tags);
        Assert.Equal(2, result.Tagging.TagSet.Tags.Count);
        Assert.Equal("a", result.Tagging.TagSet.Tags[0].Key);
        Assert.Equal("1", result.Tagging.TagSet.Tags[0].Value);
        Assert.Equal("b", result.Tagging.TagSet.Tags[1].Key);
        Assert.Equal("2", result.Tagging.TagSet.Tags[1].Value);
    }

    [Fact]
    public void TestDeleteObjectTaggingRequest() {
        var request = new DeleteObjectTaggingRequest();
        Assert.Empty(request.Headers);
        Assert.Empty(request.Parameters);
        Assert.Null(request.InnerBody);
        Assert.Equal("", request.BodyFormat);
        Assert.Null(request.Bucket);
        Assert.Null(request.Key);
        Assert.Null(request.VersionId);

        var input = new OperationInput();
        Serde.SerializeInput(request, ref input);

        Assert.Null(input.Headers);
        Assert.Null(input.Parameters);
        Assert.Null(input.Body);

        request = new DeleteObjectTaggingRequest {
            Bucket = "bucket",
            Key = "key",
            VersionId = "version-id"
        };

        Assert.Equal("bucket", request.Bucket);
        Assert.Equal("key", request.Key);
        Assert.Empty(request.Headers);

        Assert.Single(request.Parameters);
        Assert.Equal("version-id", request.Parameters["versionId"]);
        Assert.Equal("version-id", request.VersionId);

        Serde.SerializeInput(request, ref input);

        Assert.Null(input.Headers);
        Assert.Null(input.Body);
        Assert.NotNull(input.Parameters);
        Assert.Equal("version-id", input.Parameters["versionId"]);
    }

    [Fact]
    public void TestDeleteObjectTaggingResult() {
        var result = new DeleteObjectTaggingResult();
        Assert.Equal(0, result.StatusCode);
        Assert.Equal("", result.Status);
        Assert.Equal("", result.RequestId);
        Assert.Empty(result.Headers);

        var output = new OperationOutput {
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
}