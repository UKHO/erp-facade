using Newtonsoft.Json;

namespace UKHO.ERPFacade.Common.Models
{
    public partial class EESEvent
    {
        [JsonProperty("data")]
        public EESEventData Data { get; set; }
    }

    [Serializable]
    public partial class EESEventData
    {
        [JsonProperty("traceId")]
        public string TraceId { get; set; }
    }
}
