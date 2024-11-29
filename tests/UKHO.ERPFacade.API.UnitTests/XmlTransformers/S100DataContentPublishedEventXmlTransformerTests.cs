using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using NUnit.Framework;
using UKHO.ERPFacade.API.UnitTests.Common;
using UKHO.ERPFacade.API.XmlTransformers;
using UKHO.ERPFacade.Common.Constants;
using UKHO.ERPFacade.Common.Exceptions;
using UKHO.ERPFacade.Common.Logging;
using UKHO.ERPFacade.Common.Models.CloudEvents;
using UKHO.ERPFacade.Common.Models.CloudEvents.S100Event;
using UKHO.ERPFacade.Common.Models.SapActionConfigurationModels;
using UKHO.ERPFacade.Common.Operations;

namespace UKHO.ERPFacade.API.UnitTests.XmlTransformers
{
    [TestFixture]
    public class S100DataContentPublishedEventXmlTransformerTests
    {
        private ILogger<S100DataContentPublishedEventXmlTransformer> _fakeLogger;
        private IXmlOperations _fakeXmlOperations;
        private IOptions<S100DataContentPublishedEventSapActionConfiguration> _fakeSapActionConfig;
        private S100DataContentPublishedEventXmlTransformer _fakeS100DataContentPublishedEventXmlTransformer;
        private string _sapXmlTemplate;

        [SetUp]
        public void Setup()
        {
            _fakeLogger = A.Fake<ILogger<S100DataContentPublishedEventXmlTransformer>>();
            _fakeXmlOperations = A.Fake<IXmlOperations>();
            _fakeSapActionConfig = Options.Create(InitConfiguration().GetSection("S100DataContentPublishedEventSapActionConfiguration").Get<S100DataContentPublishedEventSapActionConfiguration>())!;
            _fakeS100DataContentPublishedEventXmlTransformer = new S100DataContentPublishedEventXmlTransformer(_fakeLogger, _fakeXmlOperations, _fakeSapActionConfig);
            _sapXmlTemplate = TestHelper.ReadFileData(XmlTemplateInfo.S100SapXmlTemplatePath);
        }

        private IConfiguration InitConfiguration()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory() + @"/ConfigurationFiles")
                .AddJsonFile("S100DataContentPublishedEventSapActionConfiguration.json")
                .AddEnvironmentVariables()
                .Build();

            return config;
        }

        [Test]
        public void WhenSapXmlTemplateFileNotExist_ThenThrowERPFacadeException()
        {
            var newProductEventPayloadJson = TestHelper.ReadFileData("ERPTestData\\S100TestData\\NewProduct.JSON");
            var eventData = JsonConvert.DeserializeObject<S100Event>(newProductEventPayloadJson);

            A.CallTo(() => _fakeXmlOperations.CreateXmlDocument(A<string>.Ignored)).Throws(new ERPFacadeException(EventIds.SapXmlTemplateNotFoundException.ToEventId(), "The SAP XML payload template does not exist."));

            Assert.Throws<ERPFacadeException>(() => _fakeS100DataContentPublishedEventXmlTransformer.BuildXmlPayload(eventData!, _sapXmlTemplate))
                .Message.Should().Be("The SAP XML payload template does not exist.");
        }

        [Test]
        public void WhenBuildXmlPayloadIsCalledForNewProductScenario_ThenReturnXMLDocument()
        {
            var newProductEventPayloadJson = TestHelper.ReadFileData("ERPTestData\\S100TestData\\NewProduct.JSON");
            var baseCloudEvent = JsonConvert.DeserializeObject<BaseCloudEvent>(newProductEventPayloadJson);
            S100EventData s100EventData = JsonConvert.DeserializeObject<S100EventData>(baseCloudEvent.Data.ToString()!);

            XmlDocument soapXml = new();
            soapXml.LoadXml(_sapXmlTemplate);

            A.CallTo(() => _fakeXmlOperations.CreateXmlDocument(A<string>.Ignored)).Returns(soapXml);
            var result = _fakeS100DataContentPublishedEventXmlTransformer.BuildXmlPayload(s100EventData, _sapXmlTemplate);

            result.Should().BeOfType<XmlDocument>();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
                                                && call.GetArgument<LogLevel>(0) == LogLevel.Information
                                                && call.GetArgument<EventId>(1) == EventIds.S100EventSapXmlPayloadGenerationStarted.ToEventId()
                                                && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Generation of SAP xml payload for S-100 data content published event started.").MustHaveHappenedOnceExactly();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
                                                && call.GetArgument<LogLevel>(0) == LogLevel.Information
                                                && call.GetArgument<EventId>(1) == EventIds.S100SapActionGenerationStarted.ToEventId()
                                                && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Generation of {ActionName} action started.").MustHaveHappened(3, Times.Exactly);

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
                                                && call.GetArgument<LogLevel>(0) == LogLevel.Information
                                                && call.GetArgument<EventId>(1) == EventIds.S100SapActionGenerationCompleted.ToEventId()
                                                && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Generation of {ActionName} action completed.").MustHaveHappened(3, Times.Exactly);

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
                                                && call.GetArgument<LogLevel>(0) == LogLevel.Information
                                                && call.GetArgument<EventId>(1) == EventIds.S100EventSapXmlPayloadGenerationCompleted.ToEventId()
                                                && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Generation of SAP xml payload for S-100 data content published event completed.").MustHaveHappenedOnceExactly();
        }

        [Test]
        public void WhenBuildXmlPayloadIfRequiredAttributesNotProvided_ThenThrowERPFacadeException()
        {
            var newProductEventPayloadJson = TestHelper.ReadFileData("ERPTestData\\S100TestData\\NewProductWithoutProducingAgencyAttributes.JSON");
            var baseCloudEvent = JsonConvert.DeserializeObject<BaseCloudEvent>(newProductEventPayloadJson);
            S100EventData s100EventData = JsonConvert.DeserializeObject<S100EventData>(baseCloudEvent.Data.ToString()!);

            XmlDocument soapXml = new();
            soapXml.LoadXml(_sapXmlTemplate);

            A.CallTo(() => _fakeXmlOperations.CreateXmlDocument(A<string>.Ignored)).Returns(soapXml);

            Assert.Throws<ERPFacadeException>(() => _fakeS100DataContentPublishedEventXmlTransformer.BuildXmlPayload(s100EventData, _sapXmlTemplate))
                .Message.Should().Be("Error while generating SAP action information. | Action : CREATE PRODUCT | XML Attribute : AGENCY | ErrorMessage : Object reference not set to an instance of an object.");
        }

        [Test]
        public void WhenBuildXmlPayloadIsCalledForChangeProductScenario_ThenReturnXMLDocument()
        {
            var newProductEventPayloadJson = TestHelper.ReadFileData("ERPTestData\\S100TestData\\ChangeProduct.JSON");
            var baseCloudEvent = JsonConvert.DeserializeObject<BaseCloudEvent>(newProductEventPayloadJson);
            S100EventData s100EventData = JsonConvert.DeserializeObject<S100EventData>(baseCloudEvent.Data.ToString()!);

            XmlDocument soapXml = new();
            soapXml.LoadXml(_sapXmlTemplate);

            A.CallTo(() => _fakeXmlOperations.CreateXmlDocument(A<string>.Ignored)).Returns(soapXml);
            var result = _fakeS100DataContentPublishedEventXmlTransformer.BuildXmlPayload(s100EventData, _sapXmlTemplate);

            result.Should().BeOfType<XmlDocument>();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
                                                && call.GetArgument<LogLevel>(0) == LogLevel.Information
                                                && call.GetArgument<EventId>(1) == EventIds.S100EventSapXmlPayloadGenerationStarted.ToEventId()
                                                && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Generation of SAP xml payload for S-100 data content published event started.").MustHaveHappenedOnceExactly();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
                                                && call.GetArgument<LogLevel>(0) == LogLevel.Information
                                                && call.GetArgument<EventId>(1) == EventIds.S100SapActionGenerationStarted.ToEventId()
                                                && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Generation of {ActionName} action started.").MustHaveHappened(2, Times.Exactly);

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
                                                && call.GetArgument<LogLevel>(0) == LogLevel.Information
                                                && call.GetArgument<EventId>(1) == EventIds.S100SapActionGenerationCompleted.ToEventId()
                                                && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Generation of {ActionName} action completed.").MustHaveHappened(2, Times.Exactly);

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
                                                && call.GetArgument<LogLevel>(0) == LogLevel.Information
                                                && call.GetArgument<EventId>(1) == EventIds.S100EventSapXmlPayloadGenerationCompleted.ToEventId()
                                                && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Generation of SAP xml payload for S-100 data content published event completed.").MustHaveHappenedOnceExactly();
        }

        [Test]
        public void WhenBuildXmlPayloadIsCalledForCancelProductsAndReplacedWithProductScenario_ThenReturnXMLDocument()
        {
            var newProductEventPayloadJson = TestHelper.ReadFileData("ERPTestData\\S100TestData\\CancellationAndReplacement.JSON");
            var baseCloudEvent = JsonConvert.DeserializeObject<BaseCloudEvent>(newProductEventPayloadJson);
            S100EventData s100EventData = JsonConvert.DeserializeObject<S100EventData>(baseCloudEvent.Data.ToString()!);

            XmlDocument soapXml = new();
            soapXml.LoadXml(_sapXmlTemplate);

            A.CallTo(() => _fakeXmlOperations.CreateXmlDocument(A<string>.Ignored)).Returns(soapXml);
            var result = _fakeS100DataContentPublishedEventXmlTransformer.BuildXmlPayload(s100EventData, _sapXmlTemplate);

            result.Should().BeOfType<XmlDocument>();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
                                                && call.GetArgument<LogLevel>(0) == LogLevel.Information
                                                && call.GetArgument<EventId>(1) == EventIds.S100EventSapXmlPayloadGenerationStarted.ToEventId()
                                                && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Generation of SAP xml payload for S-100 data content published event started.").MustHaveHappenedOnceExactly();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
                                                && call.GetArgument<LogLevel>(0) == LogLevel.Information
                                                && call.GetArgument<EventId>(1) == EventIds.S100SapActionGenerationStarted.ToEventId()
                                                && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Generation of {ActionName} action started.").MustHaveHappened(11, Times.Exactly);

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
                                                && call.GetArgument<LogLevel>(0) == LogLevel.Information
                                                && call.GetArgument<EventId>(1) == EventIds.S100SapActionGenerationCompleted.ToEventId()
                                                && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Generation of {ActionName} action completed.").MustHaveHappened(11, Times.Exactly);

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
                                                && call.GetArgument<LogLevel>(0) == LogLevel.Information
                                                && call.GetArgument<EventId>(1) == EventIds.S100EventSapXmlPayloadGenerationCompleted.ToEventId()
                                                && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Generation of SAP xml payload for S-100 data content published event completed.").MustHaveHappenedOnceExactly();
        }
    }
}
