using System.Xml;
using Azure.Storage.Queues.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using UKHO.ERPFacade.Common.Configuration;
using UKHO.ERPFacade.Common.Constants;
using UKHO.ERPFacade.Common.Exceptions;
using UKHO.ERPFacade.Common.Extensions;
using UKHO.ERPFacade.Common.HttpClients;
using UKHO.ERPFacade.Common.Logging;
using UKHO.ERPFacade.Common.Models;
using UKHO.ERPFacade.Common.Models.QueueEntities;
using UKHO.ERPFacade.Common.Operations.IO.Azure;
using UKHO.ERPFacade.EventAggregation.WebJob.SapMessageBuilders;
using Status = UKHO.ERPFacade.Common.Enums.Status;

namespace UKHO.ERPFacade.EventAggregation.WebJob.Services
{
    public class AggregationService : IAggregationService
    {
        private readonly ILogger<AggregationService> _logger;
        private readonly IAzureTableReaderWriter _azureTableReaderWriter;
        private readonly IAzureBlobReaderWriter _azureBlobReaderWriter;
        private readonly ISapClient _sapClient;
        private readonly IOptions<SapConfiguration> _sapConfig;
        private readonly IRecordOfSaleSapMessageBuilder _recordOfSaleSapMessageBuilder;

        public AggregationService(ILogger<AggregationService> logger,
                                  IAzureTableReaderWriter azureTableReaderWriter,
                                  IAzureBlobReaderWriter azureBlobReaderWriter,
                                  ISapClient sapClient,
                                  IOptions<SapConfiguration> sapConfig,
                                  IRecordOfSaleSapMessageBuilder recordOfSaleSapMessageBuilder)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _azureBlobReaderWriter = azureBlobReaderWriter ?? throw new ArgumentNullException(nameof(azureBlobReaderWriter));
            _azureTableReaderWriter = azureTableReaderWriter ?? throw new ArgumentNullException(nameof(azureTableReaderWriter));
            _sapClient = sapClient;
            _sapConfig = sapConfig ?? throw new ArgumentNullException(nameof(sapConfig));
            _recordOfSaleSapMessageBuilder = recordOfSaleSapMessageBuilder;
        }

        public async Task MergeRecordOfSaleEventsAsync(QueueMessage queueMessage)
        {
            List<RecordOfSaleEventPayLoad> rosEventList = new();
            RecordOfSaleQueueMessageEntity message = JsonConvert.DeserializeObject<RecordOfSaleQueueMessageEntity>(queueMessage.Body.ToString())!;

            try
            {
                _logger.LogInformation(EventIds.MessageDequeueCount.ToEventId(), "Dequeue Count : {DequeueCount} | _X-Correlation-ID : {_X-Correlation-ID} | EventID : {EventID}", queueMessage.DequeueCount.ToString(), message.CorrelationId, message.EventId);

                var entity = await _azureTableReaderWriter.GetEntityAsync(PartitionKeys.ROSPartitionKey, message.CorrelationId);

                if (entity["Status"].ToString() == Status.Incomplete.ToString())
                {
                    List<string> blob = await _azureBlobReaderWriter.GetBlobNamesInFolderAsync(AzureStorage.RecordOfSaleEventContainerName, message.CorrelationId);

                    if (message.RelatedEvents.All(blob.Contains))
                    {
                        foreach (string eventId in message.RelatedEvents)
                        {
                            _logger.LogInformation(EventIds.DownloadRecordOfSaleEventFromAzureBlob.ToEventId(), "Webjob has started downloading record of sale events from blob. | _X-Correlation-ID : {_X-Correlation-ID} | EventID : {EventID}", message.CorrelationId, message.EventId);

                            string rosEvent = await _azureBlobReaderWriter.DownloadEventAsync(message.CorrelationId + '/' + eventId + EventPayloadFiles.RecordOfSaleEventFileExtension, AzureStorage.RecordOfSaleEventContainerName);
                            rosEventList.Add(JsonConvert.DeserializeObject<RecordOfSaleEventPayLoad>(rosEvent)!);
                        }

                        XmlDocument sapPayload = _recordOfSaleSapMessageBuilder.BuildRecordOfSaleSapMessageXml(rosEventList, message.CorrelationId);

                        _logger.LogInformation(EventIds.UploadRecordOfSaleSapXmlPayloadInAzureBlob.ToEventId(), "Uploading the SAP xml payload for record of sale event in blob storage. | _X-Correlation-ID : {_X-Correlation-ID} | EventID : {EventID}", message.CorrelationId, message.EventId);
                        await _azureBlobReaderWriter.UploadEventAsync(sapPayload.ToIndentedString(), AzureStorage.RecordOfSaleEventContainerName, message.CorrelationId + '/' + EventPayloadFiles.SapXmlPayloadFileName);
                        _logger.LogInformation(EventIds.UploadedRecordOfSaleSapXmlPayloadInAzureBlob.ToEventId(), "SAP xml payload for record of sale event is uploaded in blob storage successfully. | _X-Correlation-ID : {_X-Correlation-ID} | EventID : {EventID}", message.CorrelationId, message.EventId);

                        HttpResponseMessage response = await _sapClient.SendUpdateAsync(sapPayload, _sapConfig.Value.SapEndpointForRecordOfSale, _sapConfig.Value.SapServiceOperationForRecordOfSale, _sapConfig.Value.SapUsernameForRecordOfSale, _sapConfig.Value.SapPasswordForRecordOfSale);

                        if (!response.IsSuccessStatusCode)
                        {
                            throw new ERPFacadeException(EventIds.RecordOfSaleRequestToSapFailedException.ToEventId(), $"An error occurred while sending record of sale event data to SAP. | _X-Correlation-ID : {message.CorrelationId} | EventID : {message.EventId} | StatusCode: {response.StatusCode}");
                        }

                        _logger.LogInformation(EventIds.RecordOfSalePublishedEventDataPushedToSap.ToEventId(), "The record of sale event data has been sent to SAP successfully. | _X-Correlation-ID : {_X-Correlation-ID} | EventID : {EventID} | StatusCode: {StatusCode}", message.CorrelationId, message.EventId, response.StatusCode);

                        await _azureTableReaderWriter.UpdateEntityAsync(PartitionKeys.ROSPartitionKey, message.CorrelationId, new Dictionary<string, object> { { "Status", Status.Complete.ToString() } });
                    }
                    else
                    {
                        _logger.LogWarning(EventIds.AllRelatedEventsAreNotPresentInBlob.ToEventId(), "All related events are not present in Azure blob. | _X-Correlation-ID : {_X-Correlation-ID} | EventID : {EventID}", message.CorrelationId, message.EventId);
                    }
                }
                else
                {
                    _logger.LogWarning(EventIds.RequestAlreadyCompleted.ToEventId(), "The record has been completed already. | _X-Correlation-ID : {_X-Correlation-ID} | EventID : {EventID}", message.CorrelationId, message.EventId);
                }
            }
            catch (Exception)
            {
                throw new ERPFacadeException(EventIds.UnhandledWebJobException.ToEventId(), $"Exception occurred while processing Event Aggregation WebJob. | _X-Correlation-ID : {message.CorrelationId} | EventID : {message.EventId}");
            }
        }
    }
}
