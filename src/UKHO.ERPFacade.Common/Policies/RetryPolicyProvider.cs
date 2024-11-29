using Microsoft.Extensions.Logging;
using Polly.Extensions.Http;
using Polly;
using UKHO.ERPFacade.Common.Logging;

namespace UKHO.ERPFacade.Common.Policies
{
    public class RetryPolicyProvider
    {
        private readonly ILogger<RetryPolicyProvider> _logger;

        public RetryPolicyProvider(ILogger<RetryPolicyProvider> logger)
        {
            _logger = logger;
        }

        public IAsyncPolicy<HttpResponseMessage> GetRetryPolicy(string service, EventIds eventId, int retryCount, double sleepDuration)
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(retryCount, retryAttempt => TimeSpan.FromSeconds(sleepDuration),
                onRetry: (response, timespan, retryAttempt, context) =>
                {
                    _logger.LogInformation(eventId.ToEventId(), "Failed to connect {service} | StatusCode: {statusCode}. Retry attempted: {retryAttempt}.", service, response.Result.StatusCode.ToString(), retryAttempt);
                });
        }
    }
}
