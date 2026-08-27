using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using AlibabaCloud.OSS.V2.Agentic.Models;
using AlibabaCloud.OSS.V2.Paginator;

namespace AlibabaCloud.OSS.V2.Agentic.Paginator
{
    /// <summary>
    /// A paginator for ListAgenticBuckets.
    /// </summary>
    internal sealed class ListAgenticBucketsPaginator : IPaginator<ListAgenticBucketsResult>
    {
        private readonly AgenticBucketClient _client;
        private readonly ListAgenticBucketsRequest _request;
        private int _isPaginatorInUse;

        internal ListAgenticBucketsPaginator(AgenticBucketClient client, ListAgenticBucketsRequest request, PaginatorOptions? options)
        {
            _client = client;
            _request = request;

            if (options?.Limit != null) _request.MaxKeys = options.Limit;
        }

        public IEnumerable<ListAgenticBucketsResult> IterPage()
        {
            if (Interlocked.Exchange(ref _isPaginatorInUse, 1) != 0)
                throw new InvalidOperationException(
                    "Paginator has already been consumed and cannot be reused. Please create a new instance."
                );
            var token = _request.ContinuationToken;
            ListAgenticBucketsResult result;

            do
            {
                _request.ContinuationToken = token;
                result = _client.ListAgenticBucketsAsync(_request).GetAwaiter().GetResult();
                token = result.NextContinuationToken;
                yield return result;
            } while (result.IsTruncated ?? false);
        }

        public async IAsyncEnumerable<ListAgenticBucketsResult> IterPageAsync(
            [EnumeratorCancellation] CancellationToken cancellationToken = default
        )
        {
            if (Interlocked.Exchange(ref _isPaginatorInUse, 1) != 0)
                throw new InvalidOperationException(
                    "Paginator has already been consumed and cannot be reused. Please create a new instance."
                );
            var token = _request.ContinuationToken;
            ListAgenticBucketsResult result;

            do
            {
                _request.ContinuationToken = token;
                result = await _client.ListAgenticBucketsAsync(_request, null, cancellationToken);
                token = result.NextContinuationToken;
                yield return result;
            } while (result.IsTruncated ?? false);
        }
    }

    /// <summary>
    /// A paginator for ListBucketSpaces.
    /// </summary>
    internal sealed class ListBucketSpacesPaginator : IPaginator<ListBucketSpacesResult>
    {
        private readonly AgenticBucketClient _client;
        private readonly ListBucketSpacesRequest _request;
        private int _isPaginatorInUse;

        internal ListBucketSpacesPaginator(AgenticBucketClient client, ListBucketSpacesRequest request, PaginatorOptions? options)
        {
            _client = client;
            _request = request;

            if (options?.Limit != null) _request.MaxKeys = options.Limit;
        }

        public IEnumerable<ListBucketSpacesResult> IterPage()
        {
            if (Interlocked.Exchange(ref _isPaginatorInUse, 1) != 0)
                throw new InvalidOperationException(
                    "Paginator has already been consumed and cannot be reused. Please create a new instance."
                );
            var token = _request.ContinuationToken;
            ListBucketSpacesResult result;

            do
            {
                _request.ContinuationToken = token;
                result = _client.ListBucketSpacesAsync(_request).GetAwaiter().GetResult();
                token = result.NextContinuationToken;
                yield return result;
            } while (result.IsTruncated ?? false);
        }

        public async IAsyncEnumerable<ListBucketSpacesResult> IterPageAsync(
            [EnumeratorCancellation] CancellationToken cancellationToken = default
        )
        {
            if (Interlocked.Exchange(ref _isPaginatorInUse, 1) != 0)
                throw new InvalidOperationException(
                    "Paginator has already been consumed and cannot be reused. Please create a new instance."
                );
            var token = _request.ContinuationToken;
            ListBucketSpacesResult result;

            do
            {
                _request.ContinuationToken = token;
                result = await _client.ListBucketSpacesAsync(_request, null, cancellationToken);
                token = result.NextContinuationToken;
                yield return result;
            } while (result.IsTruncated ?? false);
        }
    }
}

namespace AlibabaCloud.OSS.V2.Agentic
{
    public sealed partial class AgenticBucketClient
    {
        /// <summary>
        /// Creates a paginator for ListAgenticBuckets.
        /// </summary>
        public V2.Paginator.IPaginator<Models.ListAgenticBucketsResult> ListAgenticBucketsPaginator(
            Models.ListAgenticBucketsRequest request,
            V2.Paginator.PaginatorOptions? options = null
        )
        {
            return new Paginator.ListAgenticBucketsPaginator(this, request, options);
        }

        /// <summary>
        /// Creates a paginator for ListBucketSpaces.
        /// </summary>
        public V2.Paginator.IPaginator<Models.ListBucketSpacesResult> ListBucketSpacesPaginator(
            Models.ListBucketSpacesRequest request,
            V2.Paginator.PaginatorOptions? options = null
        )
        {
            return new Paginator.ListBucketSpacesPaginator(this, request, options);
        }
    }
}
