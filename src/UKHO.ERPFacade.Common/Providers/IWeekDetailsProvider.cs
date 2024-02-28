namespace UKHO.ERPFacade.Common.Providers
{
    public interface IWeekDetailsProvider
    { 
        public string GetThursdayDateOfWeek(int year, int week);
    }
}
