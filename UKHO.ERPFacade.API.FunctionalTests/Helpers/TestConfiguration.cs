using Microsoft.Extensions.Configuration;

namespace UKHO.ERPFacade.API.FunctionalTests.Helpers
{
    public class TestConfiguration
    {
        protected IConfigurationRoot ConfigurationRoot;
        public string PayloadFolder { get; set; }
        public string PayloadParentFolder { get; set; }
        public string WebhookPayloadFileName { get; set; }
        
        public ErpFacadeConfiguration erpfacadeConfig = new();
      

        public class ErpFacadeConfiguration
        {
            public string BaseUrl { get; set; }
        }

        public TestConfiguration()
        {
            ConfigurationRoot = new ConfigurationBuilder()
                               .AddJsonFile("appsettings.json", false)
                               .Build();

            PayloadFolder = ConfigurationRoot.GetSection("PayloadFolder").Value;
            PayloadParentFolder = ConfigurationRoot.GetSection("PayloadParentFolder").Value;
            WebhookPayloadFileName = ConfigurationRoot.GetSection("WebhookPayloadFileName").Value;
            ConfigurationRoot.Bind("ErpFacadeConfiguration", erpfacadeConfig);
         
           
        }
    }
}
