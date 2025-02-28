using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using AlibabaCloud.OSS.V2.Extensions;
using AlibabaCloud.OSS.V2.Models;

namespace AlibabaCloud.OSS.V2.Transform {
    /// <summary>
    /// The container that stores the results of the ListObjectVersions (GetBucketVersions) request.
    /// </summary>
    [XmlRoot("ListVersionsResult")]
    public sealed class XmlListVersionsResult {
        /// <summary>
        /// Indicates whether the returned results are truncated.- true: indicates that not all results are returned for the request.- false: indicates that all results are returned for the request.
        /// </summary>
        [XmlElement("IsTruncated")]
        public bool? IsTruncated { get; set; }

        /// <summary>
        /// If not all results are returned for the request, the NextVersionIdMarker parameter is included in the response to indicate the version-id-marker value of the next ListObjectVersions (GetBucketVersions) request.
        /// </summary>
        [XmlElement("NextVersionIdMarker")]
        public string? NextVersionIdMarker { get; set; }

        /// <summary>
        /// The container that stores the versions of objects except for delete markers
        /// </summary>
        [XmlElement("Version")]
        public List<ObjectVersion>? Versions { get; set; }

        /// <summary>
        /// The container that stores delete markers
        /// </summary>
        [XmlElement("DeleteMarker")]
        public List<DeleteMarkerEntry>? DeleteMarkers { get; set; }

        /// <summary>
        /// Objects whose names contain the same string that ranges from the prefix to the next occurrence of the delimiter are grouped as a single result element
        /// </summary>
        [XmlElement("CommonPrefixes")]
        public List<CommonPrefix>? CommonPrefixes { get; set; }

        /// <summary>
        /// The prefix contained in the names of the returned objects.
        /// </summary>
        [XmlElement("Prefix")]
        public string? Prefix { get; set; }

        /// <summary>
        /// The character that is used to group objects by name. The objects whose names contain the same string from the prefix to the next occurrence of the delimiter are grouped as a single result parameter in CommonPrefixes.
        /// </summary>
        [XmlElement("Delimiter")]
        public string? Delimiter { get; set; }

        /// <summary>
        /// The version from which the ListObjectVersions (GetBucketVersions) operation starts. This parameter is used together with KeyMarker.
        /// </summary>
        [XmlElement("VersionIdMarker")]
        public string? VersionIdMarker { get; set; }

        /// <summary>
        /// The maximum number of objects that can be returned in the response.
        /// </summary>
        [XmlElement("MaxKeys")]
        public int? MaxKeys { get; set; }

        /// <summary>
        /// The encoding type of the content in the response. If you specify encoding-type in the request, the values of Delimiter, Marker, Prefix, NextMarker, and Key are encoded in the response.
        /// </summary>
        [XmlElement("EncodingType")]
        public string? EncodingType { get; set; }

        /// <summary>
        /// If not all results are returned for the request, the NextKeyMarker parameter is included in the response to indicate the key-marker value of the next ListObjectVersions (GetBucketVersions) request.
        /// </summary>
        [XmlElement("NextKeyMarker")]
        public string? NextKeyMarker { get; set; }

        /// <summary>
        /// The bucket name
        /// </summary>
        [XmlElement("Name")]
        public string? Name { get; set; }

        /// <summary>
        /// Indicates the object from which the ListObjectVersions (GetBucketVersions) operation starts.
        /// </summary>
        [XmlElement("KeyMarker")]
        public string? KeyMarker { get; set; }
    }

    [XmlRoot("VersioningConfiguration")]
    public sealed class XmlVersioningConfiguration {
        /// <summary>
        /// The versioning state of the bucket.
        /// Sees <see cref="BucketVersioningStatusType"/> for supported values.
        /// </summary>
        [XmlElement("Status")]
        public string? Status { get; set; }
    }


    internal static partial class Serde {
        public static void DeserializeListObjectVersions(
            ref Models.ResultModel baseResult,
            ref OperationOutput output
        ) {
            var serializer = new XmlSerializer(typeof(XmlListVersionsResult));
            using var body = output.Body!;
            var obj = serializer.Deserialize(body) as XmlListVersionsResult;
            var result = baseResult as Models.ListObjectVersionsResult;

            if (obj == null || result == null) {
                return;
            }

            DeserializeVersionsEncodingType(ref obj);

            result.Name = obj.Name;
            result.MaxKeys = obj.MaxKeys;
            result.Delimiter = obj.Delimiter;
            result.EncodingType = obj.EncodingType;
            result.KeyMarker = obj.KeyMarker;
            result.VersionIdMarker = obj.VersionIdMarker;
            result.NextKeyMarker = obj.NextKeyMarker;
            result.NextVersionIdMarker = obj.NextVersionIdMarker;
            result.Versions = obj.Versions;
            result.Prefix = obj.Prefix;
            result.IsTruncated = obj.IsTruncated;
            result.CommonPrefixes = obj.CommonPrefixes;
            result.DeleteMarkers = obj.DeleteMarkers;
        }

        private static void DeserializeVersionsEncodingType(ref XmlListVersionsResult result) {
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

            if (result.Versions != null) {
                for (int i = 0; i < result.Versions.Count; i++) {
                    if (result.Versions[i].Key != null) {
                        result.Versions[i].Key = result.Versions[i].Key!.UrlDecode();
                    }
                }
            }

            if (result.DeleteMarkers != null) {
                for (int i = 0; i < result.DeleteMarkers.Count; i++) {
                    if (result.DeleteMarkers[i].Key != null) {
                        result.DeleteMarkers[i].Key = result.DeleteMarkers[i].Key!.UrlDecode();
                    }
                }
            }

            if (result.CommonPrefixes != null) {
                for (int i = 0; i < result.CommonPrefixes.Count; i++) {
                    if (result.CommonPrefixes[i].Prefix != null) {
                        result.CommonPrefixes[i].Prefix = result.CommonPrefixes[i].Prefix!.UrlDecode();
                    }
                }
            }
        }

        public static void DeserializeGetBucketVersioning(
        ref Models.ResultModel baseResult,
        ref OperationOutput output
    ) {
            using var body = output.Body!;
            var result = baseResult as Models.GetBucketVersioningResult;
            if (result == null) {
                return;
            }
            try {
                var serializer = new XmlSerializer(typeof(XmlVersioningConfiguration));
                var obj = serializer.Deserialize(body) as XmlVersioningConfiguration;

                if (obj == null) {
                    return;
                }

                result.InnerBody = new Models.VersioningConfiguration() {
                    Status = obj.Status
                };
            }
            catch (InvalidOperationException) {
                if (!body.CanSeek) {
                    throw;
                }
                body.Seek(0, SeekOrigin.Begin);
                var xmlDoc = new XmlDocument();
                xmlDoc.Load(body);
                if (xmlDoc.FirstChild != null && !string.Equals("VersioningConfiguration", xmlDoc.FirstChild.Name)) {
                    throw;
                }
                var node = xmlDoc.SelectSingleNode("/VersioningConfiguration/Status");
                result.InnerBody = new Models.VersioningConfiguration() {
                    Status = node?.InnerText
                };
            }
        }
    }
}