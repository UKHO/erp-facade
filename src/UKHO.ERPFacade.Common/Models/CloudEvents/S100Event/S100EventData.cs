using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace UKHO.ERPFacade.Common.Models.CloudEvents.S100Event
{
    [ExcludeFromCodeCoverage]
    public class S100EventData
    {
        [JsonProperty("correlationId")]
        public string CorrelationId { get; set; }

        [JsonProperty("products")]
        public List<S100Product> Products { get; set; }

        [JsonProperty("unitsOfSale")]
        public List<S100UnitOfSale> UnitsOfSales { get; set; }
    }
}
