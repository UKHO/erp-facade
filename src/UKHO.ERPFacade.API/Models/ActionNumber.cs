using System.Diagnostics.CodeAnalysis;

namespace UKHO.ERPFacade.API.Models
{
    [ExcludeFromCodeCoverage]
    public class ActionNumber
    {
        public string Scenario { get; set; }
        public List<int> ActionNumbers { get; set; }
    }
}
