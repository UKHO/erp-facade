using Newtonsoft.Json;

namespace UKHO.ERPFacade.API.StorageAccount
{
    [Serializable]
    public partial class ESSEventData
    {
        [JsonProperty("specversion")]
        public string specversion { get; set; }

        [JsonProperty("type")]
        public string type { get; set; }

        [JsonProperty("source")]
        public string source { get; set; }

        [JsonProperty("id")]
        public string id { get; set; }

        [JsonProperty("time")]
        public string time { get; set; }

        [JsonProperty("_COMMENT")]
        public string _COMMENT { get; set; }

        [JsonProperty("subject")]
        public string subject { get; set; }

        [JsonProperty("datacontenttype")]
        public string datacontenttype { get; set; }

        [JsonProperty("data")]
        public DataObject data { get; set; }

    }

    [Serializable]
    public partial class DataObject
    {
        [JsonProperty("traceId")]
        public string traceId { get; set; }
    }
}
