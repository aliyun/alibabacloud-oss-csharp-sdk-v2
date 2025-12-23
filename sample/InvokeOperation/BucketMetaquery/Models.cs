using System.Xml.Serialization;

namespace Sample.InvokeOperation.BucketMetaquery
{
    /// <summary>
    /// 
    /// </summary>
    [XmlRoot("Group")]
    public sealed class MetaQueryGroup
    {

        /// <summary>
        /// 
        /// </summary>
        [XmlElement("Value")]
        public string? Value { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [XmlElement("Count")]
        public long? Count { get; set; }

    }

    /// <summary>
    /// 
    /// </summary>
    [XmlRoot("MetaQueryRespSubtitle")]
    public sealed class MetaQueryRespSubtitle
    {

        /// <summary>
        /// 
        /// </summary>
        [XmlElement("CodecName")]
        public string? CodecName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [XmlElement("Language")]
        public string? Language { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [XmlElement("StartTime")]
        public double? StartTime { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [XmlElement("Duration")]
        public double? Duration { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    [XmlRoot("MetaQueryRespAddress")]
    public sealed class MetaQueryRespAddress
    {

        /// <summary>
        /// 
        /// </summary>
        [XmlElement("Language")]
        public string? Language { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [XmlElement("Province")]
        public string? Province { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [XmlElement("Township")]
        public string? Township { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [XmlElement("AddressLine")]
        public string? AddressLine { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [XmlElement("City")]
        public string? City { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [XmlElement("District")]
        public string? District { get; set; }

    }

    /// <summary>
    /// The container that stores user metadata.
    /// </summary>
    [XmlRoot("MetaQueryUserMeta")]
    public sealed class MetaQueryUserMeta
    {

        /// <summary>
        /// The key of the user metadata item.
        /// </summary>
        [XmlElement("Key")]
        public string? Key { get; set; }

        /// <summary>
        /// The value of the user metadata item.
        /// </summary>
        [XmlElement("Value")]
        public string? Value { get; set; }

    }

    /// <summary>
    /// 
    /// </summary>
    [XmlRoot("MetaQueryRespVideoStream")]
    public sealed class MetaQueryRespVideoStream
    {

        /// <summary>
        /// 
        /// </summary>
        [XmlElement("Duration")]
        public double? Duration { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [XmlElement("FrameCount")]
        public long? FrameCount { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [XmlElement("BitDepth")]
        public long? BitDepth { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [XmlElement("PixelFormat")]
        public string? PixelFormat { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [XmlElement("ColorSpace")]
        public string? ColorSpace { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [XmlElement("CodecName")]
        public string? CodecName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [XmlElement("Bitrate")]
        public long? Bitrate { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [XmlElement("FrameRate")]
        public string? FrameRate { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [XmlElement("Height")]
        public long? Height { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [XmlElement("Width")]
        public long? Width { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [XmlElement("Language")]
        public string? Language { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [XmlElement("StartTime")]
        public double? StartTime { get; set; }

    }

    /// <summary>
    /// 
    /// </summary>
    [XmlRoot("Filters")]
    public sealed class MetaQueryFilters
    {

        /// <summary>
        /// 
        /// </summary>
        [XmlElement("Filter")]
        public List<string>? Filters { get; set; }
    }

    /// <summary>
    /// The container that stores the metadata information.
    /// </summary>
    [XmlRoot("MetaQueryStatus")]
    public sealed class MetaQueryStatus
    {

        /// <summary>
        /// The time when the metadata index library was created. The value follows the RFC 3339 standard in the YYYY-MM-DDTHH:mm:ss+TIMEZONE format. YYYY-MM-DD indicates the year, month, and day. T indicates the beginning of the time element. HH:mm:ss indicates the hour, minute, and second. TIMEZONE indicates the time zone.
        /// </summary>
        [XmlElement("CreateTime")]
        public string? CreateTime { get; set; }

        /// <summary>
        /// The time when the metadata index library was updated. The value follows the RFC 3339 standard in the YYYY-MM-DDTHH:mm:ss+TIMEZONE format. YYYY-MM-DD indicates the year, month, and day. T indicates the beginning of the time element. HH:mm:ss indicates the hour, minute, and second. TIMEZONE indicates the time zone.
        /// </summary>
        [XmlElement("UpdateTime")]
        public string? UpdateTime { get; set; }

        /// <summary>
        /// The status of the metadata index library. Valid values:- Ready: The metadata index library is being prepared after it is created.In this case, the metadata index library cannot be used to query data.- Stop: The metadata index library is paused.- Running: The metadata index library is running.- Retrying: The metadata index library failed to be created and is being created again.- Failed: The metadata index library failed to be created.- Deleted: The metadata index library is deleted.
        /// </summary>
        [XmlElement("State")]
        public string? State { get; set; }

        /// <summary>
        /// The scan type. Valid values:- FullScanning: Full scanning is in progress.- IncrementalScanning: Incremental scanning is in progress.
        /// </summary>
        [XmlElement("Phase")]
        public string? Phase { get; set; }

    }

    /// <summary>
    /// The container that stores the tag information.
    /// </summary>
    [XmlRoot("MetaQueryTagging")]
    public sealed class MetaQueryTagging
    {

        /// <summary>
        /// The tag key.
        /// </summary>
        [XmlElement("Key")]
        public string? Key { get; set; }

        /// <summary>
        /// The tag value.
        /// </summary>
        [XmlElement("Value")]
        public string? Value { get; set; }

    }

    /// <summary>
    /// 
    /// </summary>
    [XmlRoot("MetaQueryRespAudioStream")]
    public sealed class MetaQueryRespAudioStream
    {

        /// <summary>
        /// 
        /// </summary>
        [XmlElement("Duration")]
        public double? Duration { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [XmlElement("Channels")]
        public long? Channels { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [XmlElement("Language")]
        public string? Language { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [XmlElement("CodecName")]
        public string? CodecName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [XmlElement("Bitrate")]
        public long? Bitrate { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [XmlElement("SampleRate")]
        public long? SampleRate { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [XmlElement("StartTime")]
        public double? StartTime { get; set; }

    }

    /// <summary>
    /// 
    /// </summary>
    [XmlRoot("AudioStreams")]
    public sealed class AudioStreams
    {

        /// <summary>
        /// 
        /// </summary>
        [XmlElement("AudioStream")]
        public MetaQueryRespAudioStream? AudioStream { get; set; }

    }

    /// <summary>
    /// 
    /// </summary>
    [XmlRoot("Files")]
    public sealed class MetaQueryFiles
    {

        /// <summary>
        /// 
        /// </summary>
        [XmlElement("File")]
        public List<MetaQueryFile>? Files { get; set; }

    }

    /// <summary>
    /// 
    /// </summary>
    [XmlRoot("Groups")]
    public sealed class MetaQueryGroups
    {

        /// <summary>
        /// 
        /// </summary>
        [XmlElement("Group")]
        public List<MetaQueryGroup>? Groups { get; set; }

    }

    /// <summary>
    /// 
    /// </summary>
    [XmlRoot("Addresses")]
    public sealed class MetaQueryAddresses
    {

        /// <summary>
        /// 
        /// </summary>
        [XmlElement("Address")]
        public MetaQueryRespAddress? Address { get; set; }

    }

    /// <summary>
    /// The list of object tags.
    /// </summary>
    [XmlRoot("OSSTagging")]
    public sealed class OSSTagging
    {

        /// <summary>
        /// The tags.
        /// </summary>
        [XmlElement("Tagging")]
        public List<MetaQueryTagging>? Taggings { get; set; }

    }

    /// <summary>
    /// 
    /// </summary>
    [XmlRoot("MetaQuery")]
    public sealed class MetaQueryResp
    {

        /// <summary>
        /// 
        /// </summary>
        [XmlElement("NextToken")]
        public string? NextToken { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [XmlElement("Files")]
        public MetaQueryFiles? Files { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [XmlElement("Aggregations")]
        public MetaQueryRespAggregations? Aggregations { get; set; }

    }

    /// <summary>
    /// 
    /// </summary>
    [XmlRoot("MetaQueryOpenRequest")]
    public sealed class MetaQueryOpenRequest
    {

        /// <summary>
        /// 
        /// </summary>
        [XmlElement("Filters")]
        public MetaQueryFilters? Filters { get; set; }

    }

    /// <summary>
    /// A short description of struct
    /// </summary>
    [XmlRoot("MetaQueryFile")]
    public sealed class MetaQueryFile
    {

        /// <summary>
        /// 
        /// </summary>
        [XmlElement("ImageWidth")]
        public long? ImageWidth { get; set; }

        /// <summary>
        /// The time when the object was last modified.
        /// </summary>
        [XmlElement("FileModifiedTime")]
        public string? FileModifiedTime { get; set; }

        /// <summary>
        /// The storage class of the object.Valid values:*   Archive        :        the Archive storage class        .*   ColdArchive        :        the Cold Archive storage class        .*   IA        :        the Infrequent Access (IA) storage class        .*   Standard        :        The Standard storage class        .
        /// </summary>
        [XmlElement("OSSStorageClass")]
        public string? OSSStorageClass { get; set; }

        /// <summary>
        /// The number of the tags of the object.
        /// </summary>
        [XmlElement("OSSTaggingCount")]
        public long? OSSTaggingCount { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [XmlElement("URI")]
        public string? URI { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [XmlElement("MediaType")]
        public string? MediaType { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [XmlElement("CacheControl")]
        public string? CacheControl { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [XmlElement("ImageHeight")]
        public long? ImageHeight { get; set; }

        /// <summary>
        /// The server-side encryption algorithm used when the object was created.
        /// </summary>
        [XmlElement("ServerSideEncryptionCustomerAlgorithm")]
        public string? ServerSideEncryptionCustomerAlgorithm { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [XmlElement("Composer")]
        public string? Composer { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [XmlElement("AudioStreams")]
        public AudioStreams? AudioStreams { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [XmlElement("LatLong")]
        public string? LatLong { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [XmlElement("AccessControlRequestMethod")]
        public string? AccessControlRequestMethod { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [XmlElement("ServerSideDataEncryption")]
        public string? ServerSideDataEncryption { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [XmlElement("ServerSideEncryptionKeyId")]
        public string? ServerSideEncryptionKeyId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [XmlElement("AlbumArtist")]
        public string? AlbumArtist { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [XmlElement("Subtitles")]
        public Subtitles? Subtitles { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [XmlElement("ProduceTime")]
        public string? ProduceTime { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [XmlElement("OSSExpiration")]
        public string? OSSExpiration { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [XmlElement("VideoWidth")]
        public long? VideoWidth { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [XmlElement("Duration")]
        public double? Duration { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [XmlElement("Addresses")]
        public MetaQueryAddresses? Addresses { get; set; }

        /// <summary>
        /// The type of the object.Valid values:*   Multipart        :        The object is uploaded by using multipart upload        .*   Symlink        :        The object is a symbolic link that was created by calling the PutSymlink operation.    *   Appendable        :        The object is uploaded by using AppendObject        .*   Normal        :        The object is uploaded by using PutObject        .
        /// </summary>
        [XmlElement("OSSObjectType")]
        public string? OSSObjectType { get; set; }

        /// <summary>
        /// The access control list (ACL) of the object.Valid values:*   default        :        the ACL of the bucket        .*   private        :        private        .*   public-read        :        public-read        .*   public-read-write        :        public-read-write        .
        /// </summary>
        [XmlElement("ObjectACL")]
        public string? ObjectACL { get; set; }

        /// <summary>
        /// The ETag of the object.
        /// </summary>
        [XmlElement("ETag")]
        public string? ETag { get; set; }

        /// <summary>
        /// The CRC-64 value of the object.
        /// </summary>
        [XmlElement("OSSCRC64")]
        public string? OSSCRC64 { get; set; }

        /// <summary>
        /// The list of object tags.
        /// </summary>
        [XmlElement("OSSTagging")]
        public OSSTagging? OSSTagging { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [XmlElement("AccessControlAllowOrigin")]
        public string? AccessControlAllowOrigin { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [XmlElement("ContentLanguage")]
        public string? ContentLanguage { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [XmlElement("ContentEncoding")]
        public string? ContentEncoding { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [XmlElement("VideoHeight")]
        public long? VideoHeight { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [XmlElement("Bitrate")]
        public long? Bitrate { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [XmlElement("Artist")]
        public string? Artist { get; set; }

        /// <summary>
        /// The full path of the object.
        /// </summary>
        [XmlElement("Filename")]
        public string? Filename { get; set; }

        /// <summary>
        /// The object size.
        /// </summary>
        [XmlElement("Size")]
        public long? Size { get; set; }

        /// <summary>
        /// The server-side encryption of the object.
        /// </summary>
        [XmlElement("ServerSideEncryption")]
        public string? ServerSideEncryption { get; set; }

        /// <summary>
        /// The container that stores user metadata.
        /// </summary>
        [XmlElement("OSSUserMeta")]
        public OSSUserMeta? OSSUserMeta { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [XmlElement("ContentType")]
        public string? ContentType { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [XmlElement("Performer")]
        public string? Performer { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [XmlElement("VideoStreams")]
        public VideoStreams? VideoStreams { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [XmlElement("Title")]
        public string? Title { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [XmlElement("ContentDisposition")]
        public string? ContentDisposition { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [XmlElement("Album")]
        public string? Album { get; set; }

    }

    /// <summary>
    /// 
    /// </summary>
    [XmlRoot("Aggregation")]
    public sealed class MetaQueryRespAggregation
    {

        /// <summary>
        /// 
        /// </summary>
        [XmlElement("Operation")]
        public string? Operation { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [XmlElement("Value")]
        public double? Value { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [XmlElement("Groups")]
        public MetaQueryGroups? Groups { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [XmlElement("Field")]
        public string? Field { get; set; }

    }

    /// <summary>
    /// 
    /// </summary>
    [XmlRoot("Aggregations")]
    public sealed class MetaQueryRespAggregations
    {

        /// <summary>
        /// 
        /// </summary>
        [XmlElement("Aggregation")]
        public List<MetaQueryRespAggregation>? Aggregations { get; set; }

    }

    /// <summary>
    /// 
    /// </summary>
    [XmlRoot("Subtitles")]
    public sealed class Subtitles
    {

        /// <summary>
        /// 
        /// </summary>
        [XmlElement("Subtitle")]
        public MetaQueryRespSubtitle? Subtitle { get; set; }

    }

    /// <summary>
    /// The container that stores user metadata.
    /// </summary>
    [XmlRoot("OSSUserMeta")]
    public sealed class OSSUserMeta
    {

        /// <summary>
        /// The user metadata items.
        /// </summary>
        [XmlElement("UserMeta")]
        public List<MetaQueryUserMeta>? UserMetas { get; set; }

    }

    /// <summary>
    /// 
    /// </summary>
    [XmlRoot("VideoStreams")]
    public sealed class VideoStreams
    {

        /// <summary>
        /// 
        /// </summary>
        [XmlElement("VideoStream")]
        public MetaQueryRespVideoStream? VideoStream { get; set; }

    }


    /// <summary>
    /// 
    /// </summary>
    [XmlRoot("Aggregation")]
    public sealed class MetaQueryAggregation
    {

        /// <summary>
        /// 
        /// </summary>
        [XmlElement("Operation")]
        public string? Operation { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [XmlElement("Field")]
        public string? Field { get; set; }

    }

    /// <summary>
    /// 
    /// </summary>
    [XmlRoot("Aggregations")]
    public sealed class MetaQueryAggregations
    {

        /// <summary>
        /// 
        /// </summary>
        [XmlElement("Aggregation")]
        public List<MetaQueryAggregation>? Aggregations { get; set; }

    }

    [XmlRoot("MediaTypes")]
    public sealed class MetaQueryMediaTypes
    {
        /// <summary>
        /// 
        /// </summary>
        [XmlElement("MediaType")]
        public List<string>? MediaTypes { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    [XmlRoot("MetaQuery")]
    public sealed class MetaQuery
    {

        [XmlElement("Aggregations")]
        public MetaQueryAggregations? Aggregations { get; set; }

        [XmlElement("NextToken")]
        public string? NextToken { get; set; }

        [XmlElement("MaxResults")]
        public long? MaxResults { get; set; }

        [XmlElement("Query")]
        public string? Query { get; set; }

        [XmlElement("Sort")]
        public string? Sort { get; set; }

        [XmlElement("Order")]
        public string? Order { get; set; }

        [XmlElement("MediaTypes")]
        public MetaQueryMediaTypes? MediaTypes { get; set; }

        [XmlElement("SimpleQuery")]
        public string? SimpleQuery { get; set; }
    }

}
