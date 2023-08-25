using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NUnit.Framework;
using UKHO.ERPFacade.API.Helpers;
using UKHO.ERPFacade.API.UnitTests.Common;
using UKHO.ERPFacade.Common.IO;
using UKHO.ERPFacade.Common.Logging;
using UKHO.ERPFacade.Common.Models;

namespace UKHO.ERPFacade.API.UnitTests.Helpers
{
    [TestFixture]
    public class LicenceUpdatedSapMessageBuilderTests
    {
        private ILogger<LicenceUpdatedSapMessageBuilder> _fakeLogger;
        private IXmlHelper _fakeXmlHelper;
        private IFileSystemHelper _fakeFileSystemHelper;

        private LicenceUpdatedSapMessageBuilder _fakeLicenceUpdatedSapMessageBuilder;
        private readonly string XpathZAddsRos = $"//*[local-name()='Z_ADDS_ROS']";
        private readonly string XpathStartDate = $"//*[local-name()='STARTDATE']";
        private readonly string XpathEndDate = $"//*[local-name()='ENDDATE']";
        private readonly string XpathLType = $"//*[local-name()='LTYPE']";
        private readonly string XpathLicDur = $"//*[local-name()='LICDUR']";
        private readonly string XpathPO = $"//*[local-name()='PO']";
        private readonly string XpathAdsOrdno = $"//*[local-name()='ADSORDNO']";
        private readonly string XpathProd = $"//*[local-name()='PROD']";
        private const string XmlNameSpace = "http://www.w3.org/2001/XMLSchema-instance";

        private readonly string RosSapXmlFile = @"<?xml version=""1.0"" encoding=""utf-8""?>
<soap:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">
  <soap:Body>
    <Z_ADDS_ROS xmlns=""urn:sap-com:document:sap:rfc:functions"">
      <IM_ORDER xmlns="""">
        <GUID></GUID>
        <SERVICETYPE></SERVICETYPE>
        <LICTRANSACTION></LICTRANSACTION>
        <SOLDTOACC></SOLDTOACC>
        <LICENSEEACC></LICENSEEACC>
        <STARTDATE></STARTDATE>
        <ENDDATE></ENDDATE>
        <LICNO></LICNO>
        <VNAME></VNAME>
        <IMO></IMO>
        <CALLSIGN></CALLSIGN>
        <SHOREBASED></SHOREBASED>
        <FLEET></FLEET>
        <USERS></USERS>
        <ENDUSERID></ENDUSERID>
        <ECDISMANUF></ECDISMANUF>
        <LTYPE></LTYPE>
        <LICDUR></LICDUR>
        <PO></PO>
        <ADSORDNO></ADSORDNO>
        <PROD>
          <item>
            <ID></ID>
            <ENDDA></ENDDA>
            <DURATION></DURATION>
            <RENEW></RENEW>
          </item>
        </PROD>
      </IM_ORDER>
    </Z_ADDS_ROS>
  </soap:Body>
</soap:Envelope>
";

        #region Data

        private readonly string licenceUpdatedJsonData = @"{""specversion"": ""1.0"",""type"": ""uk.gov.ukho.licensing.licenceUpdated.v1"",""source"": ""https://uk.gov.ukho.licensing"",""id"": ""e744fa37-0c9f-4795-adc9-7f42ad8f11c1"",""time"": ""8/23/2023 7:34:28 AM"",""subject"": ""licence update changes that need to go to SAP Record of Sale via ERP Facade"",""datacontenttype"": ""application/json"",""data"": {""correlationId"": ""123-abc-456-xyz-333"",""license"": {
      ""licenseId"": ""2"",""productType"": ""AVCS"",""transactionType"": ""CHANGELICENCE"",""distributorCustomerNumber"": ""111"",""shippingCoNumber"": ""1"",""ordernumber"": """",""orderDate"": """",""po-ref"": """",""holdingsExpiryDate"": ""2024-59-31"",""sapId"": ""76611K"",""vesselName"": ""Vessel 000002"",""imoNumber"": ""IMO000002"",""callSign"": ""CALL000002"",""licenceType"": """",""shoreBased"": """",""fleetName"": ""emailnoreply@engineering.ukho.gov.uk"",""numberLicenceUsers"": 1,""upn"": ""MARIS"",""licenceDuration"": 12,
      ""unitsOfSale"": [{""unitName"": """",""endDate"": """",""duration"": """",""renew"": """",""repeat"": """"}]}}}";

        #endregion

        [SetUp]
        public void Setup()
        {
            _fakeLogger = A.Fake<ILogger<LicenceUpdatedSapMessageBuilder>>();
            _fakeXmlHelper = A.Fake<IXmlHelper>();
            _fakeFileSystemHelper = A.Fake<IFileSystemHelper>();
            _fakeLicenceUpdatedSapMessageBuilder = new LicenceUpdatedSapMessageBuilder(_fakeLogger, _fakeXmlHelper, _fakeFileSystemHelper);
        }

        [Test]
        public void WhenTransactionTypeIsChangeLicence_ThenReturnXMLDocument()
        {
            var jsonData = JsonConvert.DeserializeObject<RecordOfSaleEventPayLoad>(licenceUpdatedJsonData);
            var correlationId = "123-abc-456-xyz-333";
            var sapReqXml = TestHelper.ReadFileData("ERPTestData\\RoSPayloadTest.xml");

            XmlDocument soapXml = new();
            soapXml.LoadXml(RosSapXmlFile);

            A.CallTo(() => _fakeFileSystemHelper.IsFileExists(A<string>.Ignored)).Returns(true);
            A.CallTo(() => _fakeXmlHelper.CreateXmlDocument(A<string>.Ignored)).Returns(soapXml);
            A.CallTo(() => _fakeXmlHelper.CreateRecordOfSaleSapXmlPayLoad(A<SapRecordOfSalePayLoad>.Ignored)).Returns(sapReqXml);

            var result = _fakeLicenceUpdatedSapMessageBuilder.BuildLicenceUpdatedSapMessageXml(jsonData!, correlationId);

            result.Should().BeOfType<XmlDocument>();

            var actionItem = result.SelectSingleNode(XpathZAddsRos);
            actionItem.ChildNodes.Count.Should().Be(1);
            actionItem.ChildNodes[0].ChildNodes.Count.Should().Be(21);

            var startDateItem = result.SelectSingleNode(XpathStartDate);
            var endDateItem = result.SelectSingleNode(XpathEndDate);
            var lTypeItem = result.SelectSingleNode(XpathLType);
            var licDurItem = result.SelectSingleNode(XpathLicDur);
            var pOItem = result.SelectSingleNode(XpathPO);
            var adsOrdNoItem = result.SelectSingleNode(XpathAdsOrdno);
            var prodItem = result.SelectSingleNode(XpathProd);

            startDateItem.InnerXml.Should().BeEmpty();
            endDateItem.InnerXml.Should().BeEmpty();
            lTypeItem.InnerXml.Should().BeEmpty();
            licDurItem.InnerXml.Should().BeEmpty();
            pOItem.InnerXml.Should().BeEmpty();
            adsOrdNoItem.InnerXml.Should().BeEmpty();
            prodItem.ChildNodes.Count.Should().Be(1);
            prodItem.ChildNodes[0].ChildNodes.Count.Should().Be(5);

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
                                                && call.GetArgument<LogLevel>(0) == LogLevel.Information
                                                && call.GetArgument<EventId>(1) == EventIds.CreatingLicenceUpdatedSapPayload.ToEventId()
                                                && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Creating licence updated SAP Payload.").MustHaveHappenedOnceExactly();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
                                                && call.GetArgument<LogLevel>(0) == LogLevel.Information
                                                && call.GetArgument<EventId>(1) == EventIds.CreatedLicenceUpdatedSapPayload.ToEventId()
                                                && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Licence updated SAP payload created.").MustHaveHappenedOnceExactly();
        }

        [Test]
        public void WhenLicenceUpdatedSapXmlTemplateFileNotExist_ThenThrowFileNotFoundException()
        {
            var jsonData = JsonConvert.DeserializeObject<RecordOfSaleEventPayLoad>(licenceUpdatedJsonData);
            var correlationId = "123-abc-456-xyz-333";

            A.CallTo(() => _fakeFileSystemHelper.IsFileExists(A<string>.Ignored)).Returns(false);

            Assert.Throws<FileNotFoundException>(() => _fakeLicenceUpdatedSapMessageBuilder.BuildLicenceUpdatedSapMessageXml(jsonData!, correlationId));

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
                                                && call.GetArgument<LogLevel>(0) == LogLevel.Error
                                                && call.GetArgument<EventId>(1) == EventIds.LicenceUpdatedSapXmlTemplateNotFound.ToEventId()
                                                && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "The licence updated SAP message xml template does not exist.").MustHaveHappenedOnceExactly();
        }

        [Test]
        public void WhenTransactionTypeIsChangeLicenceshouldReturns_SomeFieldsEmpty_SapXmlPayloadCreationTests()
        {
            var jsonData = JsonConvert.DeserializeObject<RecordOfSaleEventPayLoad>(licenceUpdatedJsonData);
            var sapReqXml = TestHelper.ReadFileData("ERPTestData\\RoSPayloadTest.xml");

            A.CallTo(() => _fakeXmlHelper.CreateRecordOfSaleSapXmlPayLoad(A<SapRecordOfSalePayLoad>.Ignored)).Returns(sapReqXml);

            MethodInfo methodInfo = typeof(LicenceUpdatedSapMessageBuilder).GetMethod("SapXmlPayloadCreation", BindingFlags.NonPublic | BindingFlags.Instance)!;
            var result = (SapRecordOfSalePayLoad)methodInfo.Invoke(_fakeLicenceUpdatedSapMessageBuilder, new object[] { jsonData! })!;

            result.Should().NotBeNull();
            result.CorrelationId.Should().Be("123-abc-456-xyz-333");
            result.PROD.UnitOfSales.Count.Should().Be(1);
            result.OrderNumber.Should().Be("");
            result.StartDate.Should().Be("");
            result.PurachaseOrder.Should().Be("");
            result.EndDate.Should().Be("");
            result.LicenceType.Should().Be("");
            result.LicenceDuration.Should().Be(null);
        }
        [Test]
        public void WhenTransactionTypeIsNotChangeLicenceShouldNotReturns_SomeFieldsEmpty_SapXmlPayloadCreationTests()
        {
            var jsonData = JsonConvert.DeserializeObject<RecordOfSaleEventPayLoad>(licenceUpdatedJsonData);
            jsonData.Data.Licence.TransactionType = "NEWLICENCE";
            jsonData.Data.Licence.OrderNumber = "1232T";
            jsonData.Data.Licence.OrderDate = "2023-7-24";
            jsonData.Data.Licence.PoRef = "ref-121";
            jsonData.Data.Licence.HoldingsExpiryDate = "2023-7-24";
            jsonData.Data.Licence.LicenceType = "1";
            jsonData.Data.Licence.LicenceDuration = 2;

            var sapReqXml = TestHelper.ReadFileData("ERPTestData\\RoSPayloadTest.xml");

            A.CallTo(() => _fakeXmlHelper.CreateRecordOfSaleSapXmlPayLoad(A<SapRecordOfSalePayLoad>.Ignored)).Returns(sapReqXml);

            MethodInfo methodInfo = typeof(LicenceUpdatedSapMessageBuilder).GetMethod("SapXmlPayloadCreation", BindingFlags.NonPublic | BindingFlags.Instance)!;
            var result = (SapRecordOfSalePayLoad)methodInfo.Invoke(_fakeLicenceUpdatedSapMessageBuilder, new object[] { jsonData! })!;

            result.Should().NotBeNull();
            result.CorrelationId.Should().Be("123-abc-456-xyz-333");
            result.PROD.UnitOfSales.Count.Should().Be(1);
            result.OrderNumber.Should().Be(jsonData.Data.Licence.OrderNumber);
            result.StartDate.Should().Be(jsonData.Data.Licence.OrderDate);
            result.PurachaseOrder.Should().Be(jsonData.Data.Licence.PoRef);
            result.EndDate.Should().Be(jsonData.Data.Licence.HoldingsExpiryDate);
            result.LicenceType.Should().Be(jsonData.Data.Licence.LicenceType);
            result.LicenceDuration.Should().Be(jsonData.Data.Licence.LicenceDuration);
            result.PROD.UnitOfSales[0].Duration.Should().Be("");
            result.PROD.UnitOfSales[0].Id.Should().Be("");
            result.PROD.UnitOfSales[0].EndDate.Should().Be("");
            result.PROD.UnitOfSales[0].ReNew.Should().Be("");
            result.PROD.UnitOfSales[0].Repeat.Should().Be("");
        }

        [Test]
        public void RemoveNullFieldsTest()
        {
            string licenceUpdatedSapPayloadXml = TestHelper.ReadFileData("ERPTestData\\SapPayloadWithnullableNameSpace.xml");

            MethodInfo methodInfo = typeof(LicenceUpdatedSapMessageBuilder).GetMethod("RemoveNullFields", BindingFlags.NonPublic | BindingFlags.Instance)!;
            string result = (string)methodInfo.Invoke(_fakeLicenceUpdatedSapMessageBuilder, new object[] { licenceUpdatedSapPayloadXml! })!;

            result.Should().NotBeNullOrEmpty();
            result.Should().NotContain(XmlNameSpace);
        }
    }
}
