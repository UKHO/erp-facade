using System.Diagnostics.CodeAnalysis;

namespace UKHO.ERPFacade.API.Models
{
    [ExcludeFromCodeCoverage]
    public class ScenarioRuleConfiguration
    {
        public ICollection<ScenarioRules> ScenarioRules { get; set; }
    }
}
