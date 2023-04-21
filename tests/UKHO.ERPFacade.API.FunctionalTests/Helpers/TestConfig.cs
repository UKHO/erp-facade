
namespace UKHO.ERPFacade.API.FunctionalTests.Helpers
{
        public class TestConfig
        {
            public string PayloadFolder { get; set; }
            public string WebhookPayloadFileName { get; set; }
            public Erpfacadeconfiguration ErpFacadeConfiguration { get; set; }
        }

        public class Erpfacadeconfiguration
        {
            public string BaseUrl { get; set; }
        }
}
