using Newtonsoft.Json.Linq;
using UKHO.ERPFacade.API.Models;

namespace UKHO.ERPFacade.API.Services
{
    public interface IERPFacadeService
    {
        List<UnitsOfSalePrices> BuildUnitOfSalePricePayload(List<PriceInformationEvent> priceInformationList);

        UnitOfSalePriceEventPayload BuildPriceEventPayload(List<UnitsOfSalePrices> unitsOfSalePriceList, string exisitingEesEvent);

        BulkPriceEventPayload BuildBulkPriceEventPayload(UnitsOfSalePrices unitsOfSalePriceList);
    }
}
