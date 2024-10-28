using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace UKHO.ERPFacade.Common.Models.CloudEvents.S57Event
{
    [ExcludeFromCodeCoverage]
    public class S57AncillaryFile
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("hash")]
        public string Hash { get; set; }

        [JsonProperty("fileSize")]
        public int FileSize { get; set; }
    }
}
