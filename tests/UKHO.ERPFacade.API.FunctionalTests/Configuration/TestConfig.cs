namespace UKHO.ERPFacade.API.FunctionalTests.Configuration
{
    public class TestConfig
    {
        public string PayloadFolder { get; set; }
        public string WebhookPayloadFileName { get; set; }
        public string GeneratedXmlFolder { get; set; }
        public ErpFacadeConfiguration ErpFacadeConfiguration { get; set; }
        public AzureADconfiguration AzureADConfiguration { get; set; }
        public AzureStorageConfiguration AzureStorageConfiguration { get; set; }
        public string LicenceUpdatedPayloadTestData { get; set; }
        public string[] RosLicenceUpdateXmlList { get; set; }
        public string[] RoSLicenceUpdatedProdXmlList { get; set; }
        public string WeekNoTag { get; set; }
        public string ValidFromTagThursday { get; set; }
        public string ValidFromTagFriday { get; set; }
        public PermitWithSameKey PermitWithSameKey { get; set; }
        public PermitWithDifferentKey PermitWithDifferentKey { get; set; }
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
