using System.Diagnostics.CodeAnalysis;
using Azure.Storage.Queues.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using UKHO.ERPFacade.Common.Logging;
using UKHO.ERPFacade.EventAggregation.WebJob.Services;

namespace UKHO.ERPFacade.EventAggregation.WebJob
{
    [ExcludeFromCodeCoverage]
    public class EventAggregationWebjob
    {
        private readonly ILogger<EventAggregationWebjob> _logger;
        private readonly IAggregationService _aggregationService;

        public EventAggregationWebjob(ILogger<EventAggregationWebjob> logger, IAggregationService aggregationService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _aggregationService = aggregationService ?? throw new ArgumentNullException(nameof(aggregationService));
        }

        public Task ProcessQueueMessage([QueueTrigger("recordofsaleevents")] QueueMessage message)
        {
            _logger.LogInformation(EventIds.WebjobForEventAggregationStarted.ToEventId(), "Webjob started for merging record of sale events.");
            _aggregationService.MergeRecordOfSaleEvents(message);
            _logger.LogInformation(EventIds.WebjobForEventAggregationCompleted.ToEventId(), "Webjob completed for merging record of sale events.");

            return Task.CompletedTask;
        }
    }
}
