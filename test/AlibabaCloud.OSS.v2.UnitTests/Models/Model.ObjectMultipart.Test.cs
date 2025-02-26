using AlibabaCloud.OSS.v2.Models;
using AlibabaCloud.OSS.v2.Transform;
using System.Text;

namespace AlibabaCloud.OSS.v2.UnitTests.Models;

public class ModelObjectMultipartTest {
    [Fact]
    public void TestInitiateMultipartUploadRequest() {
        var request = new InitiateMultipartUploadRequest();
        Assert.Empty(request.Headers);
        Assert.Empty(request.Parameters);
        Assert.Null(request.InnerBody);
        Assert.Equal("", request.BodyFormat);
        Assert.Null(request.Bucket);
        Assert.Null(request.Key);
        Assert.Null(request.ForbidOverwrite);
        Assert.Null(request.StorageClass);
        Assert.Null(request.Tagging);
        Assert.Null(request.ServerSideEncryption);
        Assert.Null(request.ServerSideDataEncryption);
        Assert.Null(request.ServerSideEncryptionKeyId);
        Assert.Null(request.CacheControl);
        Assert.Null(request.ContentDisposition);
        Assert.Null(request.ContentEncoding);
        Assert.Null(request.ContentLength);
        Assert.Null(request.ContentMd5);
        Assert.Null(request.ContentType);
        Assert.Null(request.Metadata);
        Assert.Null(request.Expires);
        Assert.Null(request.EncodingType);

        var input = new OperationInput();
        Serde.SerializeInput(request, ref input, Serde.AddMetadata);

        Assert.Null(input.Headers);
        Assert.Null(input.Parameters);
        Assert.Null(input.Body);

        request = new InitiateMultipartUploadRequest {
            Bucket          = "bucket",
            Key             = "key",
            ForbidOverwrite = true,
            StorageClass    = "IA",
            Tagging = "key1=value1&key2=value2",
            ServerSideEncryption = "AES256",
            ServerSideEncryptionKeyId = "kms-key-id",
            ServerSideDataEncryption = "sse-data",
            CacheControl = "no-cache",
            ContentDisposition = "disposition",
            ContentEncoding = "gzip",
            ContentLength = 12345,
            ContentMd5 = "md5-123",
            ContentType = "txt",
            Metadata = new Dictionary<string, string>() {
                {"key1", "value1"},
                {"key2", "value2"}
            },
            Expires = "Thu, 01 Dec 1994 16:00:00 GMT",
            EncodingType = "url"
        };

        Assert.Equal("bucket", request.Bucket);
        Assert.Equal("key", request.Key);
        Assert.NotNull(request.Headers);
        Assert.Equal("true", request.Headers["x-oss-forbid-overwrite"]);
        Assert.Equal(true, request.ForbidOverwrite);
        Assert.Equal("IA", request.Headers["x-oss-storage-class"]);
        Assert.Equal("IA", request.StorageClass);
        Assert.Equal("key1=value1&key2=value2", request.Headers["x-oss-tagging"]);
        Assert.Equal("key1=value1&key2=value2", request.Tagging);
        Assert.Equal("AES256", request.Headers["x-oss-server-side-encryption"]);
        Assert.Equal("AES256", request.ServerSideEncryption);
        Assert.Equal("kms-key-id", request.Headers["x-oss-server-side-encryption-key-id"]);
        Assert.Equal("kms-key-id", request.ServerSideEncryptionKeyId);
        Assert.Equal("sse-data", request.Headers["x-oss-server-side-data-encryption"]);
        Assert.Equal("sse-data", request.ServerSideDataEncryption);
        Assert.Equal("no-cache", request.Headers["Cache-Control"]);
        Assert.Equal("no-cache", request.CacheControl);
        Assert.Equal("disposition", request.Headers["Content-Disposition"]);
        Assert.Equal("disposition", request.ContentDisposition);
        Assert.Equal("gzip", request.Headers["Content-Encoding"]);
        Assert.Equal("gzip", request.ContentEncoding);
        Assert.Equal("12345", request.Headers["Content-Length"]);
        Assert.Equal(12345, request.ContentLength);
        Assert.Equal("md5-123", request.Headers["Content-MD5"]);
        Assert.Equal("md5-123", request.ContentMd5);
        Assert.Equal("txt", request.Headers["Content-Type"]);
        Assert.Equal("txt", request.ContentType);
        Assert.Equal("Thu, 01 Dec 1994 16:00:00 GMT", request.Headers["Expires"]);
        Assert.Equal("Thu, 01 Dec 1994 16:00:00 GMT", request.Expires);

        Assert.NotNull(request.Metadata);
        Assert.Equal("value1", request.Metadata["key1"]);
        Assert.Equal("value2", request.Metadata["key2"]);

        Assert.Equal("url", request.Parameters["encoding-type"]);
        Assert.Equal("url", request.EncodingType);

        Serde.SerializeInput(request, ref input, Serde.AddMetadata);

        Assert.NotNull(input.Headers);
        Assert.Equal("true", input.Headers["x-oss-forbid-overwrite"]);
        Assert.Equal("IA", input.Headers["x-oss-storage-class"]);
        Assert.Equal("key1=value1&key2=value2", input.Headers["x-oss-tagging"]);
        Assert.Equal("AES256", input.Headers["x-oss-server-side-encryption"]);
        Assert.Equal("kms-key-id", input.Headers["x-oss-server-side-encryption-key-id"]);
        Assert.Equal("sse-data", input.Headers["x-oss-server-side-data-encryption"]);
        Assert.Equal("no-cache", input.Headers["Cache-Control"]);
        Assert.Equal("disposition", input.Headers["Content-Disposition"]);
        Assert.Equal("gzip", input.Headers["Content-Encoding"]);
        Assert.Equal("12345", input.Headers["Content-Length"]);
        Assert.Equal("md5-123", input.Headers["Content-MD5"]);
        Assert.Equal("txt", input.Headers["Content-Type"]);
        Assert.Equal("Thu, 01 Dec 1994 16:00:00 GMT", input.Headers["Expires"]);
        Assert.Equal("value1", input.Headers["x-oss-meta-key1"]);
        Assert.Equal("value2", input.Headers["x-oss-meta-key2"]);

        Assert.NotNull(input.Parameters);
        Assert.Equal("url", input.Parameters["encoding-type"]);

        Assert.Null(input.Body);
    }

    [Fact]
    public void TestInitiateMultipartUploadResult() {
        var result = new InitiateMultipartUploadResult();
        Assert.Equal(0, result.StatusCode);
        Assert.Equal("", result.Status);
        Assert.Equal("", result.RequestId);
        Assert.Empty(result.Headers);
        Assert.Null(result.Bucket);
        Assert.Null(result.Key);
        Assert.Null(result.UploadId);
        Assert.Null(result.EncodingType);

        var xml = """
<?xml version="1.0" encoding="utf-8"?>
<InitiateMultipartUploadResult>
</InitiateMultipartUploadResult>
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
        result                = new InitiateMultipartUploadResult();
        ResultModel baseResult = result;
        Serde.DeserializeOutput(ref baseResult, ref output, Serde.DeserializeInitiateMultipartUpload);

        Assert.Equal(200, result.StatusCode);
        Assert.Equal("OK", result.Status);
        Assert.Equal("123-id", result.RequestId);
        Assert.Equal(2, result.Headers.Count);
        Assert.Equal("txt", result.Headers["content-type"]);
        Assert.Null(result.Bucket);
        Assert.Null(result.Key);
        Assert.Null(result.UploadId);
        Assert.Null(result.EncodingType);

        xml = """
<?xml version="1.0" encoding="utf-8"?>
<InitiateMultipartUploadResult>
    <Bucket>oss-example</Bucket>
    <Key>multipart.data</Key>
    <UploadId>0004B9894A22E5B1888A1E29F823****</UploadId>
</InitiateMultipartUploadResult>
""";

        output = new OperationOutput {
            StatusCode = 200,
            Status     = "OK",
            Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
                {"x-oss-request-id", "123-id"},
                {"Content-Type","txt"}
            },
            Body = new MemoryStream(Encoding.UTF8.GetBytes(xml))
        };
        result     = new InitiateMultipartUploadResult();
        baseResult = result;
        Serde.DeserializeOutput(ref baseResult, ref output, Serde.DeserializeInitiateMultipartUpload);

        Assert.Equal(200, result.StatusCode);
        Assert.Equal("OK", result.Status);
        Assert.Equal("123-id", result.RequestId);
        Assert.Equal(2, result.Headers.Count);
        Assert.Equal("txt", result.Headers["content-type"]);
        Assert.Equal("oss-example", result.Bucket);
        Assert.Equal("multipart.data", result.Key);
        Assert.Equal("0004B9894A22E5B1888A1E29F823****", result.UploadId);
        Assert.Null(result.EncodingType);

        xml = """
<?xml version="1.0" encoding="utf-8"?>
<InitiateMultipartUploadResult>
  <Bucket>oss-example</Bucket>
  <Key>folder%2Fmultipart.data</Key>
  <UploadId>0004B9894A22E5B1888A1E29F823****</UploadId>
  <EncodingType>url</EncodingType>
</InitiateMultipartUploadResult>
""";

        output = new OperationOutput {
            StatusCode = 200,
            Status     = "OK",
            Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
                {"x-oss-request-id", "123-id"},
                {"Content-Type","txt"}
            },
            Body = new MemoryStream(Encoding.UTF8.GetBytes(xml))
        };
        result     = new InitiateMultipartUploadResult();
        baseResult = result;
        Serde.DeserializeOutput(ref baseResult, ref output, Serde.DeserializeInitiateMultipartUpload);

        Assert.Equal(200, result.StatusCode);
        Assert.Equal("OK", result.Status);
        Assert.Equal("123-id", result.RequestId);
        Assert.Equal(2, result.Headers.Count);
        Assert.Equal("txt", result.Headers["content-type"]);
        Assert.Equal("oss-example", result.Bucket);
        Assert.Equal("folder/multipart.data", result.Key);
        Assert.Equal("0004B9894A22E5B1888A1E29F823****", result.UploadId);
        Assert.Equal("url", result.EncodingType);
    }

    [Fact]
    public void TestUploadPartRequest() {
        var request = new UploadPartRequest();
        Assert.Empty(request.Headers);
        Assert.Empty(request.Parameters);
        Assert.Null(request.InnerBody);
        Assert.Equal("", request.BodyFormat);
        Assert.Null(request.Bucket);
        Assert.Null(request.Key);
        Assert.Null(request.PartNumber);
        Assert.Null(request.UploadId);
        Assert.Null(request.ContentLength);
        Assert.Null(request.ContentMd5);
        Assert.Null(request.TrafficLimit);
        Assert.Null(request.Body);

        var input = new OperationInput();
        Serde.SerializeInput(request, ref input);

        Assert.Null(input.Headers);
        Assert.Null(input.Parameters);
        Assert.Null(input.Body);

        var body = new MemoryStream(Encoding.UTF8.GetBytes("hello world"));
        request = new UploadPartRequest {
            Bucket = "bucket",
            Key = "key",
            PartNumber= 1,
            UploadId = "upload-id",
            ContentLength = 12345,
            ContentMd5 = "md5-123",
            TrafficLimit = 12888,
            Body = body
        };

        Assert.Equal("bucket", request.Bucket);
        Assert.Equal("key", request.Key);
        Assert.NotEmpty(request.Headers);
        Assert.Equal("1", request.Parameters["partNumber"]);
        Assert.Equal(1, request.PartNumber);
        Assert.Equal("upload-id", request.Parameters["uploadId"]);
        Assert.Equal("upload-id", request.UploadId);
        Assert.Equal("12345", request.Headers["Content-Length"]);
        Assert.Equal(12345, request.ContentLength);
        Assert.Equal("md5-123", request.Headers["Content-MD5"]);
        Assert.Equal("md5-123", request.ContentMd5);
        Assert.Equal("12888", request.Headers["x-oss-traffic-limit"]);
        Assert.Equal(12888, request.TrafficLimit);
        Assert.NotNull(request.Body);

        Serde.SerializeInput(request, ref input);

        Assert.NotNull(input.Headers);
        Assert.Equal("12345", input.Headers["Content-Length"]);
        Assert.Equal("md5-123", input.Headers["Content-MD5"]);
        Assert.Equal("12888", input.Headers["x-oss-traffic-limit"]);

        Assert.NotNull(input.Parameters);
        Assert.Equal("1", input.Parameters["partNumber"]);
        Assert.Equal("upload-id", input.Parameters["uploadId"]);

        Assert.Equal(body, input.Body);
    }

    [Fact]
    public void TestUploadPartResult() {
        var result = new UploadPartResult();
        Assert.Equal(0, result.StatusCode);
        Assert.Equal("", result.Status);
        Assert.Equal("", result.RequestId);
        Assert.Empty(result.Headers);
        Assert.Null(result.ContentMd5);
        Assert.Null(result.ETag);
        Assert.Null(result.HashCrc64);

        var output = new OperationOutput {
            StatusCode = 200,
            Status = "OK",
            Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
                {"x-oss-request-id", "123-id"},
                {"Content-Type","txt"},
                {"Content-MD5","MD5-123"},
                {"ETag","etag-123"},
                {"x-oss-hash-crc64ecma","123456"}
            }
        };
        result = new UploadPartResult();
        ResultModel baseResult = result;
        Serde.DeserializeOutput(ref baseResult, ref output);

        Assert.Equal(200, result.StatusCode);
        Assert.Equal("OK", result.Status);
        Assert.Equal("123-id", result.RequestId);
        Assert.Equal(5, result.Headers.Count);
        Assert.Equal("txt", result.Headers["content-type"]);
        Assert.Equal("MD5-123", result.ContentMd5);
        Assert.Equal("etag-123", result.ETag);
        Assert.Equal("123456", result.HashCrc64);
    }

    [Fact]
    public void TestUploadPartCopyRequest() {
        var request = new UploadPartCopyRequest();
        Assert.Empty(request.Headers);
        Assert.Empty(request.Parameters);
        Assert.Null(request.InnerBody);
        Assert.Equal("", request.BodyFormat);
        Assert.Null(request.Bucket);
        Assert.Null(request.Key);
        Assert.Null(request.PartNumber);
        Assert.Null(request.UploadId);
        Assert.Null(request.SourceBucket);
        Assert.Null(request.SourceKey);
        Assert.Null(request.SourceVersionId);
        Assert.Null(request.SourceRange);
        Assert.Null(request.IfMatch);
        Assert.Null(request.IfNoneMatch);
        Assert.Null(request.IfUnmodifiedSince);
        Assert.Null(request.IfModifiedSince);
        Assert.Null(request.TrafficLimit);

        var input = new OperationInput();
        Serde.SerializeInput(request, ref input, Serde.AddCopySource);

        Assert.Null(input.Headers);
        Assert.Null(input.Parameters);
        Assert.Null(input.Body);

        request = new UploadPartCopyRequest {
            Bucket = "bucket",
            Key = "key",
            PartNumber = 1,
            UploadId = "upload-id",
            SourceBucket = "src-bucket",
            SourceKey = "src-key+?123.txt",
            SourceVersionId = "src-version-id",
            SourceRange = "bytes 0~9/44",
            IfMatch = "if-match",
            IfNoneMatch = "if-none-match",
            IfUnmodifiedSince = "if_unmodified_since",
            IfModifiedSince = "if_modified_since",
            TrafficLimit = 12888,
        };

        Assert.Equal("bucket", request.Bucket);
        Assert.Equal("key", request.Key);
        Assert.NotEmpty(request.Headers);
        Assert.Equal("1", request.Parameters["partNumber"]);
        Assert.Equal(1, request.PartNumber);
        Assert.Equal("upload-id", request.Parameters["uploadId"]);
        Assert.Equal("upload-id", request.UploadId);

        Assert.Equal("src-bucket", request.SourceBucket);
        Assert.Equal("src-key+?123.txt", request.SourceKey);
        Assert.Equal("src-version-id", request.SourceVersionId);
        Assert.Equal("bytes 0~9/44", request.Headers["x-oss-copy-source-range"]);
        Assert.Equal("bytes 0~9/44", request.SourceRange);
        Assert.Equal("if-match", request.Headers["x-oss-copy-source-if-match"]);
        Assert.Equal("if-match", request.IfMatch);
        Assert.Equal("if-none-match", request.Headers["x-oss-copy-source-if-none-match"]);
        Assert.Equal("if-none-match", request.IfNoneMatch);
        Assert.Equal("if_unmodified_since", request.Headers["x-oss-copy-source-if-unmodified-since"]);
        Assert.Equal("if_unmodified_since", request.IfUnmodifiedSince);
        Assert.Equal("if_modified_since", request.Headers["x-oss-copy-source-if-modified-since"]);
        Assert.Equal("if_modified_since", request.IfModifiedSince);

        Assert.Equal("12888", request.Headers["x-oss-traffic-limit"]);
        Assert.Equal(12888, request.TrafficLimit);

        Serde.SerializeInput(request, ref input, Serde.AddCopySource);

        Assert.NotNull(input.Headers);
        Assert.Equal("/src-bucket/src-key%2B%3F123.txt?versionId=src-version-id", input.Headers["x-oss-copy-source"]);
        Assert.Equal("bytes 0~9/44", input.Headers["x-oss-copy-source-range"]);
        Assert.Equal("if-match", input.Headers["x-oss-copy-source-if-match"]);
        Assert.Equal("if-none-match", input.Headers["x-oss-copy-source-if-none-match"]);
        Assert.Equal("if_unmodified_since", input.Headers["x-oss-copy-source-if-unmodified-since"]);
        Assert.Equal("if_modified_since", input.Headers["x-oss-copy-source-if-modified-since"]);
        Assert.Equal("12888", input.Headers["x-oss-traffic-limit"]);

        Assert.NotNull(input.Parameters);
        Assert.Equal("1", input.Parameters["partNumber"]);
        Assert.Equal("upload-id", input.Parameters["uploadId"]);

        Assert.Null(input.Body);
    }

    [Fact]
    public void TestUploadPartCopyResult() {
        var result = new UploadPartCopyResult();
        Assert.Equal(0, result.StatusCode);
        Assert.Equal("", result.Status);
        Assert.Equal("", result.RequestId);
        Assert.Empty(result.Headers);
        Assert.Null(result.SourceVersionId);
        Assert.Null(result.LastModified);
        Assert.Null(result.ETag);

        var xml = """
<?xml version="1.0" encoding="utf-8"?>
<CopyPartResult>
</CopyPartResult>
""";

        var output = new OperationOutput {
            StatusCode = 200,
            Status = "OK",
            Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
                {"x-oss-request-id", "123-id"},
                {"Content-Type","txt"},
                {"x-oss-copy-source-version-id","CAEQMxiBgMC0vs6D"}
            },
            Body = new MemoryStream(Encoding.UTF8.GetBytes(xml))
        };
        result = new UploadPartCopyResult();
        ResultModel baseResult = result;
        Serde.DeserializeOutput(ref baseResult, ref output, Serde.DeserializeUploadPartCopy);

        Assert.Equal(200, result.StatusCode);
        Assert.Equal("OK", result.Status);
        Assert.Equal("123-id", result.RequestId);
        Assert.Equal("txt", result.Headers["content-type"]);
        Assert.Equal("CAEQMxiBgMC0vs6D", result.SourceVersionId);
        Assert.Null(result.LastModified);
        Assert.Null(result.ETag);

        xml = """
<?xml version="1.0" encoding="utf-8"?>
<CopyPartResult>
<LastModified>2019-04-09T07:01:56.000Z</LastModified>
<ETag>"25A9F4ABFCC05743DF6E2C886C56****"</ETag>
</CopyPartResult>
""";

        output = new OperationOutput {
            StatusCode = 200,
            Status     = "OK",
            Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
                {"x-oss-request-id", "123-id"},
                {"Content-Type","txt"},
                {"x-oss-copy-source-version-id","CAEQMxiBgMC0vs6D"}
            },
            Body = new MemoryStream(Encoding.UTF8.GetBytes(xml))
        };
        result = new UploadPartCopyResult();
        baseResult = result;
        Serde.DeserializeOutput(ref baseResult, ref output, Serde.DeserializeUploadPartCopy);

        Assert.Equal(200, result.StatusCode);
        Assert.Equal("OK", result.Status);
        Assert.Equal("123-id", result.RequestId);
        Assert.Equal("txt", result.Headers["content-type"]);
        Assert.Equal("CAEQMxiBgMC0vs6D", result.SourceVersionId);
        Assert.Equal("2019/4/9 7:01:56", result.LastModified.ToString());
        Assert.Equal("\"25A9F4ABFCC05743DF6E2C886C56****\"", result.ETag);
    }

    [Fact]
    public void TestCompleteMultipartUploadRequest() {
        var request = new CompleteMultipartUploadRequest();
        Assert.Empty(request.Headers);
        Assert.Empty(request.Parameters);
        Assert.Null(request.InnerBody);
        Assert.Equal("xml", request.BodyFormat);
        Assert.Null(request.Bucket);
        Assert.Null(request.Key);
        Assert.Null(request.UploadId);
        Assert.Null(request.Acl);
        Assert.Null(request.CompleteMultipartUpload);
        Assert.Null(request.CompleteAll);
        Assert.Null(request.Callback);
        Assert.Null(request.CallbackVar);
        Assert.Null(request.ForbidOverwrite);
        Assert.Null(request.EncodingType);

        var input = new OperationInput();
        Serde.SerializeInput(request, ref input);

        Assert.Null(input.Headers);
        Assert.Null(input.Parameters);
        Assert.Null(input.Body);

        request = new CompleteMultipartUploadRequest {
            Bucket = "bucket",
            Key = "key",
            UploadId = "upload-id",
            Acl = "private",
            CompleteMultipartUpload = new CompleteMultipartUpload() {
                Parts = [
                    new UploadPart(){PartNumber = 1, ETag = "etag-1"},
                    new UploadPart(){PartNumber = 2, ETag = "etag-2"},
                ]
            },
            CompleteAll = "yes",
            Callback = "callback",
            CallbackVar = "callback-var",
            ForbidOverwrite = true,
            EncodingType = "url"
        };

        Assert.Equal("bucket", request.Bucket);
        Assert.Equal("key", request.Key);
        Assert.NotEmpty(request.Headers);
        Assert.Equal("upload-id", request.Parameters["uploadId"]);
        Assert.Equal("upload-id", request.UploadId);
        Assert.Equal("url", request.Parameters["encoding-type"]);
        Assert.Equal("url", request.EncodingType);

        Assert.Equal("private", request.Headers["x-oss-object-acl"]);
        Assert.Equal("private", request.Acl);
        Assert.Equal("yes", request.Headers["x-oss-complete-all"]);
        Assert.Equal("yes", request.CompleteAll);
        Assert.Equal("callback", request.Headers["x-oss-callback"]);
        Assert.Equal("callback", request.Callback);
        Assert.Equal("callback-var", request.Headers["x-oss-callback-var"]);
        Assert.Equal("callback-var", request.CallbackVar);
        Assert.Equal("true", request.Headers["x-oss-forbid-overwrite"]);
        Assert.Equal(true, request.ForbidOverwrite);


        Serde.SerializeInput(request, ref input);

        Assert.NotNull(input.Headers);
        Assert.Equal("private", request.Headers["x-oss-object-acl"]);
        Assert.Equal("yes", request.Headers["x-oss-complete-all"]);
        Assert.Equal("callback", request.Headers["x-oss-callback"]);
        Assert.Equal("callback-var", request.Headers["x-oss-callback-var"]);
        Assert.Equal("true", request.Headers["x-oss-forbid-overwrite"]);

        Assert.NotNull(input.Parameters);
        Assert.Equal("upload-id", request.Parameters["uploadId"]);
        Assert.Equal("url", input.Parameters["encoding-type"]);

        Assert.NotNull(input.Body);
        using var reader = new StreamReader(input.Body);
        var xml = """
<?xml version="1.0" encoding="utf-8"?>
<CompleteMultipartUpload>
  <Part>
    <ETag>etag-1</ETag>
    <PartNumber>1</PartNumber>
  </Part>
  <Part>
    <ETag>etag-2</ETag>
    <PartNumber>2</PartNumber>
  </Part>
</CompleteMultipartUpload>
""";
        Assert.Equal(xml, reader.ReadToEnd());
    }

    [Fact]
    public void TestCompleteMultipartUploadResult() {
        var result = new CompleteMultipartUploadResult();
        Assert.Equal(0, result.StatusCode);
        Assert.Equal("", result.Status);
        Assert.Equal("", result.RequestId);
        Assert.Empty(result.Headers);
        Assert.Null(result.VersionId);
        Assert.Null(result.HashCrc64);
        Assert.Null(result.EncodingType);
        Assert.Null(result.Bucket);
        Assert.Null(result.Key);
        Assert.Null(result.ETag);
        Assert.Null(result.CallbackResult);

        var xml = """
<?xml version="1.0" encoding="utf-8"?>
<CompleteMultipartUploadResult>
</CompleteMultipartUploadResult>
""";

        var output = new OperationOutput {
            StatusCode = 200,
            Status = "OK",
            Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
                {"x-oss-request-id", "123-id"},
                {"Content-Type","txt"},
                {"x-oss-version-id","CAEQMxiBgMC0vs6D"},
                {"x-oss-hash-crc64ecma","12345"}
            },
            Body = new MemoryStream(Encoding.UTF8.GetBytes(xml))
        };
        result = new CompleteMultipartUploadResult();
        ResultModel baseResult = result;
        Serde.DeserializeOutput(ref baseResult, ref output, Serde.DeserializeCompleteMultipartUpload);

        Assert.Equal(200, result.StatusCode);
        Assert.Equal("OK", result.Status);
        Assert.Equal("123-id", result.RequestId);
        Assert.Equal("txt", result.Headers["content-type"]);
        Assert.Equal("CAEQMxiBgMC0vs6D", result.VersionId);
        Assert.Equal("12345", result.HashCrc64);
        Assert.Null(result.EncodingType);
        Assert.Null(result.Bucket);
        Assert.Null(result.Key);
        Assert.Null(result.ETag);
        Assert.Null(result.CallbackResult);

        // with url
        xml = """
<?xml version="1.0" encoding="utf-8"?>
<CompleteMultipartUploadResult>
    <EncodingType>url</EncodingType>
    <Bucket>oss-example</Bucket>
    <Key>folder%2Fmultipart.data</Key>
    <ETag>"B864DB6A936D376F9F8D3ED3BBE540****"</ETag>
</CompleteMultipartUploadResult>
""";

        output = new OperationOutput {
            StatusCode = 200,
            Status     = "OK",
            Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
                {"x-oss-request-id", "123-id"},
                {"Content-Type","txt"},
                {"x-oss-version-id","CAEQMxiBgMC0vs6D"},
                {"x-oss-hash-crc64ecma","12345"}
            },
            Body = new MemoryStream(Encoding.UTF8.GetBytes(xml))
        };
        result = new CompleteMultipartUploadResult();
        baseResult = result;
        Serde.DeserializeOutput(ref baseResult, ref output, Serde.DeserializeCompleteMultipartUpload);

        Assert.Equal(200, result.StatusCode);
        Assert.Equal("OK", result.Status);
        Assert.Equal("123-id", result.RequestId);
        Assert.Equal("txt", result.Headers["content-type"]);
        Assert.Equal("CAEQMxiBgMC0vs6D", result.VersionId);
        Assert.Equal("12345", result.HashCrc64);
        Assert.Equal("url", result.EncodingType);
        Assert.Equal("oss-example", result.Bucket);
        Assert.Equal("folder/multipart.data", result.Key);
        Assert.Equal("\"B864DB6A936D376F9F8D3ED3BBE540****\"", result.ETag);
        Assert.Null(result.CallbackResult);

        // without url
        xml = """
              <?xml version="1.0" encoding="utf-8"?>
              <CompleteMultipartUploadResult>
                  <Bucket>oss-example</Bucket>
                  <Key>folder%2Fmultipart.data</Key>
                  <ETag>"B864DB6A936D376F9F8D3ED3BBE540****"</ETag>
              </CompleteMultipartUploadResult>
              """;

        output = new OperationOutput {
            StatusCode = 200,
            Status     = "OK",
            Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
                {"x-oss-request-id", "123-id"},
                {"Content-Type","txt"},
                {"x-oss-version-id","CAEQMxiBgMC0vs6D"},
                {"x-oss-hash-crc64ecma","12345"}
            },
            Body = new MemoryStream(Encoding.UTF8.GetBytes(xml))
        };
        result     = new CompleteMultipartUploadResult();
        baseResult = result;
        Serde.DeserializeOutput(ref baseResult, ref output, Serde.DeserializeCompleteMultipartUpload);

        Assert.Equal(200, result.StatusCode);
        Assert.Equal("OK", result.Status);
        Assert.Equal("123-id", result.RequestId);
        Assert.Equal("txt", result.Headers["content-type"]);
        Assert.Equal("CAEQMxiBgMC0vs6D", result.VersionId);
        Assert.Equal("12345", result.HashCrc64);
        Assert.Null(result.EncodingType);
        Assert.Equal("oss-example", result.Bucket);
        Assert.Equal("folder%2Fmultipart.data", result.Key);
        Assert.Equal("\"B864DB6A936D376F9F8D3ED3BBE540****\"", result.ETag);
        Assert.Null(result.CallbackResult);

        // json format
        xml = "json value";

        output = new OperationOutput {
            StatusCode = 200,
            Status     = "OK",
            Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
                {"x-oss-request-id", "123-id"},
                {"Content-Type","txt"},
                {"x-oss-version-id","CAEQMxiBgMC0vs6D"},
                {"x-oss-hash-crc64ecma","12345"}
            },
            Body = new MemoryStream(Encoding.UTF8.GetBytes(xml))
        };
        result     = new CompleteMultipartUploadResult();
        baseResult = result;
        Serde.DeserializeOutput(ref baseResult, ref output, Serde.DeserializeCompleteMultipartUploadCallback);

        Assert.Equal(200, result.StatusCode);
        Assert.Equal("OK", result.Status);
        Assert.Equal("123-id", result.RequestId);
        Assert.Equal("txt", result.Headers["content-type"]);
        Assert.Equal("CAEQMxiBgMC0vs6D", result.VersionId);
        Assert.Equal("12345", result.HashCrc64);
        Assert.Null(result.EncodingType);
        Assert.Null(result.Bucket);
        Assert.Null(result.Key);
        Assert.Null(result.ETag);
        Assert.Equal("json value", result.CallbackResult);
    }


    [Fact]
    public void TestAbortMultipartUploadRequest() {
        var request = new AbortMultipartUploadRequest();
        Assert.Empty(request.Headers);
        Assert.Empty(request.Parameters);
        Assert.Null(request.InnerBody);
        Assert.Equal("", request.BodyFormat);
        Assert.Null(request.Bucket);
        Assert.Null(request.Key);
        Assert.Null(request.UploadId);

        var input = new OperationInput();
        Serde.SerializeInput(request, ref input);

        Assert.Null(input.Headers);
        Assert.Null(input.Parameters);
        Assert.Null(input.Body);

        request = new AbortMultipartUploadRequest {
            Bucket = "bucket",
            Key = "key",
            UploadId = "upload-id",
        };

        Assert.Equal("bucket", request.Bucket);
        Assert.Equal("key", request.Key);
        Assert.Equal("upload-id", request.Parameters["uploadId"]);
        Assert.Equal("upload-id", request.UploadId);
        Assert.Empty(request.Headers);

        Serde.SerializeInput(request, ref input);

        Assert.Null(input.Headers);
        Assert.NotNull(input.Parameters);
        Assert.Equal("upload-id", request.Parameters["uploadId"]);
        Assert.Null(input.Body);
    }

    [Fact]
    public void TestAbortMultipartUploadResult() {
        var result = new AbortMultipartUploadResult();
        Assert.Equal(0, result.StatusCode);
        Assert.Equal("", result.Status);
        Assert.Equal("", result.RequestId);
        Assert.Empty(result.Headers);

        var output = new OperationOutput {
            StatusCode = 200,
            Status = "OK",
            Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
                {"x-oss-request-id", "123-id"},
                {"Content-Type","txt"},
            },
        };
        result = new AbortMultipartUploadResult();
        ResultModel baseResult = result;
        Serde.DeserializeOutput(ref baseResult, ref output);

        Assert.Equal(200, result.StatusCode);
        Assert.Equal("OK", result.Status);
        Assert.Equal("123-id", result.RequestId);
        Assert.Equal("txt", result.Headers["content-type"]);
    }

    [Fact]
    public void TestListMultipartUploadsRequest() {
        var request = new ListMultipartUploadsRequest();
        Assert.Empty(request.Headers);
        Assert.Empty(request.Parameters);
        Assert.Null(request.InnerBody);
        Assert.Equal("", request.BodyFormat);
        Assert.Null(request.Bucket);
        Assert.Null(request.Delimiter);
        Assert.Null(request.EncodingType);
        Assert.Null(request.KeyMarker);
        Assert.Null(request.MaxUploads);
        Assert.Null(request.Prefix);
        Assert.Null(request.UploadIdMarker);

        var input = new OperationInput();
        Serde.SerializeInput(request, ref input);

        Assert.Null(input.Headers);
        Assert.Null(input.Parameters);
        Assert.Null(input.Body);

        request = new ListMultipartUploadsRequest {
            Bucket       = "bucket",
            Delimiter    = "/",
            EncodingType = "url",
            KeyMarker    = "key-01",
            MaxUploads   = 10001,
            Prefix       = "prefix-01",
            UploadIdMarker = "upload-id-123"
        };
        Assert.Empty(request.Headers);
        Assert.Equal("bucket", request.Bucket);
        Assert.Equal("/", request.Delimiter);
        Assert.Equal("url", request.EncodingType);
        Assert.Equal("key-01", request.KeyMarker);
        Assert.Equal(10001, request.MaxUploads);
        Assert.Equal("prefix-01", request.Prefix);
        Assert.Equal("upload-id-123", request.UploadIdMarker);

        input = new OperationInput();
        Serde.SerializeInput(request, ref input);

        Assert.Null(input.Headers);
        Assert.Null(input.Body);
        Assert.NotNull(input.Parameters);
        Assert.Equal(6, input.Parameters.Count);
        Assert.Equal("/", input.Parameters["delimiter"]);
        Assert.Equal("url", input.Parameters["encoding-type"]);
        Assert.Equal("key-01", input.Parameters["key-marker"]);
        Assert.Equal("10001", input.Parameters["max-uploads"]);
        Assert.Equal("prefix-01", input.Parameters["prefix"]);
        Assert.Equal("upload-id-123", input.Parameters["upload-id-marker"]);
    }

    [Fact]
    public void TestListMultipartUploadsResult() {
        var result = new ListMultipartUploadsResult();
        Assert.Equal(0, result.StatusCode);
        Assert.Equal("", result.Status);
        Assert.Equal("", result.RequestId);
        Assert.Empty(result.Headers);
        Assert.Equal("", result.BodyFormat);
        Assert.Null(result.BodyType);
        Assert.Null(result.Bucket);
        Assert.Null(result.KeyMarker);
        Assert.Null(result.UploadIdMarker);
        Assert.Null(result.NextKeyMarker);
        Assert.Null(result.NextUploadIdMarker);
        Assert.Null(result.Delimiter);
        Assert.Null(result.Prefix);
        Assert.Null(result.MaxUploads);
        Assert.Null(result.IsTruncated);
        Assert.Null(result.Uploads);

        //empty xml
        var xml = """
<?xml version="1.0" encoding="utf-8"?>
<ListMultipartUploadsResult>
</ListMultipartUploadsResult>
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
        Serde.DeserializeOutput(ref baseResult, ref output, Serde.DeserializeListMultipartUploads);

        Assert.Equal(200, result.StatusCode);
        Assert.Equal("OK", result.Status);
        Assert.Equal("123-id", result.RequestId);
        Assert.Equal(2, result.Headers.Count);
        Assert.Equal("application/xml", result.Headers["content-type"]);
        Assert.Null(result.Bucket);
        Assert.Null(result.KeyMarker);
        Assert.Null(result.UploadIdMarker);
        Assert.Null(result.NextKeyMarker);
        Assert.Null(result.NextUploadIdMarker);
        Assert.Null(result.Delimiter);
        Assert.Null(result.Prefix);
        Assert.Null(result.MaxUploads);
        Assert.Null(result.IsTruncated);
        Assert.Empty(result.Uploads);

        //full xml without url encode
        xml = """
<ListMultipartUploadsResult>
    <Bucket>oss-example</Bucket>
    <KeyMarker></KeyMarker>
    <UploadIdMarker></UploadIdMarker>
    <NextKeyMarker>oss.avi</NextKeyMarker>
    <NextUploadIdMarker>0004B99B8E707874FC2D692FA5D77D3F</NextUploadIdMarker>
    <Delimiter></Delimiter>
    <Prefix></Prefix>
    <MaxUploads>1000</MaxUploads>
    <IsTruncated>false</IsTruncated>
    <Upload>
        <Key>multipart.data</Key>
        <UploadId>0004B999EF518A1FE585B0C9360DC4C8</UploadId>
        <Initiated>2012-02-23T04:18:23.000Z</Initiated>
    </Upload>
    <Upload>
        <Key>multipart.data</Key>
        <UploadId>0004B999EF5A239BB9138C6227D6****</UploadId>
        <Initiated>2012-02-23T04:18:24.000Z</Initiated>
    </Upload>
    <Upload>
        <Key>oss.avi</Key>
        <UploadId>0004B99B8E707874FC2D692FA5D7****</UploadId>
        <Initiated>2012-02-23T06:14:27.000Z</Initiated>
    </Upload>
</ListMultipartUploadsResult>
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
        baseResult = result; Serde.DeserializeOutput(ref baseResult, ref output, Serde.DeserializeListMultipartUploads);

        Assert.Equal(200, result.StatusCode);
        Assert.Equal("OK", result.Status);
        Assert.Equal("123-id", result.RequestId);
        Assert.Equal(2, result.Headers.Count);
        Assert.Equal("application/xml", result.Headers["content-type"]);
        Assert.Equal("oss-example", result.Bucket);
        Assert.Equal(1000, result.MaxUploads);
        Assert.Equal("", result.Delimiter);
        Assert.Null(result.EncodingType);
        Assert.Equal("", result.KeyMarker);
        Assert.Equal("oss.avi", result.NextKeyMarker);
        Assert.Equal("", result.UploadIdMarker);
        Assert.Equal("0004B99B8E707874FC2D692FA5D77D3F", result.NextUploadIdMarker);
        Assert.Equal("", result.Prefix);
        Assert.Equal(false, result.IsTruncated);

        Assert.NotNull(result.Uploads);
        Assert.Equal(3, result.Uploads.Count);
        Assert.Equal("multipart.data", result.Uploads[0].Key);
        Assert.Equal("2012/2/23 4:18:23", result.Uploads[0].Initiated.ToString());
        Assert.Equal("0004B999EF518A1FE585B0C9360DC4C8", result.Uploads[0].UploadId);

        Assert.Equal("multipart.data", result.Uploads[1].Key);
        Assert.Equal("2012/2/23 4:18:24", result.Uploads[1].Initiated.ToString());
        Assert.Equal("0004B999EF5A239BB9138C6227D6****", result.Uploads[1].UploadId);

        Assert.Equal("oss.avi", result.Uploads[2].Key);
        Assert.Equal("2012/2/23 6:14:27", result.Uploads[2].Initiated.ToString());
        Assert.Equal("0004B99B8E707874FC2D692FA5D7****", result.Uploads[2].UploadId);


        //full xml with url encode
        xml = """
<ListMultipartUploadsResult>
    <Bucket>oss-example</Bucket>
    <KeyMarker>123%2F</KeyMarker>
    <UploadIdMarker>0004B99B</UploadIdMarker>
    <NextKeyMarker>123%2Foss.avi</NextKeyMarker>
    <NextUploadIdMarker>0004B99B8E707874FC2D692FA5D77D3F</NextUploadIdMarker>
    <Delimiter>%2F</Delimiter>
    <Prefix>123%2F</Prefix>
    <MaxUploads>1000</MaxUploads>
    <IsTruncated>false</IsTruncated>
    <EncodingType>url</EncodingType>
    <Upload>
        <Key>123%2Fmultipart.data</Key>
        <UploadId>0004B999EF518A1FE585B0C9360DC4C8</UploadId>
        <Initiated>2012-02-23T04:18:23.000Z</Initiated>
    </Upload>
    <Upload>
        <Key>123%2Fmultipart.data</Key>
        <UploadId>0004B999EF5A239BB9138C6227D6****</UploadId>
        <Initiated>2012-02-23T04:18:24.000Z</Initiated>
    </Upload>
    <Upload>
        <Key>123%2Foss.avi</Key>
        <UploadId>0004B99B8E707874FC2D692FA5D7****</UploadId>
        <Initiated>2012-02-23T06:14:27.000Z</Initiated>
    </Upload>
</ListMultipartUploadsResult>
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
        baseResult = result; Serde.DeserializeOutput(ref baseResult, ref output, Serde.DeserializeListMultipartUploads);

        Assert.Equal(200, result.StatusCode);
        Assert.Equal("OK", result.Status);
        Assert.Equal("123-id", result.RequestId);
        Assert.Equal(2, result.Headers.Count);
        Assert.Equal("application/xml", result.Headers["content-type"]);
        Assert.Equal("oss-example", result.Bucket);
        Assert.Equal(1000, result.MaxUploads);
        Assert.Equal("/", result.Delimiter);
        Assert.Equal("url", result.EncodingType);
        Assert.Equal("123/", result.KeyMarker);
        Assert.Equal("123/oss.avi", result.NextKeyMarker);
        Assert.Equal("0004B99B", result.UploadIdMarker);
        Assert.Equal("0004B99B8E707874FC2D692FA5D77D3F", result.NextUploadIdMarker);
        Assert.Equal("123/", result.Prefix);
        Assert.Equal(false, result.IsTruncated);

        Assert.NotNull(result.Uploads);
        Assert.Equal(3, result.Uploads.Count);
        Assert.Equal("123/multipart.data", result.Uploads[0].Key);
        Assert.Equal("2012/2/23 4:18:23", result.Uploads[0].Initiated.ToString());
        Assert.Equal("0004B999EF518A1FE585B0C9360DC4C8", result.Uploads[0].UploadId);

        Assert.Equal("123/multipart.data", result.Uploads[1].Key);
        Assert.Equal("2012/2/23 4:18:24", result.Uploads[1].Initiated.ToString());
        Assert.Equal("0004B999EF5A239BB9138C6227D6****", result.Uploads[1].UploadId);

        Assert.Equal("123/oss.avi", result.Uploads[2].Key);
        Assert.Equal("2012/2/23 6:14:27", result.Uploads[2].Initiated.ToString());
        Assert.Equal("0004B99B8E707874FC2D692FA5D7****", result.Uploads[2].UploadId);
    }

    [Fact]
    public void TestListPartsRequest() {
        var request = new ListPartsRequest();
        Assert.Empty(request.Headers);
        Assert.Empty(request.Parameters);
        Assert.Null(request.InnerBody);
        Assert.Equal("", request.BodyFormat);
        Assert.Null(request.Bucket);
        Assert.Null(request.Key);
        Assert.Null(request.EncodingType);
        Assert.Null(request.UploadId);
        Assert.Null(request.MaxParts);
        Assert.Null(request.PartNumberMarker);

        var input = new OperationInput();
        Serde.SerializeInput(request, ref input);

        Assert.Null(input.Headers);
        Assert.Null(input.Parameters);
        Assert.Null(input.Body);

        request = new ListPartsRequest {
            Bucket = "bucket",
            Key = "key-123",
            EncodingType = "url",
            UploadId = "upload-id-01",
            MaxParts = 100,
            PartNumberMarker = 1,
        };
        Assert.Empty(request.Headers);
        Assert.Equal("bucket", request.Bucket);
        Assert.Equal("key-123", request.Key);
        Assert.Equal("url", request.EncodingType);
        Assert.Equal("upload-id-01", request.UploadId);
        Assert.Equal(100, request.MaxParts);
        Assert.Equal(1, request.PartNumberMarker);

        input = new OperationInput();
        Serde.SerializeInput(request, ref input);

        Assert.Null(input.Headers);
        Assert.Null(input.Body);
        Assert.NotNull(input.Parameters);
        Assert.Equal(4, input.Parameters.Count);
        Assert.Equal("url", input.Parameters["encoding-type"]);
        Assert.Equal("upload-id-01", input.Parameters["uploadId"]);
        Assert.Equal("100", input.Parameters["max-parts"]);
        Assert.Equal("1", input.Parameters["part-number-marker"]);
    }

    [Fact]
    public void TestListPartsResult() {
        var result = new ListPartsResult();
        Assert.Equal(0, result.StatusCode);
        Assert.Equal("", result.Status);
        Assert.Equal("", result.RequestId);
        Assert.Empty(result.Headers);
        Assert.Equal("", result.BodyFormat);
        Assert.Null(result.BodyType);
        Assert.Null(result.Bucket);
        Assert.Null(result.Key);
        Assert.Null(result.UploadId);
        Assert.Null(result.PartNumberMarker);
        Assert.Null(result.NextPartNumberMarker);
        Assert.Null(result.MaxParts);
        Assert.Null(result.IsTruncated);
        Assert.Null(result.EncodingType);
        Assert.Null(result.Parts);
        Assert.Null(result.StorageClass);

        //empty xml
        var xml = """
<?xml version="1.0" encoding="utf-8"?>
<ListPartsResult>
</ListPartsResult>
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
        Serde.DeserializeOutput(ref baseResult, ref output, Serde.DeserializeListParts);

        Assert.Equal(200, result.StatusCode);
        Assert.Equal("OK", result.Status);
        Assert.Equal("123-id", result.RequestId);
        Assert.Equal(2, result.Headers.Count);
        Assert.Equal("application/xml", result.Headers["content-type"]);
        Assert.Null(result.Bucket);
        Assert.Null(result.Key);
        Assert.Null(result.UploadId);
        Assert.Null(result.PartNumberMarker);
        Assert.Null(result.NextPartNumberMarker);
        Assert.Null(result.MaxParts);
        Assert.Null(result.IsTruncated);
        Assert.Null(result.EncodingType);
        Assert.NotNull(result.Parts);
        Assert.Empty(result.Parts);
        Assert.Null(result.StorageClass);

        //full xml without url encode
        xml = """
<?xml version="1.0" encoding="utf-8"?>
<ListPartsResult>
    <Bucket>multipart_upload</Bucket>
    <Key>multipart.data</Key>
    <UploadId>0004B999EF5A239BB9138C6227D6****</UploadId>
    <NextPartNumberMarker>5</NextPartNumberMarker>
    <MaxParts>1000</MaxParts>
    <IsTruncated>false</IsTruncated>
    <StorageClass>IA</StorageClass>
    <Part>
        <PartNumber>1</PartNumber>
        <LastModified>2012-02-23T07:01:34.000Z</LastModified>
        <ETag>"3349DC700140D7F86A0784842780****"</ETag>
        <Size>6291456</Size>
    </Part>
    <Part>
        <PartNumber>2</PartNumber>
        <LastModified>2012-02-23T07:01:12.000Z</LastModified>
        <ETag>"3349DC700140D7F86A0784842780****"</ETag>
        <Size>6291456</Size>
    </Part>
    <Part>
        <PartNumber>5</PartNumber>
        <LastModified>2012-02-23T07:02:03.000Z</LastModified>
        <ETag>"7265F4D211B56873A381D321F586****"</ETag>
        <Size>1024</Size>
    </Part>
</ListPartsResult>
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
        baseResult = result; Serde.DeserializeOutput(ref baseResult, ref output, Serde.DeserializeListParts);

        Assert.Equal(200, result.StatusCode);
        Assert.Equal("OK", result.Status);
        Assert.Equal("123-id", result.RequestId);
        Assert.Equal(2, result.Headers.Count);
        Assert.Equal("application/xml", result.Headers["content-type"]);
        Assert.Equal("multipart_upload", result.Bucket);
        Assert.Equal("multipart.data", result.Key);
        Assert.Equal("0004B999EF5A239BB9138C6227D6****", result.UploadId);
        Assert.Null(result.PartNumberMarker);
        Assert.Equal(5, result.NextPartNumberMarker);
        Assert.Equal(1000, result.MaxParts);
        Assert.Equal(false, result.IsTruncated);
        Assert.Null(result.EncodingType);
        Assert.Equal("IA", result.StorageClass);

        Assert.NotNull(result.Parts);
        Assert.Equal(3, result.Parts.Count);
        Assert.Equal(1, result.Parts[0].PartNumber);
        Assert.Equal("2012/2/23 7:01:34", result.Parts[0].LastModified.ToString());
        Assert.Equal("\"3349DC700140D7F86A0784842780****\"", result.Parts[0].ETag);
        Assert.Equal(6291456, result.Parts[0].Size);

        Assert.Equal(2, result.Parts[1].PartNumber);
        Assert.Equal("2012/2/23 7:01:12", result.Parts[1].LastModified.ToString());
        Assert.Equal("\"3349DC700140D7F86A0784842780****\"", result.Parts[1].ETag);
        Assert.Equal(6291456, result.Parts[1].Size);

        Assert.Equal(5, result.Parts[2].PartNumber);
        Assert.Equal("2012/2/23 7:02:03", result.Parts[2].LastModified.ToString());
        Assert.Equal("\"7265F4D211B56873A381D321F586****\"", result.Parts[2].ETag);
        Assert.Equal(1024, result.Parts[2].Size);


        //full xml with url encode
        xml = """
<?xml version="1.0" encoding="utf-8"?>
<ListPartsResult>
    <Bucket>multipart_upload</Bucket>
    <Key>123%2Fmultipart.data</Key>
    <UploadId>0004B999EF5A239BB9138C6227D6****</UploadId>
    <PartNumberMarker>1</PartNumberMarker>
    <NextPartNumberMarker>5</NextPartNumberMarker>
    <MaxParts>1000</MaxParts>
    <IsTruncated>false</IsTruncated>
    <EncodingType>url</EncodingType>
    <Part>
        <PartNumber>1</PartNumber>
        <LastModified>2012-02-23T07:01:34.000Z</LastModified>
        <ETag>"3349DC700140D7F86A0784842780****"</ETag>
        <Size>6291456</Size>
        <HashCrc64ecma>123</HashCrc64ecma>
    </Part>
    <Part>
        <PartNumber>2</PartNumber>
        <LastModified>2012-02-23T07:01:12.000Z</LastModified>
        <ETag>"3349DC700140D7F86A0784842780****"</ETag>
        <Size>6291456</Size>
        <HashCrc64ecma>456</HashCrc64ecma>
    </Part>
    <Part>
        <PartNumber>5</PartNumber>
        <LastModified>2012-02-23T07:02:03.000Z</LastModified>
        <ETag>"7265F4D211B56873A381D321F586****"</ETag>
        <Size>1024</Size>
        <HashCrc64ecma>789</HashCrc64ecma>
    </Part>
</ListPartsResult>
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
        baseResult = result; Serde.DeserializeOutput(ref baseResult, ref output, Serde.DeserializeListParts);

        Assert.Equal(200, result.StatusCode);
        Assert.Equal("OK", result.Status);
        Assert.Equal("123-id", result.RequestId);
        Assert.Equal(2, result.Headers.Count);
        Assert.Equal("application/xml", result.Headers["content-type"]);
        Assert.Equal("multipart_upload", result.Bucket);
        Assert.Equal("123/multipart.data", result.Key);
        Assert.Equal("0004B999EF5A239BB9138C6227D6****", result.UploadId);
        Assert.Equal(1, result.PartNumberMarker);
        Assert.Equal(5, result.NextPartNumberMarker);
        Assert.Equal(1000, result.MaxParts);
        Assert.Equal(false, result.IsTruncated);
        Assert.Equal("url", result.EncodingType);

        Assert.NotNull(result.Parts);
        Assert.Equal(3, result.Parts.Count);
        Assert.Equal(1, result.Parts[0].PartNumber);
        Assert.Equal("2012/2/23 7:01:34", result.Parts[0].LastModified.ToString());
        Assert.Equal("\"3349DC700140D7F86A0784842780****\"", result.Parts[0].ETag);
        Assert.Equal(6291456, result.Parts[0].Size);
        Assert.Equal("123", result.Parts[0].HashCrc64);

        Assert.Equal(2, result.Parts[1].PartNumber);
        Assert.Equal("2012/2/23 7:01:12", result.Parts[1].LastModified.ToString());
        Assert.Equal("\"3349DC700140D7F86A0784842780****\"", result.Parts[1].ETag);
        Assert.Equal(6291456, result.Parts[1].Size);
        Assert.Equal("456", result.Parts[1].HashCrc64);

        Assert.Equal(5, result.Parts[2].PartNumber);
        Assert.Equal("2012/2/23 7:02:03", result.Parts[2].LastModified.ToString());
        Assert.Equal("\"7265F4D211B56873A381D321F586****\"", result.Parts[2].ETag);
        Assert.Equal(1024, result.Parts[2].Size);
        Assert.Equal("789", result.Parts[2].HashCrc64);
    }

    [Fact]
    public void TestListPartsResultEmptyXml() {
        var result = new ListPartsResult();
        Assert.Equal(0, result.StatusCode);
        Assert.Equal("", result.Status);
        Assert.Equal("", result.RequestId);
        Assert.Empty(result.Headers);
        Assert.Equal("", result.BodyFormat);
        Assert.Null(result.BodyType);
        Assert.Null(result.Bucket);
        Assert.Null(result.Key);
        Assert.Null(result.UploadId);
        Assert.Null(result.PartNumberMarker);
        Assert.Null(result.NextPartNumberMarker);
        Assert.Null(result.MaxParts);
        Assert.Null(result.IsTruncated);
        Assert.Null(result.EncodingType);
        Assert.Null(result.Parts);
        Assert.Null(result.StorageClass);

        //empty xml
        var xml = """
<?xml version="1.0" encoding="utf-8"?>
<ListPartsResult>
  <EncodingType>url</EncodingType>
  <Bucket>csharp-sdk-test-bucket-b915ab-1739933357063-2fad10</Bucket>
  <Key>csharp-sdk-test-object-25-1739933357890</Key>
  <UploadId>097409BE01F</UploadId>
  <StorageClass>Standard</StorageClass>
  <PartNumberMarker></PartNumberMarker>
  <NextPartNumberMarker></NextPartNumberMarker>
  <MaxParts>1000</MaxParts>
  <IsTruncated>false</IsTruncated>
</ListPartsResult>
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
        Serde.DeserializeOutput(ref baseResult, ref output, Serde.DeserializeListParts);

        Assert.Equal(200, result.StatusCode);
        Assert.Equal("OK", result.Status);
        Assert.Equal("123-id", result.RequestId);
        Assert.Equal(2, result.Headers.Count);
        Assert.Equal("application/xml", result.Headers["content-type"]);
        Assert.Equal("csharp-sdk-test-bucket-b915ab-1739933357063-2fad10", result.Bucket);
        Assert.Equal("csharp-sdk-test-object-25-1739933357890", result.Key);
        Assert.Equal("097409BE01F", result.UploadId);
        Assert.Null(result.PartNumberMarker);
        Assert.Null(result.NextPartNumberMarker);
        Assert.Equal(1000, result.MaxParts);
        Assert.False(result.IsTruncated);
        Assert.Equal("url", result.EncodingType);
        Assert.NotNull(result.Parts);
        Assert.Empty(result.Parts);
        Assert.Equal("Standard", result.StorageClass);
    }

}