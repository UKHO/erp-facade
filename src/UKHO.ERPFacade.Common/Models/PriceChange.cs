using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace UKHO.ERPFacade.Common.Models
{
    [ExcludeFromCodeCoverage]
    public class PriceChange
    {
        [JsonProperty("corrid")]
        public string Corrid { get; set; }

        [JsonProperty("org")]
        public string Org { get; set; }

        [JsonProperty("productname")]
        public string ProductName { get; set; }

        [JsonProperty("duration")]
        public string Duration { get; set; }

        [JsonProperty("effectivedate")]
        public string EffectiveDate { get; set; }

        [JsonProperty("effectivetime")]
        public string EffectiveTime { get; set; }

        [JsonProperty("price")]
        public string Price { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("futuredate")]
        public string FutureDate { get; set; }

        [JsonProperty("futuretime")]
        public string FutureTime { get; set; }

        [JsonProperty("futureprice")]
        public string FuturePrice { get; set; }

        [JsonProperty("futurecurr")]
        public string FutureCurr { get; set; }

        [JsonProperty("reqdate")]
        public string ReqDate { get; set; }

        [JsonProperty("reqtime")]
        public string ReqTime { get; set; }
    }
}
