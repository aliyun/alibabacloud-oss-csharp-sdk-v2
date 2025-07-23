using System.Text;
using AlibabaCloud.OSS.V2.Models;
using AlibabaCloud.OSS.V2.Transform;

namespace AlibabaCloud.OSS.V2.UnitTests.Models;

public class ModelObjectBasicTest
{
    [Fact]
    public void TestPutObjectRequest()
    {
        var request = new PutObjectRequest();
        Assert.Empty(request.Headers);
        Assert.Empty(request.Parameters);
        Assert.Null(request.InnerBody);
        Assert.Equal("", request.BodyFormat);
        Assert.Null(request.Bucket);
        Assert.Null(request.Key);
        Assert.Null(request.Acl);
        Assert.Null(request.StorageClass);
        Assert.Null(request.CacheControl);
        Assert.Null(request.ContentDisposition);
        Assert.Null(request.ContentEncoding);
        Assert.Null(request.Expires);
        Assert.Null(request.ContentMd5);
        Assert.Null(request.ContentType);
        Assert.Null(request.ContentLength);
        Assert.Null(request.Metadata);
        Assert.Null(request.Tagging);
        Assert.Null(request.ServerSideEncryption);
        Assert.Null(request.ServerSideDataEncryption);
        Assert.Null(request.ServerSideEncryptionKeyId);
        Assert.Null(request.Callback);
        Assert.Null(request.CallbackVar);
        Assert.Null(request.ForbidOverwrite);
        Assert.Null(request.TrafficLimit);
        Assert.Null(request.Body);

        var input = new OperationInput();
        Serde.SerializeInput(request, ref input, Serde.AddMetadata);

        Assert.Null(input.Headers);
        Assert.Null(input.Parameters);
        Assert.Null(input.Body);

        var body = new MemoryStream(Encoding.UTF8.GetBytes("hello world"));

        request = new PutObjectRequest
        {
            Bucket = "bucket",
            Key = "key",
            Acl = "private",
            StorageClass = "IA",
            CacheControl = "no-cache",
            ContentDisposition = "disposition",
            ContentEncoding = "gzip",
            Expires = "Thu, 01 Dec 1994 16:00:00 GMT",
            ContentLength = 12345,
            ContentMd5 = "md5-123",
            ContentType = "txt",
            Metadata = new Dictionary<string, string>() {
                { "key1", "value1" },
                { "key2", "value2" }
            },
            Tagging = "key1=value1&key2=value2",
            ServerSideEncryption = "AES256",
            ServerSideEncryptionKeyId = "kms-key-id",
            ServerSideDataEncryption = "sse-data",
            Callback = "callback",
            CallbackVar = "callback-var",
            ForbidOverwrite = true,
            TrafficLimit = 8888,
            Body = body
        };

        Assert.Equal("bucket", request.Bucket);
        Assert.Equal("key", request.Key);
        Assert.NotNull(request.Headers);
        Assert.Equal("private", request.Headers["x-oss-object-acl"]);
        Assert.Equal("private", request.Acl);
        Assert.Equal("IA", request.Headers["x-oss-storage-class"]);
        Assert.Equal("IA", request.StorageClass);
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
        Assert.Equal("key1=value1&key2=value2", request.Headers["x-oss-tagging"]);
        Assert.Equal("key1=value1&key2=value2", request.Tagging);
        Assert.Equal("AES256", request.Headers["x-oss-server-side-encryption"]);
        Assert.Equal("AES256", request.ServerSideEncryption);
        Assert.Equal("kms-key-id", request.Headers["x-oss-server-side-encryption-key-id"]);
        Assert.Equal("kms-key-id", request.ServerSideEncryptionKeyId);
        Assert.Equal("sse-data", request.Headers["x-oss-server-side-data-encryption"]);
        Assert.Equal("sse-data", request.ServerSideDataEncryption);
        Assert.Equal("callback", request.Headers["x-oss-callback"]);
        Assert.Equal("callback", request.Callback);
        Assert.Equal("callback-var", request.Headers["x-oss-callback-var"]);
        Assert.Equal("callback-var", request.CallbackVar);
        Assert.Equal("true", request.Headers["x-oss-forbid-overwrite"]);
        Assert.Equal(true, request.ForbidOverwrite);
        Assert.Equal("8888", request.Headers["x-oss-traffic-limit"]);
        Assert.Equal(8888, request.TrafficLimit);

        Assert.NotNull(request.Parameters);
        Assert.Empty(request.Parameters);

        Serde.SerializeInput(request, ref input, Serde.AddMetadata);

        Assert.NotNull(input.Headers);
        Assert.Equal("private", request.Headers["x-oss-object-acl"]);
        Assert.Equal("IA", input.Headers["x-oss-storage-class"]);
        Assert.Equal("no-cache", input.Headers["Cache-Control"]);
        Assert.Equal("disposition", input.Headers["Content-Disposition"]);
        Assert.Equal("gzip", input.Headers["Content-Encoding"]);
        Assert.Equal("12345", input.Headers["Content-Length"]);
        Assert.Equal("md5-123", input.Headers["Content-MD5"]);
        Assert.Equal("txt", input.Headers["Content-Type"]);
        Assert.Equal("Thu, 01 Dec 1994 16:00:00 GMT", input.Headers["Expires"]);
        Assert.Equal("value1", input.Headers["x-oss-meta-key1"]);
        Assert.Equal("value2", input.Headers["x-oss-meta-key2"]);
        Assert.Equal("key1=value1&key2=value2", input.Headers["x-oss-tagging"]);
        Assert.Equal("AES256", input.Headers["x-oss-server-side-encryption"]);
        Assert.Equal("kms-key-id", input.Headers["x-oss-server-side-encryption-key-id"]);
        Assert.Equal("sse-data", input.Headers["x-oss-server-side-data-encryption"]);
        Assert.Equal("callback", input.Headers["x-oss-callback"]);
        Assert.Equal("callback-var", input.Headers["x-oss-callback-var"]);
        Assert.Equal("true", input.Headers["x-oss-forbid-overwrite"]);
        Assert.Equal("8888", input.Headers["x-oss-traffic-limit"]);

        Assert.Null(input.Parameters);
        Assert.Equal(body, input.Body);
    }

    [Fact]
    public void TestPutObjectResult()
    {
        var result = new PutObjectResult();
        Assert.Equal(0, result.StatusCode);
        Assert.Equal("", result.Status);
        Assert.Equal("", result.RequestId);
        Assert.Empty(result.Headers);
        Assert.Null(result.HashCrc64);
        Assert.Null(result.VersionId);
        Assert.Null(result.ContentMd5);
        Assert.Null(result.ETag);
        Assert.Null(result.CallbackResult);

        var output = new OperationOutput
        {
            StatusCode = 200,
            Status = "OK",
            Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
                { "x-oss-request-id", "123-id" },
                { "Content-Type", "txt" },
                {"Content-MD5","MD5-123"},
                {"ETag","etag-123"},
                {"x-oss-hash-crc64ecma","123456"},
                {"x-oss-version-id","123"}
            }
        };
        ResultModel baseResult = result;
        Serde.DeserializeOutput(ref baseResult, ref output);

        Assert.Equal(200, result.StatusCode);
        Assert.Equal("OK", result.Status);
        Assert.Equal("123-id", result.RequestId);
        Assert.Equal(6, result.Headers.Count);
        Assert.Equal("txt", result.Headers["content-type"]);
        Assert.Equal("MD5-123", result.ContentMd5);
        Assert.Equal("etag-123", result.ETag);
        Assert.Equal("123456", result.HashCrc64);
        Assert.Equal("123", result.VersionId);
        Assert.Null(result.CallbackResult);

        //callback
        result = new PutObjectResult
        {
            BodyFormat = "string",
            BodyType = typeof(string)

        };
        output = new OperationOutput
        {
            StatusCode = 200,
            Status = "OK",
            Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
                { "x-oss-request-id", "123-id" },
                { "Content-Type", "txt" },
                {"Content-MD5","MD5-123"},
                {"ETag","etag-123"},
                {"x-oss-hash-crc64ecma","123456"},
                {"x-oss-version-id","123"}
            },
            Body = new MemoryStream(Encoding.UTF8.GetBytes("hello world"))
        };
        baseResult = result;
        Serde.DeserializeOutput(ref baseResult, ref output, Serde.DeserializerAnyBody);

        Assert.Equal(200, result.StatusCode);
        Assert.Equal("OK", result.Status);
        Assert.Equal("123-id", result.RequestId);
        Assert.Equal(6, result.Headers.Count);
        Assert.Equal("txt", result.Headers["content-type"]);
        Assert.Equal("MD5-123", result.ContentMd5);
        Assert.Equal("etag-123", result.ETag);
        Assert.Equal("123456", result.HashCrc64);
        Assert.Equal("123", result.VersionId);
        Assert.Equal("hello world", result.CallbackResult);
    }

    [Fact]
    public void TestCopyObjectRequest()
    {
        var request = new CopyObjectRequest();
        Assert.Empty(request.Headers);
        Assert.Empty(request.Parameters);
        Assert.Null(request.InnerBody);
        Assert.Equal("", request.BodyFormat);
        Assert.Null(request.Bucket);
        Assert.Null(request.Key);
        Assert.Null(request.SourceBucket);
        Assert.Null(request.SourceKey);
        Assert.Null(request.SourceVersionId);
        Assert.Null(request.IfMatch);
        Assert.Null(request.IfNoneMatch);
        Assert.Null(request.IfUnmodifiedSince);
        Assert.Null(request.IfModifiedSince);
        Assert.Null(request.Acl);
        Assert.Null(request.StorageClass);
        Assert.Null(request.CacheControl);
        Assert.Null(request.ContentDisposition);
        Assert.Null(request.ContentEncoding);
        Assert.Null(request.Expires);
        Assert.Null(request.ContentMd5);
        Assert.Null(request.ContentType);
        Assert.Null(request.ContentLength);
        Assert.Null(request.Metadata);
        Assert.Null(request.MetadataDirective);
        Assert.Null(request.Tagging);
        Assert.Null(request.TaggingDirective);
        Assert.Null(request.ServerSideEncryption);
        Assert.Null(request.ServerSideDataEncryption);
        Assert.Null(request.ServerSideEncryptionKeyId);
        Assert.Null(request.ForbidOverwrite);
        Assert.Null(request.TrafficLimit);

        var input = new OperationInput();
        Serde.SerializeInput(request, ref input, Serde.AddCopySource, Serde.AddMetadata);

        Assert.Null(input.Headers);
        Assert.Null(input.Parameters);
        Assert.Null(input.Body);

        request = new CopyObjectRequest
        {
            Bucket = "bucket",
            Key = "key",
            SourceBucket = "src-bucket",
            SourceKey = "src-key+?123.txt",
            SourceVersionId = "src-version-id",
            IfMatch = "if-match",
            IfNoneMatch = "if-none-match",
            IfUnmodifiedSince = "if_unmodified_since",
            IfModifiedSince = "if_modified_since",
            Acl = "private",
            StorageClass = "IA",
            CacheControl = "no-cache",
            ContentDisposition = "disposition",
            ContentEncoding = "gzip",
            Expires = "Thu, 01 Dec 1994 16:00:00 GMT",
            ContentLength = 12345,
            ContentMd5 = "md5-123",
            ContentType = "txt",
            Metadata = new Dictionary<string, string>() {
                { "key1", "value1" },
                { "key2", "value2" }
            },
            MetadataDirective = "COPY",
            Tagging = "key1=value1&key2=value2",
            TaggingDirective = "REPLACE",
            ServerSideEncryption = "AES256",
            ServerSideEncryptionKeyId = "kms-key-id",
            ServerSideDataEncryption = "sse-data",
            ForbidOverwrite = true,
            TrafficLimit = 8888,
        };

        Assert.Equal("bucket", request.Bucket);
        Assert.Equal("key", request.Key);
        Assert.NotNull(request.Headers);
        Assert.Equal("src-bucket", request.SourceBucket);
        Assert.Equal("src-key+?123.txt", request.SourceKey);
        Assert.Equal("src-version-id", request.SourceVersionId);
        Assert.Equal("if-match", request.Headers["x-oss-copy-source-if-match"]);
        Assert.Equal("if-match", request.IfMatch);
        Assert.Equal("if-none-match", request.Headers["x-oss-copy-source-if-none-match"]);
        Assert.Equal("if-none-match", request.IfNoneMatch);
        Assert.Equal("if_unmodified_since", request.Headers["x-oss-copy-source-if-unmodified-since"]);
        Assert.Equal("if_unmodified_since", request.IfUnmodifiedSince);
        Assert.Equal("if_modified_since", request.Headers["x-oss-copy-source-if-modified-since"]);
        Assert.Equal("if_modified_since", request.IfModifiedSince);
        Assert.Equal("private", request.Headers["x-oss-object-acl"]);
        Assert.Equal("private", request.Acl);
        Assert.Equal("IA", request.Headers["x-oss-storage-class"]);
        Assert.Equal("IA", request.StorageClass);
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
        Assert.Equal("COPY", request.MetadataDirective);
        Assert.Equal("COPY", request.Headers["x-oss-metadata-directive"]);
        Assert.Equal("key1=value1&key2=value2", request.Headers["x-oss-tagging"]);
        Assert.Equal("key1=value1&key2=value2", request.Tagging);
        Assert.Equal("REPLACE", request.TaggingDirective);
        Assert.Equal("REPLACE", request.Headers["x-oss-tagging-directive"]);
        Assert.Equal("AES256", request.Headers["x-oss-server-side-encryption"]);
        Assert.Equal("AES256", request.ServerSideEncryption);
        Assert.Equal("kms-key-id", request.Headers["x-oss-server-side-encryption-key-id"]);
        Assert.Equal("kms-key-id", request.ServerSideEncryptionKeyId);
        Assert.Equal("sse-data", request.Headers["x-oss-server-side-data-encryption"]);
        Assert.Equal("sse-data", request.ServerSideDataEncryption);
        Assert.Equal("true", request.Headers["x-oss-forbid-overwrite"]);
        Assert.Equal(true, request.ForbidOverwrite);
        Assert.Equal("8888", request.Headers["x-oss-traffic-limit"]);
        Assert.Equal(8888, request.TrafficLimit);

        Assert.NotNull(request.Parameters);
        Assert.Empty(request.Parameters);

        Serde.SerializeInput(request, ref input, Serde.AddCopySource, Serde.AddMetadata);

        Assert.NotNull(input.Headers);
        Assert.Equal("/src-bucket/src-key%2B%3F123.txt?versionId=src-version-id", input.Headers["x-oss-copy-source"]);
        Assert.Equal("if-match", input.Headers["x-oss-copy-source-if-match"]);
        Assert.Equal("if-none-match", input.Headers["x-oss-copy-source-if-none-match"]);
        Assert.Equal("if_unmodified_since", input.Headers["x-oss-copy-source-if-unmodified-since"]);
        Assert.Equal("if_modified_since", input.Headers["x-oss-copy-source-if-modified-since"]);
        Assert.Equal("private", request.Headers["x-oss-object-acl"]);
        Assert.Equal("IA", input.Headers["x-oss-storage-class"]);
        Assert.Equal("no-cache", input.Headers["Cache-Control"]);
        Assert.Equal("disposition", input.Headers["Content-Disposition"]);
        Assert.Equal("gzip", input.Headers["Content-Encoding"]);
        Assert.Equal("12345", input.Headers["Content-Length"]);
        Assert.Equal("md5-123", input.Headers["Content-MD5"]);
        Assert.Equal("txt", input.Headers["Content-Type"]);
        Assert.Equal("Thu, 01 Dec 1994 16:00:00 GMT", input.Headers["Expires"]);
        Assert.Equal("value1", input.Headers["x-oss-meta-key1"]);
        Assert.Equal("value2", input.Headers["x-oss-meta-key2"]);
        Assert.Equal("COPY", input.Headers["x-oss-metadata-directive"]);
        Assert.Equal("key1=value1&key2=value2", input.Headers["x-oss-tagging"]);
        Assert.Equal("REPLACE", input.Headers["x-oss-tagging-directive"]);
        Assert.Equal("AES256", input.Headers["x-oss-server-side-encryption"]);
        Assert.Equal("kms-key-id", input.Headers["x-oss-server-side-encryption-key-id"]);
        Assert.Equal("sse-data", input.Headers["x-oss-server-side-data-encryption"]);
        Assert.Equal("true", input.Headers["x-oss-forbid-overwrite"]);
        Assert.Equal("8888", input.Headers["x-oss-traffic-limit"]);

        Assert.Null(input.Parameters);
        Assert.Null(input.Body);

        // no source bucket
        request = new CopyObjectRequest
        {
            Bucket = "bucket",
            Key = "key",
            SourceKey = "src-key+?123.txt",
        };
        input = new OperationInput();

        Assert.Equal("bucket", request.Bucket);
        Assert.Equal("key", request.Key);
        Assert.NotNull(request.Headers);
        Assert.Null(request.SourceBucket);
        Assert.Equal("src-key+?123.txt", request.SourceKey);
        Assert.Null(request.SourceVersionId);

        Assert.NotNull(request.Parameters);
        Assert.Empty(request.Parameters);

        Serde.SerializeInput(request, ref input, Serde.AddCopySource, Serde.AddMetadata);

        Assert.NotNull(input.Headers);
        Assert.Equal("/bucket/src-key%2B%3F123.txt", input.Headers["x-oss-copy-source"]);
        Assert.Single(input.Headers);

        Assert.Null(input.Parameters);
        Assert.Null(input.Body);
    }

    [Fact]
    public void TestCopyObjectResult()
    {
        var result = new CopyObjectResult();
        Assert.Equal(0, result.StatusCode);
        Assert.Equal("", result.Status);
        Assert.Equal("", result.RequestId);
        Assert.Empty(result.Headers);
        Assert.Null(result.HashCrc64);
        Assert.Null(result.VersionId);
        Assert.Null(result.SourceVersionId);
        Assert.Null(result.ETag);
        Assert.Null(result.LastModified);
        Assert.Null(result.ServerSideEncryption);
        Assert.Null(result.ServerSideDataEncryption);
        Assert.Null(result.ServerSideEncryptionKeyId);

        var xml = """
<?xml version="1.0" encoding="utf-8"?>
<CopyObjectResult>
    <LastModified>2019-04-09T07:01:56.000Z</LastModified>
    <ETag>"25A9F4ABFCC05743DF6E2C886C56****"</ETag>
</CopyObjectResult>
""";

        var output = new OperationOutput
        {
            StatusCode = 200,
            Status = "OK",
            Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
                { "x-oss-request-id", "123-id" },
                { "Content-Type", "txt" },
                {"x-oss-hash-crc64ecma","123456"},
                {"x-oss-version-id","123"},
                {"x-oss-copy-source-version-id","src-123"},
                {"x-oss-server-side-encryption","sse-123"},
                {"x-oss-server-side-data-encryption","sse-data-123"},
                {"x-oss-server-side-encryption-key-id","sse-kms-id"},
            },
            Body = new MemoryStream(Encoding.UTF8.GetBytes(xml))
        };
        ResultModel baseResult = result;
        Serde.DeserializeOutput(ref baseResult, ref output, Serde.DeserializeCopyObject);

        Assert.Equal(200, result.StatusCode);
        Assert.Equal("OK", result.Status);
        Assert.Equal("123-id", result.RequestId);
        Assert.Equal(8, result.Headers.Count);
        Assert.Equal("txt", result.Headers["content-type"]);
        Assert.Equal("123456", result.HashCrc64);
        Assert.Equal("123", result.VersionId);
        Assert.Equal("src-123", result.SourceVersionId);
        Assert.Equal("sse-123", result.ServerSideEncryption);
        Assert.Equal("sse-data-123", result.ServerSideDataEncryption);
        Assert.Equal("sse-kms-id", result.ServerSideEncryptionKeyId);
        Assert.Equal(new DateTime(2019, 4, 9, 7, 1, 56), result.LastModified);
        Assert.Equal("\"25A9F4ABFCC05743DF6E2C886C56****\"", result.ETag);
    }

    [Fact]
    public void TestGetObjectRequest()
    {
        var request = new GetObjectRequest();
        Assert.Empty(request.Headers);
        Assert.Empty(request.Parameters);
        Assert.Null(request.InnerBody);
        Assert.Equal("", request.BodyFormat);
        Assert.Null(request.Bucket);
        Assert.Null(request.Key);
        Assert.Null(request.Range);
        Assert.Null(request.RangeBehavior);
        Assert.Null(request.IfMatch);
        Assert.Null(request.IfNoneMatch);
        Assert.Null(request.IfModifiedSince);
        Assert.Null(request.IfUnmodifiedSince);
        Assert.Null(request.ResponseContentType);
        Assert.Null(request.ResponseCacheControl);
        Assert.Null(request.ResponseContentDisposition);
        Assert.Null(request.ResponseContentEncoding);
        Assert.Null(request.ResponseContentLanguage);
        Assert.Null(request.ResponseExpires);
        Assert.Null(request.VersionId);
        Assert.Null(request.TrafficLimit);
        Assert.Null(request.Process);

        var input = new OperationInput();
        Serde.SerializeInput(request, ref input);

        Assert.Null(input.Headers);
        Assert.Null(input.Parameters);
        Assert.Null(input.Body);

        request = new GetObjectRequest
        {
            Bucket = "bucket",
            Key = "key",
            Range = "byte=0-1",
            RangeBehavior = "standard",
            IfMatch = "if-match",
            IfNoneMatch = "if-none-match",
            IfUnmodifiedSince = "if_unmodified_since",
            IfModifiedSince = "if_modified_since",
            ResponseContentType = "txt",
            ResponseCacheControl = "no-cache",
            ResponseContentDisposition = "a.jpg",
            ResponseContentEncoding = "utf-8",
            ResponseContentLanguage = "en",
            ResponseExpires = "Expires",
            VersionId = "version-id",
            TrafficLimit = 8888,
            Process = "image process"
        };

        Assert.Equal("bucket", request.Bucket);
        Assert.Equal("key", request.Key);
        Assert.NotNull(request.Headers);
        Assert.Equal("byte=0-1", request.Headers["Range"]);
        Assert.Equal("byte=0-1", request.Range);
        Assert.Equal("standard", request.Headers["x-oss-range-behavior"]);
        Assert.Equal("standard", request.RangeBehavior);
        Assert.Equal("if-match", request.Headers["If-Match"]);
        Assert.Equal("if-match", request.IfMatch);
        Assert.Equal("if-none-match", request.Headers["If-None-Match"]);
        Assert.Equal("if-none-match", request.IfNoneMatch);
        Assert.Equal("if_unmodified_since", request.Headers["If-Unmodified-Since"]);
        Assert.Equal("if_unmodified_since", request.IfUnmodifiedSince);
        Assert.Equal("if_modified_since", request.Headers["If-Modified-Since"]);
        Assert.Equal("if_modified_since", request.IfModifiedSince);
        Assert.Equal("8888", request.Headers["x-oss-traffic-limit"]);
        Assert.Equal(8888, request.TrafficLimit);

        Assert.NotNull(request.Parameters);
        Assert.Equal("txt", request.Parameters["response-content-type"]);
        Assert.Equal("txt", request.ResponseContentType);
        Assert.Equal("no-cache", request.Parameters["response-cache-control"]);
        Assert.Equal("no-cache", request.ResponseCacheControl);
        Assert.Equal("a.jpg", request.Parameters["response-content-disposition"]);
        Assert.Equal("a.jpg", request.ResponseContentDisposition);
        Assert.Equal("utf-8", request.Parameters["response-content-encoding"]);
        Assert.Equal("utf-8", request.ResponseContentEncoding);
        Assert.Equal("en", request.Parameters["response-content-language"]);
        Assert.Equal("en", request.ResponseContentLanguage);
        Assert.Equal("Expires", request.Parameters["response-expires"]);
        Assert.Equal("Expires", request.ResponseExpires);
        Assert.Equal("version-id", request.Parameters["versionId"]);
        Assert.Equal("version-id", request.VersionId);
        Assert.Equal("image process", request.Parameters["x-oss-process"]);
        Assert.Equal("image process", request.Process);

        Serde.SerializeInput(request, ref input);

        Assert.NotNull(input.Headers);
        Assert.Equal("byte=0-1", input.Headers["Range"]);
        Assert.Equal("standard", input.Headers["x-oss-range-behavior"]);
        Assert.Equal("if-match", input.Headers["If-Match"]);
        Assert.Equal("if-none-match", input.Headers["If-None-Match"]);
        Assert.Equal("if_unmodified_since", input.Headers["If-Unmodified-Since"]);
        Assert.Equal("if_modified_since", input.Headers["If-Modified-Since"]);
        Assert.Equal("8888", request.Headers["x-oss-traffic-limit"]);

        Assert.NotNull(input.Parameters);
        Assert.Equal("txt", input.Parameters["response-content-type"]);
        Assert.Equal("no-cache", input.Parameters["response-cache-control"]);
        Assert.Equal("a.jpg", input.Parameters["response-content-disposition"]);
        Assert.Equal("utf-8", input.Parameters["response-content-encoding"]);
        Assert.Equal("en", input.Parameters["response-content-language"]);
        Assert.Equal("Expires", input.Parameters["response-expires"]);
        Assert.Equal("version-id", input.Parameters["versionId"]);
        Assert.Equal("image process", input.Parameters["x-oss-process"]);
        Assert.Null(input.Body);
    }

    [Fact]
    public void TestGetObjectResult()
    {
        var result = new GetObjectResult();
        Assert.Equal(0, result.StatusCode);
        Assert.Equal("", result.Status);
        Assert.Equal("", result.RequestId);
        Assert.Empty(result.Headers);
        Assert.Null(result.ContentLength);
        Assert.Null(result.ContentRange);
        Assert.Null(result.ContentType);
        Assert.Null(result.ETag);
        Assert.Null(result.LastModified);
        Assert.Null(result.ContentMd5);
        Assert.NotNull(result.Metadata);
        Assert.Empty(result.Metadata);
        Assert.Null(result.CacheControl);
        Assert.Null(result.ContentDisposition);
        Assert.Null(result.ContentEncoding);
        Assert.Null(result.Expires);
        Assert.Null(result.HashCrc64);
        Assert.Null(result.StorageClass);
        Assert.Null(result.ObjectType);
        Assert.Null(result.VersionId);
        Assert.Null(result.TaggingCount);
        Assert.Null(result.ServerSideEncryption);
        Assert.Null(result.ServerSideDataEncryption);
        Assert.Null(result.ServerSideEncryptionKeyId);
        Assert.Null(result.NextAppendPosition);
        Assert.Null(result.Expiration);
        Assert.Null(result.Restore);
        Assert.Null(result.ProcessStatus);
        Assert.Null(result.DeleteMarker);
        Assert.Null(result.Body);

        var output = new OperationOutput
        {
            StatusCode = 200,
            Status = "OK",
            Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
                {"x-oss-request-id", "123-id"},
            }
        };
        ResultModel baseResult = result;
        Serde.DeserializeOutput(ref baseResult, ref output, Serde.DeserializerAnyBody);

        Assert.Equal(200, result.StatusCode);
        Assert.Equal("OK", result.Status);
        Assert.Equal("123-id", result.RequestId);
        Assert.Single(result.Headers);
        Assert.Null(result.ContentLength);
        Assert.Null(result.ContentRange);
        Assert.Null(result.ContentType);
        Assert.Null(result.ETag);
        Assert.Null(result.LastModified);
        Assert.Null(result.ContentMd5);
        Assert.NotNull(result.Metadata);
        Assert.Empty(result.Metadata);
        Assert.Null(result.CacheControl);
        Assert.Null(result.ContentDisposition);
        Assert.Null(result.ContentEncoding);
        Assert.Null(result.Expires);
        Assert.Null(result.HashCrc64);
        Assert.Null(result.StorageClass);
        Assert.Null(result.ObjectType);
        Assert.Null(result.VersionId);
        Assert.Null(result.TaggingCount);
        Assert.Null(result.ServerSideEncryption);
        Assert.Null(result.ServerSideDataEncryption);
        Assert.Null(result.ServerSideEncryptionKeyId);
        Assert.Null(result.NextAppendPosition);
        Assert.Null(result.Expiration);
        Assert.Null(result.Restore);
        Assert.Null(result.ProcessStatus);
        Assert.Null(result.DeleteMarker);
        Assert.Null(result.Body);

        //all header & body
        var body = new MemoryStream(Encoding.UTF8.GetBytes("hello world"));
        output = new OperationOutput
        {
            StatusCode = 200,
            Status = "OK",
            Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
                {"x-oss-request-id", "123-id"},
                {"Content-Length", "123456789"},
                {"Content-Range", "content_range 123"},
                {"Content-Type", "txt"},
                {"ETag", "etag 123"},
                {"Last-Modified", "Fri, 24 Feb 2012 06:07:48 GMT"},
                {"Content-MD5", "content_md5 123"},
                {"Cache-Control", "no-cache"},
                {"Content-Disposition", "1.jpg"},
                {"Content-Encoding", "gzip"},
                {"Expires", "expires 123"},
                {"x-oss-meta-key1", "value1"},
                {"x-oss-meta-Key2", "value2"},
                {"x-oss-hash-crc64ecma", "123456"},
                {"x-oss-storage-class", "IA"},
                {"x-oss-object-type", "Normal"},
                {"x-oss-version-id", "version-id-123"},
                {"x-oss-tagging-count", "100"},
                {"x-oss-server-side-encryption", "KMS"},
                {"x-oss-server-side-data-encryption", "SM4"},
                {"x-oss-server-side-encryption-key-id", "kms-id"},
                {"x-oss-next-append-position", "212"},
                {"x-oss-expiration", "expiration 123"},
                {"x-oss-restore", "restore 123"},
                {"x-oss-process-status", "process 123"},
                {"x-oss-delete-marker", "true"},
            },
            Body = body,
        };
        result = new GetObjectResult();
        baseResult = result;
        Serde.DeserializeOutput(ref baseResult, ref output, Serde.DeserializerAnyBody);

        Assert.Equal(200, result.StatusCode);
        Assert.Equal("OK", result.Status);
        Assert.Equal("123-id", result.RequestId);
        Assert.NotNull(result.Headers);
        Assert.Equal(123456789, result.ContentLength);
        Assert.Equal("content_range 123", result.ContentRange);
        Assert.Equal("txt", result.ContentType);
        Assert.Equal("etag 123", result.ETag);
        Assert.Equal("Fri, 24 Feb 2012 06:07:48 GMT", result.LastModified);
        Assert.Equal("content_md5 123", result.ContentMd5);
        var metadata = result.Metadata;
        Assert.NotNull(metadata);
        Assert.Equal(2, metadata.Count);
        Assert.Equal("value1", metadata["key1"]);
        Assert.Equal("value2", metadata["key2"]);
        Assert.Equal("no-cache", result.CacheControl);
        Assert.Equal("1.jpg", result.ContentDisposition);
        Assert.Equal("gzip", result.ContentEncoding);
        Assert.Equal("expires 123", result.Expires);
        Assert.Equal("123456", result.HashCrc64);
        Assert.Equal("IA", result.StorageClass);
        Assert.Equal("Normal", result.ObjectType);
        Assert.Equal("version-id-123", result.VersionId);
        Assert.Equal(100, result.TaggingCount);
        Assert.Equal("KMS", result.ServerSideEncryption);
        Assert.Equal("SM4", result.ServerSideDataEncryption);
        Assert.Equal("kms-id", result.ServerSideEncryptionKeyId);
        Assert.Equal(212, result.NextAppendPosition);
        Assert.Equal("expiration 123", result.Expiration);
        Assert.Equal("restore 123", result.Restore);
        Assert.Equal("process 123", result.ProcessStatus);
        Assert.Equal(true, result.DeleteMarker);
        Assert.Equal(body, result.Body);
    }

    [Fact]
    public void TestAppendObjectRequest()
    {
        var request = new AppendObjectRequest();
        Assert.Empty(request.Headers);
        Assert.Empty(request.Parameters);
        Assert.Null(request.InnerBody);
        Assert.Equal("", request.BodyFormat);
        Assert.Null(request.Bucket);
        Assert.Null(request.Key);
        Assert.Null(request.Position);
        Assert.Null(request.Acl);
        Assert.Null(request.StorageClass);
        Assert.Null(request.CacheControl);
        Assert.Null(request.ContentDisposition);
        Assert.Null(request.ContentEncoding);
        Assert.Null(request.Expires);
        Assert.Null(request.ContentMd5);
        Assert.Null(request.ContentType);
        Assert.Null(request.ContentLength);
        Assert.Null(request.Metadata);
        Assert.Null(request.Tagging);
        Assert.Null(request.ServerSideEncryption);
        Assert.Null(request.ServerSideDataEncryption);
        Assert.Null(request.ServerSideEncryptionKeyId);
        Assert.Null(request.ForbidOverwrite);
        Assert.Null(request.TrafficLimit);
        Assert.Null(request.Body);

        var input = new OperationInput();
        Serde.SerializeInput(request, ref input, Serde.AddMetadata);

        Assert.Null(input.Headers);
        Assert.Null(input.Parameters);
        Assert.Null(input.Body);

        var body = new MemoryStream(Encoding.UTF8.GetBytes("hello world"));

        request = new AppendObjectRequest
        {
            Bucket = "bucket",
            Key = "key",
            Position = 111,
            Acl = "private",
            StorageClass = "IA",
            CacheControl = "no-cache",
            ContentDisposition = "disposition",
            ContentEncoding = "gzip",
            Expires = "Thu, 01 Dec 1994 16:00:00 GMT",
            ContentLength = 12345,
            ContentMd5 = "md5-123",
            ContentType = "txt",
            Metadata = new Dictionary<string, string>() {
                { "key1", "value1" },
                { "key2", "value2" }
            },
            Tagging = "key1=value1&key2=value2",
            ServerSideEncryption = "AES256",
            ServerSideEncryptionKeyId = "kms-key-id",
            ServerSideDataEncryption = "sse-data",
            ForbidOverwrite = true,
            TrafficLimit = 8888,
            Body = body
        };

        Assert.Equal("bucket", request.Bucket);
        Assert.Equal("key", request.Key);
        Assert.NotNull(request.Headers);
        Assert.Equal("private", request.Headers["x-oss-object-acl"]);
        Assert.Equal("private", request.Acl);
        Assert.Equal("IA", request.Headers["x-oss-storage-class"]);
        Assert.Equal("IA", request.StorageClass);
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
        Assert.Equal("key1=value1&key2=value2", request.Headers["x-oss-tagging"]);
        Assert.Equal("key1=value1&key2=value2", request.Tagging);
        Assert.Equal("AES256", request.Headers["x-oss-server-side-encryption"]);
        Assert.Equal("AES256", request.ServerSideEncryption);
        Assert.Equal("kms-key-id", request.Headers["x-oss-server-side-encryption-key-id"]);
        Assert.Equal("kms-key-id", request.ServerSideEncryptionKeyId);
        Assert.Equal("sse-data", request.Headers["x-oss-server-side-data-encryption"]);
        Assert.Equal("sse-data", request.ServerSideDataEncryption);
        Assert.Equal("true", request.Headers["x-oss-forbid-overwrite"]);
        Assert.Equal(true, request.ForbidOverwrite);
        Assert.Equal("8888", request.Headers["x-oss-traffic-limit"]);
        Assert.Equal(8888, request.TrafficLimit);

        Assert.NotNull(request.Parameters);
        Assert.Equal("111", request.Parameters["position"]);
        Assert.Equal(111, request.Position);

        Serde.SerializeInput(request, ref input, Serde.AddMetadata);

        Assert.NotNull(input.Headers);
        Assert.Equal("private", request.Headers["x-oss-object-acl"]);
        Assert.Equal("IA", input.Headers["x-oss-storage-class"]);
        Assert.Equal("no-cache", input.Headers["Cache-Control"]);
        Assert.Equal("disposition", input.Headers["Content-Disposition"]);
        Assert.Equal("gzip", input.Headers["Content-Encoding"]);
        Assert.Equal("12345", input.Headers["Content-Length"]);
        Assert.Equal("md5-123", input.Headers["Content-MD5"]);
        Assert.Equal("txt", input.Headers["Content-Type"]);
        Assert.Equal("Thu, 01 Dec 1994 16:00:00 GMT", input.Headers["Expires"]);
        Assert.Equal("value1", input.Headers["x-oss-meta-key1"]);
        Assert.Equal("value2", input.Headers["x-oss-meta-key2"]);
        Assert.Equal("key1=value1&key2=value2", input.Headers["x-oss-tagging"]);
        Assert.Equal("AES256", input.Headers["x-oss-server-side-encryption"]);
        Assert.Equal("kms-key-id", input.Headers["x-oss-server-side-encryption-key-id"]);
        Assert.Equal("sse-data", input.Headers["x-oss-server-side-data-encryption"]);
        Assert.Equal("true", input.Headers["x-oss-forbid-overwrite"]);
        Assert.Equal("8888", input.Headers["x-oss-traffic-limit"]);

        Assert.NotNull(input.Parameters);
        Assert.Equal("111", input.Parameters["position"]);
        Assert.Equal(body, input.Body);
    }

    [Fact]
    public void TestAppendObjectResult()
    {
        var result = new AppendObjectResult();
        Assert.Equal(0, result.StatusCode);
        Assert.Equal("", result.Status);
        Assert.Equal("", result.RequestId);
        Assert.Empty(result.Headers);
        Assert.Null(result.NextAppendPosition);
        Assert.Null(result.VersionId);
        Assert.Null(result.HashCrc64);
        Assert.Null(result.ServerSideEncryption);
        Assert.Null(result.ServerSideDataEncryption);
        Assert.Null(result.ServerSideEncryptionKeyId);

        var output = new OperationOutput
        {
            StatusCode = 200,
            Status = "OK",
            Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
                { "x-oss-request-id", "123-id" },
                { "Content-Type", "txt" },
                { "Content-MD5", "MD5-123" },
                { "ETag", "etag-123" },
                { "x-oss-hash-crc64ecma", "123456" },
                { "x-oss-version-id", "123" },
                { "x-oss-next-append-position", "1234" },
                { "x-oss-server-side-encryption", "sse-123" },
                { "x-oss-server-side-data-encryption", "sse-data-123" },
                { "x-oss-server-side-encryption-key-id", "sse-kms-id" }
            }
        };
        ResultModel baseResult = result;
        Serde.DeserializeOutput(ref baseResult, ref output);

        Assert.Equal(200, result.StatusCode);
        Assert.Equal("OK", result.Status);
        Assert.Equal("123-id", result.RequestId);
        Assert.Equal(10, result.Headers.Count);
        Assert.Equal("txt", result.Headers["content-type"]);
        Assert.Equal(1234, result.NextAppendPosition);
        Assert.Equal("123456", result.HashCrc64);
        Assert.Equal("123", result.VersionId);
        Assert.Equal("sse-123", result.ServerSideEncryption);
        Assert.Equal("sse-data-123", result.ServerSideDataEncryption);
        Assert.Equal("sse-kms-id", result.ServerSideEncryptionKeyId);
    }

    [Fact]
    public void TestDeleteObjectRequest()
    {
        var request = new DeleteObjectRequest();
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

        request = new DeleteObjectRequest
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
        Assert.Null(input.Body);
        Assert.NotNull(input.Parameters);
        Assert.Equal("version-id", input.Parameters["versionId"]);
    }

    [Fact]
    public void TestDeleteObjectResult()
    {
        var result = new DeleteObjectResult();
        Assert.Equal(0, result.StatusCode);
        Assert.Equal("", result.Status);
        Assert.Equal("", result.RequestId);
        Assert.Empty(result.Headers);
        Assert.Null(result.VersionId);
        Assert.Null(result.DeleteMarker);

        var output = new OperationOutput
        {
            StatusCode = 200,
            Status = "OK",
            Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
                { "x-oss-request-id", "123-id" },
                { "Content-Type", "txt" },
                { "x-oss-version-id", "version-id-123" },
                { "x-oss-delete-marker", "true" },
            }
        };
        ResultModel baseResult = result;
        Serde.DeserializeOutput(ref baseResult, ref output);

        Assert.Equal(200, result.StatusCode);
        Assert.Equal("OK", result.Status);
        Assert.Equal("123-id", result.RequestId);
        Assert.Equal(4, result.Headers.Count);
        Assert.Equal("txt", result.Headers["content-type"]);
        Assert.Equal("version-id-123", result.VersionId);
        Assert.Equal(true, result.DeleteMarker);
    }

    [Fact]
    public void TestHeadObjectRequest()
    {
        var request = new HeadObjectRequest();
        Assert.Empty(request.Headers);
        Assert.Empty(request.Parameters);
        Assert.Null(request.InnerBody);
        Assert.Equal("", request.BodyFormat);
        Assert.Null(request.Bucket);
        Assert.Null(request.Key);
        Assert.Null(request.IfMatch);
        Assert.Null(request.IfNoneMatch);
        Assert.Null(request.IfModifiedSince);
        Assert.Null(request.IfUnmodifiedSince);
        Assert.Null(request.VersionId);

        var input = new OperationInput();
        Serde.SerializeInput(request, ref input);

        Assert.Null(input.Headers);
        Assert.Null(input.Parameters);
        Assert.Null(input.Body);

        request = new HeadObjectRequest
        {
            Bucket = "bucket",
            Key = "key",
            IfMatch = "if-match",
            IfNoneMatch = "if-none-match",
            IfUnmodifiedSince = "if_unmodified_since",
            IfModifiedSince = "if_modified_since",
            VersionId = "version-id",
        };

        Assert.Equal("bucket", request.Bucket);
        Assert.Equal("key", request.Key);
        Assert.NotNull(request.Headers);
        Assert.Equal("if-match", request.Headers["If-Match"]);
        Assert.Equal("if-match", request.IfMatch);
        Assert.Equal("if-none-match", request.Headers["If-None-Match"]);
        Assert.Equal("if-none-match", request.IfNoneMatch);
        Assert.Equal("if_unmodified_since", request.Headers["If-Unmodified-Since"]);
        Assert.Equal("if_unmodified_since", request.IfUnmodifiedSince);
        Assert.Equal("if_modified_since", request.Headers["If-Modified-Since"]);
        Assert.Equal("if_modified_since", request.IfModifiedSince);

        Assert.NotNull(request.Parameters);
        Assert.Equal("version-id", request.Parameters["versionId"]);
        Assert.Equal("version-id", request.VersionId);

        Serde.SerializeInput(request, ref input);

        Assert.NotNull(input.Headers);
        Assert.Equal("if-match", input.Headers["If-Match"]);
        Assert.Equal("if-none-match", input.Headers["If-None-Match"]);
        Assert.Equal("if_unmodified_since", input.Headers["If-Unmodified-Since"]);
        Assert.Equal("if_modified_since", input.Headers["If-Modified-Since"]);

        Assert.NotNull(input.Parameters);
        Assert.Equal("version-id", input.Parameters["versionId"]);
        Assert.Null(input.Body);
    }

    [Fact]
    public void TestHeadObjectResult()
    {
        var result = new HeadObjectResult();
        Assert.Equal(0, result.StatusCode);
        Assert.Equal("", result.Status);
        Assert.Equal("", result.RequestId);
        Assert.Empty(result.Headers);
        Assert.Null(result.ContentLength);
        Assert.Null(result.ContentType);
        Assert.Null(result.ETag);
        Assert.Null(result.LastModified);
        Assert.Null(result.ContentMd5);
        Assert.NotNull(result.Metadata);
        Assert.Empty(result.Metadata);
        Assert.Null(result.CacheControl);
        Assert.Null(result.ContentDisposition);
        Assert.Null(result.ContentEncoding);
        Assert.Null(result.Expires);
        Assert.Null(result.HashCrc64);
        Assert.Null(result.StorageClass);
        Assert.Null(result.ObjectType);
        Assert.Null(result.VersionId);
        Assert.Null(result.TaggingCount);
        Assert.Null(result.ServerSideEncryption);
        Assert.Null(result.ServerSideDataEncryption);
        Assert.Null(result.ServerSideEncryptionKeyId);
        Assert.Null(result.NextAppendPosition);
        Assert.Null(result.Expiration);
        Assert.Null(result.Restore);
        Assert.Null(result.ProcessStatus);
        Assert.Null(result.DeleteMarker);
        Assert.Null(result.RequestCharged);
        Assert.Null(result.TransitionTime);

        var output = new OperationOutput
        {
            StatusCode = 200,
            Status = "OK",
            Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
                {"x-oss-request-id", "123-id"},
            }
        };
        ResultModel baseResult = result;
        Serde.DeserializeOutput(ref baseResult, ref output);

        Assert.Equal(200, result.StatusCode);
        Assert.Equal("OK", result.Status);
        Assert.Equal("123-id", result.RequestId);
        Assert.Single(result.Headers);
        Assert.Null(result.ContentLength);
        Assert.Null(result.ContentType);
        Assert.Null(result.ETag);
        Assert.Null(result.LastModified);
        Assert.Null(result.ContentMd5);
        Assert.NotNull(result.Metadata);
        Assert.Empty(result.Metadata);
        Assert.Null(result.CacheControl);
        Assert.Null(result.ContentDisposition);
        Assert.Null(result.ContentEncoding);
        Assert.Null(result.Expires);
        Assert.Null(result.HashCrc64);
        Assert.Null(result.StorageClass);
        Assert.Null(result.ObjectType);
        Assert.Null(result.VersionId);
        Assert.Null(result.TaggingCount);
        Assert.Null(result.ServerSideEncryption);
        Assert.Null(result.ServerSideDataEncryption);
        Assert.Null(result.ServerSideEncryptionKeyId);
        Assert.Null(result.NextAppendPosition);
        Assert.Null(result.Expiration);
        Assert.Null(result.Restore);
        Assert.Null(result.ProcessStatus);
        Assert.Null(result.DeleteMarker);
        Assert.Null(result.RequestCharged);
        Assert.Null(result.TransitionTime);

        //all header & body
        output = new OperationOutput
        {
            StatusCode = 200,
            Status = "OK",
            Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
                {"x-oss-request-id", "123-id"},
                {"Content-Length", "123456789"},
                {"Content-Range", "content_range 123"},
                {"Content-Type", "txt"},
                {"ETag", "etag 123"},
                {"Last-Modified", "Fri, 24 Feb 2012 06:07:48 GMT"},
                {"Content-MD5", "content_md5 123"},
                {"Cache-Control", "no-cache"},
                {"Content-Disposition", "1.jpg"},
                {"Content-Encoding", "gzip"},
                {"Expires", "expires 123"},
                {"x-oss-meta-key1", "value1"},
                {"x-oss-meta-Key2", "value2"},
                {"x-oss-hash-crc64ecma", "123456"},
                {"x-oss-storage-class", "IA"},
                {"x-oss-object-type", "Normal"},
                {"x-oss-version-id", "version-id-123"},
                {"x-oss-tagging-count", "100"},
                {"x-oss-server-side-encryption", "KMS"},
                {"x-oss-server-side-data-encryption", "SM4"},
                {"x-oss-server-side-encryption-key-id", "kms-id"},
                {"x-oss-next-append-position", "212"},
                {"x-oss-expiration", "expiration 123"},
                {"x-oss-restore", "restore 123"},
                {"x-oss-process-status", "process 123"},
                {"x-oss-delete-marker", "true"},
                {"x-oss-request-charged", "requester"},
                {"x-oss-transition-time", "transition"},
            }
        };
        result = new HeadObjectResult();
        baseResult = result;
        Serde.DeserializeOutput(ref baseResult, ref output);

        Assert.Equal(200, result.StatusCode);
        Assert.Equal("OK", result.Status);
        Assert.Equal("123-id", result.RequestId);
        Assert.NotNull(result.Headers);
        Assert.Equal(123456789, result.ContentLength);
        Assert.Equal("txt", result.ContentType);
        Assert.Equal("etag 123", result.ETag);
        Assert.Equal("Fri, 24 Feb 2012 06:07:48 GMT", result.LastModified);
        Assert.Equal("content_md5 123", result.ContentMd5);
        var metadata = result.Metadata;
        Assert.NotNull(metadata);
        Assert.Equal(2, metadata.Count);
        Assert.Equal("value1", metadata["key1"]);
        Assert.Equal("value2", metadata["key2"]);
        Assert.Equal("no-cache", result.CacheControl);
        Assert.Equal("1.jpg", result.ContentDisposition);
        Assert.Equal("gzip", result.ContentEncoding);
        Assert.Equal("expires 123", result.Expires);
        Assert.Equal("123456", result.HashCrc64);
        Assert.Equal("IA", result.StorageClass);
        Assert.Equal("Normal", result.ObjectType);
        Assert.Equal("version-id-123", result.VersionId);
        Assert.Equal(100, result.TaggingCount);
        Assert.Equal("KMS", result.ServerSideEncryption);
        Assert.Equal("SM4", result.ServerSideDataEncryption);
        Assert.Equal("kms-id", result.ServerSideEncryptionKeyId);
        Assert.Equal(212, result.NextAppendPosition);
        Assert.Equal("expiration 123", result.Expiration);
        Assert.Equal("restore 123", result.Restore);
        Assert.Equal("process 123", result.ProcessStatus);
        Assert.Equal(true, result.DeleteMarker);
        Assert.Equal("requester", result.RequestCharged);
        Assert.Equal("transition", result.TransitionTime);
    }

    [Fact]
    public void TestGetObjectMetaRequest()
    {
        var request = new GetObjectMetaRequest();
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

        request = new GetObjectMetaRequest
        {
            Bucket = "bucket",
            Key = "key",
            VersionId = "version-id",
        };

        Assert.Equal("bucket", request.Bucket);
        Assert.Equal("key", request.Key);
        Assert.NotNull(request.Headers);

        Assert.NotNull(request.Parameters);
        Assert.Equal("version-id", request.Parameters["versionId"]);
        Assert.Equal("version-id", request.VersionId);

        Serde.SerializeInput(request, ref input);

        Assert.Null(input.Headers);

        Assert.NotNull(input.Parameters);
        Assert.Equal("version-id", input.Parameters["versionId"]);
        Assert.Null(input.Body);
    }

    [Fact]
    public void TestGetObjectMetaResult()
    {
        var result = new GetObjectMetaResult();
        Assert.Equal(0, result.StatusCode);
        Assert.Equal("", result.Status);
        Assert.Equal("", result.RequestId);
        Assert.Empty(result.Headers);
        Assert.Null(result.ContentLength);
        Assert.Null(result.ETag);
        Assert.Null(result.LastModified);
        Assert.Null(result.VersionId);
        Assert.Null(result.TransitionTime);
        Assert.Null(result.LastAccessTime);
        Assert.Null(result.HashCrc64);

        var output = new OperationOutput
        {
            StatusCode = 200,
            Status = "OK",
            Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
                {"x-oss-request-id", "123-id"},
            }
        };
        ResultModel baseResult = result;
        Serde.DeserializeOutput(ref baseResult, ref output);

        Assert.Equal(200, result.StatusCode);
        Assert.Equal("OK", result.Status);
        Assert.Equal("123-id", result.RequestId);
        Assert.Single(result.Headers);
        Assert.Null(result.ContentLength);
        Assert.Null(result.ETag);
        Assert.Null(result.LastModified);
        Assert.Null(result.VersionId);
        Assert.Null(result.TransitionTime);
        Assert.Null(result.LastAccessTime);
        Assert.Null(result.HashCrc64);

        //all header
        output = new OperationOutput
        {
            StatusCode = 200,
            Status = "OK",
            Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
                {"x-oss-request-id", "123-id"},
                {"Content-Length", "123456789"},
                {"ETag", "etag 123"},
                {"Last-Modified", "Fri, 24 Feb 2012 06:07:48 GMT"},
                {"x-oss-version-id", "version-id-123"},
                {"x-oss-hash-crc64ecma", "123456"},
                {"x-oss-transition-time", "transition-time"},
                {"x-oss-last-access-time", "access-time"},
            }
        };
        result = new GetObjectMetaResult();
        baseResult = result;
        Serde.DeserializeOutput(ref baseResult, ref output);

        Assert.Equal(200, result.StatusCode);
        Assert.Equal("OK", result.Status);
        Assert.Equal("123-id", result.RequestId);
        Assert.NotNull(result.Headers);
        Assert.Equal(123456789, result.ContentLength);
        Assert.Equal("etag 123", result.ETag);
        Assert.Equal("Fri, 24 Feb 2012 06:07:48 GMT", result.LastModified);
        Assert.Equal("version-id-123", result.VersionId);
        Assert.Equal("transition-time", result.TransitionTime);
        Assert.Equal("access-time", result.LastAccessTime);
        Assert.Equal("123456", result.HashCrc64);
    }

    [Fact]
    public void TestRestoreObjectRequest()
    {
        var request = new RestoreObjectRequest();
        Assert.Empty(request.Headers);
        Assert.Empty(request.Parameters);
        Assert.Null(request.InnerBody);
        Assert.Equal("xml", request.BodyFormat);
        Assert.Null(request.Bucket);
        Assert.Null(request.Key);
        Assert.Null(request.VersionId);
        Assert.Null(request.RestoreRequest);

        var input = new OperationInput();
        Serde.SerializeInput(request, ref input);

        Assert.Null(input.Headers);
        Assert.Null(input.Parameters);
        Assert.Null(input.Body);

        request = new RestoreObjectRequest
        {
            Bucket = "bucket",
            Key = "key",
            VersionId = "version-id",
            RestoreRequest = new RestoreRequest()
            {
                Days = 2,
                JobParameters = new JobParameters()
                {
                    Tier = "Standard"
                }
            }
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

        Assert.NotNull(input.Body);
        using var reader = new StreamReader(input.Body);
        var xml = """
<?xml version="1.0" encoding="utf-8"?>
<RestoreRequest>
  <Days>2</Days>
  <JobParameters>
    <Tier>Standard</Tier>
  </JobParameters>
</RestoreRequest>
""";
        Assert.Equal(xml, reader.ReadToEnd());

        // only days
        request = new RestoreObjectRequest
        {
            Bucket = "bucket",
            Key = "key",
            VersionId = "version-id",
            RestoreRequest = new RestoreRequest()
            {
                Days = 2,
            }
        };
        input = new OperationInput();

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

        Assert.NotNull(input.Body);
        using var reader1 = new StreamReader(input.Body);
        xml = """
<?xml version="1.0" encoding="utf-8"?>
<RestoreRequest>
  <Days>2</Days>
</RestoreRequest>
""";
        Assert.Equal(xml, reader1.ReadToEnd());

        // no RestoreRequest
        request = new RestoreObjectRequest
        {
            Bucket = "bucket",
            Key = "key",
        };
        input = new OperationInput();

        Assert.Equal("bucket", request.Bucket);
        Assert.Equal("key", request.Key);
        Assert.Empty(request.Headers);
        Assert.Empty(request.Parameters);

        Serde.SerializeInput(request, ref input);

        Assert.Null(input.Headers);
        Assert.Null(input.Parameters);
        Assert.Null(input.Body);
    }

    [Fact]
    public void TestRestoreObjectResult()
    {
        var result = new RestoreObjectResult();
        Assert.Equal(0, result.StatusCode);
        Assert.Equal("", result.Status);
        Assert.Equal("", result.RequestId);
        Assert.Empty(result.Headers);
        Assert.Null(result.RestorePriority);
        Assert.Null(result.VersionId);

        var output = new OperationOutput
        {
            StatusCode = 200,
            Status = "OK",
            Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
                {"x-oss-request-id", "123-id"},
                {"x-oss-version-id", "version-id-123"},
                {"x-oss-object-restore-priority", "Standard"},
                {"Content-Type","txt"}
            }
        };
        ResultModel baseResult = result;
        Serde.DeserializeOutput(ref baseResult, ref output);

        Assert.Equal(200, result.StatusCode);
        Assert.Equal("OK", result.Status);
        Assert.Equal("123-id", result.RequestId);
        Assert.Equal(4, result.Headers.Count);
        Assert.Equal("txt", result.Headers["content-type"]);
        Assert.Equal("version-id-123", result.Headers["x-oss-version-id"]);
        Assert.Equal("version-id-123", result.VersionId);
        Assert.Equal("Standard", result.RestorePriority);
    }

    [Fact]
    public void TestCleanRestoredObjectRequest()
    {
        var request = new CleanRestoredObjectRequest();
        Assert.Empty(request.Headers);
        Assert.Empty(request.Parameters);
        Assert.Null(request.InnerBody);
        Assert.Equal("", request.BodyFormat);
        Assert.Null(request.Bucket);
        Assert.Null(request.Key);

        var input = new OperationInput();
        Serde.SerializeInput(request, ref input);

        Assert.Null(input.Headers);
        Assert.Null(input.Parameters);
        Assert.Null(input.Body);

        request = new CleanRestoredObjectRequest
        {
            Bucket = "bucket",
            Key = "key",
        };

        Assert.Equal("bucket", request.Bucket);
        Assert.Equal("key", request.Key);
        Assert.Empty(request.Headers);
        Assert.Empty(request.Parameters);

        Serde.SerializeInput(request, ref input);

        Assert.Null(input.Headers);
        Assert.Null(input.Body);
        Assert.Null(input.Parameters);
    }

    [Fact]
    public void TestCleanRestoredObjectResult()
    {
        var result = new CleanRestoredObjectResult();
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
    public void TestProcessObjectRequest()
    {
        var request = new ProcessObjectRequest();
        Assert.Empty(request.Headers);
        Assert.Empty(request.Parameters);
        Assert.Null(request.InnerBody);
        Assert.Equal("", request.BodyFormat);
        Assert.Null(request.Bucket);
        Assert.Null(request.Key);
        Assert.Null(request.Process);

        var input = new OperationInput();
        Serde.SerializeInput(request, ref input);

        Assert.Null(input.Headers);
        Assert.Null(input.Parameters);
        Assert.Null(input.Body);

        request = new ProcessObjectRequest
        {
            Bucket = "bucket",
            Key = "key",
            Process = "process image"
        };

        Assert.Equal("bucket", request.Bucket);
        Assert.Equal("key", request.Key);
        Assert.Empty(request.Headers);
        Assert.Empty(request.Parameters);

        Serde.SerializeInput(request, ref input, Serde.AddProcessAction);

        Assert.Null(input.Headers);
        Assert.Null(input.Parameters);
        Assert.NotNull(input.Body);
        using var reader = new StreamReader(input.Body);
        Assert.Equal("x-oss-process=process image", reader.ReadToEnd());
    }

    [Fact]
    public void TestProcessObjectResult()
    {
        var result = new ProcessObjectResult();
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
        Serde.DeserializeOutput(ref baseResult, ref output, Serde.DeserializerAnyBody);

        Assert.Equal(200, result.StatusCode);
        Assert.Equal("OK", result.Status);
        Assert.Equal("123-id", result.RequestId);
        Assert.Equal(2, result.Headers.Count);
        Assert.Equal("txt", result.Headers["content-type"]);
        Assert.Null(result.ProcessResult);

        output = new OperationOutput
        {
            StatusCode = 200,
            Status = "OK",
            Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
                {"x-oss-request-id", "123-id"},
                {"Content-Type","txt"}
            },
            Body = new MemoryStream(Encoding.UTF8.GetBytes("hello world"))
        };
        result = new ProcessObjectResult();
        baseResult = result;
        Serde.DeserializeOutput(ref baseResult, ref output, Serde.DeserializerAnyBody);

        Assert.Equal(200, result.StatusCode);
        Assert.Equal("OK", result.Status);
        Assert.Equal("123-id", result.RequestId);
        Assert.Equal(2, result.Headers.Count);
        Assert.Equal("txt", result.Headers["content-type"]);
        Assert.Equal("hello world", result.ProcessResult);
    }

    [Fact]
    public void TestAsyncProcessObjectRequest()
    {
        var request = new AsyncProcessObjectRequest();
        Assert.Empty(request.Headers);
        Assert.Empty(request.Parameters);
        Assert.Null(request.InnerBody);
        Assert.Equal("", request.BodyFormat);
        Assert.Null(request.Bucket);
        Assert.Null(request.Key);
        Assert.Null(request.Process);

        var input = new OperationInput();
        Serde.SerializeInput(request, ref input);

        Assert.Null(input.Headers);
        Assert.Null(input.Parameters);
        Assert.Null(input.Body);

        request = new AsyncProcessObjectRequest
        {
            Bucket = "bucket",
            Key = "key",
            Process = "process image"
        };

        Assert.Equal("bucket", request.Bucket);
        Assert.Equal("key", request.Key);
        Assert.Empty(request.Headers);
        Assert.Empty(request.Parameters);

        Serde.SerializeInput(request, ref input, Serde.AddProcessAction);

        Assert.Null(input.Headers);
        Assert.Null(input.Parameters);
        Assert.NotNull(input.Body);
        using var reader = new StreamReader(input.Body);
        Assert.Equal("x-oss-async-process=process image", reader.ReadToEnd());
    }

    [Fact]
    public void TestAsyncProcessObjectResult()
    {
        var result = new AsyncProcessObjectResult();
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
        Serde.DeserializeOutput(ref baseResult, ref output, Serde.DeserializerAnyBody);

        Assert.Equal(200, result.StatusCode);
        Assert.Equal("OK", result.Status);
        Assert.Equal("123-id", result.RequestId);
        Assert.Equal(2, result.Headers.Count);
        Assert.Equal("txt", result.Headers["content-type"]);
        Assert.Null(result.ProcessResult);

        output = new OperationOutput
        {
            StatusCode = 200,
            Status = "OK",
            Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
                {"x-oss-request-id", "123-id"},
                {"Content-Type","txt"}
            },
            Body = new MemoryStream(Encoding.UTF8.GetBytes("hello world"))
        };
        result = new AsyncProcessObjectResult();
        baseResult = result;
        Serde.DeserializeOutput(ref baseResult, ref output, Serde.DeserializerAnyBody);

        Assert.Equal(200, result.StatusCode);
        Assert.Equal("OK", result.Status);
        Assert.Equal("123-id", result.RequestId);
        Assert.Equal(2, result.Headers.Count);
        Assert.Equal("txt", result.Headers["content-type"]);
        Assert.Equal("hello world", result.ProcessResult);
    }

    [Fact]
    public void TestDeleteMultipleObjectsRequest()
    {
        var request = new DeleteMultipleObjectsRequest();
        Assert.Empty(request.Headers);
        Assert.Empty(request.Parameters);
        Assert.Null(request.InnerBody);
        Assert.Equal("", request.BodyFormat);
        Assert.Null(request.Bucket);
        Assert.Null(request.EncodingType);
        Assert.Null(request.Quiet);
        Assert.Null(request.Objects);

        var input = new OperationInput();
        Serde.SerializeInput(request, ref input);

        Assert.Null(input.Headers);
        Assert.Null(input.Parameters);
        Assert.Null(input.Body);

        // all
        request = new DeleteMultipleObjectsRequest
        {
            Bucket = "bucket",
            EncodingType = "url",
            Quiet = true,
            Objects = [
                new DeleteObject(){Key = "key-1", VersionId = "version-id-1"},
                new DeleteObject(){Key = "key-2", VersionId = "version-id-2"},
                ]
        };
        input = new OperationInput();

        Assert.Equal("bucket", request.Bucket);
        Assert.Empty(request.Headers);
        Assert.NotNull(request.Parameters);
        Assert.Equal("url", request.Parameters["encoding-type"]);
        Assert.Equal("url", request.EncodingType);
        Assert.Equal(true, request.Quiet);
        Assert.NotNull(request.Objects);
        Assert.Equal("key-1", request.Objects[0].Key);
        Assert.Equal("version-id-2", request.Objects[1].VersionId);

        Serde.SerializeInput(request, ref input, Serde.SerializeDeleteMultipleObjects);

        Assert.Null(input.Headers);
        Assert.NotNull(input.Parameters);
        Assert.Equal("url", input.Parameters["encoding-type"]);
        Assert.NotNull(input.Body);
        using var reader = new StreamReader(input.Body);

        var xml = """
<Delete><Quiet>true</Quiet><Object><Key>key-1</Key><VersionId>version-id-1</VersionId></Object><Object><Key>key-2</Key><VersionId>version-id-2</VersionId></Object></Delete>
""";
        Assert.Equal(xml, reader.ReadToEnd());

        // mixed version id
        request = new DeleteMultipleObjectsRequest
        {
            Bucket = "bucket",
            EncodingType = "url",
            Quiet = true,
            Objects = [
                new DeleteObject(){Key = "key-1"},
                new DeleteObject(){Key = "key-2", VersionId = "version-id-2"},
            ]
        };
        input = new OperationInput();

        Assert.Equal("bucket", request.Bucket);
        Assert.Empty(request.Headers);
        Assert.NotNull(request.Parameters);
        Assert.Equal("url", request.Parameters["encoding-type"]);
        Assert.Equal("url", request.EncodingType);
        Assert.Equal(true, request.Quiet);
        Assert.NotNull(request.Objects);
        Assert.Equal("key-1", request.Objects[0].Key);
        Assert.Equal("version-id-2", request.Objects[1].VersionId);

        Serde.SerializeInput(request, ref input, Serde.SerializeDeleteMultipleObjects);

        Assert.Null(input.Headers);
        Assert.NotNull(input.Parameters);
        Assert.Equal("url", input.Parameters["encoding-type"]);
        Assert.NotNull(input.Body);
        using var reader1 = new StreamReader(input.Body);

        xml = """
<Delete><Quiet>true</Quiet><Object><Key>key-1</Key></Object><Object><Key>key-2</Key><VersionId>version-id-2</VersionId></Object></Delete>
""";
        Assert.Equal(xml, reader1.ReadToEnd());

        // key with special char
        request = new DeleteMultipleObjectsRequest
        {
            Bucket = "bucket",
            EncodingType = "url",
            Objects = [
                new DeleteObject(){Key = "hello<>&\"'world"},
                new DeleteObject(){Key = System.Text.Encoding.UTF8.GetString([
                    0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f,
                    0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17, 0x18, 0x19, 0x1a, 0x1b, 0x1c, 0x1d, 0x1e, 0x1f,
                    0x20, 0x21, 0xe4, 0xbd, 0xa0, 0xe5, 0xa5, 0xbd])},
            ]
        };
        input = new OperationInput();

        Assert.Equal("bucket", request.Bucket);
        Assert.Empty(request.Headers);
        Assert.NotNull(request.Parameters);
        Assert.Equal("url", request.Parameters["encoding-type"]);
        Assert.Equal("url", request.EncodingType);
        Assert.Null(request.Quiet);
        Assert.NotNull(request.Objects);

        Serde.SerializeInput(request, ref input, Serde.SerializeDeleteMultipleObjects);

        Assert.Null(input.Headers);
        Assert.NotNull(input.Parameters);
        Assert.Equal("url", input.Parameters["encoding-type"]);
        Assert.NotNull(input.Body);
        using var reader2 = new StreamReader(input.Body);

        xml = """
              <Delete><Object><Key>hello&lt;&gt;&amp;&quot;&apos;world</Key></Object><Object><Key>&#00;&#01;&#02;&#03;&#04;&#05;&#06;&#07;&#08;&#09;&#10;&#11;&#12;&#13;&#14;&#15;&#16;&#17;&#18;&#19;&#20;&#21;&#22;&#23;&#24;&#25;&#26;&#27;&#28;&#29;&#30;&#31; !你好</Key></Object></Delete>
              """;
        Assert.Equal(xml, reader2.ReadToEnd());
    }

    [Fact]
    public void TestDeleteMultipleObjectsResult()
    {
        var result = new DeleteMultipleObjectsResult();
        Assert.Equal(0, result.StatusCode);
        Assert.Equal("", result.Status);
        Assert.Equal("", result.RequestId);
        Assert.Empty(result.Headers);
        Assert.Equal("", result.BodyFormat);
        Assert.Null(result.BodyType);
        Assert.Null(result.EncodingType);
        Assert.Null(result.DeletedObjects);

        //empty xml
        var xml = """
<?xml version="1.0" encoding="utf-8"?>
<DeleteResult>
</DeleteResult>
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
        result = new DeleteMultipleObjectsResult();
        ResultModel baseResult = result;
        Serde.DeserializeOutput(ref baseResult, ref output, Serde.DeserializeDeleteMultipleObjects);

        Assert.Equal(200, result.StatusCode);
        Assert.Equal("OK", result.Status);
        Assert.Equal("123-id", result.RequestId);
        Assert.Equal(2, result.Headers.Count);
        Assert.Equal("application/xml", result.Headers["content-type"]);
        Assert.Null(result.EncodingType);
        Assert.NotNull(result.DeletedObjects);
        Assert.Empty(result.DeletedObjects);

        //full xml without url encode
        xml = """
<?xml version="1.0" encoding="UTF-8"?>
<DeleteResult>
    <Deleted>
       <Key>multipart.data</Key>
       <DeleteMarker>true</DeleteMarker>
       <DeleteMarkerVersionId>CAEQMhiBgIDXiaaB0BYiIGQzYmRkZGUxMTM1ZDRjOTZhNjk4YjRjMTAyZjhl****</DeleteMarkerVersionId>
    </Deleted>
    <Deleted>
       <Key>test.jpg</Key>
       <VersionId>version-id-123</VersionId>
       <DeleteMarker>false</DeleteMarker>
    </Deleted>
</DeleteResult>
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
        result = new DeleteMultipleObjectsResult();
        baseResult = result;
        baseResult = result; Serde.DeserializeOutput(ref baseResult, ref output, Serde.DeserializeDeleteMultipleObjects);

        Assert.Equal(200, result.StatusCode);
        Assert.Equal("OK", result.Status);
        Assert.Equal("123-id", result.RequestId);
        Assert.Equal(2, result.Headers.Count);
        Assert.Equal("application/xml", result.Headers["content-type"]);
        Assert.Null(result.EncodingType);
        Assert.NotNull(result.DeletedObjects);
        Assert.Equal(2, result.DeletedObjects.Count);
        Assert.Equal("multipart.data", result.DeletedObjects[0].Key);
        Assert.Null(result.DeletedObjects[0].VersionId);
        Assert.Equal("CAEQMhiBgIDXiaaB0BYiIGQzYmRkZGUxMTM1ZDRjOTZhNjk4YjRjMTAyZjhl****", result.DeletedObjects[0].DeleteMarkerVersionId);
        Assert.Equal(true, result.DeletedObjects[0].DeleteMarker);
        Assert.Equal("test.jpg", result.DeletedObjects[1].Key);
        Assert.Equal("version-id-123", result.DeletedObjects[1].VersionId);
        Assert.Null(result.DeletedObjects[1].DeleteMarkerVersionId);
        Assert.Equal(false, result.DeletedObjects[1].DeleteMarker);

        //xml with url encode
        xml = """
<?xml version="1.0" encoding="UTF-8"?>
<DeleteResult>
    <EncodingType>url</EncodingType>
    <Deleted>
       <Key>folder%2Fmultipart.data</Key>
    </Deleted>
    <Deleted>
       <Key>folder%2Ftest.jpg</Key>
    </Deleted>
    <Deleted>
       <Key>demo.jpg</Key>
    </Deleted>
</DeleteResult>
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
        result = new DeleteMultipleObjectsResult();
        baseResult = result;
        baseResult = result; Serde.DeserializeOutput(ref baseResult, ref output, Serde.DeserializeDeleteMultipleObjects);

        Assert.Equal(200, result.StatusCode);
        Assert.Equal("OK", result.Status);
        Assert.Equal("123-id", result.RequestId);
        Assert.Equal(2, result.Headers.Count);
        Assert.Equal("application/xml", result.Headers["content-type"]);
        Assert.Equal("url", result.EncodingType);
        Assert.NotNull(result.DeletedObjects);
        Assert.Equal(3, result.DeletedObjects.Count);
        Assert.Equal("folder/multipart.data", result.DeletedObjects[0].Key);
        Assert.Null(result.DeletedObjects[0].VersionId);
        Assert.Null(result.DeletedObjects[0].DeleteMarkerVersionId);
        Assert.Null(result.DeletedObjects[0].DeleteMarker);

        Assert.Equal("folder/test.jpg", result.DeletedObjects[1].Key);
        Assert.Null(result.DeletedObjects[1].VersionId);
        Assert.Null(result.DeletedObjects[1].DeleteMarkerVersionId);
        Assert.Null(result.DeletedObjects[1].DeleteMarker);

        Assert.Equal("demo.jpg", result.DeletedObjects[2].Key);
        Assert.Null(result.DeletedObjects[2].VersionId);
        Assert.Null(result.DeletedObjects[2].DeleteMarkerVersionId);
        Assert.Null(result.DeletedObjects[2].DeleteMarker);
    }
}
