using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace UKHO.ERPFacade.Common.Models.CloudEvents.S57Event
{
    [ExcludeFromCodeCoverage]
    public class S57EventData
    {
        [JsonProperty("correlationId")]
        public string CorrelationId { get; set; }

        [JsonProperty("ukhoWeekNumber")]
        public S57UkhoWeekNumber UkhoWeekNumber { get; set; }

        [JsonProperty("products")]
        public List<S57Product> Products { get; set; }

        [JsonProperty("unitsOfSale")]
        public List<S57UnitOfSale> UnitsOfSales { get; set; }
    }
}
