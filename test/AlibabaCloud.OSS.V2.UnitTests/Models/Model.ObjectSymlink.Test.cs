using AlibabaCloud.OSS.V2.Models;
using AlibabaCloud.OSS.V2.Transform;

namespace AlibabaCloud.OSS.V2.UnitTests.Models;

public class ModelObjectSymlinkTest
{
    [Fact]
    public void TestPutSymlinkRequest()
    {
        var request = new PutSymlinkRequest();
        Assert.Empty(request.Headers);
        Assert.Empty(request.Parameters);
        Assert.Null(request.InnerBody);
        Assert.Equal("", request.BodyFormat);
        Assert.Null(request.Bucket);
        Assert.Null(request.Key);
        Assert.Null(request.SymlinkTarget);
        Assert.Null(request.ObjectAcl);
        Assert.Null(request.StorageClass);
        Assert.Null(request.ForbidOverwrite);

        var input = new OperationInput();
        Serde.SerializeInput(request, ref input);

        Assert.Null(input.Headers);
        Assert.Null(input.Parameters);
        Assert.Null(input.Body);

        request = new()
        {
            Bucket = "bucket",
            Key = "key",
            SymlinkTarget = "target-key",
            ObjectAcl = "private",
            StorageClass = "Standard",
            ForbidOverwrite = true
        };

        Assert.Equal("bucket", request.Bucket);
        Assert.Equal("key", request.Key);
        Assert.Equal(4, request.Headers.Count);
        Assert.Equal("private", request.Headers["x-oss-object-acl"]);
        Assert.Equal("private", request.ObjectAcl);
        Assert.Equal("Standard", request.Headers["x-oss-storage-class"]);
        Assert.Equal("Standard", request.StorageClass);
        Assert.Equal("target-key", request.Headers["x-oss-symlink-target"]);
        Assert.Equal("target-key", request.SymlinkTarget);
        Assert.Equal("true", request.Headers["x-oss-forbid-overwrite"]);
        Assert.Equal(true, request.ForbidOverwrite);

        Assert.Empty(request.Parameters);

        Serde.SerializeInput(request, ref input);

        Assert.NotNull(input.Headers);
        Assert.Equal("private", input.Headers["x-oss-object-acl"]);
        Assert.Equal("Standard", input.Headers["x-oss-storage-class"]);
        Assert.Equal("target-key", input.Headers["x-oss-symlink-target"]);
        Assert.Equal("true", input.Headers["x-oss-forbid-overwrite"]);

        Assert.Null(input.Parameters);
        Assert.Null(input.Body);
    }

    [Fact]
    public void TestPutSymlinkResult()
    {
        var result = new PutSymlinkResult();
        Assert.Equal(0, result.StatusCode);
        Assert.Equal("", result.Status);
        Assert.Equal("", result.RequestId);
        Assert.Empty(result.Headers);

        var output = new OperationOutput
        {
            StatusCode = 200,
            Status = "OK",
            Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
                { "x-oss-request-id", "123-id" },
                { "Content-Type", "txt" },
                { "x-oss-version-id", "version-id-123" },
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
    public void TestGetSymlinkRequest()
    {
        var request = new GetSymlinkRequest();
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

        request = new()
        {
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
        Assert.NotNull(input.Parameters);
        Assert.Equal("version-id", input.Parameters["versionId"]);
        Assert.Null(input.Body);
    }

    [Fact]
    public void TestGetSymlinkResult()
    {
        var result = new GetSymlinkResult();
        Assert.Equal(0, result.StatusCode);
        Assert.Equal("", result.Status);
        Assert.Equal("", result.RequestId);
        Assert.Empty(result.Headers);
        Assert.Equal("", result.BodyFormat);
        Assert.Null(result.VersionId);
        Assert.Null(result.SymlinkTarget);
        Assert.Null(result.ETag);
        Assert.NotNull(result.Metadata);
        Assert.Empty(result.Metadata);

        var output = new OperationOutput
        {
            StatusCode = 200,
            Status = "OK",
            Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
                { "x-oss-request-id", "123-id" },
                { "x-oss-symlink-target", "example.jpg" },
                { "Content-Type", "txt" },
                { "ETag", "etag-123" },
                { "x-oss-meta-m1", "meta-1" },
                { "x-oss-meta-M2", "meta-2" }
            }
        };
        ResultModel baseResult = result;
        Serde.DeserializeOutput(ref baseResult, ref output);

        Assert.Equal(200, result.StatusCode);
        Assert.Equal("OK", result.Status);
        Assert.Equal("123-id", result.RequestId);
        Assert.Equal(6, result.Headers.Count);
        Assert.Equal("txt", result.Headers["content-type"]);
        Assert.Equal("example.jpg", result.Headers["x-oss-symlink-target"]);
        Assert.Equal("example.jpg", result.SymlinkTarget);
        Assert.Equal("etag-123", result.ETag);
        Assert.NotNull(result.Metadata);
        var metadata = result.Metadata;
        Assert.Equal(2, metadata.Count);
        Assert.Equal("meta-1", metadata["m1"]);
        Assert.Equal("meta-2", metadata["m2"]);
    }
}
