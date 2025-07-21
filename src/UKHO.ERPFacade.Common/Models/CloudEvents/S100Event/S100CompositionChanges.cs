using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace UKHO.ERPFacade.Common.Models.CloudEvents.S100Event
{
    [ExcludeFromCodeCoverage]
    public class S100CompositionChanges
    {
        [JsonProperty("addProducts")]
        public List<string> AddProducts { get; set; }
        [JsonProperty("removeProducts")]
        public List<string> RemoveProducts { get; set; }
    }
}
