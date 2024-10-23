using UKHO.ERPFacade.Common.Models.CloudEvents;

namespace UKHO.ERPFacade.API.Handlers
{
    public class S100EventHandler : IEventHandler
    {
        public Task ProcessEventAsync(BaseCloudEvent baseCloudEvent) => throw new NotImplementedException();
    }
}
