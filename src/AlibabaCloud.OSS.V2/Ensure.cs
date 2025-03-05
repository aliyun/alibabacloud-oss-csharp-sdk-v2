
using System;

namespace AlibabaCloud.OSS.V2
{
    static class Ensure
    {
        public static T NotNull<T>(T? value, string name) => value ?? throw new ArgumentNullException(name);

        public static string NotEmptyString(object? value, string name)
        {
            var s = value as string ?? value?.ToString();
            if (s == null) throw new ArgumentNullException(name);

            return string.IsNullOrWhiteSpace(s) ? throw new ArgumentException("Parameter cannot be an empty string", name) : s;
        }
    }
}
