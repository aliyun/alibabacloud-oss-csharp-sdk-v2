using System.Text;
using AlibabaCloud.OSS.v2.Models;
using AlibabaCloud.OSS.v2.Transform;

namespace AlibabaCloud.OSS.v2.UnitTests.Models;

public class ModelBucketAclTest {
    [Fact]
    public void TestPutBucketAclRequest() {
        var request = new PutBucketAclRequest();
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

        request = new PutBucketAclRequest {
            Bucket = "bucket",
            Acl = "private"
        };
        Serde.SerializeInput(request, ref input);

        Assert.Single(request.Headers);
        Assert.Equal("bucket", request.Bucket);
        Assert.Equal("private", request.Headers["x-oss-acl"]);
        Assert.Equal("private", request.Acl);

        Assert.NotNull(input.Headers);
        Assert.Single(input.Headers);
        Assert.Equal("private", request.Headers["x-oss-acl"]);
        Assert.Null(input.Parameters);
        Assert.Null(input.Body);
    }

    [Fact]
    public void TestPutBucketAclResult() {
        var result = new PutBucketAclResult();
        Assert.Equal(0, result.StatusCode);
        Assert.Equal("", result.Status);
        Assert.Equal("", result.RequestId);
        Assert.Empty(result.Headers);

        var output = new OperationOutput {
            StatusCode = 200,
            Status     = "OK",
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
    public void TestGetBucketAclRequest() {
        var request = new GetBucketAclRequest();
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

        request = new GetBucketAclRequest {
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
    public void TestGetBucketAclResult() {
        var result = new GetBucketAclResult();
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
        Serde.DeserializeOutput(ref baseResult, ref output, Serde.DeserializerAnyBody);

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