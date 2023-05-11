using UKHO.ERPFacade.API.Models;

namespace UKHO.ERPFacade.API.Services
{
    public interface IERPFacadeService
    {
        List<UnitsOfSalePrices> BuildUnitOfSalePricePayload(List<PriceInformationEvent> priceInformationList);
    }
}
