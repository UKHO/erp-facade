using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;
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
using UKHO.ERPFacade.Common.Models;
using UKHO.ERPFacade.Common.Models.CloudEvents;
using UKHO.ERPFacade.Common.Models.CloudEvents.S57Event;
using UKHO.ERPFacade.Common.Models.SapActionConfigurationModels;
using UKHO.ERPFacade.Common.Operations;
using UKHO.ERPFacade.Common.PermitDecryption;
using UKHO.ERPFacade.Common.Providers;

namespace UKHO.ERPFacade.API.UnitTests.XmlTransformers
{
    [TestFixture]
    public class S57EncContentPublishedEventXmlTransformerTests
    {
        private ILogger<S57EncContentPublishedEventXmlTransformer> _fakeLogger;
        private IXmlOperations _fakeXmlOperations;
        private IOptions<S57EncContentPublishedEventSapActionConfiguration> _fakeS57EncContentPublishedEventSapActionConfig;
        private IWeekDetailsProvider _fakeWeekDetailsProvider;
        private IPermitDecryption _fakePermitDecryption;
        private S57EncContentPublishedEventXmlTransformer _fakeS57EncContentPublishedEventXmlTransformer;
        private string _sapXmlTemplate;


        [SetUp]
        public void Setup()
        {
            _fakeLogger = A.Fake<ILogger<S57EncContentPublishedEventXmlTransformer>>();
            _fakeXmlOperations = A.Fake<IXmlOperations>();
            _fakeWeekDetailsProvider = A.Fake<IWeekDetailsProvider>();
            _fakePermitDecryption = A.Fake<IPermitDecryption>();
            _fakeS57EncContentPublishedEventSapActionConfig = Options.Create(InitConfiguration().GetSection("S57EncContentPublishedEventSapActionConfiguration").Get<S57EncContentPublishedEventSapActionConfiguration>())!;
            _fakeS57EncContentPublishedEventXmlTransformer = new S57EncContentPublishedEventXmlTransformer(_fakeLogger, _fakeXmlOperations, _fakeWeekDetailsProvider, _fakePermitDecryption, _fakeS57EncContentPublishedEventSapActionConfig);
            _sapXmlTemplate = TestHelper.ReadFileData(XmlTemplateInfo.S57SapXmlTemplatePath);
        }

        private IConfiguration InitConfiguration()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory() + @"/ConfigurationFiles")
                .AddJsonFile("S57EncContentPublishedEventSapActionConfiguration.json")
                .AddEnvironmentVariables()
                .Build();

            return config;
        }

        [Test]
        public void WhenSapXmlTemplateFileNotExist_ThenThrowERPFacadeException()
        {
            var newCellEventPayloadJson = TestHelper.ReadFileData("ERPTestData\\NewCell.JSON");
            var eventData = JsonConvert.DeserializeObject<S57Event>(newCellEventPayloadJson);

            A.CallTo(() => _fakeXmlOperations.CreateXmlDocument(A<string>.Ignored)).Throws(new ERPFacadeException(EventIds.SapXmlTemplateNotFoundException.ToEventId(), "The SAP XML payload template does not exist."));

            Assert.Throws<ERPFacadeException>((Action)(() => _fakeS57EncContentPublishedEventXmlTransformer.BuildXmlPayload(eventData!, _sapXmlTemplate)))
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

            var result = _fakeS57EncContentPublishedEventXmlTransformer.BuildXmlPayload(s57EventData, _sapXmlTemplate);

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

            var result = _fakeS57EncContentPublishedEventXmlTransformer.BuildXmlPayload(s57EventData, _sapXmlTemplate);

            result.Should().BeOfType<XmlDocument>();

            XElement xElement = XElement.Parse(result.OuterXml);
            var itemList = xElement.Descendants("item").ToList();

            Assert.That(itemList.Count > 0, Is.True);
            Assert.That(string.IsNullOrEmpty(itemList[0].Descendants("CANCELLED").FirstOrDefault().Value), Is.True);
            Assert.That(itemList[0].Descendants("AGENCY").FirstOrDefault().Value.Length == 2, Is.True);
            Assert.That(itemList[0].Descendants().ToList().All(item => item.Value.Length <= 250), Is.True);

            A.CallTo(() => _fakeXmlOperations.AppendChildNode(A<XmlElement>.Ignored, A<XmlDocument>.Ignored, A<string>.Ignored, A<string>.Ignored)).MustHaveHappened(20, Times.Exactly);

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

            var result = _fakeS57EncContentPublishedEventXmlTransformer.BuildXmlPayload(s57EventData, _sapXmlTemplate);

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

            var result = _fakeS57EncContentPublishedEventXmlTransformer.BuildXmlPayload(s57EventData, _sapXmlTemplate);

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
        public void WhenBuildSapMessageXmlIfRequiredAttributesNotProvided_ThenThrowERPFacadeException()
        {
            var newCellEventPayloadJson = TestHelper.ReadFileData("ERPTestData\\NewCellWithoutProviderCodeAttributes.JSON");
            var baseCloudEvent = JsonConvert.DeserializeObject<BaseCloudEvent>(newCellEventPayloadJson);
            S57EventData s57EventData = JsonConvert.DeserializeObject<S57EventData>(baseCloudEvent.Data.ToString()!);

            XmlDocument soapXml = new();
            soapXml.LoadXml(_sapXmlTemplate);

            A.CallTo(() => _fakeXmlOperations.CreateXmlDocument(A<string>.Ignored)).Returns(soapXml);

            Assert.Throws<ERPFacadeException>((Action)(() => _fakeS57EncContentPublishedEventXmlTransformer.BuildXmlPayload(s57EventData, _sapXmlTemplate)))
                .Message.Should().Be("Error while generating SAP action information. | Action : CREATE ENC CELL | XML Attribute : PROVIDER | ErrorMessage : Object reference not set to an instance of an object.");
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

            Assert.Throws<ERPFacadeException>((Action)(() => _fakeS57EncContentPublishedEventXmlTransformer.BuildXmlPayload(s57EventData, _sapXmlTemplate)))
                .Message.Should().Be("Error while generating SAP action information. | Action : CREATE ENC CELL | XML Attribute : VALIDFROM | ErrorMessage : Exception of type 'System.Exception' was thrown.");
        }

        [Test]
        public void WhenUnitOfSaleIsNullWhileReplacingEncCell_ThenReturnsNull()
        {
            var cancelCellWithNewCellReplacementPayloadJson = TestHelper.ReadFileData("ERPTestData\\CancelCellWithNewCellReplacement.JSON");
            var baseCloudEvent = JsonConvert.DeserializeObject<BaseCloudEvent>(cancelCellWithNewCellReplacementPayloadJson);
            S57EventData s57EventData = JsonConvert.DeserializeObject<S57EventData>(baseCloudEvent.Data.ToString()!);
            var action = _fakeS57EncContentPublishedEventSapActionConfig.Value.Actions.FirstOrDefault(x => x.Product == XmlFields.EncCell && x.ActionName == ConfigFileFields.ReplaceEncCellAction);

            MethodInfo buildAction = typeof(S57EncContentPublishedEventXmlTransformer).GetMethod("GetUnitOfSale", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance)!;
            var result = (XmlElement)buildAction.Invoke(_fakeS57EncContentPublishedEventXmlTransformer, new object[] { action.ActionNumber, s57EventData.UnitsOfSales!, s57EventData.Products.FirstOrDefault()! })!;

            result.Should().BeNull();
        }

        [Test]
        public void WhenUnitOfSaleIsNullWhileChangingEncCell_ThenReturnsNull()
        {
            var cancelCellWithNewCellReplacementPayloadJson = TestHelper.ReadFileData("ERPTestData\\CancelCellWithNewCellReplacement.JSON");
            var baseCloudEvent = JsonConvert.DeserializeObject<BaseCloudEvent>(cancelCellWithNewCellReplacementPayloadJson);
            S57EventData s57EventData = JsonConvert.DeserializeObject<S57EventData>(baseCloudEvent.Data.ToString()!);

            var action = _fakeS57EncContentPublishedEventSapActionConfig.Value.Actions.FirstOrDefault(x => x.Product == XmlFields.EncCell && x.ActionName == ConfigFileFields.ChangeEncCellAction);

            MethodInfo buildAction = typeof(S57EncContentPublishedEventXmlTransformer).GetMethod("GetUnitOfSale", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance)!;
            var result = (XmlElement)buildAction.Invoke(_fakeS57EncContentPublishedEventXmlTransformer, new object[] { action.ActionNumber, s57EventData.UnitsOfSales!, s57EventData.Products.LastOrDefault()! })!;

            result.Should().BeNull();
        }

        [Test]
        public void WhenTransformationFailsWithNullProductField_ThenLogDetailedErrorInformation()
        {
            var newCellEventPayloadJson = TestHelper.ReadFileData("ERPTestData\\NewCellWithoutProviderCodeAttributes.JSON");
            var baseCloudEvent = JsonConvert.DeserializeObject<BaseCloudEvent>(newCellEventPayloadJson);
            S57EventData s57EventData = JsonConvert.DeserializeObject<S57EventData>(baseCloudEvent.Data.ToString()!);

            XmlDocument soapXml = new();
            soapXml.LoadXml(_sapXmlTemplate);

            A.CallTo(() => _fakeXmlOperations.CreateXmlDocument(A<string>.Ignored)).Returns(soapXml);

            Assert.Throws<ERPFacadeException>((Action)(() => _fakeS57EncContentPublishedEventXmlTransformer.BuildXmlPayload(s57EventData, _sapXmlTemplate)))
                .Message.Should().Be("Error while generating SAP action information. | Action : CREATE ENC CELL | XML Attribute : PROVIDER | ErrorMessage : Object reference not set to an instance of an object.");

            // Verify detailed error logging was called
            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
                                                && call.GetArgument<LogLevel>(0) == LogLevel.Error
                                                && call.GetArgument<EventId>(1) == EventIds.S57XmlTransformationDetailedFailure.ToEventId()
                                                && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString()!.StartsWith("S57 XML transformation failed.")).MustHaveHappenedOnceExactly();
        }

        [Test]
        public void WhenTransformationFailsForUnitOfSale_ThenLogContainsUnitDetails()
        {
            // Arrange
            XmlDocument soapXml = new();
            soapXml.LoadXml(_sapXmlTemplate);

            var attribute = new Attributes
            {
                IsRequired = true,
                Section = ConfigFileFields.UnitOfSaleSection,
                JsonPropertyName = "NonExistentProperty",
                XmlNodeName = "PROVIDER",
                SortingOrder = 1
            };

            var unit = new S57UnitOfSale
            {
                UnitName = "UNIT1",
                CompositionChanges = new S57CompositionChanges
                {
                    AddProducts = new List<string> { "ADD1" },
                    RemoveProducts = new List<string> { "REM1" }
                }
            };

            var actionAttributes = new List<(int, XmlElement)>();

            // Act
            MethodInfo processAttributes = typeof(S57EncContentPublishedEventXmlTransformer).GetMethod("ProcessAttributes", BindingFlags.NonPublic | BindingFlags.Instance)!;

            Assert.That((Func<object?>)(() => processAttributes.Invoke(_fakeS57EncContentPublishedEventXmlTransformer, new object[] { "ACTION", new List<Attributes> { attribute }, soapXml, unit, actionAttributes, null, null, null })), Throws.TypeOf<TargetInvocationException>().With.InnerException.TypeOf<ERPFacadeException>());

            // Assert - verify detailed error log contains all expected properties
            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
                && call.GetArgument<LogLevel>(0) == LogLevel.Error
                && call.GetArgument<EventId>(1) == EventIds.S57XmlTransformationDetailedFailure.ToEventId()
                && ((call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["Action"] ?? string.Empty).ToString() == SanitizeForLogLocal("ACTION"))
                && ((call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["XmlAttribute"] ?? string.Empty).ToString() == SanitizeForLogLocal("PROVIDER"))
                && ((call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["SourceType"] ?? string.Empty).ToString() == SanitizeForLogLocal("S57UnitOfSale"))
                && ((call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["ProductName"] ?? string.Empty).ToString() == string.Empty)
                && ((call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["DataSetName"] ?? string.Empty).ToString() == string.Empty)
                && ((call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["InUnitsOfSale"] ?? string.Empty).ToString() == string.Empty)
                && ((call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["jsonProperty"] ?? string.Empty).ToString() == SanitizeForLogLocal("NonExistentProperty"))
                && ((call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["UnitName"] ?? string.Empty).ToString() == SanitizeForLogLocal("UNIT1"))
                && ((call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["CompositionAdd"] ?? string.Empty).ToString() == SanitizeForLogLocal(new List<string>{ "ADD1" }))
                && ((call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["CompositionRemove"] ?? string.Empty).ToString() == SanitizeForLogLocal(new List<string>{ "REM1" }))
                && ((call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["ChildCell"] ?? string.Empty).ToString() == "N/A")
                && ((call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["ReplacedBy"] ?? string.Empty).ToString() == string.Empty)
                && (((call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["ErrorMessage"] ?? string.Empty).ToString().Length > 0)
                    && ((call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["ErrorMessage"] ?? string.Empty).ToString().IndexOf('\n') == -1)
                    && ((call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["ErrorMessage"] ?? string.Empty).ToString().IndexOf('\r') == -1))
            ).MustHaveHappenedOnceExactly();
        }

        [Test]
        public void WhenTransformationFailsWithNullSource_ThenLogContainsSourceIsNullAndSourceTypeNull()
        {
            // Arrange
            XmlDocument soapXml = new();
            soapXml.LoadXml(_sapXmlTemplate);

            var attribute = new Attributes
            {
                IsRequired = true,
                Section = ConfigFileFields.UnitOfSaleSection,
                JsonPropertyName = "Any.Property",
                XmlNodeName = "PROVIDER",
                SortingOrder = 1
            };

            var actionAttributes = new List<(int, XmlElement)>();

            // Act
            MethodInfo processAttributes = typeof(S57EncContentPublishedEventXmlTransformer).GetMethod("ProcessAttributes", BindingFlags.NonPublic | BindingFlags.Instance)!;

            Assert.That((Func<object?>)(() => processAttributes.Invoke(_fakeS57EncContentPublishedEventXmlTransformer, new object[] { "ACTION", new List<Attributes> { attribute }, soapXml, null, actionAttributes, null, null, null })), Throws.TypeOf<TargetInvocationException>().With.InnerException.TypeOf<ERPFacadeException>());

            // Assert - verify detailed error log contains all expected properties for null source
            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
                && call.GetArgument<LogLevel>(0) == LogLevel.Error
                && call.GetArgument<EventId>(1) == EventIds.S57XmlTransformationDetailedFailure.ToEventId()
                && ((call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["Action"] ?? string.Empty).ToString() == SanitizeForLogLocal("ACTION"))
                && ((call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["XmlAttribute"] ?? string.Empty).ToString() == SanitizeForLogLocal("PROVIDER"))
                && ((call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["SourceType"] ?? string.Empty).ToString() == SanitizeForLogLocal("null"))
                && ((call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["ProductName"] ?? string.Empty).ToString() == string.Empty)
                && ((call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["DataSetName"] ?? string.Empty).ToString() == string.Empty)
                && ((call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["InUnitsOfSale"] ?? string.Empty).ToString() == string.Empty)
                && ((call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["jsonProperty"] ?? string.Empty).ToString() == SanitizeForLogLocal("Any.Property"))
                && ((call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["UnitName"] ?? string.Empty).ToString() == SanitizeForLogLocal((string?)null))
                && ((call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["CompositionAdd"] ?? string.Empty).ToString() == SanitizeForLogLocal((IEnumerable<string>?)null))
                && ((call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["CompositionRemove"] ?? string.Empty).ToString() == SanitizeForLogLocal((IEnumerable<string>?)null))
                && ((call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["ChildCell"] ?? string.Empty).ToString() == "N/A")
                && ((call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["ReplacedBy"] ?? string.Empty).ToString() == string.Empty)
                && (((call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["ErrorMessage"] ?? string.Empty).ToString().Length > 0)
                    && ((call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["ErrorMessage"] ?? string.Empty).ToString().IndexOf('\n') == -1)
                    && ((call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["ErrorMessage"] ?? string.Empty).ToString().IndexOf('\r') == -1))
            ).MustHaveHappenedOnceExactly();
        }

        private static string SanitizeForLogLocal(string? value)
        {
            return string.IsNullOrEmpty(value)
                ? string.Empty
                : value.Replace("\r", string.Empty).Replace("\n", string.Empty);
        }

        private static string SanitizeForLogLocal(IEnumerable<string>? values)
        {
            if (values == null) return string.Empty;
            return SanitizeForLogLocal(string.Join(",", values));
        }
    }
}
