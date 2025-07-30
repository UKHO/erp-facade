using System.Diagnostics.CodeAnalysis;

namespace UKHO.ERPFacade.Common.Models.SapActionConfigurationModels
{
    [ExcludeFromCodeCoverage]
    public class Actions
    {
        public int ActionNumber { get; set; }
        public string ActionName { get; set; }
        public string Product { get; set; }
        public ICollection<Rules> Rules { get; set; }
        public ICollection<Attributes> Attributes { get; set; }
    }
}
