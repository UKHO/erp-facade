namespace UKHO.ERPFacade.Common.Providers
{
    public class DateTimeProvider : IDateTimeProvider
    {
        public DateTime UtcNow => DateTime.UtcNow;
        public DateTime MinValue => DateTime.MinValue;
    }
}
