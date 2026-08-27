using System.Xml.Serialization;

namespace AlibabaCloud.OSS.V2.Agentic.Models
{
    /// <summary>
    /// The configuration for the CreateAgenticBucket operation.
    /// </summary>
    [XmlRoot("CreateAgenticBucketConfiguration")]
    public sealed class CreateAgenticBucketConfiguration
    {
        /// <summary>
        /// The storage class of the agentic bucket.
        /// Sees <see cref="V2.Models.StorageClassType"/> for supported values.
        /// </summary>
        [XmlElement("StorageClass")]
        public string? StorageClass { get; set; }

        /// <summary>
        /// The data redundancy type of the agentic bucket.
        /// Sees <see cref="V2.Models.DataRedundancyType"/> for supported values.
        /// </summary>
        [XmlElement("DataRedundancyType")]
        public string? DataRedundancyType { get; set; }
    }

    /// <summary>
    /// The status configuration of an agentic bucket.
    /// </summary>
    [XmlRoot("AgenticBucketStatus")]
    public sealed class AgenticBucketStatus
    {
        /// <summary>
        /// The status of the agentic bucket.
        /// </summary>
        [XmlElement("Status")]
        public string? Status { get; set; }
    }

    /// <summary>
    /// The container that stores the default server-side encryption method.
    /// </summary>
    [XmlRoot("ApplyServerSideEncryptionByDefault")]
    public sealed class ApplyServerSideEncryptionByDefault
    {
        /// <summary>
        /// The default server-side encryption method. Valid values: KMS, AES256, and SM4.
        /// </summary>
        [XmlElement("SSEAlgorithm")]
        public string? SSEAlgorithm { get; set; }

        /// <summary>
        /// The CMK ID that is specified when SSEAlgorithm is set to KMS and a specified CMK is used for encryption.
        /// </summary>
        [XmlElement("KMSMasterKeyID")]
        public string? KMSMasterKeyID { get; set; }

        /// <summary>
        /// The algorithm that is used to encrypt objects. This parameter is valid only when SSEAlgorithm is set to KMS. Valid value: SM4.
        /// </summary>
        [XmlElement("KMSDataEncryption")]
        public string? KMSDataEncryption { get; set; }
    }

    /// <summary>
    /// The server-side encryption rule of an agentic bucket.
    /// </summary>
    [XmlRoot("ServerSideEncryptionRule")]
    public sealed class ServerSideEncryptionRule
    {
        /// <summary>
        /// The container that stores the default server-side encryption method.
        /// </summary>
        [XmlElement("ApplyServerSideEncryptionByDefault")]
        public ApplyServerSideEncryptionByDefault? ApplyServerSideEncryptionByDefault { get; set; }
    }

    /// <summary>
    /// The information about an agentic bucket.
    /// </summary>
    [XmlRoot("AgenticBucketInfo")]
    public sealed class AgenticBucketInfo
    {
        /// <summary>
        /// The name of the agentic bucket.
        /// </summary>
        [XmlElement("Name")]
        public string? Name { get; set; }

        /// <summary>
        /// The owner of the agentic bucket.
        /// </summary>
        [XmlElement("Owner")]
        public string? Owner { get; set; }

        /// <summary>
        /// The region in which the agentic bucket is located.
        /// </summary>
        [XmlElement("Region")]
        public string? Region { get; set; }

        /// <summary>
        /// The storage class of the agentic bucket.
        /// </summary>
        [XmlElement("StorageClass")]
        public string? StorageClass { get; set; }

        /// <summary>
        /// The data redundancy type of the agentic bucket.
        /// </summary>
        [XmlElement("DataRedundancyType")]
        public string? DataRedundancyType { get; set; }

        /// <summary>
        /// The status of the agentic bucket.
        /// </summary>
        [XmlElement("Status")]
        public string? Status { get; set; }

        /// <summary>
        /// The resource type of the agentic bucket.
        /// </summary>
        [XmlElement("BucketResourceType")]
        public string? BucketResourceType { get; set; }

        /// <summary>
        /// The time when the agentic bucket was created.
        /// </summary>
        [XmlElement("CreateTime")]
        public string? CreateTime { get; set; }

        /// <summary>
        /// The access control list (ACL) of the agentic bucket.
        /// </summary>
        [XmlElement("ACL")]
        public string? ACL { get; set; }

        /// <summary>
        /// The Block Public Access configuration of the agentic bucket.
        /// </summary>
        [XmlElement("PublicAccessBlock")]
        public string? PublicAccessBlock { get; set; }

        /// <summary>
        /// The server-side encryption rule of the agentic bucket.
        /// </summary>
        [XmlElement("ServerSideEncryptionRule")]
        public ServerSideEncryptionRule? ServerSideEncryptionRule { get; set; }

        /// <summary>
        /// The versioning state of the agentic bucket.
        /// </summary>
        [XmlElement("Versioning")]
        public string? Versioning { get; set; }

        /// <summary>
        /// The policy of the agentic bucket.
        /// </summary>
        [XmlElement("BucketPolicy")]
        public string? BucketPolicy { get; set; }
    }

    /// <summary>
    /// The summary of an agentic bucket in ListAgenticBuckets.
    /// </summary>
    [XmlRoot("AgenticBucket")]
    public sealed class AgenticBucketSummary
    {
        /// <summary>
        /// The name of the agentic bucket.
        /// </summary>
        [XmlElement("Name")]
        public string? Name { get; set; }

        /// <summary>
        /// The storage class of the agentic bucket.
        /// </summary>
        [XmlElement("StorageClass")]
        public string? StorageClass { get; set; }

        /// <summary>
        /// The data redundancy type of the agentic bucket.
        /// </summary>
        [XmlElement("DataRedundancyType")]
        public string? DataRedundancyType { get; set; }

        /// <summary>
        /// The time when the agentic bucket was created.
        /// </summary>
        [XmlElement("CreateTime")]
        public string? CreateTime { get; set; }
    }

    /// <summary>
    /// The summary of a bucket space in ListBucketSpaces.
    /// </summary>
    [XmlRoot("BucketSpace")]
    public sealed class BucketSpaceSummary
    {
        /// <summary>
        /// The name of the bucket space.
        /// </summary>
        [XmlElement("Name")]
        public string? Name { get; set; }

        /// <summary>
        /// The region in which the bucket space is located.
        /// </summary>
        [XmlElement("Location")]
        public string? Location { get; set; }

        /// <summary>
        /// The time when the bucket space was created.
        /// </summary>
        [XmlElement("CreationDate")]
        public string? CreationDate { get; set; }

        /// <summary>
        /// The storage class of the bucket space.
        /// </summary>
        [XmlElement("StorageClass")]
        public string? StorageClass { get; set; }
    }
}
