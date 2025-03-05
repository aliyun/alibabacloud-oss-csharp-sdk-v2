using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using AlibabaCloud.OSS.V2.Models;

namespace AlibabaCloud.OSS.V2.Paginator
{
    /// <summary>
    /// A paginator for ListMultipartUploads
    /// </summary>
    internal sealed class ListMultipartUploadsPaginator : IPaginator<ListMultipartUploadsResult>
    {
        private readonly Client _client;
        private readonly ListMultipartUploadsRequest _request;
        private int _isPaginatorInUse = 0;

        internal ListMultipartUploadsPaginator(
            Client client,
            ListMultipartUploadsRequest request,
            PaginatorOptions? options
        )
        {
            _client = client;
            _request = request;

            if (options?.Limit != null) _request.MaxUploads = options.Limit;
        }

        /// <summary>
        /// Iterates over the multipart uploads.
        /// </summary>
        public IEnumerable<ListMultipartUploadsResult> IterPage()
        {
            if (Interlocked.Exchange(ref _isPaginatorInUse, 1) != 0)
                throw new InvalidOperationException(
                    "Paginator has already been consumed and cannot be reused. Please create a new instance."
                );
            var uploadIdMarker = _request.UploadIdMarker;
            var keyMarker = _request.KeyMarker;
            ListMultipartUploadsResult result;

            do
            {
                _request.UploadIdMarker = uploadIdMarker;
                _request.KeyMarker = keyMarker;
                result = _client.ListMultipartUploadsAsync(_request).GetAwaiter().GetResult();
                uploadIdMarker = result.NextUploadIdMarker;
                keyMarker = result.NextKeyMarker;
                yield return result;
            } while (result.IsTruncated ?? false);
        }

        /// <summary>
        /// Iterates over the multipart uploads.
        /// </summary>
        public async IAsyncEnumerable<ListMultipartUploadsResult> IterPageAsync(
            [EnumeratorCancellation] CancellationToken cancellationToken = default
        )
        {
            if (Interlocked.Exchange(ref _isPaginatorInUse, 1) != 0)
                throw new InvalidOperationException(
                    "Paginator has already been consumed and cannot be reused. Please create a new instance."
                );
            var uploadIdMarker = _request.UploadIdMarker;
            var keyMarker = _request.KeyMarker;
            ListMultipartUploadsResult result;

            do
            {
                _request.UploadIdMarker = uploadIdMarker;
                _request.KeyMarker = keyMarker;
                result = await _client.ListMultipartUploadsAsync(_request, null, cancellationToken);
                uploadIdMarker = result.NextUploadIdMarker;
                keyMarker = result.NextKeyMarker;
                yield return result;
            } while (result.IsTruncated ?? false);
        }
    }
}
