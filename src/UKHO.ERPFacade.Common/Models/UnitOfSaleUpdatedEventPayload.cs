using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;
using UKHO.ERPFacade.Common.Infrastructure;

namespace UKHO.ERPFacade.Common.Models
{
    [ExcludeFromCodeCoverage]
    public class UnitOfSaleUpdatedEventPayload : EventBase<UnitOfSaleUpdatedEventData>
    {
        public UnitOfSaleUpdatedEventPayload(UnitOfSaleUpdatedEventData unitOfSaleUpdatedEventData, string subject)
        {
            Data = unitOfSaleUpdatedEventData;
            Subject = subject;
            Id = Guid.NewGuid().ToString();
            EventName = "uk.gov.ukho.erp.unitOfSaleUpdated.v1";
        }

    }

    [ExcludeFromCodeCoverage]
    public class UnitOfSaleUpdatedEventData
    {
        [JsonProperty("correlationId")]
        public string CorrelationId { get; set; }

        [JsonProperty("products")]
        public List<Product> Products { get; set; }

        [JsonProperty("unitsOfSale")]
        public List<UnitOfSale> UnitsOfSales { get; set; }

        [JsonProperty("unitsOfSalePrices")]
        public List<UnitsOfSalePrices> UnitsOfSalePrices { get; set; }
    }
}
