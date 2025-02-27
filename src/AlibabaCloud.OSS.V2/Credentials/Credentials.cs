
using System;

namespace AlibabaCloud.OSS.V2.Credentials {
    public class Credentials
    {
        #region Properties

        /// <summary>
        /// Gets the AccessKeyId property for the credentials.
        /// </summary>
        public string AccessKeyId { get; private set; }

        /// <summary>
        /// Gets the AccessKeySecret property for the credentials.
        /// </summary>
        public string AccessKeySecret { get; private set; }

        /// <summary>
        /// Gets the SecurityToken property for the credentials.
        /// </summary>
        public string SecurityToken { get; private set; }

        /// <summary>
        /// Check whether the credentials keys are set
        /// </summary>
        public bool HasKeys => !string.IsNullOrEmpty(AccessKeyId) && !string.IsNullOrEmpty(AccessKeySecret);

        /// <summary>
        /// Gets the token's expiration time.
        /// </summary>
        public DateTime? Expiration { get; private set; }

        /// <summary>
        /// Check whether the credentials have expired.
        /// </summary>
        public bool IsExpired => Expiration != null && DateTime.UtcNow >= Expiration;

        #endregion


        #region Constructors

        /// <summary>
        /// Constructs a Credentials object with supplied accessKeyId, accessKeySecret and securityToken.
        /// </summary>
        /// <param name="accessKeyId"></param>
        /// <param name="accessKeySecret"></param>
        /// <param name="securityToken">Optional. Can be set to null or empty for non-session credentials.</param>
        public Credentials(string accessKeyId, string accessKeySecret, string securityToken)
        {
            AccessKeyId = accessKeyId;
            AccessKeySecret = accessKeySecret;
            SecurityToken = securityToken;
        }

        /// <summary>
        /// Constructs a Credentials object with supplied accessKeyId, accessKeySecret.
        /// </summary>
        /// <param name="accessKeyId"></param>
        /// <param name="accessKeySecret"></param>
        public Credentials(string accessKeyId, string accessKeySecret)
        {
            AccessKeyId = accessKeyId;
            AccessKeySecret = accessKeySecret;
            SecurityToken = string.Empty;
        }

        /// <summary>
        /// Constructs a Credentials object with supplied accessKeyId, accessKeySecret, securityToken and expiration.
        /// </summary>
        /// <param name="accessKeyId"></param>
        /// <param name="accessKeySecret"></param>
        /// <param name="securityToken">Optional. Can be set to null or empty for non-session credentials.</param>
        /// <param name="expiration">Optional.The token's expiration time.</param>
        public Credentials(string accessKeyId, string accessKeySecret, string securityToken, DateTime? expiration) {
            AccessKeyId = accessKeyId;
            AccessKeySecret = accessKeySecret;
            SecurityToken = securityToken;
            Expiration = expiration;
        }

        #endregion
    }


}
