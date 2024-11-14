using UKHO.ERPFacade.Common.Models.CloudEvents;

namespace UKHO.ERPFacade.Common.Infrastructure.EventService
{
    public interface IEventPublisher
    {
        Task<Result> Publish(BaseCloudEvent eventData);
    }
}
