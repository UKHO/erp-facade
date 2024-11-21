using Newtonsoft.Json;
using UKHO.ERPFacade.Common.Constants;
using UKHO.ERPFacade.Common.Enums;
using UKHO.ERPFacade.Common.Models.CloudEvents;
using UKHO.ERPFacade.Common.Operations.IO.Azure;
using UKHO.ERPFacade.Services;

namespace UKHO.ERPFacade.API.Services
{
    public class SapCallbackService : ISapCallbackService
    {
        private readonly IAzureBlobReaderWriter _azureBlobReaderWriter;
        private readonly IAzureTableReaderWriter _azureTableReaderWriter;

        public SapCallbackService(IAzureBlobReaderWriter azureBlobReaderWriter,
            IAzureTableReaderWriter azureTableReaderWriter)
        {
            _azureTableReaderWriter = azureTableReaderWriter;
            _azureBlobReaderWriter = azureBlobReaderWriter;
        }

        public async Task<bool> IsValidCallbackAsync(string correlationId)
        {
            return await _azureTableReaderWriter.GetEntityAsync(PartitionKeys.S100PartitionKey, correlationId) is not null;
        }

        public async Task<BaseCloudEvent> GetEventPayload(string correlationId)
        {
            string s100DataPublishingEventPayloadJson = await _azureBlobReaderWriter.DownloadEventAsync(EventPayloadFiles.S100DataEventFileName, correlationId.ToLower()).ConfigureAwait(false);

            return JsonConvert.DeserializeObject<BaseCloudEvent>(s100DataPublishingEventPayloadJson);
        }
        public async Task UpdateResponseDateTimeAsync(string correlationId)
        {
            await _azureTableReaderWriter.UpdateEntityAsync(PartitionKeys.S100PartitionKey, correlationId, new Dictionary<string, object> { { "ResponseDateTime", DateTime.UtcNow } });
        }

        public async Task UpdateEventStatusAndEventPublishDateTimeEntity(string correlationId)
        {
            await _azureTableReaderWriter.UpdateEntityAsync(PartitionKeys.S100PartitionKey, correlationId, new Dictionary<string, object> { { "Status", Status.Complete.ToString() } , {"EventPublishDateTime", DateTime.UtcNow } });
        }
    }
}
