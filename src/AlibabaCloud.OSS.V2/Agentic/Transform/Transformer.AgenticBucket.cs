using System.Collections.Generic;
using System.Xml.Serialization;
using AlibabaCloud.OSS.V2.Agentic.Models;
#if NET8_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
#endif

namespace AlibabaCloud.OSS.V2.Transform
{
    /// <summary>
    /// The container that stores the results of the ListAgenticBuckets request.
    /// </summary>
    [XmlRoot("ListAgenticBucketsResult")]
    public sealed class XmlListAgenticBucketsResult
    {
        [XmlElement("Region")]
        public string? Region { get; set; }

        [XmlElement("Owner")]
        public string? Owner { get; set; }

        [XmlElement("ContinuationToken")]
        public string? ContinuationToken { get; set; }

        [XmlElement("NextContinuationToken")]
        public string? NextContinuationToken { get; set; }

        [XmlElement("IsTruncated")]
        public bool? IsTruncated { get; set; }

        [XmlArray("AgenticBuckets")]
        [XmlArrayItem("AgenticBucket")]
        public List<AgenticBucketSummary>? AgenticBuckets { get; set; }
    }

    /// <summary>
    /// The container that stores the results of the ListBucketSpaces request.
    /// </summary>
    [XmlRoot("ListBucketSpacesResult")]
    public sealed class XmlListBucketSpacesResult
    {
        [XmlElement("Owner")]
        public Models.Owner? Owner { get; set; }

        [XmlArray("BucketSpaces")]
        [XmlArrayItem("BucketSpace")]
        public List<BucketSpaceSummary>? BucketSpaces { get; set; }

        [XmlElement("Prefix")]
        public string? Prefix { get; set; }

        [XmlElement("MaxKeys")]
        public int? MaxKeys { get; set; }

        [XmlElement("ContinuationToken")]
        public string? ContinuationToken { get; set; }

        [XmlElement("NextContinuationToken")]
        public string? NextContinuationToken { get; set; }

        [XmlElement("StartAfter")]
        public string? StartAfter { get; set; }

        [XmlElement("IsTruncated")]
        public bool? IsTruncated { get; set; }
    }

    internal static partial class Serde
    {
#if NET8_0_OR_GREATER
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(AgenticBucketInfo))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(Agentic.Models.ServerSideEncryptionRule))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(ApplyServerSideEncryptionByDefault))]
#endif
        public static void DeserializeAgenticXmlBody(ref Models.ResultModel result, ref OperationOutput output)
        {
            if (output.Body == null) return;

            if (result.BodyFormat == "xml")
            {
                if (result.BodyType == null)
                {
                    throw new System.Exception("body type is null");
                }

                using var body = output.Body;
                var serializer = CreateSerializer(result.BodyType!);
                result.InnerBody = DeserializeXml(serializer, body);
            }
        }

#if NET8_0_OR_GREATER
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(XmlListAgenticBucketsResult))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(AgenticBucketSummary))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(List<AgenticBucketSummary>))]
#endif
        public static void DeserializeListAgenticBuckets(ref Models.ResultModel baseResult, ref OperationOutput output)
        {
            if (output.Body == null) return;
            using var body = output.Body;
            var serializer = CreateSerializer(typeof(XmlListAgenticBucketsResult));
            var obj = DeserializeXml(serializer, body) as XmlListAgenticBucketsResult;
            var result = baseResult as ListAgenticBucketsResult;

            if (obj == null || result == null) return;

            result.Region = obj.Region;
            result.Owner = obj.Owner;
            result.ContinuationToken = obj.ContinuationToken;
            result.NextContinuationToken = obj.NextContinuationToken;
            result.IsTruncated = obj.IsTruncated;
            result.AgenticBuckets = obj.AgenticBuckets;
        }

#if NET8_0_OR_GREATER
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(XmlListBucketSpacesResult))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(BucketSpaceSummary))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(List<BucketSpaceSummary>))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(Models.Owner))]
#endif
        public static void DeserializeListBucketSpaces(ref Models.ResultModel baseResult, ref OperationOutput output)
        {
            if (output.Body == null) return;
            using var body = output.Body;
            var serializer = CreateSerializer(typeof(XmlListBucketSpacesResult));
            var obj = DeserializeXml(serializer, body) as XmlListBucketSpacesResult;
            var result = baseResult as ListBucketSpacesResult;

            if (obj == null || result == null) return;

            result.Owner = obj.Owner;
            result.BucketSpaces = obj.BucketSpaces;
            result.Prefix = obj.Prefix;
            result.MaxKeys = obj.MaxKeys;
            result.ContinuationToken = obj.ContinuationToken;
            result.NextContinuationToken = obj.NextContinuationToken;
            result.StartAfter = obj.StartAfter;
            result.IsTruncated = obj.IsTruncated;
        }
    }
}
