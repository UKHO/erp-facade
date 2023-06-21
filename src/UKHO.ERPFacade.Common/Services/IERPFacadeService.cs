using UKHO.ERPFacade.Common.Models;

namespace UKHO.ERPFacade.Common.Services
{
    public interface IErpFacadeService
    {
        List<UnitsOfSalePrices> MapAndBuildUnitsOfSalePrices(List<PriceInformation> priceInformationList, List<string> unitOfSalesList);

        UnitOfSaleUpdatedEventPayload BuildUnitsOfSaleUpdatedEventPayload(List<UnitsOfSalePrices> unitsOfSalePriceList, string encEventPayloadJson);
    }
}
