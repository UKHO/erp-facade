using System.Diagnostics.CodeAnalysis;

namespace UKHO.ERPFacade.Common.Configuration
{
    [ExcludeFromCodeCoverage]
    public class RetryPolicyConfiguration
    {
        public int Count { get; set; }
        public int Duration { get; set; }
    }
}
