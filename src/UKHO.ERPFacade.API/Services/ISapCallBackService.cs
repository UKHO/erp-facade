using UKHO.ERPFacade.Common.Models.CloudEvents;

namespace UKHO.ERPFacade.Services;

public interface ISapCallBackService
{
    Task<bool> IsValidCallback(string correlationId);
    Task<BaseCloudEvent> GetEventPayload(string correlationId);
    Task UpdateResponseTimeEntity(string correlationId);
    Task UpdateEventStatusAndEventPublishDateTimeEntity(string correlationId);
}
