using System;
using System.Collections.Generic;
using AlibabaCloud.OSS.V2.Transform;

namespace AlibabaCloud.OSS.V2 {
    public partial class Client {
        /// <summary>
        /// Generates the pre-signed URL for GetObject operation.
        /// If you do not specify expiration, the pre-signed URL uses 15 minutes as default.
        /// </summary>
        /// <param name="request"><see cref="Models.GetObjectRequest"/>The request parameter to send.</param>
        /// <param name="expiration">Optional, The expiration time for the generated presign url.</param>
        /// <returns><see cref="Models.PresignResult" />The result instance.</returns>
        public Models.PresignResult Presign(
            Models.GetObjectRequest request,
            DateTime? expiration = null
        ) {
            Ensure.NotNull(request.Bucket, "request.Bucket");
            Ensure.NotNull(request.Key, "request.Key");

            var input = new OperationInput {
                OperationName = "GetObject",
                Method = "GET",
                Bucket = request.Bucket,
                Key = request.Key
            };

            Serde.SerializeInput(request, ref input);

            if (expiration != null) input.OperationMetadata["expiration-time"] = expiration;

            var result = _clientImpl.PresignInner(input);

            return new() {
                Method = result.Method,
                Url = result.Url,
                Expiration = result.Expiration,
                SignedHeaders = result.SignedHeaders
            };
        }

        /// <summary>
        /// Generates the pre-signed URL for PutObject operation.
        /// If you do not specify expiration, the pre-signed URL uses 15 minutes as default.
        /// </summary>
        /// <param name="request"><see cref="Models.PutObjectRequest"/>The request parameter to send.</param>
        /// <param name="expiration">Optional, The expiration time for the generated presign url.</param>
        /// <returns><see cref="Models.PresignResult" />The result instance.</returns>
        public Models.PresignResult Presign(
            Models.PutObjectRequest request,
            DateTime? expiration = null
        ) {
            Ensure.NotNull(request.Bucket, "request.Bucket");
            Ensure.NotNull(request.Key, "request.Key");

            var input = new OperationInput {
                OperationName = "PutObject",
                Method = "PUT",
                Bucket = request.Bucket,
                Key = request.Key
            };

            Serde.SerializeInput(request, ref input, Serde.AddMetadata);

            if (expiration != null) input.OperationMetadata["expiration-time"] = expiration;

            var result = _clientImpl.PresignInner(input);

            return new() {
                Method = result.Method,
                Url = result.Url,
                Expiration = result.Expiration,
                SignedHeaders = result.SignedHeaders
            };
        }

        /// <summary>
        /// Generates the pre-signed URL for HeadObject operation.
        /// If you do not specify expiration, the pre-signed URL uses 15 minutes as default.
        /// </summary>
        /// <param name="request"><see cref="Models.HeadObjectRequest"/>The request parameter to send.</param>
        /// <param name="expiration">Optional, The expiration time for the generated presign url.</param>
        /// <returns><see cref="Models.PresignResult" />The result instance.</returns>
        public Models.PresignResult Presign(
            Models.HeadObjectRequest request,
            DateTime? expiration = null
        ) {
            Ensure.NotNull(request.Bucket, "request.Bucket");
            Ensure.NotNull(request.Key, "request.Key");

            var input = new OperationInput {
                OperationName = "HeadObject",
                Method = "HEAD",
                Bucket = request.Bucket,
                Key = request.Key
            };

            Serde.SerializeInput(request, ref input);

            if (expiration != null) input.OperationMetadata["expiration-time"] = expiration;

            var result = _clientImpl.PresignInner(input);

            return new() {
                Method = result.Method,
                Url = result.Url,
                Expiration = result.Expiration,
                SignedHeaders = result.SignedHeaders
            };
        }

        /// <summary>
        /// Generates the pre-signed URL for InitiateMultipartUpload operation.
        /// If you do not specify expiration, the pre-signed URL uses 15 minutes as default.
        /// </summary>
        /// <param name="request"><see cref="Models.InitiateMultipartUploadRequest"/>The request parameter to send.</param>
        /// <param name="expiration">Optional, The expiration time for the generated presign url.</param>
        /// <returns><see cref="Models.PresignResult" />The result instance.</returns>
        public Models.PresignResult Presign(
            Models.InitiateMultipartUploadRequest request,
            DateTime? expiration = null
        ) {
            Ensure.NotNull(request.Bucket, "request.Bucket");
            Ensure.NotNull(request.Key, "request.Key");

            var input = new OperationInput {
                OperationName = "InitiateMultipartUpload",
                Method = "POST",
                Parameters = new Dictionary<string, string> {
                    { "uploads", "" }
                },
                Bucket = request.Bucket,
                Key = request.Key
            };

            Serde.SerializeInput(request, ref input, Serde.AddMetadata);

            if (expiration != null) input.OperationMetadata["expiration-time"] = expiration;

            var result = _clientImpl.PresignInner(input);

            return new() {
                Method = result.Method,
                Url = result.Url,
                Expiration = result.Expiration,
                SignedHeaders = result.SignedHeaders
            };
        }

        /// <summary>
        /// Generates the pre-signed URL for UploadPart operation.
        /// If you do not specify expiration, the pre-signed URL uses 15 minutes as default.
        /// </summary>
        /// <param name="request"><see cref="Models.UploadPartRequest"/>The request parameter to send.</param>
        /// <param name="expiration">Optional, The expiration time for the generated presign url.</param>
        /// <returns><see cref="Models.PresignResult" />The result instance.</returns>
        public Models.PresignResult Presign(
            Models.UploadPartRequest request,
            DateTime? expiration = null
        ) {
            Ensure.NotNull(request.Bucket, "request.Bucket");
            Ensure.NotNull(request.Key, "request.Key");
            Ensure.NotNull(request.UploadId, "request.UploadId");
            Ensure.NotNull(request.PartNumber, "request.PartNumber");

            var input = new OperationInput {
                OperationName = "UploadPart",
                Method = "PUT",
                Bucket = request.Bucket,
                Key = request.Key
            };

            Serde.SerializeInput(request, ref input);

            if (expiration != null) input.OperationMetadata["expiration-time"] = expiration;

            var result = _clientImpl.PresignInner(input);

            return new() {
                Method = result.Method,
                Url = result.Url,
                Expiration = result.Expiration,
                SignedHeaders = result.SignedHeaders
            };
        }

        /// <summary>
        /// Generates the pre-signed URL for CompleteMultipart operation.
        /// If you do not specify expiration, the pre-signed URL uses 15 minutes as default.
        /// </summary>
        /// <param name="request"><see cref="Models.CompleteMultipartUploadRequest"/>The request parameter to send.</param>
        /// <param name="expiration">Optional, The expiration time for the generated presign url.</param>
        /// <returns><see cref="Models.PresignResult" />The result instance.</returns>
        public Models.PresignResult Presign(
            Models.CompleteMultipartUploadRequest request,
            DateTime? expiration = null
        ) {
            Ensure.NotNull(request.Bucket, "request.Bucket");
            Ensure.NotNull(request.Key, "request.Key");
            Ensure.NotNull(request.UploadId, "request.UploadId");

            var input = new OperationInput {
                OperationName = "CompleteMultipartUpload",
                Method = "POST",
                Bucket = request.Bucket,
                Key = request.Key
            };

            Serde.SerializeInput(request, ref input);

            if (expiration != null) input.OperationMetadata["expiration-time"] = expiration;

            var result = _clientImpl.PresignInner(input);

            return new() {
                Method = result.Method,
                Url = result.Url,
                Expiration = result.Expiration,
                SignedHeaders = result.SignedHeaders
            };
        }

        /// <summary>
        /// Generates the pre-signed URL for AbortMultipartUpload operation.
        /// If you do not specify expiration, the pre-signed URL uses 15 minutes as default.
        /// </summary>
        /// <param name="request"><see cref="Models.AbortMultipartUploadRequest"/>The request parameter to send.</param>
        /// <param name="expiration">Optional, The expiration time for the generated presign url.</param>
        /// <returns><see cref="Models.PresignResult" />The result instance.</returns>
        public Models.PresignResult Presign(
            Models.AbortMultipartUploadRequest request,
            DateTime? expiration = null
        ) {
            Ensure.NotNull(request.Bucket, "request.Bucket");
            Ensure.NotNull(request.Key, "request.Key");
            Ensure.NotNull(request.UploadId, "request.UploadId");

            var input = new OperationInput {
                OperationName = "AbortMultipartUpload",
                Method = "DELETE",
                Bucket = request.Bucket,
                Key = request.Key
            };

            Serde.SerializeInput(request, ref input);

            if (expiration != null) input.OperationMetadata["expiration-time"] = expiration;

            var result = _clientImpl.PresignInner(input);

            return new() {
                Method = result.Method,
                Url = result.Url,
                Expiration = result.Expiration,
                SignedHeaders = result.SignedHeaders
            };
        }
    }
}