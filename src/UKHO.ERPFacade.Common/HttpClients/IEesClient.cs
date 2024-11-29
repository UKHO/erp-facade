using UKHO.ERPFacade.Common.Models.CloudEvents;

namespace UKHO.ERPFacade.Common.HttpClients
{
    public interface IEesClient
    {
        Task<HttpResponseMessage> GetAsync(string url);
        Task<HttpResponseMessage> PublishEventAsync(BaseCloudEvent cloudEvent);
    }
}
