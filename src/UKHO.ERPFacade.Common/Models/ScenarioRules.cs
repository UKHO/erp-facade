using System.Diagnostics.CodeAnalysis;

namespace UKHO.ERPFacade.Common.Models
{
    [ExcludeFromCodeCoverage]
    public class Rule
    {
        public List<Conditions> Conditions { get; set; }
    }
}
