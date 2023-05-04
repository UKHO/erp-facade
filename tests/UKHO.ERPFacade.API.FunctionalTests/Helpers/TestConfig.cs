
namespace UKHO.ERPFacade.API.FunctionalTests.Helpers
{
    public class TestConfig
    {
        public string PayloadFolder { get; set; }
        public string WebhookPayloadFileName { get; set; }
        public string WebhookInvalidPayloadFileName { get; set; }
        public string SAPXmlPayloads { get; set; }
        public Erpfacadeconfiguration ErpFacadeConfiguration { get; set; }
        public Azureadconfiguration AzureadConfiguration { get; set; }
        public Azurestoragesonfiguration AzureStorageConfiguration { get; set; }
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
    public class Azurestoragesonfiguration
    {
        public string ConnectionString { get; set; }
        public string TableName { get; set; }
    }
}
