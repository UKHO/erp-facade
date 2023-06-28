using Azure.Core;
using LazyCache;
using Microsoft.Extensions.Logging;
using UKHO.ERPFacade.Common.Logging;

namespace UKHO.ERPFacade.Common.Infrastructure.Authentication
{
    public class AccessTokenCache : IAccessTokenCache
    {
        private readonly IAppCache _memoryCache;
        private readonly ITokenProvider _tokenProvider;
        private readonly ILogger<AccessTokenCache> _logger;

        public AccessTokenCache(IAppCache memoryCache, ITokenProvider tokenProvider, ILogger<AccessTokenCache> logger)
        {
            _logger = logger;
            _memoryCache = memoryCache;
            _tokenProvider = tokenProvider;
        }

        public Task<string> GetTokenAsync(string scope)
        {
            _logger.LogInformation(EventIds.SapUnitsOfSalePriceInformationPayloadReceived.ToEventId(), "Started Call gettokenasync. Scope:" + scope);
            return _memoryCache.GetOrAddAsync(scope, async entry =>
            {
                AccessToken token = await _tokenProvider.GetTokenAsync(scope);

                entry.AbsoluteExpiration = token.ExpiresOn;
                _logger.LogInformation(EventIds.SapUnitsOfSalePriceInformationPayloadReceived.ToEventId(), "Token generated:" + token.Token);
                return token.Token;
            });
        }
    }
}
