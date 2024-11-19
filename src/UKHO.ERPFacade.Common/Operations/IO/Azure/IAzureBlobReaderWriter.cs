namespace UKHO.ERPFacade.Common.Operations.IO.Azure
{
    public interface IAzureBlobReaderWriter
    {
        Task UploadEventAsync(string requestEvent, string blobContainerName, string blobName);
        bool CheckIfContainerExists(string containerName);
        Task<string> DownloadEventAsync(string blobName, string blobContainerName);
        DateTime GetBlobCreateDate(string blobName, string blobContainerName);
        IEnumerable<string> GetBlobsInContainer(string blobContainerName, string corrId);
        Task<bool> DeleteBlobAsync(string blobName, string blobContainerName);
        Task<bool> DeleteContainerAsync(string blobContainerName);
        Task<bool> DeleteDirectoryAsync(string blobContainerName, string directoryName);
        List<string> GetBlobNamesInFolder(string blobContainerName, string corrId);
    }
}
