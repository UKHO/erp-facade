using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using UKHO.ERPFacade.Common.Configuration;
using UKHO.ERPFacade.Common.Exceptions;
using UKHO.ERPFacade.Common.Logging;

namespace UKHO.ERPFacade.API.Filters
{
    public class SharedApiKeyAuthFilter : IAuthorizationFilter
    {
        private readonly ILogger<SharedApiKeyAuthFilter> _logger;
        private readonly SharedApiKeyConfiguration _sharedApiKeyConfiguration;
        private readonly string _apiKey = "X-API-Key";

        public SharedApiKeyAuthFilter(ILogger<SharedApiKeyAuthFilter> logger, IOptions<SharedApiKeyConfiguration> sharedApiKeyConfiguration)
        {
            _logger = logger;
            _sharedApiKeyConfiguration = sharedApiKeyConfiguration.Value ?? throw new ArgumentNullException(nameof(sharedApiKeyConfiguration));

            if (string.IsNullOrEmpty(_sharedApiKeyConfiguration.SharedApiKey))
            {
                throw new ERPFacadeException(EventIds.SharedApiKeyConfigurationMissing.ToEventId(), "Shared API key configuration missing.");
            }
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            string sharedApiKey = context.HttpContext.Request.Headers[_apiKey];
            if (string.IsNullOrWhiteSpace(sharedApiKey))
            {
                _logger.LogWarning(EventIds.SharedApiKeyMissingInRequest.ToEventId(), "Shared key is missing in request");
                context.Result = new UnauthorizedObjectResult("Shared key is missing in request");
                return;
            }

            if (!_sharedApiKeyConfiguration.SharedApiKey.Equals(sharedApiKey))
            {
                _logger.LogWarning(EventIds.InvalidSharedApiKey.ToEventId(), "Invalid shared key");
                context.Result = new UnauthorizedObjectResult("Invalid shared key");
            }
        }
    }
}
