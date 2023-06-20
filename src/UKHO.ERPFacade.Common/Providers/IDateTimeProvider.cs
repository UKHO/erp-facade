namespace UKHO.ERPFacade.Common.Providers
{
    public interface IDateTimeProvider
    {
        DateTime UtcNow { get; }
        DateTime MinValue { get; }
    }
}