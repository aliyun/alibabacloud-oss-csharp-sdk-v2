using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace AlibabaCloud.OSS.V2.Extensions
{
    internal static partial class StringExtensions
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

        // public static string JoinToString(this IEnumerable<string> strings, string separator) => string.Join(separator, strings);

        public static string JoinToString(this IEnumerable<string> strings, char separator)
#if NET5_0_OR_GREATER
            => string.Join(separator, strings);
#else
            => string.Join(separator.ToString(), strings);
#endif

        public static string SafeString(this string? value) => value ?? string.Empty;

        public static string AddScheme(this string input, bool disableSsl)
        {
            if (input != string.Empty && !RegexUtils.IsValidScheme(input))
            {
                var scheme = disableSsl ? "http" : Defaults.HttpScheme;
                return scheme + "://" + input;
            }
            return input;
        }

        public static bool IsValidRegion(this string input) => input != string.Empty && RegexUtils.IsValidName(input);

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

        public static void EnsureBucketNameValid(this string value, string? paramName = default)
        {
            paramName ??= nameof(value);

            ExceptionUtils.ThrowIfNull(value, paramName);
            ExceptionUtils.ThrowIfOutOfRange(value.Length, 3, 64, paramName);

#if NET5_0_OR_GREATER
            if (value.StartsWith('-') || value.EndsWith('-') || !RegexUtils.IsValidName(value))
#else
            if (value.StartsWith("-", StringComparison.Ordinal) || value.EndsWith("-", StringComparison.Ordinal) || !RegexUtils.IsValidName(value))
#endif
            {
                throw new ArgumentException($"The bucket name [{value}] is invalid.", paramName);
            }
        }

        public static void EnsureObjectNameValid(this string value, string? paramName = default)
        {
            paramName ??= nameof(value);

            ExceptionUtils.ThrowIfNull(value, paramName);
            ExceptionUtils.ThrowIfOutOfRange(value.Length, 1, 1024, paramName);
        }

        private static class ExceptionUtils
        {
            public static void ThrowIfNull(string value, string paramName)
            {
#if NET6_0_OR_GREATER
                ArgumentNullException.ThrowIfNull(value, paramName);
#else
                if (value is null)
                {
                    throw new ArgumentNullException(paramName);
                }
#endif
            }

            public static void ThrowIfOutOfRange(int length, int min, int max, string paramName)
            {
#if NET8_0_OR_GREATER
                ArgumentOutOfRangeException.ThrowIfLessThan(length, min, paramName);
                ArgumentOutOfRangeException.ThrowIfGreaterThan(length, max, paramName);
#else
                if (length < min || length > max)
                {
                    throw new ArgumentOutOfRangeException(paramName, $"The length of the object name must be between 1 and 1024 characters, but was {length}.");
                }
#endif
            }
        }

        private static partial class RegexUtils
        {
#if NET7_0_OR_GREATER
            [GeneratedRegex(@"^[a-z0-9-]+$")]
            private static partial Regex NameCheck();

            [GeneratedRegex(@"^([^:]+)://")]
            private static partial Regex SchemeCheck();
#else
            private static readonly Regex s_nameCheck = new(@"^[a-z0-9-]+$");

            private static readonly Regex s_schemeCheck = new(@"^([^:]+)://");
#endif
            public static bool IsValidName(string value)
#if NET7_0_OR_GREATER
                => NameCheck().IsMatch(value);
#else
                => s_nameCheck.IsMatch(value);
#endif

            public static bool IsValidScheme(string value)
#if NET7_0_OR_GREATER
                => SchemeCheck().IsMatch(value);
#else
                => s_schemeCheck.IsMatch(value);
#endif
        }
    }
}
