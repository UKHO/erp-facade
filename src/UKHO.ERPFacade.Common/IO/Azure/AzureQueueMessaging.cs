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
    public class AzureQueueMessaging : IAzureQueueMessaging
    {
        private readonly IOptions<AzureStorageConfiguration> _azureStorageConfig;
        private const string RecordOfSaleQueueName = "recordofsaleevents";

        public AzureQueueMessaging(IOptions<AzureStorageConfiguration> azureStorageConfig)
        {
            _azureStorageConfig = azureStorageConfig ?? throw new ArgumentNullException(nameof(azureStorageConfig));
        }

        public async Task SendMessageToQueue(JObject rosEventJson)
        {
            var rosEventData = JsonConvert.DeserializeObject<RecordOfSaleEventPayLoad>(rosEventJson.ToString());
            string queueMessage = BuildQueueMessage(rosEventData);

            QueueClient queueClient = GetQueueClient(RecordOfSaleQueueName);

            await queueClient.SendMessageAsync(queueMessage);
        }

        //Private Methods
        private QueueClient GetQueueClient(string queueName)
        {
            QueueClient queueClient = new(_azureStorageConfig.Value.ConnectionString, queueName.ToLower());
            queueClient.CreateIfNotExistsAsync();

            return queueClient;
        }

        private string BuildQueueMessage(RecordOfSaleEventPayLoad recordOfSaleEventPayLoad)
        {
            QueueMessageEntity queueMessageEntity = new()
            {
                CorrelationId = recordOfSaleEventPayLoad.Data.CorrelationId,
                Type = recordOfSaleEventPayLoad.Type,
                EventId = recordOfSaleEventPayLoad.Id,
                RelatedEvents = recordOfSaleEventPayLoad.Data.RelatedEvents
            };

            string queueMessage = JsonConvert.SerializeObject(queueMessageEntity);
            return queueMessage;
        }
    }
}
