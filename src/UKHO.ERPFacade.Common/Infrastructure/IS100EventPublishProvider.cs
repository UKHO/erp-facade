using UKHO.ERPFacade.Common.Infrastructure.EventProvider;

namespace UKHO.ERPFacade.Common.Infrastructure
{
    public interface IS100EventPublishProvider
    {
        Task<Result> Publish<TData>(CloudEvent<TData> eventData);
    }
}
