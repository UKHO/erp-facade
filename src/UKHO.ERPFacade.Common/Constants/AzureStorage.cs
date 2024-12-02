using System.Diagnostics.CodeAnalysis;

namespace UKHO.ERPFacade.Common.Constants
{
    [ExcludeFromCodeCoverage]
    public static class AzureStorage
    {
        public const string EventTableName = "events";
        public const string RecordOfSaleQueueName = "recordofsaleevents";
        public const string RecordOfSaleEventContainerName = "recordofsaleblobs";
        public const string LicenceUpdatedEventContainerName = "licenceupdatedblobs";
        public const string EventStatus = "Status";
        public const string EventRequestDateTime = "RequestDateTime";
        public const string EventResponseDateTime = "ResponseDateTime";
        public const string EventPublishedDateTime = "EventPublishedDateTime";
    }
}
