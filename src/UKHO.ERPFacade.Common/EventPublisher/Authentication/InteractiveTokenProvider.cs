using System.Diagnostics.CodeAnalysis;
using Azure.Core;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using UKHO.ERPFacade.Common.Configuration;

namespace UKHO.ERPFacade.Common.EventPublisher.Authentication
{
    [ExcludeFromCodeCoverage]
    public class InteractiveTokenProvider : ITokenProvider
    {
        private readonly EESConfiguration _eesOptions;
        private readonly InteractiveLoginConfiguration _loginOptions;

        public InteractiveTokenProvider(IOptions<EESConfiguration> eesOptions, IOptions<InteractiveLoginConfiguration> loginOptions)
        {
            _loginOptions = loginOptions.Value;
            _eesOptions = eesOptions.Value;
        }

        public async Task<AccessToken> GetTokenAsync(string scope)
        {
            IConfidentialClientApplication app = ConfidentialClientApplicationBuilder.Create(_eesOptions.ClientId)
                                               .WithClientSecret(_eesOptions.ClientSecret)
                                               .WithAuthority(new Uri($"{_loginOptions.MicrosoftOnlineLoginUrl}{_loginOptions.TenantId}")).Build();

            AuthenticationResult tokenTask = await app.AcquireTokenForClient(new[]
            {
                scope
            }).ExecuteAsync();

            return new AccessToken(tokenTask.AccessToken, tokenTask.ExpiresOn);
        }
    }
}
