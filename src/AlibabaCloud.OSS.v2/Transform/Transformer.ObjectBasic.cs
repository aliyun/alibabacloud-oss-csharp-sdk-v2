using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using AlibabaCloud.OSS.v2.Extensions;

namespace AlibabaCloud.OSS.v2.Transform {
    [XmlRoot("CopyObjectResult")]
    public sealed class XmlCopyObjectResult {
        [XmlElement("LastModified")]
        public DateTime? LastModified { get; set; }

        [XmlElement("ETag")]
        public string? ETag { get; set; }
    }

    [XmlRoot("DeleteResult")]
    public sealed class XmlDeleteResult {
        [XmlElement("EncodingType")]
        public string? EncodingType { get; set; }

        [XmlElement("Deleted")]
        public List<Models.DeletedInfo>? DeletedObjects { get; set; }
    }

    internal static partial class Serde {
        public static void DeserializeCopyObject(
            ref Models.ResultModel baseResult,
            ref OperationOutput    output
        ) {
            var       serializer = new XmlSerializer(typeof(XmlCopyObjectResult));
            using var body       = output.Body!;
            var       obj        = serializer.Deserialize(body) as XmlCopyObjectResult;
            var       result     = baseResult as Models.CopyObjectResult;

            if (obj == null || result == null) return;

            result.ETag         = obj.ETag;
            result.LastModified = obj.LastModified;
        }

        public static void SerializeDeleteMultipleObjects(
            ref Models.RequestModel req,
            ref OperationInput      input
        ) {
            if (req is not Models.DeleteMultipleObjectsRequest request) {
                throw new InvalidCastException($"not DeleteMultipleObjectsRequest type, got '{req.GetType()}'");
            }

            // xml body
            var sb = new StringBuilder();
            sb.Append("<Delete>");

            if (request.Quiet != null) {
                sb.Append($"<Quiet>{Convert.ToString((bool)request.Quiet).ToLowerInvariant()}</Quiet>");
            }

            if (request.Objects != null) {
                foreach (var o in request.Objects) {
                    sb.Append("<Object>");

                    if (!string.IsNullOrEmpty(o.Key)) {
                        sb.Append($"<Key>{EscapeXml(o.Key)}</Key>");
                    }

                    if (!string.IsNullOrEmpty(o.VersionId)) {
                        sb.Append($"<VersionId>{o.VersionId}</VersionId>");
                    }

                    sb.Append("</Object>");
                }
            }

            sb.Append("</Delete>");

            input.Body = new MemoryStream(Encoding.UTF8.GetBytes(sb.ToString()));
        }

        public static void DeserializeDeleteMultipleObjects(
            ref Models.ResultModel baseResult,
            ref OperationOutput    output
        ) {
            // empty body
            using var body = output.Body;
            if (body == null || body.Length == 0) {
                return;
            }

            // non-empty body
            var serializer = new XmlSerializer(typeof(XmlDeleteResult));
            var       obj        = serializer.Deserialize(body) as XmlDeleteResult;
            var       result     = baseResult as Models.DeleteMultipleObjectsResult;

            if (obj == null || result == null) return;

            DeserializeDeleteMultipleObjectsEncodingType(ref obj);

            result.DeletedObjects = obj.DeletedObjects;
            result.EncodingType   = obj.EncodingType;
        }

        private static void DeserializeDeleteMultipleObjectsEncodingType(ref XmlDeleteResult result) {
            if (!string.Equals("url", result.EncodingType)) {
                return;
            }

            if (result.DeletedObjects != null) {
                for (int i = 0; i < result.DeletedObjects.Count; i++) {
                    if (result.DeletedObjects[i].Key != null) {
                        result.DeletedObjects[i].Key = result.DeletedObjects[i].Key!.UrlDecode();
                    }
                }
            }
        }
    }
}