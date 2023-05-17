using Azure.Core;
using Azure.Identity;

namespace UKHO.ERPFacade.Common.Infrastructure.Authentication
{
    public interface ITokenProvider
    {
        public Task<AccessToken> GetTokenAsync(string scope);
    }

    public class ManagedIdentityTokenProvider : ITokenProvider
    {
        public async Task<AccessToken> GetTokenAsync(string scope)
        {
            DefaultAzureCredential credential = new DefaultAzureCredential();
            var token = await credential.GetTokenAsync(new TokenRequestContext(new[]
            {
                scope
            }));

            return token;
        }
    }
}