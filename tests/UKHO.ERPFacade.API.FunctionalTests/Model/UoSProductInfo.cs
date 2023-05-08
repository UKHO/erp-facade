using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
