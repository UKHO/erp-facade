using System.Diagnostics.CodeAnalysis;

namespace UKHO.ERPFacade.Common.Models
{
    [ExcludeFromCodeCoverage]
    public class Error
    {
        public string Source { get; set; }
        public string Description { get; set; }
    }
}
