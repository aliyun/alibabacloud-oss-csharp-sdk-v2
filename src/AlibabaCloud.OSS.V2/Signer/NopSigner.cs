﻿
namespace AlibabaCloud.OSS.V2.Signer
{
    public class NopSigner : ISigner
    {
        public void Sign(SigningContext signingContext)
        {
        }
    }
}
