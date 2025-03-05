using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using AlibabaCloud.OSS.V2.Internal;

namespace AlibabaCloud.OSS.V2
{
    public partial class Client
    {
        /// <summary>
        /// Checks if the object exists
        /// </summary>
        /// <param name="bucket">The request parameter to send.</param>
        /// <param name="key">The request parameter to send.</param>
        /// <param name="versionId">Optional, The version ID of the source object.</param>>
        /// <param name="cancellationToken"><see cref="CancellationToken"/>Optional,The cancellation token to cancel.</param>
        /// <returns>True if the object exists, else False.</returns>
        public async Task<bool> IsObjectExistAsync(
            string bucket,
            string key,
            string? versionId = null,
            CancellationToken cancellationToken = default
        )
        {
            try
            {
                await GetObjectMetaAsync(
                    new()
                    {
                        Bucket = bucket,
                        Key = key,
                        VersionId = versionId
                    },
                    null,
                    cancellationToken
                );

                return true;
            }
            catch (OperationException e)
            {
                if (e.InnerException is ServiceException se)
                {
                    if (string.Equals(se.ErrorCode, "NoSuchKey"))
                        return false;

                    if (se.StatusCode == 404 &&
                        string.Equals(se.ErrorCode, "BadErrorResponse"))
                        return false;
                }

                throw;
            }
        }

        /// <summary>
        /// Checks if the bucket exists
        /// </summary>
        /// <param name="bucket">The request parameter to send.</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/>Optional,The cancellation token to cancel.</param>
        /// <returns>True if the object exists, else False.</returns>
        public async Task<bool> IsBucketExistAsync(
            string bucket,
            CancellationToken cancellationToken = default
        )
        {
            try
            {
                await GetBucketAclAsync(
                    new()
                    {
                        Bucket = bucket
                    },
                    null,
                    cancellationToken
                );

                return true;
            }
            catch (OperationException e)
            {
                if (e.InnerException is ServiceException se)
                    if (string.Equals(se.ErrorCode, "NoSuchBucket"))
                        return false;

                throw;
            }
        }

        /// <summary>
        /// Put object from local file.
        /// </summary>
        /// <param name="request"><see cref="Models.PutObjectRequest"/>The request parameter to send.</param>
        /// <param name="filepath">The file path name.</param>
        /// <param name="options"><see cref="OperationOptions"/>Optional, operation options</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/>Optional,The cancellation token to cancel operation.</param>
        /// <returns><see cref="Models.PutObjectResult" />The result instance.</returns>
        public async Task<Models.PutObjectResult> PutObjectFromFileAsync(
            Models.PutObjectRequest request,
            string filepath,
            OperationOptions? options = null,
            CancellationToken cancellationToken = default
        )
        {
#if NET5_0_OR_GREATER
            await using var fs = File.Open(filepath, FileMode.Open);
#else
            using var fs = File.Open(filepath, FileMode.Open);
#endif
            request.Body = fs;
            return await PutObjectAsync(request, options, cancellationToken);
        }

        /// <summary>
        /// Get object to local file.
        /// </summary>
        /// <param name="request"><see cref="Models.GetObjectRequest"/>The request parameter to send.</param>
        /// <param name="filepath">The file path name.</param>
        /// <param name="options"><see cref="OperationOptions"/>Optional, operation options</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/>Optional,The cancellation token to cancel operation.</param>
        /// <returns><see cref="Models.GetObjectResult" />The result instance.</returns>
        public async Task<Models.GetObjectResult> GetObjectToFileAsync(
            Models.GetObjectRequest request,
            string filepath,
            OperationOptions? options = null,
            CancellationToken cancellationToken = default
        )
        {
            var (retry, readTimeout) = _clientImpl.GetRuntimeContext(options);
            Models.GetObjectResult? result;
            Exception? lastEx = null;
            var i = 0;
            WriteOnlyHashStream? crcTracker = null;
            Stream? progTracker = null;
            var trackers = new List<Stream>();
            if (_clientImpl.Options.FeatureFlags.HasFlag(FeatureFlagsType.EnableCrc64CheckDownload))
            {
                crcTracker = new WriteOnlyHashStream(new HashCrc64(0));
                trackers.Add(crcTracker);
            }

            do
            {
                result = await GetObjectAsync(request, options, cancellationToken).ConfigureAwait(false);
                lastEx = null;
                if (request.ProgressFn != null && progTracker == null)
                {
                    progTracker = new ProgressStream(request.ProgressFn, result.ContentLength ?? -1);
                    trackers.Add(progTracker);
                }

                using var cts = new CancellationTokenSource(readTimeout);
                var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, cancellationToken);
                try
                {
                    using var fs = File.Open(filepath, FileMode.Create);
                    byte[] buffer = new byte[Defaults.DefaultCopyBufferSize];
                    int count;

                    while ((count = await result.Body!.ReadAsync(buffer, 0, buffer.Length, linkedCts.Token).ConfigureAwait(false)) != 0)
                    {
                        await fs.WriteAsync(buffer, 0, count, linkedCts.Token).ConfigureAwait(false);
                        foreach (var t in trackers)
                        {
                            t.Write(buffer, 0, count);
                        }
                        cts.CancelAfter(readTimeout);
                    }

                    if (crcTracker != null)
                    {
                        if (result.Headers.TryGetValue("x-oss-hash-crc64ecma", out var scrc))
                        {
                            var val = crcTracker.Hash.Final();
                            var ccrc = Convert.ToString(BitConverter.ToUInt64(val, 0), CultureInfo.InvariantCulture);
                            if (!string.Equals(ccrc, scrc))
                            {
                                result.Headers.TryGetValue("x-oss-request-id", out var requestId);
                                throw new InconsistentException(ccrc, scrc, requestId ?? "");
                            }
                        }
                    }

                    break;
                }
                catch (OperationCanceledException e)
                {
                    lastEx = e;
                    if (cts.IsCancellationRequested)
                    {
                        lastEx = new RequestTimeoutException(
                $"The operation was cancelled because it exceeded the configured timeout of {readTimeout:g}. ", e);
                    }
                    else if (cancellationToken.IsCancellationRequested)
                    {
                        break;
                    }
                }
                catch (Exception e)
                {
                    lastEx = e;
                }
                finally
                {
                    result.Body!.Dispose();
                    result.InnerBody = null;
                    if (lastEx != null)
                    {
                        foreach (var t in trackers)
                        {
                            t.Seek(0, SeekOrigin.Begin);
                        }
                    }
                }
            } while (++i < retry);

            if (lastEx != null)
            {
                throw lastEx;
            }
            return result;
        }
    }
}
