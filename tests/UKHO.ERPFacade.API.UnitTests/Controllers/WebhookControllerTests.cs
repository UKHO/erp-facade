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
using UKHO.ERPFacade.Common.IO;
using UKHO.ERPFacade.Common.Logging;

namespace UKHO.ERPFacade.API.UnitTests.Controllers
{
    [TestFixture]
    public class WebhookControllerTests
    {
        private IHttpContextAccessor _fakeHttpContextAccessor;
        private ILogger<WebhookController> _fakeLogger;
        private IAzureTableReaderWriter _fakeAzureTableReaderWriter;
        private IAzureBlobEventWriter _fakeAzureBlobEventWriter;

        private WebhookController _fakeWebHookController;

        [SetUp]
        public void Setup()
        {
            _fakeHttpContextAccessor = A.Fake<IHttpContextAccessor>();
            _fakeLogger = A.Fake<ILogger<WebhookController>>();
            _fakeAzureTableReaderWriter = A.Fake<IAzureTableReaderWriter>();
            _fakeAzureBlobEventWriter = A.Fake<IAzureBlobEventWriter>();

            _fakeWebHookController = new WebhookController(_fakeHttpContextAccessor,
                                                           _fakeLogger,
                                                           _fakeAzureTableReaderWriter,
                                                           _fakeAzureBlobEventWriter);
        }

        [Test]
        public void WhenValidHeaderRequestedInNewEncContentPublishedEventOptions_ThenWebhookReturns200OkResponse()
        {
            var responseHeaders = A.Fake<IHeaderDictionary>();
            var httpContext = A.Fake<HttpContext>();

            A.CallTo(() => httpContext.Response.Headers).Returns(responseHeaders);
            A.CallTo(() => _fakeHttpContextAccessor.HttpContext).Returns(httpContext);
            A.CallTo(() => httpContext.Request.Headers["WebHook-Request-Origin"]).Returns(new[] { "test.com" });

            var result = (OkObjectResult)_fakeWebHookController.NewEncContentPublishedEventOptions();

            result.StatusCode.Should().Be(200);

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
              && call.GetArgument<LogLevel>(0) == LogLevel.Information
              && call.GetArgument<EventId>(1) == EventIds.NewEncContentPublishedEventOptionsCallStarted.ToEventId()
              && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Started processing the Options request for the New ENC Content Published event for webhook. | WebHook-Request-Origin : {webhookRequestOrigin} | _X-Correlation-ID : {CorrelationId}").MustHaveHappenedOnceExactly();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
              && call.GetArgument<LogLevel>(0) == LogLevel.Information
              && call.GetArgument<EventId>(1) == EventIds.NewEncContentPublishedEventOptionsCallCompleted.ToEventId()
              && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Completed processing the Options request for the New ENC Content Published event for webhook. | WebHook-Request-Origin : {webhookRequestOrigin} | _X-Correlation-ID : {CorrelationId}").MustHaveHappenedOnceExactly();

            A.CallTo(() => responseHeaders.Add("WebHook-Allowed-Rate", "*")).MustHaveHappened();
            A.CallTo(() => responseHeaders.Add("WebHook-Allowed-Origin", "test.com")).MustHaveHappened();
        }

        [Test]
        public async Task WhenValidEventInNewEncContentPublishedEventReceived_ThenWebhookReturns200OkResponse()
        {
            var fakeEncEventJson = JObject.Parse(@"{""data"":{""traceId"":""123""}}");

            var result = (OkObjectResult)await _fakeWebHookController.NewEncContentPublishedEventReceived(fakeEncEventJson);

            A.CallTo(() => _fakeAzureTableReaderWriter.UpsertEntity(A<JObject>.Ignored, A<string>.Ignored, A<string>.Ignored)).MustHaveHappened();
            A.CallTo(() => _fakeAzureBlobEventWriter.UploadEvent(A<JObject>.Ignored, A<string>.Ignored, A<string>.Ignored)).MustHaveHappened();

            result.StatusCode.Should().Be(200);

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
             && call.GetArgument<LogLevel>(0) == LogLevel.Information
             && call.GetArgument<EventId>(1) == EventIds.NewEncContentPublishedEventReceived.ToEventId()
             && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "ERP Facade webhook has received new enccontentpublished event from EES. | _X-Correlation-ID : {CorrelationId}").MustHaveHappenedOnceExactly();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
             && call.GetArgument<LogLevel>(0) == LogLevel.Information
             && call.GetArgument<EventId>(1) == EventIds.StoreEncContentPublishedEventInAzureTable.ToEventId()
             && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Storing the received ENC content published event in azure table. | _X-Correlation-ID : {CorrelationId}").MustHaveHappenedOnceExactly();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
             && call.GetArgument<LogLevel>(0) == LogLevel.Information
             && call.GetArgument<EventId>(1) == EventIds.UploadEncContentPublishedEventInAzureBlob.ToEventId()
             && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Uploading the received ENC content published event in blob storage. | _X-Correlation-ID : {CorrelationId}").MustHaveHappenedOnceExactly();
        }

        [Test]
        public async Task WhenTraceIdIsMissingInNewEncContentPublishedEvent_ThenWebhookReturns400BadRequestResponse()
        {
            var fakeEncEventJson = JObject.Parse(@"{""data"":{""corId"":""123""}}");

            var result = (BadRequestObjectResult)await _fakeWebHookController.NewEncContentPublishedEventReceived(fakeEncEventJson);

            result.StatusCode.Should().Be(400);

            A.CallTo(() => _fakeAzureTableReaderWriter.UpsertEntity(A<JObject>.Ignored, A<string>.Ignored, A<string>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => _fakeAzureBlobEventWriter.UploadEvent(A<JObject>.Ignored, A<string>.Ignored, A<string>.Ignored)).MustNotHaveHappened();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
             && call.GetArgument<LogLevel>(0) == LogLevel.Warning
             && call.GetArgument<EventId>(1) == EventIds.TraceIdMissingInEvent.ToEventId()
             && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "TraceId is missing in ENC content published event. | _X-Correlation-ID : {CorrelationId}").MustHaveHappenedOnceExactly();
        }
    }
}