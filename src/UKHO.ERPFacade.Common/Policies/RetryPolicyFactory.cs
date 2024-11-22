using Microsoft.Extensions.Logging;
using Polly.Extensions.Http;
using Polly;
using UKHO.ERPFacade.Common.HttpClients;
using UKHO.ERPFacade.Common.Policies;
using UKHO.ERPFacade.Common.Logging;

public class RetryPolicyFactory
{
    private readonly ILogger<IEesClient> _logger;
    private readonly RetryPolicyConfiguration _retryPolicyConfiguration;

    public RetryPolicyFactory(ILogger<IEesClient> logger, RetryPolicyConfiguration retryPolicyConfiguration)
    {
        _logger = logger;
        _retryPolicyConfiguration = retryPolicyConfiguration;
    }

    public IAsyncPolicy<HttpResponseMessage> CreateRetryPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(_retryPolicyConfiguration.RetryCount, retryAttempt => TimeSpan.FromSeconds(_retryPolicyConfiguration.Duration),
            onRetry: (response, timespan, retryAttempt, context) =>
            {
                _logger.LogError("Failed to connect | StatusCode: {statusCode}. Retry attempted: {retryAttempt}.", response.Result.StatusCode.ToString(), retryAttempt);
            });
    }
}
