using System;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace UKHO.ERPFacade.Common.Models
{
    public class LicenceUpdatedSapAction
    {
        [JsonProperty("Attributes")]
        public Attribute[] Attributes { get; set; }
    }
    public partial class Attribute
    {
        [JsonProperty("IsRequired")]
        public bool IsRequired { get; set; }

        [JsonProperty("JsonPropertyName")]
        public string JsonPropertyName { get; set; }

        [JsonProperty("XmlNodeName")]
        public string XmlNodeName { get; set; }
    }
}
