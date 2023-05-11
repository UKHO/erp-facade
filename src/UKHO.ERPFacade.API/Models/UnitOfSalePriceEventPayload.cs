using Newtonsoft.Json;
using System.Diagnostics.CodeAnalysis;

namespace UKHO.ERPFacade.API.Models
{
    [ExcludeFromCodeCoverage]
    public class UnitOfSalePriceEventPayload
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
        public UnitOfSalePriceEventData Data { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class UnitOfSalePriceEventData
    {
        [JsonProperty("traceId")]
        public string TraceId { get; set; }

        [JsonProperty("products")]
        public List<Product> Products { get; set; }

        [JsonProperty("_COMMENT")]
        public string _COMMENT { get; set; }

        [JsonProperty("unitsOfSale")]
        public List<UnitOfSale> UnitsOfSales { get; set; }

        [JsonProperty("unitsOfSalePrices")]
        public List<UnitsOfSalePrices> UnitsOfSalePrices { get; set; }
    }
}
