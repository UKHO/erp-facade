using Microsoft.Extensions.Options;
using UKHO.ERPFacade.Common.Configuration;
using UKHO.ERPFacade.Common.Constants;
using UKHO.ERPFacade.Common.EventPublisher;
using UKHO.ERPFacade.Common.Models;
using UKHO.ERPFacade.Common.Models.CloudEvents;

namespace UKHO.ERPFacade.API.Services.EventPublishingService
{
    public class S100UnitOfSaleUpdatedEventPublishingService : IS100UnitOfSaleUpdatedEventPublishingService
    {
        private readonly IEventPublisher _eventPublisher;
        private readonly IOptions<EESConfiguration> _eesConfig;

        public S100UnitOfSaleUpdatedEventPublishingService(IEventPublisher eventPublisher, IOptions<EESConfiguration> eesConfig)
        {
            _eventPublisher = eventPublisher;
            _eesConfig = eesConfig ?? throw new ArgumentNullException(nameof(eesConfig));
        }

        public async Task<Result> PublishEvent(BaseCloudEvent baseCloudEvent)
        {
            //Logic to build the event specific to S100 unit of sale updated event and send request to event publisher.

            baseCloudEvent.Type = EventTypes.S100UnitOfSaleEventType;
            baseCloudEvent.Source = _eesConfig.Value.SourceApplicationUri;
            baseCloudEvent.Id = Guid.NewGuid().ToString();
            baseCloudEvent.Time = DateTime.UtcNow.ToString();

            return await _eventPublisher.Publish(baseCloudEvent);
        }
    }
}
