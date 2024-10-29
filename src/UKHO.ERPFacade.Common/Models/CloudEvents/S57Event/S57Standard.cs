using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace UKHO.ERPFacade.Common.Models.CloudEvents.S57Event
{
    [ExcludeFromCodeCoverage]
    public class S57Standard
    {
        [JsonProperty("priceDurations")]
        public List<S57PriceDurations> PriceDurations { get; set; }
    }
}
