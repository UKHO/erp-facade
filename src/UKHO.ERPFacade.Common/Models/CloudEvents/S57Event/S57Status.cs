using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace UKHO.ERPFacade.Common.Models.CloudEvents.S57Event
{
    [ExcludeFromCodeCoverage]
    public class S57Status
    {
        [JsonProperty("statusName")]
        public string StatusName { get; set; }

        [JsonProperty("statusDate")]
        public DateTime StatusDate { get; set; }

        [JsonProperty("isNewCell")]
        public bool IsNewCell { get; set; }
    }
}
