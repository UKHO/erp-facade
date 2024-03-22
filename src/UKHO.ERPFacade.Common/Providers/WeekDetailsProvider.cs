using UKHO.WeekNumberUtils;

namespace UKHO.ERPFacade.Common.Providers
{
    public class WeekDetailsProvider : IWeekDetailsProvider
    {
        public string GetDateOfWeek(int year, int week, bool currentWeekAlphaCorrection)
        {
            var weekDetails = new WeekNumber(year, week);
            var thursdayDate = weekDetails.Date;

            if (currentWeekAlphaCorrection)
            {
                var fridayDate = thursdayDate.AddDays(1);
                return fridayDate.ToString("yyyyMMdd");
            }

            return thursdayDate.ToString("yyyyMMdd");
        }
    }
}
