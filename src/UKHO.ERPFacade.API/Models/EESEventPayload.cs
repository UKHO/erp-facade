using Newtonsoft.Json;
using System.Diagnostics.CodeAnalysis;

namespace UKHO.ERPFacade.API.Models
{
    public class EESEventPayload
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
        public EssEventData Data { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class EssEventData
    {
        [JsonProperty("traceId")]
        public string TraceId { get; set; }

        [JsonProperty("products")]
        public List<Product> Products { get; set; }

        [JsonProperty("unitsOfSale")]
        public List<UnitOfSale> UnitsOfSales { get; set; }
    }
}
