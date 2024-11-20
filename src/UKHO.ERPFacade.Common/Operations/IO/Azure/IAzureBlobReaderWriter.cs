namespace UKHO.ERPFacade.Common.Operations.IO.Azure
{
    public interface IAzureBlobReaderWriter
    {
        Task UploadEventAsync(string requestEvent, string blobContainerName, string blobName);
        Task<string> DownloadEventAsync(string blobName, string blobContainerName);
        Task<bool> DeleteContainerAsync(string blobContainerName);
        Task<List<string>> GetBlobNamesInFolderAsync(string blobContainerName, string corrId);
    }
}
