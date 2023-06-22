using System.Diagnostics.CodeAnalysis;

namespace UKHO.ERPFacade.Common.Models
{
    [ExcludeFromCodeCoverage]
    public class SapActionConfiguration
    {
        public ICollection<SapAction> SapActions { get; set; }
    }
}
