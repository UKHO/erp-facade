using CloudNative.CloudEvents;

namespace UKHO.ERPFacade.API.Services
{
    public interface IWebhookService
    {
        Task HandleEvent(string payloadJson, CloudEvent payload);        
    }
}
