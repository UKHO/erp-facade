using Newtonsoft.Json.Linq;
using UKHO.ERPFacade.API.Handlers;
using UKHO.ERPFacade.Common.Models.CloudEvents.S100;
using UKHO.ERPFacade.Common.Models.CloudEvents.S57;

namespace UKHO.ERPFacade.API.Dispatcher
{
    public class EventDispatcher : IEventDispatcher
    {
        private readonly ILogger<EventDispatcher> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly Dictionary<string, Type> _eventHandlers = [];

        public EventDispatcher(IServiceProvider serviceProvider,
                               ILogger<EventDispatcher> logger)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;

            _eventHandlers = new Dictionary<string, Type>
            {
                { "uk.gov.ukho.encpublishing.enccontentpublished.v2.2", typeof(IEventHandler<S57Event>) },
                { "uk.gov.UKHO.ENCPublishing.s100DataContentPublished.v1", typeof(IEventHandler<S100Event>) }
            };
        }

        public async Task DispatchEventAsync(JObject cloudEvent)
        {
            var eventType = cloudEvent["type"]?.ToString();

            if (string.IsNullOrEmpty(eventType))
            {
                throw new Exception("Event type is not specified");
            }

            if (!_eventHandlers.TryGetValue(eventType, out var handlerType))
            {
                throw new Exception($"No handler registered for event type: {eventType}");
            }

            var handler = _serviceProvider.GetService(handlerType) ?? throw new Exception($"No handler implementation found for type: {handlerType.Name}");

            var eventObjectType = handlerType.GetGenericArguments()[0];
            var eventObject = cloudEvent.ToObject(eventObjectType);

            var handleMethod = handlerType.GetMethod("ProcessEventAsync");
            handleMethod.Invoke(handler, [eventObject]);

            await Task.CompletedTask;
        }
    }
}
