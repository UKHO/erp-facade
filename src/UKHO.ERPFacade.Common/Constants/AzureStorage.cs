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
    }
}
