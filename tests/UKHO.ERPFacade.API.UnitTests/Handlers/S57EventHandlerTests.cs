﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Xml;
using Azure.Data.Tables;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using NUnit.Framework;
using UKHO.ERPFacade.API.Handlers;
using UKHO.ERPFacade.API.UnitTests.Common;
using UKHO.ERPFacade.API.XmlTransformers;
using UKHO.ERPFacade.Common.Configuration;
using UKHO.ERPFacade.Common.Constants;
using UKHO.ERPFacade.Common.Enums;
using UKHO.ERPFacade.Common.Exceptions;
using UKHO.ERPFacade.Common.HttpClients;
using UKHO.ERPFacade.Common.Logging;
using UKHO.ERPFacade.Common.Models.CloudEvents;
using UKHO.ERPFacade.Common.Models.CloudEvents.S57Event;
using UKHO.ERPFacade.Common.Operations.IO.Azure;

namespace UKHO.ERPFacade.API.UnitTests.Handlers
{
    [TestFixture]
    public class S57EventHandlerTests
    {
        private ILogger<S57EventHandler> _fakeLogger;
        private IBaseXmlTransformer _fakeBaseXmlTransformer;
        private IAzureTableReaderWriter _fakeAzureTableReaderWriter;
        private IAzureBlobReaderWriter _fakeAzureBlobReaderWriter;
        private ISapClient _fakeSapClient;
        private IOptions<SapConfiguration> _fakeSapConfig;
        private S57EventHandler _fakeS57EventHandler;
        private IOptions<AioConfiguration> _fakeAioConfig;

        [SetUp]
        public void SetUp()
        {
            _fakeLogger = A.Fake<ILogger<S57EventHandler>>();
            _fakeBaseXmlTransformer = A.Fake<IBaseXmlTransformer>();
            _fakeAzureTableReaderWriter = A.Fake<IAzureTableReaderWriter>();
            _fakeAzureBlobReaderWriter = A.Fake<IAzureBlobReaderWriter>();
            _fakeSapClient = A.Fake<ISapClient>();
            _fakeAioConfig = A.Fake<IOptions<AioConfiguration>>();
            _fakeSapConfig = A.Fake<IOptions<SapConfiguration>>();
            _fakeAioConfig.Value.AioCells = "GB800001,GB800002";
            _fakeS57EventHandler = new S57EventHandler(_fakeBaseXmlTransformer,
                                                       _fakeLogger,
                                                       _fakeAzureTableReaderWriter,
                                                       _fakeAzureBlobReaderWriter,
                                                       _fakeSapClient,
                                                       _fakeSapConfig,
                                                       _fakeAioConfig);
        }

        [Test]
        public void WhenS57EventHandler_ProcessedTheEvent_ThenCheckAllRequiredMessagesLoggedAsExpected()
        {
            var newCellEventPayloadJson = TestHelper.ReadFileData("ERPTestData\\NewCell.JSON");
            var eventData = JsonConvert.DeserializeObject<BaseCloudEvent>(newCellEventPayloadJson);
            _ = _fakeS57EventHandler.ProcessEventAsync(eventData);
            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
                                              && call.GetArgument<LogLevel>(0) == LogLevel.Information
                                              && call.GetArgument<EventId>(1) == EventIds.S57EventProcessingStarted.ToEventId()
                                              && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "S57 enccontentpublished event processing started.").MustHaveHappenedOnceExactly();

            A.CallTo(() => _fakeAzureTableReaderWriter.UpsertEntityAsync(A<ITableEntity>.Ignored)).MustHaveHappened();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
                                                && call.GetArgument<LogLevel>(0) == LogLevel.Information
                                                && call.GetArgument<EventId>(1) == EventIds.S57EventEntryAddedInAzureTable.ToEventId()
                                                && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "S57 enccontentpublished event entry added in azure table.").MustHaveHappenedOnceExactly();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
                                                && call.GetArgument<LogLevel>(0) == LogLevel.Information
                                                && call.GetArgument<EventId>(1) == EventIds.S57EventJsonStoredInAzureBlobContainer.ToEventId()
                                                && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "S57 enccontentpublished event json payload is stored in azure blob container.").MustHaveHappenedOnceExactly();

            A.CallTo(() => _fakeBaseXmlTransformer.BuildXmlPayload(A<S57EventData>.Ignored, A<string>.Ignored)).Returns(new XmlDocument());

            A.CallTo(() => _fakeAzureBlobReaderWriter.UploadEventAsync(A<string>.Ignored, A<string>.Ignored, A<string>.Ignored)).MustHaveHappened(2, Times.Exactly);

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
                                                && call.GetArgument<LogLevel>(0) == LogLevel.Information
                                                && call.GetArgument<EventId>(1) == EventIds.S57EventJsonStoredInAzureBlobContainer.ToEventId()
                                                && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "S57 enccontentpublished event xml payload is stored in azure blob container.").MustHaveHappenedOnceExactly();
            A.CallTo(() => _fakeSapClient.PostEventData(A<XmlDocument>.Ignored, A<string>.Ignored, A<string>.Ignored, A<string>.Ignored, A<string>.Ignored)).Returns(new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK
            });

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
                                                && call.GetArgument<LogLevel>(0) == LogLevel.Information
                                                && call.GetArgument<EventId>(1) == EventIds.S57EventUpdateSentToSap.ToEventId()
                                                && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "S57 ENC update has been sent to SAP successfully.").MustHaveHappenedOnceExactly();

            A.CallTo(() => _fakeAzureTableReaderWriter.UpdateEntityAsync(A<string>.Ignored, A<string>.Ignored, A<Dictionary<string, object>>.That.Matches(d => d.ContainsKey("RequestDateTime") && (DateTime)d["RequestDateTime"] <= DateTime.UtcNow)))
           .MustHaveHappenedOnceExactly();

            A.CallTo(() => _fakeAzureTableReaderWriter.UpdateEntityAsync(A<string>.Ignored, A<string>.Ignored, A<Dictionary<string, object>>.That.Matches(d => d.ContainsKey("Status") && (string)d["Status"] == Status.Complete.ToString())))
            .MustHaveHappenedOnceExactly();
        }

        [Test]
        public void WhenS57EventHandler_NotProcessedTheEvent()
        {
            XmlDocument xmlDocument = new();
            var newCellEventPayloadJson = TestHelper.ReadFileData("ERPTestData\\NewCell.JSON");
            var eventData = JsonConvert.DeserializeObject<BaseCloudEvent>(newCellEventPayloadJson);
            A.CallTo(() => _fakeBaseXmlTransformer.BuildXmlPayload(A<BaseCloudEvent>.Ignored, XmlTemplateInfo.S57SapXmlTemplatePath)).Returns(xmlDocument);
            A.CallTo(() => _fakeSapClient.PostEventData(A<XmlDocument>.Ignored, A<string>.Ignored, A<string>.Ignored, A<string>.Ignored, A<string>.Ignored)).Returns(new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.Unauthorized,
            });
            Assert.ThrowsAsync<ERPFacadeException>(() => _fakeS57EventHandler.ProcessEventAsync(eventData))
                .Message.Should().Be("An error occurred while sending S57 ENC update to SAP. | Unauthorized");
        }

        [Test]
        public void WhenS57EventHandler_NotProcessedForAioCells()
        {
            var newCellEventPayloadJson = TestHelper.ReadFileData("ERPTestData\\NewAIOCell.JSON");
            var eventData = JsonConvert.DeserializeObject<BaseCloudEvent>(newCellEventPayloadJson);

            _ = _fakeS57EventHandler.ProcessEventAsync(eventData);

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
                                                && call.GetArgument<LogLevel>(0) == LogLevel.Information
                                                && call.GetArgument<EventId>(1) == EventIds.S57EventNotProcessedForAioCells.ToEventId()
                                                && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "S57 enccontentpublished event is specific to AIO cells and, as a result, it is not processed.").MustHaveHappenedOnceExactly();
        }

        [Test]
        public void WhenS57EventHandler_AioCellConfigurationIsMissing_ThenThrowERPFacadeException()
        {
            _fakeAioConfig.Value.AioCells = string.Empty;

            Assert.Throws<ERPFacadeException>(() => new S57EventHandler(_fakeBaseXmlTransformer,
                    _fakeLogger,
                    _fakeAzureTableReaderWriter,
                    _fakeAzureBlobReaderWriter,
                    _fakeSapClient,
                    _fakeSapConfig,
                    _fakeAioConfig))
                .Message.Should().Be("AIO cell configuration missing.");
        }
    }
}
