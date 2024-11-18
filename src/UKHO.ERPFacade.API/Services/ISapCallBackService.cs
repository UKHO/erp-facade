namespace UKHO.ERPFacade.API.Services;

public interface ISapCallBackService
{
    public Task DownloadS100EventAndPublishToEes(string correlationId);
}
