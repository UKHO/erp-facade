namespace UKHO.ERPFacade.API.FunctionalTests.Configuration
{
    public class AzureADConfiguration
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
}
