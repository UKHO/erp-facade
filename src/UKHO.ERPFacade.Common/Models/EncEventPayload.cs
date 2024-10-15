using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace UKHO.ERPFacade.Common.Models
{
    [ExcludeFromCodeCoverage]
    public class EncEventPayload
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
        public EesEventData Data { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class EesEventData
    {
        [JsonProperty("correlationId")]
        public string CorrelationId { get; set; }

        [JsonProperty("ukhoWeekNumber")]
        public UkhoWeekNumber UkhoWeekNumber { get; set; }

        [JsonProperty("products")]
        public List<Product> Products { get; set; }

        [JsonProperty("unitsOfSale")]
        public List<UnitOfSale> UnitsOfSales { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class UkhoWeekNumber
    {
        [JsonProperty("year")]
        public int? Year { get; set; }

        [JsonProperty("week")]
        public int? Week { get; set; }

        [JsonProperty("currentWeekAlphaCorrection")]
        public bool? CurrentWeekAlphaCorrection { get; set; }
    }
}
