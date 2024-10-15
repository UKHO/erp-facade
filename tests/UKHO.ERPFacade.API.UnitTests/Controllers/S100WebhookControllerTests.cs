using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FakeItEasy;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using UKHO.ERPFacade.API.Controllers;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Xml;
using FluentAssertions;
using UKHO.ERPFacade.Common.Logging;

namespace UKHO.ERPFacade.API.UnitTests.Controllers
{
    [TestFixture]
    public class S100WebhookControllerTests
    {
        private IHttpContextAccessor _fakeHttpContextAccessor;
        private ILogger<S100WebhookController> _fakeLogger;
        private S100WebhookController _fakeS100WebHookController;

        [SetUp]
        public void Setup()
        {
            _fakeHttpContextAccessor = A.Fake<IHttpContextAccessor>();
            _fakeLogger = A.Fake<ILogger<S100WebhookController>>();

            _fakeS100WebHookController = new S100WebhookController(_fakeHttpContextAccessor,
                _fakeLogger);
        }

        [Test]
        public void WhenValidHeaderRequestedInS100DataContentPublishedEventOptions_ThenWebhookReturns200OkResponse()
        {
            var responseHeaders = new HeaderDictionary();
            var httpContext = A.Fake<HttpContext>();

            A.CallTo(() => httpContext.Response.Headers).Returns(responseHeaders);
            A.CallTo(() => _fakeHttpContextAccessor.HttpContext).Returns(httpContext);
            A.CallTo(() => httpContext.Request.Headers["WebHook-Request-Origin"]).Returns(new[] { "test.com" });

            var result = (OkObjectResult)_fakeS100WebHookController.S100DataContentPublishedEventOptions();

            result.StatusCode.Should().Be(200);

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
              && call.GetArgument<LogLevel>(0) == LogLevel.Information
              && call.GetArgument<EventId>(1) == EventIds.S100DataContentPublishedEventOptionsCallStarted.ToEventId()
              && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Started processing the Options request for the S100 Data Content Published event for webhook. | WebHook-Request-Origin : {s100WebhookRequestOrigin}").MustHaveHappenedOnceExactly();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
              && call.GetArgument<LogLevel>(0) == LogLevel.Information
              && call.GetArgument<EventId>(1) == EventIds.S100DataContentPublishedEventOptionsCallCompleted.ToEventId()
              && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Completed processing the Options request for the S100 Data Content Published event for webhook. | WebHook-Request-Origin : {s100WebhookRequestOrigin}").MustHaveHappenedOnceExactly();

            Assert.That(responseHeaders.ContainsKey("WebHook-Allowed-Rate"), Is.True);
            Assert.That(responseHeaders["WebHook-Allowed-Rate"], Is.EqualTo("*"));
            Assert.That(responseHeaders.ContainsKey("WebHook-Allowed-Origin"), Is.True);
            Assert.That(responseHeaders["WebHook-Allowed-Origin"], Is.EqualTo("test.com"));
        }

        [Test]
        public async Task WhenValidEventInS100DataContentPublishedEventReceived_ThenWebhookReturns200OkResponse()
        {
            XmlDocument xmlDocument = new();

            var fakeS100dataEventJson = JObject.Parse(@"{""data"":{""correlationId"":""123""}}");

            var result = (OkObjectResult)await _fakeS100WebHookController.S100DataContentPublishedEventReceived(fakeS100dataEventJson);

            result.StatusCode.Should().Be(200);
        }

        [Test]
        public async Task WhenCorrelationIdIsMissingInS100DataContentPublishedEventReceived_ThenWebhookReturns400BadRequestResponse()
        {
            var fakeS100dataEventJson = JObject.Parse(@"{""data"":{""corId"":""123""}}");

            var result = (BadRequestObjectResult)await _fakeS100WebHookController.S100DataContentPublishedEventReceived(fakeS100dataEventJson);

            result.StatusCode.Should().Be(400);

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
             && call.GetArgument<LogLevel>(0) == LogLevel.Warning
             && call.GetArgument<EventId>(1) == EventIds.CorrelationIdMissingInS100DataContentPublishedEvent.ToEventId()
             && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "CorrelationId is missing in s100 data content published event.").MustHaveHappenedOnceExactly();
        }
    }
}
