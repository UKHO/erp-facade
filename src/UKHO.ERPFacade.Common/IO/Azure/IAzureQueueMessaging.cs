using Newtonsoft.Json.Linq;

namespace UKHO.ERPFacade.Common.IO.Azure
{
    public interface IAzureQueueMessaging
    {
        Task SendMessageToQueue(JObject rosEventJson);
    }
}
