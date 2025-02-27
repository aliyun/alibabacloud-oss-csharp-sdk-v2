namespace AlibabaCloud.OSS.V2.Signer {
    public interface ISigner
    {
        public void Sign(SigningContext signingContext);
    }
}
