using System.Diagnostics.CodeAnalysis;

namespace UKHO.ERPFacade.Common.Infrastructure.Config
{
    [ExcludeFromCodeCoverage]
    public class EnterpriseEventServiceConfiguration
    {
        public string ServiceUrl { get; set; }
        public string PublishEndpoint { get; set; }
        public string ClientId { get; set; }
        public string PublisherScope { get; set; }
    }
}