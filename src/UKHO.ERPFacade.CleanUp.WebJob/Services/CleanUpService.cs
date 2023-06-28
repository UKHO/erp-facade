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

        private const string CompleteStatus = "Complete";
        private const string PriceChangeContainerName = "pricechangeblobs";
        private const string PriceInformationFileName = "PriceInformation.json";

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
            CleanUpBlobsAndTablesRelatedToSlicing();
            CleanUpBlobsAndTablesRelatedToEESEvent();
        }

        //Private methods
        private void CleanUpBlobsAndTablesRelatedToSlicing()
        {
            _logger.LogInformation(EventIds.FetchMasterEntities.ToEventId(), "Fetching master entities from azure table");
            var masterEntities = _azureTableReaderWriter.GetMasterEntities(CompleteStatus);
            foreach (var masterEntity in masterEntities)
            {
                _logger.LogInformation(EventIds.FetchBlobCreateDate.ToEventId(), "Fetching create date of blob : {0}", masterEntity.CorrId);
                var blobCreateDate = _azureBlobEventWriter.GetBlobCreateDate(masterEntity.CorrId + '/' + PriceInformationFileName, PriceChangeContainerName);
                TimeSpan timediff = DateTime.Now - blobCreateDate;
                if (timediff.Days > int.Parse(_erpFacadeWebjobConfig.Value.CleanUpDurationInDays))
                {
                    _azureTableReaderWriter.DeleteUnitPriceChangeEntityForMasterCorrId(masterEntity.CorrId);
                    _azureTableReaderWriter.DeletePriceMasterEntity(masterEntity.CorrId);
                    _logger.LogInformation(EventIds.FetchBlobsFromContainer.ToEventId(), "Fetching all blobs present in container");
                    foreach (var blob in _azureBlobEventWriter.GetBlobsInContainer(PriceChangeContainerName, masterEntity.CorrId))
                    {
                        _azureBlobEventWriter.DeleteBlob(blob, PriceChangeContainerName);
                        _logger.LogInformation(EventIds.DeletedBlobSuccessful.ToEventId(), "Deleted blob : {0}  from container", blob);
                    }
                }
            }
        }

        private void CleanUpBlobsAndTablesRelatedToEESEvent()
        {
            _logger.LogInformation(EventIds.FetchEESEntities.ToEventId(), "Fetching all EES entities from azure table");
            var entities = _azureTableReaderWriter.GetAllEntityForEESTable();
            foreach (var entity in entities)
            {
                if (!entity.RequestDateTime.HasValue)
                    continue;
                TimeSpan timediff = DateTime.Now - entity.RequestDateTime.Value;
                if (timediff.Days > int.Parse(_erpFacadeWebjobConfig.Value.CleanUpDurationInDays) && entity.ResponseDateTime.HasValue)
                {
                    Task.FromResult(_azureTableReaderWriter.DeleteEESEntity(entity.CorrelationId));
                    _logger.LogInformation(EventIds.DeletedContainerSuccessful.ToEventId(), "Deleting container : {0}", entity.CorrelationId);
                    _azureBlobEventWriter.DeleteContainer(entity.CorrelationId);
                }
            }
        }
    }
}
