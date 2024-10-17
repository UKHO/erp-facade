

using CloudNative.CloudEvents;

namespace UKHO.ERPFacade.API.Handlers
{
    public interface IEventDispatcher
    {
        Task DispatchAsync(CloudEvent payload);
    }
}
