using System.Diagnostics.CodeAnalysis;

namespace UKHO.ERPFacade.Common.Configuration
{
    [ExcludeFromCodeCoverage]
    public class ErpFacadeWebJobConfiguration
    {
        public string SapCallbackDurationInMins { get; set; } = string.Empty;
        public string CleanUpDurationInDays { get; set; } = string.Empty;

    }
}
