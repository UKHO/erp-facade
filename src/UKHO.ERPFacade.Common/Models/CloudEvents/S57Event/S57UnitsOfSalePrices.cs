using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace UKHO.ERPFacade.Common.Models.CloudEvents.S57Event
{
    [ExcludeFromCodeCoverage]
    public class S57UnitsOfSalePrices
    {
        [JsonProperty("unitName")]
        public string UnitName { get; set; }

        [JsonProperty("price")]
        public List<S57Price> Price { get; set; }
    }
}
