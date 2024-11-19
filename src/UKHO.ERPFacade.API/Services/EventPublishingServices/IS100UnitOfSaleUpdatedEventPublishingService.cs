using UKHO.ERPFacade.Common.Models;
using UKHO.ERPFacade.Common.Models.CloudEvents;

namespace UKHO.ERPFacade.API.Services.EventPublishingServices;

public interface IS100UnitOfSaleUpdatedEventPublishingService
{
    public Task<Result> PublishEvent(BaseCloudEvent baseCloudEvent);
}
