using Newtonsoft.Json;

namespace UKHO.ERPFacade.Common.Models
{
    public class LicenceUpdatedSapActionConfiguration
    {
        //[JsonProperty("LicenceUpdatedSapActions")]
        //public LicenceUpdatedSapAction LicenceUpdatedSapActions { get; set; }
        [JsonProperty("Attributes")]
        public Attribute[] Attributes { get; set; }
    }
}

