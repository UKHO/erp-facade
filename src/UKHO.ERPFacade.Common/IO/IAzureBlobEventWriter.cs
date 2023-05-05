namespace UKHO.ERPFacade.Common.IO
{
    public interface IAzureBlobEventWriter
    {
        Task UploadEvent(string requestEvent, string blobContainerName, string blobName);

        bool CheckIfContainerExists(string containerName);
    }
}