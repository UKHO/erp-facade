using UKHO.ERPFacade.Common.Models;
using UKHO.ERPFacade.Common.Models.CloudEvents;

namespace UKHO.ERPFacade.Common.EventPublisher
{
    public interface IEventPublisher
    {
        Task<Result> Publish(BaseCloudEvent cloudEvent);
    }
}
