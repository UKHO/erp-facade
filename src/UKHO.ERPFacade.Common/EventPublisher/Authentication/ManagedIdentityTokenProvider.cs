using System.Diagnostics.CodeAnalysis;
using Azure.Core;
using Azure.Identity;

namespace UKHO.ERPFacade.Common.EventPublisher.Authentication
{
    [ExcludeFromCodeCoverage]
    public class ManagedIdentityTokenProvider : ITokenProvider
    {
        public async Task<AccessToken> GetTokenAsync(string scope)
        {
            DefaultAzureCredential credential = new();
            var token = await credential.GetTokenAsync(new TokenRequestContext(
            [
                scope
            ]));

            return token;
        }
    }
}
