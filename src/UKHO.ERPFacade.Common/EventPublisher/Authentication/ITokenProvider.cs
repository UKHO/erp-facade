using Azure.Core;

namespace UKHO.ERPFacade.Common.EventPublisher.Authentication
{
    public interface ITokenProvider
    {
        public Task<AccessToken> GetTokenAsync(string scope);
    }
}
