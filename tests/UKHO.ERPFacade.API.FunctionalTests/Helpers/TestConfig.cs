
namespace UKHO.ERPFacade.API.FunctionalTests.Helpers
{
    public class TestConfig
    {
        public string PayloadFolder { get; set; }
        public string WebhookPayloadFileName { get; set; } 
        public string SapMockApiPayloadFileName { get; set; }
        public string ExpectedXMLFolder { get; set; }        
        public string WebhookInvalidPayloadFileName { get; set; }
        public string GeneratedXMLFolder { get; set; }
        public SapMockConfiguration SapMockConfiguration { get; set; }
        public Erpfacadeconfiguration ErpFacadeConfiguration { get; set; }
        public Azureadconfiguration AzureadConfiguration { get; set; }
        public AzureStorageConfiguration AzureStorageConfiguration { get; set; }
        public string[] XMLActionList { get; set; }
    }


    public class Azureadconfiguration
    {

        public string MicrosoftOnlineLoginUrl { get; set; }

        public string TenantId { get; set; }

        public string ClientId { get; set; }

        public string AutoTestClientId { get; set; }

        public string ClientSecret { get; set; }

        public bool IsRunningOnLocalMachine { get; set; }

        public string AutoTestClientIdNoRole { get; set; }

        public string ClientSecretNoRole { get; set; }

    }

    public class Erpfacadeconfiguration
    {
        public string BaseUrl { get; set; }

    }

    public class SapMockConfiguration
    {
        public string BaseUrl { get; set; }
        public string Username { get; set;}
        public string Password { get; set; }
    }

    public class AzureStorageConfiguration
    {
        public string ConnectionString { get; set; }
    }

}
