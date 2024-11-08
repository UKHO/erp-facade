using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace UKHO.ERPFacade.Common.Models.CloudEvents.S57Event
{
    [ExcludeFromCodeCoverage]
    public class S57Product
    {
        [JsonProperty("dataSetName")]
        public string DataSetName { get; set; }

        [JsonProperty("productName")]
        public string ProductName { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("scale")]
        public int Scale { get; set; }

        [JsonProperty("usageBand")]
        public int UsageBand { get; set; }

        [JsonProperty("editionNumber")]
        public int? EditionNumber { get; set; }

        [JsonProperty("updateNumber")]
        public int? UpdateNumber { get; set; }

        [JsonProperty("mayAffectHoldings")]
        public bool MayAffectHoldings { get; set; }

        [JsonProperty("contentChange")]
        public bool ContentChange { get; set; }

        [JsonProperty("permit")]
        public string Permit { get; set; }

        [JsonProperty("providerCode")]
        public string ProviderCode { get; set; }

        [JsonProperty("providerName")]
        public string ProviderName { get; set; }

        [JsonProperty("size")]
        public string Size { get; set; }

        [JsonProperty("bundle")]
        public List<S57Bundle> Bundle { get; set; }

        [JsonProperty("status")]
        public S57Status Status { get; set; }

        [JsonProperty("replaces")]
        public List<string> Replaces { get; set; }

        [JsonProperty("replacedBy")]
        public List<string> ReplacedBy { get; set; }

        [JsonProperty("additionalCoverage")]
        public List<string> AdditionalCoverage { get; set; }

        [JsonProperty("geographicLimit")]
        public S57GeographicLimit GeographicLimit { get; set; }

        [JsonProperty("inUnitsOfSale")]
        public List<string> InUnitsOfSale { get; set; }

        [JsonProperty("s63")]
        public S63 S63 { get; set; }

        [JsonProperty("signature")]
        public S57Signature Signature { get; set; }

        [JsonProperty("ancillaryFiles")]
        public List<S57AncillaryFile> AncillaryFiles { get; set; }
    }
}
