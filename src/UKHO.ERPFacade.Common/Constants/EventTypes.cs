using System.Diagnostics.CodeAnalysis;

namespace UKHO.ERPFacade.Common.Constants
{
    [ExcludeFromCodeCoverage]
    public static class EventTypes
    {
        public const string S57EventType = "uk.gov.ukho.encpublishing.enccontentpublished.v2.2";
        public const string S100EventType = "uk.gov.ukho.encpublishing.s100datacontentpublished.v1";
        public const string S100UnitOfSaleUpdatedEventType = "uk.gov.ukho.erp.s100UnitOfSaleUpdated.v1";
    }
}
