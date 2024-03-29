﻿using System.Reflection;
using System.Xml;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NUnit.Framework;
using UKHO.ERPFacade.Common.IO;
using UKHO.ERPFacade.Common.Logging;
using UKHO.ERPFacade.Common.Models;
using UKHO.ERPFacade.EventAggregation.WebJob.Helpers;

namespace UKHO.ERPFacade.EventAggregation.WebJob.UnitTests.Helpers
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
        private readonly string XpathSoldToAcc = $"//*[local-name()='SOLDTOACC']";
        private readonly string XpathLicenseEAcc = $"//*[local-name()='LICENSEEACC']";
        private readonly string XpathStartDate = $"//*[local-name()='STARTDATE']";
        private readonly string XpathEndDate = $"//*[local-name()='ENDDATE']";
        private readonly string XpathVName = $"//*[local-name()='VNAME']";
        private readonly string XpathImo = $"//*[local-name()='IMO']";
        private readonly string XpathCallSign = $"//*[local-name()='CALLSIGN']";
        private readonly string XpathshoreBased = $"//*[local-name()='SHOREBASED']";
        private readonly string XpathEndUserId = $"//*[local-name()='ENDUSERID']";
        private readonly string XpathManuf = $"//*[local-name()='ECDISMANUF']";
        private readonly string XpathLType = $"//*[local-name()='LTYPE']";
        private readonly string XpathUsers = $"//*[local-name()='USERS']";
        private readonly string XpathLicDur = $"//*[local-name()='LICDUR']";
        private readonly string XpathRepeat = $"//*[local-name()='REPEAT']";

        #region Data

        private readonly string newLicencePayload = @"{""specversion"": ""1.0"",""type"": ""uk.gov.ukho.shop.recordOfSale.v1"",""source"": ""https://uk.gov.ukho.shop"",""id"": ""e744fa37-0c9f-4795-adc9-7f42ad8f11c1"",""time"": ""2023-07-20T10:40:00.000000Z"",""subject"": ""releasability set changes holdings Record of Sale"",""datacontenttype"": ""application/json"",""data"": {""correlationId"": ""123-abc-456-xyz-333"",""relatedEvents"": [""e744fa37-0c9f-4795-adc9-7f42ad8f11c1"", ""e744fa37-0c9f-4795-adc9-7f42ad8f1234""],""recordsOfSale"": 
            {""licenseId"": ""2"",""productType"": ""AVCS"",""transactionType"": ""NEWLICENCE"",""distributorCustomerNumber"": ""111"",""shippingCoNumber"": ""1"",""ordernumber"": ""5432796"",""orderDate"": ""2023-06-20"",""po-ref"": ""75277T"",""holdingsExpiryDate"": ""2025-06-30"",""sapId"": """",""vesselName"": ""Cornelia Maersk"",""imoNumber"": ""IMO9245756"",""callSign"": ""OWWS2"",""licenceType"": ""02"",""shoreBased"": ""X"",""fleetName"": """",""numberLicenceUsers"": 1,""ecdisManufacturerName"": ""MARIS"",""licenceDuration"": 12,""unitsOfSale"": [{""unitName"": ""PT111101"",""endDate"": ""2023-10-31"",""duration"": ""3"",""renew"": ""N"",""repeat"": """"},{""unitName"": ""GB302409"",""endDate"": ""2023-12-01"",""duration"": ""6"",""renew"": ""N"",""repeat"": """"}]}}}";
        private readonly string newLicencePayloadForMerging = @"{""specversion"": ""1.0"",""type"": ""uk.gov.ukho.shop.recordOfSale.v1"",""source"": ""https://uk.gov.ukho.shop"",""id"": ""e744fa37-0c9f-4795-adc9-7f42ad8f1234"",""time"": ""2023-07-20T10:40:00.000000Z"",""subject"": ""releasability set changes holdings Record of Sale"",""datacontenttype"": ""application/json"",""data"": {""correlationId"": ""123-abc-456-xyz-333"",""relatedEvents"": [""e744fa37-0c9f-4795-adc9-7f42ad8f11c1"", ""e744fa37-0c9f-4795-adc9-7f42ad8f1234""],""recordsOfSale"": 
            {""licenseId"": ""2"",""productType"": ""AVCS"",""transactionType"": ""NEWLICENCE"",""distributorCustomerNumber"": ""111"",""shippingCoNumber"": ""1"",""ordernumber"": ""5432796"",""orderDate"": ""2023-06-20"",""po-ref"": ""75277T"",""holdingsExpiryDate"": ""2025-06-30"",""sapId"": """",""vesselName"": ""Cornelia Maersk"",""imoNumber"": ""IMO9245756"",""callSign"": ""OWWS2"",""licenceType"": ""02"",""shoreBased"": ""X"",""fleetName"": """",""numberLicenceUsers"": 1,""ecdisManufacturerName"": ""MARIS"",""licenceDuration"": 12,""unitsOfSale"": [{""unitName"": ""PT111102"",""endDate"": ""2023-10-31"",""duration"": ""3"",""renew"": ""N"",""repeat"": """"},{""unitName"": ""GB302408"",""endDate"": ""2023-12-01"",""duration"": ""6"",""renew"": ""N"",""repeat"": """"}]}}}";

        private readonly string maintainHoldingsPayload = @"{""specversion"":""1.0"",""type"":""uk.gov.ukho.shop.recordOfSale.v1"",""source"":""https://uk.gov.ukho.shop"",""id"":""e744fa37-0c9f-4795-adc9-7f42ad8f11c1"",""time"":""2023-07-20T10:40:00Z"",""subject"":""releasability set changes holdings Record of Sale"",""datacontenttype"":""application/json"",""data"":{""correlationId"":""123-abc-456-xyz-333"",""relatedEvents"":[""e744fa37-0c9f-4795-adc9-7f42ad8f11c1"", ""e744fa37-0c9f-4795-adc9-7f42ad8f1234""],""recordsOfSale"":{""licenseId"":"""",""productType"":""AVCS"",""transactionType"":""MAINTAINHOLDINGS"",""distributorCustomerNumber"":"""",""shippingCoNumber"":"""",""ordernumber"":""5432796"",""orderDate"":"""",""po-ref"":""75277T"",""holdingsExpiryDate"":"""",""sapId"":""75277T"",""vesselName"":"""",""imoNumber"":"""",""callSign"":"""",""licenceType"":"""",""shoreBased"":"""",""fleetName"":"""",""numberLicenceUsers"":null,""ecdisManufacturerName"":"""",""licenceDuration"":null,""unitsOfSale"":[{""unitName"":""PT111101"",""endDate"":""2023-10-31"",""duration"":""3"",""renew"":""E"",""repeat"":""P""},{""unitName"":""GB302409"",""endDate"":""2023-12-01"",""duration"":""6"",""renew"":""E"",""repeat"":""P""}]}}}";
        private readonly string maintainHoldingsPayloadForMerging = @"{""specversion"":""1.0"",""type"":""uk.gov.ukho.shop.recordOfSale.v1"",""source"":""https://uk.gov.ukho.shop"",""id"":""e744fa37-0c9f-4795-adc9-7f42ad8f1234"",""time"":""2023-07-20T10:40:00Z"",""subject"":""releasability set changes holdings Record of Sale"",""datacontenttype"":""application/json"",""data"":{""correlationId"":""123-abc-456-xyz-333"",""relatedEvents"":[""e744fa37-0c9f-4795-adc9-7f42ad8f11c1"", ""e744fa37-0c9f-4795-adc9-7f42ad8f1234""],""recordsOfSale"":{""licenseId"":"""",""productType"":""AVCS"",""transactionType"":""MAINTAINHOLDINGS"",""distributorCustomerNumber"":"""",""shippingCoNumber"":"""",""ordernumber"":""5432796"",""orderDate"":"""",""po-ref"":""75277T"",""holdingsExpiryDate"":"""",""sapId"":""75277T"",""vesselName"":"""",""imoNumber"":"""",""callSign"":"""",""licenceType"":"""",""shoreBased"":"""",""fleetName"":"""",""numberLicenceUsers"":null,""ecdisManufacturerName"":"""",""licenceDuration"":null,""unitsOfSale"":[{""unitName"":""PT111102"",""endDate"":""2023-10-31"",""duration"":""3"",""renew"":""E"",""repeat"":""P""},{""unitName"":""GB302408"",""endDate"":""2023-12-01"",""duration"":""6"",""renew"":""E"",""repeat"":""P""}]}}}";

        private readonly string migrateNewLicencePayload = @"{""specversion"": ""1.0"",""type"": ""uk.gov.ukho.shop.recordOfSale.v1"",""source"": ""https://uk.gov.ukho.shop"",""id"": ""e744fa37-0c9f-4795-adc9-7f42ad8f11c1"",""time"": ""2023-07-20T10:40:00.000000Z"",""subject"": ""releasability set changes holdings Record of Sale"",""datacontenttype"": ""application/json"",""data"": {""correlationId"": ""123-abc-456-xyz-333"",""relatedEvents"": [""e744fa37-0c9f-4795-adc9-7f42ad8f11c1"", ""e744fa37-0c9f-4795-adc9-7f42ad8f1234""],""recordsOfSale"": 
            {""licenseId"": ""2"",""productType"": ""AVCS"",""transactionType"": ""MIGRATENEWLICENCE"",""distributorCustomerNumber"": ""111"",""shippingCoNumber"": ""1"",""ordernumber"": ""XXL005456375"",""orderDate"": ""2023-06-20"",""po-ref"": ""75277T-Bengang"",""holdingsExpiryDate"": ""2025-06-30"",""sapId"": """",""vesselName"": ""Cornelia Maersk"",""imoNumber"": ""IMO9245756"",""callSign"": ""OWWS2"",""licenceType"": ""02"",""shoreBased"": ""X"",""fleetName"": """",""numberLicenceUsers"": 1,""ecdisManufacturerName"": ""MARIS"",""licenceDuration"": 12,""unitsOfSale"": [{""unitName"": ""PT111101"",""endDate"": ""2023-10-31"",""duration"": ""3"",""renew"": ""N"",""repeat"": """"},{""unitName"": ""GB302409"",""endDate"": ""2023-12-01"",""duration"": ""6"",""renew"": ""N"",""repeat"": """"}]}}}";

        private readonly string migrateNewLicencePayloadForMerging = @"{""specversion"": ""1.0"",""type"": ""uk.gov.ukho.shop.recordOfSale.v1"",""source"": ""https://uk.gov.ukho.shop"",""id"": ""e744fa37-0c9f-4795-adc9-7f42ad8f1234"",""time"": ""2023-07-20T10:40:00.000000Z"",""subject"": ""releasability set changes holdings Record of Sale"",""datacontenttype"": ""application/json"",""data"": {""correlationId"": ""123-abc-456-xyz-333"",""relatedEvents"": [""e744fa37-0c9f-4795-adc9-7f42ad8f11c1"", ""e744fa37-0c9f-4795-adc9-7f42ad8f1234""],""recordsOfSale"": 
            {""licenseId"": ""2"",""productType"": ""AVCS"",""transactionType"": ""MIGRATENEWLICENCE"",""distributorCustomerNumber"": ""111"",""shippingCoNumber"": ""1"",""ordernumber"": ""XXL005456375"",""orderDate"": ""2023-06-20"",""po-ref"": ""75277T-Bengang"",""holdingsExpiryDate"": ""2025-06-30"",""sapId"": """",""vesselName"": ""Cornelia Maersk"",""imoNumber"": ""IMO9245756"",""callSign"": ""OWWS2"",""licenceType"": ""02"",""shoreBased"": ""X"",""fleetName"": """",""numberLicenceUsers"": 1,""ecdisManufacturerName"": ""MARIS"",""licenceDuration"": 12,""unitsOfSale"": [{""unitName"": ""PT111102"",""endDate"": ""2023-10-31"",""duration"": ""3"",""renew"": ""N"",""repeat"": """"},{""unitName"": ""GB302408"",""endDate"": ""2023-12-01"",""duration"": ""6"",""renew"": ""N"",""repeat"": """"}]}}}";

        private readonly string migrateExistingLicencePayload = @"{""specversion"":""1.0"",""type"":""uk.gov.ukho.shop.recordOfSale.v1"",""source"":""https://uk.gov.ukho.shop"",""id"":""e744fa37-0c9f-4795-adc9-7f42ad8f11c1"",""time"":""2023-07-20T10:40:00Z"",""subject"":""releasability set changes holdings Record of Sale"",""datacontenttype"":""application/json"",""data"":{""correlationId"":""123-abc-456-xyz-333"",""relatedEvents"":[""e744fa37-0c9f-4795-adc9-7f42ad8f11c1"", ""e744fa37-0c9f-4795-adc9-7f42ad8f1234""],""recordsOfSale"":{""licenseId"":"""",""productType"":""AVCS"",""transactionType"":""MIGRATEEXISTINGLICENCE"",""distributorCustomerNumber"":"""",""shippingCoNumber"":"""",""ordernumber"":""XXL005456375"",""orderDate"":"""",""po-ref"":""75277T-Bengang"",""holdingsExpiryDate"":"""",""sapId"":""75277T"",""vesselName"":"""",""imoNumber"":"""",""callSign"":"""",""licenceType"":"""",""shoreBased"":"""",""fleetName"":"""",""numberLicenceUsers"":null,""ecdisManufacturerNameacturerName"":"""",""licenceDuration"":null,""unitsOfSale"":[{""unitName"":""PT111101"",""endDate"":""2023-10-31"",""duration"":""3"",""renew"":""N"",""repeat"":""""},{""unitName"":""GB302409"",""endDate"":""2023-12-01"",""duration"":""6"",""renew"":""N"",""repeat"":""""}]}}}";
        private readonly string migrateExistingLicencePayloadForMerging = @"{""specversion"":""1.0"",""type"":""uk.gov.ukho.shop.recordOfSale.v1"",""source"":""https://uk.gov.ukho.shop"",""id"":""e744fa37-0c9f-4795-adc9-7f42ad8f1234"",""time"":""2023-07-20T10:40:00Z"",""subject"":""releasability set changes holdings Record of Sale"",""datacontenttype"":""application/json"",""data"":{""correlationId"":""123-abc-456-xyz-333"",""relatedEvents"":[""e744fa37-0c9f-4795-adc9-7f42ad8f11c1"", ""e744fa37-0c9f-4795-adc9-7f42ad8f1234""],""recordsOfSale"":{""licenseId"":"""",""productType"":""AVCS"",""transactionType"":""MIGRATEEXISTINGLICENCE"",""distributorCustomerNumber"":"""",""shippingCoNumber"":"""",""ordernumber"":""XXL005456375"",""orderDate"":"""",""po-ref"":""75277T-Bengang"",""holdingsExpiryDate"":"""",""sapId"":""75277T"",""vesselName"":"""",""imoNumber"":"""",""callSign"":"""",""licenceType"":"""",""shoreBased"":"""",""fleetName"":"""",""numberLicenceUsers"":null,""ecdisManufacturerNameacturerName"":"""",""licenceDuration"":null,""unitsOfSale"":[{""unitName"":""PT111102"",""endDate"":""2023-10-31"",""duration"":""3"",""renew"":""N"",""repeat"":""""},{""unitName"":""GB302408"",""endDate"":""2023-12-01"",""duration"":""6"",""renew"":""N"",""repeat"":""""}]}}}";

        private readonly string convertLicencePayload = @"{""specversion"":""1.0"",""type"":""uk.gov.ukho.shop.recordOfSale.v1"",""source"":""https://uk.gov.ukho.shop"",""id"":""e744fa37-0c9f-4795-adc9-7f42ad8f11c1"",""time"":""2023-07-20T10:40:00Z"",""subject"":""releasability set changes holdings Record of Sale"",""datacontenttype"":""application/json"",""data"":{""correlationId"":""123-abc-456-xyz-333"",""relatedEvents"":[""e744fa37-0c9f-4795-adc9-7f42ad8f11c1"", ""e744fa37-0c9f-4795-adc9-7f42ad8f1234""],""recordsOfSale"":{""licenseId"":"""",""productType"":""AVCS"",""transactionType"":""CONVERTLICENCE"",""distributorCustomerNumber"":"""",""shippingCoNumber"":"""",""ordernumber"":""5432796"",""orderDate"":"""",""po-ref"":""75277T"",""holdingsExpiryDate"":"""",""sapId"":""75277T"",""vesselName"":"""",""imoNumber"":"""",""callSign"":"""",""licenceType"":""1"",""shoreBased"":"""",""fleetName"":"""",""numberLicenceUsers"":null,""ecdisManufacturerName"":"""",""licenceDuration"":""12"",""unitsOfSale"":[{""unitName"":""PT111101"",""endDate"":""2023-10-31"",""duration"":""3"",""renew"":""N"",""repeat"":""""},{""unitName"":""GB302409"",""endDate"":""2023-12-01"",""duration"":""6"",""renew"":""N"",""repeat"":""""}]}}}";
        private readonly string convertLicencePayloadForMerging = @"{""specversion"":""1.0"",""type"":""uk.gov.ukho.shop.recordOfSale.v1"",""source"":""https://uk.gov.ukho.shop"",""id"":""e744fa37-0c9f-4795-adc9-7f42ad8f1234"",""time"":""2023-07-20T10:40:00Z"",""subject"":""releasability set changes holdings Record of Sale"",""datacontenttype"":""application/json"",""data"":{""correlationId"":""123-abc-456-xyz-333"",""relatedEvents"":[""e744fa37-0c9f-4795-adc9-7f42ad8f11c1"", ""e744fa37-0c9f-4795-adc9-7f42ad8f1234""],""recordsOfSale"":{""licenseId"":"""",""productType"":""AVCS"",""transactionType"":""CONVERTLICENCE"",""distributorCustomerNumber"":"""",""shippingCoNumber"":"""",""ordernumber"":""5432796"",""orderDate"":"""",""po-ref"":""75277T"",""holdingsExpiryDate"":"""",""sapId"":""75277T"",""vesselName"":"""",""imoNumber"":"""",""callSign"":"""",""licenceType"":""1"",""shoreBased"":"""",""fleetName"":"""",""numberLicenceUsers"":null,""ecdisManufacturerName"":"""",""licenceDuration"":""12"",""unitsOfSale"":[{""unitName"":""PT111102"",""endDate"":""2023-10-31"",""duration"":""3"",""renew"":""N"",""repeat"":""""},{""unitName"":""GB302408"",""endDate"":""2023-12-01"",""duration"":""6"",""renew"":""N"",""repeat"":""""}]}}}";

        #endregion

        [SetUp]
        public void Setup()
        {
            _fakeLogger = A.Fake<ILogger<RecordOfSaleSapMessageBuilder>>();
            _fakeXmlHelper = new XmlHelper();
            _fakeFileSystemHelper = A.Fake<IFileSystemHelper>();
            _fakeRecordOfSaleSapMessageBuilder = new RecordOfSaleSapMessageBuilder(_fakeLogger, _fakeXmlHelper, _fakeFileSystemHelper);
        }

        [Test]
        public void WhenTransactionTypeIsNewLicence_ThenReturnXMLDocument()
        {
            var newLicencePayloadJson = JsonConvert.DeserializeObject<RecordOfSaleEventPayLoad>(newLicencePayload);
            var newLicencePayloadJsonForMerging = JsonConvert.DeserializeObject<RecordOfSaleEventPayLoad>(newLicencePayloadForMerging);

            List<RecordOfSaleEventPayLoad> rosNewLicenceData = new()
            {
                newLicencePayloadJson!, newLicencePayloadJsonForMerging!
            };

            const string correlationId = "123-abc-456-xyz-333";

            A.CallTo(() => _fakeFileSystemHelper.IsFileExists(A<string>.Ignored)).Returns(true);
           
            var result = _fakeRecordOfSaleSapMessageBuilder.BuildRecordOfSaleSapMessageXml(rosNewLicenceData, correlationId);

            result.Should().BeOfType<XmlDocument>();

            var actionItem = result.SelectSingleNode(XpathZAddsRos);
            actionItem.ChildNodes.Count.Should().Be(1);
            actionItem.ChildNodes[0].ChildNodes.Count.Should().Be(21);

            var licNo = result.SelectSingleNode(XpathLicNo);
            var fleet = result.SelectSingleNode(XpathFleet);
            licNo.InnerXml.Should().BeEmpty();
            fleet.InnerXml.Should().BeEmpty();

            var prodItem = result.SelectSingleNode(XpathProd);
            prodItem.ChildNodes.Count.Should().Be(4);
            prodItem.ChildNodes[0].ChildNodes.Count.Should().Be(5);

            var repeat = result.SelectSingleNode(XpathRepeat);
            repeat.InnerXml.Should().BeEmpty();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
            && call.GetArgument<LogLevel>(0) == LogLevel.Information
            && call.GetArgument<EventId>(1) == EventIds.CreatingRecordOfSaleSapPayload.ToEventId()
            && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Creating the record of sale SAP Payload. | _X-Correlation-ID : {_X-Correlation-ID}").MustHaveHappenedOnceExactly();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
            && call.GetArgument<LogLevel>(0) == LogLevel.Information
            && call.GetArgument<EventId>(1) == EventIds.CreatedRecordOfSaleSapPayload.ToEventId()
            && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "The record of sale SAP payload created. | _X-Correlation-ID : {_X-Correlation-ID}").MustHaveHappenedOnceExactly();
        }

        [Test]
        public void WhenRecordOfSaleSapXmlTemplateFileNotExist_ThenThrowFileNotFoundException()
        {
            var newLicencePayloadJson = JsonConvert.DeserializeObject<RecordOfSaleEventPayLoad>(newLicencePayload);
            var newLicencePayloadJsonForMerging = JsonConvert.DeserializeObject<RecordOfSaleEventPayLoad>(newLicencePayloadForMerging);

            List<RecordOfSaleEventPayLoad> rosNewLicenceData = new()
            {
                newLicencePayloadJson!, newLicencePayloadJsonForMerging!
            };

            const string correlationId = "123-abc-456-xyz-333";

            A.CallTo(() => _fakeFileSystemHelper.IsFileExists(A<string>.Ignored)).Returns(false);

            Assert.Throws<FileNotFoundException>(() => _fakeRecordOfSaleSapMessageBuilder.BuildRecordOfSaleSapMessageXml(rosNewLicenceData, correlationId));

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
            && call.GetArgument<LogLevel>(0) == LogLevel.Error
            && call.GetArgument<EventId>(1) == EventIds.RecordOfSaleSapXmlTemplateNotFound.ToEventId()
            && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "The record of sale SAP message xml template does not exist. | _X-Correlation-ID : {_X-Correlation-ID}").MustHaveHappenedOnceExactly();
        }

        [Test]
        public void WhenTransactionTypeIsNewLicence_ThenReturns_SapXmlPayloadWithSomeEmptyFields()
        {
            var newLicencePayloadJson = JsonConvert.DeserializeObject<RecordOfSaleEventPayLoad>(newLicencePayload);
            var newLicencePayloadJsonForMerging = JsonConvert.DeserializeObject<RecordOfSaleEventPayLoad>(newLicencePayloadForMerging);

            List<RecordOfSaleEventPayLoad> rosNewLicenceData = new()
            {
                newLicencePayloadJson!, newLicencePayloadJsonForMerging!
            };

            MethodInfo methodInfo = typeof(RecordOfSaleSapMessageBuilder).GetMethod("BuildNewLicencePayload", BindingFlags.NonPublic | BindingFlags.Instance)!;
            var result = (SapRecordOfSalePayLoad)methodInfo.Invoke(_fakeRecordOfSaleSapMessageBuilder, new object[] { rosNewLicenceData })!;

            result.Should().NotBeNull();
            result.CorrelationId.Should().Be("123-abc-456-xyz-333");
            result.PROD.UnitOfSales.Count.Should().Be(4);
            result.LicenceNumber.Should().Be("");
            result.FleetName.Should().Be("");
            result.PROD.UnitOfSales[0].Repeat.Should().Be("");
            result.PROD.UnitOfSales[1].Repeat.Should().Be("");
            result.PROD.UnitOfSales[2].Repeat.Should().Be("");
            result.PROD.UnitOfSales[3].Repeat.Should().Be("");
        }

        [Test]
        public void WhenTransactionTypeIsMaintainHoldings_ThenReturnXMLDocument()
        {
            var maintainHoldingsPayloadJson = JsonConvert.DeserializeObject<RecordOfSaleEventPayLoad>(maintainHoldingsPayload);
            var maintainHoldingsPayloadJsonForMerging = JsonConvert.DeserializeObject<RecordOfSaleEventPayLoad>(maintainHoldingsPayloadForMerging);

            List<RecordOfSaleEventPayLoad> rosMaintainHoldingsData = new()
            {
                maintainHoldingsPayloadJson!, maintainHoldingsPayloadJsonForMerging!
            };

            const string correlationId = "123-abc-456-xyz-333";

            A.CallTo(() => _fakeFileSystemHelper.IsFileExists(A<string>.Ignored)).Returns(true);
          
            var result = _fakeRecordOfSaleSapMessageBuilder.BuildRecordOfSaleSapMessageXml(rosMaintainHoldingsData, correlationId);

            result.Should().BeOfType<XmlDocument>();

            var actionItem = result.SelectSingleNode(XpathZAddsRos);
            actionItem.ChildNodes.Count.Should().Be(1);
            actionItem.ChildNodes[0].ChildNodes.Count.Should().Be(21);

            var soldToAcc = result.SelectSingleNode(XpathSoldToAcc);
            var licenseEAcc = result.SelectSingleNode(XpathLicenseEAcc);
            var startDate = result.SelectSingleNode(XpathStartDate);
            var endDate = result.SelectSingleNode(XpathEndDate);
            var vName = result.SelectSingleNode(XpathVName);
            var imo = result.SelectSingleNode(XpathImo);
            var callSign = result.SelectSingleNode(XpathCallSign);
            var shoreBased = result.SelectSingleNode(XpathshoreBased);
            var fleet = result.SelectSingleNode(XpathFleet);
            var endUserId = result.SelectSingleNode(XpathEndUserId);
            var manuf = result.SelectSingleNode(XpathManuf);
            var lType = result.SelectSingleNode(XpathLType);
            var users = result.SelectSingleNode(XpathUsers);
            var licDur = result.SelectSingleNode(XpathLicDur);

            soldToAcc.InnerXml.Should().BeEmpty();
            licenseEAcc.InnerXml.Should().BeEmpty();
            startDate.InnerXml.Should().BeEmpty();
            endDate.InnerXml.Should().BeEmpty();
            vName.InnerXml.Should().BeEmpty();
            imo.InnerXml.Should().BeEmpty();
            callSign.InnerXml.Should().BeEmpty();
            shoreBased.InnerXml.Should().BeEmpty();
            fleet.InnerXml.Should().BeEmpty();
            endUserId.InnerXml.Should().BeEmpty();
            manuf.InnerXml.Should().BeEmpty();
            lType.InnerXml.Should().BeEmpty();
            users.InnerXml.Should().BeNullOrEmpty();
            licDur.InnerXml.Should().BeNullOrEmpty();

            var prodItem = result.SelectSingleNode(XpathProd);
            prodItem.ChildNodes.Count.Should().Be(4);
            prodItem.ChildNodes[0].ChildNodes.Count.Should().Be(5);

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
            && call.GetArgument<LogLevel>(0) == LogLevel.Information
            && call.GetArgument<EventId>(1) == EventIds.CreatingRecordOfSaleSapPayload.ToEventId()
            && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Creating the record of sale SAP Payload. | _X-Correlation-ID : {_X-Correlation-ID}").MustHaveHappenedOnceExactly();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
            && call.GetArgument<LogLevel>(0) == LogLevel.Information
            && call.GetArgument<EventId>(1) == EventIds.CreatedRecordOfSaleSapPayload.ToEventId()
            && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "The record of sale SAP payload created. | _X-Correlation-ID : {_X-Correlation-ID}").MustHaveHappenedOnceExactly();
        }

        [Test]
        public void WhenTransactionTypeIsMaintainHoldings_ThenReturns_SapXmlPayloadWithSomeEmptyFields()
        {
            var maintainHoldingsPayloadJson = JsonConvert.DeserializeObject<RecordOfSaleEventPayLoad>(maintainHoldingsPayload);
            var maintainHoldingsPayloadJsonForMerging = JsonConvert.DeserializeObject<RecordOfSaleEventPayLoad>(maintainHoldingsPayloadForMerging);

            List<RecordOfSaleEventPayLoad> rosMaintainHoldingsData = new()
            {
                maintainHoldingsPayloadJson!, maintainHoldingsPayloadJsonForMerging!
            };

            MethodInfo methodInfo = typeof(RecordOfSaleSapMessageBuilder).GetMethod("BuildMaintainHoldingsPayload", BindingFlags.NonPublic | BindingFlags.Instance)!;
            var result = (SapRecordOfSalePayLoad)methodInfo.Invoke(_fakeRecordOfSaleSapMessageBuilder, new object[] { rosMaintainHoldingsData })!;

            result.Should().NotBeNull();
            result.CorrelationId.Should().Be("123-abc-456-xyz-333");
            result.SoldToAcc.Should().Be("");
            result.LicenseEacc.Should().Be("");
            result.StartDate.Should().Be("");
            result.EndDate.Should().Be("");
            result.VesselName.Should().Be("");
            result.IMONumber.Should().Be("");
            result.CallSign.Should().Be("");
            result.ShoreBased.Should().Be("");
            result.FleetName.Should().Be("");
            result.Users.Should().Be(null);
            result.EndUserId.Should().Be("");
            result.LicenceDuration.Should().Be(null);
            result.ECDISMANUF.Should().Be("");
            result.LicenceType.Should().Be("");
            result.PROD.UnitOfSales.Count.Should().Be(4);
            result.PROD.UnitOfSales[0].Repeat.Should().Be("P");
            result.PROD.UnitOfSales[1].Repeat.Should().Be("P");
            result.PROD.UnitOfSales[2].Repeat.Should().Be("P");
            result.PROD.UnitOfSales[3].Repeat.Should().Be("P");
        }

        [Test]
        public void WhenTransactionTypeIsMigrateNewLicence_ThenReturnXMLDocument()
        {
            var migrateNewLicencePayloadJson = JsonConvert.DeserializeObject<RecordOfSaleEventPayLoad>(migrateNewLicencePayload);
            var migrateNewLicencePayloadJsonForMerging = JsonConvert.DeserializeObject<RecordOfSaleEventPayLoad>(migrateNewLicencePayloadForMerging);

            List<RecordOfSaleEventPayLoad> rosMigrateNewLicenceData = new()
            {
                migrateNewLicencePayloadJson!, migrateNewLicencePayloadJsonForMerging!
            };

            const string correlationId = "123-abc-456-xyz-333";
           
            A.CallTo(() => _fakeFileSystemHelper.IsFileExists(A<string>.Ignored)).Returns(true);
           
            var result = _fakeRecordOfSaleSapMessageBuilder.BuildRecordOfSaleSapMessageXml(rosMigrateNewLicenceData, correlationId);

            result.Should().BeOfType<XmlDocument>();

            var actionItem = result.SelectSingleNode(XpathZAddsRos);
            actionItem.ChildNodes.Count.Should().Be(1);
            actionItem.ChildNodes[0].ChildNodes.Count.Should().Be(21);

            var licNo = result.SelectSingleNode(XpathLicNo);
            var fleet = result.SelectSingleNode(XpathFleet);
            licNo.InnerXml.Should().BeEmpty();
            fleet.InnerXml.Should().BeEmpty();

            var prodItem = result.SelectSingleNode(XpathProd);
            prodItem.ChildNodes.Count.Should().Be(4);
            prodItem.ChildNodes[0].ChildNodes.Count.Should().Be(5);

            var repeat = result.SelectSingleNode(XpathRepeat);
            repeat.InnerXml.Should().BeEmpty();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
            && call.GetArgument<LogLevel>(0) == LogLevel.Information
            && call.GetArgument<EventId>(1) == EventIds.CreatingRecordOfSaleSapPayload.ToEventId()
            && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Creating the record of sale SAP Payload. | _X-Correlation-ID : {_X-Correlation-ID}").MustHaveHappenedOnceExactly();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
            && call.GetArgument<LogLevel>(0) == LogLevel.Information
            && call.GetArgument<EventId>(1) == EventIds.CreatedRecordOfSaleSapPayload.ToEventId()
            && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "The record of sale SAP payload created. | _X-Correlation-ID : {_X-Correlation-ID}").MustHaveHappenedOnceExactly();
        }

        [Test]
        public void WhenTransactionTypeIsMigrateNewLicence_ThenReturns_SapXmlPayloadWithSomeEmptyFields()
        {
            var migrateNewLicencePayloadJson = JsonConvert.DeserializeObject<RecordOfSaleEventPayLoad>(migrateNewLicencePayload);
            var migrateNewLicencePayloadJsonForMerging = JsonConvert.DeserializeObject<RecordOfSaleEventPayLoad>(migrateNewLicencePayloadForMerging);

            List<RecordOfSaleEventPayLoad> rosMigrateNewLicenceData = new()
            {
                migrateNewLicencePayloadJson!, migrateNewLicencePayloadJsonForMerging!
            };

            MethodInfo methodInfo = typeof(RecordOfSaleSapMessageBuilder).GetMethod("BuildMigrateNewLicencePayload", BindingFlags.NonPublic | BindingFlags.Instance)!;
            var result = (SapRecordOfSalePayLoad)methodInfo.Invoke(_fakeRecordOfSaleSapMessageBuilder, new object[] { rosMigrateNewLicenceData })!;

            result.Should().NotBeNull();
            result.CorrelationId.Should().Be("123-abc-456-xyz-333");
            result.PROD.UnitOfSales.Count.Should().Be(4);
            result.LicenceNumber.Should().Be("");
            result.FleetName.Should().Be("");
            result.PROD.UnitOfSales[0].Repeat.Should().Be("");
            result.PROD.UnitOfSales[1].Repeat.Should().Be("");
            result.PROD.UnitOfSales[2].Repeat.Should().Be("");
            result.PROD.UnitOfSales[3].Repeat.Should().Be("");
        }

        [Test]
        public void WhenTransactionTypeIsMigrateExistingLicence_ThenReturnXMLDocument()
        {
            var migrateExistingLicencePayloadJson = JsonConvert.DeserializeObject<RecordOfSaleEventPayLoad>(migrateExistingLicencePayload);
            var migrateExistingLicencePayloadJsonForMerging = JsonConvert.DeserializeObject<RecordOfSaleEventPayLoad>(migrateExistingLicencePayloadForMerging);

            List<RecordOfSaleEventPayLoad> rosMigrateExistingLicenceData = new()
            {
                migrateExistingLicencePayloadJson!, migrateExistingLicencePayloadJsonForMerging!
            };

            const string correlationId = "123-abc-456-xyz-333";
            
            A.CallTo(() => _fakeFileSystemHelper.IsFileExists(A<string>.Ignored)).Returns(true);
            
            var result = _fakeRecordOfSaleSapMessageBuilder.BuildRecordOfSaleSapMessageXml(rosMigrateExistingLicenceData, correlationId);

            result.Should().BeOfType<XmlDocument>();

            var actionItem = result.SelectSingleNode(XpathZAddsRos);
            actionItem.ChildNodes.Count.Should().Be(1);
            actionItem.ChildNodes[0].ChildNodes.Count.Should().Be(21);

            var soldToAcc = result.SelectSingleNode(XpathSoldToAcc);
            var licenseEAcc = result.SelectSingleNode(XpathLicenseEAcc);
            var startDate = result.SelectSingleNode(XpathStartDate);
            var endDate = result.SelectSingleNode(XpathEndDate);
            var vName = result.SelectSingleNode(XpathVName);
            var imo = result.SelectSingleNode(XpathImo);
            var callSign = result.SelectSingleNode(XpathCallSign);
            var shoreBased = result.SelectSingleNode(XpathshoreBased);
            var fleet = result.SelectSingleNode(XpathFleet);
            var endUserId = result.SelectSingleNode(XpathEndUserId);
            var manuf = result.SelectSingleNode(XpathManuf);
            var lType = result.SelectSingleNode(XpathLType);
            var users = result.SelectSingleNode(XpathUsers);
            var licDur = result.SelectSingleNode(XpathLicDur);

            soldToAcc.InnerXml.Should().BeEmpty();
            licenseEAcc.InnerXml.Should().BeEmpty();
            startDate.InnerXml.Should().BeEmpty();
            endDate.InnerXml.Should().BeEmpty();
            vName.InnerXml.Should().BeEmpty();
            imo.InnerXml.Should().BeEmpty();
            callSign.InnerXml.Should().BeEmpty();
            shoreBased.InnerXml.Should().BeEmpty();
            fleet.InnerXml.Should().BeEmpty();
            endUserId.InnerXml.Should().BeEmpty();
            manuf.InnerXml.Should().BeEmpty();
            lType.InnerXml.Should().BeEmpty();
            users.InnerXml.Should().BeNullOrEmpty();
            licDur.InnerXml.Should().BeNullOrEmpty();

            var prodItem = result.SelectSingleNode(XpathProd);
            prodItem.ChildNodes.Count.Should().Be(4);
            prodItem.ChildNodes[0].ChildNodes.Count.Should().Be(5);

            var repeat = result.SelectSingleNode(XpathRepeat);
            repeat.InnerXml.Should().BeEmpty();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
            && call.GetArgument<LogLevel>(0) == LogLevel.Information
            && call.GetArgument<EventId>(1) == EventIds.CreatingRecordOfSaleSapPayload.ToEventId()
            && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Creating the record of sale SAP Payload. | _X-Correlation-ID : {_X-Correlation-ID}").MustHaveHappenedOnceExactly();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
            && call.GetArgument<LogLevel>(0) == LogLevel.Information
            && call.GetArgument<EventId>(1) == EventIds.CreatedRecordOfSaleSapPayload.ToEventId()
            && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "The record of sale SAP payload created. | _X-Correlation-ID : {_X-Correlation-ID}").MustHaveHappenedOnceExactly();
        }

        [Test]
        public void WhenTransactionTypeIsMigrateExistingLicence_ThenReturns_SapXmlPayloadWithSomeEmptyFields()
        {
            var migrateExistingLicencePayloadJson = JsonConvert.DeserializeObject<RecordOfSaleEventPayLoad>(migrateExistingLicencePayload);
            var migrateExistingLicencePayloadJsonForMerging = JsonConvert.DeserializeObject<RecordOfSaleEventPayLoad>(migrateExistingLicencePayloadForMerging);

            List<RecordOfSaleEventPayLoad> rosMigrateExistingLicenceData = new()
            {
                migrateExistingLicencePayloadJson!, migrateExistingLicencePayloadJsonForMerging!
            };

            MethodInfo methodInfo = typeof(RecordOfSaleSapMessageBuilder).GetMethod("BuildMigrateExistingLicencePayload", BindingFlags.NonPublic | BindingFlags.Instance)!;
            var result = (SapRecordOfSalePayLoad)methodInfo.Invoke(_fakeRecordOfSaleSapMessageBuilder, new object[] { rosMigrateExistingLicenceData })!;

            result.Should().NotBeNull();
            result.CorrelationId.Should().Be("123-abc-456-xyz-333");
            result.SoldToAcc.Should().Be("");
            result.LicenseEacc.Should().Be("");
            result.StartDate.Should().Be("");
            result.EndDate.Should().Be("");
            result.VesselName.Should().Be("");
            result.IMONumber.Should().Be("");
            result.CallSign.Should().Be("");
            result.ShoreBased.Should().Be("");
            result.FleetName.Should().Be("");
            result.Users.Should().Be(null);
            result.EndUserId.Should().Be("");
            result.LicenceDuration.Should().Be(null);
            result.ECDISMANUF.Should().Be("");
            result.LicenceType.Should().Be("");
            result.PROD.UnitOfSales.Count.Should().Be(4);
            result.PROD.UnitOfSales[0].Repeat.Should().Be("");
            result.PROD.UnitOfSales[1].Repeat.Should().Be("");
            result.PROD.UnitOfSales[2].Repeat.Should().Be("");
            result.PROD.UnitOfSales[3].Repeat.Should().Be("");
        }

        [Test]
        public void WhenTransactionTypeIsConvertLicence_ThenReturnXMLDocument()
        {
            var convertLicencePayloadJson = JsonConvert.DeserializeObject<RecordOfSaleEventPayLoad>(convertLicencePayload);
            var convertLicencePayloadJsonForMerging = JsonConvert.DeserializeObject<RecordOfSaleEventPayLoad>(convertLicencePayloadForMerging);

            List<RecordOfSaleEventPayLoad> rosConvertLicenceData = new()
            {
                convertLicencePayloadJson!, convertLicencePayloadJsonForMerging!
            };

            const string correlationId = "123-abc-456-xyz-333";
           
            A.CallTo(() => _fakeFileSystemHelper.IsFileExists(A<string>.Ignored)).Returns(true);
            
            var result = _fakeRecordOfSaleSapMessageBuilder.BuildRecordOfSaleSapMessageXml(rosConvertLicenceData, correlationId);

            result.Should().BeOfType<XmlDocument>();

            var actionItem = result.SelectSingleNode(XpathZAddsRos);
            actionItem.ChildNodes.Count.Should().Be(1);
            actionItem.ChildNodes[0].ChildNodes.Count.Should().Be(21);

            var soldToAcc = result.SelectSingleNode(XpathSoldToAcc);
            var licenseEAcc = result.SelectSingleNode(XpathLicenseEAcc);
            var startDate = result.SelectSingleNode(XpathStartDate);
            var endDate = result.SelectSingleNode(XpathEndDate);
            var vName = result.SelectSingleNode(XpathVName);
            var imo = result.SelectSingleNode(XpathImo);
            var callSign = result.SelectSingleNode(XpathCallSign);
            var shoreBased = result.SelectSingleNode(XpathshoreBased);
            var fleet = result.SelectSingleNode(XpathFleet);
            var endUserId = result.SelectSingleNode(XpathEndUserId);
            var manuf = result.SelectSingleNode(XpathManuf);
            var lType = result.SelectSingleNode(XpathLType);
            var users = result.SelectSingleNode(XpathUsers);
            var licDur = result.SelectSingleNode(XpathLicDur);

            soldToAcc.InnerXml.Should().BeEmpty();
            licenseEAcc.InnerXml.Should().BeEmpty();
            startDate.InnerXml.Should().BeEmpty();
            endDate.InnerXml.Should().BeEmpty();
            vName.InnerXml.Should().BeEmpty();
            imo.InnerXml.Should().BeEmpty();
            callSign.InnerXml.Should().BeEmpty();
            shoreBased.InnerXml.Should().BeEmpty();
            fleet.InnerXml.Should().BeEmpty();
            endUserId.InnerXml.Should().BeEmpty();
            manuf.InnerXml.Should().BeEmpty();
            lType.InnerXml.Should().Be("1");
            users.InnerXml.Should().BeNullOrEmpty();
            licDur.InnerXml.Should().Be("12");

            var prodItem = result.SelectSingleNode(XpathProd);
            prodItem.ChildNodes.Count.Should().Be(4);
            prodItem.ChildNodes[0].ChildNodes.Count.Should().Be(5);

            var repeat = result.SelectSingleNode(XpathRepeat);
            repeat.InnerXml.Should().BeEmpty();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
                && call.GetArgument<LogLevel>(0) == LogLevel.Information
                && call.GetArgument<EventId>(1) == EventIds.CreatingRecordOfSaleSapPayload.ToEventId()
                && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Creating the record of sale SAP Payload. | _X-Correlation-ID : {_X-Correlation-ID}").MustHaveHappenedOnceExactly();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
                && call.GetArgument<LogLevel>(0) == LogLevel.Information
                && call.GetArgument<EventId>(1) == EventIds.CreatedRecordOfSaleSapPayload.ToEventId()
                && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "The record of sale SAP payload created. | _X-Correlation-ID : {_X-Correlation-ID}").MustHaveHappenedOnceExactly();
        }

        [Test]
        public void WhenTransactionTypeIsConvertLicence_ThenReturns_SapXmlPayloadWithSomeEmptyFields()
        {
            var convertLicencePayloadJson = JsonConvert.DeserializeObject<RecordOfSaleEventPayLoad>(convertLicencePayload);
            var convertLicencePayloadJsonForMerging = JsonConvert.DeserializeObject<RecordOfSaleEventPayLoad>(convertLicencePayloadForMerging);

            List<RecordOfSaleEventPayLoad> rosConvertLicenceData = new()
            {
                convertLicencePayloadJson!, convertLicencePayloadJsonForMerging!
            };

            MethodInfo methodInfo = typeof(RecordOfSaleSapMessageBuilder).GetMethod("BuildConvertLicencePayload", BindingFlags.NonPublic | BindingFlags.Instance)!;
            var result = (SapRecordOfSalePayLoad)methodInfo.Invoke(_fakeRecordOfSaleSapMessageBuilder, new object[] { rosConvertLicenceData })!;

            result.Should().NotBeNull();
            result.CorrelationId.Should().Be("123-abc-456-xyz-333");
            result.SoldToAcc.Should().Be("");
            result.LicenseEacc.Should().Be("");
            result.StartDate.Should().Be("");
            result.EndDate.Should().Be("");
            result.VesselName.Should().Be("");
            result.IMONumber.Should().Be("");
            result.CallSign.Should().Be("");
            result.ShoreBased.Should().Be("");
            result.FleetName.Should().Be("");
            result.Users.Should().Be(null);
            result.EndUserId.Should().Be("");
            result.LicenceDuration.Should().Be(12);
            result.ECDISMANUF.Should().Be("");
            result.LicenceType.Should().Be("1");
            result.PROD.UnitOfSales.Count.Should().Be(4);
            result.PROD.UnitOfSales[0].Repeat.Should().Be("");
            result.PROD.UnitOfSales[1].Repeat.Should().Be("");
            result.PROD.UnitOfSales[2].Repeat.Should().Be("");
            result.PROD.UnitOfSales[3].Repeat.Should().Be("");
        }
    }
}
