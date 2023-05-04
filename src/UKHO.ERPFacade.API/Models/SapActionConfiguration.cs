using System.Diagnostics.CodeAnalysis;

namespace UKHO.ERPFacade.API.Models
{
    [ExcludeFromCodeCoverage]
    public class SapActionConfiguration
    {
        public ICollection<SapAction> SapActions { get; set; }
    }
}
