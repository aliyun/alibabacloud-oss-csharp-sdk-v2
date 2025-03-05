using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using AlibabaCloud.OSS.V2.Models;

namespace AlibabaCloud.OSS.V2.Paginator
{
    /// <summary>
    /// A paginator for ListObjectsV2
    /// </summary>
    internal sealed class ListObjectsV2Paginator : IPaginator<ListObjectsV2Result>
    {
        private readonly Client _client;
        private readonly ListObjectsV2Request _request;
        private int _isPaginatorInUse = 0;

        internal ListObjectsV2Paginator(Client client, ListObjectsV2Request request, PaginatorOptions? options)
        {
            _client = client;
            _request = request;

            if (options?.Limit != null) _request.MaxKeys = options.Limit;
        }

        /// <summary>
        /// Iterates over the objects.
        /// </summary>
        public IEnumerable<ListObjectsV2Result> IterPage()
        {
            if (Interlocked.Exchange(ref _isPaginatorInUse, 1) != 0)
                throw new InvalidOperationException(
                    "Paginator has already been consumed and cannot be reused. Please create a new instance."
                );
            var continuationToken = _request.ContinuationToken;
            ListObjectsV2Result result;

            do
            {
                _request.ContinuationToken = continuationToken;
                result = _client.ListObjectsV2Async(_request).GetAwaiter().GetResult();
                continuationToken = result.NextContinuationToken;
                yield return result;
            } while (result.IsTruncated ?? false);
        }

        /// <summary>
        /// Iterates over the objects.
        /// </summary>
        public async IAsyncEnumerable<ListObjectsV2Result> IterPageAsync(
            [EnumeratorCancellation] CancellationToken cancellationToken = default
        )
        {
            if (Interlocked.Exchange(ref _isPaginatorInUse, 1) != 0)
                throw new InvalidOperationException(
                    "Paginator has already been consumed and cannot be reused. Please create a new instance."
                );
            var continuationToken = _request.ContinuationToken;
            ListObjectsV2Result result;

            do
            {
                _request.ContinuationToken = continuationToken;
                result = await _client.ListObjectsV2Async(_request, null, cancellationToken);
                continuationToken = result.NextContinuationToken;
                yield return result;
            } while (result.IsTruncated ?? false);
        }
    }
}
