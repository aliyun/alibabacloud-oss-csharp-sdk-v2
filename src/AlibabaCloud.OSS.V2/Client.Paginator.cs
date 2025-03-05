namespace AlibabaCloud.OSS.V2
{
    public partial class Client
    {
        /// <summary>
        /// Creates a paginator for ListBuckets
        /// </summary>
        /// <param name="request"><see cref="Models.ListBucketsRequest"/>The request parameter to send.</param>
        /// <param name="options"><see cref="OperationOptions"/>Optional, paginator options</param>
        /// <returns>A paginator instance.</returns>
        public Paginator.IPaginator<Models.ListBucketsResult> ListBucketsPaginator(
            Models.ListBucketsRequest request,
            Paginator.PaginatorOptions? options = null
        )
        {
            return new Paginator.ListBucketsPaginator(this, request, options);
        }

        /// <summary>
        /// Creates a paginator for ListObjects
        /// </summary>
        /// <param name="request"><see cref="Models.ListObjectsRequest"/>The request parameter to send.</param>
        /// <param name="options"><see cref="OperationOptions"/>Optional, paginator options</param>
        /// <returns>A paginator instance.</returns>
        public Paginator.IPaginator<Models.ListObjectsResult> ListObjectsPaginator(
            Models.ListObjectsRequest request,
            Paginator.PaginatorOptions? options = null
        )
        {
            return new Paginator.ListObjectsPaginator(this, request, options);
        }

        /// <summary>
        /// Creates a paginator for ListObjectsV2
        /// </summary>
        /// <param name="request"><see cref="Models.ListObjectsV2Request"/>The request parameter to send.</param>
        /// <param name="options"><see cref="OperationOptions"/>Optional, paginator options</param>
        /// <returns>A paginator instance.</returns>
        public Paginator.IPaginator<Models.ListObjectsV2Result> ListObjectsV2Paginator(
            Models.ListObjectsV2Request request,
            Paginator.PaginatorOptions? options = null
        )
        {
            return new Paginator.ListObjectsV2Paginator(this, request, options);
        }

        /// <summary>
        /// Creates a paginator for ListObjectVersions
        /// </summary>
        /// <param name="request"><see cref="Models.ListObjectVersionsRequest"/>The request parameter to send.</param>
        /// <param name="options"><see cref="OperationOptions"/>Optional, paginator options</param>
        /// <returns>A paginator instance.</returns>
        public Paginator.IPaginator<Models.ListObjectVersionsResult> ListObjectVersionsPaginator(
            Models.ListObjectVersionsRequest request,
            Paginator.PaginatorOptions? options = null
        )
        {
            return new Paginator.ListObjectVersionsPaginator(this, request, options);
        }

        /// <summary>
        /// Creates a paginator for ListMultipartUploads
        /// </summary>
        /// <param name="request"><see cref="Models.ListMultipartUploadsRequest"/>The request parameter to send.</param>
        /// <param name="options"><see cref="OperationOptions"/>Optional, paginator options</param>
        /// <returns>A paginator instance.</returns>
        public Paginator.IPaginator<Models.ListMultipartUploadsResult> ListMultipartUploadsPaginator(
            Models.ListMultipartUploadsRequest request,
            Paginator.PaginatorOptions? options = null
        )
        {
            return new Paginator.ListMultipartUploadsPaginator(this, request, options);
        }

        /// <summary>
        /// Creates a paginator for ListParts
        /// </summary>
        /// <param name="request"><see cref="Models.ListPartsRequest"/>The request parameter to send.</param>
        /// <param name="options"><see cref="OperationOptions"/>Optional, paginator options</param>
        /// <returns>A paginator instance.</returns>
        public Paginator.IPaginator<Models.ListPartsResult> ListPartsPaginator(
            Models.ListPartsRequest request,
            Paginator.PaginatorOptions? options = null
        )
        {
            return new Paginator.ListPartsPaginator(this, request, options);
        }
    }
}
