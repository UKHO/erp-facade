using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using UKHO.ERPFacade.Common.Configuration;
using UKHO.ERPFacade.Common.Constants;
using UKHO.ERPFacade.Common.HttpClients;
using UKHO.ERPFacade.Common.Logging;
using UKHO.ERPFacade.Common.Models;
using UKHO.ERPFacade.Common.Models.CloudEvents;
using UKHO.ERPFacade.Common.Operations.IO.Azure;

namespace UKHO.ERPFacade.API.Services.EventPublishingServices
{
    public class S100UnitOfSaleUpdatedEventPublishingService : IS100UnitOfSaleUpdatedEventPublishingService
    {
        private readonly IEesClient _eesClient;
        private readonly IOptions<EESConfiguration> _eesConfig;
        private readonly IAzureBlobReaderWriter _azureBlobReaderWriter;
        private readonly ILogger<S100UnitOfSaleUpdatedEventPublishingService> _logger;

        public S100UnitOfSaleUpdatedEventPublishingService(IEesClient eesClient,
                                                           IOptions<EESConfiguration> eesConfig,
                                                           IAzureBlobReaderWriter azureBlobReaderWriter,
                                                           ILogger<S100UnitOfSaleUpdatedEventPublishingService> logger)
        {
            _eesClient = eesClient;
            _eesConfig = eesConfig ?? throw new ArgumentNullException(nameof(eesConfig));
            _azureBlobReaderWriter = azureBlobReaderWriter;
            _logger = logger;
        }

        public async Task<Result> PublishEvent(BaseCloudEvent baseCloudEvent, string correlationId)
        {
            baseCloudEvent.Type = EventTypes.S100UnitOfSaleEventType;
            baseCloudEvent.Source = _eesConfig.Value.SourceApplicationUri;
            baseCloudEvent.Id = Guid.NewGuid().ToString();
            baseCloudEvent.Time = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ");

            await _azureBlobReaderWriter.UploadEventAsync(JsonConvert.SerializeObject(baseCloudEvent, Formatting.Indented), correlationId, EventPayloadFiles.S100UnitOfSaleUpdatedEventFileName);

            _logger.LogInformation(EventIds.S100UnitOfSaleUpdatedEventJsonStoredInAzureBlobContainer.ToEventId(), "S-100 unit of sale updated event json payload is stored in azure blob container.");
            HttpResponseMessage response = await _eesClient.PostAsync(baseCloudEvent);

            if (!response.IsSuccessStatusCode)
            {
                return Result.Failure(response.StatusCode.ToString());
            }
            return Result.Success();
        }
    }
}
