using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace UKHO.ERPFacade.Common.Models.CloudEvents.S100Event
{
    [ExcludeFromCodeCoverage]
    public class S100BoundingBox
    {
        [JsonProperty("northLimit")]
        public double NorthLimit { get; set; }
        [JsonProperty("southLimit")]
        public double SouthLimit { get; set; }
        [JsonProperty("eastLimit")]
        public double EastLimit { get; set; }
        [JsonProperty("westLimit")]
        public double WestLimit { get; set; }
    }
}
