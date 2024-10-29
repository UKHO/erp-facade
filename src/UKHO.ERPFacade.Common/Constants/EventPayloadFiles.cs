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

        public const string S100WebhookPayloadFolder = "S100PayloadTestData";
        public const string ErpFacadeExpectedXmlFiles = "ERPFacadeExpectedXmlFiles";
        public const string RosPayloadTestDataFolder = "RoSPayloadTestData";
    }
}
