using UKHO.ERPFacade.Common.Models.CloudEvents;

namespace UKHO.ERPFacade.Services;

public interface ISapCallbackService
{
    Task<bool> IsValidCallbackAsync(string correlationId);
    Task<BaseCloudEvent> GetEventPayload(string correlationId);
    Task UpdateResponseDateTimeAsync(string correlationId);
    Task UpdateEventStatusAndEventPublishDateTimeEntity(string correlationId);
}
