﻿using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics.CodeAnalysis;
using UKHO.ERPFacade.Common.Configuration;
using UKHO.ERPFacade.Common.IO;
using UKHO.ERPFacade.Common.Logging;
using UKHO.ERPFacade.WebJob.Services;

namespace UKHO.ERPFacade.WebJob
{
    [ExcludeFromCodeCoverage]
    public class ErpFacadeWebJob
    {
        private readonly ILogger<ErpFacadeWebJob> _logger;
        private readonly IOptions<AzureStorageConfiguration> _azureStorageConfig;
        private readonly IOptions<ErpFacadeWebJobConfiguration> _erpFacadeWebJobConfig;
        private readonly IMonitoringService _monitoringService;


        public ErpFacadeWebJob(ILogger<ErpFacadeWebJob> logger,
                               IOptions<AzureStorageConfiguration> azureStorageConfig,
                               IOptions<ErpFacadeWebJobConfiguration> erpFacadeWebJobConfig,
                               IMonitoringService monitoringService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _azureStorageConfig = azureStorageConfig ?? throw new ArgumentNullException(nameof(azureStorageConfig));
            _erpFacadeWebJobConfig = erpFacadeWebJobConfig ?? throw new ArgumentNullException(nameof(erpFacadeWebJobConfig));
            _monitoringService = monitoringService ?? throw new ArgumentNullException(nameof(monitoringService));
        }

        public void Start()
        {
            try
            {
                //Code to monitor the table records.
                _logger.LogInformation(EventIds.WebjobProcessEventStarted.ToEventId(), "Webjob started for the processing the incomplete transactions.");
                _monitoringService.MonitorIncompleteTransactions();
                _logger.LogInformation(EventIds.WebjobProcessEventStarted.ToEventId(), "Webjob completed for processing the incomplete transactions.");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
