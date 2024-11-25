namespace UKHO.ERPFacade.Services;

public interface IS100SapCallBackService
{
    Task<bool> IsValidCallbackAsync(string correlationId);
    Task ProcessSapCallback(string correlationId);
}
