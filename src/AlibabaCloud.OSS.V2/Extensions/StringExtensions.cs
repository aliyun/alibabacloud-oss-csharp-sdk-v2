using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace AlibabaCloud.OSS.V2.Extensions
{
    internal static class StringExtensions
    {

        public static string UrlDecode(this string input) => WebUtility.UrlDecode(input);

        public static string UrlEncode(this string input)
        {
            var encoded = WebUtility.UrlEncode(input);
            return encoded!.Replace("+", "%20");
        }

        private static bool IsUrlSafeChar(char ch)
        {
            if ((ch >= 'a' && ch <= 'z') || (ch >= 'A' && ch <= 'Z') || (ch >= '0' && ch <= '9'))
            {
                return true;
            }

            return ch switch
            {
                '-' or '_' or '.' or '~' or '/' => true,
                _ => false
            };
        }

        public static string UrlEncodePath(this string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;
            var encoded = new StringBuilder(input.Length * 2);
            foreach (char symbol in Encoding.UTF8.GetBytes(input))
            {
                if (IsUrlSafeChar(symbol))
                    encoded.Append(symbol);
                else
                    encoded.Append("%").Append($"{(int)symbol:X2}");
            }
            return encoded.ToString();
        }

        public static bool IsEmpty([NotNullWhen(false)] this string? value) => string.IsNullOrWhiteSpace(value);

        public static bool IsNotEmpty([NotNullWhen(true)] this string? value) => !string.IsNullOrWhiteSpace(value);

        public static string JoinToString(this IEnumerable<string> strings, string separator) => string.Join(separator, strings);

        public static string SafeString(this string? value) => value ?? "";

        public static string AddScheme(this string input, bool disableSsl)
        {
            if (input != "" && !Regex.IsMatch(input, @"^([^:]+)://"))
            {
                var scheme = Defaults.HttpScheme;
                if (disableSsl)
                {
                    scheme = "http";
                }
                return scheme + "://" + input;
            }
            return input;
        }

        public static bool IsValidRegion(this string input)
        {
            return input != "" && Regex.IsMatch(input, @"^[a-z0-9-]+$");
        }

        public static string ToEndpoint(this string input, bool disableSsl, string type)
        {
            var scheme = disableSsl ? "http" : "https";
            var endpoint = type switch
            {
                "internal" => $"oss-{input}-internal.aliyuncs.com",
                "dual-stack" => $"{input}.oss.aliyuncs.com",
                "accelerate" => "oss-accelerate.aliyuncs.com",
                "overseas" => "oss-accelerate-overseas.aliyuncs.com",
                _ => $"oss-{input}.aliyuncs.com",
            };
            return $"{scheme}://{endpoint}";
        }

        public static Uri? ToUri(this string input)
        {
            try
            {
                return input == "" ? null : new Uri(input);
            }
            catch (Exception)
            {
                // ignored
            }

            return null;
        }

        private static Regex _bucketNameRegex = new Regex(@"^[a-z0-9-]+$");
        public static bool IsValidBucketName(this string value)
        {
            if (value.Length < 3 || value.Length > 64) return false;

            if (value.StartsWith("-")) return false;

            if (value.EndsWith("-")) return false;

            return _bucketNameRegex.IsMatch(value);
        }

        public static bool IsValidObjectName(this string value)
        {
            return value.Length is >= 1 and <= 1024;
        }
    }
}
