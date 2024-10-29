using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace UKHO.ERPFacade.Common.Models.CloudEvents.S57Event
{

    [ExcludeFromCodeCoverage]
    public class S57PriceDurations
    {
        [JsonProperty("numberOfMonths")]
        public int NumberOfMonths { get; set; }

        [JsonProperty("rrp")]
        public decimal Rrp { get; set; }
    }
}
