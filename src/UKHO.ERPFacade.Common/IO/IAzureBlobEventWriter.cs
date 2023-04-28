namespace UKHO.ERPFacade.Common.IO
{
    public interface IAzureBlobEventWriter
    {
        Task UploadEvent(object requestObject, string requestFormat, string traceId);
    }
}