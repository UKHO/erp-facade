using System.Diagnostics.CodeAnalysis;
using Azure.Storage.Queues.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
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
            _aggregationService.MergeRecordOfSaleEvents(message);

            return Task.CompletedTask;
        }
    }
}
