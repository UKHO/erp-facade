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
        private readonly IOptions<CleanupWebJobConfiguration> _cleanuoWebjobConfig;
        private readonly IAzureTableHelper _azureTableHelper;
        private readonly IAzureBlobHelper _azureBlobHelper;

        public CleanUpService(ILogger<CleanUpService> logger,
                               IOptions<CleanupWebJobConfiguration> cleanupWebjobConfig,
                               IAzureTableHelper azureTableHelper,
                               IAzureBlobHelper azureBlobHelper)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cleanuoWebjobConfig = cleanupWebjobConfig ?? throw new ArgumentNullException(nameof(cleanupWebjobConfig));
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

                if (timediff.Days > int.Parse(_cleanuoWebjobConfig.Value.CleanUpDurationInDays))
                {
                    Task.FromResult(_azureTableHelper.DeleteEntity(correlationId));

                    _azureBlobHelper.DeleteContainer(correlationId);
                }
            }
        }
    }
}
