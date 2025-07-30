using System.Diagnostics.CodeAnalysis;

namespace UKHO.ERPFacade.Common.Policies
{
    [ExcludeFromCodeCoverage]
    public class RetryPolicyConfiguration
    {
        public int RetryCount { get; set; }
        public double Duration { get; set; }
    }
}
