using UKHO.ERPFacade.Common.Models;

namespace UKHO.ERPFacade.Common.EventPublisher
{
    public interface IEventPublisher
    {
        Task<Result> Publish<TData>(TData eventData);
    }
}
