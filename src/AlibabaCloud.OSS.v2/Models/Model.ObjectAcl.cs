namespace AlibabaCloud.OSS.v2.Models {
    /// <summary>
    /// The request for the PutObjectAcl operation.
    /// </summary>
    public sealed class PutObjectAclRequest : RequestModel {
        /// <summary>
        /// The name of the bucket.
        /// </summary>
        public string? Bucket { get; set; }

        /// <summary>
        /// The name of the object.
        /// </summary>
        public string? Key { get; set; }

        /// <summary>
        /// The access control list (ACL) of the object.
        /// Sees <see cref="ObjectAclType"/> for supported values. 
        /// </summary>
        public string? Acl {
            get => Headers.TryGetValue("x-oss-object-acl", out var value) ? value : null;
            set {
                if (value != null) Headers["x-oss-object-acl"] = value;
            }
        }

        /// <summary>
        /// The version id of the object.
        /// </summary>
        public string? VersionId {
            get => Parameters.TryGetValue("versionId", out var value) ? value : null;
            set {
                if (value != null) Parameters["versionId"] = value;
            }
        }
    }

    /// <summary>
    /// The result for the PutObjectAcl operation.
    /// </summary>
    public sealed class PutObjectAclResult : ResultModel {
        /// <summary>
        /// Version of the object.
        /// </summary>
        public string? VersionId => Headers.TryGetValue("x-oss-version-id", out var value) ? value : null;
    }

    /// <summary>
    /// The request for the GetObjectAcl operation.
    /// </summary>
    public sealed class GetObjectAclRequest : RequestModel {
        /// <summary>
        /// The name of the bucket.
        /// </summary>
        public string? Bucket { get; set; }

        /// <summary>
        /// The name of the object.
        /// </summary>
        public string? Key { get; set; }

        /// <summary>
        /// The version id of the target object.
        /// </summary>
        public string? VersionId {
            get => Parameters.TryGetValue("versionId", out var value) ? value : null;
            set {
                if (value != null) Parameters["versionId"] = value;
            }
        }
    }

    /// <summary>
    /// The result for the GetObjectAcl operation.
    /// </summary>
    public sealed class GetObjectAclResult : ResultModel {
        /// <summary>
        /// The container that stores the results of the GetObjectACL request.
        /// </summary>
        public AccessControlPolicy? AccessControlPolicy => InnerBody as AccessControlPolicy;

        /// <summary>
        /// The ACL of the object.
        /// Sees <see cref="ObjectAclType"/> for supported values.
        /// </summary>
        public string? Acl {
            get {
                if (InnerBody is AccessControlPolicy acl) return acl?.AccessControlList?.Grant;
                return null;
            }
        }

        public GetObjectAclResult() {
            BodyFormat = "xml";
            BodyType   = typeof(AccessControlPolicy);
        }
    }
}