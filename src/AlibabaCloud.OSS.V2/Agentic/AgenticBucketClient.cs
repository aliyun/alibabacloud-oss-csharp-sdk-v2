using System;
using System.Collections.Generic;
using AlibabaCloud.OSS.V2.Extensions;

namespace AlibabaCloud.OSS.V2.Agentic
{
    /// <summary>
    /// Provides access to the agentic bucket management APIs.
    ///
    /// The client automatically resolves the actual bucket name from the short name
    /// passed in each request. The final bucket name is constructed as
    /// "{bucket}-{accountId}-{region}-ab-apsr", where accountId and region come from
    /// <see cref="Configuration.AccountId"/> and <see cref="Configuration.Region"/>.
    /// For example, with bucket "my-agentic", account "123456" and region "cn-hangzhou",
    /// the resolved name is "my-agentic-123456-cn-hangzhou-ab-apsr". Both
    /// <see cref="Configuration.AccountId"/> and <see cref="Configuration.Region"/> should be set:
    /// an invalid (non-digit) account id does not fail construction; the error is deferred and
    /// surfaced when an operation is invoked.
    /// </summary>
    public sealed partial class AgenticBucketClient : IDisposable
    {
        private const string AgenticBucketSuffix = "ab-apsr";

        private readonly Client _client;

        /// <summary>
        /// Creates a client for the agentic bucket management APIs.
        /// </summary>
        /// <param name="config"><see cref="Configuration"/>The configuration. AccountId and Region should be set; an invalid account id is surfaced when an operation is invoked.</param>
        /// <param name="optFns">Optional client option functions.</param>
        public AgenticBucketClient(Configuration config, params Action<ClientOptions>[] optFns)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));

            var accountId = config.AccountId.SafeString();
            var region = config.Region.SafeString();

            var newCfg = config.Copy();
            newCfg.UserAgent = BuildUserAgent(config.UserAgent);

            var allOptFns = new Action<ClientOptions>[optFns.Length + 1];
            Array.Copy(optFns, allOptFns, optFns.Length);
            allOptFns[optFns.Length] = options => ConfigureProvider(options, accountId, region, AgenticBucketSuffix);

            _client = new Client(newCfg, allOptFns);
        }

        /// <summary>
        /// Creates an <see cref="Client"/> that operates on the bucket spaces of an agentic bucket.
        ///
        /// Pass the short bucket name in each request; the client automatically resolves
        /// it to the full name "{bucket}-{accountId}-{region}-bs-apsr", where accountId
        /// and region come from cfg.AccountId and cfg.Region.
        ///
        /// If you prefer to use a plain <see cref="Client"/> instead, build the full bucket name
        /// yourself with <see cref="BucketSpaceHelper.ToBucketName"/> and pass that as the request Bucket.
        /// </summary>
        /// <param name="config"><see cref="Configuration"/>The configuration. AccountId and Region should be set; an invalid account id is surfaced when an operation is invoked.</param>
        /// <param name="optFns">Optional client option functions.</param>
        public static Client NewBucketSpaceClient(Configuration config, params Action<ClientOptions>[] optFns)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));

            var accountId = config.AccountId.SafeString();
            var region = config.Region.SafeString();

            var newCfg = config.Copy();
            newCfg.UserAgent = BuildUserAgent(config.UserAgent);

            var allOptFns = new Action<ClientOptions>[optFns.Length + 1];
            Array.Copy(optFns, allOptFns, optFns.Length);
            allOptFns[optFns.Length] = options => ConfigureProvider(options, accountId, region, BucketSpaceHelper.BucketSpaceSuffix);

            return new Client(newCfg, allOptFns);
        }

        /// <summary>
        /// The generic operations call.
        /// </summary>
        public System.Threading.Tasks.Task<OperationOutput> InvokeOperationAsync(
            OperationInput input,
            OperationOptions? options = null,
            System.Threading.CancellationToken cancellationToken = default
        )
        {
            return _client.InvokeOperationAsync(input, options, cancellationToken);
        }

        internal static string BuildUserAgent(string? userAgent)
        {
            const string prefix = "agentic-client";
            return userAgent.IsEmpty() ? prefix : $"{prefix}/{userAgent}";
        }

        internal static void ConfigureProvider(ClientOptions options, string accountId, string region, string suffix)
        {
            var provider = new AgenticProvider(options.Endpoint, accountId, region, suffix, options.AddressStyle);
            options.EndpointProvider = provider;
            options.BucketNameResolver = provider;
        }

        public void Dispose()
        {
            _client.Dispose();
        }
    }

    /// <summary>
    /// Resolves the full bucket name and builds the request URL for the agentic clients.
    /// The resolved full bucket name ("{bucket}-{accountId}-{region}-{suffix}") is used for
    /// signing. In virtual-hosted mode (default) it is placed in the host; in path-style mode
    /// it is placed in the path; in virtual-hosted-alias mode the host carries the short
    /// alias label "{bucket}-alias-{suffix}" instead.
    /// </summary>
    internal sealed class AgenticProvider : IEndpointProvider, IBucketNameResolver
    {
        /// <summary>
        /// The literal segment that replaces "{accountId}-{region}" in the short host label.
        /// </summary>
        private const string AliasToken = "alias";

        private readonly Uri? _endpoint;
        private readonly string _accountId;
        private readonly string _region;
        private readonly string _suffix;
        private readonly AddressStyleType _addressStyle;

        public AgenticProvider(Uri? endpoint, string accountId, string region, string suffix, AddressStyleType addressStyle = AddressStyleType.VirtualHosted)
        {
            _endpoint = endpoint;
            _accountId = accountId;
            _region = region;
            _suffix = suffix;
            _addressStyle = addressStyle;
        }

        public string? BuildBucketName(OperationInput input)
        {
            if (input.Bucket == null) return null;
            if (string.IsNullOrEmpty(_accountId))
            {
                throw new ArgumentException("missing required field, AccountId");
            }
            if (string.IsNullOrEmpty(_region))
            {
                throw new ArgumentException("missing required field, Region");
            }
            return $"{input.Bucket}-{_accountId}-{_region}-{_suffix}";
        }

        // Returns an empty string when the endpoint is missing so the caller's URL
        // validation rejects the request instead of a malformed URL being sent.
        public string BuildUrl(OperationInput input)
        {
            if (_endpoint == null) return "";

            var paths = new List<string>();
            var host = _endpoint.Authority;

            if (input.Bucket != null)
            {
                switch (_addressStyle)
                {
                    case AddressStyleType.Path:
                        paths.Add(BuildBucketName(input)!);

                        if (input.Key == null)
                        {
                            paths.Add("");
                        }

                        break;
                    case AddressStyleType.VirtualHostedAlias:
                        var label = $"{input.Bucket}-{AliasToken}-{_suffix}";
                        if (label.Length > 63)
                        {
                            throw new ArgumentException(
                                $"the host label \"{label}\" exceeds the maximum length of 63 characters");
                        }
                        host = $"{label}.{_endpoint.Authority}";
                        break;
                    default:
                        var fullName = BuildBucketName(input)!;
                        if (fullName.Length > 63)
                        {
                            throw new ArgumentException(
                                $"the host label \"{fullName}\" exceeds the maximum length of 63 characters");
                        }
                        host = $"{fullName}.{_endpoint.Authority}";
                        break;
                }
            }

            if (input.Key != null)
            {
                paths.Add(input.Key.UrlEncodePath());
            }

            return $"{_endpoint.Scheme}://{host}/{paths.JoinToString('/')}";
        }
    }

    /// <summary>
    /// Builds full bucket space names for use with a plain <see cref="Client"/>.
    /// </summary>
    public sealed class BucketSpaceHelper
    {
        internal const string BucketSpaceSuffix = "bs-apsr";

        private readonly string _accountId;
        private readonly string _region;

        /// <summary>
        /// Creates a <see cref="BucketSpaceHelper"/> from the account ID and region in the configuration.
        /// </summary>
        public BucketSpaceHelper(Configuration config)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            _accountId = config.AccountId.SafeString();
            _region = config.Region.SafeString();
        }

        /// <summary>
        /// Builds the full bucket space name "{prefix}-{accountId}-{region}-bs-apsr" from a short prefix.
        /// </summary>
        public string ToBucketName(string prefix) => $"{prefix}-{_accountId}-{_region}-{BucketSpaceSuffix}";
    }
}
