using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Options;

namespace UKHO.ERPFacade.API.FunctionalTests.Helpers
{
    public class AzureBlobStorageHelper
    {
        public string DownloadJSONFromAzureBlob(string expectedfilePath, string containerAndBlobName, string fileType)
        {
            try
            {
                BlobServiceClient blobServiceClient = new BlobServiceClient(Config.TestConfig.AzureStorageConfiguration.ConnectionString);
                BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerAndBlobName);
                BlobClient blobClient = containerClient.GetBlobClient(containerAndBlobName+"_unitofsalesupdatedevent" + "." + fileType);

                BlobDownloadInfo blobDownload = blobClient.Download();
                using (FileStream downloadFileStream = new FileStream((expectedfilePath + "\\" + containerAndBlobName + "_unitofsalesupdatedevent" + "." + fileType), FileMode.Create))
                {
                    blobDownload.Content.CopyTo(downloadFileStream);
                }

                return (expectedfilePath + "\\" + containerAndBlobName + "_unitofsalesupdatedevent" + "." + fileType);
            }
            catch (Exception)
            {
                throw;
            }
        }   
    }
}
