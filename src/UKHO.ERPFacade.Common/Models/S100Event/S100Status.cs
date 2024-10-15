using Newtonsoft.Json;

namespace UKHO.ERPFacade.Common.Models.S100Event
{
    public class S100Status
    {
        [JsonProperty("statusName")]
        public string StatusName { get; set; }
        [JsonProperty("statusDate")]
        public DateTime StatusDate { get; set; }
        [JsonProperty("isNewCell")]
        public bool IsNewCell { get; set; }
    }
}
