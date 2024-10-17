using CloudNative.CloudEvents;

namespace UKHO.ERPFacade.API.Handlers
{
    public class EventDispatcher : IEventDispatcher
    {
        private readonly Dictionary<string, IEventHandler> _eventHandlers;
        public EventDispatcher(IEnumerable<IEventHandler> eventHandlers)
        {
            _eventHandlers = [];

            foreach (var handler in eventHandlers)
            {
                _eventHandlers.Add(handler.EventType, handler);
            }
        }

        public async Task DispatchAsync(CloudEvent payload)
        {
            var eventType = payload.Type;

            if (!string.IsNullOrEmpty(eventType) && _eventHandlers.TryGetValue(eventType, out var eventHandler))
            {
                await eventHandler.HandleEventAsync(payload);
            }
            else
            {
                //throw exception Unsupported event type.
            }
        }
    }
}
