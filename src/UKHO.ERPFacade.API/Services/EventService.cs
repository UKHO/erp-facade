using Microsoft.Extensions.Options;
using UKHO.ERPFacade.Common.Configuration;
using UKHO.ERPFacade.Common.EventPublisher;
using UKHO.ERPFacade.Common.Exceptions;
using UKHO.ERPFacade.Common.Logging;
using UKHO.ERPFacade.Common.Models;
using UKHO.ERPFacade.Common.Models.CloudEvents;

namespace UKHO.ERPFacade.API.Services
{
    public class EventService : IEventService
    {
        private readonly IEventPublisher _eventPublisher;
        private readonly IOptions<EESConfiguration> _eesConfig;

        public EventService(IEventPublisher eventPublisher, IOptions<EESConfiguration> eesConfig)
        {
            _eventPublisher = eventPublisher;
            _eesConfig = eesConfig ?? throw new ArgumentNullException(nameof(eesConfig));
        }

        public async Task BuildAndPublishEvent(BaseCloudEvent baseCloudEvent, string type)
        {
            baseCloudEvent.Type = type;
            baseCloudEvent.Source = _eesConfig.Value.SourceApplicationUri;
            baseCloudEvent.Time = DateTime.UtcNow.ToString();
            Result result = await _eventPublisher.Publish(baseCloudEvent);
            if (!result.IsSuccess)
            {
                throw new ERPFacadeException(EventIds.EnterpriseEventServiceEventPublisherFailure.ToEventId(),"Error occurred while publishing event to EES");
            }
        }
    }
}
