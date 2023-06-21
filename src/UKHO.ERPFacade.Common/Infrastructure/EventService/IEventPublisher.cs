namespace UKHO.ERPFacade.Common.Infrastructure.EventService
{
    public interface IEventPublisher
    {
        Task<Result> Publish<TData>(EventBase<TData> eventData);
    }
}
