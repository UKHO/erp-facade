using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Xml;
using NUnit.Framework;
using UKHO.ERPFacade.Common.IO;
using UKHO.ERPFacade.Common.Logging;
using UKHO.ERPFacade.Common.Models;
using UKHO.ERPFacade.Common.Services;
using UKHO.ERPFacade.API.UnitTests.Common;

namespace UKHO.ERPFacade.Common.UnitTests.Services
{
    [TestFixture]
    public class RecordOfSaleSapMessageBuilderTests
    {
        private ILogger<RecordOfSaleSapMessageBuilder> _fakeLogger;
        private IXmlHelper _fakeXmlHelper;
        private IFileSystemHelper _fakeFileSystemHelper;

        private RecordOfSaleSapMessageBuilder _fakeRecordOfSaleSapMessageBuilder;

        private readonly string XpathZAddsRos = $"//*[local-name()='Z_ADDS_ROS']";
        private readonly string XpathLicNo = $"//*[local-name()='LICNO']";
        private readonly string XpathFleet = $"//*[local-name()='FLEET']";
        private readonly string XpathProd = $"//*[local-name()='PROD']";

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

        private readonly string jsonForNewLicence = @"{""specversion"": ""1.0"",""type"": ""uk.gov.ukho.shop.recordOfSale.v1"",""source"": ""https://uk.gov.ukho.shop"",""id"": ""e744fa37-0c9f-4795-adc9-7f42ad8f11c1"",""time"": ""2023-07-20T10:40:00.000000Z"",""subject"": ""releasability set changes holdings Record of Sale"",""datacontenttype"": ""application/json"",""data"": {""correlationId"": ""123-abc-456-xyz-333"",""relatedEvents"": [""e744fa37-0c9f-4795-adc9-7f42ad8f11c1""],""recordsOfSale"": 
      {""licenseId"": ""2"",""productType"": ""AVCS"",""transactionType"": ""NEWLICENCE"",""distributorCustomerNumber"": ""111"",""shippingCoNumber"": ""1"",""ordernumber"": ""5432796"",""orderDate"": ""2023-06-20"",""po-ref"": ""75277T-Bengang"",""holdingsExpiryDate"": ""2025-06-30"",""sapId"": """",""vesselName"": ""Cornelia Maersk"",""imoNumber"": ""IMO9245756"",""callSign"": ""OWWS2"",""licenceType"": ""02"",""shoreBased"": ""X"",""fleetName"": """",""numberLicenceUsers"": 1,""upn"": ""MARIS"",""licenceDuration"": 12,""unitsOfSale"": [{""unitName"": ""PT111101"",""endDate"": ""2023-10-31"",""duration"": ""3"",""renew"": ""N"",""repeat"": """"},{""unitName"": ""GB302409"",""endDate"": ""2023-12-01"",""duration"": ""6"",""renew"": ""E"",""repeat"": ""P""}]}}}";

        #endregion

        [SetUp]
        public void Setup()
        {
            _fakeLogger = A.Fake<ILogger<RecordOfSaleSapMessageBuilder>>();
            _fakeXmlHelper = A.Fake<IXmlHelper>();
            _fakeFileSystemHelper = A.Fake<IFileSystemHelper>();
            _fakeRecordOfSaleSapMessageBuilder = new RecordOfSaleSapMessageBuilder(_fakeLogger, _fakeXmlHelper, _fakeFileSystemHelper);
        }

        [Test]
        public void WhenTransactionTypeIsNewLicence_ThenReturnXMLDocument()
        {
            var jsonData = JsonConvert.DeserializeObject<RecordOfSaleEventPayLoad>(jsonForNewLicence);
            var correlationId = "123-abc-456-xyz-333";
            var sapReqXml = TestHelper.ReadFileData("ERPTestData\\NewLicencePayloadTest.xml");

            XmlDocument soapXml = new();
            soapXml.LoadXml(RosSapXmlFile);

            A.CallTo(() => _fakeFileSystemHelper.IsFileExists(A<string>.Ignored)).Returns(true);
            A.CallTo(() => _fakeXmlHelper.CreateXmlDocument(A<string>.Ignored)).Returns(soapXml);
            A.CallTo(() => _fakeXmlHelper.CreateXmlPayLoad(A<SapRecordOfSalePayLoad>.Ignored)).Returns(sapReqXml);

            var result = _fakeRecordOfSaleSapMessageBuilder.BuildRecordOfSaleSapMessageXml(jsonData!, correlationId);

            result.Should().BeOfType<XmlDocument>();

            var actionItem = result.SelectSingleNode(XpathZAddsRos);
            actionItem.ChildNodes.Count.Should().Be(1);
            actionItem.ChildNodes[0].ChildNodes.Count.Should().Be(21);

            var licNo = result.SelectSingleNode(XpathLicNo);
            var fleet = result.SelectSingleNode(XpathFleet);
            licNo.InnerXml.Should().BeEmpty();
            fleet.InnerXml.Should().BeEmpty();

            var prodItem = result.SelectSingleNode(XpathProd);
            prodItem.ChildNodes.Count.Should().Be(2);
            prodItem.ChildNodes[0].ChildNodes.Count.Should().Be(5);

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
                                                && call.GetArgument<LogLevel>(0) == LogLevel.Information
                                                && call.GetArgument<EventId>(1) == EventIds.CreatingRecordOfSaleSapPayload.ToEventId()
                                                && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Creating the record of sale SAP Payload.").MustHaveHappenedOnceExactly();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
                                                && call.GetArgument<LogLevel>(0) == LogLevel.Information
                                                && call.GetArgument<EventId>(1) == EventIds.CreatedRecordOfSaleSapPayload.ToEventId()
                                                && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "The record of sale SAP payload created.").MustHaveHappenedOnceExactly();
        }

        [Test]
        public void WhenRecordOfSaleSapXmlTemplateFileNotExist_ThenThrowFileNotFoundException()
        {
            var jsonData = JsonConvert.DeserializeObject<RecordOfSaleEventPayLoad>(jsonForNewLicence);
            var correlationId = "123-abc-456-xyz-333";

            A.CallTo(() => _fakeFileSystemHelper.IsFileExists(A<string>.Ignored)).Returns(false);

            Assert.Throws<FileNotFoundException>(() => _fakeRecordOfSaleSapMessageBuilder.BuildRecordOfSaleSapMessageXml(jsonData!, correlationId));

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
                                                && call.GetArgument<LogLevel>(0) == LogLevel.Error
                                                && call.GetArgument<EventId>(1) == EventIds.RecordOfSaleSapXmlTemplateNotFound.ToEventId()
                                                && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "The record of sale SAP message xml template does not exist.").MustHaveHappenedOnceExactly();
        }

        [Test]
        public void WhenTransactionTypeIsNewLicence_ThenReturns_SomeFieldsEmptyInSapXmlPayloadCreationTests()
        {
            var jsonData = JsonConvert.DeserializeObject<RecordOfSaleEventPayLoad>(jsonForNewLicence);
            var sapReqXml = TestHelper.ReadFileData("ERPTestData\\NewLicencePayloadTest.xml");

            A.CallTo(() => _fakeXmlHelper.CreateXmlPayLoad(A<SapRecordOfSalePayLoad>.Ignored)).Returns(sapReqXml);

            MethodInfo methodInfo = typeof(RecordOfSaleSapMessageBuilder).GetMethod("SapXmlPayloadCreation", BindingFlags.NonPublic | BindingFlags.Instance)!;
            var result = (SapRecordOfSalePayLoad)methodInfo.Invoke(_fakeRecordOfSaleSapMessageBuilder, new object[] { jsonData! })!;

            result.Should().NotBeNull();
            result.CorrelationId.Should().Be("123-abc-456-xyz-333");
            result.PROD.UnitOfSales.Count.Should().Be(2);
            result.LicenceNumber.Should().Be("");
            result.FleetName.Should().Be("");
            result.PROD.UnitOfSales[0].Repeat.Should().Be("");
        }
    }
}
