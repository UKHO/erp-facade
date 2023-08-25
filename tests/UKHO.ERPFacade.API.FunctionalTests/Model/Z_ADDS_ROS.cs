using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UKHO.ERPFacade.API.FunctionalTests.Model
{

    // NOTE: Generated code may require at least .NET Framework 4.5 or .NET Core/Standard 2.0.
    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://schemas.xmlsoap.org/soap/envelope/", IsNullable = false)]
    public partial class Envelope
    {

        private EnvelopeBody bodyField;

        /// <remarks/>
        public EnvelopeBody Body
        {
            get
            {
                return this.bodyField;
            }
            set
            {
                this.bodyField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
    public partial class EnvelopeBody
    {

        private Z_ADDS_ROS z_ADDS_ROSField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Namespace = "")]
        public Z_ADDS_ROS Z_ADDS_ROS
        {
            get
            {
                return this.z_ADDS_ROSField;
            }
            set
            {
                this.z_ADDS_ROSField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class Z_ADDS_ROS
    {

        private Z_ADDS_ROSIM_ORDER iM_ORDERField;

        /// <remarks/>
        public Z_ADDS_ROSIM_ORDER IM_ORDER
        {
            get
            {
                return this.iM_ORDERField;
            }
            set
            {
                this.iM_ORDERField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class Z_ADDS_ROSIM_ORDER
    {

        private string gUIDField;

        private string sERVICETYPEField;

        private string lICTRANSACTIONField;

        private string sOLDTOACCField;

        private string lICENSEEACCField;

        private string sTARTDATEField;

        private string eNDDATEField;

        private string lICNOField;

        private string vNAMEField;

        private string iMOField;

        private string cALLSIGNField;

        private string sHOREBASEDField;

        private string fLEETField;

        private int uSERSField;

        private string eNDUSERIDField;

        private string eCDISMANUFField;

        private string lTYPEField;

        private string lICDURField;

        private string poField;

        private string aDSORDNOField;

        private Z_ADDS_ROSIM_ORDERItem[] pRODField;

        /// <remarks/>
        public string GUID
        {
            get
            {
                return this.gUIDField;
            }
            set
            {
                this.gUIDField = value;
            }
        }

        /// <remarks/>
        public string SERVICETYPE
        {
            get
            {
                return this.sERVICETYPEField;
            }
            set
            {
                this.sERVICETYPEField = value;
            }
        }

        /// <remarks/>
        public string LICTRANSACTION
        {
            get
            {
                return this.lICTRANSACTIONField;
            }
            set
            {
                this.lICTRANSACTIONField = value;
            }
        }

        /// <remarks/>
        public string SOLDTOACC
        {
            get
            {
                return this.sOLDTOACCField;
            }
            set
            {
                this.sOLDTOACCField = value;
            }
        }

        /// <remarks/>
        public string LICENSEEACC
        {
            get
            {
                return this.lICENSEEACCField;
            }
            set
            {
                this.lICENSEEACCField = value;
            }
        }

        /// <remarks/>
        public string STARTDATE
        {
            get
            {
                return this.sTARTDATEField;
            }
            set
            {
                this.sTARTDATEField = value;
            }
        }

        /// <remarks/>
        public string ENDDATE
        {
            get
            {
                return this.eNDDATEField;
            }
            set
            {
                this.eNDDATEField = value;
            }
        }

        /// <remarks/>
        public string LICNO
        {
            get
            {
                return this.lICNOField;
            }
            set
            {
                this.lICNOField = value;
            }
        }

        /// <remarks/>
        public string VNAME
        {
            get
            {
                return this.vNAMEField;
            }
            set
            {
                this.vNAMEField = value;
            }
        }

        /// <remarks/>
        public string IMO
        {
            get
            {
                return this.iMOField;
            }
            set
            {
                this.iMOField = value;
            }
        }

        /// <remarks/>
        public string CALLSIGN
        {
            get
            {
                return this.cALLSIGNField;
            }
            set
            {
                this.cALLSIGNField = value;
            }
        }

        /// <remarks/>
        public string SHOREBASED
        {
            get
            {
                return this.sHOREBASEDField;
            }
            set
            {
                this.sHOREBASEDField = value;
            }
        }

        /// <remarks/>
        public string FLEET
        {
            get
            {
                return this.fLEETField;
            }
            set
            {
                this.fLEETField = value;
            }
        }

        /// <remarks/>
        public int USERS
        {
            get
            {
                return this.uSERSField;
            }
            set
            {
                this.uSERSField = value;
            }
        }

        /// <remarks/>
        public string ENDUSERID
        {
            get
            {
                return this.eNDUSERIDField;
            }
            set
            {
                this.eNDUSERIDField = value;
            }
        }

        /// <remarks/>
        public string ECDISMANUF
        {
            get
            {
                return this.eCDISMANUFField;
            }
            set
            {
                this.eCDISMANUFField = value;
            }
        }

        /// <remarks/>
        public string LTYPE
        {
            get
            {
                return this.lTYPEField;
            }
            set
            {
                this.lTYPEField = value;
            }
        }

        /// <remarks/>
        public string LICDUR
        {
            get
            {
                return this.lICDURField;
            }
            set
            {
                this.lICDURField = value;
            }
        }

        /// <remarks/>
        public string PO
        {
            get
            {
                return this.poField;
            }
            set
            {
                this.poField = value;
            }
        }

        /// <remarks/>
        public string ADSORDNO
        {
            get
            {
                return this.aDSORDNOField;
            }
            set
            {
                this.aDSORDNOField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("item", IsNullable = false)]
        public Z_ADDS_ROSIM_ORDERItem[] PROD
        {
            get
            {
                return this.pRODField;
            }
            set
            {
                this.pRODField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class Z_ADDS_ROSIM_ORDERItem
    {

        private string idField;

        private string eNDDAField;

        private string dURATIONField;

        private string rENEWField;

        private string rEPEATField;

        /// <remarks/>
        public string ID
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }

        /// <remarks/>
        public string ENDDA
        {
            get
            {
                return this.eNDDAField;
            }
            set
            {
                this.eNDDAField = value;
            }
        }

        /// <remarks/>
        public string DURATION
        {
            get
            {
                return this.dURATIONField;
            }
            set
            {
                this.dURATIONField = value;
            }
        }

        /// <remarks/>
        public string RENEW
        {
            get
            {
                return this.rENEWField;
            }
            set
            {
                this.rENEWField = value;
            }
        }

        /// <remarks/>
        public string REPEAT
        {
            get
            {
                return this.rEPEATField;
            }
            set
            {
                this.rEPEATField = value;
            }
        }
    }


}
