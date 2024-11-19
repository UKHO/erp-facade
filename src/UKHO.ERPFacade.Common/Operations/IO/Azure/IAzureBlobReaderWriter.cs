namespace UKHO.ERPFacade.Common.Operations.IO.Azure
{
    public interface IAzureBlobReaderWriter
    {
        Task UploadEventAsync(string requestEvent, string blobContainerName, string blobName);
        bool CheckIfContainerExists(string containerName);
        string DownloadEvent(string blobName, string blobContainerName);
        DateTime GetBlobCreateDate(string blobName, string blobContainerName);
        IEnumerable<string> GetBlobsInContainer(string blobContainerName, string corrId);
        bool DeleteBlob(string blobName, string blobContainerName);
        bool DeleteContainer(string blobContainerName);
        bool DeleteDirectory(string blobContainerName, string directoryName);
        List<string> GetBlobNamesInFolder(string blobContainerName, string corrId);
        Task<string> DownloadEventAsync(string blobName, string blobContainerName);
    }
}
