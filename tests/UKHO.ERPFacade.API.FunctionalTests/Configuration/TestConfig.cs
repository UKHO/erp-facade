namespace UKHO.ERPFacade.API.FunctionalTests.Configuration
{
    public class TestConfig
    {
        public string PayloadFolder { get; set; }
        public string WebhookPayloadFileName { get; set; }
        public string GeneratedXMLFolder { get; set; }
        public ErpFacadeConfiguration ErpFacadeConfiguration { get; set; }
        public AzureADconfiguration AzureADConfiguration { get; set; }
        public AzureStorageConfiguration AzureStorageConfiguration { get; set; }
        public string[] XMLActionList { get; set; }
        public string UoSPayloadFileName { get; set; }
        public string PriceChangePayloadFileName { get; set; }
        public string ERPFacadeGeneratedProductJSON { get; set; }
        public string GeneratedProductJsonFolder { get; set; }
        public string GeneratedJSONFolder { get; set; }
        public SharedKeyConfiguration SharedKeyConfiguration { get; set; }
        public string LicenceUpdate { get; set; }
        public string[] ROSLUXMLList { get; set; }
        

    }


    public class AzureADconfiguration
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

    public class ErpFacadeConfiguration
    {
        public string BaseUrl { get; set; }
    }

    public class AzureStorageConfiguration
    {
        public string ConnectionString { get; set; }
    }

    public class SharedKeyConfiguration
    {
        public string Key { get; set; }
    }

}
