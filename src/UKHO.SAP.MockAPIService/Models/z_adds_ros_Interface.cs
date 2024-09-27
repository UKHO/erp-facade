using System.Diagnostics.CodeAnalysis;
using UKHO.SAP.MockAPIService.EntityPropertyConverter;

namespace UKHO.SAP.MockAPIService.Models
{
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "urn:sap-com:document:sap:rfc:functions")]
    public partial class Z_ADDS_ROS
    {
        private IM_ORDER iM_ORDERField;

        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Order = 0)]
        public IM_ORDER IM_ORDER
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

    [ExcludeFromCodeCoverage]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Tools.ServiceModel.Svcutil", "2.1.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "")]
    public partial class IM_ORDER
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

        private string uSERSField;

        private string eNDUSERIDField;

        private string eCDISMANUFField;

        private string lTYPEField;

        private string lICDURField;

        private string poField;

        private string aDSORDNOField;

        private ZSALES_ITEMS[] pRODField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Order = 0)]
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
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Order = 1)]
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
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Order = 2)]
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
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Order = 3)]
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
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Order = 4)]
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
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Order = 5)]
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
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Order = 6)]
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
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Order = 7)]
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
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Order = 8)]
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
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Order = 9)]
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
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Order = 10)]
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
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Order = 11)]
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
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Order = 12)]
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
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Order = 13)]
        public string USERS
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
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Order = 14)]
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
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Order = 15)]
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
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Order = 16)]
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
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Order = 17)]
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
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Order = 18)]
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
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Order = 19)]
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
        [System.Xml.Serialization.XmlArrayAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Order = 20)]
        [System.Xml.Serialization.XmlArrayItemAttribute("item", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, IsNullable = false)]
        [EntityPropertyConverter(typeof(ZSALES_ITEMS[]))]
        public ZSALES_ITEMS[] PROD
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

    [ExcludeFromCodeCoverage]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Tools.ServiceModel.Svcutil", "2.1.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class ZSALES_ITEMS
    {
        private string idField;

        private string eNDDAField;

        private string dURATIONField;

        private string rENEWField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("ID", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Order = 0)]
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
        [System.Xml.Serialization.XmlElementAttribute("ENDDA", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Order = 1)]
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
        [System.Xml.Serialization.XmlElementAttribute("DURATION", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Order = 2)]
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
        [System.Xml.Serialization.XmlElementAttribute("RENEW", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Order = 3)]
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
    }

    [ExcludeFromCodeCoverage]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Tools.ServiceModel.Svcutil", "2.1.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "urn:sap-com:document:sap:rfc:functions")]
    public partial class Z_ADDS_ROSResponse
    {
        private string eX_MESSAGEField;

        private string eX_STATUSField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Order = 0)]
        public string EX_MESSAGE
        {
            get
            {
                return this.eX_MESSAGEField;
            }
            set
            {
                this.eX_MESSAGEField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Order = 1)]
        public string EX_STATUS
        {
            get
            {
                return this.eX_STATUSField;
            }
            set
            {
                this.eX_STATUSField = value;
            }
        }
    }
}
