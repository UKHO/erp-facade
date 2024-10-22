using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Options;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using UKHO.ERPFacade.Common.Configuration;
using UKHO.ERPFacade.Common.Logging;

namespace UKHO.ERPFacade.Common.IO.Azure
{
    [ExcludeFromCodeCoverage]
    public class AzureBlobHelper : IAzureBlobHelper
    {
        private readonly IOptions<AzureStorageConfiguration> _azureStorageConfig;

        public AzureBlobHelper(IOptions<AzureStorageConfiguration> azureStorageConfig)
        {
            _azureStorageConfig = azureStorageConfig ?? throw new ArgumentNullException(nameof(azureStorageConfig));
        }

        public async Task UploadEvent(string requestEvent, string blobContainerName, string blobName)
        {
            //_logger.LogInformation(EventIds.UploadEncContentPublishedEventInAzureBlobStarted.ToEventId(), "Uploading enccontentpublished event payload in blob storage.");

            BlobClient blobClient = GetBlobClient(blobContainerName, blobName);

            Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(requestEvent ?? ""));

            await blobClient.UploadAsync(stream, overwrite: true);

            //_logger.LogInformation(EventIds.UploadEncContentPublishedEventInAzureBlobCompleted.ToEventId(), "The enccontentpublished event payload is uploaded in blob storage successfully.");
        }

        public string DownloadEvent(string blobName, string blobContainerName)
        {
            BlobContainerClient blobContainerClient = new(_azureStorageConfig.Value.ConnectionString, blobContainerName);
            BlobClient blobClient = blobContainerClient.GetBlobClient(blobName);

            BlobDownloadResult downloadResult = blobClient.DownloadContent();
            string existingEesEvent = downloadResult.Content.ToString();

            return existingEesEvent;
        }

        public DateTime GetBlobCreateDate(string blobName, string blobContainerName)
        {
            BlobContainerClient blobContainerClient = new(_azureStorageConfig.Value.ConnectionString, blobContainerName);
            BlobClient blobClient = blobContainerClient.GetBlobClient(blobName);
            return blobClient.GetProperties().Value.CreatedOn.DateTime;
        }

        public IEnumerable<string> GetBlobsInContainer(string blobContainerName, string corrId)
        {
            BlobContainerClient blobContainerClient = new(_azureStorageConfig.Value.ConnectionString, blobContainerName);
            var blobs = blobContainerClient.GetBlobs(BlobTraits.None, BlobStates.None, corrId);
            foreach (BlobItem blob in blobs)
            {
                yield return blob.Name;
            }
        }

        public bool DeleteBlob(string blobName, string blobContainerName)
        {
            BlobContainerClient blobContainerClient = new(_azureStorageConfig.Value.ConnectionString, blobContainerName);
            BlobClient blobClient = blobContainerClient.GetBlobClient(blobName);
            return blobClient.DeleteIfExists();
        }

        public bool DeleteContainer(string blobContainerName)
        {
            BlobContainerClient blobContainerClient = new(_azureStorageConfig.Value.ConnectionString, blobContainerName);
            return blobContainerClient.DeleteIfExists();
        }

        public bool DeleteDirectory(string blobContainerName, string directoryName)
        {
            BlobContainerClient blobContainerClient = new(_azureStorageConfig.Value.ConnectionString, blobContainerName);

            // List all blobs with the directory prefix
            foreach (var blobItem in blobContainerClient.GetBlobs(prefix: directoryName))
            {
                // Get the BlobClient for each blob and delete it
                BlobClient blobClient = blobContainerClient.GetBlobClient(blobItem.Name);
                Console.WriteLine($"Deleting blob: {blobItem.Name}");
                blobClient.DeleteIfExists();
            }
            return true;
        }

        public List<string> GetBlobNamesInFolder(string blobContainerName, string corrId)
        {
            List<string> blobList = new();
            BlobContainerClient blobContainerClient = new(_azureStorageConfig.Value.ConnectionString, blobContainerName);

            var blobs = blobContainerClient.GetBlobs(BlobTraits.None, BlobStates.None, corrId);

            foreach (BlobItem blob in blobs)
            {
                var blobName = blob.Name.Split("/");
                var fileName = blobName[1].Split(".");
                blobList.Add(fileName[0]);
            }

            return blobList;
        }

        //Private Methods
        private BlobClient GetBlobClient(string containerName, string blobName)
        {
            BlobContainerClient blobContainerClient = new(_azureStorageConfig.Value.ConnectionString, containerName.ToLower());
            blobContainerClient.CreateIfNotExists();

            BlobClient blobClient = blobContainerClient.GetBlobClient(blobName);
            return blobClient;
        }

        public bool CheckIfContainerExists(string containerName)
        {
            BlobServiceClient serviceClient = new(_azureStorageConfig.Value.ConnectionString);

            var container = serviceClient.GetBlobContainerClient(containerName.ToLower());
            var isExists = container.Exists();

            return isExists;
        }
    }
}
