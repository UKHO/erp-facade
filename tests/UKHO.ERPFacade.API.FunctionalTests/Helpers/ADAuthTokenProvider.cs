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

            AzureADToken = await GenerateAzureADToken(_config.TestConfig.AzureadConfiguration.ClientId, _config.TestConfig.AzureadConfiguration.ClientSecret, AzureADToken);
            return AzureADToken;
        }

        public async Task<string> GetAzureADToken(bool noRole)
        {
            _config = new();
            if (noRole)
            {
                AzureADToken = null;
                AzureADToken = await GenerateAzureADToken(_config.TestConfig.AzureadConfiguration.AutoTestClientIdNoRole, _config.TestConfig.AzureadConfiguration.ClientSecretNoRole, AzureADToken);
            }
            else
            {
                AzureADToken = await GenerateAzureADToken(_config.TestConfig.AzureadConfiguration.AutoTestClientId, _config.TestConfig.AzureadConfiguration.ClientSecret, AzureADToken);
            }
            return AzureADToken;
        }
        public async Task<string> GetAzureADToken(bool noRole,string endPointName)
        {
            _config = new();
            if (noRole)
            {
                AzureADToken = null;
                AzureADToken = await GenerateAzureADToken(_config.TestConfig.AzureadConfiguration.AutoTestClientIdNoRole, _config.TestConfig.AzureadConfiguration.ClientSecretNoRole, AzureADToken);
            }
            else if (endPointName == "UnitOfSale") 
            {
                
                AzureADToken = await GenerateAzureADToken(_config.TestConfig.AzureadConfiguration.AutoTestClientIdPricingInformationCaller, _config.TestConfig.AzureadConfiguration.ClientSecretPricingInformationCaller, AzureADToken);
            }
            else
            {
                AzureADToken = await GenerateAzureADToken(_config.TestConfig.AzureadConfiguration.AutoTestClientId, _config.TestConfig.AzureadConfiguration.ClientSecret, AzureADToken);
            }
            return AzureADToken;
        }

        private static async Task<string> GenerateAzureADToken(string clientId, string clientSecret, string token)
        {
            try
            {
                string[] scopes = new string[] { $"{_config.TestConfig.AzureadConfiguration.ClientId}/.default" };
                if (token == null)
                {
                    if (_config.TestConfig.AzureadConfiguration.IsRunningOnLocalMachine)
                    {
                        IPublicClientApplication debugApp = PublicClientApplicationBuilder.Create(_config.TestConfig.AzureadConfiguration.ClientId).
                                                            WithRedirectUri("http://localhost").Build();

                        //Acquiring token through user interaction
                        AuthenticationResult tokenTask = await debugApp.AcquireTokenInteractive(scopes)
                                                                .WithAuthority($"{_config.TestConfig.AzureadConfiguration.MicrosoftOnlineLoginUrl}{_config.TestConfig.AzureadConfiguration.TenantId}", true)
                                                                .ExecuteAsync();
                        token = tokenTask.AccessToken;
                    }
                    else
                    {

                        IConfidentialClientApplication app = ConfidentialClientApplicationBuilder.Create(clientId)
                                                        .WithClientSecret(clientSecret)
                                                        .WithAuthority(new Uri($"{_config.TestConfig.AzureadConfiguration.MicrosoftOnlineLoginUrl}{_config.TestConfig.AzureadConfiguration.TenantId}"))
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