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
using UKHO.ERPFacade.API.Helpers;
using UKHO.ERPFacade.Common.Models;
using UKHO.ERPFacade.Common.IO;
using UKHO.ERPFacade.Common.Logging;
using Polly.Utilities;
using System.Reflection;
using UKHO.ERPFacade.API.UnitTests.Common;

namespace UKHO.ERPFacade.API.UnitTests.Helpers
{
    [TestFixture]
    public class SapMessageBuilderTests
    {
        private IXmlHelper _fakeXmlHelper;
        private IFileSystemHelper _fakeFileSystemHelper;
        private IOptions<SapActionConfiguration> _fakeSapActionConfig;
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

        private readonly string scenariosDataCancelReplaceCell = @"{""specversion"":""1.0"",""type"":""uk.gov.ukho.encpublishing.enccontentpublished.v2"",""source"":""https://encpublishing.ukho.gov.uk"",""id"":""2f03a25f-28b3-46ea-b009-5943250a9a41"",""time"":""2020-10-13T12:08:03.4880776Z"",""_COMMENT"":""A comma separated list of products"",""subject"":""US5AK83M,US4AK6NT,US4AK6NU"",""datacontenttype"":""application/json"",""data"":{""correlationId"":""XX00000XXXXXX00083"",""traceId"":""367ce4a4-1d62-4f56-b359-59e178d77100"",""products"":[{""productType"":""ENC S57"",""dataSetName"":""US5AK83M.001"",""productName"":""US5AK83M"",""title"":""St. Michael Bay"",""scale"":90000,""usageBand"":5,""editionNumber"":0,""updateNumber"":1,""mayAffectHoldings"":true,""contentChanged"":true,""permit"":""permitString"",""providerCode"":1,""providerDesc"":""ICE"",""_COMMENT"":""The units in which product is included, including any 1-1 unit"",""_ENUM"":[""large"",""medium"",""small""],""size"":""small"",""agency"":""US"",""bundle"":[{""bundleType"":""DVD"",""_ENUM"":[""DVD""],""location"":""M1;B1""}],""status"":{""statusName"":""Cancellation Update"",""_ENUM"":[""New Edition"",""Re-issue"",""Update"",""Cancellation Update"",""Withdrawn"",""Suspended""],""statusDate"":""2023-03-03T00:00:00.00+01:00"",""isNewCell"":false,""_COMMENT"":""A cell new to the service""},""replaces"":[],""replacedBy"":[""US4AK6NT"",""US4AK6NU""],""additionalCoverage"":[],""geographicLimit"":{""boundingBox"":{""northLimit"":24,""southLimit"":22,""eastLimit"":120,""westLimit"":119},""polygons"":[{""polygon"":[{""latitude"":24,""longitude"":121.5},{""latitude"":121,""longitude"":56},{""latitude"":45,""longitude"":78},{""latitude"":119,""longitude"":121.5}]},{""polygon"":[{""latitude"":24,""longitude"":121.5},{""latitude"":121,""longitude"":56},{""latitude"":45,""longitude"":78},{""latitude"":119,""longitude"":121.5}]}]},""inUnitsOfSale"":[""US5AK83M"",""AVCSO"",""PAYSF""],""s63"":{""name"":""US5AK83M.001"",""hash"":""5160204288db0a543850da177ec637b71ed55946e7f3843e6180b1906ee4f4ea"",""location"":""8198201e-78ce-4af8-9145-ad68ba0472e2"",""fileSize"":""4500"",""compression"":true,""s57Crc"":""5C06E104""},""signature"":{""name"":""US5AK83M.001"",""hash"":""fd0c9e5f95c0b664a1a27c2ff98812c648ebd113b117f5639f74536c397fbac9"",""location"":""0ecf2f38-a876-4d77-bd0e-0d901d3a0e73"",""fileSize"":""2500""},""ancillaryFiles"":[{""name"":""GB123_04.TXT"",""hash"":""d030b93ad8a0801e6955e4f52599f6200d01310155882a1b80eec78c9b93662b"",""location"":""2a29932e-dcb8-4e3a-b8a2-b3cfc335ede5"",""fileSize"":""1240""},{""name"":""GB125_01.TXT"",""hash"":""bb8042082bd1d37236801837585fa0df5e96097fb8d2281b41888af2b23ceb0b"",""location"":""1ad0f9c3-8c93-495a-99a1-06a36410faa9"",""fileSize"":""1360""},{""name"":""GB162_01.TXT"",""hash"":""81470666f387a9035f6b33f59fb9bbf0872e9c296fecee58c4e919d6a1d87ab6"",""location"":""eb443aad-394c-4eb0-b391-415a261605a1"",""fileSize"":""1360""}]},{""productType"":""ENC S57"",""dataSetName"":""US4AK6NT.001"",""productName"":""US4AK6NT"",""title"":""Norton Sound - Alaska"",""scale"":12000,""usageBand"":6,""editionNumber"":8,""updateNumber"":2,""mayAffectHoldings"":true,""contentChanged"":true,""permit"":""4A4E096BC7SDSAFEQAE71194324"",""providerCode"":1,""providerDesc"":""ICE"",""_COMMENT"":"" size code not included as will be in ERP Facade"",""_ENUM"":[""large"",""medium"",""small""],""size"":""medium"",""agency"":""US"",""bundle"":[{""bundleType"":""DVD"",""_ENUM"":[""DVD""],""location"":""M1;B1""}],""status"":{""statusName"":""New Edition"",""statusDate"":""2023-03-03T00:00:00.00+01:00"",""isNewCell"":true},""replaces"":[""US5AK83M""],""replacedBy"":[],""additionalCoverage"":[],""geographicLimit"":{""boundingBox"":{""northLimit"":63,""southLimit"":63,""eastLimit"":162,""westLimit"":161},""polygons"":[{""polygon"":[{""latitude"":22.57515,""longitude"":120.21948},{""latitude"":22.64326,""longitude"":120},{""latitude"":22.57154,""longitude"":120},{""latitude"":22.57515,""longitude"":121.21948}]}]},""inUnitsOfSale"":[""US4AK6NT"",""AVCSO"",""PAYSF""],""s63"":{""name"":""US4AK6NT.001"",""hash"":""5160204288db0a543850da177ec637b71ed55946e7f3843e6180b1906ee4f4ea"",""location"":""8198201e-78ce-4af8-9145-ad68ba0472e2"",""fileSize"":""4500"",""compression"":true,""s57Crc"":""5C06E104""},""signature"":{""name"":""US4AK6NT.001"",""hash"":""fd0c9e5f95c0b664a1a27c2ff98812c648ebd113b117f5639f74536c397fbac9"",""location"":""0ecf2f38-a876-4d77-bd0e-0d901d3a0e73"",""fileSize"":""2500""},""ancillaryFiles"":[{""name"":""US4AK6NT_04.TXT"",""hash"":""d030b93ad8a0801e6955e4f52599f6200d01310155882a1b80eec78c9b93662b"",""location"":""2a29932e-dcb8-4e3a-b8a2-b3cfc335ede5"",""fileSize"":""1240""}]},{""productType"":""ENC S57"",""dataSetName"":""US4AK6NU.001"",""productName"":""US4AK6NU"",""title"":""Norton Sound - Alaska"",""scale"":12000,""usageBand"":6,""editionNumber"":8,""updateNumber"":2,""mayAffectHoldings"":true,""contentChanged"":true,""permit"":""4A4E096BC7SDSAFEQAE71194324"",""providerCode"":1,""providerDesc"":""ICE"",""_COMMENT"":"" size code not included as will be in ERP Facade"",""_ENUM"":[""large"",""medium"",""small""],""size"":""medium"",""agency"":""US"",""bundle"":[{""bundleType"":""DVD"",""_ENUM"":[""DVD""],""location"":""M1;B1""}],""status"":{""statusName"":""New Edition"",""statusDate"":""2023-03-03T00:00:00.00+01:00"",""isNewCell"":true},""replaces"":[""US5AK83M""],""replacedBy"":[],""additionalCoverage"":[],""geographicLimit"":{""boundingBox"":{""northLimit"":63,""southLimit"":63,""eastLimit"":162,""westLimit"":161},""polygons"":[{""polygon"":[{""latitude"":22.57515,""longitude"":120.21948},{""latitude"":22.64326,""longitude"":120},{""latitude"":22.57154,""longitude"":120},{""latitude"":22.57515,""longitude"":121.21948}]}]},""inUnitsOfSale"":[""US4AK6NU"",""AVCSO"",""PAYSF""],""s63"":{""name"":""US4AK6NU.001"",""hash"":""5160204288db0a543850da177ec637b71ed55946e7f3843e6180b1906ee4f4ea"",""location"":""8198201e-78ce-4af8-9145-ad68ba0472e2"",""fileSize"":""4500"",""compression"":true,""s57Crc"":""5C06E104""},""signature"":{""name"":""US4AK6NU.001"",""hash"":""fd0c9e5f95c0b664a1a27c2ff98812c648ebd113b117f5639f74536c397fbac9"",""location"":""0ecf2f38-a876-4d77-bd0e-0d901d3a0e73"",""fileSize"":""2500""},""ancillaryFiles"":[{""name"":""US4AK6NU_04.TXT"",""hash"":""d030b93ad8a0801e6955e4f52599f6200d01310155882a1b80eec78c9b93662b"",""location"":""2a29932e-dcb8-4e3a-b8a2-b3cfc335ede5"",""fileSize"":""1240""}]}],""_COMMENT"":""Prices for all units in event will be included, including Cancelled Cell"",""unitsOfSale"":[{""unitName"":""US5AK83M"",""title"":""St. Michael Bay"",""unitOfSaleType"":""unit"",""_ENUM"":[""ForSale"",""NotForSale""],""unitSize"":""large"",""unitType"":""AVCS Units Coastal"",""status"":""NotForSale"",""isNewUnitOfSale"":false,""_COMMENT"":""BoundingBox or polygon or both or neither"",""geographicLimit"":{""boundingBox"":{""northLimit"":24.146815,""southLimit"":22.581615,""eastLimit"":120.349635,""westLimit"":119.39142},""polygons"":[]},""compositionChanges"":{""addProducts"":[],""removeProducts"":[""US5AK83M""]}},{""unitName"":""US4AK6NT"",""title"":""Norton Sound - Alaska"",""unitOfSaleType"":""unit"",""_ENUM"":[""unit"",""folio""],""unitSize"":""large"",""unitType"":""AVCS Units Coastal"",""status"":""ForSale"",""isNewUnitOfSale"":true,""_COMMENT"":""avcs_cat has either boundingBox or polygon - should both be included?"",""geographicLimit"":{""boundingBox"":{""northLimit"":22.643255,""southLimit"":22.4625767,""eastLimit"":120.34972,""westLimit"":120.219475},""polygons"":[{""polygon"":[]}]},""compositionChanges"":{""addProducts"":[""US4AK6NT""],""removeProducts"":[]}},{""unitName"":""US4AK6NU"",""title"":""Norton Sound - Alaska"",""unitOfSaleType"":""unit"",""_ENUM"":[""unit"",""folio""],""unitSize"":""large"",""unitType"":""AVCS Units Coastal"",""status"":""ForSale"",""isNewUnitOfSale"":true,""_COMMENT"":""avcs_cat has either boundingBox or polygon - should both be included?"",""geographicLimit"":{""boundingBox"":{""northLimit"":22.643255,""southLimit"":22.4625767,""eastLimit"":120.34972,""westLimit"":120.219475},""polygons"":[{""polygon"":[]}]},""compositionChanges"":{""addProducts"":[""US4AK6NU""],""removeProducts"":[]}},{""unitName"":""AVCSO"",""title"":""AVCS Online Folio Some Title"",""unitOfSaleType"":""folio"",""_ENUM"":[""unit"",""folio""],""unitSize"":""large"",""unitType"":""AVCS Online Folio"",""status"":""ForSale"",""isNewUnitOfSale"":false,""geographicLimit"":{""boundingBox"":{},""polygons"":[{""polygon"":[{""latitude"":22,""longitude"":120},{""latitude"":22,""longitude"":120},{""latitude"":22,""longitude"":120},{""latitude"":22,""longitude"":121}]}]},""compositionChanges"":{""addProducts"":[""US4AK6NT"",""US4AK6NU""],""removeProducts"":[""US5AK83M""]}},{""unitName"":""PAYSF"",""title"":""World Folio"",""unitSize"":""large"",""unitType"":""AVCS Folio Transit"",""unitOfSaleType"":""folio"",""_ENUM"":[""unit"",""folio""],""status"":""ForSale"",""isNewUnitOfSale"":false,""geographicLimit"":{""boundingBox"":{},""polygons"":[{""polygon"":[{""latitude"":89,""longitude"":-179.995},{""latitude"":89,""longitude"":0},{""latitude"":89,""longitude"":179.995},{""latitude"":-89,""longitude"":179.995},{""latitude"":-89,""longitude"":0},{""latitude"":-89,""longitude"":-179.995},{""latitude"":89,""longitude"":-179.995}]}]},""compositionChanges"":{""addProducts"":[""US4AK6NT"",""US4AK6NU""],""removeProducts"":[""US5AK83M""]}}]}}";

        private readonly string scenariosDataChangeMoveCell = @"{""specversion"":""1.0"",""type"":""uk.gov.ukho.encpublishing.enccontentpublished.v2"",""source"":""https://encpublishing.ukho.gov.uk"",""id"":""2f03a25f-28b3-46ea-b009-5943250a9a41"",""time"":""2020-10-13T12:08:03.4880776Z"",""_COMMENT"":""A comma separated list of products"",""subject"":""MX545010"",""datacontenttype"":""application/json"",""data"":{""correlationId"":""XX00000XXXXXX00054"",""traceId"":""367ce4a4-1d62-4f56-b359-59e178d77100"",""products"":[{""productType"":""ENC S57"",""dataSetName"":""MX545010.001"",""productName"":""MX545010"",""title"":""Isla Clarion"",""scale"":90000,""usageBand"":5,""editionNumber"":1,""updateNumber"":0,""mayAffectHoldings"":true,""contentChanged"":false,""permit"":""permitString"",""providerCode"":1,""providerDesc"":""ICE"",""_COMMENT"":""The units in which product is included, including any 1-1 unit"",""_ENUM"":[""large"",""medium"",""small""],""size"":""medium"",""agency"":""MX"",""bundle"":[{""bundleType"":""DVD"",""_ENUM"":[""DVD""],""location"":""M1;B1""}],""status"":{""statusName"":""New Edition"",""_ENUM"":[""New Edition"",""Re-issue"",""Update"",""Cancellation Update"",""Withdrawn"",""Suspended""],""statusDate"":""2023-03-03T00:00:00.00+01:00"",""isNewCell"":false,""_COMMENT"":""A cell new to the service""},""replaces"":[],""replacedBy"":[],""additionalCoverage"":[],""geographicLimit"":{""boundingBox"":{""northLimit"":24,""southLimit"":22,""eastLimit"":120,""westLimit"":119},""polygons"":[{""polygon"":[{""latitude"":24,""longitude"":121.5},{""latitude"":121,""longitude"":56},{""latitude"":45,""longitude"":78},{""latitude"":119,""longitude"":121.5}]},{""polygon"":[{""latitude"":24,""longitude"":121.5},{""latitude"":121,""longitude"":56},{""latitude"":45,""longitude"":78},{""latitude"":119,""longitude"":121.5}]}]},""inUnitsOfSale"":[""MX545010"",""MX509226"",""AVCSO"",""PAYSF""],""s63"":{""name"":""XXXXXXXX.001"",""hash"":""5160204288db0a543850da177ec637b71ed55946e7f3843e6180b1906ee4f4ea"",""location"":""8198201e-78ce-4af8-9145-ad68ba0472e2"",""fileSize"":""4500"",""compression"":true,""s57Crc"":""5C06E104""},""signature"":{""name"":""XXXXXXXX.001"",""hash"":""fd0c9e5f95c0b664a1a27c2ff98812c648ebd113b117f5639f74536c397fbac9"",""location"":""0ecf2f38-a876-4d77-bd0e-0d901d3a0e73"",""fileSize"":""2500""},""ancillaryFiles"":[{""name"":""GBXXXXXXXX_04.TXT"",""hash"":""d030b93ad8a0801e6955e4f52599f6200d01310155882a1b80eec78c9b93662b"",""location"":""2a29932e-dcb8-4e3a-b8a2-b3cfc335ede5"",""fileSize"":""1240""},{""name"":""GBXXXXXXXX_01.TXT"",""hash"":""bb8042082bd1d37236801837585fa0df5e96097fb8d2281b41888af2b23ceb0b"",""location"":""1ad0f9c3-8c93-495a-99a1-06a36410faa9"",""fileSize"":""1360""},{""name"":""GBXXXXXXXX_01.TXT"",""hash"":""81470666f387a9035f6b33f59fb9bbf0872e9c296fecee58c4e919d6a1d87ab6"",""location"":""eb443aad-394c-4eb0-b391-415a261605a1"",""fileSize"":""1360""}]}],""_COMMENT"":""Prices for all units in event will be included, including Cancelled Cell"",""unitsOfSale"":[{""unitName"":""MX545010"",""title"":""Isla Clarion"",""unitOfSaleType"":""unit"",""unitSize"":""large"",""unitType"":""AVCS Units Coastal"",""status"":""ForSale"",""_ENUM"":[""ForSale"",""NotForSale""],""isNewUnitOfSale"":false,""_COMMENT"":""BoundingBox or polygon or both or neither"",""geographicLimit"":{""boundingBox"":{""northLimit"":24.146815,""southLimit"":22.581615,""eastLimit"":120.349635,""westLimit"":119.39142},""polygons"":[]},""compositionChanges"":{""addProducts"":[],""removeProducts"":[""MX545010""]}},{""unitName"":""MX509226"",""title"":""Playa del Carmen"",""unitOfSaleType"":""unit"",""unitSize"":""large"",""unitType"":""AVCS Units Coastal"",""status"":""ForSale"",""_ENUM"":[""ForSale"",""NotForSale""],""isNewUnitOfSale"":false,""_COMMENT"":""BoundingBox or polygon or both or neither"",""geographicLimit"":{""boundingBox"":{""northLimit"":24.146815,""southLimit"":22.581615,""eastLimit"":120.349635,""westLimit"":119.39142},""polygons"":[]},""compositionChanges"":{""addProducts"":[""MX545010""],""removeProducts"":[]}},{""unitName"":""AVCSO"",""title"":""AVCS Online Folio Some Title"",""unitOfSaleType"":""folio"",""unitSize"":""large"",""unitType"":""AVCS Online Folio"",""status"":""ForSale"",""isNewUnitOfSale"":false,""geographicLimit"":{""boundingBox"":{},""polygons"":[{""polygon"":[{""latitude"":22,""longitude"":120},{""latitude"":22,""longitude"":120},{""latitude"":22,""longitude"":120},{""latitude"":22,""longitude"":121}]}]},""compositionChanges"":{""addProducts"":[],""removeProducts"":[]}},{""unitName"":""PAYSF"",""title"":""World Folio"",""unitOfSaleType"":""folio"",""unitSize"":""large"",""unitType"":""AVCS Folio Transit"",""status"":""ForSale"",""isNewUnitOfSale"":false,""geographicLimit"":{""boundingBox"":{},""polygons"":[{""polygon"":[{""latitude"":89,""longitude"":-179.995},{""latitude"":89,""longitude"":0},{""latitude"":89,""longitude"":179.995},{""latitude"":-89,""longitude"":179.995},{""latitude"":-89,""longitude"":0},{""latitude"":-89,""longitude"":-179.995},{""latitude"":89,""longitude"":-179.995}]}]},""compositionChanges"":{""addProducts"":[],""removeProducts"":[]}}]}}";

        private readonly string scenariosDataSimpleUpdate = @"{""_COMMENT"":""A comma separated list of products"",""specversion"":""1.0"",""type"":""uk.gov.ukho.encpublishing.enccontentpublished.v2"",""source"":""https://encpublishing.ukho.gov.uk"",""id"":""2f03a25f-28b3-46ea-b009-5943250a9a41"",""time"":""2020-10-13T12:08:03.4880776Z"",""subject"":""US4FL1YE"",""datacontenttype"":""application/json"",""data"":{""correlationId"":""XX00000XXXXXX000YE"",""traceId"":""367ce4a4-1d62-4f56-b359-59e178d77100"",""products"":[{""productType"":""ENC S57"",""dataSetName"":""US4FL1YE.001"",""productName"":""US4FL1YE"",""title"":""Some Title"",""scale"":90000,""usageBand"":1,""editionNumber"":4,""updateNumber"":2,""mayAffectHoldings"":false,""contentChanged"":true,""permit"":""permitString"",""providerCode"":1,""providerDesc"":""ICE"",""_COMMENT"":""The units of sale in which product is included, including any 1-1 unit"",""_ENUM"":[""large"",""medium"",""small""],""size"":""medium"",""agency"":""US"",""bundle"":[{""bundleType"":""DVD"",""_ENUM"":[""DVD""],""location"":""M2;B1""}],""status"":{""statusName"":""Update"",""_ENUM"":[""New Edition"",""Re-issue"",""Update"",""Cancellation Update"",""Withdrawn"",""Suspended""],""statusDate"":""2020-07-16T19:20:30.45+01:00"",""isNewCell"":false,""_COMMENT"":""A cell new to the service""},""replaces"":[],""replacedBy"":[],""additionalCoverage"":[],""geographicLimit"":{""boundingBox"":{""northLimit"":24.146815,""southLimit"":22.581615,""eastLimit"":120.349635,""westLimit"":119.39142},""polygons"":[{""polygon"":[{""latitude"":24,""longitude"":121.5},{""latitude"":121,""longitude"":56},{""latitude"":45,""longitude"":78},{""latitude"":119,""longitude"":121.5}]},{""polygon"":[{""latitude"":24,""longitude"":121.5},{""latitude"":121,""longitude"":56},{""latitude"":45,""longitude"":78},{""latitude"":119,""longitude"":121.5}]}]},""inUnitsOfSale"":[""US4FL1YE"",""AVCSO"",""PAYSF""],""s63"":{""name"":""GB302409.001"",""hash"":""5160204288db0a543850da177ec637b71ed55946e7f3843e6180b1906ee4f4ea"",""location"":""8198201e-78ce-4af8-9145-ad68ba0472e2"",""fileSize"":""4500"",""compression"":true,""s57Crc"":""5C06E104""},""signature"":{""name"":""GB302409.001"",""hash"":""fd0c9e5f95c0b664a1a27c2ff98812c648ebd113b117f5639f74536c397fbac9"",""location"":""0ecf2f38-a876-4d77-bd0e-0d901d3a0e73"",""fileSize"":""2500""},""ancillaryFiles"":[{""name"":""GB123_04.TXT"",""hash"":""d030b93ad8a0801e6955e4f52599f6200d01310155882a1b80eec78c9b93662b"",""location"":""2a29932e-dcb8-4e3a-b8a2-b3cfc335ede5"",""fileSize"":""1240""},{""name"":""GB125_01.TXT"",""hash"":""bb8042082bd1d37236801837585fa0df5e96097fb8d2281b41888af2b23ceb0b"",""location"":""1ad0f9c3-8c93-495a-99a1-06a36410faa9"",""fileSize"":""1360""},{""name"":""GB162_01.TXT"",""hash"":""81470666f387a9035f6b33f59fb9bbf0872e9c296fecee58c4e919d6a1d87ab6"",""location"":""eb443aad-394c-4eb0-b391-415a261605a1"",""fileSize"":""1360""}]}],""_COMMENT"":""All units will be included, including Cancelled Cell units"",""unitsOfSale"":[{""unitName"":""US4FL1YE"",""title"":""Unit Title"",""unitOfSaleType"":""unit"",""_ENUM"":[""ForSale"",""NotForSale""],""unitSize"":""medium"",""_COMMENT"":""BoundingBox or polygon or both or neither"",""unitType"":""AVCS Units Coastal"",""status"":""ForSale"",""isNewUnitOfSale"":false,""geographicLimit"":{""boundingBox"":{""northLimit"":24.146815,""southLimit"":22.581615,""eastLimit"":120.349635,""westLimit"":119.39142},""polygons"":[]},""compositionChanges"":{""addProducts"":[],""removeProducts"":[]}},{""unitName"":""AVCSO"",""title"":""Title"",""unitOfSaleType"":""folio"",""_ENUM"":[""AVCS Units Overview"",""AVCS Units General"",""AVCS Units Coastal"",""AVCS Units Approach"",""AVCS Units Harbour"",""AVCS Units Berthing""],""unitSize"":""large"",""_COMMENT"":"" size code not included as will be mapped in SAP"",""unitType"":""AVCS Units Coastal"",""status"":""ForSale"",""isNewUnitOfSale"":false,""geographicLimit"":{""boundingBox"":{},""polygons"":[{""polygon"":[{""latitude"":22.57515,""longitude"":120.21948},{""latitude"":22.64326,""longitude"":120},{""latitude"":22.57154,""longitude"":120},{""latitude"":22.57515,""longitude"":121.21948}]}]},""compositionChanges"":{""addProducts"":[],""removeProducts"":[]}},{""unitName"":""PAYSF"",""title"":""World Folio"",""unitOfSaleType"":""folio"",""_ENUM"":[""AVCS Units Overview"",""AVCS Units General"",""AVCS Units Coastal"",""AVCS Units Approach"",""AVCS Units Harbour"",""AVCS Units Berthing""],""unitSize"":""large"",""_COMMENT"":"" size code not included as will be mapped in SAP"",""unitType"":""AVCS Units Coastal"",""status"":""ForSale"",""isNewUnitOfSale"":false,""geographicLimit"":{""boundingBox"":{},""polygons"":[{""polygon"":[{""latitude"":89,""longitude"":-179.995},{""latitude"":89,""longitude"":0},{""latitude"":89,""longitude"":179.995},{""latitude"":-89,""longitude"":179.995},{""latitude"":-89,""longitude"":0},{""latitude"":-89,""longitude"":-179.995},{""latitude"":89,""longitude"":-179.995}]}]},""compositionChanges"":{""addProducts"":[],""removeProducts"":[]}}]}}";

        #endregion scenariosData

        [SetUp]
        public void Setup()
        {
            _fakeLogger = A.Fake<ILogger<SapMessageBuilder>>();
            _fakeXmlHelper = A.Fake<IXmlHelper>();
            _fakeFileSystemHelper = A.Fake<IFileSystemHelper>();
            _fakeSapActionConfig = Options.Create(InitConfiguration().GetSection("SapActionConfiguration").Get<SapActionConfiguration>())!;
            _fakeSapMessageBuilder = new SapMessageBuilder(_fakeLogger, _fakeXmlHelper, _fakeFileSystemHelper, _fakeSapActionConfig);
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
        public void WhenBuildSapMessageXmlIsCalledWithCancelReplacecellScenario_ThenReturnXMLDocument()
        {
            var scenarios = JsonConvert.DeserializeObject<EncEventPayload>(scenariosDataCancelReplaceCell);
            var correlationId = "367ce4a4-1d62-4f56-b359-59e178d77100";

            string fakeXmlData = @"<?xml version=""1.0"" encoding=""utf-8""?><soap:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/""><soap:Body><Z_ADDS_MAT_INFO xmlns=""urn:sap-com:document:sap:rfc:functions""><Z_ADDS_MAT_INFO><IM_MATINFO xmlns=""""><CORRID>367ce4a4-1d62-4f56-b359-59e178d77100</CORRID><NOOFACTIONS>17</NOOFACTIONS><RECDATE>20230615</RECDATE><RECTIME>104457</RECTIME><ORG>UKHO</ORG><ACTIONITEMS xmlns=""urn:sap-com:document:sap:rfc:functions""><item xmlns=""""><ACTIONNUMBER>1</ACTIONNUMBER><ACTION>CREATE ENC CELL</ACTION><PRODUCT>ENC CELL</PRODUCT><PRODTYPE>S57</PRODTYPE><CHILDCELL>US4AK6NT</CHILDCELL><PRODUCTNAME>US4AK6NT</PRODUCTNAME><CANCELLED></CANCELLED><REPLACEDBY></REPLACEDBY><AGENCY>US</AGENCY><PROVIDER>1</PROVIDER><ENCSIZE>medium</ENCSIZE><TITLE>Norton Sound - Alaska</TITLE><EDITIONNO>8</EDITIONNO><UPDATENO>2</UPDATENO><UNITTYPE></UNITTYPE></item><item xmlns=""""><ACTIONNUMBER>2</ACTIONNUMBER><ACTION>CREATE ENC CELL</ACTION><PRODUCT>ENC CELL</PRODUCT><PRODTYPE>S57</PRODTYPE><CHILDCELL>US4AK6NU</CHILDCELL><PRODUCTNAME>US4AK6NU</PRODUCTNAME><CANCELLED></CANCELLED><REPLACEDBY></REPLACEDBY><AGENCY>US</AGENCY><PROVIDER>1</PROVIDER><ENCSIZE>medium</ENCSIZE><TITLE>Norton Sound - Alaska</TITLE><EDITIONNO>8</EDITIONNO><UPDATENO>2</UPDATENO><UNITTYPE></UNITTYPE></item><item xmlns=""""><ACTIONNUMBER>3</ACTIONNUMBER><ACTION>CREATE AVCS UNIT OF SALE</ACTION><PRODUCT>AVCS UNIT</PRODUCT><PRODTYPE>S57</PRODTYPE><CHILDCELL></CHILDCELL><PRODUCTNAME>US4AK6NT</PRODUCTNAME><CANCELLED></CANCELLED><REPLACEDBY></REPLACEDBY><AGENCY>US</AGENCY><PROVIDER>1</PROVIDER><ENCSIZE>large</ENCSIZE><TITLE>Norton Sound - Alaska</TITLE><EDITIONNO></EDITIONNO><UPDATENO></UPDATENO><UNITTYPE>AVCS Units Coastal</UNITTYPE></item><item xmlns=""""><ACTIONNUMBER>4</ACTIONNUMBER><ACTION>CREATE AVCS UNIT OF SALE</ACTION><PRODUCT>AVCS UNIT</PRODUCT><PRODTYPE>S57</PRODTYPE><CHILDCELL></CHILDCELL><PRODUCTNAME>US4AK6NU</PRODUCTNAME><CANCELLED></CANCELLED><REPLACEDBY></REPLACEDBY><AGENCY>US</AGENCY><PROVIDER>1</PROVIDER><ENCSIZE>large</ENCSIZE><TITLE>Norton Sound - Alaska</TITLE><EDITIONNO></EDITIONNO><UPDATENO></UPDATENO><UNITTYPE>AVCS Units Coastal</UNITTYPE></item><item xmlns=""""><ACTIONNUMBER>5</ACTIONNUMBER><ACTION>ASSIGN CELL TO AVCS UNIT OF SALE</ACTION><PRODUCT>AVCS UNIT</PRODUCT><PRODTYPE>S57</PRODTYPE><CHILDCELL>US4AK6NT</CHILDCELL><PRODUCTNAME>US4AK6NT</PRODUCTNAME><CANCELLED></CANCELLED><REPLACEDBY></REPLACEDBY><AGENCY></AGENCY><PROVIDER></PROVIDER><ENCSIZE></ENCSIZE><TITLE></TITLE><EDITIONNO></EDITIONNO><UPDATENO></UPDATENO><UNITTYPE></UNITTYPE></item><item xmlns=""""><ACTIONNUMBER>6</ACTIONNUMBER><ACTION>ASSIGN CELL TO AVCS UNIT OF SALE</ACTION><PRODUCT>AVCS UNIT</PRODUCT><PRODTYPE>S57</PRODTYPE><CHILDCELL>US4AK6NU</CHILDCELL><PRODUCTNAME>US4AK6NU</PRODUCTNAME><CANCELLED></CANCELLED><REPLACEDBY></REPLACEDBY><AGENCY></AGENCY><PROVIDER></PROVIDER><ENCSIZE></ENCSIZE><TITLE></TITLE><EDITIONNO></EDITIONNO><UPDATENO></UPDATENO><UNITTYPE></UNITTYPE></item><item xmlns=""""><ACTIONNUMBER>7</ACTIONNUMBER><ACTION>ASSIGN CELL TO AVCS UNIT OF SALE</ACTION><PRODUCT>AVCS UNIT</PRODUCT><PRODTYPE>S57</PRODTYPE><CHILDCELL>US4AK6NT</CHILDCELL><PRODUCTNAME>AVCSO</PRODUCTNAME><CANCELLED></CANCELLED><REPLACEDBY></REPLACEDBY><AGENCY></AGENCY><PROVIDER></PROVIDER><ENCSIZE></ENCSIZE><TITLE></TITLE><EDITIONNO></EDITIONNO><UPDATENO></UPDATENO><UNITTYPE></UNITTYPE></item><item xmlns=""""><ACTIONNUMBER>8</ACTIONNUMBER><ACTION>ASSIGN CELL TO AVCS UNIT OF SALE</ACTION><PRODUCT>AVCS UNIT</PRODUCT><PRODTYPE>S57</PRODTYPE><CHILDCELL>US4AK6NU</CHILDCELL><PRODUCTNAME>AVCSO</PRODUCTNAME><CANCELLED></CANCELLED><REPLACEDBY></REPLACEDBY><AGENCY></AGENCY><PROVIDER></PROVIDER><ENCSIZE></ENCSIZE><TITLE></TITLE><EDITIONNO></EDITIONNO><UPDATENO></UPDATENO><UNITTYPE></UNITTYPE></item><item xmlns=""""><ACTIONNUMBER>9</ACTIONNUMBER><ACTION>ASSIGN CELL TO AVCS UNIT OF SALE</ACTION><PRODUCT>AVCS UNIT</PRODUCT><PRODTYPE>S57</PRODTYPE><CHILDCELL>US4AK6NT</CHILDCELL><PRODUCTNAME>PAYSF</PRODUCTNAME><CANCELLED></CANCELLED><REPLACEDBY></REPLACEDBY><AGENCY></AGENCY><PROVIDER></PROVIDER><ENCSIZE></ENCSIZE><TITLE></TITLE><EDITIONNO></EDITIONNO><UPDATENO></UPDATENO><UNITTYPE></UNITTYPE></item><item xmlns=""""><ACTIONNUMBER>10</ACTIONNUMBER><ACTION>ASSIGN CELL TO AVCS UNIT OF SALE</ACTION><PRODUCT>AVCS UNIT</PRODUCT><PRODTYPE>S57</PRODTYPE><CHILDCELL>US4AK6NU</CHILDCELL><PRODUCTNAME>PAYSF</PRODUCTNAME><CANCELLED></CANCELLED><REPLACEDBY></REPLACEDBY><AGENCY></AGENCY><PROVIDER></PROVIDER><ENCSIZE></ENCSIZE><TITLE></TITLE><EDITIONNO></EDITIONNO><UPDATENO></UPDATENO><UNITTYPE></UNITTYPE></item><item xmlns=""""><ACTIONNUMBER>11</ACTIONNUMBER><ACTION>REPLACED WITH ENC CELL</ACTION><PRODUCT>ENC CELL</PRODUCT><PRODTYPE>S57</PRODTYPE><CHILDCELL>US5AK83M</CHILDCELL><PRODUCTNAME>US5AK83M</PRODUCTNAME><CANCELLED></CANCELLED><REPLACEDBY>US4AK6NT</REPLACEDBY><AGENCY></AGENCY><PROVIDER></PROVIDER><ENCSIZE></ENCSIZE><TITLE></TITLE><EDITIONNO></EDITIONNO><UPDATENO></UPDATENO><UNITTYPE></UNITTYPE></item><item xmlns=""""><ACTIONNUMBER>12</ACTIONNUMBER><ACTION>REPLACED WITH ENC CELL</ACTION><PRODUCT>ENC CELL</PRODUCT><PRODTYPE>S57</PRODTYPE><CHILDCELL>US5AK83M</CHILDCELL><PRODUCTNAME>US5AK83M</PRODUCTNAME><CANCELLED></CANCELLED><REPLACEDBY>US4AK6NU</REPLACEDBY><AGENCY></AGENCY><PROVIDER></PROVIDER><ENCSIZE></ENCSIZE><TITLE></TITLE><EDITIONNO></EDITIONNO><UPDATENO></UPDATENO><UNITTYPE></UNITTYPE></item><item xmlns=""""><ACTIONNUMBER>13</ACTIONNUMBER><ACTION>REMOVE ENC CELL FROM AVCS UNIT OF SALE</ACTION><PRODUCT>AVCS UNIT</PRODUCT><PRODTYPE>S57</PRODTYPE><CHILDCELL>US5AK83M</CHILDCELL><PRODUCTNAME>US5AK83M</PRODUCTNAME><CANCELLED></CANCELLED><REPLACEDBY></REPLACEDBY><AGENCY></AGENCY><PROVIDER></PROVIDER><ENCSIZE></ENCSIZE><TITLE></TITLE><EDITIONNO></EDITIONNO><UPDATENO></UPDATENO><UNITTYPE></UNITTYPE></item><item xmlns=""""><ACTIONNUMBER>14</ACTIONNUMBER><ACTION>REMOVE ENC CELL FROM AVCS UNIT OF SALE</ACTION><PRODUCT>AVCS UNIT</PRODUCT><PRODTYPE>S57</PRODTYPE><CHILDCELL>US5AK83M</CHILDCELL><PRODUCTNAME>AVCSO</PRODUCTNAME><CANCELLED></CANCELLED><REPLACEDBY></REPLACEDBY><AGENCY></AGENCY><PROVIDER></PROVIDER><ENCSIZE></ENCSIZE><TITLE></TITLE><EDITIONNO></EDITIONNO><UPDATENO></UPDATENO><UNITTYPE></UNITTYPE></item><item xmlns=""""><ACTIONNUMBER>15</ACTIONNUMBER><ACTION>REMOVE ENC CELL FROM AVCS UNIT OF SALE</ACTION><PRODUCT>AVCS UNIT</PRODUCT><PRODTYPE>S57</PRODTYPE><CHILDCELL>US5AK83M</CHILDCELL><PRODUCTNAME>PAYSF</PRODUCTNAME><CANCELLED></CANCELLED><REPLACEDBY></REPLACEDBY><AGENCY></AGENCY><PROVIDER></PROVIDER><ENCSIZE></ENCSIZE><TITLE></TITLE><EDITIONNO></EDITIONNO><UPDATENO></UPDATENO><UNITTYPE></UNITTYPE></item><item xmlns=""""><ACTIONNUMBER>16</ACTIONNUMBER><ACTION>CANCEL ENC CELL</ACTION><PRODUCT>ENC CELL</PRODUCT><PRODTYPE>S57</PRODTYPE><CHILDCELL>US5AK83M</CHILDCELL><PRODUCTNAME>US5AK83M</PRODUCTNAME><CANCELLED></CANCELLED><REPLACEDBY></REPLACEDBY><AGENCY></AGENCY><PROVIDER></PROVIDER><ENCSIZE></ENCSIZE><TITLE></TITLE><EDITIONNO></EDITIONNO><UPDATENO></UPDATENO><UNITTYPE></UNITTYPE></item><item xmlns=""""><ACTIONNUMBER>17</ACTIONNUMBER><ACTION>CANCEL AVCS UNIT OF SALE</ACTION><PRODUCT>AVCS UNIT</PRODUCT><PRODTYPE>S57</PRODTYPE><CHILDCELL></CHILDCELL><PRODUCTNAME>US5AK83M</PRODUCTNAME><CANCELLED></CANCELLED><REPLACEDBY></REPLACEDBY><AGENCY></AGENCY><PROVIDER></PROVIDER><ENCSIZE></ENCSIZE><TITLE></TITLE><EDITIONNO></EDITIONNO><UPDATENO></UPDATENO><UNITTYPE></UNITTYPE></item></ACTIONITEMS></IM_MATINFO></Z_ADDS_MAT_INFO></Z_ADDS_MAT_INFO></soap:Body></soap:Envelope>
";

            XmlDocument fakeXmlDoc = new XmlDocument();
            fakeXmlDoc.LoadXml(fakeXmlData);

            XmlDocument soapXml = new();
            soapXml.LoadXml(sapXmlFile);

            A.CallTo(() => _fakeFileSystemHelper.IsFileExists(A<string>.Ignored)).Returns(true);
            A.CallTo(() => _fakeXmlHelper.CreateXmlDocument(A<string>.Ignored)).Returns(soapXml);

            var result = _fakeSapMessageBuilder.BuildSapMessageXml(scenarios!, correlationId);

            result.Should().BeOfType<XmlDocument>();
            Assert.That(fakeXmlDoc.OuterXml.Length, Is.EqualTo(result.OuterXml.Length));

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
            && call.GetArgument<LogLevel>(0) == LogLevel.Information
            && call.GetArgument<EventId>(1) == EventIds.BuildingSapActionStarted.ToEventId()
            && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Building SAP actions.").MustHaveHappened();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
            && call.GetArgument<LogLevel>(0) == LogLevel.Information
            && call.GetArgument<EventId>(1) == EventIds.SapActionCreated.ToEventId()
            && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "SAP action {ActionName} created.").MustHaveHappened();
        }

        [Test]
        public void WhenBuildSapMessageXmlIsCalledWithChangeMoveCellScenario_ThenReturnXMLDocument()
        {
            var scenarios = JsonConvert.DeserializeObject<EncEventPayload>(scenariosDataChangeMoveCell);
            var correlationId = "367ce4a4-1d62-4f56-b359-59e178d77100";

            string fakeXmlData = @"<?xml version=""1.0"" encoding=""utf-8""?><soap:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/""><soap:Body><Z_ADDS_MAT_INFO xmlns=""urn:sap-com:document:sap:rfc:functions""><Z_ADDS_MAT_INFO><IM_MATINFO xmlns=""""><CORRID>367ce4a4-1d62-4f56-b359-59e178d77100</CORRID><NOOFACTIONS>7</NOOFACTIONS><RECDATE>20230615</RECDATE><RECTIME>105851</RECTIME><ORG>UKHO</ORG><ACTIONITEMS xmlns=""urn:sap-com:document:sap:rfc:functions""><item xmlns=""""><ACTIONNUMBER>1</ACTIONNUMBER><ACTION>ASSIGN CELL TO AVCS UNIT OF SALE</ACTION><PRODUCT>AVCS UNIT</PRODUCT><PRODTYPE>S57</PRODTYPE><CHILDCELL>MX545010</CHILDCELL><PRODUCTNAME>MX509226</PRODUCTNAME><CANCELLED></CANCELLED><REPLACEDBY></REPLACEDBY><AGENCY></AGENCY><PROVIDER></PROVIDER><ENCSIZE></ENCSIZE><TITLE></TITLE><EDITIONNO></EDITIONNO><UPDATENO></UPDATENO><UNITTYPE></UNITTYPE></item><item xmlns=""""><ACTIONNUMBER>2</ACTIONNUMBER><ACTION>CHANGE ENC CELL</ACTION><PRODUCT>ENC CELL</PRODUCT><PRODTYPE>S57</PRODTYPE><CHILDCELL>MX545010</CHILDCELL><PRODUCTNAME>MX545010</PRODUCTNAME><CANCELLED></CANCELLED><REPLACEDBY></REPLACEDBY><AGENCY>MX</AGENCY><PROVIDER>1</PROVIDER><ENCSIZE>medium</ENCSIZE><TITLE>Isla Clarion</TITLE><EDITIONNO>1</EDITIONNO><UPDATENO>0</UPDATENO><UNITTYPE></UNITTYPE></item><item xmlns=""""><ACTIONNUMBER>3</ACTIONNUMBER><ACTION>CHANGE AVCS UNIT OF SALE</ACTION><PRODUCT>AVCS UNIT</PRODUCT><PRODTYPE>S57</PRODTYPE><CHILDCELL></CHILDCELL><PRODUCTNAME>MX545010</PRODUCTNAME><CANCELLED></CANCELLED><REPLACEDBY></REPLACEDBY><AGENCY>MX</AGENCY><PROVIDER>1</PROVIDER><ENCSIZE>large</ENCSIZE><TITLE>Isla Clarion</TITLE><EDITIONNO></EDITIONNO><UPDATENO></UPDATENO><UNITTYPE>AVCS Units Coastal</UNITTYPE></item><item xmlns=""""><ACTIONNUMBER>4</ACTIONNUMBER><ACTION>CHANGE AVCS UNIT OF SALE</ACTION><PRODUCT>AVCS UNIT</PRODUCT><PRODTYPE>S57</PRODTYPE><CHILDCELL></CHILDCELL><PRODUCTNAME>MX509226</PRODUCTNAME><CANCELLED></CANCELLED><REPLACEDBY></REPLACEDBY><AGENCY>MX</AGENCY><PROVIDER>1</PROVIDER><ENCSIZE>large</ENCSIZE><TITLE>Playa del Carmen</TITLE><EDITIONNO></EDITIONNO><UPDATENO></UPDATENO><UNITTYPE>AVCS Units Coastal</UNITTYPE></item><item xmlns=""""><ACTIONNUMBER>5</ACTIONNUMBER><ACTION>CHANGE AVCS UNIT OF SALE</ACTION><PRODUCT>AVCS UNIT</PRODUCT><PRODTYPE>S57</PRODTYPE><CHILDCELL></CHILDCELL><PRODUCTNAME>AVCSO</PRODUCTNAME><CANCELLED></CANCELLED><REPLACEDBY></REPLACEDBY><AGENCY>MX</AGENCY><PROVIDER>1</PROVIDER><ENCSIZE>large</ENCSIZE><TITLE>AVCS Online Folio Some Title</TITLE><EDITIONNO></EDITIONNO><UPDATENO></UPDATENO><UNITTYPE>AVCS Online Folio</UNITTYPE></item><item xmlns=""""><ACTIONNUMBER>6</ACTIONNUMBER><ACTION>CHANGE AVCS UNIT OF SALE</ACTION><PRODUCT>AVCS UNIT</PRODUCT><PRODTYPE>S57</PRODTYPE><CHILDCELL></CHILDCELL><PRODUCTNAME>PAYSF</PRODUCTNAME><CANCELLED></CANCELLED><REPLACEDBY></REPLACEDBY><AGENCY>MX</AGENCY><PROVIDER>1</PROVIDER><ENCSIZE>large</ENCSIZE><TITLE>World Folio</TITLE><EDITIONNO></EDITIONNO><UPDATENO></UPDATENO><UNITTYPE>AVCS Folio Transit</UNITTYPE></item><item xmlns=""""><ACTIONNUMBER>7</ACTIONNUMBER><ACTION>REMOVE ENC CELL FROM AVCS UNIT OF SALE</ACTION><PRODUCT>AVCS UNIT</PRODUCT><PRODTYPE>S57</PRODTYPE><CHILDCELL>MX545010</CHILDCELL><PRODUCTNAME>MX545010</PRODUCTNAME><CANCELLED></CANCELLED><REPLACEDBY></REPLACEDBY><AGENCY></AGENCY><PROVIDER></PROVIDER><ENCSIZE></ENCSIZE><TITLE></TITLE><EDITIONNO></EDITIONNO><UPDATENO></UPDATENO><UNITTYPE></UNITTYPE></item></ACTIONITEMS></IM_MATINFO></Z_ADDS_MAT_INFO></Z_ADDS_MAT_INFO></soap:Body></soap:Envelope>";

            XmlDocument fakeXmlDoc = new XmlDocument();
            fakeXmlDoc.LoadXml(fakeXmlData);

            XmlDocument soapXml = new();
            soapXml.LoadXml(sapXmlFile);

            A.CallTo(() => _fakeFileSystemHelper.IsFileExists(A<string>.Ignored)).Returns(true);
            A.CallTo(() => _fakeXmlHelper.CreateXmlDocument(A<string>.Ignored)).Returns(soapXml);

            var result = _fakeSapMessageBuilder.BuildSapMessageXml(scenarios!, correlationId);

            result.Should().BeOfType<XmlDocument>();
            Assert.That(fakeXmlDoc.OuterXml.Length, Is.EqualTo(result.OuterXml.Length));

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
            && call.GetArgument<LogLevel>(0) == LogLevel.Information
            && call.GetArgument<EventId>(1) == EventIds.BuildingSapActionStarted.ToEventId()
            && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Building SAP actions.").MustHaveHappened();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
            && call.GetArgument<LogLevel>(0) == LogLevel.Information
            && call.GetArgument<EventId>(1) == EventIds.SapActionCreated.ToEventId()
            && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "SAP action {ActionName} created.").MustHaveHappened();
        }

        [Test]
        public void WhenSapXmlTemplateFileNotExist_ThenThrowFileNotFoundException()
        {
            var scenarios = JsonConvert.DeserializeObject<EncEventPayload>(scenariosDataCancelReplaceCell);
            var correlationId = "367ce4a4-1d62-4f56-b359-59e178d77100";

            A.CallTo(() => _fakeFileSystemHelper.IsFileExists(A<string>.Ignored)).Returns(false);

            Assert.Throws<FileNotFoundException>(() => _fakeSapMessageBuilder.BuildSapMessageXml(scenarios!, correlationId));

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
            && call.GetArgument<LogLevel>(0) == LogLevel.Error
            && call.GetArgument<EventId>(1) == EventIds.SapXmlTemplateNotFound.ToEventId()
            && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "The SAP message xml template does not exist.").MustHaveHappenedOnceExactly();
        }

        [Test]
        public void WhenBuildSapMessageXmlIsCalledWithSimpleUpdateCellScenario_ThenReturnXMLDocument()
        {
            var scenarios = JsonConvert.DeserializeObject<EncEventPayload>(scenariosDataSimpleUpdate);
            var correlationId = "367ce4a4-1d62-4f56-b359-59e178d77100";

            string fakeXmlData = @"<?xml version=""1.0"" encoding=""utf-8""?><soap:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/""><soap:Body><Z_ADDS_MAT_INFO xmlns=""urn:sap-com:document:sap:rfc:functions""><Z_ADDS_MAT_INFO><IM_MATINFO xmlns=""""><CORRID>367ce4a4-1d62-4f56-b359-59e178d77100</CORRID><NOOFACTIONS>1</NOOFACTIONS><RECDATE>20230615</RECDATE><RECTIME>102134</RECTIME><ORG>UKHO</ORG><ACTIONITEMS xmlns=""urn:sap-com:document:sap:rfc:functions""><item xmlns=""""><ACTIONNUMBER>1</ACTIONNUMBER><ACTION>UPDATE ENC CELL EDITION UPDATE NUMBER</ACTION><PRODUCT>ENC CELL</PRODUCT><PRODTYPE>S57</PRODTYPE><CHILDCELL>US4FL1YE</CHILDCELL><PRODUCTNAME>US4FL1YE</PRODUCTNAME><CANCELLED></CANCELLED><REPLACEDBY></REPLACEDBY><AGENCY>US</AGENCY><PROVIDER>1</PROVIDER><ENCSIZE>medium</ENCSIZE><TITLE>Some Title</TITLE><EDITIONNO>4</EDITIONNO><UPDATENO>2</UPDATENO><UNITTYPE></UNITTYPE></item></ACTIONITEMS></IM_MATINFO></Z_ADDS_MAT_INFO></Z_ADDS_MAT_INFO></soap:Body></soap:Envelope>";

            XmlDocument fakeXmlDoc = new XmlDocument();
            fakeXmlDoc.LoadXml(fakeXmlData);

            XmlDocument soapXml = new XmlDocument();
            soapXml.LoadXml(sapXmlFile);

            A.CallTo(() => _fakeFileSystemHelper.IsFileExists(A<string>.Ignored)).Returns(true);
            A.CallTo(() => _fakeXmlHelper.CreateXmlDocument(A<string>.Ignored)).Returns(soapXml);

            var result = _fakeSapMessageBuilder.BuildSapMessageXml(scenarios!, correlationId);
            result.Should().BeOfType<XmlDocument>();

            result.Should().BeOfType<XmlDocument>();
            Assert.That(fakeXmlDoc.OuterXml.Length, Is.EqualTo(result.OuterXml.Length));

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
            && call.GetArgument<LogLevel>(0) == LogLevel.Information
            && call.GetArgument<EventId>(1) == EventIds.BuildingSapActionStarted.ToEventId()
            && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Building SAP actions.").MustHaveHappened();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
            && call.GetArgument<LogLevel>(0) == LogLevel.Information
            && call.GetArgument<EventId>(1) == EventIds.SapActionCreated.ToEventId()
            && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "SAP action {ActionName} created.").MustHaveHappened();
        }
        [Test]
        [TestCase("ENC S57", "S57")]
        [TestCase("ENC", "ENC")]
        [TestCase("", "")]
        [TestCase(null, "")]
        public void GetProdTypeTest(string prodType, string actual)
        {
            MethodInfo sumPrivate = typeof(SapMessageBuilder).GetMethod("GetProdType", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance)!;
            string result = (string)sumPrivate.Invoke(_fakeSapMessageBuilder, new object[] { prodType })!;

            Assert.AreEqual(result, actual);
        }

        [Test]
        [TestCase("ENC S57", "PRODTYPE", "S57")]
        [TestCase("ENC S57", "PRODTYPE1", "ENC S57")]
        [TestCase("", "PRODTYPE1", "")]
        [TestCase(null, "PRODTYPE1", "")]
        public void GetXmlNodeValueTest(string prodType, string prod, string actual)
        {
            MethodInfo XmlNodeValue = typeof(SapMessageBuilder).GetMethod("GetXmlNodeValue", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance)!;
            string result = (string)XmlNodeValue.Invoke(_fakeSapMessageBuilder, new object[] { prodType, prod })!;

            Assert.AreEqual(result, actual);
        }

        [Test]
        public void SortXmlPayloadTest()
        {
            string expectedResult = "1CREATE ENC CELLENC CELLS57US4AK6NTUS4AK6NTUS1mediumNorton Sound - Alaska82";
            string XpathActionItems = $"//*[local-name()='ACTIONITEMS']";
            var sapReqXml = TestHelper.ReadFileData("ERPTestData\\ActionItemNodeTest.xml");

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(sapReqXml);
            XmlNode actionItemNode = xmlDoc.SelectSingleNode(XpathActionItems)!;

            MethodInfo xmlPayLoad = typeof(SapMessageBuilder).GetMethod("SortXmlPayload", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance)!;
            var result = (XmlNode)xmlPayLoad.Invoke(_fakeSapMessageBuilder, new object[] { actionItemNode })!;

            var firstNode = result.Cast<XmlNode>().FirstOrDefault().InnerText;
            Assert.AreEqual(firstNode, expectedResult);
        }
    }
}
