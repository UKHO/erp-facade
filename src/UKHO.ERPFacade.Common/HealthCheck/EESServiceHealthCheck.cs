using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Linq;
using System.Xml;
using UKHO.ERPFacade.Common.Configuration;
using UKHO.ERPFacade.Common.HttpClients;
using UKHO.ERPFacade.Common.IO;
using UKHO.ERPFacade.Common.Logging;


namespace UKHO.ERPFacade.Common.HealthCheck
{
    public class EESServiceHealthCheck : IHealthCheck
    {
        private readonly ILogger<SapServiceHealthCheck> _logger;
        private readonly IEESClient _eesClient;
        private readonly IOptions<HealthCheckEnvironmentConfiguration> _environmentConfiguration;

        public EESServiceHealthCheck( ILogger<SapServiceHealthCheck> logger,
                                         IEESClient eesClient,
                                         IOptions<HealthCheckEnvironmentConfiguration> environmentConfiguration)
        {
            _logger = logger;
            _eesClient = eesClient;
            _environmentConfiguration= environmentConfiguration;
        }
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {

            try
            {
                //Environment
                var environment = _environmentConfiguration.Value.Environment;
                var excludeEnvironments = (_environmentConfiguration.Value.ExcludeEnvironment).Split(',').ToArray();

                if(excludeEnvironments.Contains(environment))
                {
                    return HealthCheckResult.Healthy("EES is Healthy !!!");
                }

                HttpResponseMessage response = await _eesClient.EESHealthCheck();

                _logger.LogInformation(EventIds.EESHealthCheckRequestSentToEES.ToEventId(), "EES Health Check request has been sent to EES successfully. | {StatusCode}", response.StatusCode);

                if (!response.IsSuccessStatusCode)
                {
                    return HealthCheckResult.Unhealthy("EES is Unhealthy");
                }

            }
            catch (Exception ex)
            {
                _logger.LogInformation(EventIds.ErrorOccurredInEES.ToEventId(), "An error occurred while processing your request in EES", ex.Message);
                return HealthCheckResult.Unhealthy("EES is Unhealthy" + ex.Message);
            }

            return HealthCheckResult.Healthy("EES is Healthy !!!");
        }
    }
}
