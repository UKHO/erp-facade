using System.Diagnostics.CodeAnalysis;
using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.Options;
using UKHO.ERPFacade.Common.Configuration;

namespace UKHO.ERPFacade.Common.Authentication
{
    [ExcludeFromCodeCoverage]
    public class ManagedIdentityTokenProvider : ITokenProvider
    {
        private readonly IOptions<AzureADConfiguration> _azureADConfiguration;

        public ManagedIdentityTokenProvider(IOptions<AzureADConfiguration> azureADConfiguration)
        {
            _azureADConfiguration = azureADConfiguration;
        }

        public async Task<string> GetTokenAsync(string scope)
        {
            DefaultAzureCredential credential = new(new DefaultAzureCredentialOptions { ManagedIdentityClientId = _azureADConfiguration.Value.ClientId });
            var accessToken = await credential.GetTokenAsync(new TokenRequestContext([scope]));
            return accessToken.Token;
        }
    }
}
