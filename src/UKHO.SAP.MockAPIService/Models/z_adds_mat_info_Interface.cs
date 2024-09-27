using System.Diagnostics.CodeAnalysis;
using UKHO.SAP.MockAPIService.EntityPropertyConverter;

namespace UKHO.SAP.MockAPIService.Models
{
    [ExcludeFromCodeCoverage]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Tools.ServiceModel.Svcutil", "2.1.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "urn:sap-com:document:sap:rfc:functions")]
    public partial class Z_ADDS_MAT_INFO
    {

        private IM_MATINFO iM_MATINFOField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Order = 0)]
        public IM_MATINFO IM_MATINFO
        {
            get
            {
                return this.iM_MATINFOField;
            }
            set
            {
                this.iM_MATINFOField = value;
            }
        }
    }

    [ExcludeFromCodeCoverage]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Tools.ServiceModel.Svcutil", "2.1.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "")]
    public partial class IM_MATINFO
    {

        private string cORRIDField;

        private string nOOFACTIONSField;

        private string rECDATEField;

        private string rECTIMEField;

        private string oRGField;

        private ZMAT_ACTIONITEMS[] aCTIONITEMSField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, IsNullable = false)]
        public string CORRID
        {
            get
            {
                return this.cORRIDField;
            }
            set
            {
                this.cORRIDField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, IsNullable = false)]
        public string NOOFACTIONS
        {
            get
            {
                return this.nOOFACTIONSField;
            }
            set
            {
                this.nOOFACTIONSField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string RECDATE
        {
            get
            {
                return this.rECDATEField;
            }
            set
            {
                this.rECDATEField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string RECTIME
        {
            get
            {
                return this.rECTIMEField;
            }
            set
            {
                this.rECTIMEField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string ORG
        {
            get
            {
                return this.oRGField;
            }
            set
            {
                this.oRGField = value;
            }
        }

        /// <remarks/>
        //[System.Xml.Serialization.XmlArrayAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Order = 5)]
        [System.Xml.Serialization.XmlArrayItemAttribute("item", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, IsNullable = false)]
        [EntityPropertyConverter(typeof(ZMAT_ACTIONITEMS[]))]
        public ZMAT_ACTIONITEMS[] ACTIONITEMS
        {
            get
            {
                return this.aCTIONITEMSField;
            }
            set
            {
                this.aCTIONITEMSField = value;
            }
        }
    }

    [ExcludeFromCodeCoverage]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Tools.ServiceModel.Svcutil", "2.1.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class ZMAT_ACTIONITEMS
    {

        private string aCTIONNUMBERField;

        private string aCTIONField;

        private string pRODUCTField;

        private string pRODTYPEField;

        private string cHILDCELLField;

        private string pRODUCTNAMEField;

        private string cANCELLEDField;

        private string rEPLACEDBYField;

        private string aGENCYField;

        private string pROVIDERField;

        private string eNCSIZEField;

        private string tITLEField;

        private string eDITIONNOField;

        private string uPDATENOField;

        private string uNITTYPEField;

        private string wEEKNOField;

        private string vALIDFROMField;

        private string cORRECTIONField;

        private string aCTIVEKEYField;

        private string nEXTKEYField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("ACTIONNUMBER", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string ACTIONNUMBER
        {
            get
            {
                return this.aCTIONNUMBERField;
            }
            set
            {
                this.aCTIONNUMBERField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("ACTION", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string ACTION
        {
            get
            {
                return this.aCTIONField;
            }
            set
            {
                this.aCTIONField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("PRODUCT", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string PRODUCT
        {
            get
            {
                return this.pRODUCTField;
            }
            set
            {
                this.pRODUCTField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("PRODTYPE", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string PRODTYPE
        {
            get
            {
                return this.pRODTYPEField;
            }
            set
            {
                this.pRODTYPEField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("CHILDCELL", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string CHILDCELL
        {
            get
            {
                return this.cHILDCELLField;
            }
            set
            {
                this.cHILDCELLField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("PRODUCTNAME", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string PRODUCTNAME
        {
            get
            {
                return this.pRODUCTNAMEField;
            }
            set
            {
                this.pRODUCTNAMEField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("CANCELLED", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string CANCELLED
        {
            get
            {
                return this.cANCELLEDField;
            }
            set
            {
                this.cANCELLEDField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("REPLACEDBY", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string REPLACEDBY
        {
            get
            {
                return this.rEPLACEDBYField;
            }
            set
            {
                this.rEPLACEDBYField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("AGENCY", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string AGENCY
        {
            get
            {
                return this.aGENCYField;
            }
            set
            {
                this.aGENCYField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("PROVIDER", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string PROVIDER
        {
            get
            {
                return this.pROVIDERField;
            }
            set
            {
                this.pROVIDERField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("ENCSIZE", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string ENCSIZE
        {
            get
            {
                return this.eNCSIZEField;
            }
            set
            {
                this.eNCSIZEField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("TITLE", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string TITLE
        {
            get
            {
                return this.tITLEField;
            }
            set
            {
                this.tITLEField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("EDITIONNO", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string EDITIONNO
        {
            get
            {
                return this.eDITIONNOField;
            }
            set
            {
                this.eDITIONNOField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("UPDATENO", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string UPDATENO
        {
            get
            {
                return this.uPDATENOField;
            }
            set
            {
                this.uPDATENOField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("UNITTYPE", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string UNITTYPE
        {
            get
            {
                return this.uNITTYPEField;
            }
            set
            {
                this.uNITTYPEField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("WEEKNO", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string WEEKNO
        {
            get
            {
                return this.wEEKNOField;
            }
            set
            {
                this.wEEKNOField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("VALIDFROM", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string VALIDFROM
        {
            get
            {
                return this.vALIDFROMField;
            }
            set
            {
                this.vALIDFROMField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("CORRECTION", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string CORRECTION
        {
            get
            {
                return this.cORRECTIONField;
            }
            set
            {
                this.cORRECTIONField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("ACTIVEKEY", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string ACTIVEKEY
        {
            get
            {
                return this.aCTIVEKEYField;
            }
            set
            {
                this.aCTIVEKEYField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElement("NEXTKEY", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string NEXTKEY
        {
            get
            {
                return this.nEXTKEYField;
            }
            set
            {
                this.nEXTKEYField = value;
            }
        }
    }

    [ExcludeFromCodeCoverage]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Tools.ServiceModel.Svcutil", "2.1.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "urn:sap-com:document:sap:rfc:functions")]
    public partial class Z_ADDS_MAT_INFOResponse
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
