using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using Azure;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Extensions.Http;

namespace UKHO.ERPFacade.Common.Policies
{
    [ExcludeFromCodeCoverage]
    public static class RetryPolicyProvider
    {
        public static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy(ILogger logger, int retryCount, double sleepDuration)
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(retryCount, retryAttempt => TimeSpan.FromSeconds(sleepDuration),
                onRetry: (response, timespan, retryAttempt, context) =>
                {
                    if (response.Exception != null)
                    {
                        logger.LogError(response.Exception, $"Retry {retryAttempt} after {timespan.TotalSeconds} seconds due to exception.");
                    }
                    else if (response.Result != null)
                    {
                        logger.LogWarning($"Retry {retryAttempt} after {timespan.TotalSeconds} seconds due to response: {response.Result.ReasonPhrase}");
                    }
                });
        }
    }
}
