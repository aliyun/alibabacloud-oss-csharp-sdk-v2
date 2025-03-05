using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml.Serialization;

namespace AlibabaCloud.OSS.V2.Models
{
    /// <summary>
    /// The container that stores the uploaded parts.
    /// </summary>
    [XmlRoot("Part")]
    public sealed class UploadPart
    {
        /// <summary>
        /// The ETag that is generated when the object is created. ETags are used to identify the content of objects.If an object is created by calling the CompleteMultipartUpload operation, the ETag is not the MD5 hash of the object content but a unique value calculated based on a specific rule.  The ETag of an object can be used to check whether the object content changes. We recommend that you use the MD5 hash of an object rather than the ETag of the object to verify data integrity.
        /// </summary>
        [XmlElement("ETag")]
        public string? ETag { get; set; }

        /// <summary>
        /// The number of parts.
        /// </summary>
        [XmlElement("PartNumber")]
        public long? PartNumber { get; set; }
    }

    /// <summary>
    /// The information about the uploaded part.
    /// </summary>
    [XmlRoot("Part")]
    public sealed class Part
    {
        /// <summary>
        /// The ETag value of the content of the uploaded part.
        /// </summary>
        [XmlElement("ETag")]
        public string? ETag { get; set; }

        /// <summary>
        /// The number that identifies a part.
        /// </summary>
        [XmlElement("PartNumber")]
        public long? PartNumber { get; set; }

        /// <summary>
        /// The time when the part was uploaded.
        /// </summary>
        [XmlElement("LastModified")]
        public DateTime? LastModified { get; set; }

        /// <summary>
        /// The size of the uploaded parts.
        /// </summary>
        [XmlElement("Size")]
        public long? Size { get; set; }

        /// <summary>
        /// The 64-bit CRC value of the object.
        /// This value is calculated based on the ECMA-182 standard.
        /// </summary>
        [XmlElement("HashCrc64ecma")]
        public string? HashCrc64 { get; set; }
    }

    /// <summary>
    /// The container that stores the information about multipart upload tasks.
    /// </summary>
    [XmlRoot("Upload")]
    public sealed class Upload
    {
        /// <summary>
        /// The ID of the multipart upload task.
        /// </summary>
        [XmlElement("UploadId")]
        public string? UploadId { get; set; }

        /// <summary>
        /// The time when the multipart upload task was initiated.
        /// </summary>
        [XmlElement("Initiated")]
        public DateTime? Initiated { get; set; }

        /// <summary>
        /// The name of the object for which a multipart upload task was initiated.  The results returned by OSS are listed in ascending alphabetical order of object names. Multiple multipart upload tasks that are initiated to upload the same object are listed in ascending order of upload IDs.
        /// </summary>
        [XmlElement("Key")]
        public string? Key { get; set; }
    }

    /// <summary>
    /// The container that stores the content of the CompleteMultipartUpload request.
    /// </summary>
    [XmlRoot("CompleteMultipartUpload")]
    public sealed class CompleteMultipartUpload
    {
        /// <summary>
        /// The container that stores the uploaded parts.
        /// </summary>
        [XmlElement("Part")]
        public List<UploadPart>? Parts { get; set; }
    }

    /// <summary>
    /// The request for the InitiateMultipartUpload operation.
    /// </summary>
    public sealed class InitiateMultipartUploadRequest : RequestModel
    {
        /// <summary>
        /// The name of the bucket to which the object is uploaded by the multipart upload task.
        /// </summary>
        public string? Bucket { get; set; }

        /// <summary>
        /// The name of the object that is uploaded by the multipart upload task.
        /// </summary>
        public string? Key { get; set; }

        /// <summary>
        /// Specifies whether the object that is uploaded by calling the PutObject operation overwrites an existing object that has the same name.
        /// </summary>
        public bool? ForbidOverwrite
        {
            get => Headers.TryGetValue("x-oss-forbid-overwrite", out var value)
                ? Convert.ToBoolean(value, CultureInfo.InvariantCulture)
                : null;
            set
            {
                if (value != null)
                    Headers["x-oss-forbid-overwrite"] =
                        Convert.ToString(value, CultureInfo.InvariantCulture)!.ToLowerInvariant();
            }
        }

        /// <summary>
        /// The storage class of the bucket. Default value: Standard.  Valid values:- Standard- IA- Archive- ColdArchive
        /// Sees <see cref="StorageClassType"/> for supported values.
        /// </summary>
        public string? StorageClass
        {
            get => Headers.TryGetValue("x-oss-storage-class", out var value) ? value : null;
            set
            {
                if (value != null) Headers["x-oss-storage-class"] = value;
            }
        }

        /// <summary>
        /// The tag of the object. You can configure multiple tags for the object. Example: TagA=A&amp;TagB=B. The key and value of a tag must be URL-encoded. If a tag does not contain an equal sign (=), the value of the tag is considered an empty string.
        /// </summary>
        public string? Tagging
        {
            get => Headers.TryGetValue("x-oss-tagging", out var value) ? value : null;
            set
            {
                if (value != null) Headers["x-oss-tagging"] = value;
            }
        }

        /// <summary>
        /// The server-side encryption method that is used to encrypt each part of the object that you want to upload. Valid values: **AES256**, **KMS**, and **SM4**. You must activate Key Management Service (KMS) before you set this header to KMS. If you specify this header in the request, this header is included in the response. OSS uses the method specified by this header to encrypt each uploaded part. When you download the object, the x-oss-server-side-encryption header is included in the response and the header value is set to the algorithm that is used to encrypt the object.
        /// </summary>
        public string? ServerSideEncryption
        {
            get => Headers.TryGetValue("x-oss-server-side-encryption", out var value) ? value : null;
            set
            {
                if (value != null) Headers["x-oss-server-side-encryption"] = value;
            }
        }

        /// <summary>
        /// The algorithm that is used to encrypt the object that you want to upload. If this header is not specified, the object is encrypted by using AES-256. This header is valid only when **x-oss-server-side-encryption** is set to KMS. Valid value: SM4.
        /// </summary>
        public string? ServerSideDataEncryption
        {
            get => Headers.TryGetValue("x-oss-server-side-data-encryption", out var value) ? value : null;
            set
            {
                if (value != null) Headers["x-oss-server-side-data-encryption"] = value;
            }
        }

        /// <summary>
        /// The ID of the CMK that is managed by KMS. This header is valid only when **x-oss-server-side-encryption** is set to KMS.
        /// </summary>
        public string? ServerSideEncryptionKeyId
        {
            get => Headers.TryGetValue("x-oss-server-side-encryption-key-id", out var value) ? value : null;
            set
            {
                if (value != null) Headers["x-oss-server-side-encryption-key-id"] = value;
            }
        }

        /// <summary>
        /// The caching behavior of the web page when the object is downloaded.
        /// </summary>
        public string? CacheControl
        {
            get => Headers.TryGetValue("Cache-Control", out var value) ? value : null;
            set
            {
                if (value != null) Headers["Cache-Control"] = value;
            }
        }

        /// <summary>
        /// The name of the object when the object is downloaded.
        /// </summary>
        public string? ContentDisposition
        {
            get => Headers.TryGetValue("Content-Disposition", out var value) ? value : null;
            set
            {
                if (value != null) Headers["Content-Disposition"] = value;
            }
        }

        /// <summary>
        /// The content encoding format of the object when the object is downloaded.
        /// </summary>
        public string? ContentEncoding
        {
            get => Headers.TryGetValue("Content-Encoding", out var value) ? value : null;
            set
            {
                if (value != null) Headers["Content-Encoding"] = value;
            }
        }

        /// <summary>
        /// The expiration time of the request.
        /// </summary>
        public string? Expires
        {
            get => Headers.TryGetValue("Expires", out var value) ? value : null;
            set
            {
                if (value != null) Headers["Expires"] = value;
            }
        }

        /// <summary>
        /// The MD5 hash of the object that you want to upload.
        /// </summary>
        public string? ContentMd5
        {
            get => Headers.TryGetValue("Content-MD5", out var value) ? value : null;
            set
            {
                if (value != null) Headers["Content-MD5"] = value;
            }
        }

        /// <summary>
        /// A standard MIME type describing the format of the contents.
        /// </summary>
        public string? ContentType
        {
            get => Headers.TryGetValue("Content-Type", out var value) ? value : null;
            set
            {
                if (value != null) Headers["Content-Type"] = value;
            }
        }

        /// <summary>
        /// The size of the data in the HTTP message body. Unit: bytes.
        /// </summary>
        public long? ContentLength
        {
            get => Headers.TryGetValue("Content-Length", out var value)
                ? Convert.ToInt64(value, CultureInfo.InvariantCulture)
                : null;
            set
            {
                if (value != null) Headers["Content-Length"] = Convert.ToString(value, CultureInfo.InvariantCulture)!;
            }
        }

        /// <summary>
        /// The metadata of the object that you want to upload.
        /// </summary>
        public IDictionary<string, string>? Metadata { get; set; }

        /// <summary>
        /// The method used to encode the object name in the response. Only URL encoding is supported. The object name can contain characters encoded in UTF-8. However, the XML 1.0 standard cannot be used to parse specific control characters, such as characters whose ASCII values range from 0 to 10. You can configure the encoding-type parameter to encode object names that include characters that cannot be parsed by XML 1.0 in the response.brDefault value: null
        /// Sees <see cref="Models.EncodingType"/> for supported values.
        /// </summary>
        public string? EncodingType
        {
            get => Parameters.TryGetValue("encoding-type", out var value) ? value : null;
            set
            {
                if (value != null) Parameters["encoding-type"] = value;
            }
        }

        /// <summary>
        /// To disable the feature that Content-Type is automatically added based on the object name if not specified.
        /// </summary>
        public bool DisableAutoDetectMimeType { get; set; }
    }

    /// <summary>
    /// The result for the InitiateMultipartUpload operation.
    /// </summary>
    public sealed class InitiateMultipartUploadResult : ResultModel
    {
        /// <summary>
        /// The name of the bucket to which the object is uploaded by the multipart upload task.
        /// </summary>
        public string? Bucket;

        /// <summary>
        /// The name of the object that is uploaded by the multipart upload task.
        /// </summary>
        public string? Key;

        /// <summary>
        /// The upload ID that uniquely identifies the multipart upload task.
        /// </summary>
        public string? UploadId;

        /// <summary>
        /// The encoding type of the object names in the response. Valid value: url
        /// </summary>
        public string? EncodingType;
    }

    /// <summary>
    /// The request for the UploadPart operation.
    /// </summary>
    public sealed class UploadPartRequest : RequestModel
    {
        /// <summary>
        /// The name of the bucket.
        /// </summary>
        public string? Bucket { get; set; }

        /// <summary>
        /// The full path of the object.
        /// </summary>
        public string? Key { get; set; }

        /// <summary>
        /// The number that identifies a part. Valid values: 1 to 10000.The size of a part ranges from 100 KB to 5 GB.  In multipart upload, each part except the last part must be larger than or equal to 100 KB in size. When you call the UploadPart operation, the size of each part is not verified because not all parts have been uploaded and OSS does not know which part is the last part. The size of each part is verified only when you call CompleteMultipartUpload.
        /// </summary>
        public long? PartNumber
        {
            get => Parameters.TryGetValue("partNumber", out var value)
                ? Convert.ToInt64(value, CultureInfo.InvariantCulture)
                : null;
            set
            {
                if (value != null) Parameters["partNumber"] = Convert.ToString(value, CultureInfo.InvariantCulture)!;
            }
        }

        /// <summary>
        /// The ID that identifies the object to which the part that you want to upload belongs.
        /// </summary>
        public string? UploadId
        {
            get => Parameters.TryGetValue("uploadId", out var value) ? value : null;
            set
            {
                if (value != null) Parameters["uploadId"] = value;
            }
        }

        /// <summary>
        /// The MD5 hash of the object that you want to upload.
        /// </summary>
        public string? ContentMd5
        {
            get => Headers.TryGetValue("Content-MD5", out var value) ? value : null;
            set
            {
                if (value != null) Headers["Content-MD5"] = value;
            }
        }

        /// <summary>
        /// The size of the data in the HTTP message body. Unit: bytes.
        /// </summary>
        public long? ContentLength
        {
            get => Headers.TryGetValue("Content-Length", out var value)
                ? Convert.ToInt64(value, CultureInfo.InvariantCulture)
                : null;
            set
            {
                if (value != null) Headers["Content-Length"] = Convert.ToString(value, CultureInfo.InvariantCulture)!;
            }
        }

        /// <summary>
        /// Specify the speed limit value. The speed limit value ranges from  245760 to 838860800, with a unit of bit/s.
        /// </summary>
        public long? TrafficLimit
        {
            get => Headers.TryGetValue("x-oss-traffic-limit", out var value)
                ? Convert.ToInt64(value, CultureInfo.InvariantCulture)
                : null;
            set
            {
                if (value != null) Headers["x-oss-traffic-limit"] = Convert.ToString(value, CultureInfo.InvariantCulture)!;
            }
        }

        /// <summary>
        /// Progress callback function
        /// </summary>
        public ProgressFunc? ProgressFn { get; set; }

        /// <summary>
        /// The request body.
        /// </summary>
        public Stream? Body
        {
            get => InnerBody as Stream;
            set => InnerBody = value;
        }
    }

    /// <summary>
    /// The result for the UploadPart operation.
    /// </summary>
    public sealed class UploadPartResult : ResultModel
    {
        /// <summary>
        /// The MD5 hash of the part that you want to upload.
        /// </summary>
        public string? ContentMd5
            => Headers.TryGetValue("Content-MD5", out var value) ? value : null;

        /// <summary>
        ///  Entity tag for the uploaded part.
        /// </summary>
        public string? ETag
            => Headers.TryGetValue("ETag", out var value) ? value : null;

        /// <summary>
        /// The 64-bit CRC value of the part.
        /// This value is calculated based on the ECMA-182 standard.
        /// </summary>
        public string? HashCrc64
            => Headers.TryGetValue("x-oss-hash-crc64ecma", out var value) ? value : null;
    }

    /// <summary>
    /// The request for the CompleteMultipartUpload operation.
    /// </summary>
    public sealed class CompleteMultipartUploadRequest : RequestModel
    {
        public CompleteMultipartUploadRequest()
        {
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
        /// Specifies whether the object that is uploaded by calling the PutObject operation overwrites an existing object that has the same name.
        /// </summary>
        public bool? ForbidOverwrite
        {
            get => Headers.TryGetValue("x-oss-forbid-overwrite", out var value)
                ? Convert.ToBoolean(value, CultureInfo.InvariantCulture)
                : null;
            set
            {
                if (value != null)
                    Headers["x-oss-forbid-overwrite"] =
                        Convert.ToString(value, CultureInfo.InvariantCulture)!.ToLowerInvariant();
            }
        }

        /// <summary>
        /// Specifies whether to list all parts that are uploaded by using the current upload ID.Valid value: yes.- If x-oss-complete-all is set to yes in the request, OSS lists all parts that are uploaded by using the current upload ID, sorts the parts by part number, and then performs the CompleteMultipartUpload operation. When OSS performs the CompleteMultipartUpload operation, OSS cannot detect the parts that are not uploaded or currently being uploaded. Before you call the CompleteMultipartUpload operation, make sure that all parts are uploaded.- If x-oss-complete-all is specified in the request, the request body cannot be specified. Otherwise, an error occurs.- If x-oss-complete-all is specified in the request, the format of the response remains unchanged.
        /// </summary>
        public string? CompleteAll
        {
            get => Headers.TryGetValue("x-oss-complete-all", out var value) ? value : null;
            set
            {
                if (value != null) Headers["x-oss-complete-all"] = value;
            }
        }

        /// <summary>
        /// The identifier of the multipart upload task.
        /// </summary>
        public string? UploadId
        {
            get => Parameters.TryGetValue("uploadId", out var value) ? value : null;
            set
            {
                if (value != null) Parameters["uploadId"] = value;
            }
        }

        /// <summary>
        /// The access control list (ACL) of the object.
        /// Sees <see cref="ObjectAclType"/> for supported values.
        /// </summary>
        public string? Acl
        {
            get => Headers.TryGetValue("x-oss-object-acl", out var value) ? value : null;
            set
            {
                if (value != null) Headers["x-oss-object-acl"] = value;
            }
        }

        /// <summary>
        /// A callback parameter is a Base64-encoded string that contains multiple fields in the JSON format.
        /// </summary>
        public string? Callback
        {
            get => Headers.TryGetValue("x-oss-callback", out var value) ? value : null;
            set
            {
                if (value != null) Headers["x-oss-callback"] = value;
            }
        }

        /// <summary>
        /// Configure custom parameters by using the callback-var parameter.
        /// </summary>
        public string? CallbackVar
        {
            get => Headers.TryGetValue("x-oss-callback-var", out var value) ? value : null;
            set
            {
                if (value != null) Headers["x-oss-callback-var"] = value;
            }
        }

        /// <summary>
        /// The encoding type of the object name in the response. Only URL encoding is supported.The object name can contain characters that are encoded in UTF-8. However, the XML 1.0 standard cannot be used to parse control characters, such as characters with an ASCII value from 0 to 10. You can configure this parameter to encode the object name in the response.
        /// Sees <see cref="Models.EncodingType"/> for supported values.
        /// </summary>
        public string? EncodingType
        {
            get => Parameters.TryGetValue("encoding-type", out var value) ? value : null;
            set
            {
                if (value != null) Parameters["encoding-type"] = value;
            }
        }

        /// <summary>
        /// The container that stores the content of the CompleteMultipartUpload
        /// </summary>
        public CompleteMultipartUpload? CompleteMultipartUpload
        {
            get => InnerBody as CompleteMultipartUpload;
            set => InnerBody = value;
        }
    }

    /// <summary>
    /// The result for the CompleteMultipartUpload operation.
    /// </summary>
    public sealed class CompleteMultipartUploadResult : ResultModel
    {
        /// <summary>
        /// The object version id
        /// </summary>
        public string? VersionId => Headers.TryGetValue("x-oss-version-id", out var value) ? value : null;

        /// <summary>
        /// The 64-bit CRC value of the object.
        /// This value is calculated based on the ECMA-182 standard.
        /// </summary>
        public string? HashCrc64 => Headers.TryGetValue("x-oss-hash-crc64ecma", out var value) ? value : null;

        /// <summary>
        /// The encoding type of the object name in the response. If this parameter is specified in the request, the object name is encoded in the response.
        /// </summary>
        public string? EncodingType;

        /// <summary>
        /// The name of the bucket that contains the object you want to restore.
        /// </summary>
        public string? Bucket;

        /// <summary>
        /// The name of the uploaded object.
        /// </summary>
        public string? Key;

        /// <summary>
        /// The ETag that is generated when an object is created. ETags are used to identify the content of objects.If an object is created by calling the CompleteMultipartUpload operation, the ETag value is not the MD5 hash of the object content but a unique value calculated based on a specific rule. The ETag of an object can be used to check whether the object content is modified. However, we recommend that you use the MD5 hash of an object rather than the ETag value of the object to verify data integrity.
        /// </summary>
        public string? ETag;

        /// <summary>
        /// Callback result in json format.
        /// It is valid only when the callback is set.
        /// </summary>
        public string? CallbackResult;
    }

    /// <summary>
    /// The request for the UploadPartCopy operation.
    /// </summary>
    public sealed class UploadPartCopyRequest : RequestModel
    {
        /// <summary>
        /// The name of the bucket.
        /// </summary>
        public string? Bucket { get; set; }

        /// <summary>
        /// The full path of the object.
        /// </summary>
        public string? Key { get; set; }

        /// <summary>
        /// The number of parts.
        /// </summary>
        public long? PartNumber
        {
            get => Parameters.TryGetValue("partNumber", out var value)
                ? Convert.ToInt64(value, CultureInfo.InvariantCulture)
                : null;
            set
            {
                if (value != null) Parameters["partNumber"] = Convert.ToString(value, CultureInfo.InvariantCulture)!;
            }
        }

        /// <summary>
        /// The ID that identifies the object to which the parts to upload belong.
        /// </summary>
        public string? UploadId
        {
            get => Parameters.TryGetValue("uploadId", out var value) ? value : null;
            set
            {
                if (value != null) Parameters["uploadId"] = value;
            }
        }

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
        ///  The range of bytes to copy data from the source object.
        /// </summary>
        public string? SourceRange
        {
            get => Headers.TryGetValue("x-oss-copy-source-range", out var value) ? value : null;
            set
            {
                if (value != null) Headers["x-oss-copy-source-range"] = value;
            }
        }

        /// <summary>
        /// The copy operation condition. If the ETag value of the source object is the same as the ETag value provided by the user, OSS copies data. Otherwise, OSS returns 412 Precondition Failed.brDefault value: null
        /// </summary>
        public string? IfMatch
        {
            get => Headers.TryGetValue("x-oss-copy-source-if-match", out var value) ? value : null;
            set
            {
                if (value != null) Headers["x-oss-copy-source-if-match"] = value;
            }
        }

        /// <summary>
        /// The object transfer condition. If the input ETag value does not match the ETag value of the object, the system transfers the object normally and returns 200 OK. Otherwise, OSS returns 304 Not Modified.brDefault value: null
        /// </summary>
        public string? IfNoneMatch
        {
            get => Headers.TryGetValue("x-oss-copy-source-if-none-match", out var value) ? value : null;
            set
            {
                if (value != null) Headers["x-oss-copy-source-if-none-match"] = value;
            }
        }

        /// <summary>
        /// The object transfer condition. If the specified time is the same as or later than the actual modified time of the object, OSS transfers the object normally and returns 200 OK. Otherwise, OSS returns 412 Precondition Failed.brDefault value: null
        /// </summary>
        public string? IfUnmodifiedSince
        {
            get => Headers.TryGetValue("x-oss-copy-source-if-unmodified-since", out var value) ? value : null;
            set
            {
                if (value != null) Headers["x-oss-copy-source-if-unmodified-since"] = value;
            }
        }

        /// <summary>
        /// The object transfer condition. If the specified time is earlier than the actual modified time of the object, the system transfers the object normally and returns 200 OK. Otherwise, OSS returns 304 Not Modified.brDefault value: null. Time format: ddd, dd MMM yyyy HH:mm:ss GMT. Example: Fri, 13 Nov 2015 14:47:53 GMT.
        /// </summary>
        public string? IfModifiedSince
        {
            get => Headers.TryGetValue("x-oss-copy-source-if-modified-since", out var value) ? value : null;
            set
            {
                if (value != null) Headers["x-oss-copy-source-if-modified-since"] = value;
            }
        }

        /// <summary>
        /// Specify the speed limit value. The speed limit value ranges from  245760 to 838860800, with a unit of bit/s.
        /// </summary>
        public long? TrafficLimit
        {
            get => Headers.TryGetValue("x-oss-traffic-limit", out var value)
                ? Convert.ToInt64(value, CultureInfo.InvariantCulture)
                : null;
            set
            {
                if (value != null) Headers["x-oss-traffic-limit"] = Convert.ToString(value, CultureInfo.InvariantCulture)!;
            }
        }
    }

    /// <summary>
    /// The result for the UploadPartCopy operation.
    /// </summary>
    public sealed class UploadPartCopyResult : ResultModel
    {
        /// <summary>
        /// The source object version id.
        /// </summary>
        public string? SourceVersionId
            => Headers.TryGetValue("x-oss-copy-source-version-id", out var value) ? value : null;

        /// <summary>
        /// The last modified time of copy source.
        /// </summary>
        public DateTime? LastModified { get; set; }

        /// <summary>
        /// The ETag of the copied part.
        /// </summary>
        public string? ETag { get; set; }
    }

    /// <summary>
    /// The request for the AbortMultipartUpload operation.
    /// </summary>
    public sealed class AbortMultipartUploadRequest : RequestModel
    {
        /// <summary>
        /// The name of the bucket.
        /// </summary>
        public string? Bucket { get; set; }

        /// <summary>
        /// The full path of the object that you want to upload.
        /// </summary>
        public string? Key { get; set; }

        /// <summary>
        /// The ID of the multipart upload task.
        /// </summary>
        public string? UploadId
        {
            get => Parameters.TryGetValue("uploadId", out var value) ? value : null;
            set
            {
                if (value != null) Parameters["uploadId"] = value;
            }
        }
    }

    /// <summary>
    /// The result for the AbortMultipartUpload operation.
    /// </summary>
    public sealed class AbortMultipartUploadResult : ResultModel { }

    /// <summary>
    /// The request for the ListMultipartUploads operation.
    /// </summary>
    public sealed class ListMultipartUploadsRequest : RequestModel
    {
        /// <summary>
        /// The name of the bucket.
        /// </summary>
        public string? Bucket { get; set; }

        /// <summary>
        /// The character used to group objects by name. Objects whose names contain the same string that ranges from the specified prefix to the delimiter that appears for the first time are grouped as a CommonPrefixes element.
        /// </summary>
        public string? Delimiter
        {
            get => Parameters.TryGetValue("delimiter", out var value) ? value : null;
            set
            {
                if (value != null) Parameters["delimiter"] = value;
            }
        }

        /// <summary>
        /// The maximum number of multipart upload tasks that can be returned for the current request. Default value: 1000. Maximum value: 1000.
        /// </summary>
        public long? MaxUploads
        {
            get => Parameters.TryGetValue("max-uploads", out var value)
                ? Convert.ToInt64(value, CultureInfo.InvariantCulture)
                : null;
            set
            {
                if (value != null) Parameters["max-uploads"] = Convert.ToString((long)value, CultureInfo.InvariantCulture);
            }
        }

        /// <summary>
        /// This parameter is used together with the upload-id-marker parameter to specify the position from which the next list begins.- If the upload-id-marker parameter is not set, Object Storage Service (OSS) returns all multipart upload tasks in which object names are alphabetically after the key-marker value.- If the upload-id-marker parameter is set, the response includes the following tasks:  - Multipart upload tasks in which object names are alphabetically after the key-marker value in alphabetical order  - Multipart upload tasks in which object names are the same as the key-marker parameter value but whose upload IDs are greater than the upload-id-marker parameter value
        /// </summary>
        public string? KeyMarker
        {
            get => Parameters.TryGetValue("key-marker", out var value) ? value : null;
            set
            {
                if (value != null) Parameters["key-marker"] = value;
            }
        }

        /// <summary>
        /// The prefix that the returned object names must contain. If you specify a prefix in the request, the specified prefix is included in the response.You can use prefixes to group and manage objects in buckets in the same way you manage a folder in a file system.
        /// </summary>
        public string? Prefix
        {
            get => Parameters.TryGetValue("prefix", out var value) ? value : null;
            set
            {
                if (value != null) Parameters["prefix"] = value;
            }
        }

        /// <summary>
        /// The upload ID of the multipart upload task after which the list begins. This parameter is used together with the key-marker parameter.- If the key-marker parameter is not set, OSS ignores the upload-id-marker parameter.- If the key-marker parameter is configured, the query result includes:  - Multipart upload tasks in which object names are alphabetically after the key-marker value in alphabetical order  - Multipart upload tasks in which object names are the same as the key-marker parameter value but whose upload IDs are greater than the upload-id-marker parameter value
        /// </summary>
        public string? UploadIdMarker
        {
            get => Parameters.TryGetValue("upload-id-marker", out var value) ? value : null;
            set
            {
                if (value != null) Parameters["upload-id-marker"] = value;
            }
        }

        /// <summary>
        /// The encoding type of the object name in the response. Values of Delimiter, KeyMarker, Prefix, NextKeyMarker, and Key can be encoded in UTF-8. However, the XML 1.0 standard cannot be used to parse control characters such as characters with an American Standard Code for Information Interchange (ASCII) value from 0 to 10. You can set the encoding-type parameter to encode values of Delimiter, KeyMarker, Prefix, NextKeyMarker, and Key in the response.Default value: null
        /// Sees <see cref="Models.EncodingType"/> for supported values.
        /// </summary>
        public string? EncodingType
        {
            get => Parameters.TryGetValue("encoding-type", out var value) ? value : null;
            set
            {
                if (value != null) Parameters["encoding-type"] = value;
            }
        }
    }

    /// <summary>
    /// The result for the ListMultipartUploads operation.
    /// </summary>
    public sealed class ListMultipartUploadsResult : ResultModel
    {
        /// <summary>
        /// The prefix that the returned object names must contain. If you specify a prefix in the request, the specified prefix is included in the response.
        /// </summary>
        public string? Prefix;

        /// <summary>
        /// The method used to encode the object name in the response. If encoding-type is specified in the request, values of those elements including Delimiter, KeyMarker, Prefix, NextKeyMarker, and Key are encoded in the returned result.
        /// </summary>
        public string? EncodingType;

        /// <summary>
        /// The upload ID of the multipart upload task after which the list begins.
        /// </summary>
        public string? UploadIdMarker;

        /// <summary>
        /// The object name marker in the response for the next request to return the remaining results.
        /// </summary>
        public string? NextKeyMarker;

        /// <summary>
        /// The maximum number of multipart upload tasks returned by OSS.
        /// </summary>
        public long? MaxUploads;

        /// <summary>
        /// Indicates whether the list of multipart upload tasks returned in the response is truncated. Default value: false. Valid values:- true: Only part of the results are returned this time.- false: All results are returned.
        /// </summary>
        public bool? IsTruncated;

        /// <summary>
        /// The character used to group objects by name. If you specify the Delimiter parameter in the request, the response contains the CommonPrefixes element. Objects whose names contain the same string from the prefix to the next occurrence of the delimiter are grouped as a single result element in
        /// </summary>
        public string? Delimiter;

        /// <summary>
        /// The ID list of the multipart upload tasks.
        /// </summary>
        public List<Upload>? Uploads;

        /// <summary>
        /// The name of the bucket.
        /// </summary>
        public string? Bucket;

        /// <summary>
        /// The name of the object that corresponds to the multipart upload task after which the list begins.
        /// </summary>
        public string? KeyMarker;

        /// <summary>
        /// The NextUploadMarker value that is used for the UploadMarker value in the next request if the response does not contain all required results.
        /// </summary>
        public string? NextUploadIdMarker;
    }

    /// <summary>
    /// The request for the ListParts operation.
    /// </summary>
    public sealed class ListPartsRequest : RequestModel
    {
        /// <summary>
        /// The name of the bucket.
        /// </summary>
        public string? Bucket { get; set; }

        /// <summary>
        /// The name of the object.
        /// </summary>
        public string? Key { get; set; }

        /// <summary>
        /// The ID of the multipart upload task.By default, this parameter is left empty.
        /// </summary>
        public string? UploadId
        {
            get => Parameters.TryGetValue("uploadId", out var value) ? value : null;
            set
            {
                if (value != null) Parameters["uploadId"] = value;
            }
        }

        /// <summary>
        /// The maximum number of parts that can be returned by OSS.Default value: 1000.Maximum value: 1000.
        /// </summary>
        public long? MaxParts
        {
            get => Parameters.TryGetValue("max-parts", out var value)
                ? Convert.ToInt64(value, CultureInfo.InvariantCulture)
                : null;
            set
            {
                if (value != null) Parameters["max-parts"] = Convert.ToString(value, CultureInfo.InvariantCulture)!;
            }
        }

        /// <summary>
        /// The position from which the list starts. All parts whose part numbers are greater than the value of this parameter are listed.By default, this parameter is left empty.
        /// </summary>
        public long? PartNumberMarker
        {
            get => Parameters.TryGetValue("part-number-marker", out var value) ? Convert.ToInt64(value, CultureInfo.InvariantCulture) : null;
            set { if (value != null) Parameters["part-number-marker"] = Convert.ToString((long)value, CultureInfo.InvariantCulture); }
        }

        /// <summary>
        /// The maximum number of parts that can be returned by OSS. Default value: 1000.Maximum value: 1000.
        /// Sees <see cref="Models.EncodingType"/> for supported values.
        /// </summary>
        public string? EncodingType
        {
            get => Parameters.TryGetValue("encoding-type", out var value) ? value : null;
            set
            {
                if (value != null) Parameters["encoding-type"] = value;
            }
        }
    }

    /// <summary>
    /// The result for the ListParts operation.
    /// </summary>
    public sealed class ListPartsResult : ResultModel
    {
        /// <summary>
        /// The position from which the list starts. All parts whose part numbers are greater than the value of this parameter are listed.
        /// </summary>
        public long? PartNumberMarker { get; internal set; }

        /// <summary>
        /// The NextPartNumberMarker value that is used for the PartNumberMarker value in a subsequent request when the response does not contain all required results.
        /// </summary>
        public long? NextPartNumberMarker { get; internal set; }

        /// <summary>
        /// The maximum number of parts in the response.
        /// </summary>
        public long? MaxParts { get; internal set; }

        /// <summary>
        /// Indicates whether the list of parts returned in the response has been truncated. A value of true indicates that the response does not contain all required results. A value of false indicates that the response contains all required results.Valid values: true and false.
        /// </summary>
        public bool? IsTruncated { get; internal set; }

        /// <summary>
        /// The list of all parts.
        /// </summary>
        public List<Part>? Parts { get; internal set; }

        /// <summary>
        /// The name of the bucket.
        /// </summary>
        public string? Bucket { get; internal set; }

        /// <summary>
        /// The name of the object.
        /// </summary>
        public string? Key { get; internal set; }

        /// <summary>
        /// The ID of the upload task.
        /// </summary>
        public string? UploadId { get; internal set; }

        /// <summary>
        /// The method used to encode the object name in the response.
        /// If encoding-type is specified in the request, values of those elements including Key are encoded in the returned result.
        /// </summary>
        public string? EncodingType { get; set; }

        /// <summary>
        /// The storage class of the object.
        /// </summary>
        public string? StorageClass { get; set; }
    }
}
