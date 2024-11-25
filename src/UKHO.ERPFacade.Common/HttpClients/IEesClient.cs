using UKHO.ERPFacade.Common.Models;
using UKHO.ERPFacade.Common.Models.CloudEvents;

namespace UKHO.ERPFacade.Common.HttpClients
{
    public interface IEesClient
    {
        Task<HttpResponseMessage> Get(string url);
        Task<HttpResponseMessage> PostAsync(BaseCloudEvent cloudEvent);
    }
}
