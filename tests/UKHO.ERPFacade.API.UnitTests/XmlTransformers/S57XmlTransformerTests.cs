﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
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
using UKHO.ERPFacade.Common.Models;
using UKHO.ERPFacade.Common.Models.CloudEvents;
using UKHO.ERPFacade.Common.Models.CloudEvents.S57Event;
using UKHO.ERPFacade.Common.Operations;
using UKHO.ERPFacade.Common.PermitDecryption;
using UKHO.ERPFacade.Common.Providers;

namespace UKHO.ERPFacade.API.UnitTests.XmlTransformers
{
    [TestFixture]
    public class S57XmlTransformerTests
    {
        private ILogger<S57XmlTransformer> _fakeLogger;
        private IXmlOperations _fakeXmlOperations;
        private IOptions<SapActionConfiguration> _fakeSapActionConfig;
        private IWeekDetailsProvider _fakeWeekDetailsProvider;
        private IPermitDecryption _fakePermitDecryption;
        private S57XmlTransformer _fakeS57XmlTransformer;
        private string _sapXmlTemplate;


        [SetUp]
        public void Setup()
        {
            _fakeLogger = A.Fake<ILogger<S57XmlTransformer>>();
            _fakeXmlOperations = A.Fake<IXmlOperations>();
            _fakeWeekDetailsProvider = A.Fake<IWeekDetailsProvider>();
            _fakePermitDecryption = A.Fake<IPermitDecryption>();
            _fakeSapActionConfig = Options.Create(InitConfiguration().GetSection("SapActionConfiguration").Get<SapActionConfiguration>())!;
            _fakeS57XmlTransformer = new S57XmlTransformer(_fakeLogger, _fakeXmlOperations, _fakeWeekDetailsProvider, _fakePermitDecryption, _fakeSapActionConfig);
            _sapXmlTemplate = TestHelper.ReadFileData(XmlTemplateInfo.S57SapXmlTemplatePath);
        }

        private IConfiguration InitConfiguration()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory() + @"/ConfigurationFiles")
                .AddJsonFile("S57SapActions.json")
                .AddEnvironmentVariables()
                .Build();

            return config;
        }

        [Test]
        public void WhenSapXmlTemplateFileNotExist_ThenThrowERPFacadeException()
        {
            var newCellEventPayloadJson = TestHelper.ReadFileData("ERPTestData\\NewCell.JSON");
            var eventData = JsonConvert.DeserializeObject<S57Event>(newCellEventPayloadJson);

            A.CallTo(() => _fakeXmlOperations.CreateXmlDocument(A<string>.Ignored)).Throws(new ERPFacadeException(EventIds.SapXmlTemplateNotFound.ToEventId(), "The SAP XML payload template does not exist."));

            Assert.Throws<ERPFacadeException>(() => _fakeS57XmlTransformer.BuildXmlPayload(eventData!, _sapXmlTemplate))
                .Message.Should().Be("The SAP XML payload template does not exist.");
        }

        [Test]
        public void WhenBuildSapMessageXmlIsCalledWithWithCancelCellWithExistingCellReplacementScenario_ThenReturnXMLDocument()
        {
            var cancelReplaceCellEventPayloadJson = TestHelper.ReadFileData("ERPTestData\\CancelCellWithExistingCellReplacement.JSON");
            var baseCloudEvent = JsonConvert.DeserializeObject<BaseCloudEvent>(cancelReplaceCellEventPayloadJson);
            S57EventData s57EventData = JsonConvert.DeserializeObject<S57EventData>(baseCloudEvent.Data.ToString()!);

            XmlDocument soapXml = new();
            soapXml.LoadXml(_sapXmlTemplate);

            A.CallTo(() => _fakeXmlOperations.CreateXmlDocument(A<string>.Ignored)).Returns(soapXml);
            A.CallTo(() => _fakeWeekDetailsProvider.GetDateOfWeek(A<int>.Ignored, A<int>.Ignored, A<bool>.Ignored)).Returns("20240808");

            var result = _fakeS57XmlTransformer.BuildXmlPayload(s57EventData, _sapXmlTemplate);

            result.Should().BeOfType<XmlDocument>();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
            && call.GetArgument<LogLevel>(0) == LogLevel.Information
            && call.GetArgument<EventId>(1) == EventIds.S57EventSapXmlPayloadGenerationStarted.ToEventId()
            && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Generation of SAP xml payload for S57 enccontentpublished event started.").MustHaveHappenedOnceExactly();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
                                                && call.GetArgument<LogLevel>(0) == LogLevel.Information
                                                && call.GetArgument<EventId>(1) == EventIds.S57SapActionGenerationStarted.ToEventId()
                                                && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Generation of {ActionName} action started.").MustHaveHappened(7, Times.Exactly);

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
            && call.GetArgument<LogLevel>(0) == LogLevel.Information
            && call.GetArgument<EventId>(1) == EventIds.S57SapActionGenerationCompleted.ToEventId()
            && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Generation of {ActionName} action completed").MustHaveHappened(7, Times.Exactly);

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
             && call.GetArgument<LogLevel>(0) == LogLevel.Information
             && call.GetArgument<EventId>(1) == EventIds.S57EventSapXmlPayloadGenerationCompleted.ToEventId()
             && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Generation of SAP xml payload for S57 enccontentpublished event completed.").MustHaveHappenedOnceExactly();
        }

        [Test]
        public void WhenBuildSapMessageXmlIsCalledWithNewCellScenario_ThenReturnXMLDocument()
        {
            var newCellEventPayloadJson = TestHelper.ReadFileData("ERPTestData\\NewCell.JSON");
            var baseCloudEvent = JsonConvert.DeserializeObject<BaseCloudEvent>(newCellEventPayloadJson);
            S57EventData s57EventData = JsonConvert.DeserializeObject<S57EventData>(baseCloudEvent.Data.ToString()!);

            var permitKeys = new DecryptedPermit { ActiveKey = "firstkey", NextKey = "nextkey" };


            XmlDocument soapXml = new();
            soapXml.LoadXml(_sapXmlTemplate);

            A.CallTo(() => _fakeXmlOperations.CreateXmlDocument(A<string>.Ignored)).Returns(soapXml);
            A.CallTo(() => _fakePermitDecryption.Decrypt(A<string>.Ignored)).Returns(permitKeys);
            A.CallTo(() => _fakeWeekDetailsProvider.GetDateOfWeek(A<int>.Ignored, A<int>.Ignored, A<bool>.Ignored)).Returns("20240801");

            var result = _fakeS57XmlTransformer.BuildXmlPayload(s57EventData, _sapXmlTemplate);

            result.Should().BeOfType<XmlDocument>();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
                                                && call.GetArgument<LogLevel>(0) == LogLevel.Information
                                                && call.GetArgument<EventId>(1) == EventIds.S57EventSapXmlPayloadGenerationStarted.ToEventId()
                                                && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Generation of SAP xml payload for S57 enccontentpublished event started.").MustHaveHappenedOnceExactly();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
                                                && call.GetArgument<LogLevel>(0) == LogLevel.Information
                                                && call.GetArgument<EventId>(1) == EventIds.S57SapActionGenerationStarted.ToEventId()
                                                && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Generation of {ActionName} action started.").MustHaveHappened(4, Times.Exactly);

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
                                                && call.GetArgument<LogLevel>(0) == LogLevel.Information
                                                && call.GetArgument<EventId>(1) == EventIds.S57SapActionGenerationCompleted.ToEventId()
                                                && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Generation of {ActionName} action completed").MustHaveHappened(4, Times.Exactly);

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
                                                && call.GetArgument<LogLevel>(0) == LogLevel.Information
                                                && call.GetArgument<EventId>(1) == EventIds.S57EventSapXmlPayloadGenerationCompleted.ToEventId()
                                                && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Generation of SAP xml payload for S57 enccontentpublished event completed.").MustHaveHappenedOnceExactly();

        }

        [Test]
        public void WhenBuildSapMessageXmlIsCalledWithUpdateCellScenario_ThenReturnXMLDocument()
        {
            var updateCellEventPayloadJson = TestHelper.ReadFileData("ERPTestData\\UpdateCell.JSON");
            var baseCloudEvent = JsonConvert.DeserializeObject<BaseCloudEvent>(updateCellEventPayloadJson);
            S57EventData s57EventData = JsonConvert.DeserializeObject<S57EventData>(baseCloudEvent.Data.ToString()!);

            var permitKeys = new DecryptedPermit { ActiveKey = "firstkey", NextKey = "nextkey" };

            XmlDocument soapXml = new();
            soapXml.LoadXml(_sapXmlTemplate);

            A.CallTo(() => _fakeXmlOperations.CreateXmlDocument(A<string>.Ignored)).Returns(soapXml);
            A.CallTo(() => _fakePermitDecryption.Decrypt(A<string>.Ignored)).Returns(permitKeys);
            A.CallTo(() => _fakeWeekDetailsProvider.GetDateOfWeek(A<int>.Ignored, A<int>.Ignored, A<bool>.Ignored)).Returns("20240801");

            var result = _fakeS57XmlTransformer.BuildXmlPayload(s57EventData, _sapXmlTemplate);

            result.Should().BeOfType<XmlDocument>();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
                                                && call.GetArgument<LogLevel>(0) == LogLevel.Information
                                                && call.GetArgument<EventId>(1) == EventIds.S57EventSapXmlPayloadGenerationStarted.ToEventId()
                                                && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Generation of SAP xml payload for S57 enccontentpublished event started.").MustHaveHappenedOnceExactly();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
                                                && call.GetArgument<LogLevel>(0) == LogLevel.Information
                                                && call.GetArgument<EventId>(1) == EventIds.S57SapActionGenerationStarted.ToEventId()
                                                && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Generation of {ActionName} action started.").MustHaveHappened(3, Times.Exactly);

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
                                                && call.GetArgument<LogLevel>(0) == LogLevel.Information
                                                && call.GetArgument<EventId>(1) == EventIds.S57SapActionGenerationCompleted.ToEventId()
                                                && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Generation of {ActionName} action completed").MustHaveHappened(3, Times.Exactly);

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
                                                && call.GetArgument<LogLevel>(0) == LogLevel.Information
                                                && call.GetArgument<EventId>(1) == EventIds.S57EventSapXmlPayloadGenerationCompleted.ToEventId()
                                                && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Generation of SAP xml payload for S57 enccontentpublished event completed.").MustHaveHappenedOnceExactly();

        }

        [Test]
        public void WhenBuildSapMessageXmlIsCalledWithAdditionalCoverageWithNewEditionScenario_ThenReturnXMLDocument()
        {
            var additionalCoverageEventPayloadJson = TestHelper.ReadFileData("ERPTestData\\AdditionalCoverageWithNewEdition.JSON");
            var baseCloudEvent = JsonConvert.DeserializeObject<BaseCloudEvent>(additionalCoverageEventPayloadJson);
            S57EventData s57EventData = JsonConvert.DeserializeObject<S57EventData>(baseCloudEvent.Data.ToString()!);

            var permitKeys = new DecryptedPermit { ActiveKey = "firstkey", NextKey = "nextkey" };

            XmlDocument soapXml = new();
            soapXml.LoadXml(_sapXmlTemplate);

            A.CallTo(() => _fakeXmlOperations.CreateXmlDocument(A<string>.Ignored)).Returns(soapXml);
            A.CallTo(() => _fakePermitDecryption.Decrypt(A<string>.Ignored)).Returns(permitKeys);
            A.CallTo(() => _fakeWeekDetailsProvider.GetDateOfWeek(A<int>.Ignored, A<int>.Ignored, A<bool>.Ignored)).Returns("20240801");

            var result = _fakeS57XmlTransformer.BuildXmlPayload(s57EventData, _sapXmlTemplate);

            result.Should().BeOfType<XmlDocument>();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
                                                && call.GetArgument<LogLevel>(0) == LogLevel.Information
                                                && call.GetArgument<EventId>(1) == EventIds.S57EventSapXmlPayloadGenerationStarted.ToEventId()
                                                && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Generation of SAP xml payload for S57 enccontentpublished event started.").MustHaveHappenedOnceExactly();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
                                                && call.GetArgument<LogLevel>(0) == LogLevel.Information
                                                && call.GetArgument<EventId>(1) == EventIds.S57SapActionGenerationStarted.ToEventId()
                                                && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Generation of {ActionName} action started.").MustHaveHappened(4, Times.Exactly);

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
                                                && call.GetArgument<LogLevel>(0) == LogLevel.Information
                                                && call.GetArgument<EventId>(1) == EventIds.S57SapActionGenerationCompleted.ToEventId()
                                                && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Generation of {ActionName} action completed").MustHaveHappened(4, Times.Exactly);

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
                                                && call.GetArgument<LogLevel>(0) == LogLevel.Information
                                                && call.GetArgument<EventId>(1) == EventIds.S57EventSapXmlPayloadGenerationCompleted.ToEventId()
                                                && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Generation of SAP xml payload for S57 enccontentpublished event completed.").MustHaveHappenedOnceExactly();
        }

        [Test]
        public void WhenBuildSapMessageXmlIsCalledWithNewCellWithNoUnitOfSaleHavingTypeIsUnit_ThenThrowERPFacadeException()
        {
            var newCellEventPayloadJson = TestHelper.ReadFileData("ERPTestData\\NewCellWithNoUnitOfSaleHavingTypeIsUnit.JSON");
            var baseCloudEvent = JsonConvert.DeserializeObject<BaseCloudEvent>(newCellEventPayloadJson);
            S57EventData s57EventData = JsonConvert.DeserializeObject<S57EventData>(baseCloudEvent.Data.ToString()!);

            XmlDocument soapXml = new();
            soapXml.LoadXml(_sapXmlTemplate);

            A.CallTo(() => _fakeXmlOperations.CreateXmlDocument(A<string>.Ignored)).Returns(soapXml);

            Assert.Throws<ERPFacadeException>(() => _fakeS57XmlTransformer.BuildXmlPayload(s57EventData, _sapXmlTemplate))
                .Message.Should().Be("Required unit not found in S57 enccontentpublished event for US5AK9DI to generate CREATE ENC CELL action.");
        }

        [Test]
        public void WhenBuildSapMessageXmlIsCalledWithReplaceCellWithNoUnitOfSaleHavingTypeIsUnit_ThenThrowERPFacadeException()
        {
            var newCellEventPayloadJson = TestHelper.ReadFileData("ERPTestData\\ReplaceCellWithNoUnitOfSaleHavingTypeIsUnit.JSON");
            var baseCloudEvent = JsonConvert.DeserializeObject<BaseCloudEvent>(newCellEventPayloadJson);
            S57EventData s57EventData = JsonConvert.DeserializeObject<S57EventData>(baseCloudEvent.Data.ToString()!);

            XmlDocument soapXml = new();
            soapXml.LoadXml(_sapXmlTemplate);

            A.CallTo(() => _fakeXmlOperations.CreateXmlDocument(A<string>.Ignored)).Returns(soapXml);

            Assert.Throws<ERPFacadeException>(() => _fakeS57XmlTransformer.BuildXmlPayload(s57EventData, _sapXmlTemplate))
            .Message.Should().Be("Required unit not found in S57 enccontentpublished event for GB50382B to generate REPLACED WITH ENC CELL action.");
        }

        [Test]
        public void WhenBuildSapMessageXmlIfRequiredAttributesNotProvided_ThenThrowERPFacadeException()
        {
            var newCellEventPayloadJson = TestHelper.ReadFileData("ERPTestData\\NewCellWithoutProviderCodeAttributes.JSON");
            var baseCloudEvent = JsonConvert.DeserializeObject<BaseCloudEvent>(newCellEventPayloadJson);
            S57EventData s57EventData = JsonConvert.DeserializeObject<S57EventData>(baseCloudEvent.Data.ToString()!);

            XmlDocument soapXml = new();
            soapXml.LoadXml(_sapXmlTemplate);

            A.CallTo(() => _fakeXmlOperations.CreateXmlDocument(A<string>.Ignored)).Returns(soapXml);

            Assert.Throws<ERPFacadeException>(() => _fakeS57XmlTransformer.BuildXmlPayload(s57EventData, _sapXmlTemplate))
                .Message.Should().Be("Error while generating SAP action information. | Action : CREATE ENC CELL | XML Attribute : PROVIDER | ErrorMessage : Object reference not set to an instance of an object.");
        }

        [Test]
        public void WhenBuildSapMessageXmlIfUkhoWeekNumberSectionNotProvided_ThenThrowERPFacadeException()
        {
            var newCellEventPayloadJson = TestHelper.ReadFileData("ERPTestData\\NewCellWithoutUkhoWeekNumberSection.JSON");
            var baseCloudEvent = JsonConvert.DeserializeObject<BaseCloudEvent>(newCellEventPayloadJson);
            S57EventData s57EventData = JsonConvert.DeserializeObject<S57EventData>(baseCloudEvent.Data.ToString()!);

            var permitKeys = new DecryptedPermit { ActiveKey = "firstkey", NextKey = "nextkey" };

            XmlDocument soapXml = new();
            soapXml.LoadXml(_sapXmlTemplate);

            A.CallTo(() => _fakeXmlOperations.CreateXmlDocument(A<string>.Ignored)).Returns(soapXml);
            A.CallTo(() => _fakePermitDecryption.Decrypt(A<string>.Ignored)).Returns(permitKeys);

            Assert.Throws<ERPFacadeException>(() => _fakeS57XmlTransformer.BuildXmlPayload(s57EventData, _sapXmlTemplate))
            .Message.Should().Be("UkhoWeekNumber details not found in S57 enccontentpublished event to generate CREATE ENC CELL action.");
        }

        [Test]
        public void WhenBuildSapMessageXmlWithWrongUkhoWeekNumberDetails_ThenThrowERPFacadeException()
        {
            var newCellEventPayloadJson = TestHelper.ReadFileData("ERPTestData\\NewCellWithWrongUkhoWeekDetails.JSON");
            var baseCloudEvent = JsonConvert.DeserializeObject<BaseCloudEvent>(newCellEventPayloadJson);
            S57EventData s57EventData = JsonConvert.DeserializeObject<S57EventData>(baseCloudEvent.Data.ToString()!);

            var permitKeys = new DecryptedPermit { ActiveKey = "firstkey", NextKey = "nextkey" };

            XmlDocument soapXml = new();
            soapXml.LoadXml(_sapXmlTemplate);

            A.CallTo(() => _fakeXmlOperations.CreateXmlDocument(A<string>.Ignored)).Returns(soapXml);
            A.CallTo(() => _fakePermitDecryption.Decrypt(A<string>.Ignored)).Returns(permitKeys);
            A.CallTo(() => _fakeWeekDetailsProvider.GetDateOfWeek(A<int>.Ignored, A<int>.Ignored, A<bool>.Ignored)).Throws<System.Exception>();

            Assert.Throws<ERPFacadeException>(() => _fakeS57XmlTransformer.BuildXmlPayload(s57EventData, _sapXmlTemplate))
                .Message.Should().Be("Error while generating SAP action information. | Action : CREATE ENC CELL | XML Attribute : VALIDFROM | ErrorMessage : Exception of type 'System.Exception' was thrown.");
        }

        [Test]
        public void WhenUnitOfSaleIsNullWhileReplacingEncCell_ThenReturnsNull()
        {
            var cancelCellWithNewCellReplacementPayloadJson = TestHelper.ReadFileData("ERPTestData\\CancelCellWithNewCellReplacement.JSON");
            var baseCloudEvent = JsonConvert.DeserializeObject<BaseCloudEvent>(cancelCellWithNewCellReplacementPayloadJson);
            S57EventData s57EventData = JsonConvert.DeserializeObject<S57EventData>(baseCloudEvent.Data.ToString()!);
            var action = _fakeSapActionConfig.Value.SapActions.FirstOrDefault(x => x.Product == XmlFields.EncCell && x.Action == ConfigFileFields.ReplaceEncCellAction);

            MethodInfo buildAction = typeof(S57XmlTransformer).GetMethod("GetUnitOfSale", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance)!;
            var result = (XmlElement)buildAction.Invoke(_fakeS57XmlTransformer, new object[] { action.ActionNumber, s57EventData.UnitsOfSales!, s57EventData.Products.FirstOrDefault()! })!;

            result.Should().BeNull();
        }

        [Test]
        public void WhenUnitOfSaleIsNullWhileChangingEncCell_ThenReturnsNull()
        {
            var cancelCellWithNewCellReplacementPayloadJson = TestHelper.ReadFileData("ERPTestData\\CancelCellWithNewCellReplacement.JSON");
            var baseCloudEvent = JsonConvert.DeserializeObject<BaseCloudEvent>(cancelCellWithNewCellReplacementPayloadJson);
            S57EventData s57EventData = JsonConvert.DeserializeObject<S57EventData>(baseCloudEvent.Data.ToString()!);

            var action = _fakeSapActionConfig.Value.SapActions.FirstOrDefault(x => x.Product == XmlFields.EncCell && x.Action == ConfigFileFields.ChangeEncCellAction);

            MethodInfo buildAction = typeof(S57XmlTransformer).GetMethod("GetUnitOfSale", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance)!;
            var result = (XmlElement)buildAction.Invoke(_fakeS57XmlTransformer, new object[] { action.ActionNumber, s57EventData.UnitsOfSales!, s57EventData.Products.LastOrDefault()! })!;

            result.Should().BeNull();
        }

        [Test]
        [TestCase("ERPTestData\\NewCellWithNullYearInUkhoWeekNumberSection.JSON")]
        [TestCase("ERPTestData\\NewCellWithoutYearInUkhoWeekNumberSection.JSON")]
        [TestCase("ERPTestData\\NewCellWithNullcurrentWeekAlphaCorrectionInUkhoWeekNumberSection.JSON")]
        public void WhenProcessingUkhoWeekNumberAttributes(string jsonPayloadFile)
        {
            var cancelCellWithNewCellReplacementPayloadJson = TestHelper.ReadFileData(jsonPayloadFile);
            var baseCloudEvent = JsonConvert.DeserializeObject<BaseCloudEvent>(cancelCellWithNewCellReplacementPayloadJson);
            S57EventData s57EventData = JsonConvert.DeserializeObject<S57EventData>(baseCloudEvent.Data.ToString()!);

            var permitKeys = new DecryptedPermit { ActiveKey = "firstkey", NextKey = "nextkey" };

            XmlDocument soapXml = new();
            soapXml.LoadXml(_sapXmlTemplate);

            A.CallTo(() => _fakeXmlOperations.CreateXmlDocument(A<string>.Ignored)).Returns(soapXml);
            A.CallTo(() => _fakePermitDecryption.Decrypt(A<string>.Ignored)).Returns(permitKeys);
            A.CallTo(() => _fakeWeekDetailsProvider.GetDateOfWeek(A<int>.Ignored, A<int>.Ignored, A<bool>.Ignored)).Throws<System.Exception>();

            Assert.Throws<ERPFacadeException>(() => _fakeS57XmlTransformer.BuildXmlPayload(s57EventData, _sapXmlTemplate))
                .Message.Should().Be("Error while generating SAP action information. | Action : CREATE ENC CELL | XML Attribute : WEEKNO | ErrorMessage : Required property value is empty in enccontentpublished event payload. | Property Name : ");
        }

        [Test]
        public void WhenBuildSapMessageXmlWithEmptyPermit_ThenThrowERPFacadeException()
        {
            var newCellEventPayloadJson = TestHelper.ReadFileData("ERPTestData\\NewCellWithEmptyPermit.JSON");
            var baseCloudEvent = JsonConvert.DeserializeObject<BaseCloudEvent>(newCellEventPayloadJson);
            S57EventData s57EventData = JsonConvert.DeserializeObject<S57EventData>(baseCloudEvent.Data.ToString()!);

            XmlDocument soapXml = new();
            soapXml.LoadXml(_sapXmlTemplate);

            A.CallTo(() => _fakeXmlOperations.CreateXmlDocument(A<string>.Ignored)).Returns(soapXml);

            A.CallTo(() => _fakeWeekDetailsProvider.GetDateOfWeek(A<int>.Ignored, A<int>.Ignored, A<bool>.Ignored)).Throws<System.Exception>();
            A.CallTo(_fakePermitDecryption).Where(call => call.Method.Name == "Decrypt").MustNotHaveHappened();

            Assert.Throws<ERPFacadeException>(() => _fakeS57XmlTransformer.BuildXmlPayload(s57EventData, _sapXmlTemplate))
            .Message.Should().Be("Error while generating SAP action information. | Action : CREATE ENC CELL | XML Attribute : ACTIVEKEY | ErrorMessage : Required details are missing in enccontentpublished event payload. | Property Name : ");
        }
    }
}