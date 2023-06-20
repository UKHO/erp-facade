using Newtonsoft.Json;
using System.Diagnostics.CodeAnalysis;
using UKHO.ERPFacade.Common.Infrastructure;

namespace UKHO.ERPFacade.Common.Models
{
    [ExcludeFromCodeCoverage]
    public class BulkPriceEventPayload : EventBase<BulkPriceEvent>
    {
        public BulkPriceEventPayload(BulkPriceEvent bulkPriceEvent)
        {
            EventData = bulkPriceEvent;
        }

        public override string EventName => "uk.gov.ukho.encpublishing.enccontentpublished.v2";
        public override string Subject => "";
    }

    [ExcludeFromCodeCoverage]
    public class BulkPriceEvent
    {
        [JsonProperty("specversion")]
        public string SpecVersion { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("source")]
        public string Source { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("time")]
        public string Time { get; set; }

        [JsonProperty("subject")]
        public string Subject { get; set; }

        [JsonProperty("data")]
        public BulkPriceEventData Data { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class BulkPriceEventData
    {
        [JsonProperty("correlationId")]
        public string CorrelationId { get; set; }

        [JsonProperty("unitsOfSalePrices")]
        public UnitsOfSalePrices UnitsOfSalePrices { get; set; }
    }
}
