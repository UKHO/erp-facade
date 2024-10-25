using System.Diagnostics.CodeAnalysis;

namespace UKHO.ERPFacade.Common.Constants
{
    [ExcludeFromCodeCoverage]
    public static class Constants
    {
        //Event types
        public const string S57EventType = "uk.gov.ukho.encpublishing.enccontentpublished.v2.2";
        public const string S100EventType = "uk.gov.ukho.encpublishing.s100datacontentpublished.v1";

        //XmlTransformer Types
        public const string S57XmlTransformer = "S57XmlTransformer";

        //JSON field keys
        public const string EventIdKey = "id";
        public const string DataNode = "data";
        public const string CorrelationIdKey = "data.correlationId";
        public const string ProductsNode = "data.products";
        public const string UnitsOfSaleNode = "data.unitsOfSale";
        public const string UKHOWeekNumber = "data.ukhoWeekNumber";


        //SAP xml payload file name
        public const string SapXmlPayloadFileName = "SapXmlPayload.xml";

        //S57 event file name
        public const string S57EncEventFileName = "EncPublishingEvent.json";

        //event storage
        public const string EventTableName = "events";

        //S57 event storage
        public const string S57EventTableName = "encevents";
        public const string S57EventContainerName = "s57events";
        public const string ErpFacadeExpectedXmlFiles = "ERPFacadeExpectedXmlFiles";

        //LicenceUpdated event file name 
        public const string LicenceUpdatedEventFileName = "LicenceUpdatedEvent.json";

        //LicenceUpdated event storage
        public const string LicenceUpdatedEventTableName = "licenceupdatedevents";
        public const string LicenceUpdatedEventContainerName = "licenceupdatedblobs";

        //RecordOfSale event storage
        public const string RecordOfSaleQueueName = "recordofsaleevents";
        public const string RecordOfSaleEventTableName = "recordofsaleevents";
        public const string RecordOfSaleEventContainerName = "recordofsaleblobs";
        public const string RecordOfSaleEventFileExtension = ".json";

        //S57 xml template xpath
        public const string S57SapXmlTemplatePath = "SapXmlTemplates\\SAPS57Request.xml";
        public const string XpathImMatInfo = $"//*[local-name()='IM_MATINFO']";
        public const string XpathActionItems = $"//*[local-name()='ACTIONITEMS']";
        public const string XpathNoOfActions = $"//*[local-name()='NOOFACTIONS']";
        public const string XpathCorrId = $"//*[local-name()='CORRID']";
        public const string XpathRecDate = $"//*[local-name()='RECDATE']";
        public const string XpathRecTime = $"//*[local-name()='RECTIME']";
        public const string Item = "item";

        //S57 xml payload basic nodes
        public const string ActionNumber = "ACTIONNUMBER";
        public const string Action = "ACTION";
        public const string Product = "PRODUCT";
        public const string ProdType = "PRODTYPE";
        public const string ReplacedBy = "REPLACEDBY";
        public const string ChildCell = "CHILDCELL";
        public const string RecDateFormat = "yyyyMMdd";
        public const string RecTimeFormat = "hhmmss";

        public const string EncCell = "ENC CELL";
        public const string AvcsUnit = "AVCS UNIT";

        public const string ProductSection = "Product";
        public const string ProdTypeValue = "S57";
        public const string Permit = "permit";

        public const string UnitOfSaleSection = "UnitOfSale";
        public const string UnitSaleType = "unit";
        public const string UnitOfSaleStatusForSale = "ForSale";
        public const string Products = "products";
        public const string UnitsOfSale = "unitsOfSale";

        public const string UkhoWeekNumberSection = "UkhoWeekNumber";
        public const string ValidFrom = "VALIDFROM";
        public const string WeekNo = "WEEKNO";
        public const string Correction = "CORRECTION";
        public const string IsCorrectionTrue = "Y";
        public const string IsCorrectionFalse = "N";
        public const string ActiveKey = "ACTIVEKEY";
        public const string NextKey = "NEXTKEY";
        public const string Agency = "AGENCY";

        public const string CreateEncCell = "CREATE ENC CELL";
        public const string UpdateCell = "UPDATE ENC CELL EDITION UPDATE NUMBER";

        public const int MaxXmlNodeLength = 250;
        public const int MaxAgencyXmlNodeLength = 2;
        public const string UkhoWeekNoFormat = "D2";
        public const string UkhoWeekNoFormatSeparator = "";

        //RecordOfSale xml payload builder
        public const string RecordOfSaleSapXmlTemplatePath = "SapXmlTemplates\\SAPRoSRequest.xml";
        public const string XpathZAddsRos = $"//*[local-name()='Z_ADDS_ROS']";
        public const string ImOrderNameSpace = "RecordOfSale";
        public const string MaintainHoldingsType = "MAINTAINHOLDINGS";
        public const string NewLicenceType = "NEWLICENCE";
        public const string MigrateNewLicenceType = "MIGRATENEWLICENCE";
        public const string MigrateExistingLicenceType = "MIGRATEEXISTINGLICENCE";
        public const string ConvertLicenceType = "CONVERTLICENCE";

        public const string S57RequestEndPoint = "/webhook/newenccontentpublishedeventreceived";
        public const string LicenceUpdatedRequestEndPoint = "/webhook/licenceupdatedpublishedeventreceived";
        public const string RoSWebhookRequestEndPoint = "/webhook/recordofsalepublishedeventreceived";

        public const string SapHealthCheckXmlPath = "SapXmlTemplates\\SAPHealthCheckRequest.xml";
        public const string DefaultContentType = "application/json";
        public const string RosPayloadTestDataFolder = "RoSPayloadTestData";

        public const string SoldToAcc = "SOLDTOACC";
        public const string LicenceAcc = "LICENSEEACC";
        public const string StartDate = "STARTDATE";
        public const string EndDate = "ENDDATE";
        public const string VName = "VNAME";
        public const string Imo = "IMO";
        public const string CallSign = "CALLSIGN";
        public const string ShoreBased = "SHOREBASED";
        public const string Fleet = "FLEET";
        public const string Users = "USERS";
        public const string EndUserId = "ENDUSERID";
        public const string EcdisManUf = "ECDISMANUF";
        public const string LType = "LTYPE";
        public const string LicDur = "LICDUR";
        public const string LicNo = "LICNO";
        public const string Repeat = "REPEAT";
        public const string ProductOrder = "PO";
        public const string AdsOrderNumber = "ADSORDNO";
        public const string Id = "ID";
        public const string ProductEndDate = "ENDDA";
        public const string Duration = "DURATION";
        public const string Renew = "RENEW";
        public const string SoapEnvelope = "soap:Envelope";
        public const string SoapBody = "soap:Body";

        //Functional Tests
        public const string XpathActionNumber = $"//*[local-name()='ACTIONNUMBER']";
        public const string XpathAction = $"//*[local-name()='ACTION']";
        public const string ReplaceEncCellAction = "REPLACED WITH ENC CELL";
        public const string ChangeEncCellAction = "CHANGE ENC CELL";
        public const string PermitWithSameKey = "PermitWithSameKey";
        public const string PermitWithDifferentKey = "PermitWithDifferentKey";
        public const string AioKey = "AIO";

        //LicenseUpdate xml payload nodes
        public const string LicTransaction = "CHANGELICENCE";

        public const string S57PartitionKey = "S57";
        public const string ROSPartitionKey = "ROS";
    }
}
