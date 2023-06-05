using System.Diagnostics.CodeAnalysis;

namespace UKHO.ERPFacade.API.Models
{
    [ExcludeFromCodeCoverage]
    public class SapAction
    {
        public int ActionNumber { get; set; }
        public string Action { get; set; }
        public string Product { get; set; }
        public ICollection<Rule> Rules { get; set; }
        public ICollection<ActionItemAttribute> Attributes { get; set; }
    }
}
