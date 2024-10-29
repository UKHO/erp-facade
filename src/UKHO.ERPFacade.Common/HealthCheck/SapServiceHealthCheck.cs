using System.Xml;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UKHO.ERPFacade.Common.Configuration;
using UKHO.ERPFacade.Common.Constants;
using UKHO.ERPFacade.Common.HttpClients;
using UKHO.ERPFacade.Common.IO;
using UKHO.ERPFacade.Common.Logging;
using UKHO.ERPFacade.Common.Operations.IO;

namespace UKHO.ERPFacade.Common.HealthCheck
{
    public class SapServiceHealthCheck : IHealthCheck
    {
        private readonly ISapClient _sapClient;
        private readonly IOptions<SapConfiguration> _sapConfig;
        private readonly IXmlOperations _xmlHelper;
        private readonly IFileOperations _fileSystemHelper;
        private readonly ILogger<SapServiceHealthCheck> _logger;

        public SapServiceHealthCheck(ISapClient sapClient,
                                             IOptions<SapConfiguration> sapConfig,
                                             IXmlOperations xmlHelper,
                                             IFileOperations fileSystemHelper,
                                             ILogger<SapServiceHealthCheck> logger)
        {
            _sapClient = sapClient;
            _sapConfig = sapConfig;
            _xmlHelper = xmlHelper;
            _fileSystemHelper = fileSystemHelper;
            _logger = logger;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var healthCheckData = new Dictionary<string, object>();
            const string description = "Reports unhealthy if SAP endpoint is not reachable, or does not return 200 status";

            try
            {
                string sapXmlTemplatePath = Path.Combine(Environment.CurrentDirectory, XmlTemplateInfo.SapHealthCheckXmlPath);

                healthCheckData.Add("SAP Template Path", sapXmlTemplatePath);

                //Check whether template file exists or not
                if (!_fileSystemHelper.IsFileExists(sapXmlTemplatePath))
                {
                    _logger.LogWarning(EventIds.SapHealthCheckXmlTemplateNotFound.ToEventId(), "The SAP Health Check xml template does not exist.");
                    return HealthCheckResult.Unhealthy(data: healthCheckData, description: description);
                }

                XmlDocument sapPayload = _xmlHelper.CreateXmlDocument(sapXmlTemplatePath);

                healthCheckData.Add("SAP SOAP endpoint", new Uri(_sapClient.Uri, _sapConfig.Value.SapEndpointForEncEvent));
                healthCheckData.Add("SAP SOAP operation(ENC)", _sapConfig.Value.SapServiceOperationForEncEvent);

                HttpResponseMessage response = await _sapClient.PostEventData(sapPayload, _sapConfig.Value.SapEndpointForEncEvent, _sapConfig.Value.SapServiceOperationForEncEvent, _sapConfig.Value.SapUsernameForEncEvent, _sapConfig.Value.SapPasswordForEncEvent);

                _logger.LogInformation(EventIds.SapHealthCheckRequestSentToSap.ToEventId(), "SAP Health Check request has been sent to SAP successfully. | {StatusCode}", response.StatusCode);

                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

                healthCheckData.Add("http-response-code", response.StatusCode);
                healthCheckData.Add("http-response", responseContent);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError(EventIds.SAPIsUnhealthy.ToEventId(), "SAP is Unhealthy");
                    return HealthCheckResult.Unhealthy(data: healthCheckData, description: description);
                }
                _logger.LogDebug(EventIds.SAPIsHealthy.ToEventId(), "SAP is Healthy");
                return HealthCheckResult.Healthy(data: healthCheckData, description: description);
            }
            catch (Exception ex)
            {
                _logger.LogError(EventIds.S57RequestToSapFailedException.ToEventId(), "An error occurred while processing your request in SAP. | {Message}", ex.Message);
                return HealthCheckResult.Unhealthy(exception: ex, data: healthCheckData, description: description);
            }
        }
    }
}
