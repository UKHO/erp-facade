using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;
using UKHO.ERPFacade.API.Controllers;
using UKHO.ERPFacade.Common.Helpers;
using UKHO.ERPFacade.Common.HttpClients;
using UKHO.ERPFacade.Common.IO;
using UKHO.ERPFacade.Common.Logging;

namespace UKHO.ERPFacade.API.UnitTests.Controllers
{
    [TestFixture]
    public class WebhookControllerTests
    {
        private readonly IHttpContextAccessor _fakeHttpContextAccessor;
        private readonly ILogger<WebhookController> _fakeLogger;
        private readonly IAzureTableReaderWriter _fakeAzureTableReaderWriter;
        private readonly IAzureBlobEventWriter _fakeAzureBlobEventWriter;
        private readonly ISapClient _fakeSapClient;
        private readonly IXmlHelper _fakeXmlHelper;

        private readonly WebhookController _fakeWebHookController;

        [SetUp]
        public void Setup()
        {
            //_fakeHttpContextAccessor = A.Fake<IHttpContextAccessor>();
            //_fakeLogger = A.Fake<ILogger<WebhookController>>();
            //_fakeAzureTableReaderWriter = A.Fake<IAzureTableReaderWriter>();
            //_fakeAzureBlobEventWriter = A.Fake<IAzureBlobEventWriter>();
            //_fakeSapClient = A.Fake<ISapClient>();
            //_fakeXmlHelper = A.Fake<IXmlHelper>();

            //_fakeWebHookController = new WebhookController(_fakeHttpContextAccessor,
            //                                               _fakeLogger,
            //                                               _fakeAzureTableReaderWriter,
            //                                               _fakeAzureBlobEventWriter,
            //                                               _fakeSapClient,
            //                                               _fakeXmlHelper);
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
              && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Started processing the Options request for the New ENC Content Published event for webhook. | WebHook-Request-Origin : {webhookRequestOrigin}").MustHaveHappenedOnceExactly();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
              && call.GetArgument<LogLevel>(0) == LogLevel.Information
              && call.GetArgument<EventId>(1) == EventIds.NewEncContentPublishedEventOptionsCallCompleted.ToEventId()
              && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Completed processing the Options request for the New ENC Content Published event for webhook. | WebHook-Request-Origin : {webhookRequestOrigin}").MustHaveHappenedOnceExactly();

            A.CallTo(() => responseHeaders.Add("WebHook-Allowed-Rate", "*")).MustHaveHappened();
            A.CallTo(() => responseHeaders.Add("WebHook-Allowed-Origin", "test.com")).MustHaveHappened();
        }

        [Test]
        public async Task WhenValidEventInNewEncContentPublishedEventReceived_ThenWebhookReturns200OkResponse()
        {
            XmlDocument xmlDocument = new();

            var fakeEncEventJson = JObject.Parse(@"{""data"":{""traceId"":""123""}}");

            A.CallTo(() => _fakeXmlHelper.CreateXmlDocument(A<string>.Ignored)).Returns(xmlDocument);
            A.CallTo(() => _fakeSapClient.PostEventData(A<XmlDocument>.Ignored, A<string>.Ignored))
                .Returns(new HttpResponseMessage()
                {
                    StatusCode = HttpStatusCode.OK
                });

            var result = (OkObjectResult)await _fakeWebHookController.NewEncContentPublishedEventReceived(fakeEncEventJson);

            A.CallTo(() => _fakeAzureTableReaderWriter.UpsertEntity(A<JObject>.Ignored, A<string>.Ignored)).MustHaveHappened();
            A.CallTo(() => _fakeAzureBlobEventWriter.UploadEvent(A<JObject>.Ignored, A<string>.Ignored)).MustHaveHappened();

            result.StatusCode.Should().Be(200);

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
             && call.GetArgument<LogLevel>(0) == LogLevel.Information
             && call.GetArgument<EventId>(1) == EventIds.NewEncContentPublishedEventReceived.ToEventId()
             && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "ERP Facade webhook has received new enccontentpublished event from EES.").MustHaveHappenedOnceExactly();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
             && call.GetArgument<LogLevel>(0) == LogLevel.Information
             && call.GetArgument<EventId>(1) == EventIds.StoreEncContentPublishedEventInAzureTable.ToEventId()
             && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Storing the received ENC content published event in azure table.").MustHaveHappenedOnceExactly();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
             && call.GetArgument<LogLevel>(0) == LogLevel.Information
             && call.GetArgument<EventId>(1) == EventIds.UploadEncContentPublishedEventInAzureBlob.ToEventId()
             && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Uploading the received ENC content published event in blob storage.").MustHaveHappenedOnceExactly();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
             && call.GetArgument<LogLevel>(0) == LogLevel.Information
             && call.GetArgument<EventId>(1) == EventIds.DataPushedToSap.ToEventId()
             && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Data pushed to SAP successfully. | {StatusCode} | {SapResponse}").MustHaveHappenedOnceExactly();
        }

        [Test]
        public async Task WhenTraceIdIsMissingInNewEncContentPublishedEvent_ThenWebhookReturns400BadRequestResponse()
        {
            var fakeEncEventJson = JObject.Parse(@"{""data"":{""corId"":""123""}}");

            var result = (BadRequestObjectResult)await _fakeWebHookController.NewEncContentPublishedEventReceived(fakeEncEventJson);

            result.StatusCode.Should().Be(400);

            A.CallTo(() => _fakeAzureTableReaderWriter.UpsertEntity(A<JObject>.Ignored, A<string>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => _fakeAzureBlobEventWriter.UploadEvent(A<JObject>.Ignored, A<string>.Ignored)).MustNotHaveHappened();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
             && call.GetArgument<LogLevel>(0) == LogLevel.Warning
             && call.GetArgument<EventId>(1) == EventIds.TraceIdMissingInEvent.ToEventId()
             && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "TraceId is missing in ENC content published event.").MustHaveHappenedOnceExactly();
        }

        [Test]
        public void WhenSapDoesNotRespond200Ok_ThenWebhookReturns500InternalServerResponse()
        {
            XmlDocument xmlDocument = new();

            var fakeEncEventJson = JObject.Parse(@"{""data"":{""traceId"":""123""}}");

            A.CallTo(() => _fakeXmlHelper.CreateXmlDocument(A<string>.Ignored)).Returns(xmlDocument);
            A.CallTo(() => _fakeSapClient.PostEventData(A<XmlDocument>.Ignored, A<string>.Ignored))
                .Returns(new HttpResponseMessage()
                {
                    StatusCode = HttpStatusCode.Unauthorized
                });

            Assert.ThrowsAsync<Exception>(() => _fakeWebHookController.NewEncContentPublishedEventReceived(fakeEncEventJson));

            A.CallTo(() => _fakeAzureTableReaderWriter.UpsertEntity(A<JObject>.Ignored, A<string>.Ignored)).MustHaveHappened();
            A.CallTo(() => _fakeAzureBlobEventWriter.UploadEvent(A<JObject>.Ignored, A<string>.Ignored)).MustHaveHappened();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
             && call.GetArgument<LogLevel>(0) == LogLevel.Information
             && call.GetArgument<EventId>(1) == EventIds.NewEncContentPublishedEventReceived.ToEventId()
             && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "ERP Facade webhook has received new enccontentpublished event from EES.").MustHaveHappenedOnceExactly();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
             && call.GetArgument<LogLevel>(0) == LogLevel.Information
             && call.GetArgument<EventId>(1) == EventIds.StoreEncContentPublishedEventInAzureTable.ToEventId()
             && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Storing the received ENC content published event in azure table.").MustHaveHappenedOnceExactly();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
             && call.GetArgument<LogLevel>(0) == LogLevel.Information
             && call.GetArgument<EventId>(1) == EventIds.UploadEncContentPublishedEventInAzureBlob.ToEventId()
             && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Uploading the received ENC content published event in blob storage.").MustHaveHappenedOnceExactly();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
             && call.GetArgument<LogLevel>(0) == LogLevel.Error
             && call.GetArgument<EventId>(1) == EventIds.SapConnectionFailed.ToEventId()
             && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Could not connect to SAP. | {StatusCode} | {SapResponse}").MustHaveHappenedOnceExactly();
        }
    }
}