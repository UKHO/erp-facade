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

        public void Clean()
        {
            CleanS57Data("S57");
        }

        private void CleanS57Data(string partitionKey)
        {
            var entities = _azureTableHelper.GetAllEntities(partitionKey);

            foreach (var entity in entities)
            {
                if (entity["RequestDateTime"] == null)
                    continue;

                var correlationId = entity.RowKey.ToString();

                TimeSpan timediff = DateTime.Now - Convert.ToDateTime(entity["RequestDateTime"].ToString());

                if (timediff.Days > int.Parse(_erpFacadeWebjobConfig.Value.CleanUpDurationInDays))
                {
                    Task.FromResult(_azureTableHelper.DeleteEntity(correlationId));

                    _azureBlobHelper.DeleteContainer(correlationId);
                }
            }
            _logger.LogInformation(EventIds.EventDataCleanupCompleted.ToEventId(), "S57 event data clean up completed successfully.");
        }
    }
}
