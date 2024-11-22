using System.Diagnostics.CodeAnalysis;
using System.Net;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Extensions.Http;
using UKHO.ERPFacade.Common.Logging;

namespace UKHO.ERPFacade.Common.Policies
{
    [ExcludeFromCodeCoverage]
    public static class RetryPolicyProvider
    {
        public static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy(ILogger _logger, string service, int retryCount, double sleepDuration)
        {
            return Policy
            .HandleResult<HttpResponseMessage>(r => r.StatusCode == HttpStatusCode.ServiceUnavailable)
                .OrResult(r => r.StatusCode == HttpStatusCode.InternalServerError)
            .WaitAndRetryAsync(retryCount, retryAttempt => TimeSpan.FromSeconds(sleepDuration),
            onRetry: (response, timespan, retryAttempt, context) =>
            {
                _logger.LogInformation(EventIds.RetryAttemptForEnterpriseEventServiceEvent.ToEventId(), "Failed to connect {service} | StatusCode: {statusCode}. Retry attempted: {retryAttempt}.", service, response.Result.StatusCode.ToString(), retryAttempt);
            });
        }
    }
}
