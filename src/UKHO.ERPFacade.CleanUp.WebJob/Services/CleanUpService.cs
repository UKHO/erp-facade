using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UKHO.ERPFacade.Common.Configuration;
using UKHO.ERPFacade.Common.Enums;
using UKHO.ERPFacade.Common.Logging;
using UKHO.ERPFacade.Common.Operations.IO.Azure;

namespace UKHO.ERPFacade.CleanUp.WebJob.Services
{
    public class CleanUpService : ICleanUpService
    {
        private readonly ILogger<CleanUpService> _logger;
        private readonly IOptions<CleanupWebJobConfiguration> _cleanupWebjobConfig;
        private readonly IAzureTableReaderWriter _azureTableReaderWriter;
        private readonly IAzureBlobReaderWriter _azureBlobReaderWriter;

        public CleanUpService(ILogger<CleanUpService> logger,
                              IOptions<CleanupWebJobConfiguration> cleanupWebjobConfig,
                              IAzureTableReaderWriter azureTableReaderWriter,
                              IAzureBlobReaderWriter azureBlobReaderWriter)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cleanupWebjobConfig = cleanupWebjobConfig ?? throw new ArgumentNullException(nameof(cleanupWebjobConfig));
            _azureTableReaderWriter = azureTableReaderWriter ?? throw new ArgumentNullException(nameof(azureTableReaderWriter));
            _azureBlobReaderWriter = azureBlobReaderWriter ?? throw new ArgumentNullException(nameof(azureBlobReaderWriter));
        }

        public async Task Clean()
        {
            try
            {
                var statusFilter = new Dictionary<string, string>
                {
                    {"Status", Status.Complete.ToString()}
                };
                var entities = await _azureTableReaderWriter.GetFilteredEntitiesAsync(statusFilter);

                foreach (var entity in entities)
                {
                    var correlationId = entity.RowKey.ToString();

                    TimeSpan timediff = DateTime.UtcNow - DateTime.SpecifyKind(Convert.ToDateTime(entity["Timestamp"].ToString()), DateTimeKind.Utc);

                    if (timediff.Days > int.Parse(_cleanupWebjobConfig.Value.CleanUpDurationInDays))
                    {
                        await _azureTableReaderWriter.DeleteEntityAsync(entity.PartitionKey.ToString(), correlationId);

                        await _azureBlobReaderWriter.DeleteContainerAsync(correlationId);

                        _logger.LogDebug(EventIds.EventCleanupSuccessful.ToEventId(), $"Event data cleaned up for {correlationId} successfully.");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(EventIds.ErrorOccurredInCleanupWebJob.ToEventId(), ex, $"Exception occur during clean up webjob process. ErrorMessage : {ex.Message}");
            }

        }
    }
}
