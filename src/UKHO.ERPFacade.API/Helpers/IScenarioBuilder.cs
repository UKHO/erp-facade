using UKHO.ERPFacade.API.Models;

namespace UKHO.ERPFacade.API.Helpers
{
    public interface IScenarioBuilder
    {
        List<Scenario> BuildScenarios(EncEventPayload eventData);
        //List<Scenario> BuildScenariosPayment(PriceInformationEvent priceInfoEvent);

        //List<UnitsOfSalePrices> BuildUnitOfSalePricePayload(List<PriceInformationEvent> priceInformationList);


    }
}
