using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;
using UKHO.ERPFacade.Common.Infrastructure;

namespace UKHO.ERPFacade.Common.Models
{
    [ExcludeFromCodeCoverage]
    public class UnitOfSaleUpdatedEventPayload : EventBase<UnitOfSaleUpdatedEvent>
    {
        public UnitOfSaleUpdatedEventPayload(UnitOfSaleUpdatedEvent unitOfSalePriceEvent)
        {
            EventData = unitOfSalePriceEvent;
        }

        public override string EventName => "uk.gov.ukho.erpFacade.unitOfSaleUpdated.v1";
        public override string Subject => EventData.Subject;
    }

    [ExcludeFromCodeCoverage]
    public class UnitOfSaleUpdatedEvent
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

        [JsonProperty("_COMMENT")]
        public string _COMMENT { get; set; }

        [JsonProperty("subject")]
        public string Subject { get; set; }

        [JsonProperty("datacontenttype")]
        public string DataContentType { get; set; }

        [JsonProperty("data")]
        public UnitOfSaleUpdatedEventData Data { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class UnitOfSaleUpdatedEventData
    {
        [JsonProperty("correlationId")]
        public string CorrelationId { get; set; }

        [JsonProperty("unitsOfSalePrices")]
        public List<UnitsOfSalePrices> UnitsOfSalePrices { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class UnitsOfSalePrices
    {
        [JsonProperty("unitName")]
        public string UnitName { get; set; }

        [JsonProperty("price")]
        public List<Price> Price { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class Price
    {
        [JsonProperty("effectiveDate")]
        public DateTimeOffset EffectiveDate { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("standard")]
        public Standard Standard { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class Standard
    {
        [JsonProperty("priceDurations")]
        public List<PriceDurations> PriceDurations { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class PriceDurations
    {
        [JsonProperty("numberOfMonths")]
        public int NumberOfMonths { get; set; }

        [JsonProperty("rrp")]
        public string Rrp { get; set; }
    }
}
