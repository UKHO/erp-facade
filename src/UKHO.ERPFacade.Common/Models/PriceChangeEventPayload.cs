using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;
using UKHO.ERPFacade.Common.Infrastructure;

namespace UKHO.ERPFacade.Common.Models
{
    [ExcludeFromCodeCoverage]
    public class PriceChangeEventPayload : EventBase<PriceChangeEventData>
    {
        public PriceChangeEventPayload(PriceChangeEventData priceChangeEventData, string subject, string eventId)
        {
            Data = priceChangeEventData;
            Subject = subject;
            Id = eventId;
            EventName = "uk.gov.ukho.erp.priceChange.v1";
        }
    }


    [ExcludeFromCodeCoverage]
    public class PriceChangeEventData
    {
        [JsonProperty("correlationId")]
        public string CorrelationId { get; set; }

        [JsonProperty("unitsOfSalePrices")]
        public List<UnitsOfSalePrices> UnitsOfSalePrices { get; set; }
    }
}
