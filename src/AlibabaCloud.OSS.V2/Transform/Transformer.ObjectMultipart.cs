using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml.Serialization;
using AlibabaCloud.OSS.V2.Extensions;

namespace AlibabaCloud.OSS.V2.Transform {
    [XmlRoot("InitiateMultipartUploadResult")]
    public sealed class XmlInitiateMultipartUploadResult {
        [XmlElement("Bucket")]
        public string? Bucket { get; set; }

        [XmlElement("Key")]
        public string? Key { get; set; }

        [XmlElement("UploadId")]
        public string? UploadId { get; set; }

        [XmlElement("EncodingType")]
        public string? EncodingType { get; set; }
    }

    [XmlRoot("CopyPartResult")]
    public sealed class XmlCopyPartResult {
        [XmlElement("LastModified")]
        public DateTime? LastModified { get; set; }

        [XmlElement("ETag")]
        public string? ETag { get; set; }
    }

    [XmlRoot("CompleteMultipartUploadResult")]
    public sealed class XmlCompleteMultipartUploadResult {
        [XmlElement("EncodingType")]
        public string? EncodingType { get; set; }

        [XmlElement("Bucket")]
        public string? Bucket { get; set; }

        [XmlElement("Key")]
        public string? Key { get; set; }

        [XmlElement("ETag")]
        public string? ETag { get; set; }
    }

    [XmlRoot("ListMultipartUploadsResult")]
    public sealed class XmlListMultipartUploadsResult {
        [XmlElement("EncodingType")]
        public string? EncodingType { get; set; }

        [XmlElement("Bucket")]
        public string? Bucket { get; set; }

        [XmlElement("KeyMarker")]
        public string? KeyMarker { get; set; }

        [XmlElement("UploadIdMarker")]
        public string? UploadIdMarker { get; set; }

        [XmlElement("NextKeyMarker")]
        public string? NextKeyMarker { get; set; }

        [XmlElement("NextUploadIdMarker")]
        public string? NextUploadIdMarker { get; set; }

        [XmlElement("Delimiter")]
        public string? Delimiter { get; set; }

        [XmlElement("Prefix")]
        public string? Prefix { get; set; }

        [XmlElement("MaxUploads")]
        public long? MaxUploads { get; set; }

        [XmlElement("IsTruncated")]
        public bool? IsTruncated { get; set; }

        [XmlElement("Upload")]
        public List<Models.Upload>? Uploads { get; set; }
    }

    [XmlRoot("ListPartsResult")]
    public sealed class XmlListPartsResult {
        [XmlElement("EncodingType")]
        public string? EncodingType { get; set; }

        [XmlElement("PartNumberMarker")]
        public string? PartNumberMarker { get; set; }

        [XmlElement("NextPartNumberMarker")]
        public string? NextPartNumberMarker { get; set; }

        [XmlElement("MaxParts")]
        public long? MaxParts { get; set; }

        [XmlElement("IsTruncated")]
        public bool? IsTruncated { get; set; }

        [XmlElement("Part")]
        public List<Models.Part>? Parts { get; set; }

        [XmlElement("Bucket")]
        public string? Bucket { get; set; }

        [XmlElement("Key")]
        public string? Key { get; set; }

        [XmlElement("UploadId")]
        public string? UploadId { get; set; }

        [XmlElement("StorageClass")]
        public string? StorageClass { get; set; }
    }

    internal static partial class Serde {
        public static void DeserializeInitiateMultipartUpload(
            ref Models.ResultModel baseResult,
            ref OperationOutput output
        ) {
            var serializer = new XmlSerializer(typeof(XmlInitiateMultipartUploadResult));
            using var body = output.Body!;
            var obj = serializer.Deserialize(body) as XmlInitiateMultipartUploadResult;
            var result = baseResult as Models.InitiateMultipartUploadResult;

            if (obj == null || result == null) return;

            DeserializeInitiateMultipartUploadEncodingType(ref obj);

            result.Bucket = obj.Bucket;
            result.Key = obj.Key;
            result.UploadId = obj.UploadId;
            result.EncodingType = obj.EncodingType;
        }

        private static void DeserializeInitiateMultipartUploadEncodingType(ref XmlInitiateMultipartUploadResult result) {
            if (!string.Equals("url", result.EncodingType)) return;

            if (result.Key != null) result.Key = result.Key.UrlDecode();
        }

        public static void DeserializeUploadPartCopy(
            ref Models.ResultModel baseResult,
            ref OperationOutput output
        ) {
            var serializer = new XmlSerializer(typeof(XmlCopyPartResult));
            using var body = output.Body!;
            var obj = serializer.Deserialize(body) as XmlCopyPartResult;
            var result = baseResult as Models.UploadPartCopyResult;

            if (obj == null || result == null) return;

            result.ETag = obj.ETag;
            result.LastModified = obj.LastModified;
        }

        public static void DeserializeCompleteMultipartUpload(
            ref Models.ResultModel baseResult,
            ref OperationOutput output
        ) {
            var serializer = new XmlSerializer(typeof(XmlCompleteMultipartUploadResult));
            using var body = output.Body!;
            var obj = serializer.Deserialize(body) as XmlCompleteMultipartUploadResult;
            var result = baseResult as Models.CompleteMultipartUploadResult;

            if (obj == null || result == null) return;

            result.Bucket = obj.Bucket;
            result.ETag = obj.ETag;
            result.EncodingType = obj.EncodingType;
            result.Key = obj.Key;

            if (string.Equals("url", result.EncodingType) && obj.Key != null) {
                result.Key = obj.Key.UrlDecode();
            }
        }

        public static void DeserializeCompleteMultipartUploadCallback(
            ref Models.ResultModel baseResult,
            ref OperationOutput output
        ) {
            if (output.Body == null) {
                return;
            }
            using var body = output.Body;
            using var reader = new StreamReader(body);
            var result = baseResult as Models.CompleteMultipartUploadResult;
            result!.CallbackResult = reader.ReadToEnd();
        }

        public static void DeserializeListMultipartUploads(
            ref Models.ResultModel baseResult,
            ref OperationOutput output
        ) {
            var serializer = new XmlSerializer(typeof(XmlListMultipartUploadsResult));
            using var body = output.Body!;
            var obj = serializer.Deserialize(body) as XmlListMultipartUploadsResult;
            var result = baseResult as Models.ListMultipartUploadsResult;

            if (obj == null || result == null) {
                return;
            }

            DeserializeListMultipartUploadsEncodingType(ref obj);

            result.Bucket = obj.Bucket;
            result.KeyMarker = obj.KeyMarker;
            result.UploadIdMarker = obj.UploadIdMarker;
            result.NextKeyMarker = obj.NextKeyMarker;
            result.NextUploadIdMarker = obj.NextUploadIdMarker;
            result.Delimiter = obj.Delimiter;
            result.MaxUploads = obj.MaxUploads;
            result.EncodingType = obj.EncodingType;
            result.Prefix = obj.Prefix;
            result.IsTruncated = obj.IsTruncated;
            result.Uploads = obj.Uploads;
        }

        private static void DeserializeListMultipartUploadsEncodingType(ref XmlListMultipartUploadsResult result) {
            if (!string.Equals("url", result.EncodingType)) {
                return;
            }

            if (result.Prefix != null) {
                result.Prefix = result.Prefix.UrlDecode();
            }

            if (result.KeyMarker != null) {
                result.KeyMarker = result.KeyMarker.UrlDecode();
            }

            if (result.NextKeyMarker != null) {
                result.NextKeyMarker = result.NextKeyMarker.UrlDecode();
            }

            if (result.Delimiter != null) {
                result.Delimiter = result.Delimiter.UrlDecode();
            }

            if (result.Uploads != null) {
                for (int i = 0; i < result.Uploads.Count; i++) {
                    if (result.Uploads[i].Key != null) {
                        result.Uploads[i].Key = result.Uploads[i].Key!.UrlDecode();
                    }
                }
            }
        }

        public static void DeserializeListParts(
            ref Models.ResultModel baseResult,
            ref OperationOutput output
        ) {
            var serializer = new XmlSerializer(typeof(XmlListPartsResult));
            using var body = output.Body!;
            var obj = serializer.Deserialize(body) as XmlListPartsResult;
            var result = baseResult as Models.ListPartsResult;

            if (obj == null || result == null) {
                return;
            }

            result.Bucket = obj.Bucket;
            result.Key = obj.Key;
            result.PartNumberMarker = string.IsNullOrEmpty(obj.PartNumberMarker) ? null : Convert.ToInt64(obj.PartNumberMarker, CultureInfo.InvariantCulture);
            result.NextPartNumberMarker = string.IsNullOrEmpty(obj.NextPartNumberMarker) ? null : Convert.ToInt64(obj.NextPartNumberMarker, CultureInfo.InvariantCulture);
            result.MaxParts = obj.MaxParts;
            result.UploadId = obj.UploadId;
            result.Parts = obj.Parts;
            result.IsTruncated = obj.IsTruncated;
            result.EncodingType = obj.EncodingType;
            result.StorageClass = obj.StorageClass;

            if (string.Equals("url", result.EncodingType) && obj.Key != null) {
                result.Key = obj.Key.UrlDecode();
            }
        }
    }
}