using System.Xml;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UKHO.ERPFacade.API.Helpers;
using UKHO.ERPFacade.Common.Configuration;
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
        private readonly IAzureTableReaderWriter _azureTableReaderWriter;
        private const string ErpFacadeTableName = "encevents";
        private readonly ILogger<IS57Service> _logger;
        private readonly IAzureBlobEventWriter _azureBlobEventWriter;
        private readonly IEncContentSapMessageBuilder _encContentSapMessageBuilder;
        private readonly ISapClient _sapClient;
        private const string EncEventFileName = "EncPublishingEvent.json";
        private const string SapXmlPayloadFileName = "SapXmlPayload.xml";
        private readonly IOptions<SapConfiguration> _sapConfig;

        public S57Service(IAzureTableReaderWriter azureTableReaderWriter,
            ILogger<IS57Service> logger,
            IAzureBlobEventWriter azureBlobEventWriter,
            IEncContentSapMessageBuilder encContentSapMessageBuilder,
            ISapClient sapClient,
            IOptions<SapConfiguration> sapConfig)
        {
            _azureTableReaderWriter = azureTableReaderWriter;
            _logger = logger;
            _azureBlobEventWriter = azureBlobEventWriter;
            _encContentSapMessageBuilder = encContentSapMessageBuilder;
            _sapClient = sapClient;
            _sapConfig = sapConfig ?? throw new ArgumentNullException(nameof(sapConfig));
        }

        public async Task ProcessEncContentPublishedEvent(string correlationId, JObject encEventJson)
        {
            _logger.LogInformation(EventIds.StoreEncContentPublishedEventInAzureTable.ToEventId(), "Storing the received ENC content published event in azure table.");

            S57EventEntity encContentPublishEventEntity = new()
            {
                RowKey = Guid.NewGuid().ToString(),
                PartitionKey = Guid.NewGuid().ToString(),
                Timestamp = DateTime.UtcNow,
                CorrelationId = correlationId,
                RequestDateTime = DateTime.UtcNow
            };

            await _azureTableReaderWriter.UpsertEntity(correlationId, ErpFacadeTableName, encContentPublishEventEntity);

            _logger.LogInformation(EventIds.AddedEncContentPublishedEventInAzureTable.ToEventId(), "ENC content published event is added in azure table successfully.");

            _logger.LogInformation(EventIds.UploadEncContentPublishedEventInAzureBlob.ToEventId(), "Uploading the received ENC content published event in blob storage.");
            await _azureBlobEventWriter.UploadEvent(encEventJson.ToString(), correlationId, EncEventFileName);
            _logger.LogInformation(EventIds.UploadedEncContentPublishedEventInAzureBlob.ToEventId(), "ENC content published event is uploaded in blob storage successfully.");

            XmlDocument sapPayload = _encContentSapMessageBuilder.BuildSapMessageXml(JsonConvert.DeserializeObject<EncEventPayload>(encEventJson.ToString()), correlationId);

            _logger.LogInformation(EventIds.UploadSapXmlPayloadInAzureBlobStarted.ToEventId(), "Uploading the SAP xml payload in blob storage.");
            await _azureBlobEventWriter.UploadEvent(sapPayload.ToIndentedString(), correlationId, SapXmlPayloadFileName);
            _logger.LogInformation(EventIds.UploadSapXmlPayloadInAzureBlobCompleted.ToEventId(), "SAP xml payload is uploaded in blob storage successfully.");

            HttpResponseMessage response = await _sapClient.PostEventData(sapPayload, _sapConfig.Value.SapEndpointForEncEvent, _sapConfig.Value.SapServiceOperationForEncEvent, _sapConfig.Value.SapUsernameForEncEvent, _sapConfig.Value.SapPasswordForEncEvent);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(EventIds.ErrorOccuredInSap.ToEventId(), "An error occured while processing your request in SAP. | {StatusCode}", response.StatusCode);
                throw new ERPFacadeException(EventIds.ErrorOccuredInSap.ToEventId());
            }
            _logger.LogInformation(EventIds.EncUpdatePushedToSap.ToEventId(), "ENC update has been sent to SAP successfully. | {StatusCode}", response.StatusCode);

            var encContentPublishEventEntityToUpdate = new[]
                        { new KeyValuePair<string, DateTime>("RequestDateTime", DateTime.UtcNow) };
            await _azureTableReaderWriter.UpdateEntity(correlationId, ErpFacadeTableName, encContentPublishEventEntityToUpdate);
        }
    }
}

