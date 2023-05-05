using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UKHO.ERPFacade.Common.IO;

namespace UKHO.ERPFacade.WebJob.Services
{
    public class MonitoringService : IMonitoringService
    {
        private readonly ILogger<MonitoringService> _logger;
        private readonly IAzureTableReaderWriter _azureTableReaderWriter;

        public MonitoringService(ILogger<MonitoringService> logger,
                               IAzureTableReaderWriter azureTableReaderWriter)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _azureTableReaderWriter = azureTableReaderWriter ?? throw new ArgumentNullException(nameof(azureTableReaderWriter));
        }

        public void MonitorIncompleteTransactions()
        {
            _azureTableReaderWriter.ValidateAndUpdateIsNotifiedEntity();
        }
    }
}
