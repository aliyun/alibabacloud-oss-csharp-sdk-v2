using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using AlibabaCloud.OSS.V2.Models;

namespace AlibabaCloud.OSS.V2.Agentic.Models
{
    // --- CreateAgenticBucket ---

    /// <summary>
    /// The request for the CreateAgenticBucket operation.
    /// </summary>
    public sealed class CreateAgenticBucketRequest : RequestModel
    {
        public CreateAgenticBucketRequest()
        {
            BodyFormat = "xml";
        }

        /// <summary>
        /// The name of the agentic bucket.
        /// </summary>
        public string? Bucket { get; set; }

        /// <summary>
        /// The configuration for the CreateAgenticBucket operation.
        /// </summary>
        public CreateAgenticBucketConfiguration? CreateAgenticBucketConfiguration
        {
            get => InnerBody as CreateAgenticBucketConfiguration;
            set => InnerBody = value;
        }
    }

    /// <summary>
    /// The result for the CreateAgenticBucket operation.
    /// </summary>
    public sealed class CreateAgenticBucketResult : ResultModel { }

    // --- DeleteAgenticBucket ---

    /// <summary>
    /// The request for the DeleteAgenticBucket operation.
    /// </summary>
    public sealed class DeleteAgenticBucketRequest : RequestModel
    {
        /// <summary>
        /// The name of the agentic bucket.
        /// </summary>
        public string? Bucket { get; set; }
    }

    /// <summary>
    /// The result for the DeleteAgenticBucket operation.
    /// </summary>
    public sealed class DeleteAgenticBucketResult : ResultModel { }

    // --- GetAgenticBucket ---

    /// <summary>
    /// The request for the GetAgenticBucket operation.
    /// </summary>
    public sealed class GetAgenticBucketRequest : RequestModel
    {
        /// <summary>
        /// The name of the agentic bucket.
        /// </summary>
        public string? Bucket { get; set; }
    }

    /// <summary>
    /// The result for the GetAgenticBucket operation.
    /// </summary>
    public sealed class GetAgenticBucketResult : ResultModel
    {
        /// <summary>
        /// The information about the agentic bucket.
        /// </summary>
        public AgenticBucketInfo? AgenticBucketInfo => InnerBody as AgenticBucketInfo;

        public GetAgenticBucketResult()
        {
            BodyFormat = "xml";
            BodyType = typeof(AgenticBucketInfo);
        }
    }

    // --- ListAgenticBuckets ---

    /// <summary>
    /// The request for the ListAgenticBuckets operation.
    /// </summary>
    public sealed class ListAgenticBucketsRequest : RequestModel
    {
        /// <summary>
        /// The token from which the list operation starts.
        /// </summary>
        public string? ContinuationToken
        {
            get => Parameters.TryGetValue("continuation-token", out var value) ? value : null;
            set
            {
                if (value != null) Parameters["continuation-token"] = value;
            }
        }

        /// <summary>
        /// The maximum number of agentic buckets that can be returned.
        /// </summary>
        public long? MaxKeys
        {
            get => Parameters.TryGetValue("max-keys", out var value)
                ? Convert.ToInt64(value, CultureInfo.InvariantCulture)
                : null;
            set
            {
                if (value != null) Parameters["max-keys"] = Convert.ToString((long)value, CultureInfo.InvariantCulture);
            }
        }
    }

    /// <summary>
    /// The result for the ListAgenticBuckets operation.
    /// </summary>
    public sealed class ListAgenticBucketsResult : ResultModel
    {
        /// <summary>
        /// The region in which the agentic buckets are located.
        /// </summary>
        public string? Region { get; set; }

        /// <summary>
        /// The owner of the agentic buckets.
        /// </summary>
        public string? Owner { get; set; }

        /// <summary>
        /// The token from which the list operation started.
        /// </summary>
        public string? ContinuationToken { get; set; }

        /// <summary>
        /// The token from which the next list operation starts.
        /// </summary>
        public string? NextContinuationToken { get; set; }

        /// <summary>
        /// Indicates whether the returned results are truncated.
        /// </summary>
        public bool? IsTruncated { get; set; }

        /// <summary>
        /// The list of agentic buckets.
        /// </summary>
        public IList<AgenticBucketSummary>? AgenticBuckets { get; set; }
    }

    // --- PutAgenticBucketStatus ---

    /// <summary>
    /// The request for the PutAgenticBucketStatus operation.
    /// </summary>
    public sealed class PutAgenticBucketStatusRequest : RequestModel
    {
        public PutAgenticBucketStatusRequest()
        {
            BodyFormat = "xml";
        }

        /// <summary>
        /// The name of the agentic bucket.
        /// </summary>
        public string? Bucket { get; set; }

        /// <summary>
        /// The status configuration of the agentic bucket.
        /// </summary>
        public AgenticBucketStatus? AgenticBucketStatus
        {
            get => InnerBody as AgenticBucketStatus;
            set => InnerBody = value;
        }
    }

    /// <summary>
    /// The result for the PutAgenticBucketStatus operation.
    /// </summary>
    public sealed class PutAgenticBucketStatusResult : ResultModel { }

    // --- ListBucketSpaces ---

    /// <summary>
    /// The request for the ListBucketSpaces operation.
    /// </summary>
    public sealed class ListBucketSpacesRequest : RequestModel
    {
        /// <summary>
        /// The name of the agentic bucket.
        /// </summary>
        public string? Bucket { get; set; }

        /// <summary>
        /// The prefix that the names of the returned bucket spaces must contain.
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
        /// The token from which the list operation starts.
        /// </summary>
        public string? ContinuationToken
        {
            get => Parameters.TryGetValue("continuation-token", out var value) ? value : null;
            set
            {
                if (value != null) Parameters["continuation-token"] = value;
            }
        }

        /// <summary>
        /// The name of the bucket space after which the list operation begins.
        /// </summary>
        public string? StartAfter
        {
            get => Parameters.TryGetValue("start-after", out var value) ? value : null;
            set
            {
                if (value != null) Parameters["start-after"] = value;
            }
        }

        /// <summary>
        /// The maximum number of bucket spaces that can be returned.
        /// </summary>
        public long? MaxKeys
        {
            get => Parameters.TryGetValue("max-keys", out var value)
                ? Convert.ToInt64(value, CultureInfo.InvariantCulture)
                : null;
            set
            {
                if (value != null) Parameters["max-keys"] = Convert.ToString((long)value, CultureInfo.InvariantCulture);
            }
        }
    }

    /// <summary>
    /// The result for the ListBucketSpaces operation.
    /// </summary>
    public sealed class ListBucketSpacesResult : ResultModel
    {
        /// <summary>
        /// The owner of the bucket spaces.
        /// </summary>
        public Owner? Owner { get; set; }

        /// <summary>
        /// The list of bucket spaces.
        /// </summary>
        public IList<BucketSpaceSummary>? BucketSpaces { get; set; }

        /// <summary>
        /// The prefix that the names of the returned bucket spaces contain.
        /// </summary>
        public string? Prefix { get; set; }

        /// <summary>
        /// The maximum number of bucket spaces that can be returned.
        /// </summary>
        public int? MaxKeys { get; set; }

        /// <summary>
        /// The token from which the list operation started.
        /// </summary>
        public string? ContinuationToken { get; set; }

        /// <summary>
        /// The token from which the next list operation starts.
        /// </summary>
        public string? NextContinuationToken { get; set; }

        /// <summary>
        /// The name of the bucket space after which the list operation began.
        /// </summary>
        public string? StartAfter { get; set; }

        /// <summary>
        /// Indicates whether the returned results are truncated.
        /// </summary>
        public bool? IsTruncated { get; set; }
    }
}
