using System.Diagnostics.CodeAnalysis;

namespace UKHO.ERPFacade.Common.Models
{
    [ExcludeFromCodeCoverage]
    public class DecryptedPermit
    {
        public string ActiveKey { get; set; }
        public string NextKey { get; set; }
    }
}
