using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace UKHO.ERPFacade.Common.Models.CloudEvents.S57Event
{
    [ExcludeFromCodeCoverage]
    public class S57Bundle
    {
        [JsonProperty("bundleType")]
        public string BundleType { get; set; }

        [JsonProperty("location")]
        public string Location { get; set; }
    }
}
