using System.Text;
using AlibabaCloud.OSS.v2.Models;
using AlibabaCloud.OSS.v2.Transform;

namespace AlibabaCloud.OSS.v2.UnitTests.Models;

public class ModelObjectAclTest {
    [Fact]
    public void TestPutObjectAclRequest() {
        var request = new PutObjectAclRequest();
        Assert.Empty(request.Headers);
        Assert.Empty(request.Parameters);
        Assert.Null(request.InnerBody);
        Assert.Equal("", request.BodyFormat);
        Assert.Null(request.Bucket);
        Assert.Null(request.Key);
        Assert.Null(request.VersionId);
        Assert.Null(request.Acl);

        var input = new OperationInput();
        Serde.SerializeInput(request, ref input);

        Assert.Null(input.Headers);
        Assert.Null(input.Parameters);
        Assert.Null(input.Body);

        request = new PutObjectAclRequest {
            Bucket    = "bucket",
            Key = "key",
            Acl = "private",
            VersionId = "version-id"
        };

        Assert.Equal("bucket", request.Bucket);
        Assert.Equal("key", request.Key);
        Assert.Single(request.Headers);
        Assert.Equal("private", request.Headers["x-oss-object-acl"]);
        Assert.Equal("private", request.Acl);

        Assert.Single(request.Parameters);
        Assert.Equal("version-id", request.Parameters["versionId"]);
        Assert.Equal("version-id", request.VersionId);

        Serde.SerializeInput(request, ref input);

        Assert.NotNull(input.Headers);
        Assert.Equal("private", input.Headers["x-oss-object-acl"]);

        Assert.NotNull(input.Parameters);
        Assert.Equal("version-id", request.Parameters["versionId"]);
        Assert.Null(input.Body);
    }

    [Fact]
    public void TestPutObjectAclResult() {
        var result = new PutObjectAclResult();
        Assert.Equal(0, result.StatusCode);
        Assert.Equal("", result.Status);
        Assert.Equal("", result.RequestId);
        Assert.Empty(result.Headers);
        Assert.Null(result.VersionId);

        var output = new OperationOutput {
            StatusCode = 200,
            Status     = "OK",
            Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
                {"x-oss-request-id", "123-id"},
                {"Content-Type","txt"},
                {"x-oss-version-id","version-id-123"}
            }
        };
        ResultModel baseResult = result;
        Serde.DeserializeOutput(ref baseResult, ref output);

        Assert.Equal(200, result.StatusCode);
        Assert.Equal("OK", result.Status);
        Assert.Equal("123-id", result.RequestId);
        Assert.Equal(3, result.Headers.Count);
        Assert.Equal("txt", result.Headers["content-type"]);
        Assert.Equal("version-id-123", result.VersionId);
    }

    [Fact]
    public void TestGetObjectAclRequest() {
        var request = new GetObjectAclRequest();
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

        request = new GetObjectAclRequest {
            Bucket = "bucket",
            Key = "key",
            VersionId = "version-id"
        };
        Assert.Empty(request.Headers);
        Assert.Equal("bucket", request.Bucket);

        Assert.Equal("bucket", request.Bucket);
        Assert.Equal("key", request.Key);
        Assert.Equal("version-id", request.VersionId);

        Assert.Single(request.Parameters);
        Assert.Equal("version-id", request.Parameters["versionId"]);

        Serde.SerializeInput(request, ref input);
        Assert.Null(input.Headers);
        Assert.NotNull(input.Parameters);
        Assert.Equal("version-id", input.Parameters["versionId"]);
        Assert.Null(input.Body);
    }

    [Fact]
    public void TestGetObjectAclResult() {
        var result = new GetObjectAclResult();
        Assert.Equal(0, result.StatusCode);
        Assert.Equal("", result.Status);
        Assert.Equal("", result.RequestId);
        Assert.Empty(result.Headers);

        var xml = """
<?xml version="1.0" encoding="utf-8"?>
<AccessControlPolicy>
    <Owner>
        <ID>0022012****</ID>
        <DisplayName>user_example</DisplayName>
    </Owner>
    <AccessControlList>
        <Grant>public-read</Grant>
    </AccessControlList>
</AccessControlPolicy>
""";

        var output = new OperationOutput {
            StatusCode = 200,
            Status     = "OK",
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
        Assert.NotNull(result.AccessControlPolicy);
        Assert.NotNull(result.AccessControlPolicy.Owner);
        Assert.Equal("0022012****", result.AccessControlPolicy.Owner.Id);
        Assert.Equal("user_example", result.AccessControlPolicy.Owner.DisplayName);
        Assert.NotNull(result.AccessControlPolicy.AccessControlList);
        Assert.Equal("public-read", result.AccessControlPolicy.AccessControlList.Grant);
    }
}