using System.Reflection;
using System.Xml.Serialization;

namespace Sample.InvokeOperation.BucketCname
{

    /// <summary>
    /// The information about the certificate.
    /// </summary>
    [XmlRoot("CnameCertificate")]
    public sealed class CnameCertificate
    {

        /// <summary>
        /// The time when the certificate was bound.
        /// </summary>
        [XmlElement("CreationDate")]
        public string? CreationDate { get; set; }

        /// <summary>
        /// The signature of the certificate.
        /// </summary>
        [XmlElement("Fingerprint")]
        public string? Fingerprint { get; set; }

        /// <summary>
        /// The time when the certificate takes effect.
        /// </summary>
        [XmlElement("ValidStartDate")]
        public string? ValidStartDate { get; set; }

        /// <summary>
        /// The time when the certificate expires.
        /// </summary>
        [XmlElement("ValidEndDate")]
        public string? ValidEndDate { get; set; }

        /// <summary>
        /// The source of the certificate.Valid values:*   CAS            *   Upload            
        /// </summary>
        [XmlElement("Type")]
        public string? Type { get; set; }

        /// <summary>
        /// The ID of the certificate.
        /// </summary>
        [XmlElement("CertId")]
        public string? CertId { get; set; }

        /// <summary>
        /// The status of the certificate.Valid values:*   Enabled            *   Disabled            
        /// </summary>
        [XmlElement("Status")]
        public string? Status { get; set; }

    }

    /// <summary>
    /// The container that stores the CNAME token.
    /// </summary>
    [XmlRoot("CnameToken")]
    public sealed class CnameToken
    {

        /// <summary>
        /// The name of the bucket to which the CNAME record is mapped.
        /// </summary>
        [XmlElement("Bucket")]
        public string? Bucket { get; set; }

        /// <summary>
        /// The name of the CNAME record that is mapped to the bucket.
        /// </summary>
        [XmlElement("Cname")]
        public string? Cname { get; set; }

        /// <summary>
        /// The CNAME token that is returned by OSS.
        /// </summary>
        [XmlElement("Token")]
        public string? Token { get; set; }

        /// <summary>
        /// The time when the CNAME token expires.
        /// </summary>
        [XmlElement("ExpireTime")]
        public string? ExpireTime { get; set; }

    }

    /// <summary>
    /// The container for which the certificate is configured.
    /// </summary>
    [XmlRoot("CertificateConfiguration")]
    public sealed class CertificateConfiguration
    {

        /// <summary>
        /// The ID of the certificate.
        /// </summary>
        [XmlElement("CertId")]
        public string? CertId { get; set; }

        /// <summary>
        /// The public key of the certificate.
        /// </summary>
        [XmlElement("Certificate")]
        public string? Certificate { get; set; }

        /// <summary>
        /// The private key of the certificate.
        /// </summary>
        [XmlElement("PrivateKey")]
        public string? PrivateKey { get; set; }

        /// <summary>
        /// The ID of the certificate. If the Force parameter is not set to true, the OSS server checks whether the value of the Force parameter matches the current certificate ID. If the value does not match the certificate ID, an error is returned.noticeIf you do not specify the PreviousCertId parameter when you bind a certificate, you must set the Force parameter to true./notice
        /// </summary>
        [XmlElement("PreviousCertId")]
        public string? PreviousCertId { get; set; }

        /// <summary>
        /// Specifies whether to overwrite the certificate. Valid values:- true: overwrites the certificate.- false: does not overwrite the certificate.
        /// </summary>
        [XmlElement("Force")]
        public bool? Force { get; set; }

        /// <summary>
        /// Specifies whether to delete the certificate. Valid values:- true: deletes the certificate.- false: does not delete the certificate.
        /// </summary>
        [XmlElement("DeleteCertificate")]
        public bool? DeleteCertificate { get; set; }

    }

    /// <summary>
    /// The information about the CNAME records.
    /// </summary>
    [XmlRoot("CnameInfo")]
    public sealed class CnameInfo
    {

        /// <summary>
        /// The container in which the certificate information is stored.
        /// </summary>
        [XmlElement("Certificate")]
        public CnameCertificate? Certificate { get; set; }

        /// <summary>
        /// The custom domain name.
        /// </summary>
        [XmlElement("Domain")]
        public string? Domain { get; set; }

        /// <summary>
        /// The time when the custom domain name was mapped.
        /// </summary>
        [XmlElement("LastModified")]
        public string? LastModified { get; set; }

        /// <summary>
        /// The status of the domain name. Valid values:*   Enabled*   Disabled
        /// </summary>
        [XmlElement("Status")]
        public string? Status { get; set; }

    }

    /// <summary>
    /// The container that stores the CNAME information.
    /// </summary>
    [XmlRoot("Cname")]
    public sealed class Cname
    {

        /// <summary>
        /// The custom domain name.
        /// </summary>
        [XmlElement("Domain")]
        public string? Domain { get; set; }

        /// <summary>
        /// The container for which the certificate is configured.
        /// </summary>
        [XmlElement("CertificateConfiguration")]
        public CertificateConfiguration? CertificateConfiguration { get; set; }

    }

    /// <summary>
    /// The container that stores the CNAME record.
    /// </summary>
    [XmlRoot("BucketCnameConfiguration")]
    public sealed class BucketCnameConfiguration
    {

        /// <summary>
        /// The container that stores the CNAME information.
        /// </summary>
        [XmlElement("Cname")]
        public Cname? Cname { get; set; }

    }

    /// <summary>
    /// The result for the ListCname operation.
    /// </summary>
    [XmlRoot("ListCnameResult")]
    public sealed class ListCnameResult
    {
        [XmlElement("Bucket")]
        public string? Bucket { get; set; }

        [XmlElement("Cname")]
        public List<CnameInfo>? Cnames { get; set; }
    }
}
