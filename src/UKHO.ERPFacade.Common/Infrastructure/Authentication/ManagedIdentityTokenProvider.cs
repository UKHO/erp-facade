using System.Diagnostics.CodeAnalysis;
using Azure.Core;
using Azure.Identity;

namespace UKHO.ERPFacade.Common.Infrastructure.Authentication
{
    [ExcludeFromCodeCoverage]
    public class ManagedIdentityTokenProvider : ITokenProvider
    {
        public async Task<AccessToken> GetTokenAsync(string scope)
        {
            DefaultAzureCredential credential = new();
            var token = await credential.GetTokenAsync(new TokenRequestContext(new[]
            {
                scope
            }));

            return token;
        }
    }
}
