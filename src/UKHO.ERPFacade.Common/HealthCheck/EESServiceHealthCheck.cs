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
        private readonly ILogger<EESServiceHealthCheck> _logger;
        private readonly IEESClient _eesClient;
        private readonly IOptions<EESHealthCheckEnvironmentConfiguration> _eesHealthCheckEnvironmentConfiguration;

        public EESServiceHealthCheck(ILogger<EESServiceHealthCheck> logger, IEESClient eesClient, IOptions<EESHealthCheckEnvironmentConfiguration> eesHealthCheckEnvironmentConfiguration)
        {
            _logger = logger;
            _eesClient = eesClient;
            _eesHealthCheckEnvironmentConfiguration = eesHealthCheckEnvironmentConfiguration;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var healthCheckData = new Dictionary<string, object>();
            const string description = "Reports unhealthy if EES endpoint is not reachable, or does not return 200 status";

            try
            {
                healthCheckData.Add("EES health endpoint", _eesHealthCheckEnvironmentConfiguration.Value.EESHealthCheckUrl);

                HttpResponseMessage response = await _eesClient.Get(_eesHealthCheckEnvironmentConfiguration.Value.EESHealthCheckUrl);

                _logger.LogInformation(EventIds.EESHealthCheckRequestSentToEES.ToEventId(), "EES health check request has been sent to EES successfully. | {StatusCode}", response.StatusCode);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError(EventIds.EESIsUnhealthy.ToEventId(), "EES is Unhealthy");
                    return HealthCheckResult.Unhealthy(data: healthCheckData, description: description);
                }

                _logger.LogDebug(EventIds.EESIsHealthy.ToEventId(), "EES is Healthy");
                return HealthCheckResult.Healthy(data: healthCheckData, description: description);
            }
            catch (Exception ex)
            {
                _logger.LogInformation(EventIds.ErrorOccurredInEES.ToEventId(), "An error occurred while processing your request in EES. | {Message}", ex.Message);
                return HealthCheckResult.Unhealthy($"EES is Unhealthy { ex.Message}");
            }
        }
    }
}
