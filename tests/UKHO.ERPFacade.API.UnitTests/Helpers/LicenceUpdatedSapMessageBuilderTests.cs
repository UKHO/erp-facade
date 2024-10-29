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
using UKHO.ERPFacade.API.SapMessageBuilders;
using UKHO.ERPFacade.API.UnitTests.Common;
using UKHO.ERPFacade.Common.Logging;
using UKHO.ERPFacade.Common.Models;
using UKHO.ERPFacade.Common.Operations;
using UKHO.ERPFacade.Common.Operations.IO;

namespace UKHO.ERPFacade.API.UnitTests.SapMessageBuilders
{
    [TestFixture]
    public class LicenceUpdatedSapMessageBuilderTests
    {
        private ILogger<LicenceUpdatedSapMessageBuilder> _fakeLogger;
        private IXmlOperations _fakeXmlOperations;
        private IFileOperations _fakeFileOperations;

        private LicenceUpdatedSapMessageBuilder _fakeLicenceUpdatedSapMessageBuilder;
        private readonly string XpathZAddsRos = $"//*[local-name()='Z_ADDS_ROS']";
        private readonly string XpathStartDate = $"//*[local-name()='STARTDATE']";
        private readonly string XpathEndDate = $"//*[local-name()='ENDDATE']";
        private readonly string XpathLType = $"//*[local-name()='LTYPE']";
        private readonly string XpathLicDur = $"//*[local-name()='LICDUR']";
        private readonly string XpathPO = $"//*[local-name()='PO']";
        private readonly string XpathAdsOrdno = $"//*[local-name()='ADSORDNO']";
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

        private readonly string changeLicencePayload = @"{""specversion"": ""1.0"",""type"": ""uk.gov.ukho.licensing.licenceUpdated.v1"",""source"": ""https://uk.gov.ukho.licensing"",""id"": ""e744fa37-0c9f-4795-adc9-7f42ad8f11c1"",""time"": ""8/23/2023 7:34:28 AM"",""subject"": ""licence update changes that need to go to SAP Record of Sale via ERP Facade"",""datacontenttype"": ""application/json"",""data"": {""correlationId"": ""123-abc-456-xyz-333"",""license"": {
      ""licenseId"": ""2"",""productType"": ""AVCS"",""transactionType"": ""CHANGELICENCE"",""distributorCustomerNumber"": ""111"",""shippingCoNumber"": ""1"",""ordernumber"": """",""orderDate"": """",""po-ref"": """",""holdingsExpiryDate"": ""2024-59-31"",""sapId"": ""76611K"",""vesselName"": ""Vessel 000002"",""imoNumber"": ""IMO000002"",""callSign"": ""CALL000002"",""licenceType"": """",""shoreBased"": """",""fleetName"": ""emailnoreply@engineering.ukho.gov.uk"",""numberLicenceUsers"": 1,""ecdisManufacturerName"": ""MARIS"",""licenceDuration"": 12,
      ""unitsOfSale"": [{""unitName"": """",""endDate"": """",""duration"": """",""renew"": """",""repeat"": """"}]}}}";

        #endregion

        [SetUp]
        public void Setup()
        {
            _fakeLogger = A.Fake<ILogger<LicenceUpdatedSapMessageBuilder>>();
            _fakeXmlOperations = A.Fake<IXmlOperations>();
            _fakeFileOperations = A.Fake<IFileOperations>();
            _fakeLicenceUpdatedSapMessageBuilder = new LicenceUpdatedSapMessageBuilder(_fakeLogger, _fakeXmlOperations, _fakeFileOperations);
        }

        [Test]
        public void WhenTransactionTypeIsChangeLicence_ThenReturnXMLDocument()
        {
            var changeLicencePayloadJson = JsonConvert.DeserializeObject<LicenceUpdatedEventPayLoad>(changeLicencePayload);
            var correlationId = "123-abc-456-xyz-333";
            var sapReqXml = TestHelper.ReadFileData("ERPTestData\\ChangeLicencePayloadTest.xml");

            XmlDocument soapXml = new();
            soapXml.LoadXml(RosSapXmlFile);

            A.CallTo(() => _fakeFileOperations.IsFileExists(A<string>.Ignored)).Returns(true);
            A.CallTo(() => _fakeXmlOperations.CreateXmlDocument(A<string>.Ignored)).Returns(soapXml);
            A.CallTo(() => _fakeXmlOperations.CreateXmlPayLoad(A<SapRecordOfSalePayLoad>.Ignored)).Returns(sapReqXml);

            var result = _fakeLicenceUpdatedSapMessageBuilder.BuildLicenceUpdatedSapMessageXml(changeLicencePayloadJson!, correlationId);

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
            var changeLicencePayloadJson = JsonConvert.DeserializeObject<LicenceUpdatedEventPayLoad>(changeLicencePayload);
            var correlationId = "123-abc-456-xyz-333";

            A.CallTo(() => _fakeFileOperations.IsFileExists(A<string>.Ignored)).Returns(false);

            Assert.Throws<FileNotFoundException>(() => _fakeLicenceUpdatedSapMessageBuilder.BuildLicenceUpdatedSapMessageXml(changeLicencePayloadJson!, correlationId));

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
                                                && call.GetArgument<LogLevel>(0) == LogLevel.Error
                                                && call.GetArgument<EventId>(1) == EventIds.LicenceUpdatedSapXmlTemplateNotFound.ToEventId()
                                                && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "The licence updated SAP message xml template does not exist.").MustHaveHappenedOnceExactly();
        }

        [Test]
        public void WhenTransactionTypeIsChangeLicence_ThenReturns_SapXmlPayloadWithSomeEmptyFields()
        {
            var changeLicencePayloadJson = JsonConvert.DeserializeObject<LicenceUpdatedEventPayLoad>(changeLicencePayload);
            var sapReqXml = TestHelper.ReadFileData("ERPTestData\\ChangeLicencePayloadTest.xml");

            A.CallTo(() => _fakeXmlOperations.CreateXmlPayLoad(A<SapRecordOfSalePayLoad>.Ignored)).Returns(sapReqXml);

            MethodInfo methodInfo = typeof(LicenceUpdatedSapMessageBuilder).GetMethod("BuildChangeLicencePayload", BindingFlags.NonPublic | BindingFlags.Instance)!;
            var result = (SapRecordOfSalePayLoad)methodInfo.Invoke(_fakeLicenceUpdatedSapMessageBuilder, new object[] { changeLicencePayloadJson! })!;

            result.Should().NotBeNull();
            result.CorrelationId.Should().Be("123-abc-456-xyz-333");
            result.PROD.UnitOfSales.Count.Should().Be(1);
            result.OrderNumber.Should().Be("");
            result.StartDate.Should().Be("");
            result.PurachaseOrder.Should().Be("");
            result.EndDate.Should().Be("");
            result.LicenceType.Should().Be("");
            result.LicenceDuration.Should().Be(null);
            result.PROD.UnitOfSales[0].Id.Should().Be("");
            result.PROD.UnitOfSales[0].Duration.Should().Be("");
            result.PROD.UnitOfSales[0].EndDate.Should().Be("");
            result.PROD.UnitOfSales[0].ReNew.Should().Be("");
            result.PROD.UnitOfSales[0].Repeat.Should().Be("");
        }
    }
}
