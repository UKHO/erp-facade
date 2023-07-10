using System.Diagnostics.CodeAnalysis;
using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.Logging;

namespace UKHO.ERPFacade.Common.Infrastructure.Authentication
{
    [ExcludeFromCodeCoverage]
    public class ManagedIdentityTokenProvider : ITokenProvider
    {
        private readonly ILogger<ManagedIdentityTokenProvider> _logger;

        public ManagedIdentityTokenProvider(ILogger<ManagedIdentityTokenProvider> logger)
        {
            _logger = logger;
        }

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
