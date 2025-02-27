using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml.Serialization;
using AlibabaCloud.OSS.V2.Transform;

namespace AlibabaCloud.OSS.V2.Models {
    /// <summary>
    /// The request for the PutObject operation.
    /// </summary>
    public sealed class PutObjectRequest : RequestModel {
        /// <summary>
        /// The name of the bucket.
        /// </summary>
        public string? Bucket { get; set; }

        /// <summary>
        /// The full path of the object.
        /// </summary>
        public string? Key { get; set; }

        /// <summary>
        /// The access control list (ACL) of the object.
        /// Sees <see cref="ObjectAclType"/> for supported values.
        /// </summary>
        public string? Acl {
            get => Headers.TryGetValue("x-oss-object-acl", out var value) ? value : null;
            set {
                if (value != null) Headers["x-oss-object-acl"] = value;
            }
        }

        /// <summary>
        /// The storage class of the bucket. Default value: Standard.  Valid values:- Standard- IA- Archive- ColdArchive
        /// Sees <see cref="StorageClassType"/> for supported values.
        /// </summary>
        public string? StorageClass {
            get => Headers.TryGetValue("x-oss-storage-class", out var value) ? value : null;
            set {
                if (value != null) Headers["x-oss-storage-class"] = value;
            }
        }

        /// <summary>
        /// The caching behavior of the web page when the object is downloaded.
        /// </summary>
        public string? CacheControl {
            get => Headers.TryGetValue("Cache-Control", out var value) ? value : null;
            set {
                if (value != null) Headers["Cache-Control"] = value;
            }
        }

        /// <summary>
        /// The name of the object when the object is downloaded.
        /// </summary>
        public string? ContentDisposition {
            get => Headers.TryGetValue("Content-Disposition", out var value) ? value : null;
            set {
                if (value != null) Headers["Content-Disposition"] = value;
            }
        }

        /// <summary>
        /// The content encoding format of the object when the object is downloaded.
        /// </summary>
        public string? ContentEncoding {
            get => Headers.TryGetValue("Content-Encoding", out var value) ? value : null;
            set {
                if (value != null) Headers["Content-Encoding"] = value;
            }
        }

        /// <summary>
        /// The expiration time of the request.
        /// </summary>
        public string? Expires {
            get => Headers.TryGetValue("Expires", out var value) ? value : null;
            set {
                if (value != null) Headers["Expires"] = value;
            }
        }

        /// <summary>
        /// The MD5 hash of the object that you want to upload.
        /// </summary>
        public string? ContentMd5 {
            get => Headers.TryGetValue("Content-MD5", out var value) ? value : null;
            set {
                if (value != null) Headers["Content-MD5"] = value;
            }
        }

        /// <summary>
        /// A standard MIME type describing the format of the contents.
        /// </summary>
        public string? ContentType {
            get => Headers.TryGetValue("Content-Type", out var value) ? value : null;
            set {
                if (value != null) Headers["Content-Type"] = value;
            }
        }

        /// <summary>
        /// The size of the data in the HTTP message body. Unit: bytes.
        /// </summary>
        public long? ContentLength {
            get => Headers.TryGetValue("Content-Length", out var value)
                ? Convert.ToInt64(value, CultureInfo.InvariantCulture)
                : null;
            set {
                if (value != null) Headers["Content-Length"] = Convert.ToString(value, CultureInfo.InvariantCulture)!;
            }
        }

        /// <summary>
        /// The metadata of the object that you want to upload.
        /// </summary>
        public IDictionary<string, string>? Metadata { get; set; }

        /// <summary>
        /// The tag of the object. You can configure multiple tags for the object.
        /// Example: TagA=A&amp;TagB=B.  The key and value of a tag must be URL-encoded. If a tag does not contain an equal sign (=), the value of the tag is considered an empty string.
        /// </summary>
        public string? Tagging {
            get => Headers.TryGetValue("x-oss-tagging", out var value) ? value : null;
            set {
                if (value != null) Headers["x-oss-tagging"] = value;
            }
        }

        /// <summary>
        /// Specifies whether the object that is uploaded by calling the PutObject operation overwrites an existing object that has the same name.
        /// </summary>
        public bool? ForbidOverwrite {
            get => Headers.TryGetValue("x-oss-forbid-overwrite", out var value)
                ? Convert.ToBoolean(value, CultureInfo.InvariantCulture)
                : null;
            set {
                if (value != null)
                    Headers["x-oss-forbid-overwrite"] =
                        Convert.ToString(value, CultureInfo.InvariantCulture)!.ToLowerInvariant();
            }
        }

        /// <summary>
        /// The method that is used to encrypt the object on the OSS server when the object is created. Valid values: **AES256**, **KMS**, and **SM4****.If you specify the header, the header is returned in the response. OSS uses the method that is specified by this header to encrypt the uploaded object. When you download the encrypted object, the **x-oss-server-side-encryption** header is included in the response and the header value is set to the algorithm that is used to encrypt the object.
        /// </summary>
        public string? ServerSideEncryption {
            get => Headers.TryGetValue("x-oss-server-side-encryption", out var value) ? value : null;
            set {
                if (value != null) Headers["x-oss-server-side-encryption"] = value;
            }
        }

        /// <summary>
        /// The encryption method on the server side when an object is created. Valid values: **AES256**, **KMS**, and **SM4**.If you specify the header, the header is returned in the response. OSS uses the method that is specified by this header to encrypt the uploaded object. When you download the encrypted object, the **x-oss-server-side-encryption** header is included in the response and the header value is set to the algorithm that is used to encrypt the object.
        /// </summary>
        public string? ServerSideDataEncryption {
            get => Headers.TryGetValue("x-oss-server-side-data-encryption", out var value) ? value : null;
            set {
                if (value != null) Headers["x-oss-server-side-data-encryption"] = value;
            }
        }

        /// <summary>
        /// The ID of the customer master key (CMK) managed by Key Management Service (KMS). This header is valid only when the **x-oss-server-side-encryption** header is set to KMS.
        /// </summary>
        public string? ServerSideEncryptionKeyId {
            get => Headers.TryGetValue("x-oss-server-side-encryption-key-id", out var value) ? value : null;
            set {
                if (value != null) Headers["x-oss-server-side-encryption-key-id"] = value;
            }
        }

        /// <summary>
        /// A callback parameter is a Base64-encoded string that contains multiple fields in the JSON format.
        /// </summary>
        public string? Callback {
            get => Headers.TryGetValue("x-oss-callback", out var value) ? value : null;
            set {
                if (value != null) Headers["x-oss-callback"] = value;
            }
        }

        /// <summary>
        /// Configure custom parameters by using the callback-var parameter.
        /// </summary>
        public string? CallbackVar {
            get => Headers.TryGetValue("x-oss-callback-var", out var value) ? value : null;
            set {
                if (value != null) Headers["x-oss-callback-var"] = value;
            }
        }

        /// <summary>
        /// Specify the speed limit value. The speed limit value ranges from  245760 to 838860800, with a unit of bit/s.
        /// </summary>
        public long? TrafficLimit {
            get => Headers.TryGetValue("x-oss-traffic-limit", out var value)
                ? Convert.ToInt64(value, CultureInfo.InvariantCulture)
                : null;
            set {
                if (value != null) Headers["x-oss-traffic-limit"] = Convert.ToString(value, CultureInfo.InvariantCulture)!;
            }
        }

        /// <summary>
        /// Progress callback function
        /// </summary>
        public ProgressFunc? ProgressFn { get; set; }

        /// <summary>
        /// The body of the request.
        /// </summary>
        public Stream? Body {
            get => InnerBody as Stream;
            set => InnerBody = value;
        }
    }

    /// <summary>
    /// The result for the PutObject operation.
    /// </summary>
    public sealed class PutObjectResult : ResultModel {
        /// <summary>
        /// The 64-bit CRC value of the object. This value is calculated based on the ECMA-182 standard.
        /// </summary>
        public string? HashCrc64 => Headers.TryGetValue("x-oss-hash-crc64ecma", out var value) ? value : null;

        /// <summary>
        /// Version of the object.
        /// </summary>
        public string? VersionId => Headers.TryGetValue("x-oss-version-id", out var value) ? value : null;

        /// <summary>
        /// Content-Md5 for the uploaded object.
        /// </summary>
        public string? ContentMd5 => Headers.TryGetValue("Content-MD5", out var value) ? value : null;

        /// <summary>
        /// Entity tag for the uploaded object.
        /// </summary>
        public string? ETag => Headers.TryGetValue("ETag", out var value) ? value : null;

        /// <summary>
        /// Callback result in json format.
        /// It is valid only when the callback is set.
        /// </summary>
        public string? CallbackResult => InnerBody as string;
    }

    /// <summary>
    /// The request for the CopyObject operation.
    /// </summary>
    public sealed class CopyObjectRequest : RequestModel {
        /// <summary>
        /// The name of the bucket.
        /// </summary>
        public string? Bucket { get; set; }

        /// <summary>
        /// The full path of the object.
        /// </summary>
        public string? Key { get; set; }

        /// <summary>
        /// The name of the source bucket.
        /// </summary>
        public string? SourceBucket { get; set; }

        /// <summary>
        /// The name of the source object.
        /// </summary>
        public string? SourceKey { get; set; }

        /// <summary>
        /// The version ID of the source object.
        /// </summary>
        public string? SourceVersionId { get; set; }

        /// <summary>
        /// The object copy condition. If the ETag value of the source object is the same as the ETag value that you specify in the request, OSS copies the object and returns 200 OK. By default, this header is left empty.
        /// </summary>
        public string? IfMatch {
            get => Headers.TryGetValue("x-oss-copy-source-if-match", out var value) ? value : null;
            set {
                if (value != null) Headers["x-oss-copy-source-if-match"] = value;
            }
        }

        /// <summary>
        /// The object copy condition. If the ETag value of the source object is different from the ETag value that you specify in the request, OSS copies the object and returns 200 OK. By default, this header is left empty.
        /// </summary>
        public string? IfNoneMatch {
            get => Headers.TryGetValue("x-oss-copy-source-if-none-match", out var value) ? value : null;
            set {
                if (value != null) Headers["x-oss-copy-source-if-none-match"] = value;
            }
        }

        /// <summary>
        /// The object copy condition. If the time that you specify in the request is the same as or later than the modification time of the object, OSS copies the object and returns 200 OK. By default, this header is left empty.
        /// </summary>
        public string? IfUnmodifiedSince {
            get => Headers.TryGetValue("x-oss-copy-source-if-unmodified-since", out var value) ? value : null;
            set {
                if (value != null) Headers["x-oss-copy-source-if-unmodified-since"] = value;
            }
        }

        /// <summary>
        /// If the source object is modified after the time that you specify in the request, OSS copies the object. By default, this header is left empty.
        /// </summary>
        public string? IfModifiedSince {
            get => Headers.TryGetValue("x-oss-copy-source-if-modified-since", out var value) ? value : null;
            set {
                if (value != null) Headers["x-oss-copy-source-if-modified-since"] = value;
            }
        }

        /// <summary>
        /// The access control list (ACL) of the object.
        /// Sees <see cref="ObjectAclType"/> for supported values.
        /// </summary>
        public string? Acl {
            get => Headers.TryGetValue("x-oss-object-acl", out var value) ? value : null;
            set {
                if (value != null) Headers["x-oss-object-acl"] = value;
            }
        }

        /// <summary>
        /// The storage class of the object that you want to upload. Default value: Standard. If you specify a storage class when you upload the object, the storage class applies regardless of the storage class of the bucket to which you upload the object. For example, if you set **x-oss-storage-class** to Standard when you upload an object to an IA bucket, the storage class of the uploaded object is Standard.Valid values:*   Standard*   IA*   Archive*   ColdArchiveFor more information about storage classes, see [Overview](~~51374~~).
        /// Sees <see cref="StorageClassType"/> for supported values. 
        /// </summary>
        public string? StorageClass {
            get => Headers.TryGetValue("x-oss-storage-class", out var value) ? value : null;
            set {
                if (value != null) Headers["x-oss-storage-class"] = value;
            }
        }

        /// <summary>
        /// The caching behavior of the web page when the object is downloaded.
        /// </summary>
        public string? CacheControl {
            get => Headers.TryGetValue("Cache-Control", out var value) ? value : null;
            set {
                if (value != null) Headers["Cache-Control"] = value;
            }
        }

        /// <summary>
        /// The name of the object when the object is downloaded.
        /// </summary>
        public string? ContentDisposition {
            get => Headers.TryGetValue("Content-Disposition", out var value) ? value : null;
            set {
                if (value != null) Headers["Content-Disposition"] = value;
            }
        }

        /// <summary>
        /// The content encoding format of the object when the object is downloaded.
        /// </summary>
        public string? ContentEncoding {
            get => Headers.TryGetValue("Content-Encoding", out var value) ? value : null;
            set {
                if (value != null) Headers["Content-Encoding"] = value;
            }
        }

        /// <summary>
        /// The expiration time of the request.
        /// </summary>
        public string? Expires {
            get => Headers.TryGetValue("Expires", out var value) ? value : null;
            set {
                if (value != null) Headers["Expires"] = value;
            }
        }

        /// <summary>
        /// The MD5 hash of the object that you want to upload.
        /// </summary>
        public string? ContentMd5 {
            get => Headers.TryGetValue("Content-MD5", out var value) ? value : null;
            set {
                if (value != null) Headers["Content-MD5"] = value;
            }
        }

        /// <summary>
        /// A standard MIME type describing the format of the contents.
        /// </summary>
        public string? ContentType {
            get => Headers.TryGetValue("Content-Type", out var value) ? value : null;
            set {
                if (value != null) Headers["Content-Type"] = value;
            }
        }

        /// <summary>
        /// The size of the data in the HTTP message body. Unit: bytes.
        /// </summary>
        public long? ContentLength {
            get => Headers.TryGetValue("Content-Length", out var value)
                ? Convert.ToInt64(value, CultureInfo.InvariantCulture)
                : null;
            set {
                if (value != null) Headers["Content-Length"] = Convert.ToString(value, CultureInfo.InvariantCulture)!;
            }
        }

        /// <summary>
        /// The metadata of the object that you want to upload.
        /// </summary>
        public IDictionary<string, string>? Metadata { get; set; }

        /// <summary>
        /// The method that is used to configure the metadata of the destination object. Default value: COPY.*   **COPY**: The metadata of the source object is copied to the destination object. The **x-oss-server-side-encryption** attribute of the source object is not copied to the destination object. The **x-oss-server-side-encryption** header in the CopyObject request specifies the method that is used to encrypt the destination object.*   **REPLACE**: The metadata that you specify in the request is used as the metadata of the destination object.  If the path of the source object is the same as the path of the destination object and versioning is disabled for the bucket in which the source and destination objects are stored, the metadata that you specify in the CopyObject request is used as the metadata of the destination object regardless of the value of the x-oss-metadata-directive header.
        /// </summary>
        public string? MetadataDirective {
            get => Headers.TryGetValue("x-oss-metadata-directive", out var value) ? value : null;
            set {
                if (value != null) Headers["x-oss-metadata-directive"] = value;
            }
        }

        /// <summary>
        /// The entropy coding-based encryption algorithm that OSS uses to encrypt an object when you create the object. The valid values of the header are **AES256** and **KMS**. You must activate Key Management Service (KMS) in the OSS console before you can use the KMS encryption algorithm. Otherwise, the KmsServiceNotEnabled error is returned.*   If you do not specify the **x-oss-server-side-encryption** header in the CopyObject request, the destination object is not encrypted on the server regardless of whether the source object is encrypted on the server.*   If you specify the **x-oss-server-side-encryption** header in the CopyObject request, the destination object is encrypted on the server after the CopyObject operation is performed regardless of whether the source object is encrypted on the server. In addition, the response to a CopyObject request contains the **x-oss-server-side-encryption** header whose value is the encryption algorithm of the destination object. When the destination object is downloaded, the **x-oss-server-side-encryption** header is included in the response. The value of this header is the encryption algorithm of the destination object.
        /// </summary>
        public string? ServerSideEncryption {
            get => Headers.TryGetValue("x-oss-server-side-encryption", out var value) ? value : null;
            set {
                if (value != null) Headers["x-oss-server-side-encryption"] = value;
            }
        }

        /// <summary>
        /// The server side data encryption algorithm. Invalid value: SM4
        /// </summary>
        public string? ServerSideDataEncryption {
            get => Headers.TryGetValue("x-oss-server-side-data-encryption", out var value) ? value : null;
            set {
                if (value != null) Headers["x-oss-server-side-data-encryption"] = value;
            }
        }

        /// <summary>
        /// The ID of the customer master key (CMK) that is managed by KMS. This parameter is available only if you set **x-oss-server-side-encryption** to KMS.
        /// </summary>
        public string? ServerSideEncryptionKeyId {
            get => Headers.TryGetValue("x-oss-server-side-encryption-key-id", out var value) ? value : null;
            set {
                if (value != null) Headers["x-oss-server-side-encryption-key-id"] = value;
            }
        }

        /// <summary>
        /// The tag of the destination object. You can add multiple tags to the destination object. Example: TagA=A&amp;TagB=B.  The tag key and tag value must be URL-encoded. If a key-value pair does not contain an equal sign (=), the tag value is considered an empty string.
        /// </summary>
        public string? Tagging {
            get => Headers.TryGetValue("x-oss-tagging", out var value) ? value : null;
            set {
                if (value != null) Headers["x-oss-tagging"] = value;
            }
        }

        /// <summary>
        /// The method that is used to add tags to the destination object. Default value: Copy. Valid values:*   **Copy**: The tags of the source object are copied to the destination object.*   **Replace**: The tags that you specify in the request are added to the destination object.
        /// </summary>
        public string? TaggingDirective {
            get => Headers.TryGetValue("x-oss-tagging-directive", out var value) ? value : null;
            set {
                if (value != null) Headers["x-oss-tagging-directive"] = value;
            }
        }

        /// <summary>
        /// Specifies whether the object that is uploaded by calling the PutObject operation overwrites an existing object that has the same name.
        /// </summary>
        public bool? ForbidOverwrite {
            get => Headers.TryGetValue("x-oss-forbid-overwrite", out var value)
                ? Convert.ToBoolean(value, CultureInfo.InvariantCulture)
                : null;
            set {
                if (value != null)
                    Headers["x-oss-forbid-overwrite"] =
                        Convert.ToString(value, CultureInfo.InvariantCulture)!.ToLowerInvariant();
            }
        }

        /// <summary>
        /// Specify the speed limit value. The speed limit value ranges from  245760 to 838860800, with a unit of bit/s.
        /// </summary>
        public long? TrafficLimit {
            get => Headers.TryGetValue("x-oss-traffic-limit", out var value)
                ? Convert.ToInt64(value, CultureInfo.InvariantCulture)
                : null;
            set {
                if (value != null) Headers["x-oss-traffic-limit"] = Convert.ToString(value, CultureInfo.InvariantCulture)!;
            }
        }
    }

    /// <summary>
    /// The result for the CopyObject operation.
    /// </summary>
    public sealed class CopyObjectResult : ResultModel {
        /// <summary>
        /// The version ID of the source object.
        /// </summary>
        public string? SourceVersionId
            => Headers.TryGetValue("x-oss-copy-source-version-id", out var value) ? value : null;

        /// <summary>
        /// Version of the object.
        /// </summary>
        public string? VersionId => Headers.TryGetValue("x-oss-version-id", out var value) ? value : null;

        /// <summary>
        /// The 64-bit CRC value of the object. This value is calculated based on the ECMA-182 standard.
        /// </summary>
        public string? HashCrc64 => Headers.TryGetValue("x-oss-hash-crc64ecma", out var value) ? value : null;

        /// <summary>
        /// The encryption method on the server side when an object is created. Valid values: AES256 and KMS
        /// </summary>
        public string? ServerSideEncryption
            => Headers.TryGetValue("x-oss-server-side-encryption", out var value) ? value : null;

        /// <summary>
        /// The encryption algorithm of the object. AES256 or SM4.
        /// </summary>
        public string? ServerSideDataEncryption
            => Headers.TryGetValue("x-oss-server-side-data-encryption", out var value) ? value : null;

        /// <summary>
        /// The ID of the customer master key (CMK) that is managed by Key Management Service (KMS).
        /// </summary>
        public string? ServerSideEncryptionKeyId
            => Headers.TryGetValue("x-oss-server-side-encryption-key-id", out var value) ? value : null;

        /// <summary>
        /// The ETag value of the destination object.
        /// </summary>
        public string? ETag;

        /// <summary>
        /// The time when the destination object was last modified.
        /// </summary>
        public DateTime? LastModified;
    }

    /// <summary>
    /// The request for the GetObject operation.
    /// </summary>
    public sealed class GetObjectRequest : RequestModel {
        /// <summary>
        /// The name of the bucket.
        /// </summary>
        public string? Bucket { get; set; }

        /// <summary>
        /// The full path of the object.
        /// </summary>
        public string? Key { get; set; }

        /// <summary>
        /// The range of data of the object to be returned.   - If the value of Range is valid, OSS returns the response that includes the total size of the object and the range of data returned. For example, Content-Range: bytes 0~9/44 indicates that the total size of the object is 44 bytes, and the range of data returned is the first 10 bytes.   - However, if the value of Range is invalid, the entire object is returned, and the response returned by OSS excludes Content-Range. Default value: null
        /// </summary>
        public string? Range {
            get => Headers.TryGetValue("Range", out var value) ? value : null;
            set {
                if (value != null) Headers["Range"] = value;
            }
        }

        /// <summary>
        /// Specify standard behaviors to download data by range.
        /// If the value is "standard", the download behavior is modified when the specified range is not within the valid range.
        /// For an object whose size is 1,000 bytes:
        /// 1) If you set Range: bytes to 500-2000, the value at the end of the range is invalid. In this case, OSS returns HTTP status code 206 and the data that is within the range of byte 500 to byte 999.
        /// 2) If you set Range: bytes to 1000-2000, the value at the start of the range is invalid. In this case, OSS returns HTTP status code 416 and the InvalidRange error code.
        /// </summary>
        public string? RangeBehavior {
            get => Headers.TryGetValue("x-oss-range-behavior", out var value) ? value : null;
            set {
                if (value != null) Headers["x-oss-range-behavior"] = value;
            }
        }

        /// <summary>
        /// If the time specified in this header is earlier than the object modified time or is invalid, OSS returns the object and 200 OK. If the time specified in this header is later than or the same as the object modified time, OSS returns 304 Not Modified. The time must be in GMT. Example: `Fri, 13 Nov 2015 14:47:53 GMT`.Default value: null
        /// </summary>
        public string? IfModifiedSince {
            get => Headers.TryGetValue("If-Modified-Since", out var value) ? value : null;
            set {
                if (value != null) Headers["If-Modified-Since"] = value;
            }
        }

        /// <summary>
        /// If the time specified in this header is the same as or later than the object modified time, OSS returns the object and 200 OK. If the time specified in this header is earlier than the object modified time, OSS returns 412 Precondition Failed.                               The time must be in GMT. Example: `Fri, 13 Nov 2015 14:47:53 GMT`.You can specify both the **If-Modified-Since** and **If-Unmodified-Since** headers in a request. Default value: null
        /// </summary>
        public string? IfUnmodifiedSince {
            get => Headers.TryGetValue("If-Unmodified-Since", out var value) ? value : null;
            set {
                if (value != null) Headers["If-Unmodified-Since"] = value;
            }
        }

        /// <summary>
        /// If the ETag specified in the request matches the ETag value of the object, OSS transmits the object and returns 200 OK. If the ETag specified in the request does not match the ETag value of the object, OSS returns 412 Precondition Failed. The ETag value of an object is used to check whether the content of the object has changed. You can check data integrity by using the ETag value. Default value: null
        /// </summary>
        public string? IfMatch {
            get => Headers.TryGetValue("If-Match", out var value) ? value : null;
            set {
                if (value != null) Headers["If-Match"] = value;
            }
        }

        /// <summary>
        /// If the ETag specified in the request does not match the ETag value of the object, OSS transmits the object and returns 200 OK. If the ETag specified in the request matches the ETag value of the object, OSS returns 304 Not Modified. You can specify both the **If-Match** and **If-None-Match** headers in a request. Default value: null
        /// </summary>
        public string? IfNoneMatch {
            get => Headers.TryGetValue("If-None-Match", out var value) ? value : null;
            set {
                if (value != null) Headers["If-None-Match"] = value;
            }
        }

        /// <summary>
        /// The content-type header in the response that OSS returns.
        /// </summary>
        public string? ResponseContentType {
            get => Parameters.TryGetValue("response-content-type", out var value) ? value : null;
            set {
                if (value != null) Parameters["response-content-type"] = value;
            }
        }

        /// <summary>
        /// The content-language header in the response that OSS returns.
        /// </summary>
        public string? ResponseContentLanguage {
            get => Parameters.TryGetValue("response-content-language", out var value) ? value : null;
            set {
                if (value != null) Parameters["response-content-language"] = value;
            }
        }

        /// <summary>
        /// The expires header in the response that OSS returns.
        /// </summary>
        public string? ResponseExpires {
            get => Parameters.TryGetValue("response-expires", out var value) ? value : null;
            set {
                if (value != null) Parameters["response-expires"] = value;
            }
        }

        /// <summary>
        /// The cache-control header in the response that OSS returns.
        /// </summary>
        public string? ResponseCacheControl {
            get => Parameters.TryGetValue("response-cache-control", out var value) ? value : null;
            set {
                if (value != null) Parameters["response-cache-control"] = value;
            }
        }

        /// <summary>
        /// The content-disposition header in the response that OSS returns.
        /// </summary>
        public string? ResponseContentDisposition {
            get => Parameters.TryGetValue("response-content-disposition", out var value) ? value : null;
            set {
                if (value != null) Parameters["response-content-disposition"] = value;
            }
        }

        /// <summary>
        /// The content-encoding header in the response that OSS returns.
        /// </summary>
        public string? ResponseContentEncoding {
            get => Parameters.TryGetValue("response-content-encoding", out var value) ? value : null;
            set {
                if (value != null) Parameters["response-content-encoding"] = value;
            }
        }

        /// <summary>
        /// The version ID of the object that you want to query.
        /// </summary>
        public string? VersionId {
            get => Parameters.TryGetValue("versionId", out var value) ? value : null;
            set {
                if (value != null) Parameters["versionId"] = value;
            }
        }

        /// <summary>
        /// Specify the speed limit value. The speed limit value ranges from  245760 to 838860800, with a unit of bit/s.
        /// </summary>
        public long? TrafficLimit {
            get => Headers.TryGetValue("x-oss-traffic-limit", out var value)
                ? Convert.ToInt64(value, CultureInfo.InvariantCulture)
                : null;
            set {
                if (value != null) Headers["x-oss-traffic-limit"] = Convert.ToString(value, CultureInfo.InvariantCulture)!;
            }
        }

        /// <summary>
        /// Image processing parameters.
        /// </summary>
        public string? Process {
            get => Parameters.TryGetValue("x-oss-process", out var value) ? value : null;
            set {
                if (value != null) Parameters["x-oss-process"] = value;
            }
        }

        /// <summary>
        /// Progress callback function
        /// </summary>
        public ProgressFunc? ProgressFn { get; set; }
    }

    /// <summary>
    /// The result for the GetObject operation.
    /// </summary>
    public sealed class GetObjectResult : ResultModel {
        /// <summary>
        /// Size of the body in bytes.
        /// </summary>
        public long? ContentLength => Headers.TryGetValue("Content-Length", out var value)
            ? Convert.ToInt64(value, CultureInfo.InvariantCulture)
            : null;

        /// <summary>
        /// The portion of the object returned in the response.
        /// </summary>
        public string? ContentRange => Headers.TryGetValue("Content-Range", out var value) ? value : null;

        /// <summary>
        /// A standard MIME type describing the format of the object data.
        /// </summary>
        public string? ContentType => Headers.TryGetValue("Content-Type", out var value) ? value : null;

        /// <summary>
        /// The entity tag (ETag).
        /// An ETag is created when an object is created to identify the content of the object.
        /// </summary>
        public string? ETag => Headers.TryGetValue("ETag", out var value) ? value : null;

        /// <summary>
        /// The time when the returned objects were last modified.
        /// </summary>
        public string? LastModified => Headers.TryGetValue("Last-Modified", out var value) ? value : null;

        /// <summary>
        /// the Md5 hash for the uploaded object.
        /// </summary>
        public string? ContentMd5 => Headers.TryGetValue("Content-Md5", out var value) ? value : null;

        /// <summary>
        /// A map of metadata to store with the object.
        /// It is a case-insensitive dictionary
        /// </summary>
        public IDictionary<string, string>? Metadata => Serde.ToUserMetadata(Headers);

        /// <summary>
        /// The caching behavior of the web page when the object is downloaded.
        /// </summary>
        public string? CacheControl => Headers.TryGetValue("Cache-Control", out var value) ? value : null;

        /// <summary>
        /// The method that is used to access the object.
        /// </summary>
        public string? ContentDisposition => Headers.TryGetValue("Content-Disposition", out var value) ? value : null;

        /// <summary>
        /// The method that is used to encode the object.
        /// </summary>
        public string? ContentEncoding => Headers.TryGetValue("Content-Encoding", out var value) ? value : null;

        /// <summary>
        /// The expiration time of the cache in UTC.
        /// </summary>
        public string? Expires => Headers.TryGetValue("Expires", out var value) ? value : null;

        /// <summary>
        /// The 64-bit CRC value of the object.
        /// This value is calculated based on the ECMA-182 standard.
        /// </summary>
        public string? HashCrc64 => Headers.TryGetValue("x-oss-hash-crc64ecma", out var value) ? value : null;

        /// <summary>
        /// The storage class of the object.
        /// </summary>
        public string? StorageClass => Headers.TryGetValue("x-oss-storage-class", out var value) ? value : null;

        /// <summary>
        /// The type of the object.
        /// </summary>
        public string? ObjectType => Headers.TryGetValue("x-oss-object-type", out var value) ? value : null;

        /// <summary>
        ///  Version of the object.
        /// </summary>
        public string? VersionId => Headers.TryGetValue("x-oss-version-id", out var value) ? value : null;

        /// <summary>
        /// The number of tags added to the object.
        /// This header is included in the response only when you have read permissions on tags.
        /// </summary>
        public long? TaggingCount => Headers.TryGetValue("x-oss-tagging-count", out var value)
            ? Convert.ToInt64(value, CultureInfo.InvariantCulture)
            : null;

        /// <summary>
        /// If the requested object is encrypted by
        /// using a server-side encryption algorithm based on entropy encoding, OSS automatically decrypts
        /// the object and returns the decrypted object after OSS receives the GetObject request.
        /// The x-oss-server-side-encryption header is included in the response to indicate the encryption algorithm used to encrypt the object on the server.
        /// </summary>
        public string? ServerSideEncryption
            => Headers.TryGetValue("x-oss-server-side-encryption", out var value) ? value : null;

        /// <summary>
        /// The server side data encryption algorithm.
        /// </summary>
        public string? ServerSideDataEncryption
            => Headers.TryGetValue("x-oss-server-side-data-encryption", out var value) ? value : null;

        /// <summary>
        /// The ID of the customer master key (CMK) that is managed by Key Management Service (KMS).
        /// </summary>
        public string? ServerSideEncryptionKeyId
            => Headers.TryGetValue("x-oss-server-side-encryption-key-id", out var value) ? value : null;

        /// <summary>
        /// The position for the next append operation.
        /// If the type of the object is Appendable, this header is included in the response.
        /// </summary>
        public long? NextAppendPosition => Headers.TryGetValue("x-oss-next-append-position", out var value)
            ? Convert.ToInt64(value, CultureInfo.InvariantCulture)
            : null;

        /// <summary>
        /// The lifecycle information about the object.
        /// If lifecycle rules are configured for the object, this header is included in the response.
        /// This header contains the following parameters: expiry-date that indicates the expiration time of the object, and rule-id that indicates the ID of the matched lifecycle rule.
        /// </summary>
        public string? Expiration => Headers.TryGetValue("x-oss-expiration", out var value) ? value : null;

        /// <summary>
        /// The status of the object when you restore an object.
        /// If the storage class of the bucket is Archive and a RestoreObject request is submitted.
        /// </summary>
        public string? Restore => Headers.TryGetValue("x-oss-restore", out var value) ? value : null;

        /// <summary>
        /// The result of an event notification that is triggered for the object.
        /// </summary>
        public string? ProcessStatus => Headers.TryGetValue("x-oss-process-status", out var value) ? value : null;

        /// <summary>
        /// Specifies whether the object retrieved was (true) or was not (false) a Delete  Marker.
        /// </summary>
        public bool? DeleteMarker
            => Headers.TryGetValue("x-oss-delete-marker", out var value) ? Convert.ToBoolean(value) : null;

        /// <summary>
        /// Object data.
        /// </summary>
        public Stream? Body => InnerBody as Stream;

        public GetObjectResult() {
            BodyFormat = "stream";
        }
    }

    /// <summary>
    /// The request for the AppendObject operation.
    /// </summary>
    public sealed class AppendObjectRequest : RequestModel {
        /// <summary>
        /// The name of the bucket.
        /// </summary>
        public string? Bucket { get; set; }

        /// <summary>
        /// The full path of the object.
        /// </summary>
        public string? Key { get; set; }

        /// <summary>
        /// The position from which the AppendObject operation starts.  Each time an AppendObject operation succeeds, the x-oss-next-append-position header is included in the response to specify the position from which the next AppendObject operation starts. The value of position in the first AppendObject operation performed on an object must be 0. The value of position in subsequent AppendObject operations performed on the object is the current length of the object. For example, if the value of position specified in the first AppendObject request is 0 and the value of content-length is 65536, the value of position in the second AppendObject request must be 65536. - If the value of position in the AppendObject request is 0 and the name of the object that you want to append is unique, you can set headers such as x-oss-server-side-encryption in an AppendObject request in the same way as you set in a PutObject request. If you add the x-oss-server-side-encryption header to an AppendObject request, the x-oss-server-side-encryption header is included in the response to the request. If you want to modify metadata, you can call the CopyObject operation. - If you call an AppendObject operation to append a 0 KB object whose position value is valid to an Appendable object, the status of the Appendable object is not changed.
        /// </summary>
        public long? Position {
            get => Parameters.TryGetValue("position", out var value)
                ? Convert.ToInt64(value, CultureInfo.InvariantCulture)
                : null;
            set {
                if (value != null) Parameters["position"] = Convert.ToString((long)value, CultureInfo.InvariantCulture);
            }
        }

        /// <summary>
        /// Specifies whether the object that is uploaded by calling the PutObject operation overwrites an existing object that has the same name.
        /// </summary>
        public bool? ForbidOverwrite {
            get => Headers.TryGetValue("x-oss-forbid-overwrite", out var value)
                ? Convert.ToBoolean(value, CultureInfo.InvariantCulture)
                : null;
            set {
                if (value != null)
                    Headers["x-oss-forbid-overwrite"] =
                        Convert.ToString(value, CultureInfo.InvariantCulture)!.ToLowerInvariant();
            }
        }

        /// <summary>
        /// The method that is used to encrypt the object on the OSS server when the object is created. Valid values: **AES256**, **KMS**, and **SM4****.If you specify the header, the header is returned in the response. OSS uses the method that is specified by this header to encrypt the uploaded object. When you download the encrypted object, the **x-oss-server-side-encryption** header is included in the response and the header value is set to the algorithm that is used to encrypt the object.
        /// </summary>
        public string? ServerSideEncryption {
            get => Headers.TryGetValue("x-oss-server-side-encryption", out var value) ? value : null;
            set {
                if (value != null) Headers["x-oss-server-side-encryption"] = value;
            }
        }

        /// <summary>
        /// The encryption method on the server side when an object is created. Valid values: **AES256**, **KMS**, and **SM4**.If you specify the header, the header is returned in the response. OSS uses the method that is specified by this header to encrypt the uploaded object. When you download the encrypted object, the **x-oss-server-side-encryption** header is included in the response and the header value is set to the algorithm that is used to encrypt the object.
        /// </summary>
        public string? ServerSideDataEncryption {
            get => Headers.TryGetValue("x-oss-server-side-data-encryption", out var value) ? value : null;
            set {
                if (value != null) Headers["x-oss-server-side-data-encryption"] = value;
            }
        }

        /// <summary>
        /// The ID of the customer master key (CMK) managed by Key Management Service (KMS). This header is valid only when the **x-oss-server-side-encryption** header is set to KMS.
        /// </summary>
        public string? ServerSideEncryptionKeyId {
            get => Headers.TryGetValue("x-oss-server-side-encryption-key-id", out var value) ? value : null;
            set {
                if (value != null) Headers["x-oss-server-side-encryption-key-id"] = value;
            }
        }

        /// <summary>
        /// The access control list (ACL) of the object.
        /// Sees <see cref="ObjectAclType"/> for supported values.
        /// </summary>
        public string? Acl {
            get => Headers.TryGetValue("x-oss-object-acl", out var value) ? value : null;
            set {
                if (value != null) Headers["x-oss-object-acl"] = value;
            }
        }

        /// <summary>
        /// The storage class of the bucket. Default value: Standard.  Valid values:- Standard- IA- Archive- ColdArchive
        /// Sees <see cref="StorageClassType"/> for supported values.
        /// </summary>
        public string? StorageClass {
            get => Headers.TryGetValue("x-oss-storage-class", out var value) ? value : null;
            set {
                if (value != null) Headers["x-oss-storage-class"] = value;
            }
        }

        /// <summary>
        /// The caching behavior of the web page when the object is downloaded.
        /// </summary>
        public string? CacheControl {
            get => Headers.TryGetValue("Cache-Control", out var value) ? value : null;
            set {
                if (value != null) Headers["Cache-Control"] = value;
            }
        }

        /// <summary>
        /// The name of the object when the object is downloaded.
        /// </summary>
        public string? ContentDisposition {
            get => Headers.TryGetValue("Content-Disposition", out var value) ? value : null;
            set {
                if (value != null) Headers["Content-Disposition"] = value;
            }
        }

        /// <summary>
        /// The content encoding format of the object when the object is downloaded.
        /// </summary>
        public string? ContentEncoding {
            get => Headers.TryGetValue("Content-Encoding", out var value) ? value : null;
            set {
                if (value != null) Headers["Content-Encoding"] = value;
            }
        }

        /// <summary>
        /// The expiration time of the request.
        /// </summary>
        public string? Expires {
            get => Headers.TryGetValue("Expires", out var value) ? value : null;
            set {
                if (value != null) Headers["Expires"] = value;
            }
        }

        /// <summary>
        /// The MD5 hash of the object that you want to upload.
        /// </summary>
        public string? ContentMd5 {
            get => Headers.TryGetValue("Content-MD5", out var value) ? value : null;
            set {
                if (value != null) Headers["Content-MD5"] = value;
            }
        }

        /// <summary>
        /// A standard MIME type describing the format of the contents.
        /// </summary>
        public string? ContentType {
            get => Headers.TryGetValue("Content-Type", out var value) ? value : null;
            set {
                if (value != null) Headers["Content-Type"] = value;
            }
        }

        /// <summary>
        /// The size of the data in the HTTP message body. Unit: bytes.
        /// </summary>
        public long? ContentLength {
            get => Headers.TryGetValue("Content-Length", out var value)
                ? Convert.ToInt64(value, CultureInfo.InvariantCulture)
                : null;
            set {
                if (value != null) Headers["Content-Length"] = Convert.ToString(value, CultureInfo.InvariantCulture)!;
            }
        }

        /// <summary>
        /// The metadata of the object that you want to upload.
        /// </summary>
        public IDictionary<string, string>? Metadata { get; set; }

        /// <summary>
        /// The tag of the object. You can configure multiple tags for the object.
        /// Example: TagA=A&amp;TagB=B.  The key and value of a tag must be URL-encoded. If a tag does not contain an equal sign (=), the value of the tag is considered an empty string.
        /// </summary>
        public string? Tagging {
            get => Headers.TryGetValue("x-oss-tagging", out var value) ? value : null;
            set {
                if (value != null) Headers["x-oss-tagging"] = value;
            }
        }

        /// <summary>
        /// Specify the speed limit value. The speed limit value ranges from  245760 to 838860800, with a unit of bit/s.
        /// </summary>
        public long? TrafficLimit {
            get => Headers.TryGetValue("x-oss-traffic-limit", out var value)
                ? Convert.ToInt64(value, CultureInfo.InvariantCulture)
                : null;
            set {
                if (value != null) Headers["x-oss-traffic-limit"] = Convert.ToString(value, CultureInfo.InvariantCulture)!;
            }
        }

        /// <summary>
        /// Specify the initial value of CRC64. If not set, the crc check is ignored.
        /// </summary>
        public string? InitHashCrc64 { get; set; }

        /// <summary>
        /// Progress callback function
        /// </summary>
        public ProgressFunc? ProgressFn { get; set; }

        /// <summary>
        /// Object data.
        /// </summary>
        public Stream? Body {
            get => InnerBody as Stream;
            set => InnerBody = value;
        }
    }

    /// <summary>
    /// The result for the AppendObject operation.
    /// </summary>
    public sealed class AppendObjectResult : ResultModel {
        /// <summary>
        /// The position that must be provided in the next request, which is the current length of the object.
        /// </summary>
        public long? NextAppendPosition => Headers.TryGetValue("x-oss-next-append-position", out var value)
            ? Convert.ToInt64(value, CultureInfo.InvariantCulture)
            : null;

        /// <summary>
        /// Version of the object.
        /// </summary>
        public string? VersionId => Headers.TryGetValue("x-oss-version-id", out var value) ? value : null;

        /// <summary>
        /// The 64-bit CRC value of the object. This value is calculated based on the ECMA-182 standard.
        /// </summary>
        public string? HashCrc64 => Headers.TryGetValue("x-oss-hash-crc64ecma", out var value) ? value : null;

        /// <summary>
        /// The encryption method on the server side when an object is created. Valid values: AES256 and KMS
        /// </summary>
        public string? ServerSideEncryption
            => Headers.TryGetValue("x-oss-server-side-encryption", out var value) ? value : null;

        /// <summary>
        /// The encryption algorithm of the object. AES256 or SM4.
        /// </summary>
        public string? ServerSideDataEncryption
            => Headers.TryGetValue("x-oss-server-side-data-encryption", out var value) ? value : null;

        /// <summary>
        /// The ID of the customer master key (CMK) that is managed by Key Management Service (KMS).
        /// </summary>
        public string? ServerSideEncryptionKeyId
            => Headers.TryGetValue("x-oss-server-side-encryption-key-id", out var value) ? value : null;
    }

    /// <summary>
    /// The request for the DeleteObject operation.
    /// </summary>
    public sealed class DeleteObjectRequest : RequestModel {
        /// <summary>
        /// The information about the bucket.
        /// </summary>
        public string? Bucket { get; set; }

        /// <summary>
        /// The full path of the object.
        /// </summary>
        public string? Key { get; set; }

        /// <summary>
        /// The version ID of the object.
        /// </summary>
        public string? VersionId {
            get => Parameters.TryGetValue("versionId", out var value) ? value : null;
            set {
                if (value != null) Parameters["versionId"] = value;
            }
        }
    }

    /// <summary>
    /// The result for the DeleteObject operation.
    /// </summary>
    public sealed class DeleteObjectResult : ResultModel {
        /// <summary>
        /// Indicates whether the deleted version is a delete marker.
        /// </summary>
        public bool? DeleteMarker
            => Headers.TryGetValue("x-oss-delete-marker", out var value) ? Convert.ToBoolean(value) : null;

        /// <summary>
        /// Version of the object.
        /// </summary>
        public string? VersionId => Headers.TryGetValue("x-oss-version-id", out var value) ? value : null;
    }

    /// <summary>
    /// The request for the HeadObject operation.
    /// </summary>
    public sealed class HeadObjectRequest : RequestModel {
        /// <summary>
        /// The name of the bucket.
        /// </summary>
        public string? Bucket { get; set; }

        /// <summary>
        /// The full path of the object.
        /// </summary>
        public string? Key { get; set; }

        /// <summary>
        /// If the time that is specified in the request is earlier than the time when the object is modified, OSS returns 200 OK and the metadata of the object. Otherwise, OSS returns 304 not modified. Default value: null.
        /// </summary>
        public string? IfModifiedSince {
            get => Headers.TryGetValue("If-Modified-Since", out var value) ? value : null;
            set {
                if (value != null) Headers["If-Modified-Since"] = value;
            }
        }

        /// <summary>
        /// If the time that is specified in the request is later than or the same as the time when the object is modified, OSS returns 200 OK and the metadata of the object. Otherwise, OSS returns 412 precondition failed. Default value: null.
        /// </summary>
        public string? IfUnmodifiedSince {
            get => Headers.TryGetValue("If-Unmodified-Since", out var value) ? value : null;
            set {
                if (value != null) Headers["If-Unmodified-Since"] = value;
            }
        }

        /// <summary>
        /// If the ETag value that is specified in the request matches the ETag value of the object, OSS returns 200 OK and the metadata of the object. Otherwise, OSS returns 412 precondition failed. Default value: null.
        /// </summary>
        public string? IfMatch {
            get => Headers.TryGetValue("If-Match", out var value) ? value : null;
            set {
                if (value != null) Headers["If-Match"] = value;
            }
        }

        /// <summary>
        /// If the ETag value that is specified in the request does not match the ETag value of the object, OSS returns 200 OK and the metadata of the object. Otherwise, OSS returns 304 Not Modified. Default value: null.
        /// </summary>
        public string? IfNoneMatch {
            get => Headers.TryGetValue("If-None-Match", out var value) ? value : null;
            set {
                if (value != null) Headers["If-None-Match"] = value;
            }
        }

        /// <summary>
        /// The version ID of the object.
        /// </summary>
        public string? VersionId {
            get => Parameters.TryGetValue("versionId", out var value) ? value : null;
            set {
                if (value != null) Parameters["versionId"] = value;
            }
        }
    }

    /// <summary>
    /// The result for the HeadObject operation.
    /// </summary>
    public sealed class HeadObjectResult : ResultModel {
        /// <summary>
        /// Size of the body in bytes.
        /// </summary>
        public long? ContentLength => Headers.TryGetValue("Content-Length", out var value)
            ? Convert.ToInt64(value, CultureInfo.InvariantCulture)
            : null;

        /// <summary>
        /// A standard MIME type describing the format of the object data.
        /// </summary>
        public string? ContentType => Headers.TryGetValue("Content-Type", out var value) ? value : null;

        /// <summary>
        /// The entity tag (ETag).
        /// An ETag is created when an object is created to identify the content of the object.
        /// </summary>
        public string? ETag => Headers.TryGetValue("ETag", out var value) ? value : null;

        /// <summary>
        /// The time when the returned objects were last modified.
        /// </summary>
        public string? LastModified => Headers.TryGetValue("Last-Modified", out var value) ? value : null;

        /// <summary>
        /// the Md5 hash for the uploaded object.
        /// </summary>
        public string? ContentMd5 => Headers.TryGetValue("Content-Md5", out var value) ? value : null;

        /// <summary>
        /// A map of metadata to store with the object.
        /// It is a case-insensitive dictionary
        /// </summary>
        public IDictionary<string, string>? Metadata => Serde.ToUserMetadata(Headers);

        /// <summary>
        /// The caching behavior of the web page when the object is downloaded.
        /// </summary>
        public string? CacheControl => Headers.TryGetValue("Cache-Control", out var value) ? value : null;

        /// <summary>
        /// The method that is used to access the object.
        /// </summary>
        public string? ContentDisposition => Headers.TryGetValue("Content-Disposition", out var value) ? value : null;

        /// <summary>
        /// The method that is used to encode the object.
        /// </summary>
        public string? ContentEncoding => Headers.TryGetValue("Content-Encoding", out var value) ? value : null;

        /// <summary>
        /// The expiration time of the cache in UTC.
        /// </summary>
        public string? Expires => Headers.TryGetValue("Expires", out var value) ? value : null;

        /// <summary>
        /// The 64-bit CRC value of the object.
        /// This value is calculated based on the ECMA-182 standard.
        /// </summary>
        public string? HashCrc64 => Headers.TryGetValue("x-oss-hash-crc64ecma", out var value) ? value : null;

        /// <summary>
        /// The storage class of the object.
        /// </summary>
        public string? StorageClass => Headers.TryGetValue("x-oss-storage-class", out var value) ? value : null;

        /// <summary>
        /// The type of the object.
        /// </summary>
        public string? ObjectType => Headers.TryGetValue("x-oss-object-type", out var value) ? value : null;

        /// <summary>
        ///  Version of the object.
        /// </summary>
        public string? VersionId => Headers.TryGetValue("x-oss-version-id", out var value) ? value : null;

        /// <summary>
        /// The number of tags added to the object.
        /// This header is included in the response only when you have read permissions on tags.
        /// </summary>
        public long? TaggingCount => Headers.TryGetValue("x-oss-tagging-count", out var value)
            ? Convert.ToInt64(
                value,
                CultureInfo.InvariantCulture
            )
            : null;

        /// <summary>
        /// If the requested object is encrypted by
        /// using a server-side encryption algorithm based on entropy encoding, OSS automatically decrypts
        /// the object and returns the decrypted object after OSS receives the GetObject request.
        /// The x-oss-server-side-encryption header is included in the response to indicate the encryption algorithm used to encrypt the object on the server.
        /// </summary>
        public string? ServerSideEncryption
            => Headers.TryGetValue("x-oss-server-side-encryption", out var value) ? value : null;

        /// <summary>
        /// The server side data encryption algorithm.
        /// </summary>
        public string? ServerSideDataEncryption
            => Headers.TryGetValue("x-oss-server-side-data-encryption", out var value) ? value : null;

        /// <summary>
        /// The ID of the customer master key (CMK) that is managed by Key Management Service (KMS).
        /// </summary>
        public string? ServerSideEncryptionKeyId
            => Headers.TryGetValue("x-oss-server-side-encryption-key-id", out var value) ? value : null;

        /// <summary>
        /// The position for the next append operation.
        /// If the type of the object is Appendable, this header is included in the response.
        /// </summary>
        public long? NextAppendPosition => Headers.TryGetValue("x-oss-next-append-position", out var value)
            ? Convert.ToInt64(value, CultureInfo.InvariantCulture)
            : null;

        /// <summary>
        /// The lifecycle information about the object.
        /// If lifecycle rules are configured for the object, this header is included in the response.
        /// This header contains the following parameters: expiry-date that indicates the expiration time of the object, and rule-id that indicates the ID of the matched lifecycle rule.
        /// </summary>
        public string? Expiration => Headers.TryGetValue("x-oss-expiration", out var value) ? value : null;

        /// <summary>
        /// The status of the object when you restore an object.
        /// If the storage class of the bucket is Archive and a RestoreObject request is submitted.
        /// </summary>
        public string? Restore => Headers.TryGetValue("x-oss-restore", out var value) ? value : null;

        /// <summary>
        /// The result of an event notification that is triggered for the object.
        /// </summary>
        public string? ProcessStatus => Headers.TryGetValue("x-oss-process-status", out var value) ? value : null;

        /// <summary>
        /// Specifies whether the object retrieved was (true) or was not (false) a Delete  Marker.
        /// </summary>
        public bool? DeleteMarker
            => Headers.TryGetValue("x-oss-delete-marker", out var value) ? Convert.ToBoolean(value) : null;

        /// <summary>
        /// The requester. This header is included in the response if the pay-by-requester mode is enabled for the bucket and the requester is not the bucket owner. The value of this header is requester.
        /// </summary>
        public string? RequestCharged => Headers.TryGetValue("x-oss-request-charged", out var value) ? value : null;

        /// <summary>
        /// The time when the storage class of the returned objects is changed to Cold Archive or Deep Cold Archive based on lifecycle rules.
        /// </summary>
        public string? TransitionTime => Headers.TryGetValue("x-oss-transition-time", out var value) ? value : null;
    }

    /// <summary>
    /// The request for the GetObjectMeta operation.
    /// </summary>
    public sealed class GetObjectMetaRequest : RequestModel {
        /// <summary>
        /// The name of the bucket.
        /// </summary>
        public string? Bucket { get; set; }

        /// <summary>
        /// The full path of the object.
        /// </summary>
        public string? Key { get; set; }

        /// <summary>
        /// The versionID of the object.
        /// </summary>
        public string? VersionId {
            get => Parameters.TryGetValue("versionId", out var value) ? value : null;
            set {
                if (value != null) Parameters["versionId"] = value;
            }
        }
    }

    /// <summary>
    /// The result for the GetObjectMeta operation.
    /// </summary>
    public sealed class GetObjectMetaResult : ResultModel {
        /// <summary>
        /// Version of the object.
        /// </summary>
        public string? VersionId => Headers.TryGetValue("x-oss-version-id", out var value) ? value : null;

        /// <summary>
        /// The entity tag (ETag).
        /// An ETag is created when an object is created to identify the content of the object.
        /// </summary>
        public string? ETag => Headers.TryGetValue("ETag", out var value) ? value : null;

        /// <summary>
        /// Size of the body in bytes.
        /// </summary>
        public long? ContentLength => Headers.TryGetValue("Content-Length", out var value)
            ? Convert.ToInt64(value, CultureInfo.InvariantCulture)
            : null;

        /// <summary>
        /// The time when the object was last accessed.
        /// </summary>
        public string? LastAccessTime => Headers.TryGetValue("x-oss-last-access-time", out var value) ? value : null;

        /// <summary>
        /// The time when the returned objects were last modified.
        /// </summary>
        public string? LastModified => Headers.TryGetValue("Last-Modified", out var value) ? value : null;

        /// <summary>
        /// The time when the storage class of the returned objects is changed to Cold Archive or Deep Cold Archive based on lifecycle rules.
        /// </summary>
        public string? TransitionTime => Headers.TryGetValue("x-oss-transition-time", out var value) ? value : null;

        /// <summary>
        /// The 64-bit CRC value of the object.
        /// This value is calculated based on the ECMA-182 standard.
        /// </summary>
        public string? HashCrc64 => Headers.TryGetValue("x-oss-hash-crc64ecma", out var value) ? value : null;
    }

    /// <summary>
    /// The container that stores the restoration priority configuration. This configuration takes effect only when the request is sent to restore Cold Archive objects. If you do not specify the JobParameters parameter, the default restoration priority Standard is used.
    /// </summary>
    [XmlRoot("JobParameters")]
    public sealed class JobParameters {
        /// <summary>
        /// The restoration priority. Valid values:*   Expedited: The object is restored within 1 hour.*   Standard: The object is restored within 2 to 5 hours.*   Bulk: The object is restored within 5 to 12 hours.
        /// </summary>
        [XmlElement("Tier")]
        public string? Tier { get; set; }
    }

    /// <summary>
    /// The container that stores information about the RestoreObject request.
    /// </summary>
    [XmlRoot("RestoreRequest")]
    public sealed class RestoreRequest {
        /// <summary>
        /// The duration in which the object can remain in the restored state. Unit: days. Valid values: 1 to 7.
        /// </summary>
        [XmlElement("Days")]
        public long? Days { get; set; }

        /// <summary>
        /// The container that stores the restoration priority configuration. This configuration takes effect only when the request is sent to restore Cold Archive objects. If you do not specify the JobParameters parameter, the default restoration priority Standard is used.
        /// </summary>
        [XmlElement("JobParameters")]
        public JobParameters? JobParameters { get; set; }
    }

    /// <summary>
    /// The request for the RestoreObject operation.
    /// </summary>
    public sealed class RestoreObjectRequest : RequestModel {
        public RestoreObjectRequest() {
            BodyFormat = "xml";
        }

        /// <summary>
        /// The name of the bucket.
        /// </summary>
        public string? Bucket { get; set; }

        /// <summary>
        /// The full path of the object.
        /// </summary>
        public string? Key { get; set; }

        /// <summary>
        /// The version number of the object that you want to restore.
        /// </summary>
        public string? VersionId {
            get => Parameters.TryGetValue("versionId", out var value) ? value : null;
            set {
                if (value != null) Parameters["versionId"] = value;
            }
        }

        /// <summary>
        /// The container that stores information about the RestoreObject request.
        /// </summary>
        public RestoreRequest? RestoreRequest {
            get => InnerBody as RestoreRequest;
            set => InnerBody = value;
        }
    }

    /// <summary>
    /// The result for the RestoreObject operation.
    /// </summary>
    public sealed class RestoreObjectResult : ResultModel {
        /// <summary>
        /// The restoration priority.
        /// This header is displayed only for the Cold Archive or Deep Cold Archive object in the restored state.
        /// </summary>
        public string? RestorePriority
            => Headers.TryGetValue("x-oss-object-restore-priority", out var value) ? value : null;

        /// <summary>
        /// Version of the object.
        /// </summary>
        public string? VersionId => Headers.TryGetValue("x-oss-version-id", out var value) ? value : null;
    }

    /// <summary>
    /// The request for the CleanRestoredObject operation.
    /// </summary>
    public sealed class CleanRestoredObjectRequest : RequestModel {
        /// <summary>
        /// The name of the bucket
        /// </summary>
        public string? Bucket { get; set; }

        /// <summary>
        /// The name of the object.
        /// </summary>
        public string? Key { get; set; }
    }

    /// <summary>
    /// The result for the CleanRestoredObject operation.
    /// </summary>
    public sealed class CleanRestoredObjectResult : ResultModel { }

    /// <summary>
    /// The request for the ProcessObject operation.
    /// </summary>
    public sealed class ProcessObjectRequest : RequestModel {
        /// <summary>
        /// The name of the bucket
        /// </summary>
        public string? Bucket { get; set; }

        /// <summary>
        /// The name of the object.
        /// </summary>
        public string? Key { get; set; }

        /// <summary>
        /// Image processing parameters.
        /// </summary>
        public string? Process { get; set; }
    }

    /// <summary>
    /// The result for the ProcessObject operation.
    /// </summary>
    public sealed class ProcessObjectResult : ResultModel {
        /// <summary>
        /// Process result in json format.
        /// contains bucket, file_size, object and status
        /// </summary>
        public string? ProcessResult => InnerBody as string;

        public ProcessObjectResult() {
            BodyFormat = "string";
        }
    }

    /// <summary>
    /// The request for the AsyncProcessObject operation.
    /// </summary>
    public sealed class AsyncProcessObjectRequest : RequestModel {
        /// <summary>
        /// The name of the bucket
        /// </summary>
        public string? Bucket { get; set; }

        /// <summary>
        /// The name of the object.
        /// </summary>
        public string? Key { get; set; }

        /// <summary>
        /// Image processing parameters.
        /// </summary>
        public string? Process { get; set; }
    }

    /// <summary>
    /// The result for the AsyncProcessObject operation.
    /// </summary>
    public sealed class AsyncProcessObjectResult : ResultModel {
        /// <summary>
        /// Process result in json format.
        /// contains EventId, TaskId and RequestId
        /// </summary>
        public string? ProcessResult => InnerBody as string;

        public AsyncProcessObjectResult() {
            BodyFormat = "string";
        }
    }

    /// <summary>
    /// The information about a delete object.
    /// </summary>
    [XmlRoot("Object")]
    public sealed class DeleteObject {
        /// <summary>
        /// The name of the object that you want to delete.
        /// </summary>
        [XmlElement("Key")]
        public string? Key { get; set; }

        /// <summary>
        /// The version ID of the object that you want to delete.
        /// </summary>
        [XmlElement("VersionId")]
        public string? VersionId { get; set; }
    }

    /// <summary>
    /// The information about a deleted object.
    /// </summary>
    [XmlRoot("Deleted")]
    public sealed class DeletedInfo {
        /// <summary>
        /// The name of the deleted object.
        /// </summary>
        [XmlElement("Key")]
        public string? Key { get; set; }

        /// <summary>
        /// The version ID of the object that you deleted.
        /// </summary>
        [XmlElement("VersionId")]
        public string? VersionId { get; set; }

        /// <summary>
        /// Indicates whether the deleted version is a delete marker.
        /// </summary>
        [XmlElement("DeleteMarker")]
        public bool? DeleteMarker { get; set; }

        /// <summary>
        /// The version ID of the delete marker.
        /// </summary>
        [XmlElement("DeleteMarkerVersionId")]
        public string? DeleteMarkerVersionId { get; set; }
    }

    /// <summary>
    /// The request for the DeleteMultipleObjects operation.
    /// </summary>
    public sealed class DeleteMultipleObjectsRequest : RequestModel {
        /// <summary>
        /// The name of the bucket
        /// </summary>
        public string? Bucket { get; set; }

        /// <summary>
        /// The encoding type of the object names in the response. Valid value: url
        /// Sees <see cref="Models.EncodingType"/> for supported values.
        /// </summary>
        public string? EncodingType {
            get => Parameters.TryGetValue("encoding-type", out var value) ? value : null;
            set {
                if (value != null) Parameters["encoding-type"] = value;
            }
        }

        /// <summary>
        /// Specifies whether to enable the Quiet return mode.
        /// The DeleteMultipleObjects operation provides the following return modes: Valid value: true,false
        /// </summary>
        public bool? Quiet { get; set; }

        /// <summary>
        /// The container that stores information about you want to delete objects.
        /// </summary>
        public IList<DeleteObject>? Objects { get; set; }
    }

    /// <summary>
    /// The result for the DeleteMultipleObjects operation.
    /// </summary>
    public sealed class DeleteMultipleObjectsResult : ResultModel {
        /// <summary>
        /// The container that stores information about you want to delete objects.
        /// </summary>
        public IList<DeletedInfo>? DeletedObjects { get; internal set; }

        /// <summary>
        /// The encoding type of the object names in the response. Valid value: url
        /// Sees <see cref="Models.EncodingType"/> for supported values.
        /// </summary>
        public string? EncodingType { get; internal set; }
    }
}