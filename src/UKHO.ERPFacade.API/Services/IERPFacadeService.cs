using Newtonsoft.Json.Linq;
using UKHO.ERPFacade.API.Models;

namespace UKHO.ERPFacade.API.Services
{
    public interface IERPFacadeService
    {
        List<UnitsOfSalePrices> BuildUnitOfSalePricePayload(List<PriceInformationEvent> priceInformationList);

        JObject BuildPriceEventPayload(List<UnitsOfSalePrices> unitsOfSalePriceList, string exisitingEesEvent);

        JObject BuildBulkPriceEventPayload(UnitsOfSalePrices unitsOfSalePriceList);
    }
}
