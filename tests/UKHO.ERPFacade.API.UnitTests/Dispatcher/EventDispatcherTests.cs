using System.Threading.Tasks;
using System;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using UKHO.ERPFacade.API.Dispatcher;
using UKHO.ERPFacade.API.Handlers;
using UKHO.ERPFacade.Common.Models.CloudEvents;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using UKHO.ERPFacade.Common.Logging;
using System.Linq;

namespace UKHO.ERPFacade.API.Tests.Dispatcher
{
    [TestFixture]
    public class EventDispatcherTests
    {
        private ILogger<EventDispatcher> _fakeLogger;
        private IServiceProvider _fakeServiceProvider;
        private EventDispatcher _fakeEventDispatcher;

        [SetUp]
        public void Setup()
        {
            _fakeLogger = A.Fake<ILogger<EventDispatcher>>();
            _fakeServiceProvider = A.Fake<IServiceProvider>();

            _fakeEventDispatcher = new EventDispatcher(_fakeLogger, _fakeServiceProvider);
        }

        //[Test]
        //public async Task WhenEventHandlerExists_ThenEventDispatcherCallsProcessEventAsync()
        //{
        //    var eventType = "uk.gov.ukho.encpublishing.enccontentpublished.v2.2";
        //    var baseCloudEvent = new BaseCloudEvent { Type = eventType };
        //    var eventHandler = A.Fake<IEventHandler>();

        //    A.CallTo(() => _fakeServiceProvider.GetKeyedService<IEventHandler>(eventType)).Returns(eventHandler);

        //    await _fakeEventDispatcher.DispatchEventAsync(baseCloudEvent);

        //    A.CallTo(() => eventHandler.ProcessEventAsync(baseCloudEvent)).MustHaveHappenedOnceExactly();
        //}

        //[Test]
        //public async Task WhenEventHandlerDoesNotExists_ThenEventDispatcherLogsWarning()
        //{
        //    var eventType = "test";
        //    var baseCloudEvent = new BaseCloudEvent { Type = eventType };

        //    A.CallTo(() => _fakeServiceProvider.GetKeyedService<IEventHandler>(eventType)).Returns(null);

        //    await _fakeEventDispatcher.DispatchEventAsync(baseCloudEvent);

        //    A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
        //      && call.GetArgument<LogLevel>(0) == LogLevel.Warning
        //      && call.GetArgument<EventId>(1) == EventIds.InvalidEventTypeReceived.ToEventId()
        //      && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Invalid event type received. No event handler registred for event type {EventType}").MustHaveHappenedOnceExactly();
        //}
    }
}
