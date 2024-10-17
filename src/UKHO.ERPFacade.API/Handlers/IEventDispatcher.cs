using Newtonsoft.Json.Linq;

namespace UKHO.ERPFacade.API.Handlers
{
    public interface IEventDispatcher
    {
        Task DispatchAsync(JObject payload);
    }
}
