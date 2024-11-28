using UKHO.ERPFacade.API.Handlers;
using UKHO.ERPFacade.Common.Logging;
using UKHO.ERPFacade.Common.Models.CloudEvents;

namespace UKHO.ERPFacade.API.Dispatcher
{
    public class EventDispatcher : IEventDispatcher
    {
        private readonly ILogger<EventDispatcher> _logger;
        private readonly Dictionary<string, IEventHandler> _eventHandlers;

        public EventDispatcher(ILogger<EventDispatcher> logger, IEnumerable<IEventHandler> eventHandlers)
        {
            _logger = logger;
            _eventHandlers = [];

            foreach (var handler in eventHandlers)
            {
                _eventHandlers.Add(handler.EventType, handler);
            }
        }

        public async Task<bool> DispatchEventAsync(BaseCloudEvent baseCloudEvent)
        {
            if (!_eventHandlers.TryGetValue(baseCloudEvent.Type, out var eventHandler))
            {
                _logger.LogWarning(EventIds.InvalidEventTypeReceived.ToEventId(), "Invalid event type received.");
                return false;
            }

            await eventHandler.ProcessEventAsync(baseCloudEvent);

            return true;
        }
    }
}
