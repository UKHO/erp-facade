using Newtonsoft.Json.Linq;

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

        public async Task DispatchAsync(JObject payload)
        {
            var eventType = payload["type"]?.ToString();

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
