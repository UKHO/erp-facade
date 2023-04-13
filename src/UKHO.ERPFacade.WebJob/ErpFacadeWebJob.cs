using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics.CodeAnalysis;
using UKHO.ERPFacade.Common.Configuration;
using UKHO.ERPFacade.Common.IO;
using UKHO.ERPFacade.Common.Logging;

namespace UKHO.ERPFacade.WebJob
{
    [ExcludeFromCodeCoverage]
    public class ErpFacadeWebJob
    {
        private readonly ILogger<ErpFacadeWebJob> _logger;
        private readonly IOptions<AzureStorageConfiguration> _azureStorageConfig;
        private readonly IOptions<ErpFacadeWebJobConfiguration> _erpFacadeWebJobConfig;
        private readonly IAzureTableReaderWriter _azureTableReaderWriter;


        public ErpFacadeWebJob(ILogger<ErpFacadeWebJob> logger,
                               IOptions<AzureStorageConfiguration> azureStorageConfig,
                               IOptions<ErpFacadeWebJobConfiguration> erpFacadeWebJobConfig,
                               IAzureTableReaderWriter azureTableReaderWriter)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _azureStorageConfig = azureStorageConfig ?? throw new ArgumentNullException(nameof(azureStorageConfig));
            _erpFacadeWebJobConfig = erpFacadeWebJobConfig ?? throw new ArgumentNullException(nameof(erpFacadeWebJobConfig));
            _azureTableReaderWriter = azureTableReaderWriter ?? throw new ArgumentNullException(nameof(azureTableReaderWriter));
        }

        public void Start()
        {
            try
            {
                //Code to monitor the table records.
                _logger.LogInformation(EventIds.WebjobProcessEventStarted.ToEventId(), "Webjob started for the processing the incomplete transactions.");
                _azureTableReaderWriter.ValidateEntity();
                _logger.LogInformation(EventIds.WebjobProcessEventStarted.ToEventId(), "Webjob completed for processing the incomplete transactions.");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
