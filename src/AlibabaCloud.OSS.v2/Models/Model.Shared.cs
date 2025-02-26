using System.Collections.Generic;
using System.Xml.Serialization;

namespace AlibabaCloud.OSS.v2.Models {

    /// <summary>
    /// Stores information about the owner.
    /// </summary>
    [XmlRoot("Owner")]
    public sealed class Owner {
        /// <summary>
        /// The ID of the owner.
        /// </summary>
        [XmlElement(ElementName = "ID")]
        public string? Id { get; set; }

        /// <summary>
        /// The name of the owner.
        /// </summary>
        [XmlElement(ElementName = "DisplayName")]
        public string? DisplayName { get; set; }
    }

    /// <summary>
    /// Store ACL information.
    /// </summary>
    [XmlRoot("AccessControlList")]
    public sealed class AccessControlList {
        /// <summary>
        /// The ACL of the bucket or object.
        /// Sees <see cref="BucketAclType"/> or <see cref="ObjectAclType"/> for supported values.
        /// </summary>
        [XmlElement(ElementName = "Grant")]
        public string? Grant { get; set; }
    }


    /// <summary>
    /// Store access control policy information.
    /// </summary>
    [XmlRoot("AccessControlPolicy")]
    public sealed class AccessControlPolicy {
        /// <summary>
        /// The container that stores information about the owner.
        /// </summary>
        [XmlElement(ElementName = "Owner")]
        public Owner? Owner { get; set; }

        /// <summary>
        /// The container that stores information about the acl.
        /// </summary>
        [XmlElement(ElementName = "AccessControlList")]
        public AccessControlList? AccessControlList { get; set; }
    }

    /// <summary>
    /// The information about the tag.
    /// </summary>
    [XmlRoot("Tag")]
    public sealed class Tag {
        /// <summary>
        /// The key of the tag.
        /// </summary>
        [XmlElement("Key")]
        public string? Key { get; set; }

        /// <summary>
        /// The value of the tag.
        /// </summary>
        [XmlElement("Value")]
        public string? Value { get; set; }
    }

    /// <summary>
    /// The collection of tags.
    /// </summary>
    [XmlRoot("TagSet")]
    public sealed class TagSet {
        /// <summary>
        /// A list of tags.
        /// </summary>
        [XmlElement("Tag")]
        public List<Tag>? Tags { get; set; }
    }

    /// <summary>
    /// The container used to store the collection of tags.
    /// </summary>
    [XmlRoot("Tagging")]
    public sealed class Tagging {
        /// <summary>
        /// The tag set of the target object.
        /// </summary>
        [XmlElement("TagSet")]
        public TagSet? TagSet { get; set; }
    }
}
