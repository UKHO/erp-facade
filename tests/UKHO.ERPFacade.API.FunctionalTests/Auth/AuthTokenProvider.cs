using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using UKHO.ERPFacade.API.FunctionalTests.Configuration;

namespace UKHO.ERPFacade.API.FunctionalTests.Auth
{
    public class AuthTokenProvider : TestFixtureBase
    {
        private readonly AzureADConfiguration _azureADConfiguration;

        public AuthTokenProvider()
        {
            var serviceProvider = GetServiceProvider();
            _azureADConfiguration = serviceProvider!.GetRequiredService<IOptions<AzureADConfiguration>>().Value;
        }

        public async Task<string> GetAzureADTokenAsync(bool noRole)
        {
            string azureADToken;

            if (noRole)
            {
                azureADToken = await GenerateAzureADTokenAsync(_azureADConfiguration.AutoTestClientIdNoRole, _azureADConfiguration.ClientSecretNoRole);
            }
            else
            {
                azureADToken = await GenerateAzureADTokenAsync(_azureADConfiguration.AutoTestClientId, _azureADConfiguration.ClientSecret);
            }
            return azureADToken;
        }

        private async Task<string> GenerateAzureADTokenAsync(string clientId, string clientSecret)
        {
            try
            {
                string[] scopes = { $"{_azureADConfiguration.ClientId}/.default" };

                IPublicClientApplication publicClientApp;

                if (_azureADConfiguration.IsRunningOnLocalMachine)
                {
                    publicClientApp = PublicClientApplicationBuilder.Create(_azureADConfiguration.ClientId)
                                                       .WithRedirectUri("http://localhost")
                                                       .WithTenantId(_azureADConfiguration.TenantId).Build();

                    // Acquiring token through user interaction
                    AuthenticationResult tokenTask = await publicClientApp.AcquireTokenInteractive(scopes)
                                                    .ExecuteAsync();

                    return tokenTask.AccessToken;
                }
                else
                {
                    IConfidentialClientApplication confidentialClientApp = ConfidentialClientApplicationBuilder.Create(clientId)
                                                        .WithClientSecret(clientSecret)
                                                        .WithAuthority(new Uri($"{_azureADConfiguration.MicrosoftOnlineLoginUrl}{_azureADConfiguration.TenantId}"))
                                                        .Build();

                    AuthenticationResult tokenTask = await confidentialClientApp.AcquireTokenForClient(scopes)
                                                    .ExecuteAsync();
                    return tokenTask.AccessToken;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return null;
            }
        }
    }
}
