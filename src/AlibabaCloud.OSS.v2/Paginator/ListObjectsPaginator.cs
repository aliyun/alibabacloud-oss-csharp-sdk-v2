using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using AlibabaCloud.OSS.v2.Models;

namespace AlibabaCloud.OSS.v2.Paginator {
    /// <summary>
    /// A paginator for ListObjects
    /// </summary>
    internal sealed class ListObjectsPaginator : IPaginator<ListObjectsResult> {
        private readonly Client             _client;
        private readonly ListObjectsRequest _request;
        private          int                _isPaginatorInUse = 0;

        internal ListObjectsPaginator(Client client, ListObjectsRequest request, PaginatorOptions? options) {
            _client  = client;
            _request = request;

            if (options?.Limit != null) _request.MaxKeys = options.Limit;
        }

        /// <summary>
        /// Iterates over the objects.
        /// </summary>
        public IEnumerable<ListObjectsResult> IterPage() {
            if (Interlocked.Exchange(ref _isPaginatorInUse, 1) != 0)
                throw new InvalidOperationException(
                    "Paginator has already been consumed and cannot be reused. Please create a new instance."
                );
            var               marker = _request.Marker;
            ListObjectsResult result;

            do {
                _request.Marker = marker;
                result          = _client.ListObjectsAsync(_request).GetAwaiter().GetResult();
                marker          = result.NextMarker;
                yield return result;
            } while (result.IsTruncated ?? false);
        }

        /// <summary>
        /// Iterates over the objects.
        /// </summary>
        public async IAsyncEnumerable<ListObjectsResult> IterPageAsync(
            [EnumeratorCancellation] CancellationToken cancellationToken = default
        ) {
            if (Interlocked.Exchange(ref _isPaginatorInUse, 1) != 0)
                throw new InvalidOperationException(
                    "Paginator has already been consumed and cannot be reused. Please create a new instance."
                );
            var               marker = _request.Marker;
            ListObjectsResult result;

            do {
                _request.Marker = marker;
                result          = await _client.ListObjectsAsync(_request, null, cancellationToken);
                marker          = result.NextMarker;
                yield return result;
            } while (result.IsTruncated ?? false);
        }
    }
}