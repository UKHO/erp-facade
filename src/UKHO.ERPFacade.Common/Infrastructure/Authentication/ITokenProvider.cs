using Azure.Core;

namespace UKHO.ERPFacade.Common.Infrastructure.Authentication
{
    public interface ITokenProvider
    {
        public Task<AccessToken> GetTokenAsync(string scope);
    }
}