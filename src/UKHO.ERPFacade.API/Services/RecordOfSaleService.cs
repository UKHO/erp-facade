using Azure.Data.Tables;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UKHO.ERPFacade.Common.IO.Azure;
using UKHO.ERPFacade.Common.Logging;
using UKHO.ERPFacade.Common.Models.QueueEntities;
using UKHO.ERPFacade.Common.Models;
using UKHO.ERPFacade.Common.Enums;

namespace UKHO.ERPFacade.API.Services
{
    public class RecordOfSaleService : IRecordOfSaleService
    {
        private readonly IAzureTableReaderWriter _azureTableReaderWriter;
        private const string RecordOfSaleContainerName = "recordofsaleblobs";
        private readonly ILogger<AzureTableReaderWriter> _logger;
        private readonly IAzureBlobEventWriter _azureBlobEventWriter;
        private const string JsonFileType = ".json";
        private readonly IAzureQueueHelper _azureQueueHelper;
        private const string RecordOfSaleTableName = "recordofsaleevents";

        public RecordOfSaleService(IAzureTableReaderWriter azureTableReaderWriter,
            ILogger<AzureTableReaderWriter> logger,
            IAzureBlobEventWriter azureBlobEventWriter,
            IAzureQueueHelper azureQueueHelper) {
            _azureTableReaderWriter = azureTableReaderWriter;
            _logger = logger;
            _azureBlobEventWriter = azureBlobEventWriter;
            _azureQueueHelper = azureQueueHelper;
        }

        public async Task ProcessRecordOfSaleEvent(string correlationId, JObject recordOfSaleEventJson, string eventId)
        {
            _logger.LogInformation(EventIds.StoreRecordOfSalePublishedEventInAzureTable.ToEventId(), "Storing the received Record of sale published event in azure table.");

            TableEntity rosEventEntity = new TableEntity();
            rosEventEntity["Status"] = Statuses.Incomplete.ToString();

            await _azureTableReaderWriter.UpsertEntity(correlationId, RecordOfSaleTableName, rosEventEntity);

            _logger.LogInformation(EventIds.UploadRecordOfSalePublishedEventInAzureBlob.ToEventId(), "Uploading the received Record of sale published event in blob storage.");
            await _azureBlobEventWriter.UploadEvent(recordOfSaleEventJson.ToString(), RecordOfSaleContainerName, correlationId + '/' + eventId + JsonFileType);
            _logger.LogInformation(EventIds.UploadedRecordOfSalePublishedEventInAzureBlob.ToEventId(), "Record of sale published event is uploaded in blob storage successfully.");

            _logger.LogInformation(EventIds.AddMessageToAzureQueue.ToEventId(), "Adding the received Record of sale published event in queue storage.");

            var rosEventData = JsonConvert.DeserializeObject<RecordOfSaleEventPayLoad>(recordOfSaleEventJson.ToString());
            var queueMessage = BuildMessage(rosEventData);

            await _azureQueueHelper.AddMessage(queueMessage);
            _logger.LogInformation(EventIds.AddedMessageToAzureQueue.ToEventId(), "Record of sale published event is added in queue storage successfully.");
        }

        private string BuildMessage(RecordOfSaleEventPayLoad recordOfSaleEventPayLoad)
        {
            RecordOfSaleQueueMessageEntity recordOfSaleQueueMessageEntity = new()
            {
                CorrelationId = recordOfSaleEventPayLoad.Data.CorrelationId,
                Type = recordOfSaleEventPayLoad.Type,
                EventId = recordOfSaleEventPayLoad.Id,
                TransactionType = recordOfSaleEventPayLoad.Data.RecordsOfSale.TransactionType,
                RelatedEvents = recordOfSaleEventPayLoad.Data.RelatedEvents
            };

            string queueMessage = JsonConvert.SerializeObject(recordOfSaleQueueMessageEntity);
            return queueMessage;
        }
    }
}
