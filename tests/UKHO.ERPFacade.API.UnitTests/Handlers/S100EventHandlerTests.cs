﻿using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Azure.Data.Tables;
using FakeItEasy;
using Microsoft.AspNetCore.Mvc.Diagnostics;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using UKHO.ERPFacade.API.Handlers;
using UKHO.ERPFacade.API.XmlTransformers;
using UKHO.ERPFacade.Common.IO.Azure;
using UKHO.ERPFacade.Common.Logging;
using UKHO.ERPFacade.Common.Models.CloudEvents;

namespace UKHO.ERPFacade.API.UnitTests.Handlers
{
    [TestFixture]
    public class S100EventHandlerTests
    {
        private ILogger<S100EventHandler> _fakeLogger;
        private IAzureTableHelper _fakeAzureTableHelper;
        private IAzureBlobHelper _fakeAzureBlobHelper;
        private S100EventHandler _fakes100EventHandler;
        private IBaseXmlTransformer _fakeBaseXmlTransformer;

        [SetUp]
        public void Setup()
        {
            _fakeLogger = A.Fake<ILogger<S100EventHandler>>();
            _fakeBaseXmlTransformer = A.Fake<IBaseXmlTransformer>();
            _fakeAzureTableHelper = A.Fake<IAzureTableHelper>();
            _fakeAzureBlobHelper = A.Fake<IAzureBlobHelper>();
            _fakes100EventHandler = new S100EventHandler(_fakeBaseXmlTransformer, _fakeLogger, _fakeAzureTableHelper, _fakeAzureBlobHelper);
        }

        [Test]
        public void WhenValidS100EventHandlerEventReceived_ThenProcessingS100Event()
        {
            var fakeS100EventDataJson = JObject.Parse(@"{""data"":{""correlationId"":""123""}}");
            var fakeS100EventData = JsonConvert.DeserializeObject<BaseCloudEvent>(fakeS100EventDataJson.ToString());
            _ = _fakes100EventHandler.ProcessEventAsync(fakeS100EventData);
            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
                                                && call.GetArgument<LogLevel>(0) == LogLevel.Information
                                                && call.GetArgument<EventId>(1) == EventIds.S100EventProcessingStarted.ToEventId()
                                                && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "S100 data content published event processing started.").MustHaveHappenedOnceExactly();

            A.CallTo(() => _fakeAzureTableHelper.UpsertEntity(A<ITableEntity>.Ignored)).MustHaveHappened();
            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
                                                && call.GetArgument<LogLevel>(0) == LogLevel.Information
                                                && call.GetArgument<EventId>(1) == EventIds.S100EventEntryAddedInAzureTable.ToEventId()
                                                && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "S100 data content published event entry added in azure table.").MustHaveHappenedOnceExactly();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
                                                && call.GetArgument<LogLevel>(0) == LogLevel.Information
                                                && call.GetArgument<EventId>(1) == EventIds.S100EventJsonStoredInAzureBlobContainer.ToEventId()
                                                && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "S100 data content published event json payload is stored in azure blob container.").MustHaveHappenedOnceExactly();

            A.CallTo(() => _fakeBaseXmlTransformer.BuildXmlPayload(A<EventData>.Ignored, A<string>.Ignored)).Returns(new XmlDocument());
            A.CallTo(() => _fakeAzureBlobHelper.UploadEvent(A<string>.Ignored, A<string>.Ignored, A<string>.Ignored)).MustHaveHappened(2, Times.Exactly);
            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
                                                && call.GetArgument<LogLevel>(0) == LogLevel.Information
                                                && call.GetArgument<EventId>(1) == EventIds.S100EventXMLStoredInAzureBlobContainer.ToEventId()
                                                && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "S100 data content published event xml payload is stored in azure blob container.").MustHaveHappenedOnceExactly();

        }
    }
}
