namespace AlibabaCloud.OSS.V2.Models {

    /// <summary>
    /// The request for the PutObjectTagging operation.
    /// </summary>
    public sealed class PutObjectTaggingRequest : RequestModel {
        public PutObjectTaggingRequest() {
            BodyFormat = "xml";
        }

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

        /// <summary>
        /// The request body schema.
        /// </summary>
        public Tagging? Tagging {
            get => InnerBody as Tagging;
            set => InnerBody = value;
        }
    }

    /// <summary>
    /// The result for the PutObjectTagging operation.
    /// </summary>
    public sealed class PutObjectTaggingResult : ResultModel {
        /// <summary>
        /// The object version id.
        /// </summary>
        public string? VersionId => Headers.TryGetValue("x-oss-version-id", out var value) ? value : null;
    }

    /// <summary>
    /// The request for the GetObjectTagging operation.
    /// </summary>
    public sealed class GetObjectTaggingRequest : RequestModel {
        /// <summary>
        /// The name of the bucket.
        /// </summary>
        public string? Bucket { get; set; }

        /// <summary>
        /// The full path of the object.
        /// </summary>
        public string? Key { get; set; }

        /// <summary>
        /// The version id of the object that you want to query.
        /// </summary>
        public string? VersionId {
            get => Parameters.TryGetValue("versionId", out var value) ? value : null;
            set {
                if (value != null) Parameters["versionId"] = value;
            }
        }
    }

    /// <summary>
    /// The result for the GetObjectTagging operation.
    /// </summary>
    public sealed class GetObjectTaggingResult : ResultModel {
        /// <summary>
        /// The container that stores the returned tag of the bucket.
        /// </summary>
        public Tagging? Tagging => InnerBody as Tagging;

        public GetObjectTaggingResult() {
            BodyFormat = "xml";
            BodyType = typeof(Tagging);
        }
    }

    /// <summary>
    /// The request for the DeleteObjectTagging operation.
    /// </summary>
    public sealed class DeleteObjectTaggingRequest : RequestModel {
        /// <summary>
        /// The name of the bucket.
        /// </summary>
        public string? Bucket { get; set; }

        /// <summary>
        /// The full path of the object.
        /// </summary>
        public string? Key { get; set; }

        /// <summary>
        /// The version ID of the object that you want to delete.
        /// </summary>
        public string? VersionId {
            get => Parameters.TryGetValue("versionId", out var value) ? value : null;
            set {
                if (value != null) Parameters["versionId"] = value;
            }
        }
    }

    /// <summary>
    /// The result for the DeleteObjectTagging operation.
    /// </summary>
    public sealed class DeleteObjectTaggingResult : ResultModel { }
}