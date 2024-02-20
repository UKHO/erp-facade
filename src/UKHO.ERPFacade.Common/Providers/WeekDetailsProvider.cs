using System.Diagnostics.CodeAnalysis;
using UKHO.WeekNumberUtils;

namespace UKHO.ERPFacade.Common.Providers
{
    [ExcludeFromCodeCoverage]
    public class WeekDetailsProvider : IWeekDetailsProvider
    {
        public string GetThursdayDateOfWeek(int year, int week)
        {
            var weekDetails = new WeekNumber(year, week);

            var thursdayDate = weekDetails.Date;

            return thursdayDate.ToString("yyyyMMdd");
        }
    }
}
