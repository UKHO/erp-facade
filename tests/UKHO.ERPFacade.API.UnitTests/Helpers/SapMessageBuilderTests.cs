using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using UKHO.ERPFacade.API.Helpers;
using UKHO.ERPFacade.API.Models;
using UKHO.ERPFacade.Common.IO;
using UKHO.ERPFacade.Common.Logging;

namespace UKHO.ERPFacade.API.UnitTests.Helpers
{
    [TestFixture]
    public class SapMessageBuilderTests
    {
        private IXmlHelper _fakeXmlHelper;
        private IFileSystemHelper _fakeFileSystemHelper;
        private IOptions<SapActionConfiguration> _fakeSapActionConfig;
        private IOptions<ActionNumberConfiguration> _fakeActionNumberConfig;
        private ILogger<SapMessageBuilder> _fakeLogger;

        private SapMessageBuilder _fakeSapMessageBuilder;

        private readonly string sapXmlFile = @"<?xml version=""1.0"" encoding=""utf-8""?>
<soap:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">
  <soap:Body>
    <Z_ADDS_MAT_INFO xmlns=""urn:sap-com:document:sap:rfc:functions"">
      <Z_ADDS_MAT_INFO>
        <IM_MATINFO xmlns="""">
          <CORRID></CORRID>
          <NOOFACTIONS></NOOFACTIONS>
          <RECDATE></RECDATE>
          <RECTIME></RECTIME>
          <ORG>UKHO</ORG>
          <ACTIONITEMS xmlns=""urn:sap-com:document:sap:rfc:functions"">
          </ACTIONITEMS>
        </IM_MATINFO>
      </Z_ADDS_MAT_INFO>
    </Z_ADDS_MAT_INFO>
  </soap:Body>
</soap:Envelope>
";

        #region scenariosData

        private readonly string scenariosDataCancelReplaceCell = @"[{""ScenarioType"":2,""IsCellReplaced"":true,""Product"":{""productType"":""ENC S57"",""dataSetName"":""US5AK83M.001"",""productName"":""US5AK83M"",""title"":""St. Michael Bay"",""scale"":90000,""usageBand"":5,""editionNumber"":0,""updateNumber"":1,""mayAffectHoldings"":true,""contentChanged"":true,""permit"":""permitString"",""providerCode"":""1"",""providerDesc"":""ICE"",""size"":""small"",""agency"":""US"",""bundle"":[{""bundleType"":""DVD"",""location"":""M1;B1""}],""status"":{""statusName"":""Cancellation Update"",""statusDate"":""2023-03-03T04:30:00+05:30"",""isNewCell"":false},""replaces"":[],""replacedBy"":[""US4AK6NT"",""US4AK6NU""],""additionalCoverage"":[],""geographicLimit"":{""boundingBox"":{""northLimit"":24.0,""southLimit"":22.0,""eastLimit"":120.0,""westLimit"":119.0},""polygons"":[{""polygon"":[{""latitude"":24.0,""longitude"":121.5},{""latitude"":121.0,""longitude"":56.0},{""latitude"":45.0,""longitude"":78.0},{""latitude"":119.0,""longitude"":121.5}]},{""polygon"":[{""latitude"":24.0,""longitude"":121.5},{""latitude"":121.0,""longitude"":56.0},{""latitude"":45.0,""longitude"":78.0},{""latitude"":119.0,""longitude"":121.5}]}]},""inUnitsOfSale"":[""US5AK83M"",""AVCSO"",""PAYSF""],""s63"":{""name"":""US5AK83M.001"",""hash"":""5160204288db0a543850da177ec637b71ed55946e7f3843e6180b1906ee4f4ea"",""location"":""8198201e-78ce-4af8-9145-ad68ba0472e2"",""fileSize"":""4500"",""compression"":true,""s57Crc"":""5C06E104""},""signature"":{""name"":""US5AK83M.001"",""hash"":""fd0c9e5f95c0b664a1a27c2ff98812c648ebd113b117f5639f74536c397fbac9"",""location"":""0ecf2f38-a876-4d77-bd0e-0d901d3a0e73"",""fileSize"":""2500""},""ancillaryFiles"":[{""name"":""GB123_04.TXT"",""hash"":""d030b93ad8a0801e6955e4f52599f6200d01310155882a1b80eec78c9b93662b"",""location"":""2a29932e-dcb8-4e3a-b8a2-b3cfc335ede5"",""fileSize"":""1240""},{""name"":""GB125_01.TXT"",""hash"":""bb8042082bd1d37236801837585fa0df5e96097fb8d2281b41888af2b23ceb0b"",""location"":""1ad0f9c3-8c93-495a-99a1-06a36410faa9"",""fileSize"":""1360""},{""name"":""GB162_01.TXT"",""hash"":""81470666f387a9035f6b33f59fb9bbf0872e9c296fecee58c4e919d6a1d87ab6"",""location"":""eb443aad-394c-4eb0-b391-415a261605a1"",""fileSize"":""1360""}]},""InUnitOfSales"":[""US5AK83M"",""AVCSO"",""PAYSF""],""UnitOfSales"":[{""unitName"":""US5AK83M"",""title"":""St. Michael Bay"",""unitOfSaleType"":""unit"",""unitSize"":""large"",""unitType"":""AVCS Units Coastal"",""status"":""NotForSale"",""isNewUnitOfSale"":false,""geographicLimit"":{""boundingBox"":{""northLimit"":24.146815,""southLimit"":22.581615,""eastLimit"":120.349635,""westLimit"":119.39142},""polygons"":[]},""compositionChanges"":{""addProducts"":[],""removeProducts"":[""US5AK83M""]}},{""unitName"":""AVCSO"",""title"":""AVCS Online Folio Some Title"",""unitOfSaleType"":""folio"",""unitSize"":""large"",""unitType"":""AVCS Online Folio"",""status"":""ForSale"",""isNewUnitOfSale"":false,""geographicLimit"":{""boundingBox"":{""northLimit"":0.0,""southLimit"":0.0,""eastLimit"":0.0,""westLimit"":0.0},""polygons"":[{""polygon"":[{""latitude"":22.0,""longitude"":120.0},{""latitude"":22.0,""longitude"":120.0},{""latitude"":22.0,""longitude"":120.0},{""latitude"":22.0,""longitude"":121.0}]}]},""compositionChanges"":{""addProducts"":[""US4AK6NT"",""US4AK6NU""],""removeProducts"":[""US5AK83M""]}},{""unitName"":""PAYSF"",""title"":""World Folio"",""unitOfSaleType"":""folio"",""unitSize"":""large"",""unitType"":""AVCS Folio Transit"",""status"":""ForSale"",""isNewUnitOfSale"":false,""geographicLimit"":{""boundingBox"":{""northLimit"":0.0,""southLimit"":0.0,""eastLimit"":0.0,""westLimit"":0.0},""polygons"":[{""polygon"":[{""latitude"":89.0,""longitude"":-179.995},{""latitude"":89.0,""longitude"":0.0},{""latitude"":89.0,""longitude"":179.995},{""latitude"":-89.0,""longitude"":179.995},{""latitude"":-89.0,""longitude"":0.0},{""latitude"":-89.0,""longitude"":-179.995},{""latitude"":89.0,""longitude"":-179.995}]}]},""compositionChanges"":{""addProducts"":[""US4AK6NT"",""US4AK6NU""],""removeProducts"":[""US5AK83M""]}}]},{""ScenarioType"":1,""IsCellReplaced"":false,""Product"":{""productType"":""ENC S57"",""dataSetName"":""US4AK6NT.001"",""productName"":""US4AK6NT"",""title"":""Norton Sound - Alaska"",""scale"":12000,""usageBand"":6,""editionNumber"":8,""updateNumber"":2,""mayAffectHoldings"":true,""contentChanged"":true,""permit"":""4A4E096BC7SDSAFEQAE71194324"",""providerCode"":""1"",""providerDesc"":""ICE"",""size"":""medium"",""agency"":""US"",""bundle"":[{""bundleType"":""DVD"",""location"":""M1;B1""}],""status"":{""statusName"":""New Edition"",""statusDate"":""2023-03-03T04:30:00+05:30"",""isNewCell"":true},""replaces"":[""US5AK83M""],""replacedBy"":[],""additionalCoverage"":[],""geographicLimit"":{""boundingBox"":{""northLimit"":63.0,""southLimit"":63.0,""eastLimit"":162.0,""westLimit"":161.0},""polygons"":[{""polygon"":[{""latitude"":22.57515,""longitude"":120.21948},{""latitude"":22.64326,""longitude"":120.0},{""latitude"":22.57154,""longitude"":120.0},{""latitude"":22.57515,""longitude"":121.21948}]}]},""inUnitsOfSale"":[""US4AK6NT"",""AVCSO"",""PAYSF""],""s63"":{""name"":""US4AK6NT.001"",""hash"":""5160204288db0a543850da177ec637b71ed55946e7f3843e6180b1906ee4f4ea"",""location"":""8198201e-78ce-4af8-9145-ad68ba0472e2"",""fileSize"":""4500"",""compression"":true,""s57Crc"":""5C06E104""},""signature"":{""name"":""US4AK6NT.001"",""hash"":""fd0c9e5f95c0b664a1a27c2ff98812c648ebd113b117f5639f74536c397fbac9"",""location"":""0ecf2f38-a876-4d77-bd0e-0d901d3a0e73"",""fileSize"":""2500""},""ancillaryFiles"":[{""name"":""US4AK6NT_04.TXT"",""hash"":""d030b93ad8a0801e6955e4f52599f6200d01310155882a1b80eec78c9b93662b"",""location"":""2a29932e-dcb8-4e3a-b8a2-b3cfc335ede5"",""fileSize"":""1240""}]},""InUnitOfSales"":[""US4AK6NT"",""AVCSO"",""PAYSF""],""UnitOfSales"":[{""unitName"":""US4AK6NT"",""title"":""Norton Sound - Alaska"",""unitOfSaleType"":""unit"",""unitSize"":""large"",""unitType"":""AVCS Units Coastal"",""status"":""ForSale"",""isNewUnitOfSale"":true,""geographicLimit"":{""boundingBox"":{""northLimit"":22.643255,""southLimit"":22.4625767,""eastLimit"":120.34972,""westLimit"":120.219475},""polygons"":[{""polygon"":[]}]},""compositionChanges"":{""addProducts"":[""US4AK6NT""],""removeProducts"":[]}},{""unitName"":""AVCSO"",""title"":""AVCS Online Folio Some Title"",""unitOfSaleType"":""folio"",""unitSize"":""large"",""unitType"":""AVCS Online Folio"",""status"":""ForSale"",""isNewUnitOfSale"":false,""geographicLimit"":{""boundingBox"":{""northLimit"":0.0,""southLimit"":0.0,""eastLimit"":0.0,""westLimit"":0.0},""polygons"":[{""polygon"":[{""latitude"":22.0,""longitude"":120.0},{""latitude"":22.0,""longitude"":120.0},{""latitude"":22.0,""longitude"":120.0},{""latitude"":22.0,""longitude"":121.0}]}]},""compositionChanges"":{""addProducts"":[""US4AK6NT"",""US4AK6NU""],""removeProducts"":[""US5AK83M""]}},{""unitName"":""PAYSF"",""title"":""World Folio"",""unitOfSaleType"":""folio"",""unitSize"":""large"",""unitType"":""AVCS Folio Transit"",""status"":""ForSale"",""isNewUnitOfSale"":false,""geographicLimit"":{""boundingBox"":{""northLimit"":0.0,""southLimit"":0.0,""eastLimit"":0.0,""westLimit"":0.0},""polygons"":[{""polygon"":[{""latitude"":89.0,""longitude"":-179.995},{""latitude"":89.0,""longitude"":0.0},{""latitude"":89.0,""longitude"":179.995},{""latitude"":-89.0,""longitude"":179.995},{""latitude"":-89.0,""longitude"":0.0},{""latitude"":-89.0,""longitude"":-179.995},{""latitude"":89.0,""longitude"":-179.995}]}]},""compositionChanges"":{""addProducts"":[""US4AK6NT"",""US4AK6NU""],""removeProducts"":[""US5AK83M""]}}]},{""ScenarioType"":1,""IsCellReplaced"":false,""Product"":{""productType"":""ENC S57"",""dataSetName"":""US4AK6NU.001"",""productName"":""US4AK6NU"",""title"":""Norton Sound - Alaska"",""scale"":12000,""usageBand"":6,""editionNumber"":8,""updateNumber"":2,""mayAffectHoldings"":true,""contentChanged"":true,""permit"":""4A4E096BC7SDSAFEQAE71194324"",""providerCode"":""1"",""providerDesc"":""ICE"",""size"":""medium"",""agency"":""US"",""bundle"":[{""bundleType"":""DVD"",""location"":""M1;B1""}],""status"":{""statusName"":""New Edition"",""statusDate"":""2023-03-03T04:30:00+05:30"",""isNewCell"":true},""replaces"":[""US5AK83M""],""replacedBy"":[],""additionalCoverage"":[],""geographicLimit"":{""boundingBox"":{""northLimit"":63.0,""southLimit"":63.0,""eastLimit"":162.0,""westLimit"":161.0},""polygons"":[{""polygon"":[{""latitude"":22.57515,""longitude"":120.21948},{""latitude"":22.64326,""longitude"":120.0},{""latitude"":22.57154,""longitude"":120.0},{""latitude"":22.57515,""longitude"":121.21948}]}]},""inUnitsOfSale"":[""US4AK6NU"",""AVCSO"",""PAYSF""],""s63"":{""name"":""US4AK6NU.001"",""hash"":""5160204288db0a543850da177ec637b71ed55946e7f3843e6180b1906ee4f4ea"",""location"":""8198201e-78ce-4af8-9145-ad68ba0472e2"",""fileSize"":""4500"",""compression"":true,""s57Crc"":""5C06E104""},""signature"":{""name"":""US4AK6NU.001"",""hash"":""fd0c9e5f95c0b664a1a27c2ff98812c648ebd113b117f5639f74536c397fbac9"",""location"":""0ecf2f38-a876-4d77-bd0e-0d901d3a0e73"",""fileSize"":""2500""},""ancillaryFiles"":[{""name"":""US4AK6NU_04.TXT"",""hash"":""d030b93ad8a0801e6955e4f52599f6200d01310155882a1b80eec78c9b93662b"",""location"":""2a29932e-dcb8-4e3a-b8a2-b3cfc335ede5"",""fileSize"":""1240""}]},""InUnitOfSales"":[""US4AK6NU"",""AVCSO"",""PAYSF""],""UnitOfSales"":[{""unitName"":""US4AK6NU"",""title"":""Norton Sound - Alaska"",""unitOfSaleType"":""unit"",""unitSize"":""large"",""unitType"":""AVCS Units Coastal"",""status"":""ForSale"",""isNewUnitOfSale"":true,""geographicLimit"":{""boundingBox"":{""northLimit"":22.643255,""southLimit"":22.4625767,""eastLimit"":120.34972,""westLimit"":120.219475},""polygons"":[{""polygon"":[]}]},""compositionChanges"":{""addProducts"":[""US4AK6NU""],""removeProducts"":[]}},{""unitName"":""AVCSO"",""title"":""AVCS Online Folio Some Title"",""unitOfSaleType"":""folio"",""unitSize"":""large"",""unitType"":""AVCS Online Folio"",""status"":""ForSale"",""isNewUnitOfSale"":false,""geographicLimit"":{""boundingBox"":{""northLimit"":0.0,""southLimit"":0.0,""eastLimit"":0.0,""westLimit"":0.0},""polygons"":[{""polygon"":[{""latitude"":22.0,""longitude"":120.0},{""latitude"":22.0,""longitude"":120.0},{""latitude"":22.0,""longitude"":120.0},{""latitude"":22.0,""longitude"":121.0}]}]},""compositionChanges"":{""addProducts"":[""US4AK6NT"",""US4AK6NU""],""removeProducts"":[""US5AK83M""]}},{""unitName"":""PAYSF"",""title"":""World Folio"",""unitOfSaleType"":""folio"",""unitSize"":""large"",""unitType"":""AVCS Folio Transit"",""status"":""ForSale"",""isNewUnitOfSale"":false,""geographicLimit"":{""boundingBox"":{""northLimit"":0.0,""southLimit"":0.0,""eastLimit"":0.0,""westLimit"":0.0},""polygons"":[{""polygon"":[{""latitude"":89.0,""longitude"":-179.995},{""latitude"":89.0,""longitude"":0.0},{""latitude"":89.0,""longitude"":179.995},{""latitude"":-89.0,""longitude"":179.995},{""latitude"":-89.0,""longitude"":0.0},{""latitude"":-89.0,""longitude"":-179.995},{""latitude"":89.0,""longitude"":-179.995}]}]},""compositionChanges"":{""addProducts"":[""US4AK6NT"",""US4AK6NU""],""removeProducts"":[""US5AK83M""]}}]}]";

        private readonly string scenariosDataChangeMoveCell = @"[{""ScenarioType"":4,""IsCellReplaced"":false,""Product"":{""productType"":""ENCS57"",""dataSetName"":""MX545010.001"",""productName"":""MX545010"",""title"":"""",""scale"":90000,""usageBand"":5,""editionNumber"":1,""updateNumber"":0,""mayAffectHoldings"":true,""contentChanged"":false,""permit"":""permitString"",""providerCode"":""1"",""providerDesc"":""ICE"",""size"":""medium"",""agency"":""MX"",""bundle"":[{""bundleType"":""DVD"",""location"":""M1;B1""}],""status"":{""statusName"":""New Edition"",""statusDate"":""2023-03-03T04:30:00+05:30"",""isNewCell"":false},""replaces"":[],""replacedBy"":[],""additionalCoverage"":[],""geographicLimit"":{""boundingBox"":{""northLimit"":24,""southLimit"":22,""eastLimit"":120,""westLimit"":119},""polygons"":[{""polygon"":[{""latitude"":24,""longitude"":121.5},{""latitude"":121,""longitude"":56},{""latitude"":45,""longitude"":78},{""latitude"":119,""longitude"":121.5}]},{""polygon"":[{""latitude"":24,""longitude"":121.5},{""latitude"":121,""longitude"":56},{""latitude"":45,""longitude"":78},{""latitude"":119,""longitude"":121.5}]}]},""inUnitsOfSale"":[""MX545010"",""MX509226"",""AVCSO"",""PAYSF""],""s63"":{""name"":""XXXXXXXX.001"",""hash"":""5160204288db0a543850da177ec637b71ed55946e7f3843e6180b1906ee4f4ea"",""location"":""8198201e-78ce-4af8-9145-ad68ba0472e2"",""fileSize"":""4500"",""compression"":true,""s57Crc"":""5C06E104""},""signature"":{""name"":""XXXXXXXX.001"",""hash"":""fd0c9e5f95c0b664a1a27c2ff98812c648ebd113b117f5639f74536c397fbac9"",""location"":""0ecf2f38-a876-4d77-bd0e-0d901d3a0e73"",""fileSize"":""2500""},""ancillaryFiles"":[{""name"":""GBXXXXXXXX_04.TXT"",""hash"":""d030b93ad8a0801e6955e4f52599f6200d01310155882a1b80eec78c9b93662b"",""location"":""2a29932e-dcb8-4e3a-b8a2-b3cfc335ede5"",""fileSize"":""1240""},{""name"":""GBXXXXXXXX_01.TXT"",""hash"":""bb8042082bd1d37236801837585fa0df5e96097fb8d2281b41888af2b23ceb0b"",""location"":""1ad0f9c3-8c93-495a-99a1-06a36410faa9"",""fileSize"":""1360""},{""name"":""GBXXXXXXXX_01.TXT"",""hash"":""81470666f387a9035f6b33f59fb9bbf0872e9c296fecee58c4e919d6a1d87ab6"",""location"":""eb443aad-394c-4eb0-b391-415a261605a1"",""fileSize"":""1360""}]},""InUnitOfSales"":[""MX545010"",""MX509226"",""AVCSO"",""PAYSF""],""UnitOfSales"":[{""unitName"":""MX545010"",""title"":""Isla Clarion"",""unitOfSaleType"":null,""unitSize"":""large"",""unitType"":""AVCS Units Coastal"",""status"":""ForSale"",""isNewUnitOfSale"":false,""geographicLimit"":{""boundingBox"":{""northLimit"":24.146815,""southLimit"":22.581615,""eastLimit"":120.349635,""westLimit"":119.39142},""polygons"":[]},""compositionChanges"":{""addProducts"":[],""removeProducts"":[""MX545010""]}},{""unitName"":""MX509226"",""title"":""Playa del Carmen"",""unitOfSaleType"":null,""unitSize"":""large"",""unitType"":""AVCS Units Coastal"",""status"":""ForSale"",""isNewUnitOfSale"":false,""geographicLimit"":{""boundingBox"":{""northLimit"":24.146815,""southLimit"":22.581615,""eastLimit"":120.349635,""westLimit"":119.39142},""polygons"":[]},""compositionChanges"":{""addProducts"":[""MX545010""],""removeProducts"":[]}},{""unitName"":""AVCSO"",""title"":""AVCS Online Folio Some Title"",""unitOfSaleType"":null,""unitSize"":""large"",""unitType"":""AVCS Online Folio"",""status"":""ForSale"",""isNewUnitOfSale"":false,""geographicLimit"":{""boundingBox"":{""northLimit"":0,""southLimit"":0,""eastLimit"":0,""westLimit"":0},""polygons"":[{""polygon"":[{""latitude"":22,""longitude"":120},{""latitude"":22,""longitude"":120},{""latitude"":22,""longitude"":120},{""latitude"":22,""longitude"":121}]}]},""compositionChanges"":{""addProducts"":[],""removeProducts"":[]}},{""unitName"":""PAYSF"",""title"":""World Folio"",""unitOfSaleType"":null,""unitSize"":""large"",""unitType"":""AVCS Folio Transit"",""status"":""ForSale"",""isNewUnitOfSale"":false,""geographicLimit"":{""boundingBox"":{""northLimit"":0,""southLimit"":0,""eastLimit"":0,""westLimit"":0},""polygons"":[{""polygon"":[{""latitude"":89,""longitude"":-179.995},{""latitude"":89,""longitude"":0},{""latitude"":89,""longitude"":179.995},{""latitude"":-89,""longitude"":179.995},{""latitude"":-89,""longitude"":0},{""latitude"":-89,""longitude"":-179.995},{""latitude"":89,""longitude"":-179.995}]}]},""compositionChanges"":{""addProducts"":[],""removeProducts"":[]}}]}]";

        #endregion scenariosData

        [SetUp]
        public void Setup()
        {
            _fakeLogger = A.Fake<ILogger<SapMessageBuilder>>();
            _fakeXmlHelper = A.Fake<IXmlHelper>();
            _fakeFileSystemHelper = A.Fake<IFileSystemHelper>();
            _fakeSapActionConfig = Options.Create(InitConfiguration().GetSection("SapActionConfiguration").Get<SapActionConfiguration>())!;
            _fakeActionNumberConfig = Options.Create(InitConfiguration().GetSection("ActionNumberConfiguration").Get<ActionNumberConfiguration>())!;
            _fakeSapMessageBuilder = new SapMessageBuilder(_fakeLogger, _fakeXmlHelper, _fakeFileSystemHelper, _fakeSapActionConfig, _fakeActionNumberConfig);
        }

        private IConfiguration InitConfiguration()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory() + @"/ConfigurationFiles")
                .AddJsonFile("ActionNumbers.json")
                .AddJsonFile("SapActions.json")
                .AddEnvironmentVariables()
                .Build();

            return config;
        }

        [Test]
        public void WhenBuildSapMessageXmlIsCalledWithCancelReplacecellScenario_ThenReturnXMLDocument()
        {
            var scenarios = JsonConvert.DeserializeObject<List<Scenario>>(scenariosDataCancelReplaceCell);
            var traceId = "2f03a25f-28b3-46ea-b009-5943250a9a41";

            XmlDocument soapXml = new();
            soapXml.LoadXml(sapXmlFile);

            A.CallTo(() => _fakeFileSystemHelper.IsFileExists(A<string>.Ignored)).Returns(true);
            A.CallTo(() => _fakeXmlHelper.CreateXmlDocument(A<string>.Ignored)).Returns(soapXml);

            var result = _fakeSapMessageBuilder.BuildSapMessageXml(scenarios!, traceId);

            result.Should().BeOfType<XmlDocument>();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
            && call.GetArgument<LogLevel>(0) == LogLevel.Information
            && call.GetArgument<EventId>(1) == EventIds.BuildingSapActionStarted.ToEventId()
            && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Building SAP actions for {Scenario} scenario.").MustHaveHappened();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
            && call.GetArgument<LogLevel>(0) == LogLevel.Information
            && call.GetArgument<EventId>(1) == EventIds.SapActionCreated.ToEventId()
            && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "SAP action {ActionName} created for {Scenario}.").MustHaveHappened();
        }

        [Test]
        public void WhenBuildSapMessageXmlIsCalledWithChangeMoveCellScenario_ThenReturnXMLDocument()
        {
            var scenarios = JsonConvert.DeserializeObject<List<Scenario>>(scenariosDataChangeMoveCell);
            var traceId = "2f03a25f-28b3-46ea-b009-5943250a9a41";

            XmlDocument soapXml = new();
            soapXml.LoadXml(sapXmlFile);

            A.CallTo(() => _fakeFileSystemHelper.IsFileExists(A<string>.Ignored)).Returns(true);
            A.CallTo(() => _fakeXmlHelper.CreateXmlDocument(A<string>.Ignored)).Returns(soapXml);

            var result = _fakeSapMessageBuilder.BuildSapMessageXml(scenarios!, traceId);

            result.Should().BeOfType<XmlDocument>();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
            && call.GetArgument<LogLevel>(0) == LogLevel.Information
            && call.GetArgument<EventId>(1) == EventIds.BuildingSapActionStarted.ToEventId()
            && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Building SAP actions for {Scenario} scenario.").MustHaveHappened();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
            && call.GetArgument<LogLevel>(0) == LogLevel.Information
            && call.GetArgument<EventId>(1) == EventIds.SapActionCreated.ToEventId()
            && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "SAP action {ActionName} created for {Scenario}.").MustHaveHappened();
        }

        [Test]
        public void WhenSapXmlTemplateFileNotExist_ThenThrowFileNotFoundException()
        {
            var scenarios = JsonConvert.DeserializeObject<List<Scenario>>(scenariosDataCancelReplaceCell);
            var traceId = "2f03a25f-28b3-46ea-b009-5943250a9a41";

            A.CallTo(() => _fakeFileSystemHelper.IsFileExists(A<string>.Ignored)).Returns(false);

            Assert.Throws<FileNotFoundException>(() => _fakeSapMessageBuilder.BuildSapMessageXml(scenarios!, traceId));

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
            && call.GetArgument<LogLevel>(0) == LogLevel.Error
            && call.GetArgument<EventId>(1) == EventIds.SapXmlTemplateNotFound.ToEventId()
            && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "The SAP message xml template does not exist.").MustHaveHappenedOnceExactly();
        }
    }
}