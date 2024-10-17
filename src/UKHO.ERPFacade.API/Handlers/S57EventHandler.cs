using CloudNative.CloudEvents;
using Newtonsoft.Json;
using UKHO.ERPFacade.API.XmlTransformers;
using UKHO.ERPFacade.Common.Constants;
using UKHO.ERPFacade.Common.IO;
using UKHO.ERPFacade.Common.IO.Azure;
using UKHO.ERPFacade.Common.Logging;
using UKHO.ERPFacade.Common.Models;
using UKHO.ERPFacade.Common.Models.TableEntities;
namespace UKHO.ERPFacade.API.Handlers
{
    public class S57EventHandler : IEventHandler
    {
        private readonly ILogger<S57EventHandler> _logger;
        private readonly IBaseXmlTransformer _baseXmlTransformer;
        private readonly IAzureTableReaderWriter _azureTableReaderWriter;
        private readonly IAzureBlobEventWriter _azureBlobEventWriter;
        public string EventType => "uk.gov.ukho.encpublishing.enccontentpublished.v2.2";

        public S57EventHandler([FromKeyedServices("S57XmlTransformer")] IBaseXmlTransformer baseXmlTransformer,
                                                                        IAzureTableReaderWriter azureTableReaderWriter,
                                                                        IAzureBlobEventWriter azureBlobEventWriter,
                                                                         ILogger<S57EventHandler> logger)
        {
            _baseXmlTransformer = baseXmlTransformer;
            _logger = logger;
            _azureTableReaderWriter = azureTableReaderWriter;
            _azureBlobEventWriter = azureBlobEventWriter;
        }

        public async Task HandleEventAsync(CloudEvent payload)
        {
            EncEventPayload eventData = new EncEventPayload()
            {
                SpecVersion = payload.SpecVersion.ToString(),
                Type = payload.Type.ToString(),
                Source = payload.Source.ToString(),
                Id = payload.Id.ToString(),
                Time = payload.Time.ToString(),
                Subject = payload.Subject.ToString(),
                DataContentType = payload.DataContentType.ToString(),
                Data = JsonConvert.DeserializeObject<EesEventData>(payload.Data.ToString())
            };

            EncEventEntity encEventEntity = new()
            {
                RowKey = Guid.NewGuid().ToString(),
                PartitionKey = Guid.NewGuid().ToString(),
                Timestamp = DateTime.UtcNow,
                CorrelationId = eventData.Data.CorrelationId,
                RequestDateTime = null
            };

            _logger.LogInformation(EventIds.AddingEntryForEncContentPublishedEventInAzureTable.ToEventId(), "Adding/Updating entry for enccontentpublished event in azure table.");
            await _azureTableReaderWriter.UpsertEntity(encEventEntity.CorrelationId, Constants.S57EventTableName, encEventEntity);
            _logger.LogInformation(EventIds.UploadEncContentPublishedEventInAzureBlobStarted.ToEventId(), "Uploading enccontentpublished event payload in blob storage.");
            await _azureBlobEventWriter.UploadEvent(eventData.ToString(), Constants.S57EventContainerName, encEventEntity.CorrelationId + '/' + Constants.S57EncEventFileName);
            _logger.LogInformation(EventIds.UploadEncContentPublishedEventInAzureBlobCompleted.ToEventId(), "The enccontentpublished event payload is uploaded in blob storage successfully.");

            var sapPayload = _baseXmlTransformer.BuildSapMessageXml(eventData, Constants.S57SapXmlTemplatePath);

            _logger.LogInformation(EventIds.UploadSapXmlPayloadInAzureBlobStarted.ToEventId(), "Uploading the SAP XML payload in blob storage.");
            await _azureBlobEventWriter.UploadEvent(sapPayload.ToIndentedString(), Constants.S57EventContainerName, encEventEntity.CorrelationId + '/' + Constants.SapXmlPayloadFileName);
            _logger.LogInformation(EventIds.UploadSapXmlPayloadInAzureBlobCompleted.ToEventId(), "SAP XML payload is uploaded in blob storage successfully.");

            //var response = await _sapClient.PostEventData(sapPayload, _sapConfig.Value.SapEndpointForEncEvent, _sapConfig.Value.SapServiceOperationForEncEvent, _sapConfig.Value.SapUsernameForEncEvent, _sapConfig.Value.SapPasswordForEncEvent);

            //if (!response.IsSuccessStatusCode)
            //{
            //    throw new ERPFacadeException(EventIds.RequestToSapFailed.ToEventId(), $"An error occurred while sending a request to SAP. | {response.StatusCode}");
            //}
            //_logger.LogInformation(EventIds.EncUpdateSentToSap.ToEventId(), "ENC update has been sent to SAP successfully. | {StatusCode}", response.StatusCode);

            await _azureTableReaderWriter.UpdateEntity(encEventEntity.CorrelationId, Constants.S57EventTableName, new[] { new KeyValuePair<string, DateTime>("RequestDateTime", DateTime.UtcNow) });
            //return response;
        }
    }
}
