namespace UKHO.ERPFacade.API.FunctionalTests.Configuration
{
    public class ErpFacadeConfiguration
    {
        public string BaseUrl { get; set; }
        public string WebhookEndpointUrl { get; set; }
        public string LicenceUpdatedRequestEndPoint { get; set; }
        public string RoSWebhookRequestEndPoint { get; set; }
        public string SapCallbackRequestEndPoint { get; set; }
        public PermitWithSameKey PermitWithSameKey { get; set; }
        public PermitWithDifferentKey PermitWithDifferentKey { get; set; }
        public string[] RosLicenceUpdateXmlList { get; set; }
        public string[] RoSLicenceUpdatedProdXmlList { get; set; }
    }
}
