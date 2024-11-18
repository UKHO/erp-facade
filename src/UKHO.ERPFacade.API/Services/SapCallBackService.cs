using Newtonsoft.Json;
using UKHO.ERPFacade.Common.Constants;
using UKHO.ERPFacade.Common.Logging;
using UKHO.ERPFacade.Common.Models.CloudEvents;
using UKHO.ERPFacade.Common.Operations.IO.Azure;

namespace UKHO.ERPFacade.API.Services
{
    public class SapCallBackService : ISapCallBackService
    {
        private readonly IEventService _eventService;
        private readonly IAzureBlobReaderWriter _azureBlobReaderWriter;
        private readonly ILogger<SapCallBackService> _logger;

        public SapCallBackService(IEventService eventService, IAzureBlobReaderWriter azureBlobReaderWriter, ILogger<SapCallBackService> logger)
        {
            _eventService = eventService;
            _azureBlobReaderWriter = azureBlobReaderWriter;
            _logger = logger;
        }

        public async Task DownloadS100EventAndPublishToEes(string correlationId)
        {
            _logger.LogInformation(EventIds.DownloadS100DataPublishingEventPayloadStarted.ToEventId(), "Downloading the S-100 data publishing event payload from azure blob storage.");

            string s100DataPublishingEventPayloadJson = _azureBlobReaderWriter.DownloadEvent(EventPayloadFiles.S100DataEventFileName, correlationId.ToLower());

            BaseCloudEvent s100DataPublishingEventPayloadData = JsonConvert.DeserializeObject<BaseCloudEvent>(s100DataPublishingEventPayloadJson);

            _logger.LogInformation(EventIds.DownloadS100DataPublishingEventPayloadCompleted.ToEventId(), "S-100 data publishing event payload is downloaded from azure blob storage successfully.");

            await _eventService.BuildAndPublishEvent(s100DataPublishingEventPayloadData, EventTypes.S100UnitOfSaleEventType);
        }
    }
}
