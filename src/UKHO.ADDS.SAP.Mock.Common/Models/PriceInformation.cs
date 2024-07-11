using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace UKHO.ADDS.SAP.Mock.Common.Models
{
    [ExcludeFromCodeCoverage]
    public class PriceInformation
    {
        [JsonPropertyName("corrid")]
        public string? Corrid { get; set; }

        [JsonPropertyName("org")]
        public string? Org { get; set; }

        [JsonPropertyName("productname")]
        public string? ProductName { get; set; }

        [JsonPropertyName("duration")]
        public string? Duration { get; set; }

        [JsonPropertyName("effectivedate")]
        public string? EffectiveDate { get; set; }

        [JsonPropertyName("effectivetime")]
        public string? EffectiveTime { get; set; }

        [JsonPropertyName("price")]
        public string? Price { get; set; }

        [JsonPropertyName("currency")]
        public string? Currency { get; set; }

        [JsonPropertyName("futuredate")]
        public string? FutureDate { get; set; }

        [JsonPropertyName("futuretime")]
        public string? FutureTime { get; set; }

        [JsonPropertyName("futureprice")]
        public string? FuturePrice { get; set; }

        [JsonPropertyName("futurecurr")]
        public string? FutureCurr { get; set; }

        [JsonPropertyName("reqdate")]
        public string? ReqDate { get; set; }

        [JsonPropertyName("reqtime")]
        public string? ReqTime { get; set; }
    }
}
