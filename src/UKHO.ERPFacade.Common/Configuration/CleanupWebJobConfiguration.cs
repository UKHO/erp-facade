using System.Diagnostics.CodeAnalysis;

namespace UKHO.ERPFacade.Common.Configuration
{
    [ExcludeFromCodeCoverage]
    public class CleanupWebJobConfiguration
    {       
        public string CleanUpDurationInDays { get; set; } = string.Empty;
    }
}
