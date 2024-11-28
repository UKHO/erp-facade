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

        public async Task CleanAsync()
        {
            try
            {
                var statusFilter = new Dictionary<string, string> { { "Status", Status.Complete.ToString() } };

                var entities = await _azureTableReaderWriter.GetFilteredEntitiesAsync(statusFilter);

                var cleanupDurationInDays = int.Parse(_cleanupWebjobConfig.Value.CleanUpDurationInDays);

                foreach (var entity in entities)
                {
                    var correlationId = entity.RowKey.ToString();

                    var timestamp = DateTime.SpecifyKind(Convert.ToDateTime(entity["RequestDateTime"].ToString()), DateTimeKind.Utc);
                    var timediff = DateTime.UtcNow - timestamp;

                    if (timediff.Days > cleanupDurationInDays)
                    {
                        await _azureTableReaderWriter.DeleteEntityAsync(entity.PartitionKey.ToString(), correlationId);
                        await _azureBlobReaderWriter.DeleteContainerAsync(correlationId);

                        _logger.LogDebug(EventIds.EventCleanupSuccessful.ToEventId(), "Data clean up completed for {CorrelationId} successfully.", correlationId);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(EventIds.ErrorOccurredInCleanupWebJob.ToEventId(), "An error occured during clean up webjob process. ErrorMessage : {Exception}", ex.Message);
            }

        }
    }
}
