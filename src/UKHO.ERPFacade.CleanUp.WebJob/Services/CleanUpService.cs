using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UKHO.ERPFacade.Common.Configuration;
using UKHO.ERPFacade.Common.IO.Azure;

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
        private const string RequestFormat = "json";

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
            var masterEntities = _azureTableReaderWriter.GetMasterEntities(CompleteStatus);
            foreach (var masterEntity in masterEntities)
            {
                var blobMetadata = _azureBlobEventWriter.GetBlobMetadata(masterEntity.CorrId + '/' + masterEntity.CorrId + '.' + RequestFormat, PriceChangeContainerName);
                TimeSpan timediff = DateTime.Now - blobMetadata.CreatedOn.DateTime;
                if (timediff.TotalDays > int.Parse(_erpFacadeWebjobConfig.Value.CleanUpDurationInDays))
                {
                    _azureTableReaderWriter.DeleteUnitPriceChangeEntityForMasterCorrId(masterEntity.CorrId);
                    _azureTableReaderWriter.DeletePriceMasterEntity(masterEntity.CorrId);
                    foreach (var blob in _azureBlobEventWriter.GetBlobsInContainer(PriceChangeContainerName))
                    {
                        _azureBlobEventWriter.DeleteBlob(blob, PriceChangeContainerName);
                    }
                }
            }
        }

        private void CleanUpBlobsAndTablesRelatedToEESEvent()
        {
            var entities = _azureTableReaderWriter.GetAllEntityForEESTable();
            foreach (var entity in entities)
            {
                if (!entity.RequestDateTime.HasValue)
                    continue;
                TimeSpan timediff = DateTime.Now - entity.RequestDateTime.Value;
                if (timediff.TotalDays > int.Parse(_erpFacadeWebjobConfig.Value.CleanUpDurationInDays))
                {
                    Task.FromResult(_azureTableReaderWriter.DeleteEESEntity(entity.CorrelationId));
                    _azureBlobEventWriter.DeleteContainer(entity.CorrelationId);
                }
            }
        }
    }
}
