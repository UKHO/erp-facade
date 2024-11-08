using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace UKHO.ERPFacade.Common.Models.CloudEvents.S57Event
{
    [ExcludeFromCodeCoverage]
    public class S57UkhoWeekNumber
    {
        [JsonProperty("year")]
        public int? Year { get; set; }

        [JsonProperty("week")]
        public int? Week { get; set; }

        [JsonProperty("currentWeekAlphaCorrection")]
        public bool? CurrentWeekAlphaCorrection { get; set; }
    }
}
