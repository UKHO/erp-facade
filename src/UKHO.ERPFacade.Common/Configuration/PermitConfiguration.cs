using System.Diagnostics.CodeAnalysis;

namespace UKHO.ERPFacade.Common.Configuration
{
    [ExcludeFromCodeCoverage]
    public class PermitConfiguration
    {
        public string PermitDecryptionHardwareId { get; set; } = string.Empty;
    }
}
