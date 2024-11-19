using UKHO.ERPFacade.Common.Models.CloudEvents;

namespace UKHO.ERPFacade.API.Services;

public interface ISapCallBackService
{
    Task<bool> IsValidCallback(string correlationId);
    Task<BaseCloudEvent> GetEventPayload(string correlationId);
}
