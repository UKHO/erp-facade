using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UKHO.ERPFacade.API.Helpers;
using UKHO.ERPFacade.Common.Configuration;
using UKHO.ERPFacade.Common.Constants;
using UKHO.ERPFacade.Common.Exceptions;
using UKHO.ERPFacade.Common.HttpClients;
using UKHO.ERPFacade.Common.IO;
using UKHO.ERPFacade.Common.IO.Azure;
using UKHO.ERPFacade.Common.Logging;
using UKHO.ERPFacade.Common.Models;
using UKHO.ERPFacade.Common.Models.TableEntities;

namespace UKHO.ERPFacade.API.Services
{
    public class S57Service : IS57Service
    {
        private readonly ILogger<S57Service> _logger;
        private readonly IAzureTableReaderWriter _azureTableReaderWriter;
        private readonly IAzureBlobEventWriter _azureBlobEventWriter;
        private readonly IAzureQueueHelper _azureQueueHelper;
        private readonly ISapClient _sapClient;
        private readonly IEncContentSapMessageBuilder _encContentSapMessageBuilder;
        private readonly IOptions<SapConfiguration> _sapConfig;
        private readonly IOptions<AioConfiguration> _aioConfig;
        private List<string> _aioCells = [];

        public S57Service(ILogger<S57Service> logger,
                                 IAzureTableReaderWriter azureTableReaderWriter,
                                 IAzureBlobEventWriter azureBlobEventWriter,
                                 IAzureQueueHelper azureQueueHelper,
                                 ISapClient sapClient,
                                 IEncContentSapMessageBuilder encContentSapMessageBuilder,
                                 IOptions<SapConfiguration> sapConfig,
                                 IOptions<AioConfiguration> aioConfig)
        {
            _logger = logger;
            _azureTableReaderWriter = azureTableReaderWriter;
            _azureBlobEventWriter = azureBlobEventWriter;
            _azureQueueHelper = azureQueueHelper;
            _sapClient = sapClient;
            _encContentSapMessageBuilder = encContentSapMessageBuilder;
            _sapConfig = sapConfig ?? throw new ArgumentNullException(nameof(sapConfig));
            _aioConfig = aioConfig;

            _aioCells = !string.IsNullOrEmpty(_aioConfig.Value.AioCells) ? new(_aioConfig.Value.AioCells.Split(',').Select(s => s.Trim())) :
                throw new ERPFacadeException(EventIds.AioConfigurationNotFoundException.ToEventId(), "Aio cell configuration not found.");
        }

        public async Task ProcessS57Event(JObject encEventJson)
        {
            var eventData = JsonConvert.DeserializeObject<EncEventPayload>(encEventJson.ToString());

            if (IsAioCell(eventData.Data.Products.Select(x => x.ProductName).ToList()))
            {
                _logger.LogInformation(EventIds.NoProcessingOfNewEncContentPublishedEventForAioCells.ToEventId(), "The enccontentpublished event will not be processed for Aio cells.");
                return;
            }

            EncEventEntity encEventEntity = new()
            {
                RowKey = Guid.NewGuid().ToString(),
                PartitionKey = Guid.NewGuid().ToString(),
                Timestamp = DateTime.UtcNow,
                CorrelationId = eventData.Data.CorrelationId,
                RequestDateTime = null
            };

            _logger.LogInformation(EventIds.AddingEntryForEncContentPublishedEventInAzureTable.ToEventId(), "Adding/Updating entry for enccontentpublished event in azure table.");
            await _azureTableReaderWriter.UpsertEntity(eventData.Data.CorrelationId, Constants.S57EventTableName, encEventEntity);

            _logger.LogInformation(EventIds.UploadEncContentPublishedEventInAzureBlobStarted.ToEventId(), "Uploading enccontentpublished event payload in blob storage.");
            await _azureBlobEventWriter.UploadEvent(encEventJson.ToString(), eventData.Data.CorrelationId, Constants.S57EncEventFileName);
            _logger.LogInformation(EventIds.UploadEncContentPublishedEventInAzureBlobCompleted.ToEventId(), "The enccontentpublished event payload is uploaded in blob storage successfully.");

            var sapPayload = _encContentSapMessageBuilder.BuildSapMessageXml(JsonConvert.DeserializeObject<EncEventPayload>(encEventJson.ToString()));

            _logger.LogInformation(EventIds.UploadSapXmlPayloadInAzureBlobStarted.ToEventId(), "Uploading the SAP XML payload in blob storage.");
            await _azureBlobEventWriter.UploadEvent(sapPayload.ToIndentedString(), eventData.Data.CorrelationId, Constants.SapXmlPayloadFileName);
            _logger.LogInformation(EventIds.UploadSapXmlPayloadInAzureBlobCompleted.ToEventId(), "SAP XML payload is uploaded in blob storage successfully.");

            var response = await _sapClient.PostEventData(sapPayload, _sapConfig.Value.SapEndpointForEncEvent, _sapConfig.Value.SapServiceOperationForEncEvent, _sapConfig.Value.SapUsernameForEncEvent, _sapConfig.Value.SapPasswordForEncEvent);

            if (!response.IsSuccessStatusCode)
            {
                throw new ERPFacadeException(EventIds.RequestToSapFailed.ToEventId(), $"An error occurred while sending a request to SAP. | {response.StatusCode}");
            }
            _logger.LogInformation(EventIds.EncUpdateSentToSap.ToEventId(), "ENC update has been sent to SAP successfully");

            await _azureTableReaderWriter.UpdateEntity(eventData.Data.CorrelationId, Constants.S57EventTableName, new[] { new KeyValuePair<string, DateTime>("RequestDateTime", DateTime.UtcNow) });
        }

        private bool IsAioCell(IEnumerable<string> products)
        {
            return products.Any(_aioCells.Contains);
        }
    }
}
