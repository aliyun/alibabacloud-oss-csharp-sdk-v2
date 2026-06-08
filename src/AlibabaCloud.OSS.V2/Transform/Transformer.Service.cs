using System.Collections.Generic;
using System.Xml.Serialization;
#if NET8_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
#endif

namespace AlibabaCloud.OSS.V2.Transform
{

    [XmlRoot("ListAllMyBucketsResult")]
    public sealed class XmlListAllMyBucketsResult
    {
        [XmlElement(ElementName = "Prefix")]
        public string? Prefix { get; set; }

        [XmlElement(ElementName = "Marker")]
        public string? Marker { get; set; }

        [XmlElement(ElementName = "MaxKeys")]
        public long? MaxKeys { get; set; }

        [XmlElement(ElementName = "IsTruncated")]
        public bool? IsTruncated { get; set; }

        [XmlElement(ElementName = "NextMarker")]
        public string? NextMarker { get; set; }

        [XmlElement(ElementName = "Owner")]
        public Models.Owner? Owner { get; set; }

        [XmlArrayItem(ElementName = "Bucket")]
        public List<Models.BucketProperties>? Buckets { get; set; }
    }

    internal static partial class Serde
    {
#if NET8_0_OR_GREATER
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(Models.BucketProperties))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(List<Models.BucketProperties>))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(Models.Owner))]
#endif
        public static void DeserializeListBuckets(
            ref Models.ResultModel baseResult,
            ref OperationOutput output
        )
        {
            var serializer = CreateSerializer(typeof(XmlListAllMyBucketsResult));
            using var body = output.Body!;
            var obj = DeserializeXml(serializer, body) as XmlListAllMyBucketsResult;
            var result = baseResult as Models.ListBucketsResult;

            result!.Prefix = obj!.Prefix;
            result.Marker = obj.Marker;
            result.MaxKeys = obj.MaxKeys;
            result.IsTruncated = obj.IsTruncated;
            result.NextMarker = obj.NextMarker;
            result.Owner = obj.Owner;
            result.Buckets = obj.Buckets;
        }
    }
}
