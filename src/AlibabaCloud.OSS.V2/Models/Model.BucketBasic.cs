using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml.Serialization;

namespace AlibabaCloud.OSS.V2.Models {
    /// <summary>
    /// The container that stores all information returned for the GetBucketStat request.
    /// </summary>
    [XmlRoot("BucketStat")]
    public sealed class BucketStat {
        /// <summary>
        /// The number of multipart parts in the bucket.
        /// </summary>
        [XmlElement("MultipartPartCount")]
        public long? MultipartPartCount { get; set; }

        /// <summary>
        /// The actual storage usage of Cold Archive objects in the bucket. Unit: bytes.
        /// </summary>
        [XmlElement("ColdArchiveRealStorage")]
        public long? ColdArchiveRealStorage { get; set; }

        /// <summary>
        /// The actual storage usage of Deep Cold Archive objects in the bucket. Unit: bytes.
        /// </summary>
        [XmlElement("DeepColdArchiveRealStorage")]
        public long? DeepColdArchiveRealStorage { get; set; }

        /// <summary>
        /// The total number of objects in the bucket.
        /// </summary>
        [XmlElement("ObjectCount")]
        public long? ObjectCount { get; set; }

        /// <summary>
        /// The number of multipart upload tasks that have been initiated but are not completed or canceled.
        /// </summary>
        [XmlElement("MultipartUploadCount")]
        public long? MultipartUploadCount { get; set; }

        /// <summary>
        /// The actual storage usage of IA objects in the bucket. Unit: bytes.
        /// </summary>
        [XmlElement("InfrequentAccessRealStorage")]
        public long? InfrequentAccessRealStorage { get; set; }

        /// <summary>
        /// The actual storage usage of Archive objects in the bucket. Unit: bytes.
        /// </summary>
        [XmlElement("ArchiveRealStorage")]
        public long? ArchiveRealStorage { get; set; }

        /// <summary>
        /// The billed storage usage of Cold Archive objects in the bucket. Unit: bytes.
        /// </summary>
        [XmlElement("ColdArchiveStorage")]
        public long? ColdArchiveStorage { get; set; }

        /// <summary>
        /// The number of Cold Archive objects in the bucket.
        /// </summary>
        [XmlElement("ColdArchiveObjectCount")]
        public long? ColdArchiveObjectCount { get; set; }

        /// <summary>
        /// The storage usage of the bucket. Unit: bytes.
        /// </summary>
        [XmlElement("Storage")]
        public long? Storage { get; set; }

        /// <summary>
        /// The number of delete marker in the bucket.
        /// </summary>
        [XmlElement("DeleteMarkerCount")]
        public long? DeleteMarkerCount { get; set; }

        /// <summary>
        /// The number of Standard objects in the bucket.
        /// </summary>
        [XmlElement("StandardObjectCount")]
        public long? StandardObjectCount { get; set; }

        /// <summary>
        /// The billed storage usage of IA objects in the bucket. Unit: bytes.
        /// </summary>
        [XmlElement("InfrequentAccessStorage")]
        public long? InfrequentAccessStorage { get; set; }

        /// <summary>
        /// The number of Archive objects in the bucket.
        /// </summary>
        [XmlElement("ArchiveObjectCount")]
        public long? ArchiveObjectCount { get; set; }

        /// <summary>
        /// The billed storage usage of Deep Cold Archive objects in the bucket. Unit: bytes.
        /// </summary>
        [XmlElement("DeepColdArchiveStorage")]
        public long? DeepColdArchiveStorage { get; set; }

        /// <summary>
        /// The number of Deep Cold Archive objects in the bucket.
        /// </summary>
        [XmlElement("DeepColdArchiveObjectCount")]
        public long? DeepColdArchiveObjectCount { get; set; }

        /// <summary>
        /// The number of LiveChannels in the bucket.
        /// </summary>
        [XmlElement("LiveChannelCount")]
        public long? LiveChannelCount { get; set; }

        /// <summary>
        /// The storage usage of Standard objects in the bucket. Unit: bytes.
        /// </summary>
        [XmlElement("StandardStorage")]
        public long? StandardStorage { get; set; }

        /// <summary>
        /// The billed storage usage of Archive objects in the bucket. Unit: bytes.
        /// </summary>
        [XmlElement("ArchiveStorage")]
        public long? ArchiveStorage { get; set; }

        /// <summary>
        /// The time when the obtained information was last modified. The value of this parameter is a UNIX timestamp. Unit: seconds.
        /// </summary>
        [XmlElement("LastModifiedTime")]
        public long? LastModifiedTime { get; set; }

        /// <summary>
        /// The number of IA objects in the bucket.
        /// </summary>
        [XmlElement("InfrequentAccessObjectCount")]
        public long? InfrequentAccessObjectCount { get; set; }
    }

    /// <summary>
    /// The character that is used to group the objects that you want to list by name. Objects whose names contain the same string that stretches from the specified prefix to the first occurrence of the delimiter are grouped as a CommonPrefixes element.
    /// </summary>
    [XmlRoot("CommonPrefix")]
    public sealed class CommonPrefix {
        /// <summary>
        /// The prefix that must be included in the names of objects you want to list.
        /// </summary>
        [XmlElement("Prefix")]
        public string? Prefix { get; set; }
    }

    /// <summary>
    /// The server-side encryption configurations of the bucket.
    /// </summary>
    [XmlRoot("ServerSideEncryptionRule")]
    public sealed class ServerSideEncryptionRule {
        /// <summary>
        /// The default server-side encryption method.Valid values: KMS, AES-256, and SM4.
        /// </summary>
        [XmlElement("SSEAlgorithm")]
        public string? SSEAlgorithm { get; set; }

        /// <summary>
        /// The key that is managed by Key Management Service (KMS).
        /// </summary>
        [XmlElement("KMSMasterKeyID")]
        public string? KMSMasterKeyID { get; set; }

        /// <summary>
        /// The algorithm that is used to encrypt objects. If you do not configure this parameter, objects are encrypted by using AES-256. This parameter is valid only when SSEAlgorithm is set to KMS.Valid value: SM4.
        /// </summary>
        [XmlElement("KMSDataEncryption")]
        public string? KMSDataEncryption { get; set; }
    }

    /// <summary>
    /// The log configurations of the bucket.
    /// </summary>
    [XmlRoot("BucketPolicy")]
    public sealed class BucketPolicy {
        /// <summary>
        /// The name of the bucket that stores the logs.
        /// </summary>
        [XmlElement("LogBucket")]
        public string? LogBucket { get; set; }

        /// <summary>
        /// The directory in which logs are stored.
        /// </summary>
        [XmlElement("LogPrefix")]
        public string? LogPrefix { get; set; }
    }

    /// <summary>
    /// The configurations of the bucket storage class and redundancy type.
    /// </summary>
    [XmlRoot("CreateBucketConfiguration")]
    public sealed class CreateBucketConfiguration {
        /// <summary>
        /// The storage class of the bucket. Valid values:*   Standard (default)*   IA*   Archive*   ColdArchive
        /// Sees <see cref="StorageClassType"/> for supported values.
        /// </summary>
        [XmlElement("StorageClass")]
        public string? StorageClass { get; set; }

        /// <summary>
        /// The redundancy type of the bucket.*   LRS (default)    LRS stores multiple copies of your data on multiple devices in the same zone. LRS ensures data durability and availability even if hardware failures occur on two devices.*   ZRS    ZRS stores multiple copies of your data across three zones in the same region. Even if a zone becomes unavailable due to unexpected events, such as power outages and fires, data can still be accessed.  You cannot set the redundancy type of Archive buckets to ZRS.
        /// Sees <see cref="Models.DataRedundancyType"/> for supported values.
        /// </summary>
        [XmlElement("DataRedundancyType")]
        public string? DataRedundancyType { get; set; }
    }

    /// <summary>
    /// The container that stores the bucket information.
    /// </summary>
    [XmlRoot("Bucket")]
    public sealed class BucketInfo {
        /// <summary>
        /// The region in which the bucket is located.
        /// </summary>
        [XmlElement("Location")]
        public string? Location { get; set; }

        /// <summary>
        /// Indicates whether transfer acceleration is enabled for the bucket.Valid values:*   Enabled            *   Disabled            
        /// </summary>
        [XmlElement("TransferAcceleration")]
        public string? TransferAcceleration { get; set; }

        /// <summary>
        /// The versioning status of the bucket.
        /// </summary>
        [XmlElement("Versioning")]
        public string? Versioning { get; set; }

        /// <summary>
        /// The ACL of the bucket.
        /// </summary>
        [XmlElement("AccessControlList")]
        public AccessControlList? AccessControlList { get; set; }

        /// <summary>
        /// The server-side encryption configurations of the bucket.
        /// </summary>
        [XmlElement("ServerSideEncryptionRule")]
        public ServerSideEncryptionRule? ServerSideEncryptionRule { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [XmlElement("BlockPublicAccess")]
        public bool? BlockPublicAccess { get; set; }

        /// <summary>
        /// Indicates whether cross-region replication (CRR) is enabled for the bucket.Valid values:*   Enabled            *   Disabled            
        /// </summary>
        [XmlElement("CrossRegionReplication")]
        public string? CrossRegionReplication { get; set; }

        /// <summary>
        /// The redundancy type of the bucket.
        /// </summary>
        [XmlElement("DataRedundancyType")]
        public string? DataRedundancyType { get; set; }

        /// <summary>
        /// The log configurations of the bucket.
        /// </summary>
        [XmlElement("BucketPolicy")]
        public BucketPolicy? BucketPolicy { get; set; }

        /// <summary>
        /// The description of the bucket.
        /// </summary>
        [XmlElement("Comment")]
        public string? Comment { get; set; }

        /// <summary>
        /// The time when the bucket is created. The time is in UTC.
        /// </summary>
        [XmlElement("CreationDate")]
        public DateTime? CreationDate { get; set; }

        /// <summary>
        /// The internal endpoint of the bucket.
        /// </summary>
        [XmlElement("IntranetEndpoint")]
        public string? IntranetEndpoint { get; set; }

        /// <summary>
        /// The name of the bucket.
        /// </summary>
        [XmlElement("Name")]
        public string? Name { get; set; }

        /// <summary>
        /// The ID of the resource group to which the bucket belongs.
        /// </summary>
        [XmlElement("ResourceGroupId")]
        public string? ResourceGroupId { get; set; }

        /// <summary>
        /// The storage class of the bucket.
        /// </summary>
        [XmlElement("StorageClass")]
        public string? StorageClass { get; set; }

        /// <summary>
        /// The owner of the bucket.
        /// </summary>
        [XmlElement("Owner")]
        public Owner? Owner { get; set; }

        /// <summary>
        /// Indicates whether access tracking is enabled for the bucket.Valid values:*   Enabled            *   Disabled            
        /// </summary>
        [XmlElement("AccessMonitor")]
        public string? AccessMonitor { get; set; }

        /// <summary>
        /// The public endpoint of the bucket.
        /// </summary>
        [XmlElement("ExtranetEndpoint")]
        public string? ExtranetEndpoint { get; set; }
    }

    /// <summary>
    /// The container that stores the information about the bucket.
    /// </summary>
    [XmlRoot("BucketInfo")]
    public sealed class XmlBucketInfo {
        /// <summary>
        /// The container that stores the bucket information.
        /// </summary>
        [XmlElement("Bucket")]
        public BucketInfo? Bucket { get; set; }
    }

    /// <summary>
    /// The container that stores the information about the bucket.
    /// </summary>
    [XmlRoot("LocationConstraint")]
    public sealed class XmlLocationConstraint {
        /// <summary>
        /// The container that stores the bucket location.
        /// </summary>
        [XmlText()]
        public string? Text { get; set; }
    }

    /// <summary>
    /// The container that stores the returned object metadata.
    /// </summary>
    //[XmlRoot("Contents")]
    public sealed class ObjectSummary {
        /// <summary>
        /// The size of the object. Unit: bytes.
        /// </summary>
        [XmlElement("Size")]
        public long? Size { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [XmlElement("TransitionTime")]
        public DateTime? TransitionTime { get; set; }

        /// <summary>
        /// The name of the object.
        /// </summary>
        [XmlElement("Key")]
        public string? Key { get; set; }

        /// <summary>
        /// The time when the object was last modified.
        /// </summary>
        [XmlElement("LastModified")]
        public DateTime? LastModified { get; set; }

        /// <summary>
        /// The entity tag (ETag). An ETag is created when the object is created to identify the content of an object.*   For an object that is created by calling the PutObject operation, the ETag value of the object is the MD5 hash of the object content.*   For an object that is created by using another method, the ETag value is not the MD5 hash of the object content but a unique value calculated based on a specific rule.*   The ETag of an object can be used to check whether the object content changes. However, we recommend that you use the MD5 hash of an object rather than the ETag value of the object to verify data integrity.
        /// </summary>
        [XmlElement("ETag")]
        public string? ETag { get; set; }

        /// <summary>
        /// The type of the object. An object has one of the following types:*   Normal: The object is created by using simple upload.*   Multipart: The object is created by using multipart upload.*   Appendable: The object is created by using append upload. An appendable object can be appended.
        /// </summary>
        [XmlElement("Type")]
        public string? Type { get; set; }

        /// <summary>
        /// The storage class of the object.
        /// </summary>
        [XmlElement("StorageClass")]
        public string? StorageClass { get; set; }

        /// <summary>
        /// The container that stores the information about the bucket owner.
        /// </summary>
        [XmlElement("Owner")]
        public Owner? Owner { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [XmlElement("RestoreInfo")]
        public string? RestoreInfo { get; set; }
    }

    /// <summary>
    /// The request for the GetBucketStat operation.
    /// </summary>
    public sealed class GetBucketStatRequest : RequestModel {
        /// <summary>
        /// The bucket about which you want to query the information.
        /// </summary>
        public string? Bucket { get; set; }
    }

    /// <summary>
    /// The result for the GetBucketStat operation.
    /// </summary>
    public sealed class GetBucketStatResult : ResultModel {
        /// <summary>
        /// The container that stores all information returned for the GetBucketStat request.
        /// </summary>
        public BucketStat? BucketStat => InnerBody as BucketStat;

        public GetBucketStatResult() {
            BodyFormat = "xml";
            BodyType   = typeof(BucketStat);
        }
    }

    /// <summary>
    /// The request for the PutBucket operation.
    /// </summary>
    public sealed class PutBucketRequest : RequestModel {
        public PutBucketRequest() {
            BodyFormat = "xml";
        }

        /// <summary>
        /// The name of the bucket. The name of a bucket must comply with the following naming conventions:*   The name can contain only lowercase letters, digits, and hyphens (-).*   It must start and end with a lowercase letter or a digit.*   The name must be 3 to 63 characters in length.
        /// </summary>
        public string? Bucket { get; set; }

        /// <summary>
        /// The access control list (ACL) of the bucket to be created. Valid values:*   public-read-write*   public-read*   private (default)For more information, see [Bucket ACL](~~31843~~).
        /// </summary>
        public string? Acl {
            get => Headers.TryGetValue("x-oss-acl", out var value) ? value : null;
            set {
                if (value != null) Headers["x-oss-acl"] = value;
            }
        }

        /// <summary>
        /// The ID of the resource group.*   If you include the header in the request and specify the ID of the resource group, the bucket that you create belongs to the resource group. If the specified resource group ID is rg-default-id, the bucket that you create belongs to the default resource group.*   If you do not include the header in the request, the bucket that you create belongs to the default resource group.You can obtain the ID of a resource group in the Resource Management console or by calling the ListResourceGroups operation. For more information, see [View basic information of a resource group](~~151181~~) and [ListResourceGroups](~~158855~~).  You cannot configure a resource group for an Anywhere Bucket.
        /// </summary>
        public string? ResourceGroupId {
            get => Headers.TryGetValue("x-oss-resource-group-id", out var value) ? value : null;
            set {
                if (value != null) Headers["x-oss-resource-group-id"] = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string? BucketTagging {
            get => Headers.TryGetValue("x-oss-bucket-tagging", out var value) ? value : null;
            set {
                if (value != null) Headers["x-oss-bucket-tagging"] = value;
            }
        }

        /// <summary>
        /// The container that stores the request body.
        /// </summary>
        public CreateBucketConfiguration? CreateBucketConfiguration {
            get => InnerBody as CreateBucketConfiguration;
            set => InnerBody = value;
        }
    }

    /// <summary>
    /// The result for the PutBucket operation.
    /// </summary>
    public sealed class PutBucketResult : ResultModel { }

    /// <summary>
    /// The request for the DeleteBucket operation.
    /// </summary>
    public sealed class DeleteBucketRequest : RequestModel {
        /// <summary>
        /// The name of the bucket.
        /// </summary>
        public string? Bucket { get; set; }
    }

    /// <summary>
    /// The result for the DeleteBucket operation.
    /// </summary>
    public sealed class DeleteBucketResult : ResultModel { }

    /// <summary>
    /// The request for the ListObjects operation.
    /// </summary>
    public sealed class ListObjectsRequest : RequestModel {
        /// <summary>
        /// The name of the bucket.
        /// </summary>
        public string? Bucket { get; set; }

        /// <summary>
        /// The character that is used to group objects by name. If you specify delimiter in the request, the response contains CommonPrefixes. The objects whose names contain the same string from the prefix to the next occurrence of the delimiter are grouped as a single result element in CommonPrefixes.
        /// </summary>
        public string? Delimiter {
            get => Parameters.TryGetValue("delimiter", out var value) ? value : null;
            set {
                if (value != null) Parameters["delimiter"] = value;
            }
        }

        /// <summary>
        /// The name of the object after which the GetBucket (ListObjects) operation begins. If this parameter is specified, objects whose names are alphabetically after the value of marker are returned.The objects are returned by page based on marker. The value of marker can be up to 1,024 bytes.If the value of marker does not exist in the list when you perform a conditional query, the GetBucket (ListObjects) operation starts from the object whose name is alphabetically after the value of marker.
        /// </summary>
        public string? Marker {
            get => Parameters.TryGetValue("marker", out var value) ? value : null;
            set {
                if (value != null) Parameters["marker"] = value;
            }
        }

        /// <summary>
        /// The maximum number of objects that can be returned. If the number of objects to be returned exceeds the value of max-keys specified in the request, NextMarker is included in the returned response. The value of NextMarker is used as the value of marker for the next request.Valid values: 1 to 999.Default value: 100.
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
        /// The prefix that must be contained in names of the returned objects.*   The value of prefix can be up to 1,024 bytes in length.*   If you specify prefix, the names of the returned objects contain the prefix.If you set prefix to a directory name, the object whose names start with this prefix are listed. The objects consist of all recursive objects and subdirectories in this directory.If you set prefix to a directory name and set delimiter to a forward slash (/), only the objects in the directory are listed. The subdirectories in the directory are listed in CommonPrefixes. Recursive objects and subdirectories in the subdirectories are not listed.For example, a bucket contains the following three objects: fun/test.jpg, fun/movie/001.avi, and fun/movie/007.avi. If prefix is set to fun/, the three objects are returned. If prefix is set to fun/ and delimiter is set to a forward slash (/), fun/test.jpg and fun/movie/ are returned.
        /// </summary>
        public string? Prefix {
            get => Parameters.TryGetValue("prefix", out var value) ? value : null;
            set {
                if (value != null) Parameters["prefix"] = value;
            }
        }

        /// <summary>
        /// The encoding format of the content in the response.  The value of Delimiter, Marker, Prefix, NextMarker, and Key are UTF-8 encoded. If the values of Delimiter, Marker, Prefix, NextMarker, and Key contain a control character that is not supported by Extensible Markup Language (XML) 1.0, you can specify encoding-type to encode the value in the response.
        /// Sees <see cref="Models.EncodingType"/> for supported values.
        /// </summary>
        public string? EncodingType {
            get => Parameters.TryGetValue("encoding-type", out var value) ? value : null;
            set {
                if (value != null) Parameters["encoding-type"] = value;
            }
        }
    }

    /// <summary>
    /// The result for the ListObjects operation.
    /// </summary>
    public sealed class ListObjectsResult : ResultModel {
        /// <summary>
        /// The name of the bucket.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// The maximum number of returned objects in the response.
        /// </summary>
        public int? MaxKeys { get; set; }

        /// <summary>
        /// The character that is used to group objects by name. The objects whose names contain the same string from the prefix to the next occurrence of the delimiter are grouped as a single result element in CommonPrefixes.
        /// </summary>
        public string? Delimiter { get; set; }

        /// <summary>
        /// The encoding type of the content in the response. If you specify encoding-type in the request, the values of Delimiter, StartAfter, Prefix, NextContinuationToken, and Key are encoded in the response.
        /// </summary>
        public string? EncodingType { get; set; }

        /// <summary>
        /// The name of the object after which the list operation begins.
        /// For ListObjects
        /// </summary>
        public string? Marker { get; set; }

        /// <summary>
        /// The position from which the next list operation starts.
        /// For ListObjects
        /// </summary>
        public string? NextMarker { get; set; }

        /// <summary>
        /// The container that stores the metadata of the returned objects.
        /// </summary>
        public IList<ObjectSummary>? Contents { get; set; }

        /// <summary>
        /// The prefix in the names of the returned objects.
        /// </summary>
        public string? Prefix { get; set; }

        /// <summary>
        /// Indicates whether the returned results are truncated. Valid values:- true- false
        /// </summary>
        public bool? IsTruncated { get; set; }

        /// <summary>
        /// Objects whose names contain the same string that ranges from the prefix to the next occurrence of the delimiter are grouped as a single result element
        /// </summary>
        public IList<CommonPrefix>? CommonPrefixes { get; set; }
    }

    /// <summary>
    /// The request for the ListObjectsV2 operation.
    /// </summary>
    public sealed class ListObjectsV2Request : RequestModel {
        /// <summary>
        /// The name of the bucket.
        /// </summary>
        public string? Bucket { get; set; }

        /// <summary>
        /// The character that is used to group objects by name. If you specify delimiter in the request, the response contains CommonPrefixes. The objects whose names contain the same string from the prefix to the next occurrence of the delimiter are grouped as a single result element in CommonPrefixes.
        /// </summary>
        public string? Delimiter {
            get => Parameters.TryGetValue("delimiter", out var value) ? value : null;
            set {
                if (value != null) Parameters["delimiter"] = value;
            }
        }

        /// <summary>
        /// The maximum number of objects to be returned.Valid values: 1 to 999.Default value: 100.  If the number of returned objects exceeds the value of max-keys, the response contains NextContinuationToken.Use the value of NextContinuationToken as the value of continuation-token in the next request.
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
        /// The prefix that must be contained in names of the returned objects.*   The value of prefix can be up to 1,024 bytes in length.*   If you specify prefix, the names of the returned objects contain the prefix.If you set prefix to a directory name, the objects whose names start with this prefix are listed. The objects consist of all objects and subdirectories in this directory.If you set prefix to a directory name and set delimiter to a forward slash (/), only the objects in the directory are listed. The subdirectories in the directory are returned in CommonPrefixes. Objects and subdirectories in the subdirectories are not listed.For example, a bucket contains the following three objects: fun/test.jpg, fun/movie/001.avi, and fun/movie/007.avi. If prefix is set to fun/, the three objects are returned. If prefix is set to fun/ and delimiter is set to a forward slash (/), fun/test.jpg and fun/movie/ are returned.
        /// </summary>
        public string? Prefix {
            get => Parameters.TryGetValue("prefix", out var value) ? value : null;
            set {
                if (value != null) Parameters["prefix"] = value;
            }
        }

        /// <summary>
        /// The encoding format of the returned objects in the response.  The values of Delimiter, StartAfter, Prefix, NextContinuationToken, and Key are UTF-8 encoded. If the value of Delimiter, StartAfter, Prefix, NextContinuationToken, or Key contains a control character that is not supported by Extensible Markup Language (XML) 1.0, you can specify encoding-type to encode the value in the response.
        /// Sees <see cref="Models.EncodingType"/> for supported values.
        /// </summary>
        public string? EncodingType {
            get => Parameters.TryGetValue("encoding-type", out var value) ? value : null;
            set {
                if (value != null) Parameters["encoding-type"] = value;
            }
        }

        /// <summary>
        /// Specifies whether to include the information about the bucket owner in the response. Valid values:*   true*   false
        /// </summary>
        public bool? FetchOwner {
            get => Parameters.TryGetValue("fetch-owner", out var value)
                ? Convert.ToBoolean(value, CultureInfo.InvariantCulture)
                : null;
            set {
                if (value != null)
                    Parameters["fetch-owner"] = Convert.ToString(value, CultureInfo.InvariantCulture)!.ToLowerInvariant();
            }
        }

        /// <summary>
        /// The name of the object after which the list operation begins. If this parameter is specified, objects whose names are alphabetically after the value of start-after are returned.The objects are returned by page based on start-after. The value of start-after can be up to 1,024 bytes in length.If the value of start-after does not exist when you perform a conditional query, the list starts from the object whose name is alphabetically after the value of start-after.
        /// </summary>
        public string? StartAfter {
            get => Parameters.TryGetValue("start-after", out var value) ? value : null;
            set {
                if (value != null) Parameters["start-after"] = value;
            }
        }

        /// <summary>
        /// The token from which the list operation starts. You can obtain the token from NextContinuationToken in the response of the ListObjectsV2 request.
        /// </summary>
        public string? ContinuationToken {
            get => Parameters.TryGetValue("continuation-token", out var value) ? value : null;
            set {
                if (value != null) Parameters["continuation-token"] = value;
            }
        }
    }

    /// <summary>
    /// The result for the ListObjectsV2 operation.
    /// </summary>
    public sealed class ListObjectsV2Result : ResultModel {
        /// <summary>
        /// The name of the bucket.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// The maximum number of returned objects in the response.
        /// </summary>
        public int? MaxKeys { get; set; }

        /// <summary>
        /// The character that is used to group objects by name. The objects whose names contain the same string from the prefix to the next occurrence of the delimiter are grouped as a single result element in CommonPrefixes.
        /// </summary>
        public string? Delimiter { get; set; }

        /// <summary>
        /// The encoding type of the content in the response. If you specify encoding-type in the request, the values of Delimiter, StartAfter, Prefix, NextContinuationToken, and Key are encoded in the response.
        /// </summary>
        public string? EncodingType { get; set; }

        /// <summary>
        /// If continuation-token is specified in the request, the response contains ContinuationToken.
        /// For ListObjectsV2
        /// </summary>
        public string? ContinuationToken { get; set; }

        /// <summary>
        /// The token from which the next list operation starts. Use the value of NextContinuationToken as the value of continuation-token in the next request.
        /// For ListObjectsV2
        /// </summary>
        public string? NextContinuationToken { get; set; }

        /// <summary>
        /// The container that stores the metadata of the returned objects.
        /// </summary>
        public IList<ObjectSummary>? Contents { get; set; }

        /// <summary>
        /// The prefix in the names of the returned objects.
        /// </summary>
        public string? Prefix { get; set; }

        /// <summary>
        /// If start-after is specified in the request, the response contains StartAfter.
        /// </summary>
        public string? StartAfter { get; set; }

        /// <summary>
        /// Indicates whether the returned results are truncated. Valid values:- true- false
        /// </summary>
        public bool? IsTruncated { get; set; }

        /// <summary>
        /// The number of objects returned for this request. If delimiter is specified in the request, the value of KeyCount is the sum of the values of Key and CommonPrefixes.
        /// </summary>
        public int? KeyCount { get; set; }

        /// <summary>
        /// Objects whose names contain the same string that ranges from the prefix to the next occurrence of the delimiter are grouped as a single result element
        /// </summary>
        public IList<CommonPrefix>? CommonPrefixes { get; set; }
    }

    /// <summary>
    /// The request for the GetBucketInfo operation.
    /// </summary>
    public sealed class GetBucketInfoRequest : RequestModel {
        /// <summary>
        /// The name of the bucket.
        /// </summary>
        public string? Bucket { get; set; }
    }

    /// <summary>
    /// The result for the GetBucketInfo operation.
    /// </summary>
    public sealed class GetBucketInfoResult : ResultModel {
        /// <summary>
        /// The container that stores the information about the bucket.
        /// </summary>
        public BucketInfo? BucketInfo {
            get {
                var info = InnerBody as XmlBucketInfo;
                return info?.Bucket;
            }
        }

        public GetBucketInfoResult() {
            BodyFormat = "xml";
            BodyType   = typeof(XmlBucketInfo);
        }
    }

    /// <summary>
    /// The request for the GetBucketLocation operation.
    /// </summary>
    public sealed class GetBucketLocationRequest : RequestModel {
        /// <summary>
        /// The name of the bucket.
        /// </summary>
        public string? Bucket { get; set; }
    }

    /// <summary>
    /// The result for the GetBucketLocation operation.
    /// </summary>
    public sealed class GetBucketLocationResult : ResultModel {
        /// <summary>
        /// The region in which the bucket resides.Examples: oss-cn-hangzhou, oss-cn-shanghai, oss-cn-qingdao, oss-cn-beijing, oss-cn-zhangjiakou, oss-cn-hongkong, oss-cn-shenzhen, oss-us-west-1, oss-us-east-1, and oss-ap-southeast-1.For more information about the regions in which buckets reside, see [Regions and endpoints](~~31837~~).
        /// </summary>
        public string? LocationConstraint {
            get {
                var info = InnerBody as XmlLocationConstraint;
                return info?.Text;
            }
        }

        public GetBucketLocationResult() {
            BodyFormat = "xml";
            BodyType   = typeof(XmlLocationConstraint);
        }
    }
}