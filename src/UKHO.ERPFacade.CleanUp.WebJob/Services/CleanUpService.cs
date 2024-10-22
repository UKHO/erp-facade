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
        private readonly IAzureTableHelper _azureTableHelper;
        private readonly IAzureBlobHelper _azureBlobHelper;

        public CleanUpService(ILogger<CleanUpService> logger,
                               IOptions<ErpFacadeWebJobConfiguration> erpFacadeWebjobConfig,
                               IAzureTableHelper azureTableHelper,
                               IAzureBlobHelper azureBlobHelper)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _erpFacadeWebjobConfig = erpFacadeWebjobConfig ?? throw new ArgumentNullException(nameof(erpFacadeWebjobConfig));
            _azureTableHelper = azureTableHelper ?? throw new ArgumentNullException(nameof(azureTableHelper));
            _azureBlobHelper = azureBlobHelper ?? throw new ArgumentNullException(nameof(azureBlobHelper));
        }

        public void CleanUpAzureTableAndBlobs()
        {
            CleanUpEvents(Constants.S57EventTableName, Constants.S57EventContainerName);
        }

        private void CleanUpEvents(string tableName, string eventContainerName)
        {
            _logger.LogInformation(EventIds.FetchEESEntities.ToEventId(), "Fetching all records from azure table {TableName}", tableName);

            var entities = _azureTableHelper.GetAllEntities(tableName);

            foreach (var entity in entities)
            {
                if (entity["RequestDateTime"] == null)
                    continue;

                TimeSpan timediff = DateTime.Now - Convert.ToDateTime(entity["RequestDateTime"].ToString());

                if (timediff.Days > int.Parse(_erpFacadeWebjobConfig.Value.CleanUpDurationInDays))
                {
                    Task.FromResult(_azureTableHelper.DeleteEntity(entity["CorrelationId"].ToString(), tableName));

                    _azureBlobHelper.DeleteContainer(entity["CorrelationId"].ToString().ToLower());

                    _logger.LogInformation(EventIds.DeletedContainerSuccessful.ToEventId(), "Event data cleaned up for {CorrelationId} successfully.", entity["CorrelationId"].ToString());
                }
            }
        }
    }
}
