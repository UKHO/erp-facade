using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace UKHO.ERPFacade.Common.Models.CloudEvents.S57Event
{
    [ExcludeFromCodeCoverage]
    public class S57UnitOfSale
    {
        [JsonProperty("unitName")]
        public string UnitName { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("unitOfSaleType")]
        public string UnitOfSaleType { get; set; }

        [JsonProperty("unitSize")]
        public string UnitSize { get; set; }

        [JsonProperty("unitType")]
        public string UnitType { get; set; }

        [JsonProperty("providerCode")]
        public string ProviderCode { get; set; }

        [JsonProperty("providerName")]
        public string ProviderName { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("isNewUnitOfSale")]
        public bool IsNewUnitOfSale { get; set; }

        [JsonProperty("geographicLimit")]
        public S57GeographicLimit GeographicLimit { get; set; }

        [JsonProperty("compositionChanges")]
        public S57CompositionChanges CompositionChanges { get; set; }
    }
}
