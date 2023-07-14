using UKHO.ERPFacade.Common.Models;

namespace UKHO.ERPFacade.Common.Services
{
    public interface IErpFacadeService
    {
        List<UnitsOfSalePrices> MapAndBuildUnitsOfSalePrices(List<PriceInformation> priceInformationList, List<string> unitOfSalesList, string correlationId, string eventId);

        UnitOfSaleUpdatedEventPayload BuildUnitsOfSaleUpdatedEventPayload(List<UnitsOfSalePrices> unitsOfSalePriceList, string encEventPayloadJson, string correlationId, string eventId);

        PriceChangeEventPayload BuildPriceChangeEventPayload(List<UnitsOfSalePrices> unitsOfSalePriceList, string unitName, string correlationId, string eventId);
    }
}
