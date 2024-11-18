using UKHO.ERPFacade.Common.Models.CloudEvents;

namespace UKHO.ERPFacade.API.Services;

public interface IEventService
{
    public Task BuildAndPublishEvent(BaseCloudEvent baseCloudEvent, string type);
}
