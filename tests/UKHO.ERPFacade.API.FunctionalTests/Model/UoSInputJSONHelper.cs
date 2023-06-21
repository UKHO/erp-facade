using Newtonsoft.Json;


namespace UKHO.ERPFacade.API.FunctionalTests.Model
{
    public class UoSInputJSONHelper
    {
        [JsonProperty("corrid")]
        public string Corrid { get; set; }

        [JsonProperty("org")]
        public string Org { get; set; }

        [JsonProperty("productname")]
        public string Productname { get; set; }

        [JsonProperty("duration")]
        //[JsonConverter(typeof(ParseStringConverter))]
        public long Duration { get; set; }

        [JsonProperty("effectivedate")]
        //[JsonConverter(typeof(ParseStringConverter))]
        public long Effectivedate { get; set; }

        [JsonProperty("effectivetime")]
        //[JsonConverter(typeof(ParseStringConverter))]
        public long Effectivetime { get; set; }

        [JsonProperty("price")]
        public string Price { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("futuredate")]
        //[JsonConverter(typeof(ParseStringConverter))]
        public long? Futuredate { get; set; }

        [JsonProperty("futuretime")]
        public string? Futuretime { get; set; }

        [JsonProperty("futureprice")]
        public string Futureprice { get; set; }

        [JsonProperty("futurecurr")]
        public string? Futurecurr { get; set; }

        [JsonProperty("reqdate")]
        //[JsonConverter(typeof(ParseStringConverter))]
        public long Reqdate { get; set; }

        [JsonProperty("reqtime")]
        //[JsonConverter(typeof(ParseStringConverter))]
        public long Reqtime { get; set; }
    }

}
