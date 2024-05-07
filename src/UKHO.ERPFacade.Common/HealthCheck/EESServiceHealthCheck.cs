using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using UKHO.ERPFacade.Common.HttpClients;
using UKHO.ERPFacade.Common.Logging;


namespace UKHO.ERPFacade.Common.HealthCheck
{
    public class EESServiceHealthCheck : IHealthCheck
    {
        private readonly ILogger<EESServiceHealthCheck> _logger;
        private readonly IEESClient _eesClient;

        public EESServiceHealthCheck(ILogger<EESServiceHealthCheck> logger,IEESClient eesClient)
        {
            _logger = logger;
            _eesClient = eesClient;
        }
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                HttpResponseMessage response = await _eesClient.EESHealthCheck();

                _logger.LogInformation(EventIds.EESHealthCheckRequestSentToEES.ToEventId(), "EES health check request has been sent to EES successfully. | {StatusCode}", response.StatusCode);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError(EventIds.EESIsUnhealthy.ToEventId(), "EES is Unhealthy !!!");
                    return HealthCheckResult.Unhealthy("EES is Unhealthy");
                }
                
                _logger.LogDebug(EventIds.EESIsHealthy.ToEventId(), "EES is Healthy");
                return HealthCheckResult.Healthy("EES is Healthy !!!");
            }
            catch (Exception ex)
            {
                _logger.LogInformation(EventIds.ErrorOccurredInEES.ToEventId(), "An error occurred while processing your request in EES. | {Message}", ex.Message);
                return HealthCheckResult.Unhealthy("EES is Unhealthy" + ex.Message);
            }
        }
    }
}
