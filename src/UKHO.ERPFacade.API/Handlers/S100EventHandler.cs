using Newtonsoft.Json.Linq;

namespace UKHO.ERPFacade.API.Handlers
{
    public class S100EventHandler : IEventHandler
    {
        public string EventType => "uk.gov.UKHO.ENCPublishing.s100DataContentPublished.v1";

        public Task HandleEventAsync(JObject payload)
        {
            //logic
            return Task.CompletedTask;
        }
    }
}
