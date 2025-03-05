using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using AlibabaCloud.OSS.V2.Extensions;

namespace AlibabaCloud.OSS.V2.Signer
{
    public class SignerV1 : ISigner
    {
        private const string Rfc822DateFormat = "ddd, dd MMM yyyy HH:mm:ss \\G\\M\\T";

        private static readonly IList<string> ParametersToSign = new List<string> {
            "acl",
            "bucketInfo",
            "location",
            "stat",
            "delete",
            "append",
            "tagging",
            "objectMeta",
            "uploads",
            "uploadId",
            "partNumber",
            "security-token",
            "position",
            "response-content-type",
            "response-content-language",
            "response-expires",
            "response-cache-control",
            "response-content-disposition",
            "response-content-encoding",
            "restore",
            "callback",
            "callback-var",
            "versions",
            "versioning",
            "versionId",
            "sequential",
            "continuation-token",
            "regionList",
            "cloudboxes",
            "symlink"
        };

        public void Sign(SigningContext signingContext)
        {
            if (signingContext.Request == null) throw new ArgumentException("signingContext.Request is null");

            if (signingContext.Credentials == null) throw new ArgumentException("signingContext.Credentials is null");

            if (signingContext.AuthMethodQuery)
                AuthQuery(signingContext);
            else
                AuthHeader(signingContext);
        }

        private static void AuthQuery(SigningContext signingContext)
        {
            var request = signingContext.Request;
            var credentials = signingContext.Credentials;
            var subResource = signingContext.SubResource;

            // Expiration & Date
            var expiration = signingContext.Expiration ?? DateTime.UtcNow.AddMinutes(15);
            var date = FormatUnixTime(expiration);

            // Headers
            var headers = request!.Headers;

            // Credentials information
            if (credentials!.SecurityToken.IsNotEmpty())
                request.RequestUri =
                    request.RequestUri.AppendToQuery($"security-token={credentials.SecurityToken.UrlEncode()}");

            // StringToSign
            var stringToSign = CalcStringToSign(
                request,
                ResourcePath(signingContext.Bucket, signingContext.Key),
                headers,
                date,
                subResource
            );

            // Signature
            var signature = CalcSignature(credentials.AccessKeySecret, stringToSign);

            // append Signature into request uri
            var queryStr = string.Format(
                "Expires={0}&OSSAccessKeyId={1}&Signature={2}",
                date,
                credentials.AccessKeyId.UrlEncode(),
                signature.UrlEncode()
            );
            request.RequestUri = request.RequestUri.AppendToQuery(queryStr);

            //update
            signingContext.Request = request;
            signingContext.Expiration = expiration;
            signingContext.StringToSign = stringToSign;
        }

        private static void AuthHeader(SigningContext signingContext)
        {
            var request = signingContext.Request;
            var credentials = signingContext.Credentials;
            var subResource = signingContext.SubResource;

            // Date
            var signTime = signingContext.SignTime ?? DateTime.UtcNow;
            var date = FormatRfc822Date(signTime);

            // Credentials information
            if (credentials!.SecurityToken.IsNotEmpty())
                request!.Headers["x-oss-security-token"] = credentials.SecurityToken;

            // Other Headers
            request!.Headers["Date"] = date;

            // Headers
            var headers = request.Headers;

            // StringToSign
            var stringToSign = CalcStringToSign(
                request,
                ResourcePath(signingContext.Bucket, signingContext.Key),
                headers,
                date,
                subResource
            );

            // Signature
            var signature = CalcSignature(credentials.AccessKeySecret, stringToSign);

            //Console.WriteLine(stringToSign);
            //Console.WriteLine(signature);

            // set Signature into header
            request.Headers["Authorization"] = $"OSS {credentials.AccessKeyId}:{signature}";

            //update
            signingContext.StringToSign = stringToSign;
        }

        public static string FormatRfc822Date(DateTime time)
        {
            return time.ToUniversalTime().ToString(Rfc822DateFormat, CultureInfo.InvariantCulture);
        }

        public static string FormatUnixTime(DateTime time)
        {
            const long ticksOf1970 = 621355968000000000;
            return ((time.ToUniversalTime().Ticks - ticksOf1970) / 10000000L).ToString(CultureInfo.InvariantCulture);
        }

        private static string ResourcePath(string? bucket, string? key)
        {
            var resourcePath = "/" + (bucket ?? string.Empty) + (key != null ? "/" + key : "");
            if (bucket != null && key == null) resourcePath = resourcePath + "/";
            return resourcePath;
        }

        private static string CalcStringToSign(
            RequestMessage request,
            string resourcePath,
            IDictionary<string, string> headers,
            string date,
            IList<string>? subResource
        )
        {
            /*
            SignToString =
            VERB + "\n"
            + Content-MD5 + "\n"
            + Content-Type + "\n"
            + Date + "\n"
            + CanonicalizedOSSHeaders
            + CanonicalizedResource
            Signature = base64(hmac-sha1(AccessKeySecret, SignToString))
            */
            var httpMethod = request.Method.ToUpperInvariant();
            var contentMd5 = request.Headers.TryGetValue("Content-MD5", out var value) ? value : "";
            var contentType = request.Headers.TryGetValue("Content-Type", out value) ? value : "";

            // CanonicalizedOSSHeaders
            var canonicalizedOssHeaders = CanonicalizedOssHeaders(headers);

            // CanonicalizedResource
            var canonicalizedResource = CanonicalizedResource(
                resourcePath,
                request.RequestUri.GetQueryParameters(),
                subResource
            );

            var sb = new StringBuilder();
            sb.Append(httpMethod).Append('\n');
            sb.Append(contentMd5).Append('\n');
            sb.Append(contentType).Append('\n');
            sb.Append(date).Append('\n');
            sb.Append(canonicalizedOssHeaders);
            sb.Append(canonicalizedResource);

            return sb.ToString();
        }

        private static string CanonicalizedOssHeaders(IDictionary<string, string> headers)
        {
            var sortedHeaders = new SortedDictionary<string, string>(StringComparer.Ordinal);

            foreach (var header in headers)
            {
                var lowerKey = header.Key.ToLowerInvariant();
                if (lowerKey.StartsWith("x-oss-")) sortedHeaders[lowerKey] = header.Value;
            }

            var sb = new StringBuilder();
            foreach (var header in sortedHeaders) sb.AppendFormat("{0}:{1}\n", header.Key, header.Value.Trim());
            return sb.ToString();
        }

        private static string CanonicalizedResource(
            string resourcePath,
            IDictionary<string, string> parameters,
            IList<string>? subResource
        )
        {
            var canonicalizedResource = new StringBuilder();
            canonicalizedResource.Append(resourcePath);
            var parameterNames = new List<string>(parameters.Keys);
            parameterNames.Sort();

            var separator = '?';

            foreach (var paramName in parameterNames)
            {
                if (!(ParametersToSign.Contains(paramName) ||
                        paramName.StartsWith("x-oss-") ||
                        (subResource != null && subResource.Contains(paramName))))
                    continue;

                canonicalizedResource.Append(separator);
                canonicalizedResource.Append(paramName);
                var paramValue = parameters[paramName];

                if (!string.IsNullOrEmpty(paramValue))
                    canonicalizedResource.Append("=").Append(paramValue);

                separator = '&';
            }

            return canonicalizedResource.ToString();
        }

        private static string CalcSignature(string accessKeySecret, string stringToSign)
        {
            using var algorithm = new HMACSHA1();
            algorithm.Key = Encoding.UTF8.GetBytes(accessKeySecret);
            return Convert.ToBase64String(algorithm.ComputeHash(Encoding.UTF8.GetBytes(stringToSign)));
        }
    }
}
