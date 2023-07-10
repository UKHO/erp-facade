using System.Diagnostics.CodeAnalysis;

namespace UKHO.ERPFacade.Common.Providers
{
    [ExcludeFromCodeCoverage]
    public class DateTimeProvider : IDateTimeProvider
    {
        public DateTime UtcNow => DateTime.UtcNow;
        public DateTime MinValue => DateTime.MinValue;
    }
}
