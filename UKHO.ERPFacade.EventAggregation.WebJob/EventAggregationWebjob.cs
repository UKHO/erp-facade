using System.Diagnostics.CodeAnalysis;
using Azure.Storage.Queues.Models;
using Microsoft.Azure.WebJobs;

namespace UKHO.ERPFacade.EventAggregation.WebJob
{
    [ExcludeFromCodeCoverage]
    public class EventAggregationWebjob
    {
        public async Task ProcessQueueMessage([QueueTrigger("recordofsaleevents")] QueueMessage message)
        {
            //call AggregationService.cs

            throw new Exception();
        }
    }
}
