using Newtonsoft.Json;

namespace UKHO.ERPFacade.Common.Models.S100Event
{
    public class S100BoundingBox
    {
        [JsonProperty("northLimit")]
        public double NorthLimit { get; set; }
        [JsonProperty("southLimit")]
        public int SouthLimit { get; set; }
        [JsonProperty("eastLimit")]
        public int EastLimit { get; set; }
        [JsonProperty("westLimit")]
        public double WestLimit { get; set; }
    }
}
