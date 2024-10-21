using UKHO.ERPFacade.Common.Models.CloudEvents;

namespace UKHO.ERPFacade.API.Handlers
{
    public interface IEventHandler
    {
        Task ProcessEventAsync(BaseCloudEvent baseCloudEvent);
    }
}
