using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using NUnit.Framework;
using UKHO.ERPFacade.API.Helpers;
using UKHO.ERPFacade.API.UnitTests.Common;
using UKHO.ERPFacade.Common.Exceptions;
using UKHO.ERPFacade.Common.IO;
using UKHO.ERPFacade.Common.Logging;
using UKHO.ERPFacade.Common.Models;
using UKHO.ERPFacade.Common.PermitDecryption;
using UKHO.ERPFacade.Common.Providers;

namespace UKHO.ERPFacade.API.UnitTests.Helpers
{
    [TestFixture]
    public class EncContentSapMessageBuilderTests
    {
        private IXmlHelper _fakeXmlHelper;
        private IFileSystemHelper _fakeFileSystemHelper;
        private IOptions<SapActionConfiguration> _fakeSapActionConfig;
        private ILogger<EncContentSapMessageBuilder> _fakeLogger;
        private IWeekDetailsProvider _fakeWeekDetailsProvider;
        private IPermitDecryption _fakePermitDecryption;
        private EncContentSapMessageBuilder _fakeEncContentSapMessageBuilder;
        private string _sapXmlTemplate;

        private const string XpathActionItems = $"//*[local-name()='ACTIONITEMS']";
        private const string EncCell = "ENC CELL";
        private const string XpathProductName = $"//*[local-name()='PRODUCTNAME']";
        private const string XpathCorrection = $"//*[local-name()='CORRECTION']";
        private const string XpathWeekNo = $"//*[local-name()='WEEKNO']";
        private const string XpathValidFrom = $"//*[local-name()='VALIDFROM']";
        private const string XpathActiveKey = $"//*[local-name()='ACTIVEKEY']";
        private const string XpathNextKey = $"//*[local-name()='NEXTKEY']";
        private const string XpathChildCell = $"//*[local-name()='CHILDCELL']";
        private const string XpathReplacedBy = $"//*[local-name()='REPLACEDBY']";
        private const string XpathActionNumber = $"//*[local-name()='ACTIONNUMBER']";
        private const string XpathAction = $"//*[local-name()='ACTION']";
        private const string ReplaceEncCellAction = "REPLACED WITH ENC CELL";
        private const string ChangeEncCellAction = "CHANGE ENC CELL";

        [SetUp]
        public void Setup()
        {
            _fakeLogger = A.Fake<ILogger<EncContentSapMessageBuilder>>();
            _fakeXmlHelper = A.Fake<IXmlHelper>();
            _fakeFileSystemHelper = A.Fake<IFileSystemHelper>();
            _fakeSapActionConfig = Options.Create(InitConfiguration().GetSection("SapActionConfiguration").Get<SapActionConfiguration>())!;
            _fakeWeekDetailsProvider = A.Fake<IWeekDetailsProvider>();
            _fakePermitDecryption = A.Fake<IPermitDecryption>();
            _fakeEncContentSapMessageBuilder = new EncContentSapMessageBuilder(_fakeLogger, _fakeXmlHelper, _fakeFileSystemHelper, _fakeSapActionConfig, _fakeWeekDetailsProvider, _fakePermitDecryption);
            _sapXmlTemplate = TestHelper.ReadFileData("SapXmlTemplates\\SAPRequest.xml");
        }

        private IConfiguration InitConfiguration()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory() + @"/ConfigurationFiles")
                .AddJsonFile("SapActions.json")
                .AddEnvironmentVariables()
                .Build();

            return config;
        }

        [Test]
        public void WhenSapXmlTemplateFileNotExist_ThenThrowERPFacadeException()
        {
            var newCellEventPayloadJson = TestHelper.ReadFileData("ERPTestData\\NewCell.JSON");
            var eventData = JsonConvert.DeserializeObject<EncEventPayload>(newCellEventPayloadJson);

            A.CallTo(() => _fakeFileSystemHelper.IsFileExists(A<string>.Ignored)).Returns(false);

            Assert.Throws<ERPFacadeException>(() => _fakeEncContentSapMessageBuilder.BuildSapMessageXml(eventData!))
                .Message.Should().Be("The SAP XML payload template does not exist.");       
        }

        [Test]
        public void WhenBuildSapMessageXmlIsCalledWithCancelCellWithExistingCellReplacementScenario_ThenReturnXMLDocument()
        {
            var cancelReplaceCellEventPayloadJson = TestHelper.ReadFileData("ERPTestData\\CancelCellWithExistingCellReplacement.JSON");
            var eventData = JsonConvert.DeserializeObject<EncEventPayload>(cancelReplaceCellEventPayloadJson);

            XmlDocument soapXml = new();
            soapXml.LoadXml(_sapXmlTemplate);

            A.CallTo(() => _fakeFileSystemHelper.IsFileExists(A<string>.Ignored)).Returns(true);
            A.CallTo(() => _fakeXmlHelper.CreateXmlDocument(A<string>.Ignored)).Returns(soapXml);
            A.CallTo(() => _fakeWeekDetailsProvider.GetDateOfWeek(A<int>.Ignored, A<int>.Ignored, A<bool>.Ignored)).Returns("20240808");

            var result = _fakeEncContentSapMessageBuilder.BuildSapMessageXml(eventData!);

            result.Should().BeOfType<XmlDocument>();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
            && call.GetArgument<LogLevel>(0) == LogLevel.Information
            && call.GetArgument<EventId>(1) == EventIds.GenerationOfSapXmlPayloadStarted.ToEventId()
            && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Generation of SAP XML payload started.").MustHaveHappenedOnceExactly();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
             && call.GetArgument<LogLevel>(0) == LogLevel.Information
             && call.GetArgument<EventId>(1) == EventIds.EncCellSapActionGenerationStarted.ToEventId()
             && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Building ENC cell SAP actions.").MustHaveHappenedOnceExactly();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
             && call.GetArgument<LogLevel>(0) == LogLevel.Information
             && call.GetArgument<EventId>(1) == EventIds.BuildingSapActionStarted.ToEventId()
             && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Building SAP action {ActionName}.").MustHaveHappened();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
            && call.GetArgument<LogLevel>(0) == LogLevel.Information
            && call.GetArgument<EventId>(1) == EventIds.SapActionCreated.ToEventId()
            && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "SAP action {ActionName} created.").MustHaveHappened(7, Times.Exactly);

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
             && call.GetArgument<LogLevel>(0) == LogLevel.Information
             && call.GetArgument<EventId>(1) == EventIds.GenerationOfSapXmlPayloadCompleted.ToEventId()
             && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Generation of SAP XML payload completed.").MustHaveHappenedOnceExactly();
        }

        [Test]
        public void WhenBuildSapMessageXmlIsCalledWithNewCellScenario_ThenReturnXMLDocument()
        {
            var newCellEventPayloadJson = TestHelper.ReadFileData("ERPTestData\\NewCell.JSON");
            var eventData = JsonConvert.DeserializeObject<EncEventPayload>(newCellEventPayloadJson);
            var permitKeys = new DecryptedPermit { ActiveKey = "firstkey", NextKey = "nextkey" };

            XmlDocument soapXml = new();
            soapXml.LoadXml(_sapXmlTemplate);

            A.CallTo(() => _fakeFileSystemHelper.IsFileExists(A<string>.Ignored)).Returns(true);
            A.CallTo(() => _fakeXmlHelper.CreateXmlDocument(A<string>.Ignored)).Returns(soapXml);
            A.CallTo(() => _fakePermitDecryption.Decrypt(A<string>.Ignored)).Returns(permitKeys);
            A.CallTo(() => _fakeWeekDetailsProvider.GetDateOfWeek(A<int>.Ignored, A<int>.Ignored, A<bool>.Ignored)).Returns("20240801");

            var result = _fakeEncContentSapMessageBuilder.BuildSapMessageXml(eventData!);

            result.Should().BeOfType<XmlDocument>();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
            && call.GetArgument<LogLevel>(0) == LogLevel.Information
            && call.GetArgument<EventId>(1) == EventIds.GenerationOfSapXmlPayloadStarted.ToEventId()
            && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Generation of SAP XML payload started.").MustHaveHappenedOnceExactly();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
             && call.GetArgument<LogLevel>(0) == LogLevel.Information
             && call.GetArgument<EventId>(1) == EventIds.EncCellSapActionGenerationStarted.ToEventId()
             && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Building ENC cell SAP actions.").MustHaveHappenedOnceExactly();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
             && call.GetArgument<LogLevel>(0) == LogLevel.Information
             && call.GetArgument<EventId>(1) == EventIds.BuildingSapActionStarted.ToEventId()
             && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Building SAP action {ActionName}.").MustHaveHappened();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
                                                && call.GetArgument<LogLevel>(0) == LogLevel.Information
                                                && call.GetArgument<EventId>(1) == EventIds.SapActionCreated.ToEventId()
            && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "SAP action {ActionName} created.").MustHaveHappened(4, Times.Exactly);

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
             && call.GetArgument<LogLevel>(0) == LogLevel.Information
             && call.GetArgument<EventId>(1) == EventIds.GenerationOfSapXmlPayloadCompleted.ToEventId()
             && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Generation of SAP XML payload completed.").MustHaveHappenedOnceExactly();
        }

        [Test]
        public void WhenBuildSapMessageXmlIsCalledWithUpdateCellScenario_ThenReturnXMLDocument()
        {
            var updateCellEventPayloadJson = TestHelper.ReadFileData("ERPTestData\\UpdateCell.JSON");
            var eventData = JsonConvert.DeserializeObject<EncEventPayload>(updateCellEventPayloadJson);
            var permitKeys = new DecryptedPermit { ActiveKey = "firstkey", NextKey = "nextkey" };

            XmlDocument soapXml = new();
            soapXml.LoadXml(_sapXmlTemplate);

            A.CallTo(() => _fakeFileSystemHelper.IsFileExists(A<string>.Ignored)).Returns(true);
            A.CallTo(() => _fakeXmlHelper.CreateXmlDocument(A<string>.Ignored)).Returns(soapXml);
            A.CallTo(() => _fakePermitDecryption.Decrypt(A<string>.Ignored)).Returns(permitKeys);
            A.CallTo(() => _fakeWeekDetailsProvider.GetDateOfWeek(A<int>.Ignored, A<int>.Ignored, A<bool>.Ignored)).Returns("20240801");

            var result = _fakeEncContentSapMessageBuilder.BuildSapMessageXml(eventData!);

            result.Should().BeOfType<XmlDocument>();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
            && call.GetArgument<LogLevel>(0) == LogLevel.Information
            && call.GetArgument<EventId>(1) == EventIds.GenerationOfSapXmlPayloadStarted.ToEventId()
            && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Generation of SAP XML payload started.").MustHaveHappenedOnceExactly();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
             && call.GetArgument<LogLevel>(0) == LogLevel.Information
             && call.GetArgument<EventId>(1) == EventIds.EncCellSapActionGenerationStarted.ToEventId()
             && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Building ENC cell SAP actions.").MustHaveHappenedOnceExactly();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
             && call.GetArgument<LogLevel>(0) == LogLevel.Information
             && call.GetArgument<EventId>(1) == EventIds.BuildingSapActionStarted.ToEventId()
             && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Building SAP action {ActionName}.").MustHaveHappened();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
                                                && call.GetArgument<LogLevel>(0) == LogLevel.Information
                                                && call.GetArgument<EventId>(1) == EventIds.SapActionCreated.ToEventId()
            && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "SAP action {ActionName} created.").MustHaveHappened(3, Times.Exactly);

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
             && call.GetArgument<LogLevel>(0) == LogLevel.Information
             && call.GetArgument<EventId>(1) == EventIds.GenerationOfSapXmlPayloadCompleted.ToEventId()
             && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Generation of SAP XML payload completed.").MustHaveHappenedOnceExactly();
        }

        [Test]
        public void WhenBuildSapMessageXmlIsCalledWithAdditionalCoverageWithNewEditionScenario_ThenReturnXMLDocument()
        {
            var additionalCoverageEventPayloadJson = TestHelper.ReadFileData("ERPTestData\\AdditionalCoverageWithNewEdition.JSON");
            var eventData = JsonConvert.DeserializeObject<EncEventPayload>(additionalCoverageEventPayloadJson);
            var permitKeys = new DecryptedPermit { ActiveKey = "firstkey", NextKey = "nextkey" };

            XmlDocument soapXml = new();
            soapXml.LoadXml(_sapXmlTemplate);

            A.CallTo(() => _fakeFileSystemHelper.IsFileExists(A<string>.Ignored)).Returns(true);
            A.CallTo(() => _fakeXmlHelper.CreateXmlDocument(A<string>.Ignored)).Returns(soapXml);
            A.CallTo(() => _fakePermitDecryption.Decrypt(A<string>.Ignored)).Returns(permitKeys);
            A.CallTo(() => _fakeWeekDetailsProvider.GetDateOfWeek(A<int>.Ignored, A<int>.Ignored, A<bool>.Ignored)).Returns("20240801");

            var result = _fakeEncContentSapMessageBuilder.BuildSapMessageXml(eventData!);

            result.Should().BeOfType<XmlDocument>();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
            && call.GetArgument<LogLevel>(0) == LogLevel.Information
            && call.GetArgument<EventId>(1) == EventIds.GenerationOfSapXmlPayloadStarted.ToEventId()
            && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Generation of SAP XML payload started.").MustHaveHappenedOnceExactly();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
             && call.GetArgument<LogLevel>(0) == LogLevel.Information
             && call.GetArgument<EventId>(1) == EventIds.EncCellSapActionGenerationStarted.ToEventId()
             && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Building ENC cell SAP actions.").MustHaveHappenedOnceExactly();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
             && call.GetArgument<LogLevel>(0) == LogLevel.Information
             && call.GetArgument<EventId>(1) == EventIds.BuildingSapActionStarted.ToEventId()
             && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Building SAP action {ActionName}.").MustHaveHappened();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
            && call.GetArgument<LogLevel>(0) == LogLevel.Information
            && call.GetArgument<EventId>(1) == EventIds.SapActionCreated.ToEventId()
            && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "SAP action {ActionName} created.").MustHaveHappened(4, Times.Exactly);

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
             && call.GetArgument<LogLevel>(0) == LogLevel.Information
             && call.GetArgument<EventId>(1) == EventIds.GenerationOfSapXmlPayloadCompleted.ToEventId()
             && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Generation of SAP XML payload completed.").MustHaveHappenedOnceExactly();
        }

        [Test]
        public void SortXmlPayloadTest()
        {
            var expectedActionNumber = "1";
            var expectedAction = "CREATE ENC CELL";
            var sapReqXml = TestHelper.ReadFileData("ERPTestData\\UnsortedActionItems.xml");

            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(sapReqXml);
            var actionItemNode = xmlDoc.SelectSingleNode(XpathActionItems);

            var sortedXmlPayLoad = typeof(EncContentSapMessageBuilder).GetMethod("SortXmlPayload", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance)!;
            var result = (XmlNode)sortedXmlPayLoad.Invoke(_fakeEncContentSapMessageBuilder, new object[] { actionItemNode! })!;

            var firstActionNumber = result.SelectSingleNode(XpathActionNumber);
            firstActionNumber.InnerXml.Should().Be(expectedActionNumber);

            var firstActionName = result.SelectSingleNode(XpathAction);
            firstActionName.InnerXml.Should().Be(expectedAction);
        }

        [Test]
        public void WhenBuildSapMessageXmlIsCalledWithNewCellWithNoUnitOfSaleHavingTypeIsUnit_ThenThrowERPFacadeException()
        {
            var newCellEventPayloadJson = TestHelper.ReadFileData("ERPTestData\\NewCellWithNoUnitOfSaleHavingTypeIsUnit.JSON");
            var eventData = JsonConvert.DeserializeObject<EncEventPayload>(newCellEventPayloadJson);

            XmlDocument soapXml = new();
            soapXml.LoadXml(_sapXmlTemplate);

            A.CallTo(() => _fakeFileSystemHelper.IsFileExists(A<string>.Ignored)).Returns(true);
            A.CallTo(() => _fakeXmlHelper.CreateXmlDocument(A<string>.Ignored)).Returns(soapXml);

            Assert.Throws<ERPFacadeException>(() => _fakeEncContentSapMessageBuilder.BuildSapMessageXml(eventData!))
                .Message.Should().Be("Required unit not found in event payload to generate CREATE ENC CELL action for US5AK9DI.");
        }

        [Test]
        public void WhenBuildSapMessageXmlIsCalledWithReplaceCellWithNoUnitOfSaleHavingTypeIsUnit_ThenThrowERPFacadeException()
        {
            var newCellEventPayloadJson = TestHelper.ReadFileData("ERPTestData\\ReplaceCellWithNoUnitOfSaleHavingTypeIsUnit.JSON");
            var eventData = JsonConvert.DeserializeObject<EncEventPayload>(newCellEventPayloadJson);

            XmlDocument soapXml = new();
            soapXml.LoadXml(_sapXmlTemplate);

            A.CallTo(() => _fakeFileSystemHelper.IsFileExists(A<string>.Ignored)).Returns(true);
            A.CallTo(() => _fakeXmlHelper.CreateXmlDocument(A<string>.Ignored)).Returns(soapXml);

            Assert.Throws<ERPFacadeException>(() => _fakeEncContentSapMessageBuilder.BuildSapMessageXml(eventData!))
            .Message.Should().Be("Required unit not found in event payload to generate REPLACED WITH ENC CELL action for GB50382B.");
        }

        [Test]
        public void WhenBuildSapMessageXmlIfRequiredAttributesNotProvided_ThenThrowERPFacadeException()
        {
            var newCellEventPayloadJson = TestHelper.ReadFileData("ERPTestData\\NewCellWithoutProviderCodeAttributes.JSON");
            var eventData = JsonConvert.DeserializeObject<EncEventPayload>(newCellEventPayloadJson);

            XmlDocument soapXml = new();
            soapXml.LoadXml(_sapXmlTemplate);

            A.CallTo(() => _fakeFileSystemHelper.IsFileExists(A<string>.Ignored)).Returns(true);
            A.CallTo(() => _fakeXmlHelper.CreateXmlDocument(A<string>.Ignored)).Returns(soapXml);

            Assert.Throws<ERPFacadeException>(() => _fakeEncContentSapMessageBuilder.BuildSapMessageXml(eventData!))
                .Message.Should().Be("Error while generating SAP action information. | Action : CREATE ENC CELL | XML Attribute : PROVIDER | ErrorMessage : Object reference not set to an instance of an object.");
        }

        [Test]
        public void WhenBuildSapMessageXmlIfUkhoWeekNumberSectionNotProvided_ThenThrowERPFacadeException()
        {
            var newCellEventPayloadJson = TestHelper.ReadFileData("ERPTestData\\NewCellWithoutUkhoWeekNumberSection.JSON");
            var eventData = JsonConvert.DeserializeObject<EncEventPayload>(newCellEventPayloadJson);
            var permitKeys = new DecryptedPermit { ActiveKey = "firstkey", NextKey = "nextkey" };

            XmlDocument soapXml = new();
            soapXml.LoadXml(_sapXmlTemplate);

            A.CallTo(() => _fakeFileSystemHelper.IsFileExists(A<string>.Ignored)).Returns(true);
            A.CallTo(() => _fakeXmlHelper.CreateXmlDocument(A<string>.Ignored)).Returns(soapXml);
            A.CallTo(() => _fakePermitDecryption.Decrypt(A<string>.Ignored)).Returns(permitKeys);

            Assert.Throws<ERPFacadeException>(() => _fakeEncContentSapMessageBuilder.BuildSapMessageXml(eventData!))
            .Message.Should().Be("UkhoWeekNumber section not found in enccontentpublished event payload while creating CREATE ENC CELL action.");
        }

        [Test]
        public void WhenBuildSapMessageXmlWithWrongUkhoWeekNumberDetails_ThenThrowERPFacadeException()
        {
            var newCellEventPayloadJson = TestHelper.ReadFileData("ERPTestData\\NewCellWithWrongUkhoWeekDetails.JSON");
            var eventData = JsonConvert.DeserializeObject<EncEventPayload>(newCellEventPayloadJson);
            var permitKeys = new DecryptedPermit { ActiveKey = "firstkey", NextKey = "nextkey" };

            XmlDocument soapXml = new();
            soapXml.LoadXml(_sapXmlTemplate);

            A.CallTo(() => _fakeFileSystemHelper.IsFileExists(A<string>.Ignored)).Returns(true);
            A.CallTo(() => _fakeXmlHelper.CreateXmlDocument(A<string>.Ignored)).Returns(soapXml);
            A.CallTo(() => _fakePermitDecryption.Decrypt(A<string>.Ignored)).Returns(permitKeys);
            A.CallTo(() => _fakeWeekDetailsProvider.GetDateOfWeek(A<int>.Ignored, A<int>.Ignored, A<bool>.Ignored)).Throws<System.Exception>();

            Assert.Throws<ERPFacadeException>(() => _fakeEncContentSapMessageBuilder.BuildSapMessageXml(eventData!))
                .Message.Should().Be("Error while generating SAP action information. | Action : CREATE ENC CELL | XML Attribute : VALIDFROM | ErrorMessage : Exception of type 'System.Exception' was thrown.");
        }        

        [Test]
        public void WhenUnitOfSaleIsNullWhileReplacingEncCell_ThenReturnsNull()
        {
            var cancelCellWithNewCellReplacementPayloadJson = TestHelper.ReadFileData("ERPTestData\\CancelCellWithNewCellReplacement.JSON");
            var eventData = JsonConvert.DeserializeObject<EncEventPayload>(cancelCellWithNewCellReplacementPayloadJson);
            var action = _fakeSapActionConfig.Value.SapActions.FirstOrDefault(x => x.Product == EncCell && x.Action == ReplaceEncCellAction);

            MethodInfo buildAction = typeof(EncContentSapMessageBuilder).GetMethod("GetUnitOfSale", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance)!;
            var result = (XmlElement)buildAction.Invoke(_fakeEncContentSapMessageBuilder, new object[] { action.ActionNumber, eventData.Data.UnitsOfSales!, eventData.Data.Products.FirstOrDefault()! })!;

            result.Should().BeNull();
        }

        [Test]
        public void WhenUnitOfSaleIsNullWhileChangingEncCell_ThenReturnsNull()
        {
            var cancelCellWithNewCellReplacementPayloadJson = TestHelper.ReadFileData("ERPTestData\\CancelCellWithNewCellReplacement.JSON");
            var eventData = JsonConvert.DeserializeObject<EncEventPayload>(cancelCellWithNewCellReplacementPayloadJson);
            var action = _fakeSapActionConfig.Value.SapActions.FirstOrDefault(x => x.Product == EncCell && x.Action == ChangeEncCellAction);

            MethodInfo buildAction = typeof(EncContentSapMessageBuilder).GetMethod("GetUnitOfSale", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance)!;
            var result = (XmlElement)buildAction.Invoke(_fakeEncContentSapMessageBuilder, new object[] { action.ActionNumber, eventData.Data.UnitsOfSales!, eventData.Data.Products.LastOrDefault()! })!;

            result.Should().BeNull();
        }

        [Test]
        [TestCase("fieldvalue", null, "fieldvalue")]
        [TestCase("fieldvalue", "AGENCY", "fi")]
        [TestCase("fieldvalue", "", "fieldvalue")]
        public void WhenGetXmlNodeValue(string fieldValue, string xmlNodeName, string expectedValue)
        {
            MethodInfo buildAction = typeof(EncContentSapMessageBuilder).GetMethod("GetXmlNodeValue", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance)!;
            var result = (string)buildAction.Invoke(_fakeEncContentSapMessageBuilder, new object[] { fieldValue, xmlNodeName })!;

            result.Should().Be(expectedValue);
        }
    }
}
