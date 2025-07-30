using System.Diagnostics.CodeAnalysis;

namespace UKHO.ERPFacade.Common.Constants
{
    [ExcludeFromCodeCoverage]
    public static class ErrorDetails
    {
        public const string Source = "erpfacade";
        public const string CorrelationIdNotFoundMessage = "Correlation ID Not Found.";
        public const string UnknownEventTypeMessage = "Unknown event type received in payload.";
    }
}
