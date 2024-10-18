using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Xml;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using UKHO.ERPFacade.API.Helpers;
using UKHO.ERPFacade.API.Services;
using UKHO.ERPFacade.API.UnitTests.Common;
using UKHO.ERPFacade.Common.Configuration;
using UKHO.ERPFacade.Common.Exceptions;
using UKHO.ERPFacade.Common.HttpClients;
using UKHO.ERPFacade.Common.IO.Azure;
using UKHO.ERPFacade.Common.Logging;
using UKHO.ERPFacade.Common.Models;
using UKHO.ERPFacade.Common.Models.TableEntities;

namespace UKHO.ERPFacade.API.UnitTests.Services
{
    [TestFixture]
    public class S57ServiceTests
    {
        private ILogger<S57Service> _fakeLogger;
        private IAzureTableReaderWriter _fakeAzureTableReaderWriter;
        private IAzureBlobEventWriter _fakeAzureBlobEventWriter;
        private IAzureQueueHelper _fakeAzureQueueHelper;
        private ISapClient _fakeSapClient;
        private IEncContentSapMessageBuilder _fakeEncContentSapMessageBuilder;
        private IOptions<SapConfiguration> _fakeSapConfig;
        private IOptions<AioConfiguration> _fakeAioConfig;
        private S57Service _fakeNewEncContentPublishedEventReceived;

        [SetUp]
        public void Setup()
        {
            _fakeLogger = A.Fake<ILogger<S57Service>>();
            _fakeAioConfig = A.Fake<IOptions<AioConfiguration>>();
            _fakeAzureTableReaderWriter = A.Fake<IAzureTableReaderWriter>();
            _fakeAzureBlobEventWriter = A.Fake<IAzureBlobEventWriter>();
            _fakeAzureQueueHelper = A.Fake<IAzureQueueHelper>();
            _fakeSapClient = A.Fake<ISapClient>();
            _fakeEncContentSapMessageBuilder = A.Fake<IEncContentSapMessageBuilder>();
            _fakeSapConfig = Options.Create(new SapConfiguration()
            {
                SapServiceOperationForEncEvent = "Z_ADDS_MAT_INFO"
            });

            _fakeAioConfig.Value.AioCells = "GB800001,GB800002";

            _fakeNewEncContentPublishedEventReceived = new S57Service(_fakeLogger, _fakeAzureTableReaderWriter, _fakeAzureBlobEventWriter, _fakeAzureQueueHelper, _fakeSapClient, _fakeEncContentSapMessageBuilder, _fakeSapConfig, _fakeAioConfig);
        }

        [Test]
        public void WhenSapDoesNotRespond200Ok_ThenReturn500InternalServerResponse()
        {
            XmlDocument xmlDocument = new();

            var newCellEventPayloadJson = TestHelper.ReadFileData("ERPTestData\\NewCell.JSON");
            var fakeEncEventJson = JObject.Parse(newCellEventPayloadJson);

            A.CallTo(() => _fakeEncContentSapMessageBuilder.BuildSapMessageXml(A<EncEventPayload>.Ignored)).Returns(xmlDocument);
            A.CallTo(() => _fakeSapClient.PostEventData(A<XmlDocument>.Ignored, A<string>.Ignored, A<string>.Ignored, A<string>.Ignored, A<string>.Ignored))
                .Returns(new HttpResponseMessage()
                {
                    StatusCode = HttpStatusCode.Unauthorized
                });

            Assert.ThrowsAsync<ERPFacadeException>(() => _fakeNewEncContentPublishedEventReceived.ProcessS57Event(fakeEncEventJson))
                 .Message.Should().Be("An error occurred while sending a request to SAP. | Unauthorized");

            A.CallTo(() => _fakeAzureTableReaderWriter.UpsertEntity(A<string>.Ignored, A<string>.Ignored, A<EncEventEntity>.Ignored)).MustHaveHappened();
            A.CallTo(() => _fakeAzureBlobEventWriter.UploadEvent(A<string>.Ignored, A<string>.Ignored, A<string>.Ignored)).MustHaveHappened();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
             && call.GetArgument<LogLevel>(0) == LogLevel.Information
             && call.GetArgument<EventId>(1) == EventIds.AddingEntryForEncContentPublishedEventInAzureTable.ToEventId()
             && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Adding/Updating entry for enccontentpublished event in azure table.").MustHaveHappenedOnceExactly();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
             && call.GetArgument<LogLevel>(0) == LogLevel.Information
             && call.GetArgument<EventId>(1) == EventIds.UploadEncContentPublishedEventInAzureBlobStarted.ToEventId()
             && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Uploading enccontentpublished event payload in blob storage.").MustHaveHappenedOnceExactly();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
             && call.GetArgument<LogLevel>(0) == LogLevel.Information
             && call.GetArgument<EventId>(1) == EventIds.UploadEncContentPublishedEventInAzureBlobCompleted.ToEventId()
             && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "The enccontentpublished event payload is uploaded in blob storage successfully.").MustHaveHappenedOnceExactly();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
             && call.GetArgument<LogLevel>(0) == LogLevel.Information
             && call.GetArgument<EventId>(1) == EventIds.UploadSapXmlPayloadInAzureBlobStarted.ToEventId()
             && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Uploading the SAP XML payload in blob storage.").MustHaveHappenedOnceExactly();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
             && call.GetArgument<LogLevel>(0) == LogLevel.Information
             && call.GetArgument<EventId>(1) == EventIds.UploadSapXmlPayloadInAzureBlobCompleted.ToEventId()
             && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "SAP XML payload is uploaded in blob storage successfully.").MustHaveHappenedOnceExactly();
        }

        [Test]
        public void WhenSapDoesResponds200Ok_ThenCheckAllRequiredMessagesLoggedAsExpected()
        {
            XmlDocument xmlDocument = new();

            var newCellEventPayloadJson = TestHelper.ReadFileData("ERPTestData\\NewCell.JSON");
            var fakeEncEventJson = JObject.Parse(newCellEventPayloadJson);

            A.CallTo(() => _fakeEncContentSapMessageBuilder.BuildSapMessageXml(A<EncEventPayload>.Ignored)).Returns(xmlDocument);
            A.CallTo(() => _fakeSapClient.PostEventData(A<XmlDocument>.Ignored, A<string>.Ignored, A<string>.Ignored, A<string>.Ignored, A<string>.Ignored))
                .Returns(new HttpResponseMessage()
                {
                    StatusCode = HttpStatusCode.OK
                });

            _ = _fakeNewEncContentPublishedEventReceived.ProcessS57Event(fakeEncEventJson);

            A.CallTo(() => _fakeAzureTableReaderWriter.UpdateEntity(A<string>.Ignored, A<string>.Ignored, A<KeyValuePair<string, DateTime>[]>.Ignored)).MustHaveHappened();
            A.CallTo(() => _fakeAzureBlobEventWriter.UploadEvent(A<string>.Ignored, A<string>.Ignored, A<string>.Ignored)).MustHaveHappened();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
             && call.GetArgument<LogLevel>(0) == LogLevel.Information
             && call.GetArgument<EventId>(1) == EventIds.AddingEntryForEncContentPublishedEventInAzureTable.ToEventId()
             && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Adding/Updating entry for enccontentpublished event in azure table.").MustHaveHappenedOnceExactly();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
             && call.GetArgument<LogLevel>(0) == LogLevel.Information
             && call.GetArgument<EventId>(1) == EventIds.UploadEncContentPublishedEventInAzureBlobStarted.ToEventId()
             && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Uploading enccontentpublished event payload in blob storage.").MustHaveHappenedOnceExactly();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
             && call.GetArgument<LogLevel>(0) == LogLevel.Information
             && call.GetArgument<EventId>(1) == EventIds.UploadEncContentPublishedEventInAzureBlobCompleted.ToEventId()
             && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "The enccontentpublished event payload is uploaded in blob storage successfully.").MustHaveHappenedOnceExactly();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
             && call.GetArgument<LogLevel>(0) == LogLevel.Information
             && call.GetArgument<EventId>(1) == EventIds.UploadSapXmlPayloadInAzureBlobStarted.ToEventId()
             && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Uploading the SAP XML payload in blob storage.").MustHaveHappenedOnceExactly();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
             && call.GetArgument<LogLevel>(0) == LogLevel.Information
             && call.GetArgument<EventId>(1) == EventIds.UploadSapXmlPayloadInAzureBlobCompleted.ToEventId()
             && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "SAP XML payload is uploaded in blob storage successfully.").MustHaveHappenedOnceExactly();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
           && call.GetArgument<LogLevel>(0) == LogLevel.Information
           && call.GetArgument<EventId>(1) == EventIds.EncUpdateSentToSap.ToEventId()
           && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "ENC update has been sent to SAP successfully").MustHaveHappenedOnceExactly();
        }

        [Test]
        public void WhenPayloadHasAioCells_ThenReturn200OkResponse()
        {
            var newCellEventPayloadJson = TestHelper.ReadFileData("ERPTestData\\NewAIOCell.JSON");
            var fakeEncEventJson = JObject.Parse(newCellEventPayloadJson);

            _ = _fakeNewEncContentPublishedEventReceived.ProcessS57Event(fakeEncEventJson);

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
             && call.GetArgument<LogLevel>(0) == LogLevel.Information
             && call.GetArgument<EventId>(1) == EventIds.NoProcessingOfNewEncContentPublishedEventForAioCells.ToEventId()
             && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "The enccontentpublished event will not be processed for Aio cells.").MustHaveHappenedOnceExactly();
        }

        [Test]
        public void WhenBuildSapMessageXmlIsCalledWithoutAIOCellConfiguration_ThenThrowERPFacadeException()
        {
            _fakeAioConfig.Value.AioCells = string.Empty;

            Assert.Throws<ERPFacadeException>(() => new S57Service(_fakeLogger, _fakeAzureTableReaderWriter, _fakeAzureBlobEventWriter, _fakeAzureQueueHelper, _fakeSapClient, _fakeEncContentSapMessageBuilder, _fakeSapConfig, _fakeAioConfig))
           .Message.Should().Be("Aio cell configuration not found.");
        }
    }
}
