using System.Diagnostics.CodeAnalysis;

namespace UKHO.ERPFacade.API.Models
{
    [ExcludeFromCodeCoverage]
    public class ActionNumberConfiguration
    {
        public ICollection<ActionNumber> Actions { get; set; }
    }
}
