using Newtonsoft.Json;
using UKHO.ERPFacade.Common.Constants;
using UKHO.ERPFacade.Common.Logging;
using UKHO.ERPFacade.Common.Models.CloudEvents;
using UKHO.ERPFacade.Common.Operations.IO.Azure;

namespace UKHO.ERPFacade.API.Services
{
    public class SapCallBackService : ISapCallBackService
    {
        private readonly IAzureBlobReaderWriter _azureBlobReaderWriter;
        private readonly IAzureTableReaderWriter _azureTableReaderWriter;
        private readonly ILogger<SapCallBackService> _logger;

        public SapCallBackService(IAzureBlobReaderWriter azureBlobReaderWriter,
                                  IAzureTableReaderWriter azureTableReaderWriter,
                                  ILogger<SapCallBackService> logger)
        {
            _azureTableReaderWriter = azureTableReaderWriter;
            _azureBlobReaderWriter = azureBlobReaderWriter;
            _logger = logger;
        }

        public async Task<bool> IsValidCallback(string correlationId)
        {
            return await _azureTableReaderWriter.GetEntityAsync(PartitionKeys.S100PartitionKey, correlationId) is not null;
        }

        public async Task<BaseCloudEvent> GetEventPayload(string correlationId)
        {
            string s100DataPublishingEventPayloadJson = await _azureBlobReaderWriter.DownloadEventAsync(EventPayloadFiles.S100DataEventFileName, correlationId.ToLower()).ConfigureAwait(false);

            return JsonConvert.DeserializeObject<BaseCloudEvent>(s100DataPublishingEventPayloadJson);
        }
    }
}
