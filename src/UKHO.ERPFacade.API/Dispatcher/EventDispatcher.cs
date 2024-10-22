using UKHO.ERPFacade.API.Handlers;
using UKHO.ERPFacade.Common.Models.CloudEvents;

namespace UKHO.ERPFacade.API.Dispatcher
{
    public class EventDispatcher : IEventDispatcher
    {
        private readonly ILogger<EventDispatcher> _logger;
        private readonly IServiceProvider _serviceProvider;
        public EventDispatcher(ILogger<EventDispatcher> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        public async Task DispatchEventAsync(BaseCloudEvent baseCloudEvent)
        {
            var eventType = baseCloudEvent.Type;

            var eventHandler = _serviceProvider.GetKeyedService<IEventHandler>(eventType);

            if (eventHandler is null)
            {
                _logger.LogWarning("No handler registred for event type {EventType}", eventType);
                return;
            }

            await eventHandler.ProcessEventAsync(baseCloudEvent);
        }
    }
}
