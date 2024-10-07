using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UKHO.ERPFacade.Common.Configuration;
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
        private const string ErpFacadeTableName = "encevents";

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
            CleanUpBlobsAndTablesRelatedToEESEvent();
        }

        //Private methods       
        private void CleanUpBlobsAndTablesRelatedToEESEvent()
        {
            _logger.LogInformation(EventIds.FetchEESEntities.ToEventId(), "Fetching all EES entities from azure table");

            var entities = _azureTableReaderWriter.GetAllEntities(ErpFacadeTableName);

            foreach (var entity in entities)
            {
                if (entity["RequestDateTime"] == null)
                    continue;
                TimeSpan timediff = DateTime.Now - Convert.ToDateTime(entity["RequestDateTime"].ToString());
                if (timediff.Days > int.Parse(_erpFacadeWebjobConfig.Value.CleanUpDurationInDays))
                {
                    Task.FromResult(_azureTableReaderWriter.DeleteEntity(entity["CorrelationId"].ToString(), ErpFacadeTableName));
                    _logger.LogInformation(EventIds.DeletedContainerSuccessful.ToEventId(), "Deleting container : {0}", entity["CorrelationId"].ToString());
                    _azureBlobEventWriter.DeleteContainer(entity["CorrelationId"].ToString().ToLower());
                }
            }
        }
    }
}
