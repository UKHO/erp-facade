using System.Diagnostics.CodeAnalysis;
using Azure.Core;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;
using UKHO.ERPFacade.Common.Infrastructure.Config;


namespace UKHO.ERPFacade.Common.Infrastructure.Authentication
{
    [ExcludeFromCodeCoverage]
    public class InteractiveTokenProvider : ITokenProvider
    {
        private const string RedirectUri = "http://localhost";
        private readonly EnterpriseEventServiceConfiguration _eesOptions;
        private readonly InteractiveLoginConfiguration _loginOptions;
    
        public InteractiveTokenProvider(IOptions<EnterpriseEventServiceConfiguration> eesOptions, IOptions<InteractiveLoginConfiguration> loginOptions)
        {           
            _loginOptions = loginOptions.Value;
            _eesOptions = eesOptions.Value;
        }


        public async Task<AccessToken> GetTokenAsync(string scope)
        {        
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
