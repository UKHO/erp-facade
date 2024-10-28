using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace UKHO.ERPFacade.Common.Models.CloudEvents.S57Event
{
    [ExcludeFromCodeCoverage]
    public class S57GeographicLimit
    {
        [JsonProperty("boundingBox")]
        public S57BoundingBox BoundingBox { get; set; }
    }
}
