using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;
using UKHO.ERPFacade.Common.Exceptions;
using UKHO.ERPFacade.Common.Logging;
using UKHO.ERPFacade.WebJob.Services;

namespace UKHO.ERPFacade.WebJob
{
    [ExcludeFromCodeCoverage]
    public class ErpFacadeWebJob
    {
        private readonly ILogger<ErpFacadeWebJob> _logger;
        private readonly IMonitoringService _monitoringService;

        public ErpFacadeWebJob(ILogger<ErpFacadeWebJob> logger,
                               IMonitoringService monitoringService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _monitoringService = monitoringService ?? throw new ArgumentNullException(nameof(monitoringService));
        }

        public void Start()
        {
            try
            {
                //Code to monitor the table records.
                _logger.LogInformation(EventIds.WebjobProcessEventStarted.ToEventId(), "Webjob started the monitoring of callbacks from SAP.");

                _monitoringService.MonitorIncompleteTransactions();

                _logger.LogInformation(EventIds.WebjobProcessEventCompleted.ToEventId(), "Webjob completed the monitoring of callbacks from SAP.");
            }
            catch (Exception ex)
            {
                var exceptionType = ex.GetType();

                if (exceptionType == typeof(ERPFacadeException))
                {
                    EventIds eventId = (EventIds)((ERPFacadeException)ex).EventId.Id;
                    _logger.LogError(eventId.ToEventId(), ex, eventId.ToString());
                }
                else
                {
                    _logger.LogError(EventIds.UnhandledException.ToEventId(), ex, "Exception occured while processing ErpFacade WebJob.");
                }
            }
        }
    }
}