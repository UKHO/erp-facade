using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using UKHO.ERPFacade.Common.Logging;
using UKHO.ERPFacade.PublishPriceChange.WebJob.Services;

namespace UKHO.ERPFacade.PublishPriceChange.WebJob
{
    [ExcludeFromCodeCoverage]
    public class PublishPriceChangeWebJob
    {
        private readonly ILogger<PublishPriceChangeWebJob> _logger;
        private readonly ISlicingPublishingService _slicingPublishingService;

        public PublishPriceChangeWebJob(ILogger<PublishPriceChangeWebJob> logger, ISlicingPublishingService slicingPublishingService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _slicingPublishingService = slicingPublishingService ?? throw new ArgumentNullException(nameof(slicingPublishingService));
        }

        public void Start()
        {
            _logger.LogInformation(EventIds.WebjobPublishingPriceChangesEventStarted.ToEventId(), "Webjob started for publishing price changes");
            _slicingPublishingService.SliceAndPublishPriceChangeEvents();
            _logger.LogInformation(EventIds.WebjobPublishingPriceChangesEventCompleted.ToEventId(), "Webjob completed publishing price changes");
        }
    }
}
