using System.Diagnostics.CodeAnalysis;

namespace UKHO.ERPFacade.Common.Constants
{
    [ExcludeFromCodeCoverage]
    public static class DateTimeFormats
    {
        public const string EventJsonDateTimeFormat = "yyyy-MM-ddTHH:mm:ss.fffffffZ";
        public const string RecDateFormat = "yyyyMMdd";
        public const string RecTimeFormat = "hhmmss";
    }
}
