using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml.Serialization;

namespace AlibabaCloud.OSS.V2.Models {
    /// <summary>
    /// The container that stores the versioning state of the bucket.
    /// </summary>
    [XmlRoot("VersioningConfiguration")]
    public sealed class VersioningConfiguration {
        /// <summary>
        /// The versioning state of the bucket.
        /// Sees <see cref="BucketVersioningStatusType"/> for supported values.
        /// </summary>
        [XmlElement("Status")]
        public string? Status { get; set; }
    }

    /// <summary>
    /// The container that stores the versions of objects, excluding delete markers.
    /// </summary>
    [XmlRoot("ObjectVersion")]
    public sealed class ObjectVersion {
        /// <summary>
        /// The container that stores the information about the bucket owner.
        /// </summary>
        [XmlElement("Owner")]
        public Owner? Owner { get; set; }

        /// <summary>
        /// The name of the object.
        /// </summary>
        [XmlElement("Key")]
        public string? Key { get; set; }

        /// <summary>
        /// Indicates whether the version is the current version. Valid values:*   true: The version is the current version.*   false: The version is a previous version.
        /// </summary>
        [XmlElement("IsLatest")]
        public bool? IsLatest { get; set; }

        /// <summary>
        /// The time when the object was last modified.
        /// </summary>
        [XmlElement("LastModified")]
        public DateTime? LastModified { get; set; }

        /// <summary>
        /// The storage class of the object.
        /// </summary>
        [XmlElement("StorageClass")]
        public string? StorageClass { get; set; }

        /// <summary>
        /// Restore info of the object.
        /// </summary>
        [XmlElement("RestoreInfo")]
        public string? RestoreInfo { get; set; }

        /// <summary>
        /// The time when the Object version is transitioned to cold archive or deep cold archive storage class by the lifecycle.
        /// </summary>
        [XmlElement("TransitionTime")]
        public DateTime? TransitionTime { get; set; }

        /// <summary>
        /// The version ID of the object.
        /// </summary>
        [XmlElement("VersionId")]
        public string? VersionId { get; set; }

        /// <summary>
        /// The ETag that is generated when an object is created. ETags are used to identify the content of objects.*   If an object is created by calling the PutObject operation, the ETag of the object is the MD5 hash of the object content.*   If an object is created by using another method, the ETag is not the MD5 hash of the object content but a unique value that is calculated based on a specific rule.  The ETag of an object can be used only to check whether the object content is modified. However, we recommend that you use the MD5 hash of an object rather than the ETag of the object to verify data integrity.
        /// </summary>
        [XmlElement("ETag")]
        public string? ETag { get; set; }

        /// <summary>
        /// The size of the object. Unit: bytes.
        /// </summary>
        [XmlElement("Size")]
        public long? Size { get; set; }

        /// <summary>
        /// The type of the object. An object has one of the following types:*   Normal: The object is created by using simple upload.*   Multipart: The object is created by using multipart upload.*   Appendable: The object is created by using append upload. An appendable object can be appended.
        /// </summary>
        [XmlElement("Type")]
        public string? Type { get; set; }
    }

    /// <summary>
    /// The container that stores delete markers.
    /// </summary>
    [XmlRoot("DeleteMarkerEntry")]
    public sealed class DeleteMarkerEntry {
        /// <summary>
        /// Indicates whether the version is the current version. Valid values:*   true: The version is the current version.*   false: The version is a previous version.
        /// </summary>
        [XmlElement("IsLatest")]
        public bool? IsLatest { get; set; }

        /// <summary>
        /// The time when the object was last modified.
        /// </summary>
        [XmlElement("LastModified")]
        public DateTime? LastModified { get; set; }

        /// <summary>
        /// The container that stores the information about the bucket owner.
        /// </summary>
        [XmlElement("Owner")]
        public Owner? Owner { get; set; }

        /// <summary>
        /// The name of the object.
        /// </summary>
        [XmlElement("Key")]
        public string? Key { get; set; }

        /// <summary>
        /// The version ID of the object.
        /// </summary>
        [XmlElement("VersionId")]
        public string? VersionId { get; set; }
    }

    /// <summary>
    /// The request for the PutBucketVersioning operation.
    /// </summary>
    public sealed class PutBucketVersioningRequest : RequestModel {
        public PutBucketVersioningRequest() {
            BodyFormat = "xml";
        }

        /// <summary>
        /// The name of the bucket.
        /// </summary>
        public string? Bucket { get; set; }

        /// <summary>
        /// The container of the request body.
        /// </summary>
        public Models.VersioningConfiguration? VersioningConfiguration {
            get => InnerBody as Models.VersioningConfiguration;
            set => InnerBody = value;
        }
    }

    /// <summary>
    /// The result for the PutBucketVersioning operation.
    /// </summary>
    public sealed class PutBucketVersioningResult : ResultModel { }

    /// <summary>
    /// The request for the GetBucketVersioning operation.
    /// </summary>
    public sealed class GetBucketVersioningRequest : RequestModel {
        /// <summary>
        /// The name of the bucket.
        /// </summary>
        public string? Bucket { get; set; }
    }

    /// <summary>
    /// The result for the GetBucketVersioning operation.
    /// </summary>
    public sealed class GetBucketVersioningResult : ResultModel {
        /// <summary>
        /// The container that stores the versioning state of the bucket.
        /// </summary>
        public Models.VersioningConfiguration? VersioningConfiguration => InnerBody as Models.VersioningConfiguration;

        public GetBucketVersioningResult() {
            BodyFormat = "xml";
            BodyType = typeof(Models.VersioningConfiguration);
        }
    }

    /// <summary>
    /// The request for the ListObjectVersions operation.
    /// </summary>
    public sealed class ListObjectVersionsRequest : RequestModel {
        /// <summary>
        /// The name of the bucket.
        /// </summary>
        public string? Bucket { get; set; }

        /// <summary>
        /// The character that is used to group objects by name. If you specify prefix and delimiter in the request, the response contains CommonPrefixes. The objects whose name contains the same string from the prefix to the next occurrence of the delimiter are grouped as a single result element in CommonPrefixes. If you specify prefix and set delimiter to a forward slash (/), only the objects in the directory are listed. The subdirectories in the directory are returned in CommonPrefixes. Objects and subdirectories in the subdirectories are not listed.By default, this parameter is left empty.
        /// </summary>
        public string? Delimiter {
            get => Parameters.TryGetValue("delimiter", out var value) ? value : null;
            set {
                if (value != null) Parameters["delimiter"] = value;
            }
        }

        /// <summary>
        /// The name of the object after which the GetBucketVersions (ListObjectVersions) operation begins. If this parameter is specified, objects whose name is alphabetically after the value of key-marker are returned. Use key-marker and version-id-marker in combination. The value of key-marker must be less than 1,024 bytes in length.By default, this parameter is left empty.  You must also specify key-marker if you specify version-id-marker.
        /// </summary>
        public string? KeyMarker {
            get => Parameters.TryGetValue("key-marker", out var value) ? value : null;
            set {
                if (value != null) Parameters["key-marker"] = value;
            }
        }

        /// <summary>
        /// The version ID of the object specified in key-marker after which the GetBucketVersions (ListObjectVersions) operation begins. The versions are returned from the latest version to the earliest version. If version-id-marker is not specified, the GetBucketVersions (ListObjectVersions) operation starts from the latest version of the object whose name is alphabetically after the value of key-marker by default.By default, this parameter is left empty.Valid values: version IDs.
        /// </summary>
        public string? VersionIdMarker {
            get => Parameters.TryGetValue("version-id-marker", out var value) ? value : null;
            set {
                if (value != null) Parameters["version-id-marker"] = value;
            }
        }

        /// <summary>
        /// The maximum number of objects to be returned. If the number of returned objects exceeds the value of max-keys, the response contains `NextKeyMarker` and `NextVersionIdMarker`. Specify the values of `NextKeyMarker` and `NextVersionIdMarker` as the markers for the next request. Valid values: 1 to 999. Default value: 100.
        /// </summary>
        public long? MaxKeys {
            get => Parameters.TryGetValue("max-keys", out var value)
                ? Convert.ToInt64(value, CultureInfo.InvariantCulture)
                : null;
            set {
                if (value != null) Parameters["max-keys"] = Convert.ToString((long)value, CultureInfo.InvariantCulture);
            }
        }

        /// <summary>
        /// The prefix that the names of returned objects must contain.*   The value of prefix must be less than 1,024 bytes in length.*   If you specify prefix, the names of the returned objects contain the prefix.If you set prefix to a directory name, the objects whose name starts with the prefix are listed. The returned objects consist of all objects and subdirectories in the directory.By default, this parameter is left empty.
        /// </summary>
        public string? Prefix {
            get => Parameters.TryGetValue("prefix", out var value) ? value : null;
            set {
                if (value != null) Parameters["prefix"] = value;
            }
        }

        /// <summary>
        /// The encoding type of the content in the response. By default, this parameter is left empty. Set the value to URL.  The values of Delimiter, Marker, Prefix, NextMarker, and Key are UTF-8 encoded. If the value of Delimiter, Marker, Prefix, NextMarker, or Key contains a control character that is not supported by Extensible Markup Language (XML) 1.0, you can specify encoding-type to encode the value in the response.
        /// </summary>
        public string? EncodingType {
            get => Parameters.TryGetValue("encoding-type", out var value) ? value : null;
            set {
                if (value != null) Parameters["encoding-type"] = value;
            }
        }
    }

    /// <summary>
    /// The result for the ListObjectVersions operation.
    /// </summary>
    public sealed class ListObjectVersionsResult : ResultModel {
        /// <summary>
        /// Indicates whether the returned results are truncated.- true: indicates that not all results are returned for the request.- false: indicates that all results are returned for the request.
        /// </summary>
        public bool? IsTruncated { get; set; }

        /// <summary>
        /// If not all results are returned for the request, the NextVersionIdMarker parameter is included in the response to indicate the version-id-marker value of the next ListObjectVersions (GetBucketVersions) request.
        /// </summary>
        public string? NextVersionIdMarker { get; set; }

        /// <summary>
        /// The container that stores the versions of objects except for delete markers
        /// </summary>
        public IList<ObjectVersion>? Versions { get; set; }

        /// <summary>
        /// The container that stores delete markers
        /// </summary>
        public IList<DeleteMarkerEntry>? DeleteMarkers { get; set; }

        /// <summary>
        /// Objects whose names contain the same string that ranges from the prefix to the next occurrence of the delimiter are grouped as a single result element
        /// </summary>
        public IList<CommonPrefix>? CommonPrefixes { get; set; }

        /// <summary>
        /// The prefix contained in the names of the returned objects.
        /// </summary>
        public string? Prefix { get; set; }

        /// <summary>
        /// The character that is used to group objects by name. The objects whose names contain the same string from the prefix to the next occurrence of the delimiter are grouped as a single result parameter in CommonPrefixes.
        /// </summary>
        public string? Delimiter { get; set; }

        /// <summary>
        /// The version from which the ListObjectVersions (GetBucketVersions) operation starts. This parameter is used together with KeyMarker.
        /// </summary>
        public string? VersionIdMarker { get; set; }

        /// <summary>
        /// The maximum number of objects that can be returned in the response.
        /// </summary>
        public int? MaxKeys { get; set; }

        /// <summary>
        /// The encoding type of the content in the response. If you specify encoding-type in the request, the values of Delimiter, Marker, Prefix, NextMarker, and Key are encoded in the response.
        /// </summary>
        public string? EncodingType { get; set; }

        /// <summary>
        /// If not all results are returned for the request, the NextKeyMarker parameter is included in the response to indicate the key-marker value of the next ListObjectVersions (GetBucketVersions) request.
        /// </summary>
        public string? NextKeyMarker { get; set; }

        /// <summary>
        /// The bucket name
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Indicates the object from which the ListObjectVersions (GetBucketVersions) operation starts.
        /// </summary>
        public string? KeyMarker { get; set; }
    }
}