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
        public static string UrlDecode(this string input) => WebUtility.UrlDecode(input); //  Uri.UnescapeDataString(input)?

        public static string UrlEncode(this string input)
        {
            var encoded = WebUtility.UrlEncode(input);
            return encoded!.Replace("+", "%20"); // Should use Uri.EscapeDataString(input)?
        }

        private static bool IsUrlSafeChar(char ch)
        {
            if (IsAsciiLetterOrDigit(ch))
            {
                return true;
            }

            return ch is '-' or '_' or '.' or '~' or '/';

#if NET7_0_OR_GREATER
            static bool IsAsciiLetterOrDigit(char c) => char.IsAsciiLetterOrDigit(c);
#else
            static bool IsAsciiLetterOrDigit(char c) => IsAsciiLetter(c) | IsBetween(c, '0', '9');
            static bool IsAsciiLetter(char c) => (uint)((c | 0x20) - 'a') <= 'z' - 'a';
            static bool IsBetween(char c, char minInclusive, char maxInclusive) => (uint)(c - minInclusive) <= (uint)(maxInclusive - minInclusive);
#endif
        }

        public static string UrlEncodePath(this string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;
            var inputBytes = Encoding.UTF8.GetBytes(input);
            var encoded = new StringBuilder(inputBytes.Length);
            foreach (var ib in inputBytes)
            {
                var symbol = (char)ib;
                if (IsUrlSafeChar(symbol))
                    encoded.Append(symbol);
                else
                    encoded.Append('%').Append(ib.ToString("X2"));
            }
            return encoded.ToString();
        }

        public static bool IsEmpty([NotNullWhen(false)] this string? value) => string.IsNullOrWhiteSpace(value);

        public static bool IsNotEmpty([NotNullWhen(true)] this string? value) => !string.IsNullOrWhiteSpace(value);

        public static string JoinToString(this IEnumerable<string> strings, string separator) => string.Join(separator, strings);

        public static string SafeString(this string? value) => value ?? string.Empty;

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
            if (string.IsNullOrEmpty(input))
            {
                return default;
            }

            try
            {
                return new Uri(input);
            }
            catch (Exception)
            {
                // ignored
            }
            return default;
        }

        private static readonly Regex s_bucketNameRegex = new(@"^[a-z0-9-]+$");
        public static bool IsValidBucketName(this string value)
        {
            if (value.Length < 3 || value.Length > 64) return false;

            if (value.StartsWith("-")) return false;

            if (value.EndsWith("-")) return false;

            return s_bucketNameRegex.IsMatch(value);
        }

        public static void EnsureObjectNameValid(this string value, string? paramName = default)
        {
            paramName ??= nameof(value);
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(value, paramName);
#else
            if (value is null)
            {
                throw new ArgumentNullException(paramName);
            }
#endif

#if NET8_0_OR_GREATER
            ArgumentOutOfRangeException.ThrowIfLessThan(value.Length, 1, paramName);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(value.Length, 1024, paramName);
#else
            if (value.Length is < 1 or > 1024)
            {
                throw new ArgumentOutOfRangeException(paramName, $"The length of the object name must be between 1 and 1024 characters, but was {value.Length}.");
            }
#endif
        }
    }
}
