using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UKHO.ERPFacade.API.Controllers;
using UKHO.ERPFacade.Common.Configuration;
using UKHO.ERPFacade.Common.Logging;

namespace UKHO.ERPFacade.API.UnitTests.Controllers
{
    [TestFixture]
    public class WebhookControllerTests
    {
        private IHttpContextAccessor _fakeHttpContextAccessor;
        private ILogger<WebhookController> _fakeLogger;

        private WebhookController _fakeWebHookController;

        [SetUp]
        public void Setup()
        {
            _fakeHttpContextAccessor = A.Fake<IHttpContextAccessor>();
            _fakeLogger = A.Fake<ILogger<WebhookController>>();

            _fakeWebHookController = new WebhookController(_fakeHttpContextAccessor, _fakeLogger);
        }

        [Test]
        public void TestDoesWebhookReturns200OkResponseWhenValidHeaderRequestedInNewEncEventPublishedOptions()
        {
            var responseHeaders = A.Fake<IHeaderDictionary>();
            var httpContext = A.Fake<HttpContext>();

            A.CallTo(() => httpContext.Response.Headers).Returns(responseHeaders);
            A.CallTo(() => _fakeHttpContextAccessor.HttpContext).Returns(httpContext);
            A.CallTo(() => httpContext.Request.Headers["WebHook-Request-Origin"]).Returns(new[] { "test.com" });

            var result = (OkObjectResult)_fakeWebHookController.EncContentPublishedOptions();

            result.StatusCode.Should().Be(200);

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
              && call.GetArgument<LogLevel>(0) == LogLevel.Information
              && call.GetArgument<EventId>(1) == EventIds.NewEnsEventPublishedWebhookOptionsCallStarted.ToEventId()
              && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Started processing the Options request for the New Ens Published event for webhook. | WebHook-Request-Origin : {webhookRequestOrigin} | _X-Correlation-ID : {CorrelationId}").MustHaveHappenedOnceExactly();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
              && call.GetArgument<LogLevel>(0) == LogLevel.Information
              && call.GetArgument<EventId>(1) == EventIds.NewEnsEventPublishedWebhookOptionsCallCompleted.ToEventId()
              && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Completed processing the Options request for the New Ens Published event for webhook. | WebHook-Request-Origin : {webhookRequestOrigin} | _X-Correlation-ID : {CorrelationId}").MustHaveHappenedOnceExactly();

            A.CallTo(() => responseHeaders.Add("WebHook-Allowed-Rate", "*")).MustHaveHappened();
            A.CallTo(() => responseHeaders.Add("WebHook-Allowed-Origin", "test.com")).MustHaveHappened();
        }

        [Test]
        public async Task TestDoesWebhookReturns200OkResponseWhenValidNewEncEventReceived()
        {
            var fakeEnsEventJson = JObject.Parse(@"{""dataContentType"":""application/json""}");

            var result = (OkObjectResult)await _fakeWebHookController.EncContentPublished(fakeEnsEventJson);

            result.StatusCode.Should().Be(200);

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
             && call.GetArgument<LogLevel>(0) == LogLevel.Information
             && call.GetArgument<EventId>(1) == EventIds.NewEnsEventReceived.ToEventId()
             && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "ERP Facade webhook has received new event fron EES. | _X-Correlation-ID : {CorrelationId}").MustHaveHappenedOnceExactly();
        }
    }
}