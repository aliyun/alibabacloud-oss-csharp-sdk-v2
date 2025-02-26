using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using AlibabaCloud.OSS.v2.Extensions;

namespace AlibabaCloud.OSS.v2.Signer {
    public class SignerV4 : ISigner {
        private const string UnsignedPayload  = "UNSIGNED-PAYLOAD";
        private const string DateTimeFormat   = "yyyyMMdd'T'HHmmss'Z'";
        private const string DateFormat       = "yyyyMMdd";
        private const string Rfc822DateFormat = @"ddd, dd MMM yyyy HH:mm:ss \G\M\T";

        public void Sign(SigningContext signingContext) {
            if (signingContext.Request == null) throw new ArgumentException("signingContext.Request is null");

            if (signingContext.Credentials == null) throw new ArgumentException("signingContext.Credentials is null");

            if (signingContext.Region == null) throw new ArgumentException("signingContext.Region is null");

            if (signingContext.Product == null) throw new ArgumentException("signingContext.Product is null");

            if (signingContext.AuthMethodQuery)
                AuthQuery(signingContext);
            else
                AuthHeader(signingContext);
        }

        private static void AuthQuery(SigningContext signingContext) {
            var request     = signingContext.Request;
            var credentials = signingContext.Credentials;
            var region      = signingContext.Region ?? "";
            var product     = signingContext.Product ?? "";

            // Date
            var signTime = signingContext.SignTime ?? DateTime.UtcNow;
            var datetime = FormatDateTime(signTime);
            var date     = FormatDate(signTime);

            // Expiration 
            var expiration = signingContext.Expiration ?? DateTime.UtcNow.AddMinutes(15);
            var expires    = ((long)expiration.Subtract(signTime).TotalSeconds).ToString(CultureInfo.InvariantCulture);

            // Scope
            var scope = $"{date}/{region}/{product}/aliyun_v4_request";

            // Headers
            var headers = request!.Headers;

            var additionalHeaders = GetAdditionalHeaders(headers, signingContext.AdditionalHeaders);
            additionalHeaders.Sort();

            // Credentials information
            var parameters = new Dictionary<string, string>();
            if (credentials!.SecurityToken.IsNotEmpty()) parameters.Add("x-oss-security-token", credentials.SecurityToken);
            parameters.Add("x-oss-signature-version", "OSS4-HMAC-SHA256");
            parameters.Add("x-oss-date", datetime);
            parameters.Add("x-oss-expires", expires);
            parameters.Add("x-oss-credential", $"{credentials.AccessKeyId}/{scope}");

            if (additionalHeaders.Count > 0)
                parameters.Add("x-oss-additional-headers", additionalHeaders.JoinToString(";"));

            // update query 
            var queryStr = parameters
                .Select(
                    x => x.Value.IsEmpty() ? x.Key.UrlEncode() : x.Key.UrlEncode() + "=" + x.Value.UrlEncode()
                )
                .JoinToString("&");
            request.RequestUri = request.RequestUri.AppendToQuery(queryStr);

            // CanonicalRequest
            var canonicalRequest = CanonicalizeRequest(
                request,
                ResourcePath(signingContext.Bucket, signingContext.Key),
                headers,
                additionalHeaders
            );

            // StringToSign
            var stringToSign = CalcStringToSign(datetime, scope, canonicalRequest);

            // Signature
            var signature = CalcSignature(credentials.AccessKeySecret, date, region, product, stringToSign);

            // Credential
            request.RequestUri = request.RequestUri.AppendToQuery($"x-oss-signature={signature.UrlEncode()}");

            //Console.WriteLine("canonicalRequest:{0}\n", canonicalRequest);
            //Console.WriteLine("stringToSign:{0}\n", stringToSign);
            //Console.WriteLine("signature:{0}\n", signature);
            //update
            signingContext.Request      = request;
            signingContext.Expiration   = expiration;
            signingContext.StringToSign = stringToSign;
        }

        private static void AuthHeader(SigningContext signingContext) {
            var request     = signingContext.Request;
            var credentials = signingContext.Credentials;
            var region      = signingContext.Region ?? "";
            var product     = signingContext.Product ?? "";

            // Date
            var signTime    = signingContext.SignTime ?? DateTime.UtcNow;
            var datetime    = FormatDateTime(signTime);
            var date        = FormatDate(signTime);
            var datetimeGmt = FormatRfc822Date(signTime);

            // Scope
            var scope = $"{date}/{region}/{product}/aliyun_v4_request";

            // Credentials information
            if (credentials!.SecurityToken.IsNotEmpty())
                request!.Headers["x-oss-security-token"] = credentials.SecurityToken;

            // Other Headers
            request!.Headers["x-oss-content-sha256"] = UnsignedPayload;
            request.Headers["x-oss-date"]            = datetime;
            request.Headers["Date"]                  = datetimeGmt;

            // lower key & Sorted Headers
            // the headers is OrdinalIgnoreCase
            var headers = request.Headers;

            var additionalHeaders = GetAdditionalHeaders(headers, signingContext.AdditionalHeaders);
            additionalHeaders.Sort();

            // CanonicalRequest
            var canonicalRequest = CanonicalizeRequest(
                request,
                ResourcePath(signingContext.Bucket, signingContext.Key),
                headers,
                additionalHeaders
            );

            // StringToSign
            var stringToSign = CalcStringToSign(datetime, scope, canonicalRequest);

            // Signature
            var signature = CalcSignature(credentials.AccessKeySecret, date, region, product, stringToSign);

            // Credential
            var sb = new StringBuilder();
            sb.AppendFormat("OSS4-HMAC-SHA256 Credential={0}/{1}", credentials.AccessKeyId, scope);
            if (additionalHeaders.Count > 0) sb.AppendFormat(",AdditionalHeaders={0}", additionalHeaders.JoinToString(";"));
            sb.AppendFormat(",Signature={0}", signature);

            request.Headers["Authorization"] = sb.ToString();

            //Console.WriteLine("canonicalRequest:{0}\n", canonicalRequest);
            //Console.WriteLine("stringToSign:{0}\n", stringToSign);
            //Console.WriteLine("signature:{0}\n", signature);

            //update
            signingContext.StringToSign = stringToSign;
        }

        private static string FormatDateTime(DateTime time) {
            return time.ToUniversalTime().ToString(DateTimeFormat, CultureInfo.InvariantCulture);
        }

        private static string FormatDate(DateTime time) {
            return time.ToUniversalTime().ToString(DateFormat, CultureInfo.InvariantCulture);
        }

        public static string FormatRfc822Date(DateTime time) {
            return time.ToUniversalTime().ToString(Rfc822DateFormat, CultureInfo.InvariantCulture);
        }

        private static string ResourcePath(string? bucket, string? key) {
            var resourcePath = "/" + (bucket ?? string.Empty) + (key != null ? "/" + key : "");
            if (bucket != null && key == null) resourcePath = resourcePath + "/";
            return resourcePath;
        }

        private static string CanonicalizeRequest(
            RequestMessage              request,
            string                      resourcePath,
            IDictionary<string, string> headers,
            List<string>                additionalHeaders
        ) {
            /*
                Canonical Request
                HTTP Verb + "\n" +
                Canonical URI + "\n" +
                Canonical Query String + "\n" +
                Canonical Headers + "\n" +
                Additional Headers + "\n" +
                Hashed PayLoad
            */
            var httpMethod = request.Method.ToUpperInvariant();

            // Canonical Uri
            var canonicalUri = resourcePath.UrlEncodePath();

            // Canonical Query
            var sortedParameters = new SortedDictionary<string, string>(StringComparer.Ordinal);

            if (request.RequestUri.Query.IsNotEmpty()) {
                var query                        = request.RequestUri.Query;
                if (query.StartsWith("?")) query = query.Substring(1);

                foreach (var param in query.Split(new char[]{ '&' }, StringSplitOptions.RemoveEmptyEntries)) {
                    var parts = param.Split(new char[] { '=' }, 2);
                    sortedParameters.Add(parts[0], parts.Length == 1 ? "" : parts[1]);
                }
            }

            var sb = new StringBuilder();

            foreach (var p in sortedParameters) {
                if (sb.Length > 0)
                    sb.Append("&");
                sb.AppendFormat("{0}", p.Key);

                if (p.Value.Length > 0)
                    sb.AppendFormat("={0}", p.Value);
            }

            var canonicalQueryString = sb.ToString();

            var canonicalHeaderString = CanonicalizeHeaders(headers, additionalHeaders);

            // Additional Headers
            var additionalHeadersString = additionalHeaders.JoinToString(";");

            var hashBody = CanonicalizeBodyHash(headers);

            var canonicalRequest = new StringBuilder();
            canonicalRequest.AppendFormat("{0}\n", httpMethod);
            canonicalRequest.AppendFormat("{0}\n", canonicalUri);
            canonicalRequest.AppendFormat("{0}\n", canonicalQueryString);
            canonicalRequest.AppendFormat("{0}\n", canonicalHeaderString);
            canonicalRequest.AppendFormat("{0}\n", additionalHeadersString);
            canonicalRequest.AppendFormat("{0}", hashBody);

            return canonicalRequest.ToString();
        }

        private static string CanonicalizeHeaders(IDictionary<string, string> headers, List<string> additionalHeaders) {
            if (headers.Count == 0)
                return string.Empty;

            var addHeadersMap = new Dictionary<string, string>();
            foreach (var header in additionalHeaders) addHeadersMap[header.ToLowerInvariant()] = string.Empty;

            var sortedHeaderMap = new SortedDictionary<string, string>(StringComparer.Ordinal);

            foreach (var header in headers) {
                if (header.Value == null) continue;
                var lowerKey = header.Key.ToLowerInvariant();

                if (IsDefaultSignedHeader(lowerKey) ||
                    addHeadersMap.ContainsKey(lowerKey))
                    sortedHeaderMap[lowerKey] = header.Value.Trim();
            }

            var sb = new StringBuilder();
            foreach (var header in sortedHeaderMap) sb.AppendFormat("{0}:{1}\n", header.Key, header.Value.Trim());

            return sb.ToString();
        }

        private static string CanonicalizeBodyHash(IDictionary<string, string> headers) {
            return headers.TryGetValue("x-oss-content-sha256", out var value) ? value : UnsignedPayload;
        }

        private static bool IsDefaultSignedHeader(string lowerKey) {
            return lowerKey == "content-type" ||
                lowerKey == "content-md5" ||
                lowerKey.StartsWith("x-oss-");
        }

        private static List<string> GetAdditionalHeaders(
            IDictionary<string, string> headers,
            List<string>?               additionalHeaders
        ) {
            var keys = new List<string>();

            if (additionalHeaders == null ||
                additionalHeaders.Count == 0 ||
                headers.Count == 0)
                return keys;

            foreach (var k in additionalHeaders) {
                var lowK = k.ToLowerInvariant();

                if (IsDefaultSignedHeader(lowK))
                    continue;
                else if (headers.ContainsKey(lowK)) keys.Add(lowK);
            }

            return keys;
        }

        private static string CalcStringToSign(string datetime, string scope, string canonicalRequest) {
            /*
            StringToSign
            "OSS4-HMAC-SHA256" + "\n" +
            TimeStamp + "\n" +
            Scope + "\n" +
            Hex(SHA256Hash(Canonical Request))
            */
            using var hash      = SHA256.Create();
            var       hashBytes = hash.ComputeHash(Encoding.UTF8.GetBytes(canonicalRequest));

            return "OSS4-HMAC-SHA256" +
                "\n" +
                datetime +
                "\n" +
                scope +
                "\n" +
                ToHexString(hashBytes, true);
        }

        private static string CalcSignature(
            string accessKeySecret,
            string date,
            string region,
            string product,
            string stringToSign
        ) {
            using var kha = new HMACSHA256();

            var ksecret = Encoding.UTF8.GetBytes("aliyun_v4" + accessKeySecret);

            kha.Key = ksecret;
            var hashDate = kha.ComputeHash(Encoding.UTF8.GetBytes(date));

            kha.Key = hashDate;
            var hashRegion = kha.ComputeHash(Encoding.UTF8.GetBytes(region));

            kha.Key = hashRegion;
            var hashProduct = kha.ComputeHash(Encoding.UTF8.GetBytes(product));

            kha.Key = hashProduct;
            var signingKey = kha.ComputeHash(Encoding.UTF8.GetBytes("aliyun_v4_request"));

            kha.Key = signingKey;
            var signature = kha.ComputeHash(Encoding.UTF8.GetBytes(stringToSign));

            //Console.WriteLine("ksecret:{0}\n", OssUtils.ToHexString(ksecret, true));
            //Console.WriteLine("hashDate:{0}\n", OssUtils.ToHexString(hashDate, true));
            //Console.WriteLine("hashRegion:{0}\n", OssUtils.ToHexString(hashRegion, true));
            //Console.WriteLine("hashProduct:{0}\n", OssUtils.ToHexString(hashProduct, true));
            //Console.WriteLine("signature:{0}\n", OssUtils.ToHexString(signature, true));

            return ToHexString(signature, true);
        }

        internal static string ToHexString(byte[] data, bool lowercase) {
            var sb = new StringBuilder();
            for (var i = 0; i < data.Length; i++) sb.Append(data[i].ToString(lowercase ? "x2" : "X2"));
            return sb.ToString();
        }
    }
}