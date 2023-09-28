using Azure.Storage.Queues.Models;

namespace UKHO.ERPFacade.EventAggregation.WebJob.Services
{
    public interface IAggregationService
    {
        Task MergeRecordOfSaleEvents(QueueMessage message);
    }
}
