using System.Diagnostics.CodeAnalysis;

namespace UKHO.ERPFacade.Common.Infrastructure.Config
{
    [ExcludeFromCodeCoverage]
    public class InteractiveLoginConfiguration
    {
        public string MicrosoftOnlineLoginUrl { get; set; }
        public string TenantId { get; set; }
    }
}