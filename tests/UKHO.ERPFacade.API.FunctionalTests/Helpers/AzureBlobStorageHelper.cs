using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Options;

namespace UKHO.ERPFacade.API.FunctionalTests.Helpers
{
    public class AzureBlobStorageHelper
    {
        public string DownloadJSONFromAzureBlob(string expectedfilePath, string containerAndBlobName, string fileType)
        {

            //BlobServiceClient blobServiceClient = new BlobServiceClient(_azureStorageConfig.Value.ConnectionString);
            var ConnectionString = "DefaultEndpointsProtocol=https;AccountName=storageerptest;AccountKey=UX0ZtBf6bM7CSE0/xG0RXKybWAozRbLMftEji3fMNrBuWWF9Xgq7Kki5qAwLzFYhtTdiH5+GKun8+ASt4tF/zQ==;EndpointSuffix=core.windows.net";
            BlobServiceClient blobServiceClient = new BlobServiceClient(ConnectionString);
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerAndBlobName);
            BlobClient blobClient = containerClient.GetBlobClient(containerAndBlobName + "."+fileType);

            BlobDownloadInfo blobDownload = blobClient.Download();
            using (FileStream downloadFileStream = new FileStream((expectedfilePath + "\\" + containerAndBlobName + "." + fileType), FileMode.Create))
            {
                blobDownload.Content.CopyTo(downloadFileStream);
            }
            
            return (expectedfilePath + "\\" + containerAndBlobName + "." + fileType);
        }   
    }
}
