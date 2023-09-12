using System.Diagnostics.CodeAnalysis;

namespace UKHO.ERPFacade.Common.Configuration
{
    [ExcludeFromCodeCoverage]
    public class AzureStorageConfiguration
    {
        public string ConnectionString { get; set; } = string.Empty;

        public string QueueName { get; set; } = string.Empty;
    }
}
