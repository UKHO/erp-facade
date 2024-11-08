using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace UKHO.ERPFacade.Common.Models.CloudEvents.S100Event
{
    [ExcludeFromCodeCoverage]
    public class S100Product
    {
        [JsonProperty("productType")]
        public string ProductType { get; set; }
        [JsonProperty("productIdentifier")]
        public string ProductIdentifier { get; set; }
        [JsonProperty("productName")]
        public string ProductName { get; set; }
        [JsonProperty("editionNumber")]
        public int EditionNumber { get; set; }
        [JsonProperty("updateNumber")]
        public int UpdateNumber { get; set; }
        [JsonProperty("mayAffectHoldings")]
        public bool MayAffectHoldings { get; set; }
        [JsonProperty("contentChange")]
        public bool ContentChange { get; set; }
        [JsonProperty("providerCode")]
        public string ProviderCode { get; set; }
        [JsonProperty("providerName")]
        public string ProviderName { get; set; }
        [JsonProperty("size")]
        public string Size { get; set; }
        [JsonProperty("producingAgency")]
        public string ProducingAgency { get; set; }
        [JsonProperty("status")]
        public S100Status Status { get; set; }
        [JsonProperty("replaces")]
        public List<string> Replaces { get; set; }
        [JsonProperty("dataReplacement")]
        public List<string> DataReplacement { get; set; }
        [JsonProperty("boundingBox")]
        public S100BoundingBox BoundingBox { get; set; }
        [JsonProperty("fileSize")]
        public int FileSize { get; set; }
        [JsonProperty("inUnitsOfSale")]
        public List<string> InUnitsOfSale { get; set; }
    }
}
