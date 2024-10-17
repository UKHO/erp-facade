

using CloudNative.CloudEvents;

namespace UKHO.ERPFacade.API.Handlers
{
    public interface IEventHandler
    {
        string EventType { get; }
        Task HandleEventAsync(CloudEvent payload);
    }
}
