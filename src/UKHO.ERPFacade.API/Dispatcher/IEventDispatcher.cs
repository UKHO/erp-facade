using UKHO.ERPFacade.Common.Models.CloudEvents;

namespace UKHO.ERPFacade.API.Dispatcher
{
    public interface IEventDispatcher
    {
        Task<bool> DispatchEventAsync(BaseCloudEvent cloudEvent);
    }
}
