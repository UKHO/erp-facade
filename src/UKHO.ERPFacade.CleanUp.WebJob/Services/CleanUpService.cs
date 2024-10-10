using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UKHO.ERPFacade.Common.Configuration;
using UKHO.ERPFacade.Common.Constants;
using UKHO.ERPFacade.Common.IO.Azure;
using UKHO.ERPFacade.Common.Logging;

namespace UKHO.ERPFacade.CleanUp.WebJob.Services
{
    public class CleanUpService : ICleanUpService
    {
        private readonly ILogger<CleanUpService> _logger;
        private readonly IOptions<ErpFacadeWebJobConfiguration> _erpFacadeWebjobConfig;
        private readonly IAzureTableReaderWriter _azureTableReaderWriter;
        private readonly IAzureBlobEventWriter _azureBlobEventWriter;

        public CleanUpService(ILogger<CleanUpService> logger,
                               IOptions<ErpFacadeWebJobConfiguration> erpFacadeWebjobConfig,
                               IAzureTableReaderWriter azureTableReaderWriter,
                               IAzureBlobEventWriter azureBlobEventWriter)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _erpFacadeWebjobConfig = erpFacadeWebjobConfig ?? throw new ArgumentNullException(nameof(erpFacadeWebjobConfig));
            _azureTableReaderWriter = azureTableReaderWriter ?? throw new ArgumentNullException(nameof(azureTableReaderWriter));
            _azureBlobEventWriter = azureBlobEventWriter ?? throw new ArgumentNullException(nameof(azureBlobEventWriter));
        }

        public void CleanUpAzureTableAndBlobs()
        {
            CleanUpEvents(Constants.S57EventTableName, Constants.S57EventContainerName);
        }

        private void CleanUpEvents(string tableName, string eventContainerName)
        {
            _logger.LogInformation(EventIds.FetchEESEntities.ToEventId(), "Fetching all records from azure table {0}", tableName);

            var entities = _azureTableReaderWriter.GetAllEntities(tableName);

            foreach (var entity in entities)
            {
                if (entity["RequestDateTime"] == null)
                    continue;

                TimeSpan timediff = DateTime.Now - Convert.ToDateTime(entity["RequestDateTime"].ToString());

                if (timediff.Days > int.Parse(_erpFacadeWebjobConfig.Value.CleanUpDurationInDays))
                {
                    Task.FromResult(_azureTableReaderWriter.DeleteEntity(entity["CorrelationId"].ToString(), tableName));

                    _logger.LogInformation(EventIds.DeletedContainerSuccessful.ToEventId(), "Deleting directory {0} from {1} container", entity["CorrelationId"].ToString(), eventContainerName);

                    _azureBlobEventWriter.DeleteDirectory(eventContainerName, entity["CorrelationId"].ToString().ToLower());
                }
            }
        }
    }
}
