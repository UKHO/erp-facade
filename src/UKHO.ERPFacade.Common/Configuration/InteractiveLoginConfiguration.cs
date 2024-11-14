using System.Diagnostics.CodeAnalysis;

namespace UKHO.ERPFacade.Common.Configuration
{
    [ExcludeFromCodeCoverage]
    public class InteractiveLoginConfiguration
    {
        public string MicrosoftOnlineLoginUrl { get; set; }
        public string TenantId { get; set; }
    }
}
