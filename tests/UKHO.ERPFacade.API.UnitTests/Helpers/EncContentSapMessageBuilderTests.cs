﻿using System.Collections.Generic;
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

        private const string SapXmlFile = @"<?xml version=""1.0"" encoding=""utf-8""?>
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

        private const string ScenariosDataCancelReplaceCell = @"{""specversion"":""1.0"",""type"":""uk.gov.ukho.encpublishing.enccontentpublished.v2.1"",""source"":""https://encpublishing.ukho.gov.uk"",""id"":""2f03a25f-28b3-46ea-b009-5943250a9a41"",""time"":""2020-10-13T12:08:03.4880776Z"",""subject"":""US5AK83M,US4AK6NT,US4AK6NU"",""datacontenttype"":""application/json"",""data"":{""correlationId"":""1c23a350-6239-472a-a0f7-09ad5fb2d51d"",""ukhoWeekNumber"":{""year"":2024,""week"":3,""currentWeekAlphaCorrection"":false},""products"":[{""productType"":""ENC S57"",""dataSetName"":""US5AK83M.001"",""productName"":""US5AK83M"",""title"":""St. Michael Bay"",""scale"":90000,""usageBand"":5,""editionNumber"":0,""updateNumber"":1,""mayAffectHoldings"":true,""contentChange"":true,""permit"":""permitString"",""providerCode"":""1"",""providerName"":""ICE"",""size"":""small"",""agency"":""US"",""bundle"":[{""bundleType"":""DVD"",""location"":""M1;B1""}],""status"":{""statusName"":""Cancellation Update"",""statusDate"":""2023-03-03T04:30:00+05:30"",""isNewCell"":false},""replaces"":[],""replacedBy"":[""US4AK6NT"",""US4AK6NU""],""additionalCoverage"":[],""geographicLimit"":{""boundingBox"":{""northLimit"":24.0,""southLimit"":22.0,""eastLimit"":120.0,""westLimit"":119.0}},""inUnitsOfSale"":[""US5AK83M"",""AVCSO"",""PAYSF""],""s63"":{""name"":""US5AK83M.001"",""hash"":""5160204288db0a543850da177ec637b71ed55946e7f3843e6180b1906ee4f4ea"",""location"":""8198201e-78ce-4af8-9145-ad68ba0472e2"",""fileSize"":""4500"",""compression"":true,""s57Crc"":""5C06E104""},""signature"":{""name"":""US5AK83M.001"",""hash"":""fd0c9e5f95c0b664a1a27c2ff98812c648ebd113b117f5639f74536c397fbac9"",""location"":""0ecf2f38-a876-4d77-bd0e-0d901d3a0e73"",""fileSize"":""2500""},""ancillaryFiles"":[{""name"":""GB123_04.TXT"",""hash"":""d030b93ad8a0801e6955e4f52599f6200d01310155882a1b80eec78c9b93662b"",""location"":""2a29932e-dcb8-4e3a-b8a2-b3cfc335ede5"",""fileSize"":""1240""},{""name"":""GB125_01.TXT"",""hash"":""bb8042082bd1d37236801837585fa0df5e96097fb8d2281b41888af2b23ceb0b"",""location"":""1ad0f9c3-8c93-495a-99a1-06a36410faa9"",""fileSize"":""1360""},{""name"":""GB162_01.TXT"",""hash"":""81470666f387a9035f6b33f59fb9bbf0872e9c296fecee58c4e919d6a1d87ab6"",""location"":""eb443aad-394c-4eb0-b391-415a261605a1"",""fileSize"":""1360""}]},{""productType"":""ENC S57"",""dataSetName"":""US4AK6NT.001"",""productName"":""US4AK6NT"",""title"":""Norton Sound - Alaska"",""scale"":12000,""usageBand"":6,""editionNumber"":8,""updateNumber"":2,""mayAffectHoldings"":true,""contentChange"":true,""permit"":""permitString"",""providerCode"":""1"",""providerName"":""ICE"",""size"":""medium"",""agency"":""US"",""bundle"":[{""bundleType"":""DVD"",""location"":""M1;B1""}],""status"":{""statusName"":""New Edition"",""statusDate"":""2023-03-03T04:30:00+05:30"",""isNewCell"":true},""replaces"":[""US5AK83M""],""replacedBy"":[],""additionalCoverage"":[],""geographicLimit"":{""boundingBox"":{""northLimit"":63.0,""southLimit"":63.0,""eastLimit"":162.0,""westLimit"":161.0}},""inUnitsOfSale"":[""US4AK6NT"",""AVCSO"",""PAYSF""],""s63"":{""name"":""US4AK6NT.001"",""hash"":""5160204288db0a543850da177ec637b71ed55946e7f3843e6180b1906ee4f4ea"",""location"":""8198201e-78ce-4af8-9145-ad68ba0472e2"",""fileSize"":""4500"",""compression"":true,""s57Crc"":""5C06E104""},""signature"":{""name"":""US4AK6NT.001"",""hash"":""fd0c9e5f95c0b664a1a27c2ff98812c648ebd113b117f5639f74536c397fbac9"",""location"":""0ecf2f38-a876-4d77-bd0e-0d901d3a0e73"",""fileSize"":""2500""},""ancillaryFiles"":[{""name"":""US4AK6NT_04.TXT"",""hash"":""d030b93ad8a0801e6955e4f52599f6200d01310155882a1b80eec78c9b93662b"",""location"":""2a29932e-dcb8-4e3a-b8a2-b3cfc335ede5"",""fileSize"":""1240""}]},{""productType"":""ENC S57"",""dataSetName"":""US4AK6NU.001"",""productName"":""US4AK6NU"",""title"":""Norton Sound - Alaska"",""scale"":12000,""usageBand"":6,""editionNumber"":8,""updateNumber"":2,""mayAffectHoldings"":true,""contentChange"":true,""permit"":""permitString"",""providerCode"":""1"",""providerName"":""ICE"",""size"":""medium"",""agency"":""US"",""bundle"":[{""bundleType"":""DVD"",""location"":""M1;B1""}],""status"":{""statusName"":""New Edition"",""statusDate"":""2023-03-03T04:30:00+05:30"",""isNewCell"":true},""replaces"":[""US5AK83M""],""replacedBy"":[],""additionalCoverage"":[],""geographicLimit"":{""boundingBox"":{""northLimit"":63.0,""southLimit"":63.0,""eastLimit"":162.0,""westLimit"":161.0}},""inUnitsOfSale"":[""US4AK6NU"",""AVCSO"",""PAYSF""],""s63"":{""name"":""US4AK6NU.001"",""hash"":""5160204288db0a543850da177ec637b71ed55946e7f3843e6180b1906ee4f4ea"",""location"":""8198201e-78ce-4af8-9145-ad68ba0472e2"",""fileSize"":""4500"",""compression"":true,""s57Crc"":""5C06E104""},""signature"":{""name"":""US4AK6NU.001"",""hash"":""fd0c9e5f95c0b664a1a27c2ff98812c648ebd113b117f5639f74536c397fbac9"",""location"":""0ecf2f38-a876-4d77-bd0e-0d901d3a0e73"",""fileSize"":""2500""},""ancillaryFiles"":[{""name"":""US4AK6NU_04.TXT"",""hash"":""d030b93ad8a0801e6955e4f52599f6200d01310155882a1b80eec78c9b93662b"",""location"":""2a29932e-dcb8-4e3a-b8a2-b3cfc335ede5"",""fileSize"":""1240""}]}],""unitsOfSale"":[{""unitName"":""US5AK83M"",""title"":""St. Michael Bay"",""unitOfSaleType"":""unit"",""unitSize"":""large"",""unitType"":""AVCS Units Coastal"",""status"":""NotForSale"",""isNewUnitOfSale"":false,""geographicLimit"":{""boundingBox"":{""northLimit"":24.146815,""southLimit"":22.581615,""eastLimit"":120.349635,""westLimit"":119.39142}},""compositionChanges"":{""addProducts"":[],""removeProducts"":[""US5AK83M""]}},{""unitName"":""US4AK6NT"",""title"":""Norton Sound - Alaska"",""unitOfSaleType"":""unit"",""unitSize"":""large"",""unitType"":""AVCS Units Coastal"",""status"":""ForSale"",""isNewUnitOfSale"":true,""geographicLimit"":{""boundingBox"":{""northLimit"":22.643255,""southLimit"":22.4625767,""eastLimit"":120.34972,""westLimit"":120.219475}},""compositionChanges"":{""addProducts"":[""US4AK6NT""],""removeProducts"":[]}},{""unitName"":""US4AK6NU"",""title"":""Norton Sound - Alaska"",""unitOfSaleType"":""unit"",""unitSize"":""large"",""unitType"":""AVCS Units Coastal"",""status"":""ForSale"",""isNewUnitOfSale"":true,""geographicLimit"":{""boundingBox"":{""northLimit"":22.643255,""southLimit"":22.4625767,""eastLimit"":120.34972,""westLimit"":120.219475}},""compositionChanges"":{""addProducts"":[""US4AK6NU""],""removeProducts"":[]}},{""unitName"":""AVCSO"",""title"":""AVCS Online Folio Some Title"",""unitOfSaleType"":""folio"",""unitSize"":""large"",""unitType"":""AVCS Online Folio"",""status"":""ForSale"",""isNewUnitOfSale"":false,""geographicLimit"":{""boundingBox"":{""northLimit"":0.0,""southLimit"":0.0,""eastLimit"":0.0,""westLimit"":0.0}},""compositionChanges"":{""addProducts"":[""US4AK6NT"",""US4AK6NU""],""removeProducts"":[""US5AK83M""]}},{""unitName"":""PAYSF"",""title"":""World Folio"",""unitOfSaleType"":""folio"",""unitSize"":""large"",""unitType"":""AVCS Folio Transit"",""status"":""ForSale"",""isNewUnitOfSale"":false,""geographicLimit"":{""boundingBox"":{""northLimit"":0.0,""southLimit"":0.0,""eastLimit"":0.0,""westLimit"":0.0}},""compositionChanges"":{""addProducts"":[""US4AK6NT"",""US4AK6NU""],""removeProducts"":[""US5AK83M""]}}]}}";

        private const string ScenariosDataChangeMoveCell = @"{""specversion"":""1.0"",""type"":""uk.gov.ukho.encpublishing.enccontentpublished.v2.1"",""source"":""https://encpublishing.ukho.gov.uk"",""id"":""2f03a25f-28b3-46ea-b009-5943250a9a41"",""time"":""2020-10-13T12:08:03.4880776Z"",""subject"":""MX545010"",""datacontenttype"":""application/json"",""data"":{""correlationId"":""365ce4a4-1d62-4f56-b359-59e178d77202"",""ukhoWeekNumber"":{""year"":2024,""week"":3,""currentWeekAlphaCorrection"":false},""products"":[{""productType"":""ENC S57"",""dataSetName"":""MX545010.001"",""productName"":""MX545010"",""title"":""Isla Clarion"",""scale"":90000,""usageBand"":5,""editionNumber"":1,""updateNumber"":0,""mayAffectHoldings"":true,""contentChange"":false,""permit"":""permitString"",""providerCode"":""1"",""providerName"":""ICE"",""size"":""medium"",""agency"":""MX"",""bundle"":[{""bundleType"":""DVD"",""location"":""M1;B1""}],""status"":{""statusName"":""New Edition"",""statusDate"":""2023-03-03T04:30:00+05:30"",""isNewCell"":false},""replaces"":[],""replacedBy"":[],""additionalCoverage"":[],""geographicLimit"":{""boundingBox"":{""northLimit"":24.0,""southLimit"":22.0,""eastLimit"":120.0,""westLimit"":119.0}},""inUnitsOfSale"":[""MX545010"",""MX509226"",""AVCSO"",""PAYSF""],""s63"":{""name"":""XXXXXXXX.001"",""hash"":""5160204288db0a543850da177ec637b71ed55946e7f3843e6180b1906ee4f4ea"",""location"":""8198201e-78ce-4af8-9145-ad68ba0472e2"",""fileSize"":""4500"",""compression"":true,""s57Crc"":""5C06E104""},""signature"":{""name"":""XXXXXXXX.001"",""hash"":""fd0c9e5f95c0b664a1a27c2ff98812c648ebd113b117f5639f74536c397fbac9"",""location"":""0ecf2f38-a876-4d77-bd0e-0d901d3a0e73"",""fileSize"":""2500""},""ancillaryFiles"":[{""name"":""GBXXXXXXXX_04.TXT"",""hash"":""d030b93ad8a0801e6955e4f52599f6200d01310155882a1b80eec78c9b93662b"",""location"":""2a29932e-dcb8-4e3a-b8a2-b3cfc335ede5"",""fileSize"":""1240""},{""name"":""GBXXXXXXXX_01.TXT"",""hash"":""bb8042082bd1d37236801837585fa0df5e96097fb8d2281b41888af2b23ceb0b"",""location"":""1ad0f9c3-8c93-495a-99a1-06a36410faa9"",""fileSize"":""1360""},{""name"":""GBXXXXXXXX_01.TXT"",""hash"":""81470666f387a9035f6b33f59fb9bbf0872e9c296fecee58c4e919d6a1d87ab6"",""location"":""eb443aad-394c-4eb0-b391-415a261605a1"",""fileSize"":""1360""}]}],""unitsOfSale"":[{""unitName"":""MX545010"",""title"":""Isla Clarion"",""unitOfSaleType"":""unit"",""unitSize"":""large"",""unitType"":""AVCS Units Coastal"",""status"":""ForSale"",""isNewUnitOfSale"":false,""geographicLimit"":{""boundingBox"":{""northLimit"":24.146815,""southLimit"":22.581615,""eastLimit"":120.349635,""westLimit"":119.39142}},""compositionChanges"":{""addProducts"":[],""removeProducts"":[""MX545010""]}},{""unitName"":""MX509226"",""title"":""Playa del Carmen"",""unitOfSaleType"":""unit"",""unitSize"":""large"",""unitType"":""AVCS Units Coastal"",""status"":""ForSale"",""isNewUnitOfSale"":false,""geographicLimit"":{""boundingBox"":{""northLimit"":24.146815,""southLimit"":22.581615,""eastLimit"":120.349635,""westLimit"":119.39142}},""compositionChanges"":{""addProducts"":[""MX545010""],""removeProducts"":[]}},{""unitName"":""AVCSO"",""title"":""AVCS Online Folio Some Title"",""unitOfSaleType"":""folio"",""unitSize"":""large"",""unitType"":""AVCS Online Folio"",""status"":""ForSale"",""isNewUnitOfSale"":false,""geographicLimit"":{""boundingBox"":{""northLimit"":0.0,""southLimit"":0.0,""eastLimit"":0.0,""westLimit"":0.0}},""compositionChanges"":{""addProducts"":[],""removeProducts"":[]}},{""unitName"":""PAYSF"",""title"":""World Folio"",""unitOfSaleType"":""folio"",""unitSize"":""large"",""unitType"":""AVCS Folio Transit"",""status"":""ForSale"",""isNewUnitOfSale"":false,""geographicLimit"":{""boundingBox"":{""northLimit"":0.0,""southLimit"":0.0,""eastLimit"":0.0,""westLimit"":0.0}},""compositionChanges"":{""addProducts"":[],""removeProducts"":[]}}]}}";

        private const string ScenariosDataCreateEncCell = @"{""specversion"":""1.0"",""type"":""uk.gov.ukho.encpublishing.enccontentpublished.v2"",""source"":""https://encpublishing.ukho.gov.uk"",""id"":""2f03a25f-28b3-46ea-b009-5943250a9a41"",""time"":""2020-10-13T12:08:03.4880776Z"",""subject"":""MX545010"",""datacontenttype"":""application/json"",""data"":{""correlationId"":""765ce4a4-1d62-4f56-b359-59e178d77107"",""ukhoWeekNumber"":{""year"":2024,""week"":3,""currentWeekAlphaCorrection"":false},""products"":[{""productType"":""ENC S57"",""dataSetName"":""MX545010.001"",""productName"":""MX545010"",""title"":""Isla Clarion"",""scale"":90000,""usageBand"":5,""editionNumber"":1,""updateNumber"":0,""mayAffectHoldings"":true,""contentChange"":true,""permit"":""permitString"",""providerCode"":""1"",""providerName"":""ICE"",""size"":""medium"",""agency"":""MX"",""bundle"":[{""bundleType"":""DVD"",""location"":""M1;B1""}],""status"":{""statusName"":""New Edition"",""statusDate"":""2023-03-03T04:30:00+05:30"",""isNewCell"":true},""replaces"":[],""replacedBy"":[],""additionalCoverage"":[],""geographicLimit"":{""boundingBox"":{""northLimit"":24.0,""southLimit"":22.0,""eastLimit"":120.0,""westLimit"":119.0}},""inUnitsOfSale"":[""MX545010"",""AVCSO"",""PAYSF""],""s63"":{""name"":""XXXXXXXX.001"",""hash"":""5160204288db0a543850da177ec637b71ed55946e7f3843e6180b1906ee4f4ea"",""location"":""8198201e-78ce-4af8-9145-ad68ba0472e2"",""fileSize"":""4500"",""compression"":true,""s57Crc"":""5C06E104""},""signature"":{""name"":""XXXXXXXX.001"",""hash"":""fd0c9e5f95c0b664a1a27c2ff98812c648ebd113b117f5639f74536c397fbac9"",""location"":""0ecf2f38-a876-4d77-bd0e-0d901d3a0e73"",""fileSize"":""2500""},""ancillaryFiles"":[{""name"":""GBXXXXXXXX_04.TXT"",""hash"":""d030b93ad8a0801e6955e4f52599f6200d01310155882a1b80eec78c9b93662b"",""location"":""2a29932e-dcb8-4e3a-b8a2-b3cfc335ede5"",""fileSize"":""1240""},{""name"":""GBXXXXXXXX_01.TXT"",""hash"":""bb8042082bd1d37236801837585fa0df5e96097fb8d2281b41888af2b23ceb0b"",""location"":""1ad0f9c3-8c93-495a-99a1-06a36410faa9"",""fileSize"":""1360""},{""name"":""GBXXXXXXXX_01.TXT"",""hash"":""81470666f387a9035f6b33f59fb9bbf0872e9c296fecee58c4e919d6a1d87ab6"",""location"":""eb443aad-394c-4eb0-b391-415a261605a1"",""fileSize"":""1360""}]}],""unitsOfSale"":[{""unitName"":""MX545010"",""title"":""Isla Clarion"",""unitOfSaleType"":""unit"",""unitSize"":""large"",""unitType"":""AVCS Units Coastal"",""status"":""ForSale"",""isNewUnitOfSale"":true,""geographicLimit"":{""boundingBox"":{""northLimit"":24.146815,""southLimit"":22.581615,""eastLimit"":120.349635,""westLimit"":119.39142}},""compositionChanges"":{""addProducts"":[""MX545010""],""removeProducts"":[]}},{""unitName"":""AVCSO"",""title"":""AVCS Online Folio Some Title"",""unitOfSaleType"":""folio"",""unitSize"":""large"",""unitType"":""AVCS Online Folio"",""status"":""ForSale"",""isNewUnitOfSale"":false,""geographicLimit"":{""boundingBox"":{""northLimit"":0.0,""southLimit"":0.0,""eastLimit"":0.0,""westLimit"":0.0}},""compositionChanges"":{""addProducts"":[""MX545010""],""removeProducts"":[]}},{""unitName"":""PAYSF"",""title"":""World Folio"",""unitOfSaleType"":""folio"",""unitSize"":""large"",""unitType"":""AVCS Folio Transit"",""status"":""ForSale"",""isNewUnitOfSale"":false,""geographicLimit"":{""boundingBox"":{""northLimit"":0.0,""southLimit"":0.0,""eastLimit"":0.0,""westLimit"":0.0}},""compositionChanges"":{""addProducts"":[""MX545010""],""removeProducts"":[]}}]}}";

        private const string ScenariosDataUpdateEncCell = @"{""specversion"":""1.0"",""type"":""uk.gov.ukho.encpublishing.enccontentpublished.v2.1"",""source"":""https://encpublishing.ukho.gov.uk"",""id"":""2f03a25f-28b3-46ea-b009-5943250a9a41"",""time"":""2020-10-13T12:08:03.4880776Z"",""subject"":""DE416080"",""datacontenttype"":""application/json"",""data"":{""correlationId"":""765ce4a4-1d62-4f56-b359-59e178d77104"",""ukhoWeekNumber"":{""year"":2024,""week"":3,""currentWeekAlphaCorrection"":false},""products"":[{""productType"":""ENC S57"",""dataSetName"":""DE416080.001"",""productName"":""DE416080"",""title"":""Greifswalder Bodden"",""scale"":100000,""usageBand"":4,""editionNumber"":11,""updateNumber"":0,""mayAffectHoldings"":true,""contentChange"":true,""permit"":""permitString"",""providerCode"":""1"",""providerName"":""ICE"",""size"":""medium"",""agency"":""DE"",""bundle"":[{""bundleType"":""DVD"",""location"":""M1;B1""}],""status"":{""statusName"":""New Edition"",""statusDate"":""2023-03-03T04:30:00+05:30"",""isNewCell"":false},""replaces"":[],""replacedBy"":[],""additionalCoverage"":[],""geographicLimit"":{""boundingBox"":{""northLimit"":24.0,""southLimit"":22.0,""eastLimit"":120.0,""westLimit"":119.0}},""inUnitsOfSale"":[""DE416080"",""AVCSO"",""PAYSF""],""s63"":{""name"":""XXXXXXXX.001"",""hash"":""5160204288db0a543850da177ec637b71ed55946e7f3843e6180b1906ee4f4ea"",""location"":""8198201e-78ce-4af8-9145-ad68ba0472e2"",""fileSize"":""4500"",""compression"":true,""s57Crc"":""5C06E104""},""signature"":{""name"":""XXXXXXXX.001"",""hash"":""fd0c9e5f95c0b664a1a27c2ff98812c648ebd113b117f5639f74536c397fbac9"",""location"":""0ecf2f38-a876-4d77-bd0e-0d901d3a0e73"",""fileSize"":""2500""},""ancillaryFiles"":[{""name"":""GBXXXXXXXX_04.TXT"",""hash"":""d030b93ad8a0801e6955e4f52599f6200d01310155882a1b80eec78c9b93662b"",""location"":""2a29932e-dcb8-4e3a-b8a2-b3cfc335ede5"",""fileSize"":""1240""},{""name"":""GBXXXXXXXX_01.TXT"",""hash"":""bb8042082bd1d37236801837585fa0df5e96097fb8d2281b41888af2b23ceb0b"",""location"":""1ad0f9c3-8c93-495a-99a1-06a36410faa9"",""fileSize"":""1360""},{""name"":""GBXXXXXXXX_01.TXT"",""hash"":""81470666f387a9035f6b33f59fb9bbf0872e9c296fecee58c4e919d6a1d87ab6"",""location"":""eb443aad-394c-4eb0-b391-415a261605a1"",""fileSize"":""1360""}]}],""unitsOfSale"":[{""unitName"":""DE416080"",""title"":""Greifswalder Bodden Unit"",""unitOfSaleType"":""unit"",""unitSize"":""large"",""unitType"":""AVCS Units Coastal"",""status"":""ForSale"",""isNewUnitOfSale"":false,""geographicLimit"":{""boundingBox"":{""northLimit"":24.146815,""southLimit"":22.581615,""eastLimit"":120.349635,""westLimit"":119.39142}},""compositionChanges"":{""addProducts"":[],""removeProducts"":[]}},{""unitName"":""AVCSO"",""title"":""AVCS Online Folio Some Title"",""unitOfSaleType"":null,""unitSize"":""large"",""unitType"":""AVCS Online Folio"",""status"":""ForSale"",""isNewUnitOfSale"":false,""geographicLimit"":{""boundingBox"":{""northLimit"":0.0,""southLimit"":0.0,""eastLimit"":0.0,""westLimit"":0.0}},""compositionChanges"":{""addProducts"":[],""removeProducts"":[""DE416080""]}},{""unitName"":""PAYSF"",""title"":""World Folio"",""unitOfSaleType"":null,""unitSize"":""large"",""unitType"":""AVCS Folio Transit"",""status"":""ForSale"",""isNewUnitOfSale"":false,""geographicLimit"":{""boundingBox"":{""northLimit"":0.0,""southLimit"":0.0,""eastLimit"":0.0,""westLimit"":0.0}},""compositionChanges"":{""addProducts"":[],""removeProducts"":[""DE416080""]}}]}}";

        private const string ScenariosDataChangeEncCell = @"{""specversion"":""1.0"",""type"":""uk.gov.ukho.encpublishing.enccontentpublished.v2.1"",""source"":""https://encpublishing.ukho.gov.uk"",""id"":""2f03a25f-28b3-46ea-b009-5943250a9a41"",""time"":""2020-10-13T12:08:03.4880776Z"",""subject"":""MX545010"",""datacontenttype"":""application/json"",""data"":{""correlationId"":""465ce4a4-1d62-4f56-b359-59e178d77102"",""ukhoWeekNumber"":{""year"":2024,""week"":3,""currentWeekAlphaCorrection"":false},""products"":[{""productType"":""ENC S57"",""dataSetName"":""MX545010.001"",""productName"":""MX545010"",""title"":""Isla Clarion"",""scale"":90000,""usageBand"":5,""editionNumber"":1,""updateNumber"":0,""mayAffectHoldings"":false,""contentChange"":false,""permit"":""permitString"",""providerCode"":""2"",""providerName"":""PRIMAR"",""size"":""medium"",""agency"":""MX"",""bundle"":[{""bundleType"":""DVD"",""location"":""M1;B1""}],""status"":{""statusName"":""New Edition"",""statusDate"":""2023-03-03T04:30:00+05:30"",""isNewCell"":false},""replaces"":[],""replacedBy"":[],""additionalCoverage"":[],""geographicLimit"":{""boundingBox"":{""northLimit"":24.0,""southLimit"":22.0,""eastLimit"":120.0,""westLimit"":119.0}},""inUnitsOfSale"":[""MX545010"",""AVCSO"",""PAYSF""],""s63"":{""name"":""XXXXXXXX.001"",""hash"":""5160204288db0a543850da177ec637b71ed55946e7f3843e6180b1906ee4f4ea"",""location"":""8198201e-78ce-4af8-9145-ad68ba0472e2"",""fileSize"":""4500"",""compression"":true,""s57Crc"":""5C06E104""},""signature"":{""name"":""XXXXXXXX.001"",""hash"":""fd0c9e5f95c0b664a1a27c2ff98812c648ebd113b117f5639f74536c397fbac9"",""location"":""0ecf2f38-a876-4d77-bd0e-0d901d3a0e73"",""fileSize"":""2500""},""ancillaryFiles"":[{""name"":""GBXXXXXXXX_04.TXT"",""hash"":""d030b93ad8a0801e6955e4f52599f6200d01310155882a1b80eec78c9b93662b"",""location"":""2a29932e-dcb8-4e3a-b8a2-b3cfc335ede5"",""fileSize"":""1240""},{""name"":""GBXXXXXXXX_01.TXT"",""hash"":""bb8042082bd1d37236801837585fa0df5e96097fb8d2281b41888af2b23ceb0b"",""location"":""1ad0f9c3-8c93-495a-99a1-06a36410faa9"",""fileSize"":""1360""},{""name"":""GBXXXXXXXX_01.TXT"",""hash"":""81470666f387a9035f6b33f59fb9bbf0872e9c296fecee58c4e919d6a1d87ab6"",""location"":""eb443aad-394c-4eb0-b391-415a261605a1"",""fileSize"":""1360""}]}],""unitsOfSale"":[{""unitName"":""MX545010"",""title"":""Isla Clarion"",""unitOfSaleType"":""unit"",""unitSize"":""large"",""unitType"":""AVCS Units Coastal"",""status"":""ForSale"",""isNewUnitOfSale"":false,""geographicLimit"":{""boundingBox"":{""northLimit"":24.146815,""southLimit"":22.581615,""eastLimit"":120.349635,""westLimit"":119.39142}},""compositionChanges"":{""addProducts"":[],""removeProducts"":[]}},{""unitName"":""AVCSO"",""title"":""AVCS Online Folio Some Title"",""unitOfSaleType"":""folio"",""unitSize"":""large"",""unitType"":""AVCS Online Folio"",""status"":""ForSale"",""isNewUnitOfSale"":false,""geographicLimit"":{""boundingBox"":{""northLimit"":0.0,""southLimit"":0.0,""eastLimit"":0.0,""westLimit"":0.0}},""compositionChanges"":{""addProducts"":[],""removeProducts"":[]}},{""unitName"":""PAYSF"",""title"":""World Folio"",""unitOfSaleType"":""folio"",""unitSize"":""large"",""unitType"":""AVCS Folio Transit"",""status"":""ForSale"",""isNewUnitOfSale"":false,""geographicLimit"":{""boundingBox"":{""northLimit"":0.0,""southLimit"":0.0,""eastLimit"":0.0,""westLimit"":0.0}},""compositionChanges"":{""addProducts"":[],""removeProducts"":[]}}]}}";

        private const string ScenariosDataAdditionalCoverage = @"{""specversion"":""1.0"",""type"":""uk.gov.ukho.encpublishing.enccontentpublished.v2.1"",""source"":""https://encpublishing.ukho.gov.uk"",""id"":""2f03a25f-28b3-46ea-b009-5943250a9a41"",""time"":""2020-10-13T12:08:03.4880776Z"",""subject"":""DE416080"",""datacontenttype"":""application/json"",""data"":{""correlationId"":""765ce4a4-1d62-4f56-b359-59e178d77104"",""ukhoWeekNumber"":{""year"":2024,""week"":3,""currentWeekAlphaCorrection"":false},""products"":[{""productType"":""ENC S57"",""dataSetName"":""DE416080.001"",""productName"":""DE416080"",""title"":""Greifswalder Bodden"",""scale"":100000,""usageBand"":4,""editionNumber"":11,""updateNumber"":0,""mayAffectHoldings"":true,""contentChange"":true,""permit"":"" permitstring"",""providerCode"":""1"",""providerName"":""ICE"",""_ENUM"":[""large"",""medium"",""small""],""size"":""medium"",""agency"":""DE"",""bundle"":[{""bundleType"":""DVD"",""_ENUM"":[""DVD""],""location"":""M1;B1""}],""status"":{""statusName"":""New Edition"",""_ENUM"":[""New Edition"",""Re-issue"",""Update"",""Cancellation Update"",""Withdrawn"",""Suspended""],""statusDate"":""2023-03-03T00:00:00.00+01:00"",""isNewCell"":false},""replaces"":[],""replacedBy"":[],""additionalCoverage"":[""DK4LIMFW"",""DK5LIMFW""],""geographicLimit"":{""boundingBox"":{""northLimit"":24,""southLimit"":22,""eastLimit"":120,""westLimit"":119}},""inUnitsOfSale"":[""DE416080"",""AVCSO"",""PAYSF""],""s63"":{""name"":""XXXXXXXX.001"",""hash"":""5160204288db0a543850da177ec637b71ed55946e7f3843e6180b1906ee4f4ea"",""location"":""8198201e-78ce-4af8-9145-ad68ba0472e2"",""fileSize"":""4500"",""compression"":true,""s57Crc"":""5C06E104""},""signature"":{""name"":""XXXXXXXX.001"",""hash"":""fd0c9e5f95c0b664a1a27c2ff98812c648ebd113b117f5639f74536c397fbac9"",""location"":""0ecf2f38-a876-4d77-bd0e-0d901d3a0e73"",""fileSize"":""2500""},""ancillaryFiles"":[{""name"":""GBXXXXXXXX_04.TXT"",""hash"":""d030b93ad8a0801e6955e4f52599f6200d01310155882a1b80eec78c9b93662b"",""location"":""2a29932e-dcb8-4e3a-b8a2-b3cfc335ede5"",""fileSize"":""1240""},{""name"":""GBXXXXXXXX_01.TXT"",""hash"":""bb8042082bd1d37236801837585fa0df5e96097fb8d2281b41888af2b23ceb0b"",""location"":""1ad0f9c3-8c93-495a-99a1-06a36410faa9"",""fileSize"":""1360""},{""name"":""GBXXXXXXXX_01.TXT"",""hash"":""81470666f387a9035f6b33f59fb9bbf0872e9c296fecee58c4e919d6a1d87ab6"",""location"":""eb443aad-394c-4eb0-b391-415a261605a1"",""fileSize"":""1360""}]}],""unitsOfSale"":[{""unitName"":""DE416080"",""title"":""Greifswalder Bodden Unit"",""unitType"":""AVCS Units Coastal"",""unitSize"":""large"",""status"":""ForSale"",""_ENUM"":[""ForSale"",""NotForSale""],""isNewUnitOfSale"":false,""unitOfSaleType"":""unit"",""geographicLimit"":{""boundingBox"":{""northLimit"":24.146815,""southLimit"":22.581615,""eastLimit"":120.349635,""westLimit"":119.39142}},""compositionChanges"":{""addProducts"":[],""removeProducts"":[]}},{""unitName"":""AVCSO"",""title"":""AVCS Online Folio Some Title"",""unitSize"":""large"",""unitType"":""AVCS Online Folio"",""status"":""ForSale"",""isNewUnitOfSale"":false,""geographicLimit"":{""boundingBox"":{}},""compositionChanges"":{""addProducts"":[],""removeProducts"":[]}},{""unitName"":""PAYSF"",""title"":""World Folio"",""unitType"":""AVCS Folio Transit"",""unitSize"":""large"",""status"":""ForSale"",""isNewUnitOfSale"":false,""geographicLimit"":{""boundingBox"":{}},""compositionChanges"":{""addProducts"":[],""removeProducts"":[]}}]}}";

        #endregion scenariosData

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
            var scenarios = JsonConvert.DeserializeObject<EncEventPayload>(ScenariosDataCancelReplaceCell);
            var correlationId = "367ce4a4-1d62-4f56-b359-59e178d77100";
            var permitKeys = new PermitKey { ActiveKey = "", NextKey = "" };

            XmlDocument soapXml = new();
            soapXml.LoadXml(SapXmlFile);

            A.CallTo(() => _fakeFileSystemHelper.IsFileExists(A<string>.Ignored)).Returns(true);
            A.CallTo(() => _fakeXmlHelper.CreateXmlDocument(A<string>.Ignored)).Returns(soapXml);
            A.CallTo(() => _fakePermitDecryption.GetPermitKeys(A<string>.Ignored)).Returns(permitKeys);

            var result = _fakeEncContentSapMessageBuilder.BuildSapMessageXml(scenarios!, correlationId);

            result.Should().BeOfType<XmlDocument>();
            var actionItem = result.SelectSingleNode(XpathActionItems);
            actionItem.ChildNodes.Count.Should().Be(17);

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
            && call.GetArgument<LogLevel>(0) == LogLevel.Information
            && call.GetArgument<EventId>(1) == EventIds.BuildingSapActionStarted.ToEventId()
            && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Building SAP actions.").MustHaveHappenedOnceExactly();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
            && call.GetArgument<LogLevel>(0) == LogLevel.Information
            && call.GetArgument<EventId>(1) == EventIds.SapActionCreated.ToEventId()
            && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "SAP action {ActionName} created.").MustHaveHappened(17, Times.Exactly);
        }

        [Test]
        public void WhenBuildSapMessageXmlIsCalledWithChangeEncCellScenario_ThenReturnXMLDocument()
        {
            var scenarios = JsonConvert.DeserializeObject<EncEventPayload>(ScenariosDataChangeEncCell);
            var correlationId = "367ce4a4-1d62-4f56-b359-59e178d77100";
            var permitKeys = new PermitKey { ActiveKey = "", NextKey = "" };

            XmlDocument soapXml = new();
            soapXml.LoadXml(SapXmlFile);

            A.CallTo(() => _fakeFileSystemHelper.IsFileExists(A<string>.Ignored)).Returns(true);
            A.CallTo(() => _fakeXmlHelper.CreateXmlDocument(A<string>.Ignored)).Returns(soapXml);
            A.CallTo(() => _fakePermitDecryption.GetPermitKeys(A<string>.Ignored)).Returns(permitKeys);

            var result = _fakeEncContentSapMessageBuilder.BuildSapMessageXml(scenarios!, correlationId);

            result.Should().BeOfType<XmlDocument>();
            var actionItem = result.SelectSingleNode(XpathActionItems);
            actionItem.ChildNodes.Count.Should().Be(4);

            var activeKey = result.SelectSingleNode(XpathActiveKey);
            activeKey.InnerText.Should().Be(string.Empty);

            var nextKey = result.SelectSingleNode(XpathNextKey);
            nextKey.InnerText.Should().Be(string.Empty);

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
        public void WhenBuildSapMessageXmlIsCalledWithCreateEnccellScenario_ThenReturnXMLDocument()
        {
            var scenarios = JsonConvert.DeserializeObject<EncEventPayload>(ScenariosDataCreateEncCell);
            var correlationId = "367ce4a4-1d62-4f56-b359-59e178d77100";
            var permitKeys = new PermitKey { ActiveKey = "firstkey", NextKey = "nextkey" };

            XmlDocument soapXml = new();
            soapXml.LoadXml(SapXmlFile);

            A.CallTo(() => _fakeFileSystemHelper.IsFileExists(A<string>.Ignored)).Returns(true);
            A.CallTo(() => _fakeXmlHelper.CreateXmlDocument(A<string>.Ignored)).Returns(soapXml);
            A.CallTo(() => _fakePermitDecryption.GetPermitKeys(A<string>.Ignored)).Returns(permitKeys);

            var result = _fakeEncContentSapMessageBuilder.BuildSapMessageXml(scenarios!, correlationId);

            result.Should().BeOfType<XmlDocument>();
            var actionItem = result.SelectSingleNode(XpathActionItems);
            actionItem.ChildNodes.Count.Should().Be(5);

            var activeKey = result.SelectSingleNode(XpathActiveKey);
            activeKey.InnerText.Should().Be("firstkey");

            var nextKey = result.SelectSingleNode(XpathNextKey);
            nextKey.InnerText.Should().Be("nextkey");

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
                                                && call.GetArgument<LogLevel>(0) == LogLevel.Information
            && call.GetArgument<EventId>(1) == EventIds.BuildingSapActionStarted.ToEventId()
            && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Building SAP actions.").MustHaveHappenedOnceExactly();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
                                                && call.GetArgument<LogLevel>(0) == LogLevel.Information
                                                && call.GetArgument<EventId>(1) == EventIds.SapActionCreated.ToEventId()
            && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "SAP action {ActionName} created.").MustHaveHappened(5, Times.Exactly);
        }

        [Test]
        public void WhenBuildSapMessageXmlIsCalledWithUpdateEncCellScenario_ThenReturnXMLDocument()
        {
            var scenarios = JsonConvert.DeserializeObject<EncEventPayload>(ScenariosDataUpdateEncCell);
            var correlationId = "367ce4a4-1d62-4f56-b359-59e178d77100";
            var permitKeys = new PermitKey { ActiveKey = "activekey", NextKey = "nextkey" };

            XmlDocument soapXml = new();
            soapXml.LoadXml(SapXmlFile);

            A.CallTo(() => _fakeFileSystemHelper.IsFileExists(A<string>.Ignored)).Returns(true);
            A.CallTo(() => _fakeXmlHelper.CreateXmlDocument(A<string>.Ignored)).Returns(soapXml);
            A.CallTo(() => _fakePermitDecryption.GetPermitKeys(A<string>.Ignored)).Returns(permitKeys);

            var result = _fakeEncContentSapMessageBuilder.BuildSapMessageXml(scenarios!, correlationId);

            result.Should().BeOfType<XmlDocument>();
            var actionItem = result.SelectSingleNode(XpathActionItems);
            actionItem.ChildNodes.Count.Should().Be(3);

            var activeKey = result.SelectSingleNode(XpathActiveKey);
            activeKey.InnerText.Should().Be("activekey");

            var nextKey = result.SelectSingleNode(XpathNextKey);
            nextKey.InnerText.Should().Be("nextkey");

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
        public void WhenBuildSapMessageXmlIsCalledWithChangeMoveCellScenario_ThenReturnXMLDocument()
        {
            var scenarios = JsonConvert.DeserializeObject<EncEventPayload>(ScenariosDataChangeMoveCell);
            var correlationId = "367ce4a4-1d62-4f56-b359-59e178d77100";

            XmlDocument soapXml = new();
            soapXml.LoadXml(SapXmlFile);

            A.CallTo(() => _fakeFileSystemHelper.IsFileExists(A<string>.Ignored)).Returns(true);
            A.CallTo(() => _fakeXmlHelper.CreateXmlDocument(A<string>.Ignored)).Returns(soapXml);

            var result = _fakeEncContentSapMessageBuilder.BuildSapMessageXml(scenarios!, correlationId);

            result.Should().BeOfType<XmlDocument>();
            var actionItem = result.SelectSingleNode(XpathActionItems);
            actionItem.ChildNodes.Count.Should().Be(7);

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
        public void WhenSapXmlTemplateFileNotExist_ThenThrowFileNotFoundException()
        {
            var scenarios = JsonConvert.DeserializeObject<EncEventPayload>(ScenariosDataCancelReplaceCell);
            var correlationId = "367ce4a4-1d62-4f56-b359-59e178d77100";

            A.CallTo(() => _fakeFileSystemHelper.IsFileExists(A<string>.Ignored)).Returns(false);

            Assert.Throws<FileNotFoundException>(() => _fakeEncContentSapMessageBuilder.BuildSapMessageXml(scenarios!, correlationId));

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
                                                && call.GetArgument<LogLevel>(0) == LogLevel.Error
            && call.GetArgument<EventId>(1) == EventIds.SapXmlTemplateNotFound.ToEventId()
            && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "The SAP message xml template does not exist.").MustHaveHappenedOnceExactly();
        }

        [Test]
        public void WhenBuildSapMessageXmlIsCalledWithAdditionalCoverageScenario_ThenReturnXMLDocument()
        {
            var scenarios = JsonConvert.DeserializeObject<EncEventPayload>(ScenariosDataAdditionalCoverage);
            var correlationId = "765ce4a4-1d62-4f56-b359-59e178d77104";

            XmlDocument soapXml = new();
            soapXml.LoadXml(SapXmlFile);

            A.CallTo(() => _fakeFileSystemHelper.IsFileExists(A<string>.Ignored)).Returns(true);
            A.CallTo(() => _fakeXmlHelper.CreateXmlDocument(A<string>.Ignored)).Returns(soapXml);
            A.CallTo(() => _fakeWeekDetailsProvider.GetDateOfWeek(A<int>.Ignored, A<int>.Ignored, A<bool>.Ignored)).Returns("20240118");

            var result = _fakeEncContentSapMessageBuilder.BuildSapMessageXml(scenarios!, correlationId);

            result.Should().BeOfType<XmlDocument>();
            var actionItem = result.SelectSingleNode(XpathActionItems);
            actionItem.ChildNodes.Count.Should().Be(3);
            var childCell = result.SelectSingleNode(XpathChildCell);
            childCell.InnerXml.Should().Be("DE416080");
            var replacedBy = result.SelectSingleNode(XpathReplacedBy);
            replacedBy.InnerXml.Should().Be("DK4LIMFW");

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
        public void WhenMoreThanOneUnitOfSaleHavingUnitOfSaleTypeUnitIsPassedToGetUnitOfSaleForEncCell_ThenReturnsFirstUnitOfSaleHavingAddProduct()
        {
            var listOfUnitOfSales = new List<UnitOfSale>()
            {
                 new UnitOfSale() { UnitName = "MX545010", Title = "Title1", UnitOfSaleType = "unit",
                    CompositionChanges = new CompositionChanges { AddProducts = new List<string>() } },
                new UnitOfSale() { UnitName = "MX509226", Title = "Title2", UnitOfSaleType = "unit",
                    CompositionChanges = new CompositionChanges { AddProducts = new List<string>(){ "MX545010" } } }
            };

            var product = new Product()
            {
                ProductName = "MX545010",
                InUnitsOfSale = new List<string>() { "MX545010", "MX509226" }
            };

            MethodInfo methodInfo = typeof(EncContentSapMessageBuilder).GetMethod("GetUnitOfSaleForEncCell", BindingFlags.NonPublic | BindingFlags.Instance)!;
            var result = (UnitOfSale)methodInfo.Invoke(_fakeEncContentSapMessageBuilder, new object[] { listOfUnitOfSales, product })!;

            result.UnitName.Should().BeSameAs("MX509226");
            listOfUnitOfSales.Count.Should().Be(2);
        }

        [Test]
        public void WhenOneUnitOfSaleHavingUnitOfSaleTypeUnitIsPassedToGetUnitOfSaleForEncCell_ThenReturnsFirstOrDefaultUnitOfSale()
        {
            var listOfUnitOfSales = new List<UnitOfSale>()
            {
                new UnitOfSale() { UnitName = "MX509226", Title = "Title1", UnitOfSaleType = "unit" },
            };

            var product = new Product()
            {
                ProductName = "MX545010",
                InUnitsOfSale = new List<string>() { "MX509226" }
            };

            MethodInfo methodInfo = typeof(EncContentSapMessageBuilder).GetMethod("GetUnitOfSaleForEncCell", BindingFlags.NonPublic | BindingFlags.Instance)!;
            var result = (UnitOfSale)methodInfo.Invoke(_fakeEncContentSapMessageBuilder, new object[] { listOfUnitOfSales, product })!;

            result.UnitName.Should().BeSameAs("MX509226");
        }

        [Test]
        [TestCase("ENC S57", "S57")]
        [TestCase("ENC", "ENC")]
        [TestCase("", "")]
        [TestCase(null, "")]
        public void GetProdTypeTest(string prodType, string actual)
        {
            MethodInfo getProdType = typeof(EncContentSapMessageBuilder).GetMethod("GetProdType", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance)!;
            string result = (string)getProdType.Invoke(_fakeEncContentSapMessageBuilder, new object[] { prodType })!;

            Assert.That(actual, Is.EqualTo(result));
        }

        [Test]
        [TestCase("ENC S57", "PRODTYPE", "S57")]
        [TestCase("ENC S57", "PRODTYPE1", "ENC S57")]
        [TestCase("", "PRODTYPE1", "")]
        [TestCase(null, "PRODTYPE1", "")]
        public void GetXmlNodeValueTest(string prodType, string prod, string actual)
        {
            MethodInfo xmlNodeValue = typeof(EncContentSapMessageBuilder).GetMethod("GetXmlNodeValue", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance)!;
            string result = (string)xmlNodeValue.Invoke(_fakeEncContentSapMessageBuilder, new object[] { prodType, prod })!;

            Assert.That(actual, Is.EqualTo(result));
        }

        [Test]
        public void SortXmlPayloadTest()
        {
            string expectedResult = "1CREATE ENC CELLENC CELLS57US4AK6NTUS4AK6NTUS1mediumNorton Sound - Alaska82";
            string xpathActionItems = $"//*[local-name()='ACTIONITEMS']";
            var sapReqXml = TestHelper.ReadFileData("ERPTestData\\ActionItemNodeTest.xml");

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(sapReqXml);
            XmlNode actionItemNode = xmlDoc.SelectSingleNode(xpathActionItems)!;

            MethodInfo xmlPayLoad = typeof(EncContentSapMessageBuilder).GetMethod("SortXmlPayload", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance)!;
            var result = (XmlNode)xmlPayLoad.Invoke(_fakeEncContentSapMessageBuilder, new object[] { actionItemNode })!;

            var firstNode = result.Cast<XmlNode>().FirstOrDefault().InnerText;
            Assert.That(expectedResult, Is.EqualTo(firstNode));
        }

        [Test]
        public void BuildActionTest()
        {
            var actualXmlElement = @"<ACTIONNUMBER>1</ACTIONNUMBER><ACTION>CREATE ENC CELL</ACTION><PRODUCT>ENC CELL</PRODUCT><PRODTYPE>S57</PRODTYPE><CHILDCELL>US5AK83M</CHILDCELL><PRODUCTNAME>US5AK83M</PRODUCTNAME><CANCELLED></CANCELLED><REPLACEDBY></REPLACEDBY><AGENCY>US</AGENCY><PROVIDER>1</PROVIDER><ENCSIZE>small</ENCSIZE><TITLE>St. Michael Bay</TITLE><EDITIONNO>0</EDITIONNO><UPDATENO>1</UPDATENO><UNITTYPE></UNITTYPE><WEEKNO></WEEKNO><VALIDFROM></VALIDFROM><CORRECTION></CORRECTION><ACTIVEKEY></ACTIVEKEY><NEXTKEY></NEXTKEY>";
            var permitKeys = new PermitKey { ActiveKey = "", NextKey = "" };

            var scenarios = JsonConvert.DeserializeObject<EncEventPayload>(ScenariosDataCancelReplaceCell);
            XmlDocument soapXml = new();
            soapXml.LoadXml(SapXmlFile);
            A.CallTo(() => _fakePermitDecryption.GetPermitKeys(A<string>.Ignored)).Returns(permitKeys);

            MethodInfo getUnitOfSaleForEncCell = typeof(EncContentSapMessageBuilder).GetMethod("GetUnitOfSaleForEncCell", BindingFlags.NonPublic | BindingFlags.Instance)!;
            var unitOfSale = (UnitOfSale)getUnitOfSaleForEncCell.Invoke(_fakeEncContentSapMessageBuilder, new object[] { scenarios.Data.UnitsOfSales,
                scenarios.Data.Products.FirstOrDefault()! })!;

            var action = _fakeSapActionConfig.Value.SapActions.FirstOrDefault(x => x.Product == EncCell);

            MethodInfo buildAction = typeof(EncContentSapMessageBuilder).GetMethod("BuildAction", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance)!;
            var result = (XmlElement)buildAction.Invoke(_fakeEncContentSapMessageBuilder, new object[] {soapXml,scenarios.Data.Products.FirstOrDefault()!,
                unitOfSale,action!,null,null,null})!;

            result.ChildNodes.Count.Should().Be(20);
            result.InnerXml.Should().Be(actualXmlElement);
        }

        [Test]
        public void WhenUnitOfSaleIsNull_ThenReturnsXmlPayloadWithEmptyNode()
        {
            var scenarios = JsonConvert.DeserializeObject<EncEventPayload>(ScenariosDataCancelReplaceCell);

            XmlDocument soapXml = new();
            soapXml.LoadXml(SapXmlFile);

            A.CallTo(() => _fakeWeekDetailsProvider.GetDateOfWeek(A<int>.Ignored, A<int>.Ignored, A<bool>.Ignored)).Returns("20240118");

            var action = _fakeSapActionConfig.Value.SapActions.FirstOrDefault(x => x.Product == EncCell);

            MethodInfo buildAction = typeof(EncContentSapMessageBuilder).GetMethod("BuildAction", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance)!;
            var result = (XmlElement)buildAction.Invoke(_fakeEncContentSapMessageBuilder, new object[] {soapXml,scenarios.Data.Products.FirstOrDefault()!,
                null,action!,null,null,null})!;

            result.ChildNodes.Count.Should().Be(20);
            var productName = result.SelectSingleNode(XpathProductName);
            productName.InnerXml.Should().BeEmpty();
        }

        [Test]
        public void WhenValidUkhoWeekNumberIsPassed_ThenReturnsXmlPayloadWithNodeValues()
        {
            var scenarios = JsonConvert.DeserializeObject<EncEventPayload>(ScenariosDataCancelReplaceCell);

            XmlDocument soapXml = new();
            soapXml.LoadXml(SapXmlFile);

            A.CallTo(() => _fakeWeekDetailsProvider.GetDateOfWeek(A<int>.Ignored, A<int>.Ignored, A<bool>.Ignored)).Returns("20240118");

            MethodInfo getUnitOfSaleForEncCell = typeof(EncContentSapMessageBuilder).GetMethod("GetUnitOfSaleForEncCell", BindingFlags.NonPublic | BindingFlags.Instance)!;
            var unitOfSale = (UnitOfSale)getUnitOfSaleForEncCell.Invoke(_fakeEncContentSapMessageBuilder, new object[] { scenarios.Data.UnitsOfSales,
                scenarios.Data.Products.FirstOrDefault()! })!;

            var action = _fakeSapActionConfig.Value.SapActions.FirstOrDefault(x => x.Product == EncCell);

            MethodInfo buildAction = typeof(EncContentSapMessageBuilder).GetMethod("BuildAction", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance)!;
            var result = (XmlElement)buildAction.Invoke(_fakeEncContentSapMessageBuilder, new object[] {soapXml,scenarios.Data.Products.FirstOrDefault()!,
                unitOfSale,action!,scenarios.Data.UkhoWeekNumber,null,null})!;

            result.ChildNodes.Count.Should().Be(20);
            var correction = result.SelectSingleNode(XpathCorrection);
            var weekNo = result.SelectSingleNode(XpathWeekNo);
            var validFrom = result.SelectSingleNode(XpathValidFrom);
            correction.InnerXml.Should().Be("N");
            weekNo.InnerXml.Should().Be("202403");
            validFrom.InnerXml.Should().Be("20240118");
        }

        [Test]
        public void WhenUkhoWeekNumberIsNullOrNodeRequiredIsFalse_ThenReturnsXmlPayloadWithEmptyNodes()
        {
            var scenarios = JsonConvert.DeserializeObject<EncEventPayload>(ScenariosDataCancelReplaceCell);

            XmlDocument soapXml = new();
            soapXml.LoadXml(SapXmlFile);

            MethodInfo getUnitOfSaleForEncCell = typeof(EncContentSapMessageBuilder).GetMethod("GetUnitOfSaleForEncCell", BindingFlags.NonPublic | BindingFlags.Instance)!;
            var unitOfSale = (UnitOfSale)getUnitOfSaleForEncCell.Invoke(_fakeEncContentSapMessageBuilder, new object[] { scenarios.Data.UnitsOfSales,
                scenarios.Data.Products.FirstOrDefault()! })!;

            var action = _fakeSapActionConfig.Value.SapActions.FirstOrDefault(x => x.Product == EncCell);
            action.Attributes.LastOrDefault().IsRequired = false;

            MethodInfo buildAction = typeof(EncContentSapMessageBuilder).GetMethod("BuildAction", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance)!;
            var result = (XmlElement)buildAction.Invoke(_fakeEncContentSapMessageBuilder, new object[] {soapXml,scenarios.Data.Products.FirstOrDefault()!,
                unitOfSale,action!,null,null,null})!;

            result.ChildNodes.Count.Should().Be(20);
            var correction = result.SelectSingleNode(XpathCorrection);
            var weekNo = result.SelectSingleNode(XpathWeekNo);
            var validFrom = result.SelectSingleNode(XpathValidFrom);
            correction.InnerXml.Should().BeEmpty();
            weekNo.InnerXml.Should().BeEmpty();
            validFrom.InnerXml.Should().BeEmpty();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
            && call.GetArgument<LogLevel>(0) == LogLevel.Error
            && call.GetArgument<EventId>(1) == EventIds.InvalidUkhoWeekNumber.ToEventId()
            && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Invalid UkhoWeekNumber field received in enccontentpublished event.").MustHaveHappened();
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

            MethodInfo getUkhoWeekNumber = typeof(EncContentSapMessageBuilder).GetMethod("GetUkhoWeekNumberData", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance)!;
            string result = (string)getUkhoWeekNumber.Invoke(_fakeEncContentSapMessageBuilder, new object[] { ukhoWeekNumber })!;

            Assert.That(expectedResult, Is.EqualTo(result));
        }

        [Test]
        [TestCase(0, 0, false)]
        [TestCase(52, 0, false)]
        [TestCase(0, 2023, false)]
        public void ValidWeekNumberTest_WhenInvalidUkhoWeekNumberIsPassed(int validWeek, int validYear, bool expectedResult)
        {
            UkhoWeekNumber ukhoWeekNumber = new()
            {
                Year = validYear,
                Week = validWeek
            };

            MethodInfo isValidWeekNumber = typeof(EncContentSapMessageBuilder).GetMethod("IsValidWeekNumber", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance)!;
            bool result = (bool)isValidWeekNumber.Invoke(_fakeEncContentSapMessageBuilder, new object[] { ukhoWeekNumber })!;

            Assert.That(expectedResult, Is.EqualTo(result));
        }
    }
}
