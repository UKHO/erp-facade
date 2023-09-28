using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Options;
using UKHO.ERPFacade.Common.Configuration;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using UKHO.ERPFacade.Common.Models;
using Azure.Storage.Queues;
using UKHO.ERPFacade.Common.Models.QueueEntities;

namespace UKHO.ERPFacade.Common.IO.Azure
{
    [ExcludeFromCodeCoverage]
    public class AzureQueueHelper : IAzureQueueHelper
    {
        private readonly IOptions<AzureStorageConfiguration> _azureStorageConfig;
        private const string RecordOfSaleQueueName = "recordofsaleevents";

        public AzureQueueHelper(IOptions<AzureStorageConfiguration> azureStorageConfig)
        {
            _azureStorageConfig = azureStorageConfig ?? throw new ArgumentNullException(nameof(azureStorageConfig));
        }

        public async Task AddMessage(JObject rosEventJson)
        {
            var rosEventData = JsonConvert.DeserializeObject<RecordOfSaleEventPayLoad>(rosEventJson.ToString());
            string queueMessage = BuildQueueMessage(rosEventData);

            QueueClient queueClient = GetQueueClient(RecordOfSaleQueueName);

            await queueClient.SendMessageAsync(queueMessage);
        }

        //Private Methods
        private QueueClient GetQueueClient(string queueName)
        {
            QueueClient queueClient = new(_azureStorageConfig.Value.ConnectionString, queueName.ToLower(), new QueueClientOptions
            {
                MessageEncoding = QueueMessageEncoding.Base64
            });
            queueClient.CreateIfNotExistsAsync();

            return queueClient;
        }

        private string BuildQueueMessage(RecordOfSaleEventPayLoad recordOfSaleEventPayLoad)
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
