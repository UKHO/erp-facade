using Newtonsoft.Json;
using UKHO.ERPFacade.API.Services.EventPublishingServices;
using UKHO.ERPFacade.Common.Constants;
using UKHO.ERPFacade.Common.Enums;
using UKHO.ERPFacade.Common.Exceptions;
using UKHO.ERPFacade.Common.Logging;
using UKHO.ERPFacade.Common.Models.CloudEvents;
using UKHO.ERPFacade.Common.Operations.IO.Azure;
using UKHO.ERPFacade.Services;

namespace UKHO.ERPFacade.API.Services
{
    public class SapCallbackService : ISapCallbackService
    {
        private readonly IAzureBlobReaderWriter _azureBlobReaderWriter;
        private readonly IAzureTableReaderWriter _azureTableReaderWriter;
        private readonly ILogger<SapCallbackService> _logger;
        private readonly IS100UnitOfSaleUpdatedEventPublishingService _s100UnitOfSaleUpdatedEventPublishingService;

        public SapCallbackService(IAzureBlobReaderWriter azureBlobReaderWriter,
            IAzureTableReaderWriter azureTableReaderWriter,
            ILogger<SapCallbackService> logger,
            IS100UnitOfSaleUpdatedEventPublishingService s100UnitOfSaleUpdatedEventPublishingService)
        {
            _azureTableReaderWriter = azureTableReaderWriter;
            _azureBlobReaderWriter = azureBlobReaderWriter;
            _logger = logger;
            _s100UnitOfSaleUpdatedEventPublishingService = s100UnitOfSaleUpdatedEventPublishingService;
        }

        public async Task<bool> IsValidCallbackAsync(string correlationId)
        {
            return await _azureTableReaderWriter.GetEntityAsync(PartitionKeys.S100PartitionKey, correlationId) is not null;
        }

        public async Task ProcessSapCallback(string correlationId)
        {
            _logger.LogInformation(EventIds.ValidS100SapCallback.ToEventId(), "Processing of valid S-100 SAP callback request started.");

            await UpdateResponseDateTimeAsync(correlationId);

            _logger.LogInformation(EventIds.DownloadS100UnitOfSaleUpdatedEventIsStarted.ToEventId(), "Download S-100 Unit Of Sale Updated Event from blob container is started.");

            var baseCloudEvent = await GetEventPayload(correlationId);

            _logger.LogInformation(EventIds.DownloadS100UnitOfSaleUpdatedEventIsCompleted.ToEventId(), "Download S-100 Unit Of Sale Updated Event from blob container is completed.");

            _logger.LogInformation(EventIds.PublishingUnitOfSaleUpdatedEventToEesStarted.ToEventId(), "The publishing unit of sale updated event to EES is started.");

            var result = await _s100UnitOfSaleUpdatedEventPublishingService.PublishEvent(baseCloudEvent, correlationId);

            if (!result.IsSuccess)
            {
                _logger.LogError(EventIds.ErrorOccurredWhilePublishingUnitOfSaleUpdatedEventToEes.ToEventId(), "Error occurred while publishing S-100 unit of sale updated event to EES. | Status : {Status}", result.Error);
                throw new ERPFacadeException(EventIds.ErrorOccurredWhilePublishingUnitOfSaleUpdatedEventToEes.ToEventId(), "Error occurred while publishing S-100 unit of sale updated event to EES.");
            }

            _logger.LogInformation(EventIds.UnitOfSaleUpdatedEventPublished.ToEventId(), "The unit of sale updated event published to EES successfully.");

            await UpdateEventStatusAndEventPublishDateTimeEntity(correlationId);

            _logger.LogInformation(EventIds.S100DataContentPublishedEventTableEntryUpdated.ToEventId(), "Status and event published date time for S-100 data content published event is updated successfully.");
        }

        private async Task<BaseCloudEvent> GetEventPayload(string correlationId)
        {
            string s100DataPublishingEventPayloadJson = await _azureBlobReaderWriter.DownloadEventAsync(EventPayloadFiles.S100DataEventFileName, correlationId.ToLower()).ConfigureAwait(false);

            return JsonConvert.DeserializeObject<BaseCloudEvent>(s100DataPublishingEventPayloadJson);
        }
        private async Task UpdateResponseDateTimeAsync(string correlationId)
        {
            await _azureTableReaderWriter.UpdateEntityAsync(PartitionKeys.S100PartitionKey, correlationId, new Dictionary<string, object> { { "ResponseDateTime", DateTime.UtcNow } });
        }

        private async Task UpdateEventStatusAndEventPublishDateTimeEntity(string correlationId)
        {
            await _azureTableReaderWriter.UpdateEntityAsync(PartitionKeys.S100PartitionKey, correlationId, new Dictionary<string, object> { { "Status", Status.Complete.ToString() } , {"EventPublishDateTime", DateTime.UtcNow } });
        }


    }
}
