using System;
using System.Collections.Generic;

namespace AlibabaCloud.OSS.v2.Extensions {
    static class CollectionExtensions {
        public static void ForEach<T>(this IEnumerable<T> items, Action<T> action) {
            foreach (var item in items) action(item);
        }
    }
}