using System.Diagnostics.CodeAnalysis;

namespace UKHO.ERPFacade.Common.Constants
{
    [ExcludeFromCodeCoverage]
    public class XmlTemplateInfo
    {
        public const string S57SapXmlTemplatePath = "SapXmlTemplates\\SAPS57Request.xml";
        public const string RecordOfSaleSapXmlTemplatePath = "SapXmlTemplates\\SAPRoSRequest.xml";
        public const string SapHealthCheckXmlPath = "SapXmlTemplates\\SAPHealthCheckRequest.xml";
        public const string S100SapXmlTemplatePath = "SapXmlTemplates\\SAPS100Request.xml";

        public const string SoapEnvelope = "soap:Envelope";
        public const string SoapBody = "soap:Body";
        public const string XpathZAddsRos = $"//*[local-name()='Z_ADDS_ROS']";

        public const string XpathImMatInfo = $"//*[local-name()='IM_MATINFO']";
        public const string XpathActionItems = $"//*[local-name()='ACTIONITEMS']";
        public const string XpathActionNumber = $"//*[local-name()='ACTIONNUMBER']";
        public const string XpathAction = $"//*[local-name()='ACTION']";
        public const string XpathNoOfActions = $"//*[local-name()='NOOFACTIONS']";
        public const string XpathCorrId = $"//*[local-name()='CORRID']";
        public const string XpathRecDate = $"//*[local-name()='RECDATE']";
        public const string XpathRecTime = $"//*[local-name()='RECTIME']";
        public const string Item = "item";

        public const string S100XpathZShopMatInfo = $"//*[local-name()='ZSHOPMAT_INFO']";
    }
}
