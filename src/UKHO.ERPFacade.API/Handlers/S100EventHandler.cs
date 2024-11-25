using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using UKHO.ERPFacade.API.Services.EventPublishingServices;
using UKHO.ERPFacade.API.XmlTransformers;
using UKHO.ERPFacade.Common.Configuration;
using UKHO.ERPFacade.Common.Constants;
using UKHO.ERPFacade.Common.Enums;
using UKHO.ERPFacade.Common.Exceptions;
using UKHO.ERPFacade.Common.Extensions;
using UKHO.ERPFacade.Common.HttpClients;
using UKHO.ERPFacade.Common.Logging;
using UKHO.ERPFacade.Common.Models.CloudEvents;
using UKHO.ERPFacade.Common.Models.CloudEvents.S100Event;
using UKHO.ERPFacade.Common.Models.TableEntities;
using UKHO.ERPFacade.Common.Operations.IO.Azure;

namespace UKHO.ERPFacade.API.Handlers
{
    public class S100EventHandler : IEventHandler
    {
        public string EventType => EventTypes.S100EventType;

        private readonly ILogger<S100EventHandler> _logger;
        private readonly IAzureTableReaderWriter _azureTableReaderWriter;
        private readonly IAzureBlobReaderWriter _azureBlobReaderWriter;
        private readonly IBaseXmlTransformer _baseXmlTransformer;
        private readonly ISapClient _sapClient;
        private readonly IOptions<SapConfiguration> _sapConfig;
        private readonly IS100UnitOfSaleUpdatedEventPublishingService _s100UnitOfSaleUpdatedEventPublishingService;

        public S100EventHandler([FromKeyedServices("S100XmlTransformer")] IBaseXmlTransformer baseXmlTransformer,
                                ILogger<S100EventHandler> logger,
                                IAzureTableReaderWriter azureTableReaderWriter,
                                IAzureBlobReaderWriter azureBlobReaderWriter,
                                ISapClient sapClient,
                                IOptions<SapConfiguration> sapConfig,
                                IS100UnitOfSaleUpdatedEventPublishingService s100UnitOfSaleUpdatedEventPublishingService)
        {
            _logger = logger;
            _baseXmlTransformer = baseXmlTransformer;
            _azureTableReaderWriter = azureTableReaderWriter;
            _azureBlobReaderWriter = azureBlobReaderWriter;
            _sapClient = sapClient;
            _sapConfig = sapConfig;
            _s100UnitOfSaleUpdatedEventPublishingService = s100UnitOfSaleUpdatedEventPublishingService;
        }

        public async Task ProcessEventAsync(BaseCloudEvent baseCloudEvent)
        {
            _logger.LogInformation(EventIds.S100EventProcessingStarted.ToEventId(), "S-100 data content published event processing started.");

            S100EventData s100EventData = JsonConvert.DeserializeObject<S100EventData>(baseCloudEvent.Data.ToString());

            EventEntity eventEntity = new()
            {
                RowKey = s100EventData.CorrelationId,
                PartitionKey = PartitionKeys.S100PartitionKey,
                Timestamp = DateTime.UtcNow,
                RequestDateTime = null,
                ResponseDateTime = null,
                Status = Status.Incomplete.ToString()
            };

            await _azureTableReaderWriter.UpsertEntityAsync(eventEntity);

            _logger.LogInformation(EventIds.S100EventEntryAddedInAzureTable.ToEventId(), "S-100 data content published event entry added in azure table.");

            await _azureBlobReaderWriter.UploadEventAsync(JsonConvert.SerializeObject(baseCloudEvent, Formatting.Indented), s100EventData.CorrelationId, EventPayloadFiles.S100DataEventFileName);

            _logger.LogInformation(EventIds.S100EventJsonStoredInAzureBlobContainer.ToEventId(), "S-100 data content published event json payload is stored in azure blob container.");

            var sapPayload = _baseXmlTransformer.BuildXmlPayload(s100EventData, XmlTemplateInfo.S100SapXmlTemplatePath);

            await _azureBlobReaderWriter.UploadEventAsync(sapPayload.ToIndentedString(), s100EventData.CorrelationId, EventPayloadFiles.SapXmlPayloadFileName);

            _logger.LogInformation(EventIds.S100EventXMLStoredInAzureBlobContainer.ToEventId(), "S-100 data content published event xml payload is stored in azure blob container.");

            if (sapPayload.DocumentElement != null && int.TryParse(sapPayload.SelectSingleNode(XmlTemplateInfo.XpathNoOfActions).InnerText, out int actionCount) && actionCount <= 0)
            {
                var result = await _s100UnitOfSaleUpdatedEventPublishingService.PublishEvent(baseCloudEvent, s100EventData.CorrelationId);

                if (!result.IsSuccess)
                {
                    _logger.LogError(EventIds.ErrorOccurredWhilePublishingUnitOfSaleUpdatedEventToEes.ToEventId(), "Error occurred while publishing S-100 unit of sale updated event to EES. | Status:{status}", result.Error);
                    throw new ERPFacadeException(EventIds.ErrorOccurredWhilePublishingUnitOfSaleUpdatedEventToEes.ToEventId(), "Error occurred while publishing S-100 unit of sale updated event to EES.");
                }

                _logger.LogInformation(EventIds.UnitOfSaleUpdatedEventPublished.ToEventId(), "The unit of sale updated event published to EES successfully.");

                await _azureTableReaderWriter.UpdateEntityAsync(PartitionKeys.S100PartitionKey, s100EventData.CorrelationId, new Dictionary<string, object> { { "RequestDateTime", DateTime.UtcNow }, { "Status", Status.Complete.ToString() }, { "EventPublishedDateTime", DateTime.UtcNow } });
            }
            else
            {
                var response = await _sapClient.PostEventData(sapPayload, _sapConfig.Value.SapEndpointForS100Event, _sapConfig.Value.SapServiceOperationForS100Event, _sapConfig.Value.SapUsernameForS100Event, _sapConfig.Value.SapPasswordForS100Event);

                if (!response.IsSuccessStatusCode)
                {
                    throw new ERPFacadeException(EventIds.S100RequestToSapFailedException.ToEventId(), $"An error occurred while sending S-100 data content to SAP. | {response.StatusCode}");
                }

                _logger.LogInformation(EventIds.S100EventUpdateSentToSap.ToEventId(), "S-100 data content has been sent to SAP successfully.");

                await _azureTableReaderWriter.UpdateEntityAsync(eventEntity.PartitionKey, eventEntity.RowKey, new Dictionary<string, object> { { "RequestDateTime", DateTime.UtcNow } });
            }
        }
    }
}
