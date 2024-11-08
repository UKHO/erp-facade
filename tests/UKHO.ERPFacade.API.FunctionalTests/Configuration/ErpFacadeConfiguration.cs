namespace UKHO.ERPFacade.API.FunctionalTests.Configuration
{
    public class ErpFacadeConfiguration
    {
        public string BaseUrl { get; set; }
        public string WebhookEndpointUrl { get; set; }
        public string LicenceUpdatedRequestEndPoint { get; set; }
        public string RoSWebhookRequestEndPoint { get; set; }
        public PermitWithSameKey PermitWithSameKey { get; set; }
        public PermitWithDifferentKey PermitWithDifferentKey { get; set; }
        public string[] RosLicenceUpdateXmlList { get; set; }
        public string[] RoSLicenceUpdatedProdXmlList { get; set; }
    }

    public class PermitWithSameKey
    {
        public string Permit { get; set; }
        public string ActiveKey { get; set; }
        public string NextKey { get; set; }
    }

    public class PermitWithDifferentKey
    {
        public string Permit { get; set; }
        public string ActiveKey { get; set; }
        public string NextKey { get; set; }
    }
}
