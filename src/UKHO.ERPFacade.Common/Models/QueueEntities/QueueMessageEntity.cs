using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace UKHO.ERPFacade.Common.Models.QueueEntities
{
    [ExcludeFromCodeCoverage]
    public class QueueMessageEntity
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("eventId")]
        public string EventId { get; set; }

        [JsonProperty("correlationId")]
        public string CorrelationId { get; set; }

        [JsonProperty("relatedEvents")]
        public List<string> RelatedEvents { get; set; }
    }
}
