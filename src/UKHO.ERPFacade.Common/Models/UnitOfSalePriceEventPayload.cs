using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;
using UKHO.ERPFacade.Common.Infrastructure;

namespace UKHO.ERPFacade.Common.Models
{
    [ExcludeFromCodeCoverage]
    public class UnitOfSalePriceEventPayload : EventBase<UnitOfSalePriceEvent>
    {
        public UnitOfSalePriceEventPayload(UnitOfSalePriceEvent unitOfSalePriceEvent)
        {
            Data = unitOfSalePriceEvent;
        }

    }

    [ExcludeFromCodeCoverage]
    public class UnitOfSalePriceEvent
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

        [JsonProperty("datacontenttype")]
        public string DataContentType { get; set; }

        [JsonProperty("data")]
        public UnitOfSalePriceEventData Data { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class UnitOfSalePriceEventData
    {
        [JsonProperty("correlationId")]
        public string CorrelationId { get; set; }

        [JsonProperty("unitsOfSalePrices")]
        public List<UnitsOfSalePrices> UnitsOfSalePrices { get; set; }
    }
}
