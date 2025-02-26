using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml.Serialization;

namespace AlibabaCloud.OSS.v2.Models {
    /// <summary>
    /// The configuration information for the bucket.
    /// </summary>
    [XmlRoot("Bucket")]
    public sealed class BucketProperties {
        /// <summary>
        /// The name of the bucket.
        /// </summary>
        [XmlElement(ElementName = "Name")]
        public string? Name { get; set; }

        /// <summary>
        /// The time when the bucket was created. Format: yyyy-mm-ddThh:mm:ss.timezone.
        /// </summary>
        [XmlElement(ElementName = "CreationDate")]
        public DateTime? CreationDate { get; set; }

        /// <summary>
        /// The storage class of the bucket. Valid values:
        /// Standard, IA, Archive, ColdArchive and DeepColdArchive.
        /// </summary>
        [XmlElement(ElementName = "StorageClass")]
        public string? StorageClass { get; set; }

        /// <summary>
        /// The public endpoint of the region in which the bucket resides.
        /// </summary>
        [XmlElement(ElementName = "ExtranetEndpoint")]
        public string? ExtranetEndpoint { get; set; }

        /// <summary>
        /// The internal endpoint of the region in which the bucket you access from ECS instances resides. The bucket and ECS instances are in the same region.
        /// </summary>
        [XmlElement(ElementName = "IntranetEndpoint")]
        public string? IntranetEndpoint { get; set; }

        /// <summary>
        /// The region in which the bucket is located.
        /// </summary>
        [XmlElement(ElementName = "Region")]
        public string? Region { get; set; }

        /// <summary>
        /// The ID of the resource group to which the bucket belongs.
        /// </summary>
        [XmlElement(ElementName = "ResourceGroupId")]
        public string? ResourceGroupId { get; set; }

        /// <summary>
        /// The data center in which the bucket is located.
        /// </summary>
        [XmlElement(ElementName = "Location")]
        public string? Location { get; set; }
    }

    /// <summary>
    /// The request for the ListBuckets operation.
    /// </summary>
    public sealed class ListBucketsRequest : RequestModel {
        /// <summary>
        /// The ID of the resource group to which the bucket belongs.
        /// </summary>
        public string? ResourceGroupId {
            get => Headers.TryGetValue("x-oss-resource-group-id", out var value) ? value : null;
            set {
                if (value != null) Headers["x-oss-resource-group-id"] = value;
            }
        }

        /// <summary>
        /// The prefix that the names of returned buckets must contain.
        /// </summary>
        public string? Prefix {
            get => Parameters.TryGetValue("prefix", out var value) ? value : null;
            set {
                if (value != null) Parameters["prefix"] = value;
            }
        }

        /// <summary>
        /// The name of the bucket from which the list operation begins.
        /// </summary>
        public string? Marker {
            get => Parameters.TryGetValue("marker", out var value) ? value : null;
            set {
                if (value != null) Parameters["marker"] = value;
            }
        }

        /// <summary>
        /// The maximum number of buckets that can be returned in the single query.
        /// </summary>
        public long? MaxKeys {
            get => Parameters.TryGetValue("max-keys", out var value) ? Convert.ToInt64(value) : null;
            set {
                if (value != null) Parameters["max-keys"] = Convert.ToString(value, CultureInfo.InvariantCulture)!;
            }
        }
    }

    /// <summary>
    /// The result for the ListBuckets operation.
    /// </summary>
    public sealed class ListBucketsResult : ResultModel {
        /// <summary>
        /// The prefix contained in the names of the returned bucket.
        /// </summary>
        public string? Prefix { get; internal set; }

        /// <summary>
        /// The name of the bucket after which the ListBuckets  operation starts.
        /// </summary>
        public string? Marker { get; internal set; }

        /// <summary>
        /// The maximum number of buckets that can be returned for the request.
        /// </summary>
        public long? MaxKeys { get; internal set; }

        /// <summary>
        /// Indicates whether all results are returned.
        /// true: Only part of the results are returned for the request.
        /// false: All results are returned for the request.
        /// </summary>
        public bool? IsTruncated { get; internal set; }

        /// <summary>
        /// The marker for the next ListBuckets request, which can be used to return the remaining results.
        /// </summary>
        public string? NextMarker { get; internal set; }

        /// <summary>
        /// Stores information about the owner.
        /// </summary>
        public Owner? Owner { get; internal set; }

        /// <summary>
        /// The container that stores information about buckets.
        /// </summary>
        public IList<BucketProperties>? Buckets { get; internal set; }
    }
}