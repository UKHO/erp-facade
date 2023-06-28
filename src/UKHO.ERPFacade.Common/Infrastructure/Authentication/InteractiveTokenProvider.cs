using System.Diagnostics.CodeAnalysis;
using Azure.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using UKHO.ERPFacade.Common.Infrastructure.Config;
using UKHO.ERPFacade.Common.Logging;

namespace UKHO.ERPFacade.Common.Infrastructure.Authentication
{
    [ExcludeFromCodeCoverage]
    public class InteractiveTokenProvider : ITokenProvider
    {
        private const string RedirectUri = "http://localhost";
        private readonly EnterpriseEventServiceConfiguration _eesOptions;
        private readonly InteractiveLoginConfiguration _loginOptions;
        private readonly ILogger<InteractiveTokenProvider> _logger;

        public InteractiveTokenProvider(IOptions<EnterpriseEventServiceConfiguration> eesOptions, IOptions<InteractiveLoginConfiguration> loginOptions, ILogger<InteractiveTokenProvider> logger)
        {
            _logger = logger;
            _loginOptions = loginOptions.Value;
            _eesOptions = eesOptions.Value;
        }


        public async Task<AccessToken> GetTokenAsync(string scope)
        {
            _logger.LogInformation(EventIds.SapUnitsOfSalePriceInformationPayloadReceived.ToEventId(), "Call gettokenasync. Scope:" + scope);
            IPublicClientApplication debugApp = PublicClientApplicationBuilder.Create(_eesOptions.ClientId).WithRedirectUri(RedirectUri).Build();
            AuthenticationResult tokenTask = await debugApp.AcquireTokenInteractive(new[]
                                                           {
                                                           scope
                                                       })
                                                           .WithAuthority($"{_loginOptions.MicrosoftOnlineLoginUrl}{_loginOptions.TenantId}", true)
                                                           .ExecuteAsync();

            return new AccessToken(tokenTask.AccessToken, tokenTask.ExpiresOn);
        }
    }
}
