using Newtonsoft.Json.Linq;

namespace UKHO.ERPFacade.API.Dispatcher
{
    public interface IEventDispatcher
    {
        Task DispatchEventAsync(JObject cloudEvent);
    }
}
