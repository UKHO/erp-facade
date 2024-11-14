using Azure.Core;
using LazyCache;

namespace UKHO.ERPFacade.Common.Infrastructure.Authentication
{
    public class AccessTokenCache : IAccessTokenCache
    {
        private readonly IAppCache _memoryCache;
        private readonly ITokenProvider _tokenProvider;

        public AccessTokenCache(IAppCache memoryCache, ITokenProvider tokenProvider)
        {
            _memoryCache = memoryCache;
            _tokenProvider = tokenProvider;
        }

        public Task<string> GetTokenAsync(string scope)
        {
            return _memoryCache.GetOrAddAsync(scope, async entry =>
            {
                AccessToken token = await _tokenProvider.GetTokenAsync(scope);
                entry.AbsoluteExpiration = token.ExpiresOn;
                return token.Token;
            });
        }
    }
}
