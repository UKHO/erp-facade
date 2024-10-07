using Newtonsoft.Json.Linq;

namespace UKHO.ERPFacade.Common.IO.Azure
{
    public interface IAzureQueueHelper
    {
        Task AddMessage(string queueMessage);
    }
}
