using UKHO.ERPFacade.Common.Models.CloudEvents;

namespace UKHO.ERPFacade.API.Handlers
{
    public interface IEventHandler
    {
        string EventType { get; }
        Task ProcessEventAsync(BaseCloudEvent baseCloudEvent);
    }
}
