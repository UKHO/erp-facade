using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using UKHO.ERPFacade.API.XmlTransformers;
using UKHO.ERPFacade.Common.Configuration;
using UKHO.ERPFacade.Common.Constants;
using UKHO.ERPFacade.Common.Enums;
using UKHO.ERPFacade.Common.Exceptions;
using UKHO.ERPFacade.Common.Extensions;
using UKHO.ERPFacade.Common.HttpClients;
using UKHO.ERPFacade.Common.Logging;
using UKHO.ERPFacade.Common.Models.CloudEvents;
using UKHO.ERPFacade.Common.Models.CloudEvents.S57Event;
using UKHO.ERPFacade.Common.Models.TableEntities;
using UKHO.ERPFacade.Common.Operations.IO.Azure;

namespace UKHO.ERPFacade.API.Handlers
{
    public class S57EncContentPublishedEventHandler : IEventHandler
    {
        public string EventType => EventTypes.S57EventType;

        private readonly ILogger<S57EncContentPublishedEventHandler> _logger;
        private readonly IXmlTransformer _xmlTransformer;
        private readonly IAzureTableReaderWriter _azureTableReaderWriter;
        private readonly IAzureBlobReaderWriter _azureBlobReaderWriter;
        private readonly ISapClient _sapClient;
        private readonly IOptions<SapConfiguration> _sapConfig;
        private readonly AioConfiguration _aioConfig;

        private readonly List<string> _aioCells = [];

        public S57EncContentPublishedEventHandler([FromKeyedServices("S57EncContentPublishedEventXmlTransformer")] IXmlTransformer xmlTransformer,
                                                  ILogger<S57EncContentPublishedEventHandler> logger,
                                                  IAzureTableReaderWriter azureTableReaderWriter,
                                                  IAzureBlobReaderWriter azureBlobEventWriter,
                                                  ISapClient sapClient,
                                                  IOptions<SapConfiguration> sapConfig,
                                                  IOptions<AioConfiguration> aioConfig)
        {
            _logger = logger;
            _xmlTransformer = xmlTransformer;
            _azureTableReaderWriter = azureTableReaderWriter;
            _azureBlobReaderWriter = azureBlobEventWriter;
            _sapClient = sapClient;
            _sapConfig = sapConfig;
            _aioConfig = aioConfig.Value ?? throw new ArgumentNullException(nameof(aioConfig));

            if (string.IsNullOrEmpty(_aioConfig.AioCells))
            {
                throw new ERPFacadeException(EventIds.AioConfigurationMissingException.ToEventId(), "AIO cell configuration missing.");
            }

            _aioCells = new List<string>(_aioConfig.AioCells.Split(',').Select(s => s.Trim()));
        }

        public async Task ProcessEventAsync(BaseCloudEvent baseCloudEvent)
        {
            _logger.LogInformation(EventIds.S57EventProcessingStarted.ToEventId(), "S57 enccontentpublished event processing started.");

            var s57EventData = JsonConvert.DeserializeObject<S57EventData>(baseCloudEvent.Data.ToString());

            if (s57EventData.Products.Any(x => _aioCells.Contains(x.ProductName)))
            {
                _logger.LogInformation(EventIds.S57EventNotProcessedForAioCells.ToEventId(), "S57 enccontentpublished event is specific to AIO cells and, as a result, it is not processed.");
                return;
            }

            var eventEntity = new EventEntity()
            {
                RowKey = s57EventData.CorrelationId,
                PartitionKey = PartitionKeys.S57PartitionKey,
                Timestamp = DateTime.UtcNow,
                RequestDateTime = null,
                Status = Status.Incomplete.ToString()
            };

            await _azureTableReaderWriter.UpsertEntityAsync(eventEntity);

            _logger.LogInformation(EventIds.S57EventEntryAddedInAzureTable.ToEventId(), "S57 enccontentpublished event entry added in azure table.");

            await _azureBlobReaderWriter.UploadEventAsync(JsonConvert.SerializeObject(baseCloudEvent, Formatting.Indented), s57EventData.CorrelationId, EventPayloadFiles.S57EncEventFileName);

            _logger.LogInformation(EventIds.S57EventJsonStoredInAzureBlobContainer.ToEventId(), "S57 enccontentpublished event json payload is stored in azure blob container.");

            var sapPayload = _xmlTransformer.BuildXmlPayload(s57EventData, XmlTemplateInfo.S57SapXmlTemplatePath);

            await _azureBlobReaderWriter.UploadEventAsync(sapPayload.ToIndentedString(), s57EventData.CorrelationId, EventPayloadFiles.SapXmlPayloadFileName);

            _logger.LogInformation(EventIds.S57EventXmlStoredInAzureBlobContainer.ToEventId(), "S57 enccontentpublished event xml payload is stored in azure blob container.");

            var response = await _sapClient.SendUpdateAsync(sapPayload, _sapConfig.Value.SapEndpointForEncEvent, _sapConfig.Value.SapServiceOperationForEncEvent, _sapConfig.Value.SapUsernameForEncEvent, _sapConfig.Value.SapPasswordForEncEvent);

            if (!response.IsSuccessStatusCode)
            {
                throw new ERPFacadeException(EventIds.S57RequestToSapFailedException.ToEventId(), $"An error occurred while sending S57 ENC update to SAP. | {response.StatusCode}");
            }

            _logger.LogInformation(EventIds.S57EventUpdateSentToSap.ToEventId(), "S57 ENC update has been sent to SAP successfully.");

            await _azureTableReaderWriter.UpdateEntityAsync(eventEntity.PartitionKey, eventEntity.RowKey, new Dictionary<string, object> { { AzureStorage.EventRequestDateTime, DateTime.UtcNow }, { AzureStorage.EventStatus, Status.Complete.ToString() } });
        }
    }
}
