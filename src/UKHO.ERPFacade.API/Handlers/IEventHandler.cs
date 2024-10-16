using Newtonsoft.Json.Linq;

namespace UKHO.ERPFacade.API.Handlers
{
    public interface IEventHandler
    {
        string EventType { get; }
        Task HandleEventAsync(JObject payload);
    }
}
