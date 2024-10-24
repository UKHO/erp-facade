using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using UKHO.ERPFacade.API.Dispatcher;
using UKHO.ERPFacade.API.Handlers;
using UKHO.ERPFacade.Common.Logging;
using UKHO.ERPFacade.Common.Models.CloudEvents;

namespace UKHO.ERPFacade.API.Tests.Dispatcher
{
    [TestFixture]
    public class EventDispatcherTests
    {
        private ILogger<EventDispatcher> _fakeLogger;
        private EventDispatcher _fakeEventDispatcher;
        private IEnumerable<IEventHandler> eventHandlers;
        private readonly Dictionary<string, IEventHandler> _eventHandlers = new Dictionary<string, IEventHandler>();

        [SetUp]
        public void Setup()
        {
            _fakeLogger = A.Fake<ILogger<EventDispatcher>>();
            eventHandlers = A.Fake<IEnumerable<IEventHandler>>();
            _eventHandlers.Add("uk.gov.ukho.encpublishing.enccontentpublished.v2.2", eventHandlers.FirstOrDefault());
            _fakeEventDispatcher = new EventDispatcher(_fakeLogger, eventHandlers);
        }

        [Test]
        public async Task WhenEventHandlerExists_ThenEventDispatcherCallsProcessEventAsync()
        {
            var eventType = "uk.gov.ukho.encpublishing.enccontentpublished.v2.2";
            var baseCloudEvent = new BaseCloudEvent { Type = eventType };

            await _fakeEventDispatcher.DispatchEventAsync(baseCloudEvent);

            if (_eventHandlers.TryGetValue(eventType, out var eventHandler))
            {
                A.CallTo(() => eventHandler.ProcessEventAsync(baseCloudEvent)).MustHaveHappenedOnceExactly();
            }
        }

        [Test]
        public async Task WhenEventHandlerDoesNotExists_ThenEventDispatcherLogsWarning()
        {
            var eventType = "test";
            var baseCloudEvent = new BaseCloudEvent { Type = eventType };

            await _fakeEventDispatcher.DispatchEventAsync(baseCloudEvent);

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
              && call.GetArgument<LogLevel>(0) == LogLevel.Warning
              && call.GetArgument<EventId>(1) == EventIds.InvalidEventTypeReceived.ToEventId()
              && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Invalid event type received. No event handler registred for event type {EventType}").MustHaveHappenedOnceExactly();
        }
    }
}
