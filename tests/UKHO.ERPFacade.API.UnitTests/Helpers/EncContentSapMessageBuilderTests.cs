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
using UKHO.ERPFacade.Common.IO;
using UKHO.ERPFacade.Common.Logging;
using UKHO.ERPFacade.Common.Models;
using UKHO.ERPFacade.Common.Providers;
using UKHO.ERPFacade.Common.PermitDecryption;

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
        public void WhenBuildSapMessageXmlIsCalledWithCancelCellWithExistingCellReplacementScenario_ThenReturnXMLDocument()
        {
            var cancelReplaceCellEventPayloadJson = TestHelper.ReadFileData("ERPTestData\\CancelCellWithExistingCellReplacement.JSON");
            var evenData = JsonConvert.DeserializeObject<EncEventPayload>(cancelReplaceCellEventPayloadJson);
            var cancelReplaceCellXmlPayload = TestHelper.ReadFileData("ERPTestData\\CancelCellWithExistingReplacement.xml");

            XmlDocument soapXml = new();
            soapXml.LoadXml(_sapXmlTemplate);

            A.CallTo(() => _fakeFileSystemHelper.IsFileExists(A<string>.Ignored)).Returns(true);
            A.CallTo(() => _fakeXmlHelper.CreateXmlDocument(A<string>.Ignored)).Returns(soapXml);
            A.CallTo(() => _fakeWeekDetailsProvider.GetDateOfWeek(A<int>.Ignored, A<int>.Ignored, A<bool>.Ignored)).Returns("20240808");

            var result = _fakeEncContentSapMessageBuilder.BuildSapMessageXml(evenData!);

            result.Should().BeOfType<XmlDocument>();
            var actionItem = result.SelectSingleNode(XpathActionItems);

            soapXml.LoadXml(cancelReplaceCellXmlPayload);
            var expectedActionItem = soapXml.SelectSingleNode(XpathActionItems);
            actionItem.InnerXml.Should().Be(expectedActionItem.InnerXml);

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
                                                && call.GetArgument<LogLevel>(0) == LogLevel.Information
            && call.GetArgument<EventId>(1) == EventIds.BuildingSapActionStarted.ToEventId()
            && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Building SAP actions.").MustHaveHappenedOnceExactly();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
                                                && call.GetArgument<LogLevel>(0) == LogLevel.Information
                                                && call.GetArgument<EventId>(1) == EventIds.SapActionCreated.ToEventId()
            && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "SAP action {ActionName} created.").MustHaveHappened(7, Times.Exactly);
        }

        [Test]
        public void WhenBuildSapMessageXmlIsCalledWithNewCellScenario_ThenReturnXMLDocument()
        {
            var newCellEventPayloadJson = TestHelper.ReadFileData("ERPTestData\\NewCell.JSON");
            var evenData = JsonConvert.DeserializeObject<EncEventPayload>(newCellEventPayloadJson);
            var newCellXmlPayload = TestHelper.ReadFileData("ERPTestData\\NewCell.xml");
            var permitKeys = new DecryptedPermit { ActiveKey = "firstkey", NextKey = "nextkey" };

            XmlDocument soapXml = new();
            soapXml.LoadXml(_sapXmlTemplate);

            A.CallTo(() => _fakeFileSystemHelper.IsFileExists(A<string>.Ignored)).Returns(true);
            A.CallTo(() => _fakeXmlHelper.CreateXmlDocument(A<string>.Ignored)).Returns(soapXml);
            A.CallTo(() => _fakePermitDecryption.Decrypt(A<string>.Ignored)).Returns(permitKeys);
            A.CallTo(() => _fakeWeekDetailsProvider.GetDateOfWeek(A<int>.Ignored, A<int>.Ignored, A<bool>.Ignored)).Returns("20240801");

            var result = _fakeEncContentSapMessageBuilder.BuildSapMessageXml(evenData!);

            result.Should().BeOfType<XmlDocument>();
            var actionItem = result.SelectSingleNode(XpathActionItems);

            soapXml.LoadXml(newCellXmlPayload);
            var expectedActionItem = soapXml.SelectSingleNode(XpathActionItems);
            actionItem.InnerXml.Should().Be(expectedActionItem.InnerXml);

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
                                                && call.GetArgument<LogLevel>(0) == LogLevel.Information
            && call.GetArgument<EventId>(1) == EventIds.BuildingSapActionStarted.ToEventId()
            && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Building SAP actions.").MustHaveHappenedOnceExactly();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
                                                && call.GetArgument<LogLevel>(0) == LogLevel.Information
                                                && call.GetArgument<EventId>(1) == EventIds.SapActionCreated.ToEventId()
            && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "SAP action {ActionName} created.").MustHaveHappened(4, Times.Exactly);
        }

        [Test]
        public void WhenBuildSapMessageXmlIsCalledWithUpdateCellScenario_ThenReturnXMLDocument()
        {
            var updateCellEventPayloadJson = TestHelper.ReadFileData("ERPTestData\\UpdateCell.JSON");
            var eventData = JsonConvert.DeserializeObject<EncEventPayload>(updateCellEventPayloadJson);
            var updateCellXmlPayload = TestHelper.ReadFileData("ERPTestData\\UpdateCell.xml");
            var permitKeys = new DecryptedPermit { ActiveKey = "firstkey", NextKey = "nextkey" };

            XmlDocument soapXml = new();
            soapXml.LoadXml(_sapXmlTemplate);

            A.CallTo(() => _fakeFileSystemHelper.IsFileExists(A<string>.Ignored)).Returns(true);
            A.CallTo(() => _fakeXmlHelper.CreateXmlDocument(A<string>.Ignored)).Returns(soapXml);
            A.CallTo(() => _fakePermitDecryption.Decrypt(A<string>.Ignored)).Returns(permitKeys);
            A.CallTo(() => _fakeWeekDetailsProvider.GetDateOfWeek(A<int>.Ignored, A<int>.Ignored, A<bool>.Ignored)).Returns("20240801");

            var result = _fakeEncContentSapMessageBuilder.BuildSapMessageXml(eventData!);

            result.Should().BeOfType<XmlDocument>();
            var actionItem = result.SelectSingleNode(XpathActionItems);

            soapXml.LoadXml(updateCellXmlPayload);
            var expectedActionItem = soapXml.SelectSingleNode(XpathActionItems);
            actionItem.InnerXml.Should().Be(expectedActionItem.InnerXml);

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
                                                && call.GetArgument<LogLevel>(0) == LogLevel.Information
            && call.GetArgument<EventId>(1) == EventIds.BuildingSapActionStarted.ToEventId()
            && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Building SAP actions.").MustHaveHappenedOnceExactly();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
                                                && call.GetArgument<LogLevel>(0) == LogLevel.Information
                                                && call.GetArgument<EventId>(1) == EventIds.SapActionCreated.ToEventId()
            && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "SAP action {ActionName} created.").MustHaveHappened(3, Times.Exactly);
        }
        
        [Test]
        public void WhenBuildSapMessageXmlIsCalledWithAdditionalCoverageWithNewEditionScenario_ThenReturnXMLDocument()
        {
            var additionalCoverageEventPayloadJson = TestHelper.ReadFileData("ERPTestData\\AdditionalCoverageWithNewEdition.JSON");
            var evenData = JsonConvert.DeserializeObject<EncEventPayload>(additionalCoverageEventPayloadJson);
            var additionalCoverageXmlPayload = TestHelper.ReadFileData("ERPTestData\\AdditionalCoverageWithNewEdition.xml");
            var permitKeys = new DecryptedPermit { ActiveKey = "firstkey", NextKey = "nextkey" };

            XmlDocument soapXml = new();
            soapXml.LoadXml(_sapXmlTemplate);

            A.CallTo(() => _fakeFileSystemHelper.IsFileExists(A<string>.Ignored)).Returns(true);
            A.CallTo(() => _fakeXmlHelper.CreateXmlDocument(A<string>.Ignored)).Returns(soapXml);
            A.CallTo(() => _fakePermitDecryption.Decrypt(A<string>.Ignored)).Returns(permitKeys);
            A.CallTo(() => _fakeWeekDetailsProvider.GetDateOfWeek(A<int>.Ignored, A<int>.Ignored, A<bool>.Ignored)).Returns("20240801");

            var result = _fakeEncContentSapMessageBuilder.BuildSapMessageXml(evenData!);

            result.Should().BeOfType<XmlDocument>();
            var actionItem = result.SelectSingleNode(XpathActionItems);

            soapXml.LoadXml(additionalCoverageXmlPayload);
            var expectedActionItem = soapXml.SelectSingleNode(XpathActionItems);
            actionItem.InnerXml.Should().Be(expectedActionItem.InnerXml);

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
                                                && call.GetArgument<LogLevel>(0) == LogLevel.Information
            && call.GetArgument<EventId>(1) == EventIds.BuildingSapActionStarted.ToEventId()
            && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Building SAP actions.").MustHaveHappenedOnceExactly();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
                                                && call.GetArgument<LogLevel>(0) == LogLevel.Information
                                                && call.GetArgument<EventId>(1) == EventIds.SapActionCreated.ToEventId()
            && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "SAP action {ActionName} created.").MustHaveHappened(4, Times.Exactly);
        }

        [Test]
        public void WhenSapXmlTemplateFileNotExist_ThenThrowFileNotFoundException()
        {
            var newCellEventPayloadJson = TestHelper.ReadFileData("ERPTestData\\NewCell.JSON");
            var evenData = JsonConvert.DeserializeObject<EncEventPayload>(newCellEventPayloadJson);

            A.CallTo(() => _fakeFileSystemHelper.IsFileExists(A<string>.Ignored)).Returns(false);

            Assert.Throws<FileNotFoundException>(() => _fakeEncContentSapMessageBuilder.BuildSapMessageXml(evenData!));

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
                                                && call.GetArgument<LogLevel>(0) == LogLevel.Error
            && call.GetArgument<EventId>(1) == EventIds.SapXmlTemplateNotFound.ToEventId()
            && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "The SAP xml payload template does not exist.").MustHaveHappenedOnceExactly();
        }        

        [Test]
        public void WhenMoreThanOneUnitOfSaleHavingUnitOfSaleTypeUnitIsPassedToGetUnitOfSaleForEncCell_ThenReturnsFirstUnitOfSaleHavingAddProduct()
        {
            var listOfUnitOfSales = new List<UnitOfSale>()
            {
                 new UnitOfSale() { UnitName = "MX545010", Title = "Title1", UnitOfSaleType = "unit", Status = "ForSale",
                    CompositionChanges = new CompositionChanges { AddProducts = new List<string>() } },
                new UnitOfSale() { UnitName = "MX509226", Title = "Title2", UnitOfSaleType = "unit", Status = "ForSale",
                    CompositionChanges = new CompositionChanges { AddProducts = new List<string>(){ "MX545010" } } }
            };

            var product = new Product()
            {
                ProductName = "MX545010",
                InUnitsOfSale = new List<string>() { "MX545010", "MX509226" }
            };

            var methodInfo = typeof(EncContentSapMessageBuilder).GetMethod("GetUnitOfSale", BindingFlags.NonPublic | BindingFlags.Instance)!;
            var result = (UnitOfSale)methodInfo.Invoke(_fakeEncContentSapMessageBuilder, [1, listOfUnitOfSales, product])!;

            result.UnitName.Should().Be("MX509226");
        }

        [Test]
        [TestCase("RU4O5XQ0", "AGENCY", "RU")]
        [TestCase("", "Agency", "")]
        [TestCase(null, "Agency", "")]
        public void GetXmlNodeValueTest(string fieldValue, string xmlNodeName, string actual)
        {
            MethodInfo xmlNodeValue = typeof(EncContentSapMessageBuilder).GetMethod("GetXmlNodeValue", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance)!;
            var result = (string)xmlNodeValue.Invoke(_fakeEncContentSapMessageBuilder, new object[] { fieldValue, xmlNodeName })!;

            result.Should().Be(actual);
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
            var result = (XmlNode)sortedXmlPayLoad.Invoke(_fakeEncContentSapMessageBuilder, [actionItemNode!])!;

            var firstActionNumber = result.SelectSingleNode(XpathActionNumber);
            firstActionNumber.InnerXml.Should().Be(expectedActionNumber);

            var firstActionName = result.SelectSingleNode(XpathAction);
            firstActionName.InnerXml.Should().Be(expectedAction);
        }

        [Test]
        [TestCase("202403", 3, 2024)]
        [TestCase("202512", 12, 2025)]
        public void GetUkhoWeekNumberDataTest(string expectedResult, int week, int year)
        {
            UkhoWeekNumber ukhoWeekNumber = new()
            {
                Year = year,
                Week = week
            };

            var getUkhoWeekNumber = typeof(EncContentSapMessageBuilder).GetMethod("GetUkhoWeekNumberData", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance)!;
            var result = (string)getUkhoWeekNumber.Invoke(_fakeEncContentSapMessageBuilder, [ukhoWeekNumber])!;

            result.Should().Be(expectedResult);
        }
    }
}
