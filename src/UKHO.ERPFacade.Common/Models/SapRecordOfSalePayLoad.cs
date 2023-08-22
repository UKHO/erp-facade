using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;

namespace UKHO.ERPFacade.Common.Models
{
    [ExcludeFromCodeCoverage]
   [XmlRoot(ElementName = "IM_ORDER", Namespace = "RecordOfSale")]
    public class SapRecordOfSalePayLoad
    {
        [XmlElement(ElementName = "GUID")]
        public string CorrelationId { get; set; }

        [XmlElement(ElementName = "SERVICETYPE")]
        public string ServiceType { get; set; }

        [XmlElement(ElementName = "LICTRANSACTION")]
        public string LicTransaction { get; set; }

        [XmlElement(ElementName = "SOLDTOACC")]
        public string SoldToAcc { get; set; }

        [XmlElement(ElementName = "LICENSEEACC")]
        public string LicenseEacc { get; set; }

        [XmlElement(ElementName = "STARTDATE")]
        public string StartDate { get; set; }

        [XmlElement(ElementName = "ENDDATE")]
        public string EndDate { get; set; }

        [XmlElement(ElementName = "LICNO")]
        public string LicenceNumber { get; set; }

        [XmlElement(ElementName = "VNAME")]
        public string VesselName { get; set; }

        [XmlElement(ElementName = "IMO")]
        public string IMONumber { get; set; }

        [XmlElement(ElementName = "CALLSIGN")]
        public string CallSign { get; set; }

        [XmlElement(ElementName = "SHOREBASED")]
        public string ShoreBased { get; set; }

        [XmlElement(ElementName = "FLEET")]
        public string FleetName { get; set; }

        [XmlElement(ElementName = "USERS")]
        public int? Users { get; set; }

        [XmlElement(ElementName = "ENDUSERID")]
        public string EndUserId { get; set; }

        [XmlElement(ElementName = "ECDISMANUF")]
        public string ECDISMANUF { get; set; }

        [XmlElement(ElementName = "LTYPE")]
        public int? LicenceType { get; set; }

        [XmlElement(ElementName = "LICDUR")]
        public int LicenceDuration { get; set; }

        [XmlElement(ElementName = "PO")]
        public string PurachaseOrder { get; set; }

        [XmlElement(ElementName = "ADSORDNO")]
        public string OrderNumber { get; set; }

        [XmlElement(ElementName = "PROD")]
        public PROD PROD { get; set; }
    }

    [ExcludeFromCodeCoverage]
    [XmlRoot(ElementName = "PROD")]
    public class PROD
    {
        [XmlElement(ElementName = "item") ]
        public List<UnitOfSales> UnitOfSales { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class UnitOfSales
    {
        [XmlElement(ElementName = "ID")]
        public string Id { get; set; }

        [XmlElement(ElementName = "ENDDA")]
        public string EndDate { get; set; }

        [XmlElement(ElementName = "DURATION")]
        public int? Duration { get; set; }

        [XmlElement(ElementName = "RENEW")]
        public string ReNew { get; set; }

        [XmlElement(ElementName = "REPEAT")]
        public string Repeat { get; set; }
    }
}
