using System.Diagnostics.CodeAnalysis;

namespace UKHO.ERPFacade.Common.Constants
{
    [ExcludeFromCodeCoverage]
    public static class EventPayloadFiles
    {
        public const string S57EncEventFileName = "EncPublishingEvent.json";
        public const string LicenceUpdatedEventFileName = "LicenceUpdatedEvent.json";
        public const string SapXmlPayloadFileName = "SapXmlPayload.xml";
        public const string S100DataEventFileName = "S100DataPublishingEvent.json";
        public const string RecordOfSaleEventFileExtension = ".json";
        public const string S100UnitOfSaleUpdatedEventFileName = "S100UnitOfSaleUpdatedEvent.json";

        /// <summary>
        /// Constants for functional test project files and folder names
        /// </summary>
        ///
        public const string PayloadFolder = "ERPFacadePayloadTestData";
        public const string LicenceUpdatedPayloadTestData = "LicenceUpdatedPayloadTestData";
        public const string RosPayloadTestDataFolder = "RoSPayloadTestData";
        public const string S57PayloadFolder = "S57PayloadTestData";
        public const string S100PayloadFolder = "S100PayloadTestData";

        public const string WebhookPayloadFileName = "WebhookPayload.JSON";

        public const string GeneratedXmlFolder = "ERPFacadeGeneratedXmlFiles";
        public const string GeneratedJsonFolder = "ERPFacadeGeneratedJsonFiles";

        public const string ErpFacadeExpectedXmlFolder = "ERPFacadeExpectedXmlFiles";
        public const string S100ExpectedXmlFiles = "S100ExpectedXmlFiles";
        public const string S57ExpectedXmlFolder = "S57ExpectedXmlFiles";
    }
}
