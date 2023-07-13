using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Xml;
using UKHO.ERPFacade.Common.Configuration;
using UKHO.ERPFacade.Common.HttpClients;
using UKHO.ERPFacade.Common.IO;
using UKHO.ERPFacade.Common.Logging;

namespace UKHO.ERPFacade.Common.HealthCheck
{
    public class SapServiceHealthCheck : IHealthCheck
    {
        private readonly ISapClient _sapClient;
        private readonly IOptions<SapConfiguration> _sapConfig;
        private readonly IXmlHelper _xmlHelper;
        private readonly IFileSystemHelper _fileSystemHelper;
        private readonly ILogger<SapServiceHealthCheck> _logger;

        private const string SapHealthCheckXmlPath = "SapXmlTemplates\\SAPHealthCheckRequest.xml";

        public SapServiceHealthCheck(ISapClient sapClient,
                                             IOptions<SapConfiguration> sapConfig,
                                             IXmlHelper xmlHelper,
                                             IFileSystemHelper fileSystemHelper,
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
            try
            {
                string sapXmlTemplatePath = Path.Combine(Environment.CurrentDirectory, SapHealthCheckXmlPath);

                //Check whether template file exists or not
                if (!_fileSystemHelper.IsFileExists(sapXmlTemplatePath))
                {
                    _logger.LogWarning(EventIds.SapHealthCheckXmlTemplateNotFound.ToEventId(), "The SAP Health Check xml template does not exist.");
                    throw new FileNotFoundException();
                }

                XmlDocument sapPayload = _xmlHelper.CreateXmlDocument(sapXmlTemplatePath);

                HttpResponseMessage response = await _sapClient.PostEventData(sapPayload, _sapConfig.Value.SapServiceOperation);

                _logger.LogInformation(EventIds.SapHealthCheckRequestSentToSap.ToEventId(), "SAP Health Check request has been sent to SAP successfully. | {StatusCode}", response.StatusCode);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError(EventIds.SAPIsUnhealthy.ToEventId(), "SAP is Unhealthy !!!");
                    return HealthCheckResult.Unhealthy("SAP is Unhealthy");
                }
                _logger.LogDebug(EventIds.SAPIsHealthy.ToEventId(), "SAP is Healthy");
                return HealthCheckResult.Healthy("SAP is Healthy !!!");
            }
            catch (Exception ex)
            {
                _logger.LogError(EventIds.ErrorOccuredInSap.ToEventId(), "An error occured while processing your request in SAP. | {Message}", ex.Message);
                return HealthCheckResult.Unhealthy("SAP is Unhealthy" + ex.Message);
            }
        }
    }
}
