
using Microsoft.Identity.Client;
using Newtonsoft.Json.Linq;
using UKHO.ERPFacade.API.FunctionalTests.FunctionalTests;

namespace UKHO.ERPFacade.API.FunctionalTests.Helpers
{
    public class ADAuthTokenProvider
    {
        static string AzureADToken = null;
        public static Config _config;

        public async Task<string> GetAzureADToken()
        {
            _config = new();

            AzureADToken = await GenerateAzureADToken(_config.testConfig.AzureadConfiguration.ClientId, _config.testConfig.AzureadConfiguration.ClientSecret, AzureADToken);
            return AzureADToken;
        }

        public async Task<string> GetAzureADToken(Boolean noRole)
        {
            _config = new();
            if (noRole)
            {
                AzureADToken = null;
                AzureADToken = await GenerateAzureADToken(_config.testConfig.AzureadConfiguration.AutoTestClientIdNoRole, _config.testConfig.AzureadConfiguration.ClientSecretNoRole, AzureADToken);
            }
            else
            {
                AzureADToken = await GenerateAzureADToken(_config.testConfig.AzureadConfiguration.AutoTestClientId, _config.testConfig.AzureadConfiguration.ClientSecret, AzureADToken);
            }
            return AzureADToken;
        }

        private static async Task<string> GenerateAzureADToken(string ClientId, string ClientSecret, string Token)
        {
            try
            {
                string[] scopes = new string[] { $"{_config.testConfig.AzureadConfiguration.ClientId}/.default" };
                if (Token == null)
                {
                    if (_config.testConfig.AzureadConfiguration.IsRunningOnLocalMachine)
                    {
                        IPublicClientApplication debugApp = PublicClientApplicationBuilder.Create(_config.testConfig.AzureadConfiguration.ClientId).
                                                            WithRedirectUri("http://localhost").Build();

                        //Acquiring token through user interaction
                        AuthenticationResult tokenTask = await debugApp.AcquireTokenInteractive(scopes)
                                                                .WithAuthority($"{_config.testConfig.AzureadConfiguration.MicrosoftOnlineLoginUrl}{_config.testConfig.AzureadConfiguration.TenantId}", true)
                                                                .ExecuteAsync();
                        Token = tokenTask.AccessToken;
                    }
                    else
                    {

                        IConfidentialClientApplication app = ConfidentialClientApplicationBuilder.Create(ClientId)
                                                        .WithClientSecret(ClientSecret)
                                                        .WithAuthority(new Uri($"{_config.testConfig.AzureadConfiguration.MicrosoftOnlineLoginUrl}{_config.testConfig.AzureadConfiguration.TenantId}"))
                                                        .Build();

                        AuthenticationResult tokenTask = await app.AcquireTokenForClient(scopes).ExecuteAsync();
                        Token = tokenTask.AccessToken;



                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return Token;
        }
    }
}