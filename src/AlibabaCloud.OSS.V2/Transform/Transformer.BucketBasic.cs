using System.Collections.Generic;
using System.Xml.Serialization;
using AlibabaCloud.OSS.V2.Extensions;

namespace AlibabaCloud.OSS.V2.Transform {
    [XmlRoot("ListBucketResult")]
    public sealed class XmlListBucketResult {
        [XmlElement("Name")]
        public string? Name { get; set; }

        [XmlElement("MaxKeys")]
        public int? MaxKeys { get; set; }

        [XmlElement("Delimiter")]
        public string? Delimiter { get; set; }

        [XmlElement("EncodingType")]
        public string? EncodingType { get; set; }

        [XmlElement("Marker")]
        public string? Marker { get; set; }

        [XmlElement("NextMarker")]
        public string? NextMarker { get; set; }

        [XmlElement("ContinuationToken")]
        public string? ContinuationToken { get; set; }

        [XmlElement("NextContinuationToken")]
        public string? NextContinuationToken { get; set; }

        [XmlElement("Contents")]
        public List<Models.ObjectSummary>? Contents { get; set; }

        [XmlElement("Prefix")]
        public string? Prefix { get; set; }

        [XmlElement("StartAfter")]
        public string? StartAfter { get; set; }

        [XmlElement("IsTruncated")]
        public bool? IsTruncated { get; set; }

        [XmlElement("KeyCount")]
        public int? KeyCount { get; set; }

        [XmlElement("CommonPrefixes")]
        public List<Models.CommonPrefix>? CommonPrefixes { get; set; }
    }

    internal static partial class Serde {
        public static void DeserializeListObjects(
            ref Models.ResultModel baseResult,
            ref OperationOutput output
        ) {
            var serializer = new XmlSerializer(typeof(XmlListBucketResult));
            using var body = output.Body!;
            var obj = serializer.Deserialize(body) as XmlListBucketResult;
            var result = baseResult as Models.ListObjectsResult;

            if (obj == null || result == null) {
                return;
            }

            DeserializeEncodingType(ref obj);

            result.Name = obj.Name;
            result.MaxKeys = obj.MaxKeys;
            result.Delimiter = obj.Delimiter;
            result.EncodingType = obj.EncodingType;
            result.Marker = obj.Marker;
            result.NextMarker = obj.NextMarker;
            result.Contents = obj.Contents;
            result.Prefix = obj.Prefix;
            result.IsTruncated = obj.IsTruncated;
            result.CommonPrefixes = obj.CommonPrefixes;
        }

        public static void DeserializeListObjectsV2(
            ref Models.ResultModel baseResult,
            ref OperationOutput output
        ) {
            var serializer = new XmlSerializer(typeof(XmlListBucketResult));
            using var body = output.Body!;
            var obj = serializer.Deserialize(body) as XmlListBucketResult;
            var result = baseResult as Models.ListObjectsV2Result;

            if (obj == null || result == null) {
                return;
            }

            DeserializeEncodingType(ref obj);

            result.Name = obj.Name;
            result.MaxKeys = obj.MaxKeys;
            result.Delimiter = obj.Delimiter;
            result.EncodingType = obj.EncodingType;
            result.StartAfter = obj.StartAfter;
            result.ContinuationToken = obj.ContinuationToken;
            result.NextContinuationToken = obj.NextContinuationToken;
            result.Contents = obj.Contents;
            result.Prefix = obj.Prefix;
            result.IsTruncated = obj.IsTruncated;
            result.KeyCount = obj.KeyCount;
            result.CommonPrefixes = obj.CommonPrefixes;
        }

        private static void DeserializeEncodingType(ref XmlListBucketResult result) {
            if (!string.Equals("url", result.EncodingType)) {
                return;
            }

            if (result.Prefix != null) {
                result.Prefix = result.Prefix.UrlDecode();
            }

            if (result.Marker != null) {
                result.Marker = result.Marker.UrlDecode();
            }

            if (result.NextMarker != null) {
                result.NextMarker = result.NextMarker.UrlDecode();
            }

            if (result.Delimiter != null) {
                result.Delimiter = result.Delimiter.UrlDecode();
            }

            if (result.StartAfter != null) {
                result.StartAfter = result.StartAfter.UrlDecode();
            }

            if (result.ContinuationToken != null) {
                result.ContinuationToken = result.ContinuationToken.UrlDecode();
            }

            if (result.NextContinuationToken != null) {
                result.NextContinuationToken = result.NextContinuationToken.UrlDecode();
            }

            if (result.Contents != null) {
                for (var i = 0; i < result.Contents.Count; i++) {
                    if (result.Contents[i].Key != null) {
                        result.Contents[i].Key = result.Contents[i].Key!.UrlDecode();
                    }
                }
            }

            if (result.CommonPrefixes != null) {
                for (var i = 0; i < result.CommonPrefixes.Count; i++) {
                    if (result.CommonPrefixes[i].Prefix != null) {
                        result.CommonPrefixes[i].Prefix = result.CommonPrefixes[i].Prefix!.UrlDecode();
                    }
                }
            }
        }
    }
}