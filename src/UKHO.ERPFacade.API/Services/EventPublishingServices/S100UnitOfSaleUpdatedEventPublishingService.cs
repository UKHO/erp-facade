using Microsoft.Extensions.Options;
using UKHO.ERPFacade.Common.Configuration;
using UKHO.ERPFacade.Common.Constants;
using UKHO.ERPFacade.Common.HttpClients;
using UKHO.ERPFacade.Common.Models;
using UKHO.ERPFacade.Common.Models.CloudEvents;

namespace UKHO.ERPFacade.API.Services.EventPublishingServices
{
    public class S100UnitOfSaleUpdatedEventPublishingService : IS100UnitOfSaleUpdatedEventPublishingService
    {
        private readonly IEESClient _eesClient;
        private readonly IOptions<EESConfiguration> _eesConfig;

        public S100UnitOfSaleUpdatedEventPublishingService(IEESClient eesClient, IOptions<EESConfiguration> eesConfig)
        {
            _eesClient = eesClient;
            _eesConfig = eesConfig ?? throw new ArgumentNullException(nameof(eesConfig));
        }

        public async Task<Result> PublishEvent(BaseCloudEvent baseCloudEvent)
        {
            baseCloudEvent.Type = EventTypes.S100UnitOfSaleEventType;
            baseCloudEvent.Source = _eesConfig.Value.SourceApplicationUri;
            baseCloudEvent.Id = Guid.NewGuid().ToString();
            baseCloudEvent.Time = DateTime.UtcNow.ToString();

            return await _eesClient.PostAsync(baseCloudEvent);
        }
    }
}
