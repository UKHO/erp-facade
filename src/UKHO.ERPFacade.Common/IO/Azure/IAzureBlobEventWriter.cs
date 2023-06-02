namespace UKHO.ERPFacade.Common.IO.Azure
{
    public interface IAzureBlobEventWriter
    {
        Task UploadEvent(string requestEvent, string blobContainerName, string blobName);

        bool CheckIfContainerExists(string containerName);

        string DownloadEvent(string blobName, string blobContainerName);
    }
}