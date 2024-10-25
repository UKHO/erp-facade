using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Newtonsoft.Json;
using UKHO.ERPFacade.API.XmlTransformers;
using UKHO.ERPFacade.Common.Configuration;
using UKHO.ERPFacade.Common.Constants;
using UKHO.ERPFacade.Common.Exceptions;
using UKHO.ERPFacade.Common.HttpClients;
using UKHO.ERPFacade.Common.IO;
using UKHO.ERPFacade.Common.IO.Azure;
using UKHO.ERPFacade.Common.Logging;
using UKHO.ERPFacade.Common.Models.CloudEvents;
using UKHO.ERPFacade.Common.Models.CloudEvents.S57;
using UKHO.ERPFacade.Common.Models.TableEntities;

namespace UKHO.ERPFacade.API.Handlers
{
    public class S57EventHandler : IEventHandler
    {
        public string EventType => Constants.S57EventType;

        private readonly ILogger<S57EventHandler> _logger;
        private readonly IBaseXmlTransformer _baseXmlTransformer;
        private readonly IAzureTableHelper _azureTableHelper;
        private readonly IAzureBlobHelper _azureBlobHelper;
        private readonly ISapClient _sapClient;
        private readonly IOptions<SapConfiguration> _sapConfig;
        private readonly AioConfiguration _aioConfig;

        private List<string> _aioCells = [];

        public S57EventHandler([FromKeyedServices("S57XmlTransformer")] IBaseXmlTransformer baseXmlTransformer,
                               ILogger<S57EventHandler> logger,
                               IAzureTableHelper azureTableReaderWriter,
                               IAzureBlobHelper azureBlobEventWriter,
                               ISapClient sapClient,
                               IOptions<SapConfiguration> sapConfig,
                               IOptions<AioConfiguration> aioConfig)
        {
            _logger = logger;
            _baseXmlTransformer = baseXmlTransformer;
            _azureTableHelper = azureTableReaderWriter;
            _azureBlobHelper = azureBlobEventWriter;
            _sapClient = sapClient;
            _sapConfig = sapConfig;
            _aioConfig = aioConfig.Value ?? throw new ArgumentNullException(nameof(aioConfig));

            if (string.IsNullOrEmpty(_aioConfig.AioCells))
            {
                throw new ERPFacadeException(EventIds.AioConfigurationMissingException.ToEventId(), "AIO cell configuration missing.");
            }
        }

        public async Task ProcessEventAsync(BaseCloudEvent baseCloudEvent)
        {
            _logger.LogInformation(EventIds.S57EventProcessingStarted.ToEventId(), "S57 enccontentpublished event processing started.");

            S57EventData s57EventData = JsonConvert.DeserializeObject<S57EventData>(baseCloudEvent.Data.ToString());

            if (IsAioCell(s57EventData.Products.Select(x => x.ProductName).ToList()))
            {
                _logger.LogInformation(EventIds.S57EventNotProcessedForAioCells.ToEventId(), "S57 enccontentpublished event is specific to AIO cells and, as a result, it is not processed.");
                return;
            }

            EventEntity eventEntity = new()
            {
                RowKey = s57EventData.CorrelationId,
                PartitionKey = "S57",
                Timestamp = DateTime.UtcNow,
                RequestDateTime = null
            };

            await _azureTableHelper.UpsertEntity(eventEntity);

            _logger.LogInformation(EventIds.S57EventEntryAddedInAzureTable.ToEventId(), "S57 enccontentpublished event entry added in azure table.");

            await _azureBlobHelper.UploadEvent(JsonConvert.SerializeObject(baseCloudEvent, Formatting.Indented), s57EventData.CorrelationId, Constants.S57EncEventFileName);

            _logger.LogInformation(EventIds.S57EventJsonStoredInAzureBlobContainer.ToEventId(), "S57 enccontentpublished event json payload is stored in azure blob container.");

            var sapPayload = _baseXmlTransformer.BuildXmlPayload(s57EventData, Constants.S57SapXmlTemplatePath);

            await _azureBlobHelper.UploadEvent(sapPayload.ToIndentedString(), s57EventData.CorrelationId, Constants.SapXmlPayloadFileName);

            _logger.LogInformation(EventIds.S57EventJsonStoredInAzureBlobContainer.ToEventId(), "S57 enccontentpublished event xml payload is stored in azure blob container.");

            var response = await _sapClient.PostEventData(sapPayload, _sapConfig.Value.SapEndpointForEncEvent, _sapConfig.Value.SapServiceOperationForEncEvent, _sapConfig.Value.SapUsernameForEncEvent, _sapConfig.Value.SapPasswordForEncEvent);

            if (!response.IsSuccessStatusCode)
            {
                throw new ERPFacadeException(EventIds.S57RequestToSapFailedException.ToEventId(), $"An error occurred while sending S57 ENC update to SAP. | {response.StatusCode}");
            }

            _logger.LogInformation(EventIds.S57EventUpdateSentToSap.ToEventId(), "S57 ENC update has been sent to SAP successfully.");

            await _azureTableHelper.UpdateEntity(eventEntity.PartitionKey, eventEntity.RowKey, new[] { new KeyValuePair<string, DateTime>("RequestDateTime", DateTime.UtcNow) });
        }

        /// <summary>
        /// Private method to check if the received enccontentpublished event is for AIO cell
        /// </summary>
        /// <param name="products"></param>
        /// <returns></returns>
        private bool IsAioCell(IEnumerable<string> products)
        {
            var aioCells = !string.IsNullOrEmpty(_aioConfig.AioCells) ? new(_aioConfig.AioCells.Split(',').Select(s => s.Trim())) : new List<string>();
            return products.Any(aioCells.Contains);
        }
    }
}
