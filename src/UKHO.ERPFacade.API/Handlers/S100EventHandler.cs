using Newtonsoft.Json;
using UKHO.ERPFacade.Common.Constants;
using UKHO.ERPFacade.Common.Enums;
using UKHO.ERPFacade.Common.IO.Azure;
using UKHO.ERPFacade.Common.Logging;
using UKHO.ERPFacade.Common.Models.CloudEvents;
using UKHO.ERPFacade.Common.Models.CloudEvents.S100;
using UKHO.ERPFacade.Common.Models.TableEntities;

namespace UKHO.ERPFacade.API.Handlers
{
    public class S100EventHandler : IEventHandler
    {
        public string EventType => Constants.S100EventType;
        private readonly ILogger<S100EventHandler> _logger;
        private readonly IAzureTableHelper _azureTableHelper;
        private readonly IAzureBlobHelper _azureBlobHelper;

        public S100EventHandler(ILogger<S100EventHandler> logger, IAzureTableHelper azureTableHelper, IAzureBlobHelper azureBlobHelper)
        {
            _logger = logger;
            _azureTableHelper = azureTableHelper;
            _azureBlobHelper = azureBlobHelper;
        }

        public async Task ProcessEventAsync(BaseCloudEvent baseCloudEvent)
        {
            _logger.LogInformation(EventIds.S100EventProcessingStarted.ToEventId(), "S100 data content published event processing started.");

            S100EventData s100EventData = JsonConvert.DeserializeObject<S100EventData>(baseCloudEvent.Data.ToString());

            EventEntity eventEntity = new()
            {
                RowKey = s100EventData.CorrelationId,
                PartitionKey = Constants.S100PartitionKey,
                Timestamp = DateTime.UtcNow,
                RequestDateTime = null,
                ResponseDateTime = null,
                Status = Status.Incomplete.ToString()
            };

            await _azureTableHelper.UpsertEntity(eventEntity);

            _logger.LogInformation(EventIds.S100EventEntryAddedInAzureTable.ToEventId(), "S100 data content published event entry added in azure table.");

            await _azureBlobHelper.UploadEvent(JsonConvert.SerializeObject(baseCloudEvent, Formatting.Indented), s100EventData.CorrelationId, Constants.S100DataEventFileName);

            _logger.LogInformation(EventIds.S100EventJsonStoredInAzureBlobContainer.ToEventId(), "S100 data content published event json payload is stored in azure blob container.");
        }
    }
}
