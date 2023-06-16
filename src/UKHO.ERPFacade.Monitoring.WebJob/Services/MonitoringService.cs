using System.Diagnostics.CodeAnalysis;
using UKHO.ERPFacade.Common.IO.Azure;

namespace UKHO.ERPFacade.Monitoring.WebJob.Services
{
    [ExcludeFromCodeCoverage]
    public class MonitoringService : IMonitoringService
    {
        private readonly IAzureTableReaderWriter _azureTableReaderWriter;

        public MonitoringService(IAzureTableReaderWriter azureTableReaderWriter)
        {
            _azureTableReaderWriter = azureTableReaderWriter ?? throw new ArgumentNullException(nameof(azureTableReaderWriter));
        }

        public void MonitorIncompleteTransactions()
        {
            _azureTableReaderWriter.ValidateAndUpdateIsNotifiedEntity();
        }
    }
}