﻿using System.Xml;
using Azure.Storage.Queues.Models;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using UKHO.ERPFacade.Common.IO;
using UKHO.ERPFacade.Common.Configuration;
using UKHO.ERPFacade.Common.HttpClients;
using UKHO.ERPFacade.Common.IO.Azure;
using UKHO.ERPFacade.Common.Models;
using UKHO.ERPFacade.Common.Models.QueueEntities;
using UKHO.ERPFacade.EventAggregation.WebJob.Helpers;
using Microsoft.Extensions.Logging;
using UKHO.ERPFacade.Common.Logging;
using UKHO.ERPFacade.Common.Exceptions;
using Status = UKHO.ERPFacade.Common.Enums.Status;
using UKHO.ERPFacade.Common.Constants;

namespace UKHO.ERPFacade.EventAggregation.WebJob.Services
{
    public class AggregationService : IAggregationService
    {
        private readonly ILogger<AggregationService> _logger;
        private readonly IAzureTableReaderWriter _azureTableReaderWriter;
        private readonly IAzureBlobEventWriter _azureBlobEventWriter;
        private readonly ISapClient _sapClient;
        private readonly IOptions<SapConfiguration> _sapConfig;
        private readonly IRecordOfSaleSapMessageBuilder _recordOfSaleSapMessageBuilder;

        public AggregationService(ILogger<AggregationService> logger, IAzureTableReaderWriter azureTableReaderWriter, IAzureBlobEventWriter azureBlobEventWriter,
            ISapClient sapClient, IOptions<SapConfiguration> sapConfig,
            IRecordOfSaleSapMessageBuilder recordOfSaleSapMessageBuilder)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _azureBlobEventWriter = azureBlobEventWriter ?? throw new ArgumentNullException(nameof(azureBlobEventWriter));
            _azureTableReaderWriter = azureTableReaderWriter ?? throw new ArgumentNullException(nameof(azureTableReaderWriter));
            _sapClient = sapClient;
            _sapConfig = sapConfig ?? throw new ArgumentNullException(nameof(sapConfig));
            _recordOfSaleSapMessageBuilder = recordOfSaleSapMessageBuilder;
        }

        public async Task MergeRecordOfSaleEvents(QueueMessage queueMessage)
        {
            List<RecordOfSaleEventPayLoad> rosEventList = new();
            RecordOfSaleQueueMessageEntity message = JsonConvert.DeserializeObject<RecordOfSaleQueueMessageEntity>(queueMessage.Body.ToString())!;

            try
            {
                _logger.LogInformation(EventIds.MessageDequeueCount.ToEventId(), "Dequeue Count : {DequeueCount} | _X-Correlation-ID : {_X-Correlation-ID} | EventID : {EventID}", queueMessage.DequeueCount.ToString(), message.CorrelationId, message.EventId);

                var entity = await _azureTableReaderWriter.GetEntity(message.CorrelationId, Constants.RecordOfSaleEventTableName);

                if (entity["Status"].ToString() == Status.Incomplete.ToString())
                {
                    List<string> blob = _azureBlobEventWriter.GetBlobNamesInFolder(Constants.RecordOfSaleEventContainerName, message.CorrelationId);

                    if (message.RelatedEvents.All(x => blob.Contains(x)))
                    {
                        foreach (string eventId in message.RelatedEvents)
                        {
                            _logger.LogInformation(EventIds.DownloadRecordOfSaleEventFromAzureBlob.ToEventId(), "Webjob has started downloading record of sale events from blob. | _X-Correlation-ID : {_X-Correlation-ID} | EventID : {EventID}", message.CorrelationId, message.EventId);

                            string rosEvent = _azureBlobEventWriter.DownloadEvent(message.CorrelationId + '/' + eventId + Constants.RecordOfSaleEventFileExtension, Constants.RecordOfSaleEventContainerName);
                            rosEventList.Add(JsonConvert.DeserializeObject<RecordOfSaleEventPayLoad>(rosEvent)!);
                        }

                        XmlDocument sapPayload = _recordOfSaleSapMessageBuilder.BuildRecordOfSaleSapMessageXml(rosEventList, message.CorrelationId);

                        _logger.LogInformation(EventIds.UploadRecordOfSaleSapXmlPayloadInAzureBlob.ToEventId(), "Uploading the SAP xml payload for record of sale event in blob storage. | _X-Correlation-ID : {_X-Correlation-ID} | EventID : {EventID}", message.CorrelationId, message.EventId);
                        await _azureBlobEventWriter.UploadEvent(sapPayload.ToIndentedString(), Constants.RecordOfSaleEventContainerName, message.CorrelationId + '/' + Constants.SapXmlPayloadFileName);
                        _logger.LogInformation(EventIds.UploadedRecordOfSaleSapXmlPayloadInAzureBlob.ToEventId(), "SAP xml payload for record of sale event is uploaded in blob storage successfully. | _X-Correlation-ID : {_X-Correlation-ID} | EventID : {EventID}", message.CorrelationId, message.EventId);

                        HttpResponseMessage response = await _sapClient.PostEventData(sapPayload, _sapConfig.Value.SapEndpointForRecordOfSale, _sapConfig.Value.SapServiceOperationForRecordOfSale, _sapConfig.Value.SapUsernameForRecordOfSale, _sapConfig.Value.SapPasswordForRecordOfSale);

                        if (!response.IsSuccessStatusCode)
                        {
                            throw new ERPFacadeException(EventIds.ErrorOccurredInSapForRecordOfSalePublishedEvent.ToEventId(), $"An error occurred while sending record of sale event data to SAP. | _X-Correlation-ID : {message.CorrelationId} | EventID : {message.EventId} | StatusCode: {response.StatusCode}");
                        }

                        _logger.LogInformation(EventIds.RecordOfSalePublishedEventDataPushedToSap.ToEventId(), "The record of sale event data has been sent to SAP successfully. | _X-Correlation-ID : {_X-Correlation-ID} | EventID : {EventID} | StatusCode: {StatusCode}", message.CorrelationId, message.EventId, response.StatusCode);

                        await _azureTableReaderWriter.UpdateEntity(message.CorrelationId, Constants.RecordOfSaleEventTableName, new[] { new KeyValuePair<string, string>("Status", Status.Complete.ToString()) });
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
