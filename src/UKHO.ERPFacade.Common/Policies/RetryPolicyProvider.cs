using System.Diagnostics.CodeAnalysis;
using Polly;
using Polly.Extensions.Http;

namespace UKHO.ERPFacade.Common.Policies
{
    [ExcludeFromCodeCoverage]
    public static class RetryPolicyProvider
    {
        public static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy(int retryCount, double sleepDuration)
        {
            return HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(retryCount, retryAttempt => TimeSpan.FromSeconds(sleepDuration),
            onRetry: (response, timespan, retryAttempt, context) =>
            {
            });
        }
    }
}
