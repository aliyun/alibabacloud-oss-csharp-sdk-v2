using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;

namespace AlibabaCloud.OSS.V2.Extensions
{
    internal static class UriExtensions
    {

        public static bool IsHostIp(this Uri uri)
        {
            return !string.IsNullOrEmpty(uri.Host) &&
            uri.Host.IndexOf(".", StringComparison.InvariantCulture) >= 0 &&
            IPAddress.TryParse(uri.Host, out _);
        }

        public static Uri AppendToQuery(this Uri uri, string? query)
        {
            if (query == null) return uri;

            var absoluteUri = uri.AbsoluteUri;
            var separator = absoluteUri.Contains('?') ? "&" : "?";

            return new($"{absoluteUri}{separator}{query}");
        }

        public static Uri AppendToPath(this Uri uri, string segment)
        {
            var builder = new UriBuilder(uri);
            var path = builder.Path;
            var seperator = (path.Length == 0 || path[path.Length - 1] != '/') ? "/" : "";
            builder.Path += seperator + segment;
            return builder.Uri;
        }

        public static Uri ReplaceQuery(this Uri uri, string? query)
        {
            if (query == null) return uri;
            var absoluteUri = uri.AbsoluteUri;
            var parts = absoluteUri.Split('?');
            return new(string.Join("?", parts[0], query));
        }

        public static string GetPath(this Uri uri)
        {
            return (uri.AbsolutePath[0] == '/') ?
                uri.AbsolutePath.Substring(1) :
                uri.AbsolutePath;
        }

        public static IDictionary<string, string> GetQueryParameters(this Uri uri)
        {
            var parameters = new Dictionary<string, string>();
            var query = uri.Query;
            if (!string.IsNullOrEmpty(query))
            {
                if (query.StartsWith("?", true, CultureInfo.InvariantCulture))
                {
                    query = query.Substring(1);
                }
                foreach (var param in query.Split(new char[] { '&' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    var parts = param.Split(new char[] { '=' }, 2);
                    var name = parts[0].UrlDecode();
                    if (parts.Length == 1)
                    {
                        parameters.Add(name, "");
                    }
                    else
                    {
                        parameters.Add(name, parts[1].UrlDecode());
                    }
                }
            }
            return parameters;
        }
    }
}
