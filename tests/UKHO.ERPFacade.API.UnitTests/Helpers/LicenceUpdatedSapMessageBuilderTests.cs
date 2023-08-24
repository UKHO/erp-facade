using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NUnit.Framework;
using UKHO.ERPFacade.API.Helpers;
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
        public void WhenBuildSapMessageXml_ThenReturnXMLDocument()
        {
            var jsonData = JsonConvert.DeserializeObject<RecordOfSaleEventPayLoad>(licenceUpdatedJsonData);
            var correlationId = "123-abc-456-xyz-333";

            XmlDocument soapXml = new();
            soapXml.LoadXml(RosSapXmlFile);

            A.CallTo(() => _fakeFileSystemHelper.IsFileExists(A<string>.Ignored)).Returns(true);
            A.CallTo(() => _fakeXmlHelper.CreateXmlDocument(A<string>.Ignored)).Returns(soapXml);

            var result = _fakeLicenceUpdatedSapMessageBuilder.BuildLicenceUpdatedSapMessageXml(jsonData!, correlationId);

            result.Should().BeOfType<XmlDocument>();

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
    }
}
