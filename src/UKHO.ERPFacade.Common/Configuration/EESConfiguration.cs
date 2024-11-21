using System.Diagnostics.CodeAnalysis;

namespace UKHO.ERPFacade.Common.Configuration
{
    [ExcludeFromCodeCoverage]
    public class EESConfiguration
    {
        public string ServiceUrl { get; set; } = string.Empty;
        public string PublishEndpoint { get; set; } = string.Empty;
        public string ClientId { get; set; } = string.Empty;
        public string PublisherScope { get; set; } = string.Empty;
        public string SourceApplicationUri { get; set; } = string.Empty;
        public bool UseLocalResources { get; set; }
    }
}
