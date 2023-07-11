using Newtonsoft.Json;

namespace UKHO.ERPFacade.API.FunctionalTests.Model
{
    public class JsonInputPriceChangeHelper
    {
        [JsonProperty("corrid")]
        public string? Corrid { get; set; }

        [JsonProperty("org")]
        public string Org { get; set; }

        [JsonProperty("productname")]
        public string Productname { get; set; }

        [JsonProperty("duration")]
        public long Duration { get; set; }

        [JsonProperty("effectivedate")]
        public long Effectivedate { get; set; }

        [JsonProperty("effectivetime")]
        public long Effectivetime { get; set; }

        [JsonProperty("price")]
        public string Price { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("futuredate")]
        public long? Futuredate { get; set; }

        [JsonProperty("futuretime")]
        public string? Futuretime { get; set; }

        [JsonProperty("futureprice")]
        public string Futureprice { get; set; }

        [JsonProperty("futurecurr")]
        public string? Futurecurr { get; set; }

        [JsonProperty("reqdate")]
        public long Reqdate { get; set; }

        [JsonProperty("reqtime")]
        public long Reqtime { get; set; }
    }
}
