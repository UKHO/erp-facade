using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;
using Azure.Data.Tables;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using UKHO.ERPFacade.API.Controllers;
using UKHO.ERPFacade.API.Dispatcher;
using UKHO.ERPFacade.API.Helpers;
using UKHO.ERPFacade.Common.Configuration;
using UKHO.ERPFacade.Common.Exceptions;
using UKHO.ERPFacade.Common.HttpClients;
using UKHO.ERPFacade.Common.Logging;
using UKHO.ERPFacade.Common.Models;
using UKHO.ERPFacade.Common.Models.CloudEvents;
using UKHO.ERPFacade.Common.Models.TableEntities;
using UKHO.ERPFacade.Common.Operations.IO.Azure;

namespace UKHO.ERPFacade.API.UnitTests.Controllers
{
    [TestFixture]
    public class WebhookControllerTests
    {
        private IHttpContextAccessor _fakeHttpContextAccessor;
        private ILogger<WebhookController> _fakeLogger;
        private IEventDispatcher _fakeEventDispatcher;
        private IAzureTableReaderWriter _fakeAzureTableReaderWriter;
        private IAzureBlobReaderWriter _fakeAzureBlobReaderWriter;
        private IAzureQueueReaderWriter _fakeAzureQueueReaderWriter;
        private ISapClient _fakeSapClient;
        private IOptions<SapConfiguration> _fakeSapConfig;
        private WebhookController _fakeWebHookController;
        private ILicenceUpdatedSapMessageBuilder _fakeLicenceUpdatedSapMessageBuilder;

        [SetUp]
        public void Setup()
        {
            _fakeHttpContextAccessor = A.Fake<IHttpContextAccessor>();
            _fakeLogger = A.Fake<ILogger<WebhookController>>();
            _fakeEventDispatcher = A.Fake<IEventDispatcher>();
            _fakeAzureTableReaderWriter = A.Fake<IAzureTableReaderWriter>();
            _fakeAzureBlobReaderWriter = A.Fake<IAzureBlobReaderWriter>();
            _fakeAzureQueueReaderWriter = A.Fake<IAzureQueueReaderWriter>();
            _fakeSapClient = A.Fake<ISapClient>();
            _fakeLicenceUpdatedSapMessageBuilder = A.Fake<ILicenceUpdatedSapMessageBuilder>();
            _fakeSapConfig = Options.Create(new SapConfiguration()
            {
                SapServiceOperationForEncEvent = "Z_ADDS_MAT_INFO"
            });

            _fakeWebHookController = new WebhookController(_fakeHttpContextAccessor,
                                                           _fakeLogger,
                                                           _fakeEventDispatcher,
                                                           _fakeAzureTableReaderWriter,
                                                           _fakeAzureBlobReaderWriter,
                                                           _fakeAzureQueueReaderWriter,
                                                           _fakeLicenceUpdatedSapMessageBuilder,
                                                           _fakeSapClient,
                                                           _fakeSapConfig);
        }

        [Test]
        public void WhenValidHeaderRequestedInEventOptionsEndpoint_ThenWebhookReturns200OkResponse()
        {
            var responseHeaders = new HeaderDictionary();
            var httpContext = A.Fake<HttpContext>();

            A.CallTo(() => httpContext.Response.Headers).Returns(responseHeaders);
            A.CallTo(() => _fakeHttpContextAccessor.HttpContext).Returns(httpContext);
            A.CallTo(() => httpContext.Request.Headers["WebHook-Request-Origin"]).Returns(new[] { "test.com" });

            var result = (OkObjectResult)_fakeWebHookController.ReceiveEvents();

            result.StatusCode.Should().Be(200);

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
              && call.GetArgument<LogLevel>(0) == LogLevel.Information
              && call.GetArgument<EventId>(1) == EventIds.ErpFacadeWebhookOptionsEndPointRequested.ToEventId()
              && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "ERP facade webhook options endpoint requested.").MustHaveHappenedOnceExactly();

            Assert.That(responseHeaders.ContainsKey("WebHook-Allowed-Rate"), Is.True);
            Assert.That(responseHeaders["WebHook-Allowed-Rate"], Is.EqualTo("*"));
            Assert.That(responseHeaders.ContainsKey("WebHook-Allowed-Origin"), Is.True);
            Assert.That(responseHeaders["WebHook-Allowed-Origin"], Is.EqualTo("test.com"));
        }

        [Test]
        public async Task WhenValidCloudEventReceived_ThenWebhookReturnsOkResponse()
        {
            var fakeCloudEvent = JObject.Parse(@"{""data"":{""correlationId"":""123""}}");

            var result = (OkObjectResult)await _fakeWebHookController.ReceiveEventsAsync(fakeCloudEvent);

            Assert.That(StatusCodes.Status200OK, Is.EqualTo(result.StatusCode));

            A.CallTo(() => _fakeEventDispatcher.DispatchEventAsync(A<BaseCloudEvent>.Ignored)).MustHaveHappenedOnceExactly();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
             && call.GetArgument<LogLevel>(0) == LogLevel.Information
             && call.GetArgument<EventId>(1) == EventIds.NewCloudEventReceived.ToEventId()
             && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "ERP facade received new cloud event.").MustHaveHappenedOnceExactly();
        }

        [Test]
        public void WhenParamterIsNull_ThenWebhookThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new WebhookController(_fakeHttpContextAccessor,
                                                                             _fakeLogger,
                                                                             _fakeEventDispatcher,
                                                                             _fakeAzureTableReaderWriter,
                                                                             _fakeAzureBlobReaderWriter,
                                                                             _fakeAzureQueueReaderWriter,
                                                                             _fakeLicenceUpdatedSapMessageBuilder,
                                                                             _fakeSapClient,
                                                                             null))
             .ParamName
             .Should().Be("sapConfig");
        }

        [Test]
        public void WhenValidHeaderRequestedInRecordOfSalePublishedEventOptions_ThenWebhookReturns200OkResponse()
        {
            var responseHeaders = new HeaderDictionary();
            var httpContext = A.Fake<HttpContext>();

            A.CallTo(() => httpContext.Response.Headers).Returns(responseHeaders);
            A.CallTo(() => _fakeHttpContextAccessor.HttpContext).Returns(httpContext);
            A.CallTo(() => httpContext.Request.Headers["WebHook-Request-Origin"]).Returns(new[] { "test.com" });

            var result = (OkObjectResult)_fakeWebHookController.RecordOfSalePublishedEventOptions();

            result.StatusCode.Should().Be(200);

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
              && call.GetArgument<LogLevel>(0) == LogLevel.Information
              && call.GetArgument<EventId>(1) == EventIds.RecordOfSalePublishedEventOptionsCallStarted.ToEventId()
              && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Started processing the Options request for the Record of Sale Published event for webhook. | WebHook-Request-Origin : {webhookRequestOrigin}").MustHaveHappenedOnceExactly();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
              && call.GetArgument<LogLevel>(0) == LogLevel.Information
              && call.GetArgument<EventId>(1) == EventIds.RecordOfSalePublishedEventOptionsCallCompleted.ToEventId()
              && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Completed processing the Options request for the Record of Sale Published event for webhook. | WebHook-Request-Origin : {webhookRequestOrigin}").MustHaveHappenedOnceExactly();

            Assert.That(responseHeaders.ContainsKey("WebHook-Allowed-Rate"), Is.True);
            Assert.That(responseHeaders["WebHook-Allowed-Rate"], Is.EqualTo("*"));
            Assert.That(responseHeaders.ContainsKey("WebHook-Allowed-Origin"), Is.True);
            Assert.That(responseHeaders["WebHook-Allowed-Origin"], Is.EqualTo("test.com"));
        }

        [Test]
        public async Task WhenValidEventInRecordOfSalePublishedEventReceived_ThenWebhookReturns200OkResponse()
        {
            var fakeRosEventJson = JObject.Parse(@"{""data"":{""correlationId"":""123""}}");
            var eventEntity = new EventEntity();

            var result = (OkObjectResult)await _fakeWebHookController.RecordOfSalePublishedEventReceived(fakeRosEventJson);
            result.StatusCode.Should().Be(200);

            A.CallTo(() => _fakeAzureTableReaderWriter.UpsertEntityAsync(A<ITableEntity>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _fakeAzureBlobReaderWriter.UploadEventAsync(A<string>.Ignored, A<string>.Ignored, A<string>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _fakeAzureQueueReaderWriter.AddMessageAsync(fakeRosEventJson)).MustHaveHappenedOnceExactly();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
             && call.GetArgument<LogLevel>(0) == LogLevel.Information
             && call.GetArgument<EventId>(1) == EventIds.RecordOfSalePublishedEventReceived.ToEventId()
             && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "ERP Facade webhook has received record of sale event from EES.").MustHaveHappenedOnceExactly();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
             && call.GetArgument<LogLevel>(0) == LogLevel.Information
             && call.GetArgument<EventId>(1) == EventIds.StoreRecordOfSalePublishedEventInAzureTable.ToEventId()
             && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Storing the received Record of sale published event in azure table.").MustHaveHappenedOnceExactly();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
             && call.GetArgument<LogLevel>(0) == LogLevel.Information
             && call.GetArgument<EventId>(1) == EventIds.UploadRecordOfSalePublishedEventInAzureBlob.ToEventId()
             && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Uploading the received Record of sale published event in blob storage.").MustHaveHappenedOnceExactly();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
            && call.GetArgument<LogLevel>(0) == LogLevel.Information
            && call.GetArgument<EventId>(1) == EventIds.UploadedRecordOfSalePublishedEventInAzureBlob.ToEventId()
            && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Record of sale published event is uploaded in blob storage successfully.").MustHaveHappenedOnceExactly();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
            && call.GetArgument<LogLevel>(0) == LogLevel.Information
            && call.GetArgument<EventId>(1) == EventIds.AddMessageToAzureQueue.ToEventId()
            && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Adding the received Record of sale published event in queue storage.").MustHaveHappenedOnceExactly();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
            && call.GetArgument<LogLevel>(0) == LogLevel.Information
            && call.GetArgument<EventId>(1) == EventIds.AddedMessageToAzureQueue.ToEventId()
            && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Record of sale published event is added in queue storage successfully.").MustHaveHappenedOnceExactly();
        }

        [Test]
        public async Task WhenCorrelationIdIsMissingInRecordOfSalePublishedEvent_ThenWebhookReturns400BadRequestResponse()
        {
            var fakeRosEventJson = JObject.Parse(@"{""data"":{""corId"":""123""}}");

            var result = (BadRequestObjectResult)await _fakeWebHookController.RecordOfSalePublishedEventReceived(fakeRosEventJson);

            result.StatusCode.Should().Be(400);

            A.CallTo(() => _fakeAzureTableReaderWriter.UpsertEntityAsync(A<ITableEntity>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => _fakeAzureBlobReaderWriter.UploadEventAsync(A<string>.Ignored, A<string>.Ignored, A<string>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => _fakeAzureQueueReaderWriter.AddMessageAsync(fakeRosEventJson)).MustNotHaveHappened();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
                                                && call.GetArgument<LogLevel>(0) == LogLevel.Information
                                                && call.GetArgument<EventId>(1) == EventIds.RecordOfSalePublishedEventReceived.ToEventId()
                                                && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "ERP Facade webhook has received record of sale event from EES.").MustHaveHappenedOnceExactly();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
                                                && call.GetArgument<LogLevel>(0) == LogLevel.Warning
                                                && call.GetArgument<EventId>(1) == EventIds.CorrelationIdMissingInRecordOfSaleEvent.ToEventId()
                                                && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "CorrelationId is missing in Record of Sale published event.").MustHaveHappenedOnceExactly();
        }

        [Test]
        public void WhenValidHeaderRequestedInLicenceUpdatedPublishedEventOptions_ThenWebhookReturns200OkResponse()
        {
            var responseHeaders = new HeaderDictionary();
            var httpContext = A.Fake<HttpContext>();

            A.CallTo(() => httpContext.Response.Headers).Returns(responseHeaders);
            A.CallTo(() => _fakeHttpContextAccessor.HttpContext).Returns(httpContext);
            A.CallTo(() => httpContext.Request.Headers["WebHook-Request-Origin"]).Returns(new[] { "test.com" });

            var result = (OkObjectResult)_fakeWebHookController.LicenceUpdatedPublishedEventReceivedOption();

            result.StatusCode.Should().Be(200);

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
              && call.GetArgument<LogLevel>(0) == LogLevel.Information
              && call.GetArgument<EventId>(1) == EventIds.LicenceUpdatedEventOptionsCallStarted.ToEventId()
              && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Started processing the Options request for the Licence updated event for webhook. | WebHook-Request-Origin : {webhookRequestOrigin}").MustHaveHappenedOnceExactly();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
              && call.GetArgument<LogLevel>(0) == LogLevel.Information
              && call.GetArgument<EventId>(1) == EventIds.LicenceUpdatedEventOptionsCallCompleted.ToEventId()
              && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Completed processing the Options request for the Licence updated event for webhook. | WebHook-Request-Origin : {webhookRequestOrigin}").MustHaveHappenedOnceExactly();

            Assert.That(responseHeaders.ContainsKey("WebHook-Allowed-Rate"), Is.True);
            Assert.That(responseHeaders["WebHook-Allowed-Rate"], Is.EqualTo("*"));
            Assert.That(responseHeaders.ContainsKey("WebHook-Allowed-Origin"), Is.True);
            Assert.That(responseHeaders["WebHook-Allowed-Origin"], Is.EqualTo("test.com"));
        }

        [Test]
        public async Task WhenValidEventInLicenceUpdatedPublishedEventReceived_ThenWebhookReturns200OkResponse()
        {
            var fakeLicenceUpdatedEventJson = JObject.Parse(@"{""data"":{""correlationId"":""123""}}");

            A.CallTo(() => _fakeSapClient.PostEventData(A<XmlDocument>.Ignored, A<string>.Ignored, A<string>.Ignored, A<string>.Ignored, A<string>.Ignored))
                .Returns(new HttpResponseMessage()
                {
                    StatusCode = HttpStatusCode.OK
                });

            var result = (OkObjectResult)await _fakeWebHookController.LicenceUpdatedPublishedEventReceived(fakeLicenceUpdatedEventJson);

            A.CallTo(() => _fakeAzureTableReaderWriter.UpsertEntityAsync(A<ITableEntity>.Ignored)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _fakeAzureBlobReaderWriter.UploadEventAsync(A<string>.Ignored, A<string>.Ignored, A<string>.Ignored)).MustHaveHappenedTwiceExactly();

            result.StatusCode.Should().Be(200);

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
                                                && call.GetArgument<LogLevel>(0) == LogLevel.Information
                                                && call.GetArgument<EventId>(1) == EventIds.LicenceUpdatedEventPublishedEventReceived.ToEventId()
                                                && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "ERP Facade webhook has received new licence updated publish event from EES.").MustHaveHappenedOnceExactly();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
             && call.GetArgument<LogLevel>(0) == LogLevel.Information
             && call.GetArgument<EventId>(1) == EventIds.StoreLicenceUpdatedPublishedEventInAzureTable.ToEventId()
             && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Storing the received Licence updated published event in azure table.").MustHaveHappenedOnceExactly();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
             && call.GetArgument<LogLevel>(0) == LogLevel.Information
             && call.GetArgument<EventId>(1) == EventIds.UploadLicenceUpdatedPublishedEventInAzureBlob.ToEventId()
             && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Uploading the received Licence updated  published event in blob storage.").MustHaveHappenedOnceExactly();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
             && call.GetArgument<LogLevel>(0) == LogLevel.Information
             && call.GetArgument<EventId>(1) == EventIds.UploadedLicenceUpdatedPublishedEventInAzureBlob.ToEventId()
             && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Licence updated  published event is uploaded in blob storage successfully.").MustHaveHappenedOnceExactly();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
                                                && call.GetArgument<LogLevel>(0) == LogLevel.Information
                                                && call.GetArgument<EventId>(1) == EventIds.UploadLicenceUpdatedSapXmlPayloadInAzureBlob.ToEventId()
                                                && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Uploading the SAP xml payload for licence updated event in blob storage.").MustHaveHappenedOnceExactly();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
                                                && call.GetArgument<LogLevel>(0) == LogLevel.Information
                                                && call.GetArgument<EventId>(1) == EventIds.UploadedLicenceUpdatedSapXmlPayloadInAzureBlob.ToEventId()
                                                && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "SAP xml payload for licence updated event is uploaded in blob storage successfully.").MustHaveHappenedOnceExactly();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
                                                && call.GetArgument<LogLevel>(0) == LogLevel.Information
                                                && call.GetArgument<EventId>(1) == EventIds.LicenceUpdatedPublishedEventUpdatePushedToSap.ToEventId()
                                                && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "The licence updated event data has been sent to SAP successfully. | {StatusCode}").MustHaveHappenedOnceExactly();
        }

        [Test]
        public async Task WhenCorrelationIdIsMissingInLicenceUpdatedPublishedEvent_ThenWebhookReturns400BadRequestResponse()
        {
            var fakeLicenceUpdatedEventJson = JObject.Parse(@"{""data"":{""corId"":""123""}}");

            var result = (BadRequestObjectResult)await _fakeWebHookController.LicenceUpdatedPublishedEventReceived(fakeLicenceUpdatedEventJson);

            result.StatusCode.Should().Be(400);

            A.CallTo(() => _fakeAzureTableReaderWriter.UpsertEntityAsync(A<ITableEntity>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => _fakeAzureBlobReaderWriter.UploadEventAsync(A<string>.Ignored, A<string>.Ignored, A<string>.Ignored)).MustNotHaveHappened();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
                                                && call.GetArgument<LogLevel>(0) == LogLevel.Information
                                                && call.GetArgument<EventId>(1) == EventIds.LicenceUpdatedEventPublishedEventReceived.ToEventId()
                                                && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "ERP Facade webhook has received new licence updated publish event from EES.").MustHaveHappenedOnceExactly();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
                                                && call.GetArgument<LogLevel>(0) == LogLevel.Warning
                                                && call.GetArgument<EventId>(1) == EventIds.CorrelationIdMissingInLicenceUpdatedEvent.ToEventId()
                                                && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "CorrelationId is missing in Licence updated published event.").MustHaveHappenedOnceExactly();
        }

        [Test]
        public void WhenSapDoesNotRespond200Ok_ThenLicenceUpdatedWebhookReturns500InternalServerResponse()
        {
            XmlDocument xmlDocument = new();

            var fakeLicenceUpdatedEventJson = JObject.Parse(@"{""data"":{""correlationId"":""123""}}");

            A.CallTo(() =>
                    _fakeLicenceUpdatedSapMessageBuilder.BuildLicenceUpdatedSapMessageXml(
                        A<LicenceUpdatedEventPayLoad>.Ignored, A<string>.Ignored))
                .Returns(xmlDocument);

            A.CallTo(() => _fakeSapClient.PostEventData(A<XmlDocument>.Ignored, A<string>.Ignored, A<string>.Ignored, A<string>.Ignored, A<string>.Ignored))
                .Returns(new HttpResponseMessage()
                {
                    StatusCode = HttpStatusCode.Unauthorized
                });

            Assert.ThrowsAsync<ERPFacadeException>(() => _fakeWebHookController.LicenceUpdatedPublishedEventReceived(fakeLicenceUpdatedEventJson));

            A.CallTo(() => _fakeAzureTableReaderWriter.UpsertEntityAsync(A<ITableEntity>.Ignored)).MustHaveHappened();
            A.CallTo(() => _fakeAzureBlobReaderWriter.UploadEventAsync(A<string>.Ignored, A<string>.Ignored, A<string>.Ignored)).MustHaveHappened();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
               && call.GetArgument<LogLevel>(0) == LogLevel.Information
               && call.GetArgument<EventId>(1) == EventIds.LicenceUpdatedEventPublishedEventReceived.ToEventId()
               && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "ERP Facade webhook has received new licence updated publish event from EES.").MustHaveHappenedOnceExactly();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
             && call.GetArgument<LogLevel>(0) == LogLevel.Information
             && call.GetArgument<EventId>(1) == EventIds.StoreLicenceUpdatedPublishedEventInAzureTable.ToEventId()
             && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Storing the received Licence updated published event in azure table.").MustHaveHappenedOnceExactly();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
             && call.GetArgument<LogLevel>(0) == LogLevel.Information
             && call.GetArgument<EventId>(1) == EventIds.UploadLicenceUpdatedPublishedEventInAzureBlob.ToEventId()
             && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Uploading the received Licence updated  published event in blob storage.").MustHaveHappenedOnceExactly();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
             && call.GetArgument<LogLevel>(0) == LogLevel.Information
             && call.GetArgument<EventId>(1) == EventIds.UploadedLicenceUpdatedPublishedEventInAzureBlob.ToEventId()
             && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Licence updated  published event is uploaded in blob storage successfully.").MustHaveHappenedOnceExactly();
        }
    }
}
