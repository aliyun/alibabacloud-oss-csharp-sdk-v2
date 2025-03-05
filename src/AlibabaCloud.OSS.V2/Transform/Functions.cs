using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Serialization;
using AlibabaCloud.OSS.V2.Extensions;

namespace AlibabaCloud.OSS.V2.Transform
{
    internal static partial class Serde
    {
        public delegate void CustomSerializer(ref Models.RequestModel request, ref OperationInput input);

        public delegate void CustomDeserializer(ref Models.ResultModel result, ref OperationOutput output);

        public static void SerializeInput(
            Models.RequestModel request,
            ref OperationInput input,
            params CustomSerializer[] customSerializer
        )
        {
            // Headers
            if (input.Headers == null && request.Headers.Any())
                input.Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (var h in request.Headers) input.Headers![h.Key] = h.Value;

            // Parameters
            if (input.Parameters == null && request.Parameters.Any()) input.Parameters = new Dictionary<string, string>();

            foreach (var h in request.Parameters) input.Parameters![h.Key] = h.Value;

            // body
            if (request.InnerBody != null)
                input.Body = request.BodyFormat switch
                {
                    "xml" => new MemoryStream(Encoding.UTF8.GetBytes(SerializeXml(request.InnerBody))),
                    _ => request.InnerBody as Stream ??
                        throw new NotImplementedException($"not support body type '{request.BodyFormat}'")
                };

            // custom serializer
            foreach (var serializer in customSerializer) serializer(ref request, ref input);
        }

        static string SerializeXml(object? obj)
        {
            if (obj == null) return "";

            var ns = new XmlSerializerNamespaces();
            ns.Add(string.Empty, string.Empty);
            var serializer = new XmlSerializer(obj.GetType());
            var writer = new Serializers.EncodingStringWriter(Encoding.UTF8);
            serializer.Serialize(writer, obj, ns);
            writer.Flush();
            return writer.ToString();
        }

        public static void AddContentMd5(ref Models.RequestModel _, ref OperationInput input)
        {
            if (input.Headers != null &&
                input.Headers.ContainsKey("Content-MD5"))
                return;

            var md5V = "1B2M2Y8AsgTpgAmY7PhCfg==";

            if (input.Body != null && input.Body.Length > 0)
            {
                var off = input.Body.Position;

                using (var md5 = MD5.Create())
                {
                    md5V = Convert.ToBase64String(md5.ComputeHash(input.Body));
                }

                input.Body.Seek(off, SeekOrigin.Begin);
            }

            input.Headers ??= new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            input.Headers["Content-MD5"] = md5V;
        }

        public static void AddContentType(ref Models.RequestModel _, ref OperationInput input)
        {
            if (input.Headers != null &&
                input.Headers.ContainsKey("Content-Type"))
                return;

            var contentType = MimeUtils.GetMimeType(input.Key ?? "", "application/octet-stream");

            input.Headers ??= new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            input.Headers["Content-Type"] = contentType;
        }

        public static void AddMetadata(ref Models.RequestModel request, ref OperationInput input)
        {
            switch (request)
            {
                case Models.InitiateMultipartUploadRequest req:
                {
                    if (req.Metadata != null)
                    {
                        foreach (var h in req.Metadata)
                        {
                            input.Headers!["x-oss-meta-" + h.Key] = h.Value;
                        }
                    }

                    break;
                }
                case Models.PutObjectRequest req:
                {
                    if (req.Metadata != null)
                    {
                        foreach (var h in req.Metadata)
                        {
                            input.Headers!["x-oss-meta-" + h.Key] = h.Value;
                        }
                    }

                    break;
                }
                case Models.CopyObjectRequest req:
                {
                    if (req.Metadata != null)
                    {
                        foreach (var h in req.Metadata)
                        {
                            input.Headers!["x-oss-meta-" + h.Key] = h.Value;
                        }
                    }

                    break;
                }
                case Models.AppendObjectRequest req:
                {
                    if (req.Metadata != null)
                    {
                        foreach (var h in req.Metadata)
                        {
                            input.Headers!["x-oss-meta-" + h.Key] = h.Value;
                        }
                    }

                    break;
                }
            }
        }

        public static void AddCopySource(ref Models.RequestModel request, ref OperationInput input)
        {
            switch (request)
            {
                case Models.UploadPartCopyRequest req:
                {
                    var bucket = req.SourceBucket ?? req.Bucket;

                    if (bucket != null)
                    {
                        var key = req.SourceKey ?? "";
                        var source = $"/{bucket}/{key.UrlEncodePath()}";

                        if (req.SourceVersionId != null)
                        {
                            source += $"?versionId={req.SourceVersionId}";
                        }

                        if (input.Headers == null)
                        {
                            input.Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                        }

                        input.Headers!["x-oss-copy-source"] = source;
                    }

                    break;
                }
                case Models.CopyObjectRequest req:
                {
                    var bucket = req.SourceBucket ?? req.Bucket;

                    if (bucket != null)
                    {
                        var key = req.SourceKey ?? "";
                        var source = $"/{bucket}/{key.UrlEncodePath()}";

                        if (req.SourceVersionId != null)
                        {
                            source += $"?versionId={req.SourceVersionId}";
                        }

                        if (input.Headers == null)
                        {
                            input.Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                        }

                        input.Headers!["x-oss-copy-source"] = source;
                    }

                    break;
                }
            }
        }

        public static void AddProcessAction(ref Models.RequestModel request, ref OperationInput input)
        {
            switch (request)
            {
                case Models.ProcessObjectRequest req:
                {
                    if (req.Process != null)
                        input.Body = new MemoryStream(Encoding.UTF8.GetBytes($"x-oss-process={req.Process}"));

                    break;
                }
                case Models.AsyncProcessObjectRequest req:
                {
                    if (req.Process != null)
                        input.Body = new MemoryStream(Encoding.UTF8.GetBytes($"x-oss-async-process={req.Process}"));

                    break;
                }
            }
        }

        public static void DeserializeOutput(
            ref Models.ResultModel result,
            ref OperationOutput output,
            params CustomDeserializer[] customDeserializer
        )
        {
            result.Status = output.Status;
            result.StatusCode = output.StatusCode;
            result.Headers = output.Headers ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            // custom serializer
            foreach (var deserializer in customDeserializer) deserializer(ref result, ref output);
        }

        public static void DeserializerAnyBody(ref Models.ResultModel result, ref OperationOutput output)
        {
            if (output.Body == null) return;

            switch (result.BodyFormat)
            {
                case "xml":
                {
                    using var body = output.Body;

                    if (result.BodyType == null)
                    {
                        throw new Exception("body type is null");
                    }

                    var serializer = new XmlSerializer(result.BodyType);
                    result.InnerBody = serializer.Deserialize(body);
                }
                break;
                case "string":
                {
                    using var body = output.Body;
                    var reader = new StreamReader(body);
                    result.InnerBody = reader.ReadToEnd();
                }
                break;
                case "stream":
                {
                    result.InnerBody = output.Body;
                }
                break;
            }
        }

        public static void DeserializerXmlBody(ref Models.ResultModel result, ref OperationOutput output)
        {
            if (output.Body == null) return;

            switch (result.BodyFormat)
            {
                case "xml":
                {
                    if (result.BodyType == null)
                    {
                        throw new Exception("body type is null");
                    }

                    using var body = output.Body;
                    var serializer = new XmlSerializer(result.BodyType);
                    result.InnerBody = serializer.Deserialize(body);
                }
                break;
            }
        }

        public static bool ToBool(string? value)
        {
            return value != null && "true".Equals(value, StringComparison.OrdinalIgnoreCase);
        }

        public static IDictionary<string, string>? ToUserMetadata(IDictionary<string, string>? headers)
        {
            if (headers == null) return null;
            var metadata = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (var item in headers)
            {
                var key = item.Key.ToLowerInvariant();

                if (key.StartsWith("x-oss-meta-"))
                {
                    metadata[key.Substring(11)] = item.Value;
                }
            }

            return metadata;
        }

        /// <summary>
        /// Replaces invalid XML characters in a string with their valid XML equivalent.
        /// </summary>
        public static string? EscapeXml(string? str)
        {
            if (str == null)
                return null;

            StringBuilder? sb = null;

            var strLen = str.Length;
            var newIndex = 0;

            while (true)
            {
                var index = -1;
                var s = "";

                for (var idx = newIndex; idx < strLen; idx++)
                {
                    var ch = str[idx];

                    switch (ch)
                    {
                        case '<':
                            s = "&lt;";
                            index = idx;
                            break;
                        case '>':
                            s = "&gt;";
                            index = idx;
                            break;
                        case '\"':
                            s = "&quot;";
                            index = idx;
                            break;
                        case '\'':
                            s = "&apos;";
                            index = idx;
                            break;
                        case '&':
                            s = "&amp;";
                            index = idx;
                            break;
                        default:
                            if (ch < 0x20)
                            {
                                s = $"&#{(int)ch:D2};";
                                index = idx;
                            }

                            break;
                    }

                    if (index != -1) break;
                }

                if (index == -1)
                {
                    if (sb == null)
                    {
                        return str;
                    }
                    else
                    {
                        sb.Append(str, newIndex, strLen - newIndex);
                        return sb.ToString();
                    }
                }
                else
                {
                    sb ??= new();
                    sb.Append(str, newIndex, index - newIndex);
                    sb.Append(s);
                    newIndex = index + 1;
                }
            }
        }
    }
}
