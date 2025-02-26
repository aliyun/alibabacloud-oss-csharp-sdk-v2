using System.Collections.Generic;
using System.Xml.Serialization;

namespace AlibabaCloud.OSS.v2.Models {
    /// <summary>
    /// The information about the region.
    /// </summary>
    [XmlRoot("RegionInfo")]
    public sealed class RegionInfo {
        /// <summary>
        /// The internal endpoint of the region.
        /// </summary>
        [XmlElement("InternalEndpoint")]
        public string? InternalEndpoint { get; set; }

        /// <summary>
        /// The acceleration endpoint of the region. The value is always oss-accelerate.aliyuncs.com.
        /// </summary>
        [XmlElement("AccelerateEndpoint")]
        public string? AccelerateEndpoint { get; set; }

        /// <summary>
        /// The region ID.
        /// </summary>
        [XmlElement("Region")]
        public string? Region { get; set; }

        /// <summary>
        /// The public endpoint of the region.
        /// </summary>
        [XmlElement("InternetEndpoint")]
        public string? InternetEndpoint { get; set; }
    }

    /// <summary>
    /// The information about the regions.
    /// </summary>
    [XmlRoot("RegionInfoList")]
    public sealed class RegionInfoList {
        /// <summary>
        /// The information about the regions.
        /// </summary>
        [XmlElement("RegionInfo")]
        public List<RegionInfo>? RegionInfos { get; set; }
    }

    /// <summary>
    /// The request for the DescribeRegions operation.
    /// </summary>
    public sealed class DescribeRegionsRequest : RequestModel {
        /// <summary>
        /// The region ID of the request.
        /// </summary>
        public string? Regions {
            get => Parameters.TryGetValue("regions", out var value) ? value : null;
            set {
                if (value != null) Parameters["regions"] = value;
            }
        }
    }

    /// <summary>
    /// The result for the DescribeRegions operation.
    /// </summary>
    public sealed class DescribeRegionsResult : ResultModel {
        /// <summary>
        /// The information about the regions.
        /// </summary>
        public RegionInfoList? RegionInfoList => InnerBody as RegionInfoList;

        public DescribeRegionsResult() {
            BodyFormat = "xml";
            BodyType   = typeof(RegionInfoList);
        }
    }
}