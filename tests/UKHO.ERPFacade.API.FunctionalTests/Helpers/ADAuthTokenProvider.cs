using Microsoft.Identity.Client;
using UKHO.ERPFacade.API.FunctionalTests.Configuration;

namespace UKHO.ERPFacade.API.FunctionalTests.Helpers
{
    public class ADAuthTokenProvider
    {
        static string AzureADToken;
        public ADAuthTokenProvider()
        {
            AzureADToken = null;
        }
        public async Task<string> GetAzureADToken()
        {
            AzureADToken = await GenerateAzureADToken(Config.TestConfig.AzureADConfiguration.ClientId, Config.TestConfig.AzureADConfiguration.ClientSecret, AzureADToken);
            await Console.Out.WriteLineAsync("value1:"+ Config.TestConfig.AzureADConfiguration.ClientId);
            await Console.Out.WriteLineAsync("value1:" + Config.TestConfig.AzureADConfiguration.ClientSecret);
            return AzureADToken;
        }

        public async Task<string> GetAzureADToken(bool noRole)
        {
            if (noRole)
            {
                AzureADToken = null;
                AzureADToken = await GenerateAzureADToken(Config.TestConfig.AzureADConfiguration.AutoTestClientIdNoRole, Config.TestConfig.AzureADConfiguration.ClientSecretNoRole, AzureADToken);
            }
            else
            {
                AzureADToken = null;
                AzureADToken = await GenerateAzureADToken(Config.TestConfig.AzureADConfiguration.AutoTestClientId, Config.TestConfig.AzureADConfiguration.ClientSecret, AzureADToken);
            }
            return AzureADToken;
        }
        public async Task<string> GetAzureADToken(bool noRole,string endPointName)
        {
            if (noRole)
            {
                AzureADToken = null;
                AzureADToken = await GenerateAzureADToken(Config.TestConfig.AzureADConfiguration.AutoTestClientIdNoRole, Config.TestConfig.AzureADConfiguration.ClientSecretNoRole, AzureADToken);
            }
            else if (endPointName == "UnitOfSale"|| endPointName == "BulkPriceUpdate") 
            {
                AzureADToken = null;
                AzureADToken = await GenerateAzureADToken(Config.TestConfig.AzureADConfiguration.AutoTestClientIdPricingInformationCaller, Config.TestConfig.AzureADConfiguration.ClientSecretPricingInformationCaller, AzureADToken);
            }

            else
            {
                AzureADToken = await GenerateAzureADToken(Config.TestConfig.AzureADConfiguration.AutoTestClientId, Config.TestConfig.AzureADConfiguration.ClientSecret, AzureADToken);
            }
            return AzureADToken;
        }

        private static async Task<string> GenerateAzureADToken(string clientId, string clientSecret, string token)
        {
            try
            {
                string[] scopes = new string[] { $"{Config.TestConfig.AzureADConfiguration.ClientId}/.default" };
                if (token == null)
                {
                    if (Config.TestConfig.AzureADConfiguration.IsRunningOnLocalMachine)
                    {
                        IPublicClientApplication debugApp = PublicClientApplicationBuilder.Create(Config.TestConfig.AzureADConfiguration.ClientId).
                                                            WithRedirectUri("http://localhost").Build();

                        //Acquiring token through user interaction
                        AuthenticationResult tokenTask = await debugApp.AcquireTokenInteractive(scopes)
                                                                .WithAuthority($"{Config.TestConfig.AzureADConfiguration.MicrosoftOnlineLoginUrl}{Config.TestConfig.AzureADConfiguration.TenantId}", true)
                                                                .ExecuteAsync();
                        token = tokenTask.AccessToken;
                    }
                    else
                    {

                        IConfidentialClientApplication app = ConfidentialClientApplicationBuilder.Create(clientId)
                                                        .WithClientSecret(clientSecret)
                                                        .WithAuthority(new Uri($"{Config.TestConfig.AzureADConfiguration.MicrosoftOnlineLoginUrl}{Config.TestConfig.AzureADConfiguration.TenantId}"))
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
