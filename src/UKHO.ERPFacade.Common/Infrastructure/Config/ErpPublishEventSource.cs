using System.Diagnostics.CodeAnalysis;

namespace UKHO.ERPFacade.Common.Infrastructure.Config
{
    [ExcludeFromCodeCoverage]
    public class ErpPublishEventSource
    {
        public string ServiceUrl { get; set; }
        public string PublishEndpoint { get; set; }
        public string ClientId { get; set; }
        public string PublisherScope { get; set; }
        public string ApplicationUri { get; set; }
    }
}
