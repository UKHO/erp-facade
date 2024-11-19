using UKHO.ERPFacade.Common.Models;
using UKHO.ERPFacade.Common.Models.CloudEvents;

namespace UKHO.ERPFacade.Common.HttpClients
{
    public interface IEESClient
    {
        Task<HttpResponseMessage> Get(string url);
        Task<Result> PostAsync(BaseCloudEvent cloudEvent);
    }
}
