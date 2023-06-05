namespace UKHO.ERPFacade.API.FunctionalTests.Model
{

    // NOTE: Generated code may require at least .NET Framework 4.5 or .NET Core/StandardUoS 2.0.
    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class Z_ADDS_MAT_INFO
    {

        private Z_ADDS_MAT_INFOIM_MATINFO iM_MATINFOField;

        /// <remarks/>
        public Z_ADDS_MAT_INFOIM_MATINFO IM_MATINFO
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

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class Z_ADDS_MAT_INFOIM_MATINFO
    {

        private string cORRIDField;

        private byte nOOFACTIONSField;

        private uint rECDATEField;

        private uint rECTIMEField;

        private string oRGField;

        private Item[] aCTIONITEMSField;

        /// <remarks/>
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
        public byte NOOFACTIONS
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
        public uint RECDATE
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
        public uint RECTIME
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
        [System.Xml.Serialization.XmlArrayAttribute(Namespace = "")]
        [System.Xml.Serialization.XmlArrayItemAttribute("item", Namespace = "", IsNullable = false)]
        public Item[] ACTIONITEMS
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

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    //[System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class Item
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

        /// <remarks/>
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
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "urn:sap-com:document:sap:rfc:functions")]
    //[System.Xml.Serialization.XmlRootAttribute(Namespace = "urn:sap-com:document:sap:rfc:functions", IsNullable = false)]
    public partial class ACTIONITEMS
    {

        private Item[] itemField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("item", Namespace = "")]
        public Item[] item
        {
            get
            {
                return this.itemField;
            }
            set
            {
                this.itemField = value;
            }
        }
    }


}
