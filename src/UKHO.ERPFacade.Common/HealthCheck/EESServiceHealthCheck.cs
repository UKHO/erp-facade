using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UKHO.ERPFacade.Common.Configuration;
using UKHO.ERPFacade.Common.HttpClients;
using UKHO.ERPFacade.Common.Logging;


namespace UKHO.ERPFacade.Common.HealthCheck
{
    public class EESServiceHealthCheck : IHealthCheck
    {
        private readonly ILogger<SapServiceHealthCheck> _logger;
        private readonly IEESClient _eesClient;
        private readonly IOptions<EESHealthCheckEnvironmentConfiguration> _eesHealthCheckEnvironmentConfiguration;

        public EESServiceHealthCheck(ILogger<SapServiceHealthCheck> logger,
                                     IEESClient eesClient,
                                     IOptions<EESHealthCheckEnvironmentConfiguration> eesHealthCheckEnvironmentConfiguration)
        {
            _logger = logger;
            _eesClient = eesClient;
            _eesHealthCheckEnvironmentConfiguration = eesHealthCheckEnvironmentConfiguration;
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
                _logger.LogInformation(EventIds.ErrorOccurredInEES.ToEventId(), "An error occurred while processing your request in EES", ex.Message);
                return HealthCheckResult.Unhealthy("EES is Unhealthy" + ex.Message);
            }
        }
    }
}
