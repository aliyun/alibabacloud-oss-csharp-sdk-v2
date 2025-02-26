using System;
using System.Collections.Generic;
using System.Globalization;
using AlibabaCloud.OSS.v2.Transform;

namespace AlibabaCloud.OSS.v2.Models {
    /// <summary>
    /// The request for the PutSymlink operation.
    /// </summary>
    public sealed class PutSymlinkRequest : RequestModel {
        /// <summary>
        /// The name of the bucket.
        /// </summary>
        public string? Bucket { get; set; }

        /// <summary>
        /// The full path of the object.
        /// </summary>
        public string? Key { get; set; }

        /// <summary>
        /// The target object to which the symbolic link points. The naming conventions for target objects are the same as those for objects.  - Similar to ObjectName, TargetObjectName must be URL-encoded.   - The target object to which a symbolic link points cannot be a symbolic link.
        /// </summary>
        public string? SymlinkTarget {
            get => Headers.TryGetValue("x-oss-symlink-target", out var value) ? value : null;
            set {
                if (value != null) Headers["x-oss-symlink-target"] = value;
            }
        }

        /// <summary>
        /// The access control list (ACL) of the object. Default value: default. Valid values:- default: The ACL of the object is the same as that of the bucket in which the object is stored. - private: The ACL of the object is private. Only the owner of the object and authorized users can read and write this object. - public-read: The ACL of the object is public-read. Only the owner of the object and authorized users can read and write this object. Other users can only read the object. Exercise caution when you set the object ACL to this value. - public-read-write: The ACL of the object is public-read-write. All users can read and write this object. Exercise caution when you set the object ACL to this value.
        /// Sees <see cref="ObjectAclType"/> for supported values.
        /// </summary>
        public string? ObjectAcl {
            get => Headers.TryGetValue("x-oss-object-acl", out var value) ? value : null;
            set {
                if (value != null) Headers["x-oss-object-acl"] = value;
            }
        }

        /// <summary>
        /// The storage class of the bucket. Default value: Standard.  Valid values:- Standard- IA- Archive- ColdArchive
        /// Sees <see cref="StorageClassType"/> for supported values.
        /// </summary>
        public string? StorageClass {
            get => Headers.TryGetValue("x-oss-storage-class", out var value) ? value : null;
            set {
                if (value != null) Headers["x-oss-storage-class"] = value;
            }
        }

        /// <summary>
        /// Specifies whether the PutSymlink operation overwrites the object that has the same name as that of the symbolic link you want to create.   - If the value of **x-oss-forbid-overwrite** is not specified or set to **false**, existing objects can be overwritten by objects that have the same names.   - If the value of **x-oss-forbid-overwrite** is set to **true**, existing objects cannot be overwritten by objects that have the same names. If you specify the **x-oss-forbid-overwrite** request header, the queries per second (QPS) performance of OSS is degraded. If you want to use the **x-oss-forbid-overwrite** request header to perform a large number of operations (QPS greater than 1,000), contact technical support.  The **x-oss-forbid-overwrite** request header is invalid when versioning is enabled or suspended for the destination bucket. In this case, the object with the same name can be overwritten.
        /// </summary>
        public bool? ForbidOverwrite {
            get => Headers.TryGetValue("x-oss-forbid-overwrite", out var value)
                ? Convert.ToBoolean(value, CultureInfo.InvariantCulture)
                : null;
            set {
                if (value != null)
                    Headers["x-oss-forbid-overwrite"] =
                        Convert.ToString(value, CultureInfo.InvariantCulture)!.ToLowerInvariant();
            }
        }
    }

    /// <summary>
    /// The result for the PutSymlink operation.
    /// </summary>
    public sealed class PutSymlinkResult : ResultModel {
        /// <summary>
        /// The object version id.
        /// </summary>
        public string? VersionId => Headers.TryGetValue("x-oss-version-id", out var value) ? value : null;
    }

    /// <summary>
    /// The request for the GetSymlink operation.
    /// </summary>
    public sealed class GetSymlinkRequest : RequestModel {
        /// <summary>
        /// The name of the bucket.
        /// </summary>
        public string? Bucket { get; set; }

        /// <summary>
        /// The full path of the object.
        /// </summary>
        public string? Key { get; set; }

        /// <summary>
        /// The version of the object to which the symbolic link points.
        /// </summary>
        public string? VersionId {
            get => Parameters.TryGetValue("versionId", out var value) ? value : null;
            set {
                if (value != null) Parameters["versionId"] = value;
            }
        }
    }

    /// <summary>
    /// The result for the GetSymlink operation.
    /// </summary>
    public sealed class GetSymlinkResult : ResultModel {
        /// <summary>
        /// Version of the object.
        /// </summary>
        public string? SymlinkTarget => Headers.TryGetValue("x-oss-symlink-target", out var value) ? value : null;

        /// <summary>
        /// Indicates the target object that the symbol link directs to.
        /// </summary>
        public string? VersionId => Headers.TryGetValue("x-oss-version-id", out var value) ? value : null;

        /// <summary>
        /// The entity tag (ETag).
        /// An ETag is created when an object is created to identify the content of the object.
        /// </summary>
        public string? ETag => Headers.TryGetValue("ETag", out var value) ? value : null;

        /// <summary>
        /// A map of metadata to store with the object.
        /// It is a case-insensitive dictionary
        /// </summary>
        public IDictionary<string, string>? Metadata => Serde.ToUserMetadata(Headers);

    }
}