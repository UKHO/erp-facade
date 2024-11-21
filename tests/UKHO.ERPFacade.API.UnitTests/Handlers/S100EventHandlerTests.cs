using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Xml;
using Azure.Data.Tables;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using UKHO.ERPFacade.API.Handlers;
using UKHO.ERPFacade.API.XmlTransformers;
using UKHO.ERPFacade.Common.Configuration;
using UKHO.ERPFacade.Common.HttpClients;
using UKHO.ERPFacade.Common.Logging;
using UKHO.ERPFacade.Common.Models.CloudEvents;
using UKHO.ERPFacade.Common.Models.CloudEvents.S100Event;
using UKHO.ERPFacade.Common.Operations.IO.Azure;
using UKHO.ERPFacade.API.UnitTests.Common;
using UKHO.ERPFacade.Common.Constants;
using UKHO.ERPFacade.Common.Exceptions;

namespace UKHO.ERPFacade.API.UnitTests.Handlers
{
    [TestFixture]
    public class S100EventHandlerTests
    {
        private ILogger<S100EventHandler> _fakeLogger;
        private IAzureTableReaderWriter _fakeAzureTableReaderWriter;
        private IAzureBlobReaderWriter _fakeAzureBlobReaderWriter;
        private S100EventHandler _fakes100EventHandler;
        private IBaseXmlTransformer _fakeBaseXmlTransformer;
        private ISapClient _fakeSapClient;
        private IOptions<SapConfiguration> _fakeSapConfig;

        [SetUp]
        public void Setup()
        {
            _fakeLogger = A.Fake<ILogger<S100EventHandler>>();
            _fakeAzureTableReaderWriter = A.Fake<IAzureTableReaderWriter>();
            _fakeAzureBlobReaderWriter = A.Fake<IAzureBlobReaderWriter>();
            _fakeBaseXmlTransformer = A.Fake<IBaseXmlTransformer>();
            _fakeSapClient = A.Fake<ISapClient>();
            _fakeSapConfig = A.Fake<IOptions<SapConfiguration>>();

            _fakes100EventHandler = new S100EventHandler(_fakeBaseXmlTransformer, _fakeLogger, _fakeAzureTableReaderWriter, _fakeAzureBlobReaderWriter, _fakeSapClient, _fakeSapConfig);
        }

        [Test]
        public void WhenValidEventReceived_ThenS100EventHandlerProcessEvent()
        {
            var fakeS100EventDataJson = JObject.Parse(@"{""data"":{""correlationId"":""123""}}");
            var fakeS100EventData = JsonConvert.DeserializeObject<BaseCloudEvent>(fakeS100EventDataJson.ToString());
            _ = _fakes100EventHandler.ProcessEventAsync(fakeS100EventData);
            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
                                                && call.GetArgument<LogLevel>(0) == LogLevel.Information
                                                && call.GetArgument<EventId>(1) == EventIds.S100EventProcessingStarted.ToEventId()
                                                && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "S-100 data content published event processing started.").MustHaveHappenedOnceExactly();

            A.CallTo(() => _fakeAzureTableReaderWriter.UpsertEntityAsync(A<ITableEntity>.Ignored)).MustHaveHappened();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
                                                && call.GetArgument<LogLevel>(0) == LogLevel.Information
                                                && call.GetArgument<EventId>(1) == EventIds.S100EventEntryAddedInAzureTable.ToEventId()
                                                && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "S-100 data content published event entry added in azure table.").MustHaveHappenedOnceExactly();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
                                                && call.GetArgument<LogLevel>(0) == LogLevel.Information
                                                && call.GetArgument<EventId>(1) == EventIds.S100EventJsonStoredInAzureBlobContainer.ToEventId()
                                                && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "S-100 data content published event json payload is stored in azure blob container.").MustHaveHappenedOnceExactly();

            A.CallTo(() => _fakeBaseXmlTransformer.BuildXmlPayload(A<S100EventData>.Ignored, A<string>.Ignored)).Returns(new XmlDocument());

            A.CallTo(() => _fakeAzureBlobReaderWriter.UploadEventAsync(A<string>.Ignored, A<string>.Ignored, A<string>.Ignored)).MustHaveHappened(2, Times.Exactly);

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
                                                && call.GetArgument<LogLevel>(0) == LogLevel.Information
                                                && call.GetArgument<EventId>(1) == EventIds.S100EventXMLStoredInAzureBlobContainer.ToEventId()
                                                && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "S-100 data content published event xml payload is stored in azure blob container.").MustHaveHappenedOnceExactly();
            A.CallTo(() => _fakeSapClient.PostEventData(A<XmlDocument>.Ignored, A<string>.Ignored, A<string>.Ignored, A<string>.Ignored, A<string>.Ignored)).Returns(new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK
            });

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
                                                && call.GetArgument<LogLevel>(0) == LogLevel.Information
                                                && call.GetArgument<EventId>(1) == EventIds.S100EventUpdateSentToSap.ToEventId()
                                                && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "S-100 data content has been sent to SAP successfully.").MustHaveHappenedOnceExactly();

            A.CallTo(() => _fakeAzureTableReaderWriter.UpdateEntityAsync(A<string>.Ignored, A<string>.Ignored, A<KeyValuePair<string, DateTime>[]>.Ignored)).MustHaveHappened();
        }

        [Test]
        public void WhenSapRespondsWith401Unauthorized_ThenS100EventHandlerRaiseAnException()
        {
            XmlDocument xmlDocument = new();
            var newCellEventPayloadJson = TestHelper.ReadFileData("ERPTestData\\S100TestData\\CancellationAndReplacement.JSON");
            var eventData = JsonConvert.DeserializeObject<BaseCloudEvent>(newCellEventPayloadJson);
            A.CallTo(() => _fakeBaseXmlTransformer.BuildXmlPayload(A<BaseCloudEvent>.Ignored, XmlTemplateInfo.S57SapXmlTemplatePath)).Returns(xmlDocument);
            A.CallTo(() => _fakeSapClient.PostEventData(A<XmlDocument>.Ignored, A<string>.Ignored, A<string>.Ignored, A<string>.Ignored, A<string>.Ignored)).Returns(new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.Unauthorized,
            });
            Assert.ThrowsAsync<ERPFacadeException>(() => _fakes100EventHandler.ProcessEventAsync(eventData))
                .Message.Should().Be("An error occurred while sending S-100 data content to SAP. | Unauthorized");

            A.CallTo(() => _fakeAzureTableReaderWriter.UpdateEntityAsync(A<string>.Ignored, A<string>.Ignored, A<KeyValuePair<string, DateTime>[]>.Ignored)).MustNotHaveHappened();
        }
    }
}
