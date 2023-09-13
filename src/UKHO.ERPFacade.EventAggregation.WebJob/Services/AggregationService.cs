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

        private const string RecordOfSaleTableName = "recordofsaleevents";
        private const string RecordOfSaleContainerName = "recordofsaleblobs";
        private const string SapXmlPayloadFileName = "SapXmlPayload.xml";
        private const string IncompleteStatus = "Incomplete";

        public AggregationService(ILogger<AggregationService> logger, IAzureTableReaderWriter azureTableReaderWriter, IAzureBlobEventWriter azureBlobEventWriter,
            ISapClient sapClient, IOptions<SapConfiguration> sapConfig,
            IRecordOfSaleSapMessageBuilder recordOfSaleSapMessageBuilder)
        {
            _logger = logger;
            _azureBlobEventWriter = azureBlobEventWriter ?? throw new ArgumentNullException(nameof(azureBlobEventWriter));
            _azureTableReaderWriter = azureTableReaderWriter ?? throw new ArgumentNullException(nameof(azureTableReaderWriter));
            _sapClient = sapClient;
            _sapConfig = sapConfig ?? throw new ArgumentNullException(nameof(sapConfig));
            _recordOfSaleSapMessageBuilder = recordOfSaleSapMessageBuilder;
        }
    
        public async Task MergeRecordOfSaleEvents(QueueMessage queueMessage)
        {
            List<RecordOfSaleEventPayLoad> rosEventList = new();

            if (queueMessage != null!)
            {
                QueueMessageEntity message = JsonConvert.DeserializeObject<QueueMessageEntity>(queueMessage.Body.ToString())!;

                string status = _azureTableReaderWriter.GetEntityStatus(message.CorrelationId);

                if (status == IncompleteStatus)
                {
                    List<string> blob = _azureBlobEventWriter.GetBlobNamesInFolder(RecordOfSaleContainerName, message.CorrelationId);

                    if (blob.All(x => message.RelatedEvents.Contains(x)))
                    {
                        foreach (string eventId in message.RelatedEvents)
                        {
                            _logger.LogInformation(EventIds.DownloadRecordOfSaleEventFromAzureBlob.ToEventId(), "Webjob started downloading record of sale events from blob.");

                            string rosEvent = _azureBlobEventWriter.DownloadEvent(message.CorrelationId + '/' + eventId + ".json", RecordOfSaleContainerName);
                            rosEventList.Add(JsonConvert.DeserializeObject<RecordOfSaleEventPayLoad>(rosEvent));
                        }

                        XmlDocument sapPayload = _recordOfSaleSapMessageBuilder.BuildRecordOfSaleSapMessageXml(rosEventList, message.CorrelationId);

                        _logger.LogInformation(EventIds.UploadRecordOfSaleSapXmlPayloadInAzureBlob.ToEventId(), "Uploading the SAP xml payload for record of sale event in blob storage.");
                        await _azureBlobEventWriter.UploadEvent(sapPayload.ToIndentedString(), RecordOfSaleContainerName, message.CorrelationId + '/' + SapXmlPayloadFileName);
                        _logger.LogInformation(EventIds.UploadedRecordOfSaleSapXmlPayloadInAzureBlob.ToEventId(), "SAP xml payload for record of sale event is uploaded in blob storage successfully.");

                        HttpResponseMessage response = await _sapClient.PostEventData(sapPayload, _sapConfig.Value.SapServiceOperationForRecordOfSale, _sapConfig.Value.SapUsernameForRecordOfSale, _sapConfig.Value.SapPasswordForRecordOfSale);

                        if (!response.IsSuccessStatusCode)
                        {
                            _logger.LogError(EventIds.ErrorOccurredInSapForRecordOfSalePublishedEvent.ToEventId(), "An error occurred while sending record of sale event data to SAP. | {StatusCode}", response.StatusCode);
                            throw new ERPFacadeException(EventIds.ErrorOccurredInSapForRecordOfSalePublishedEvent.ToEventId());
                        }

                        _logger.LogInformation(EventIds.RecordOfSalePublishedEventDataPushedToSap.ToEventId(), "The record of sale event data has been sent to SAP successfully. | {StatusCode}", response.StatusCode);

                        await _azureTableReaderWriter.UpdateRecordOfSaleEventStatus(message.CorrelationId);
                    }

                    else
                    {
                        _logger.LogWarning(EventIds.AllRelatedEventsAreNotPresentInBlob.ToEventId(), "All related events are not present in Azure blob.");
                    } 
                }
                else
                {
                    _logger.LogWarning(EventIds.RequestAlreadyCompleted.ToEventId(), "The record has been completed already.");
                }
            }
            else
            {
                _logger.LogWarning(EventIds.NoMessageFoundInQueue.ToEventId(), "No message found in Queue Message.");
            }
        }
    }
}
