using System.Diagnostics.CodeAnalysis;

namespace UKHO.ERPFacade.API.Models
{
    [ExcludeFromCodeCoverage]
    public class ScenarioRules
    {
        public ScenarioType Scenario { get; set; }
        public List<Rule> Rules { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class Rule
    {
        public string AttributeDataType { get; set; }
        public string AttributeName { get; set; }
        public string AttriuteValue { get; set; }
    }

    public enum ScenarioType
    {
        NewCell = 1,
        CancelReplaceCell = 2,
        UpdateCell = 3,
        ChangeCell = 4,
        ChangeUnitOfSale = 5
    }
}
