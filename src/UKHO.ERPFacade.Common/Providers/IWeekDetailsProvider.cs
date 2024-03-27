namespace UKHO.ERPFacade.Common.Providers
{
    public interface IWeekDetailsProvider
    { 
        public string GetDateOfWeek(int year, int week, bool currentWeekAlphaCorrection);
    }
}
