using UKHO.ERPFacade.API.Models;

namespace UKHO.ERPFacade.API.Helpers
{
    public interface IScenarioBuilder
    {
        List<Scenario> BuildScenarios(EESEventPayload eventData);
        //List<Scenario> BuildScenariosPayment(PriceInformationEvent priceInfoEvent);

        //List<UnitsOfSalePrices> BuildUnitOfSalePricePayload(List<PriceInformationEvent> priceInformationList);


    }
}
