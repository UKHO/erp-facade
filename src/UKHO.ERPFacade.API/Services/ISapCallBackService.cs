namespace UKHO.ERPFacade.Services;

public interface ISapCallbackService
{
    Task<bool> IsValidCallbackAsync(string correlationId);
    Task ProcessSapCallback(string correlationId);
}
