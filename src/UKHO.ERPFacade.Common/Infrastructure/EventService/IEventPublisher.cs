using UKHO.ERPFacade.Common.Infrastructure.EventService.EventProvider;

namespace UKHO.ERPFacade.Common.Infrastructure.EventService
{
    public interface IEventPublisher
    {
        Task<Result> Publish<TData>(CloudEvent<TData> eventData);
    }
}
