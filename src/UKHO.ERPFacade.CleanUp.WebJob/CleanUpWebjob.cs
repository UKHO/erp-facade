using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UKHO.ERPFacade.CleanUp.WebJob.Services;
using UKHO.ERPFacade.Common.Configuration;
using UKHO.ERPFacade.Common.Logging;

namespace UKHO.ERPFacade.CleanUp.WebJob
{
    public class CleanUpWebjob
    {
        private readonly ILogger<CleanUpWebjob> _logger;
        private readonly ICleanUpService _cleanUpService;

        public CleanUpWebjob(ILogger<CleanUpWebjob> logger,
                               ICleanUpService cleanUpService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cleanUpService = cleanUpService ?? throw new ArgumentNullException(nameof(cleanUpService));
        }

        public void Start()
        {
            _logger.LogInformation(EventIds.WebjobCleanUpEventStarted.ToEventId(), "Webjob started for clean up");
            _cleanUpService.CleanUpAzureTableAndBlobs();
            _logger.LogInformation(EventIds.WebjobCleanUpEventCompleted.ToEventId(), "Webjob completed clean up");
        }
    }
}
