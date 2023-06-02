using UKHO.ERPFacade.API.Models;

namespace UKHO.ERPFacade.API.Helpers
{
    public interface IScenarioBuilder
    {
        List<Scenario> BuildScenarios(EncEventPayload eventData);
    }
}