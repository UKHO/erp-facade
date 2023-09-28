namespace UKHO.ERPFacade.Common.IO.Azure
{
    public interface IAzureBlobEventWriter
    {
        Task UploadEvent(string requestEvent, string blobContainerName, string blobName);

        bool CheckIfContainerExists(string containerName);

        string DownloadEvent(string blobName, string blobContainerName);

        DateTime GetBlobCreateDate(string blobName, string blobContainerName);

        IEnumerable<string> GetBlobsInContainer(string blobContainerName, string corrId);

        bool DeleteBlob(string blobName, string blobContainerName);

        bool DeleteContainer(string blobContainerName);

        List<string> GetBlobNamesInFolder(string blobContainerName, string corrId);
    }
}
