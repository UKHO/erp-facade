using Microsoft.Identity.Client;

namespace UKHO.ERPFacade.API.FunctionalTests.Helpers
{
    public class ADAuthTokenProvider
    {
        static string AzureADToken = null;
        private static Config _config;

        public async Task<string> GetAzureADToken()
        {
            _config = new();

            AzureADToken = await GenerateAzureADToken(_config.TestConfig.AzureADConfiguration.ClientId, _config.TestConfig.AzureADConfiguration.ClientSecret, AzureADToken);
            return AzureADToken;
        }

        public async Task<string> GetAzureADToken(bool noRole)
        {
            _config = new();
            if (noRole)
            {
                AzureADToken = null;
                AzureADToken = await GenerateAzureADToken(_config.TestConfig.AzureADConfiguration.AutoTestClientIdNoRole, _config.TestConfig.AzureADConfiguration.ClientSecretNoRole, AzureADToken);
            }
            else
            {
                AzureADToken = await GenerateAzureADToken(_config.TestConfig.AzureADConfiguration.AutoTestClientId, _config.TestConfig.AzureADConfiguration.ClientSecret, AzureADToken);
            }
            return AzureADToken;
        }
        public async Task<string> GetAzureADToken(bool noRole,string endPointName)
        {
            _config = new();
            if (noRole)
            {
                AzureADToken = null;
                AzureADToken = await GenerateAzureADToken(_config.TestConfig.AzureADConfiguration.AutoTestClientIdNoRole, _config.TestConfig.AzureADConfiguration.ClientSecretNoRole, AzureADToken);
            }
            else if (endPointName == "UnitOfSale"|| endPointName == "BulkPriceUpdate") 
            {
                
                AzureADToken = await GenerateAzureADToken(_config.TestConfig.AzureADConfiguration.AutoTestClientIdPricingInformationCaller, _config.TestConfig.AzureADConfiguration.ClientSecretPricingInformationCaller, AzureADToken);
            }

            else
            {
                AzureADToken = await GenerateAzureADToken(_config.TestConfig.AzureADConfiguration.AutoTestClientId, _config.TestConfig.AzureADConfiguration.ClientSecret, AzureADToken);
            }
            return AzureADToken;
        }

        private static async Task<string> GenerateAzureADToken(string clientId, string clientSecret, string token)
        {
            try
            {
                string[] scopes = new string[] { $"{_config.TestConfig.AzureADConfiguration.ClientId}/.default" };
                if (token == null)
                {
                    if (_config.TestConfig.AzureADConfiguration.IsRunningOnLocalMachine)
                    {
                        IPublicClientApplication debugApp = PublicClientApplicationBuilder.Create(_config.TestConfig.AzureADConfiguration.ClientId).
                                                            WithRedirectUri("http://localhost").Build();

                        //Acquiring token through user interaction
                        AuthenticationResult tokenTask = await debugApp.AcquireTokenInteractive(scopes)
                                                                .WithAuthority($"{_config.TestConfig.AzureADConfiguration.MicrosoftOnlineLoginUrl}{_config.TestConfig.AzureADConfiguration.TenantId}", true)
                                                                .ExecuteAsync();
                        token = tokenTask.AccessToken;
                    }
                    else
                    {

                        IConfidentialClientApplication app = ConfidentialClientApplicationBuilder.Create(clientId)
                                                        .WithClientSecret(clientSecret)
                                                        .WithAuthority(new Uri($"{_config.TestConfig.AzureADConfiguration.MicrosoftOnlineLoginUrl}{_config.TestConfig.AzureADConfiguration.TenantId}"))
                                                        .Build();

                        AuthenticationResult tokenTask = await app.AcquireTokenForClient(scopes).ExecuteAsync();
                        token = tokenTask.AccessToken;



                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return token;
        }
    }
}