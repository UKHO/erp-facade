using Newtonsoft.Json;

namespace UKHO.ERPFacade.Common.Models.S100Event
{
    public class S100UnitsOfSale
    {
        [JsonProperty("unitName")]
        public string UnitName { get; set; }
        [JsonProperty("title")]
        public string Title { get; set; }
        [JsonProperty("unitSize")]
        public string UnitSize { get; set; }
        [JsonProperty("status")]
        public string Status { get; set; }
        [JsonProperty("isNewUnitOfSale")]
        public bool IsNewUnitOfSale { get; set; }
        [JsonProperty("boundingBox")]
        public S100BoundingBox BoundingBox { get; set; }
        [JsonProperty("compositionChanges")]
        public S100CompositionChanges CompositionChanges { get; set; }
    }
}
