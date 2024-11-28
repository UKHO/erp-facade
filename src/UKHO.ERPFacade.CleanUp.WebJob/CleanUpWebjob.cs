using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using UKHO.ERPFacade.CleanUp.WebJob.Services;
using UKHO.ERPFacade.Common.Logging;

namespace UKHO.ERPFacade.CleanUp.WebJob
{
    [ExcludeFromCodeCoverage]
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

        public async Task Start()
        {
            _logger.LogInformation(EventIds.CleanupWebjobStarted.ToEventId(), "Clean up webjob started.");
             await _cleanUpService.CleanAsync();
            _logger.LogInformation(EventIds.CleanupWebjobCompleted.ToEventId(), "Clean up webjob completed.");
        }
    }
}
