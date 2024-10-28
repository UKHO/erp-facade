using UKHO.ERPFacade.Common.Constants;
using UKHO.ERPFacade.Common.Models.CloudEvents;

namespace UKHO.ERPFacade.API.Handlers
{
    public class S100EventHandler : IEventHandler
    {
        public string EventType => Events.S100EventType;
        public Task ProcessEventAsync(BaseCloudEvent baseCloudEvent) => throw new NotImplementedException();
    }
}
