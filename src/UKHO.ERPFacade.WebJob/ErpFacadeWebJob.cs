using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics.CodeAnalysis;
using UKHO.ERPFacade.Common.Configuration;

namespace UKHO.ERPFacade.WebJob
{
    [ExcludeFromCodeCoverage]
    public class ErpFacadeWebJob
    {
        private readonly ILogger<ErpFacadeWebJob> _logger;
        private readonly IOptions<AzureStorageConfiguration> _azureStorageConfig;
        private readonly IOptions<ErpFacadeWebJobConfiguration> _erpFacadeWebJobConfig;


        public ErpFacadeWebJob(ILogger<ErpFacadeWebJob> logger,
                               IOptions<AzureStorageConfiguration> azureStorageConfig,
                               IOptions<ErpFacadeWebJobConfiguration> erpFacadeWebJobConfig)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _azureStorageConfig = azureStorageConfig ?? throw new ArgumentNullException(nameof(azureStorageConfig));
            _erpFacadeWebJobConfig = erpFacadeWebJobConfig ?? throw new ArgumentNullException(nameof(erpFacadeWebJobConfig));
        }

        public void Start()
        {
            try
            {
                //Code to monitor the table records.
                _logger.LogInformation("Add code to monitor the table records");
            }
            catch (Exception)
            {
            }
        }
    }
}
