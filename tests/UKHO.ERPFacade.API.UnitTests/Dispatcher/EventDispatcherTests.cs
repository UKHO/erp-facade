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
        private IEventHandler _fakeEventTypeAHandler;
        private IEventHandler _fakeEventTypeBHandler;
        private readonly IList<IEventHandler> _eventHandlers = new List<IEventHandler>();

        [OneTimeSetUp]
        public void Setup()
        {
            _fakeLogger = A.Fake<ILogger<EventDispatcher>>();

            _fakeEventTypeAHandler = A.Fake<IEventHandler>();
            A.CallTo(() => _fakeEventTypeAHandler.EventType).Returns("test_eventype_A");

            _fakeEventTypeBHandler = A.Fake<IEventHandler>();
            A.CallTo(() => _fakeEventTypeBHandler.EventType).Returns("test_eventype_B");

            _eventHandlers.Add(_fakeEventTypeAHandler);
            _eventHandlers.Add(_fakeEventTypeBHandler);

            _fakeEventDispatcher = new EventDispatcher(_fakeLogger, _eventHandlers);
        }

        [Test]
        public async Task WhenEventHandlerExists_ThenEventDispatcherCallsProcessEventAsync()
        {
            var eventType = "test_eventype_A";
            var baseCloudEvent = new BaseCloudEvent { Type = eventType };

            await _fakeEventDispatcher.DispatchEventAsync(baseCloudEvent);

            A.CallTo(() => _fakeEventTypeAHandler.ProcessEventAsync(baseCloudEvent)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _fakeEventTypeBHandler.ProcessEventAsync(baseCloudEvent)).MustNotHaveHappened();
        }

        [Test]
        public async Task WhenEventHandlerDoesNotExists_ThenEventDispatcherLogsWarning()
        {
            var eventType = "test";
            var baseCloudEvent = new BaseCloudEvent { Type = eventType };

            await _fakeEventDispatcher.DispatchEventAsync(baseCloudEvent);

            A.CallTo(() => _fakeEventTypeAHandler.ProcessEventAsync(baseCloudEvent)).MustNotHaveHappened();
            A.CallTo(() => _fakeEventTypeBHandler.ProcessEventAsync(baseCloudEvent)).MustNotHaveHappened();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
              && call.GetArgument<LogLevel>(0) == LogLevel.Warning
              && call.GetArgument<EventId>(1) == EventIds.InvalidEventTypeReceived.ToEventId()
              && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Invalid event type received. No event handler registered for event type {EventType}").MustHaveHappenedOnceExactly();
        }
    }
}
