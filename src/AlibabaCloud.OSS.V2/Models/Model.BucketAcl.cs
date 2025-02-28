namespace AlibabaCloud.OSS.V2.Models {
    /// <summary>
    /// The request for the PutBucketAcl operation.
    /// </summary>
    public sealed class PutBucketAclRequest : RequestModel {
        /// <summary>
        /// The name of the bucket.
        /// </summary>
        public string? Bucket { get; set; }

        /// <summary>
        /// The ACL that you want to configure or modify for the bucket. The x-oss-acl header is included in PutBucketAcl requests to configure or modify the ACL of the bucket. If this header is not included, the ACL configurations do not take effect.Valid values:*   public-read-write: All users can read and write objects in the bucket. Exercise caution when you set the value to public-read-write.*   public-read: Only the owner and authorized users of the bucket can read and write objects in the bucket. Other users can only read objects in the bucket. Exercise caution when you set the value to public-read.*   private: Only the owner and authorized users of this bucket can read and write objects in the bucket. Other users cannot access objects in the bucket.
        /// Sees <see cref="BucketAclType"/> for supported values.
        /// </summary>
        public string? Acl {
            get => Headers.TryGetValue("x-oss-acl", out var value) ? value : null;
            set {
                if (value != null) Headers["x-oss-acl"] = value;
            }
        }
    }

    /// <summary>
    /// The result for the PutBucketAcl operation.
    /// </summary>
    public sealed class PutBucketAclResult : ResultModel { }

    /// <summary>
    /// The request for the GetBucketAcl operation.
    /// </summary>
    public sealed class GetBucketAclRequest : RequestModel {
        /// <summary>
        /// The name of the bucket.
        /// </summary>
        public string? Bucket { get; set; }
    }

    /// <summary>
    /// The result for the GetBucketAcl operation.
    /// </summary>
    public sealed class GetBucketAclResult : ResultModel {
        /// <summary>
        /// The container that stores the ACL information.
        /// </summary>
        public AccessControlPolicy? AccessControlPolicy => InnerBody as AccessControlPolicy;

        public GetBucketAclResult() {
            BodyFormat = "xml";
            BodyType = typeof(AccessControlPolicy);
        }
    }
}