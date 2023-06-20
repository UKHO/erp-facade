using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Threading.Tasks;
using System.Xml;
using UKHO.ERPFacade.API.Controllers;
using UKHO.ERPFacade.API.Helpers;
using UKHO.ERPFacade.API.Models;
using UKHO.ERPFacade.Common.Configuration;
using UKHO.ERPFacade.Common.Exceptions;
using UKHO.ERPFacade.Common.HttpClients;
using UKHO.ERPFacade.Common.IO.Azure;
using UKHO.ERPFacade.Common.IO;
using UKHO.ERPFacade.Common.Logging;
using System;

namespace UKHO.ERPFacade.API.UnitTests.Controllers
{
    [TestFixture]
    public class WebhookControllerTests
    {
        private IHttpContextAccessor _fakeHttpContextAccessor;
        private ILogger<WebhookController> _fakeLogger;
        private IAzureTableReaderWriter _fakeAzureTableReaderWriter;
        private IAzureBlobEventWriter _fakeAzureBlobEventWriter;
        private ISapClient _fakeSapClient;
        private IXmlHelper _fakeXmlHelper;
        private ISapMessageBuilder _fakeSapMessageBuilder;
        private IOptions<SapConfiguration> _fakeSapConfig;
        private WebhookController _fakeWebHookController;

        [SetUp]
        public void Setup()
        {
            _fakeHttpContextAccessor = A.Fake<IHttpContextAccessor>();
            _fakeLogger = A.Fake<ILogger<WebhookController>>();
            _fakeAzureTableReaderWriter = A.Fake<IAzureTableReaderWriter>();
            _fakeAzureBlobEventWriter = A.Fake<IAzureBlobEventWriter>();
            _fakeSapClient = A.Fake<ISapClient>();
            _fakeXmlHelper = A.Fake<IXmlHelper>();
            _fakeSapMessageBuilder = A.Fake<ISapMessageBuilder>();
            _fakeSapConfig = Options.Create(new SapConfiguration()
            {
                SapServiceOperation = "Z_ADDS_MAT_INFO"
            });

            _fakeWebHookController = new WebhookController(_fakeHttpContextAccessor,
                                                           _fakeLogger,
                                                           _fakeAzureTableReaderWriter,
                                                           _fakeAzureBlobEventWriter,
                                                           _fakeSapClient,
                                                           _fakeSapMessageBuilder,
                                                           _fakeSapConfig);
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

            var fakeEncEventJson = JObject.Parse(@"{""data"":{""correlationId"":""123""}}");            

            A.CallTo(() => _fakeXmlHelper.CreateXmlDocument(A<string>.Ignored)).Returns(xmlDocument);
            A.CallTo(() => _fakeSapClient.PostEventData(A<XmlDocument>.Ignored, A<string>.Ignored))
                .Returns(new HttpResponseMessage()
                {
                    StatusCode = HttpStatusCode.OK
                });

            var result = (OkObjectResult)await _fakeWebHookController.NewEncContentPublishedEventReceived(fakeEncEventJson);

            A.CallTo(() => _fakeAzureTableReaderWriter.UpsertEntity(A<string>.Ignored)).MustHaveHappened();
            A.CallTo(() => _fakeAzureTableReaderWriter.UpdateRequestTimeEntity(A<string>.Ignored)).MustHaveHappened();
            A.CallTo(() => _fakeAzureBlobEventWriter.UploadEvent(A<string>.Ignored, A<string>.Ignored, A<string>.Ignored)).MustHaveHappened();
            A.CallTo(() => _fakeSapClient.PostEventData(A<XmlDocument>.Ignored, "Z_ADDS_MAT_INFO")).MustHaveHappened();

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
             && call.GetArgument<EventId>(1) == EventIds.UploadedEncContentPublishedEventInAzureBlob.ToEventId()
             && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "ENC content published event is uploaded in blob storage successfully.").MustHaveHappenedOnceExactly();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
             && call.GetArgument<LogLevel>(0) == LogLevel.Information
             && call.GetArgument<EventId>(1) == EventIds.EncUpdatePushedToSap.ToEventId()
             && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "ENC update has been sent to SAP successfully. | {StatusCode}").MustHaveHappenedOnceExactly();
        }

        [Test]
        public async Task WhenCorrelationIdIsMissingInNewEncContentPublishedEvent_ThenWebhookReturns400BadRequestResponse()
        {
            var fakeEncEventJson = JObject.Parse(@"{""data"":{""corId"":""123""}}");

            var result = (BadRequestObjectResult)await _fakeWebHookController.NewEncContentPublishedEventReceived(fakeEncEventJson);

            result.StatusCode.Should().Be(400);

            A.CallTo(() => _fakeAzureTableReaderWriter.UpsertEntity(A<string>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => _fakeAzureTableReaderWriter.UpdateRequestTimeEntity(A<string>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => _fakeAzureBlobEventWriter.UploadEvent(A<string>.Ignored, A<string>.Ignored, A<string>.Ignored)).MustNotHaveHappened();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
             && call.GetArgument<LogLevel>(0) == LogLevel.Warning
             && call.GetArgument<EventId>(1) == EventIds.CorrelationIdMissingInEvent.ToEventId()
             && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "CorrelationId is missing in ENC content published event.").MustHaveHappenedOnceExactly();
        }

        [Test]
        public void WhenSapDoesNotRespond200Ok_ThenWebhookReturns500InternalServerResponse()
        {
            XmlDocument xmlDocument = new();
            
            var fakeEncEventJson = JObject.Parse(@"{""data"":{""correlationId"":""123""}}");

            A.CallTo(() => _fakeXmlHelper.CreateXmlDocument(A<string>.Ignored)).Returns(xmlDocument);
            A.CallTo(() => _fakeSapClient.PostEventData(A<XmlDocument>.Ignored, A<string>.Ignored))
                .Returns(new HttpResponseMessage()
                {
                    StatusCode = HttpStatusCode.Unauthorized
                });

            Assert.ThrowsAsync<ERPFacadeException>(() => _fakeWebHookController.NewEncContentPublishedEventReceived(fakeEncEventJson));

            A.CallTo(() => _fakeAzureTableReaderWriter.UpsertEntity(A<string>.Ignored)).MustHaveHappened();
            A.CallTo(() => _fakeAzureTableReaderWriter.UpdateRequestTimeEntity(A<string>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => _fakeAzureBlobEventWriter.UploadEvent(A<string>.Ignored, A<string>.Ignored, A<string>.Ignored)).MustHaveHappened();

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
             && call.GetArgument<EventId>(1) == EventIds.UploadedEncContentPublishedEventInAzureBlob.ToEventId()
             && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "ENC content published event is uploaded in blob storage successfully.").MustHaveHappenedOnceExactly();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
             && call.GetArgument<LogLevel>(0) == LogLevel.Error
             && call.GetArgument<EventId>(1) == EventIds.ErrorOccuredInSap.ToEventId()
             && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "An error occured while processing your request in SAP. | {StatusCode}").MustHaveHappenedOnceExactly();
        }

        [Test]
        public void Does_Constructor_Throws_ArgumentNullException_When_Paramter_Is_Null()
        {
            Assert.Throws<ArgumentNullException>(
             () => new WebhookController(_fakeHttpContextAccessor,
                                                           _fakeLogger,
                                                           _fakeAzureTableReaderWriter,
                                                           _fakeAzureBlobEventWriter,
                                                           _fakeSapClient,
                                                           _fakeSapMessageBuilder,
                                                           null))
             .ParamName
             .Should().Be("sapConfig");
        }
    }
}
