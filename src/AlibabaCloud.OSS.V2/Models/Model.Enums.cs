namespace AlibabaCloud.OSS.V2.Models
{
    /// <summary>
    /// The access control list (ACL) of the bucket
    /// </summary>
    public enum BucketAclType
    {
        /// <summary>
        /// Only the bucket owner can perform read and write operations on objects in the bucket.
        /// Other users cannot access the objects in the bucket.
        /// </summary>
        Private,

        /// <summary>
        /// Only the bucket owner can write data to objects in the bucket.
        /// Other users, including anonymous users, can only read objects in the bucket.
        /// </summary>
        PublicRead,

        /// <summary>
        /// All users, including anonymous users, can perform read and write operations on the bucket.
        /// </summary>
        PublicReadWrite
    }

    public static class BucketAclTypeExtensions
    {
        public static string GetString(this BucketAclType me)
        {
            return me switch
            {
                BucketAclType.Private => "private",
                BucketAclType.PublicRead => "public-read",
                BucketAclType.PublicReadWrite => "public-read-write",
                _ => "NO VALUE GIVEN"
            };
        }
    }

    public enum AccessMonitorStatusType
    {
        Enabled,
        Disabled
    }

    public static class AccessMonitorStatusTypeExtensions
    {
        public static string GetString(this AccessMonitorStatusType me)
        {
            return me switch
            {
                AccessMonitorStatusType.Enabled => "Enabled",
                AccessMonitorStatusType.Disabled => "Disabled",
                _ => "NO VALUE GIVEN"
            };
        }
    }

    /// <summary>
    /// The storage class of the bucket or object.
    /// </summary>
    public enum StorageClassType
    {
        /// <summary>
        /// Standard provides highly reliable, highly available and high-performance object storage  for data that is frequently accessed.
        /// </summary>
        Standard,

        /// <summary>
        /// IA provides highly durable storage at lower prices compared with Standard.
        /// It has a minimum billable size of 64 KB and a minimum billable storage duration of 30 days.
        /// </summary>
        IA,

        /// <summary>
        /// Archive provides high-durability storage at lower prices compared with Standard and IA.
        /// It has a minimum billable size of 64 KB and a minimum billable storage duration of 60 days.
        /// </summary>
        Archive,

        /// <summary>
        /// Cold Archive provides highly durable storage at lower prices compared with Archive.
        /// It has a minimum billable size of 64 KB and a minimum billable storage duration of 180 days.
        /// </summary>
        ColdArchive,

        /// <summary>
        /// Deep Cold Archive provides highly durable storage at lower prices compared with Cold Archive.
        /// It has a minimum billable size of 64 KB and a minimum billable storage duration of 180 days.
        /// </summary>
        DeepColdArchive
    }

    public static class StorageClassTypeExtensions
    {
        public static string GetString(this StorageClassType me)
        {
            return me switch
            {
                StorageClassType.Standard => "Standard",
                StorageClassType.IA => "IA",
                StorageClassType.Archive => "Archive",
                StorageClassType.ColdArchive => "ColdArchive",
                StorageClassType.DeepColdArchive => "DeepColdArchive",
                _ => "NO VALUE GIVEN"
            };
        }
    }

    /// <summary>
    /// The redundancy type of the bucket.
    /// </summary>
    public enum DataRedundancyType
    {
        /// <summary>
        /// LRS Locally redundant storage(LRS) stores copies of each object across different devices
        /// in the same zone. This ensures data reliability and availability even if two storage devices
        /// are damaged at the same time.
        /// </summary>
        LRS,

        /// <summary>
        /// ZRS Zone-redundant storage(ZRS) uses the multi-zone mechanism to distribute user data across
        /// multiple zones in the same region. If one zone becomes unavailable, you can continue to
        /// access the data that is stored in other zones.
        /// </summary>
        ZRS,
    }

    public static class DataRedundancyTypeExtensions
    {
        public static string GetString(this DataRedundancyType me)
        {
            return me switch
            {
                DataRedundancyType.LRS => "LRS",
                DataRedundancyType.ZRS => "ZRS",
                _ => "NO VALUE GIVEN"
            };
        }
    }

    /// <summary>
    /// The access control list (ACL) of the object
    /// </summary>
    public enum ObjectAclType
    {
        /// <summary>
        /// Only the object owner is allowed to perform read and write operations on the object.
        /// Other users cannot access the object.
        /// </summary>
        Private,

        /// <summary>
        /// Only the object owner can write data to the object.
        /// Other users, including anonymous users, can only read the object.
        /// </summary>
        PublicRead,

        /// <summary>
        /// All users, including anonymous users, can perform read and write operations on the object.
        /// </summary>
        PublicReadWrite,

        /// <summary>
        /// The ACL of the object is the same as that of the bucket in which the object is stored.
        /// </summary>
        Default
    }

    public static class ObjectAclTypeExtensions
    {
        public static string GetString(this ObjectAclType me)
        {
            return me switch
            {
                ObjectAclType.Private => "private",
                ObjectAclType.PublicRead => "public-read",
                ObjectAclType.PublicReadWrite => "public-read-write",
                ObjectAclType.Default => "default",
                _ => "NO VALUE GIVEN"
            };
        }
    }

    /// <summary>
    /// specifies the encoding method to use
    /// </summary>
    public enum EncodingType
    {
        Url
    }

    public static class EncodingTypeExtensions
    {
        public static string GetString(this EncodingType me)
        {
            return me switch
            {
                EncodingType.Url => "url",
                _ => "NO VALUE GIVEN"
            };
        }
    }

    /// <summary>
    /// The bucket's versioning status
    /// </summary>
    public enum BucketVersioningStatusType
    {
        Enabled,
        Suspended
    }

    public static class BucketVersioningStatusTypeExtensions
    {
        public static string GetString(this BucketVersioningStatusType me)
        {
            return me switch
            {
                BucketVersioningStatusType.Enabled => "Enabled",
                BucketVersioningStatusType.Suspended => "Suspended",
                _ => "NO VALUE GIVEN"
            };
        }
    }
}
