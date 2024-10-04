using Newtonsoft.Json;

namespace UKHO.ERPFacade.Common.Models.S100Event
{
    public class S100CompositionChanges
    {
        [JsonProperty("addProducts")]
        public List<string> AddProducts { get; set; }
        [JsonProperty("removeProducts")]
        public List<string> RemoveProducts { get; set; }
    }
}
