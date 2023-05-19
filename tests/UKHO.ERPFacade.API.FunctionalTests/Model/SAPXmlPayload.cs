/*using System;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace UKHO.ERPFacade.API.FunctionalTests.Model
{
    [XmlRoot(ElementName = "Z_ADDS_MAT_INFO")]
    public class SAPXmlPayload
    {
        [XmlElement(ElementName = "IM_MATINFO")]
        public IM_MATINFO IM_MATINFO { get; set; }
    }
    [XmlRoot(ElementName = "item")]
    public class Item
    {
        [XmlElement(ElementName = "ACTIONNUMBER")]
        public string ACTIONNUMBER { get; set; }
        [XmlElement(ElementName = "ACTION")]
        public string ACTION { get; set; }
        [XmlElement(ElementName = "PRODUCT")]
        public string PRODUCT { get; set; }
        [XmlElement(ElementName = "PRODTYPE")]
        public string PRODTYPE { get; set; }
        [XmlElement(ElementName = "CHILDCELL")]
        public string CHILDCELL { get; set; }
        [XmlElement(ElementName = "PRODUCTNAME")]
        public string PRODUCTNAME { get; set; }
        [XmlElement(ElementName = "CANCELLED")]
        public string CANCELLED { get; set; }
        [XmlElement(ElementName = "REPLACEDBY")]
        public string REPLACEDBY { get; set; }
        [XmlElement(ElementName = "AGENCY")]
        public string AGENCY { get; set; }
        [XmlElement(ElementName = "PROVIDER")]
        public string PROVIDER { get; set; }
        [XmlElement(ElementName = "ENCSIZE")]
        public string ENCSIZE { get; set; }
        [XmlElement(ElementName = "TITLE")]
        public string TITLE { get; set; }
        [XmlElement(ElementName = "EDITIONNO")]
        public string EDITIONNO { get; set; }
        [XmlElement(ElementName = "UPDATENO")]
        public string UPDATENO { get; set; }
        [XmlElement(ElementName = "UNITTYPE")]
        public string UNITTYPE { get; set; }
    }

    [XmlRoot(ElementName = "ACTIONITEMS")]
    public class ACTIONITEMS
    {
        [XmlElement(ElementName = "item")]
        public List<Item> Item { get; set; }
    }

    [XmlRoot(ElementName = "IM_MATINFO")]
    public class IM_MATINFO
    {
        [XmlElement(ElementName = "CORRID")]
        public string CORRID { get; set; }
        [XmlElement(ElementName = "NOOFACTIONS")]
        public string NOOFACTIONS { get; set; }
        [XmlElement(ElementName = "RECDATE")]
        public string RECDATE { get; set; }
        [XmlElement(ElementName = "RECTIME")]
        public string RECTIME { get; set; }
        [XmlElement(ElementName = "ORG")]
        public string ORG { get; set; }
        [XmlElement(ElementName = "ACTIONITEMS")]
        public ACTIONITEMS ACTIONITEMS { get; set; }
    }




}
*/