using System.Diagnostics.CodeAnalysis;

namespace UKHO.ERPFacade.Common.Constants
{
    [ExcludeFromCodeCoverage]
    public static class EventPayloadFiles
    {
        public const string S57EncEventFileName = "EncPublishingEvent.json";
        public const string LicenceUpdatedEventFileName = "LicenceUpdatedEvent.json";
        public const string SapXmlPayloadFileName = "SapXmlPayload.xml";

        public const string RecordOfSaleEventFileExtension = ".json";

        public const string ErpFacadeExpectedXmlFiles = "ERPFacadeExpectedXmlFiles";
        public const string RosPayloadTestDataFolder = "RoSPayloadTestData";
        public const string LicenceUpdatedPayloadTestData = "LicenceUpdatedPayloadTestData";
        public const string PayloadFolder = "ERPFacadePayloadTestData";
        public const string S57PayloadFolder = "S57PayloadTestData";
        public const string WebhookPayloadFileName = "WebhookPayload.JSON";

        public const string GeneratedXmlFolder = "ERPFacadeGeneratedXmlFiles";
    }
}
