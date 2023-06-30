using Newtonsoft.Json;

namespace UKHO.ERPFacade.API.FunctionalTests.Model
{
    internal class UoSProductInfo
    {
        [JsonProperty("productType")]
        public string ProductType { get; set; }

        [JsonProperty("providerCode")]
        public string ProviderCode { get; set; }

        [JsonProperty("agency")]
        public string Agency { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }
    }
}
