using System.Diagnostics.CodeAnalysis;
using System.Text;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Options;
using UKHO.ERPFacade.Common.Configuration;

namespace UKHO.ERPFacade.Common.Operations.IO.Azure
{
    [ExcludeFromCodeCoverage]
    public class AzureBlobReaderWriter : IAzureBlobReaderWriter
    {
        private readonly IOptions<AzureStorageConfiguration> _azureStorageConfig;

        public AzureBlobReaderWriter(IOptions<AzureStorageConfiguration> azureStorageConfig)
        {
            _azureStorageConfig = azureStorageConfig ?? throw new ArgumentNullException(nameof(azureStorageConfig));
        }

        public async Task UploadEventAsync(string requestEvent, string blobContainerName, string blobName)
        {
            BlobClient blobClient = GetBlobClient(blobContainerName, blobName);

            Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(requestEvent ?? ""));

            await blobClient.UploadAsync(stream, overwrite: true);
        }

        public async Task<string> DownloadEventAsync(string blobName, string blobContainerName)
        {
            BlobContainerClient blobContainerClient = new(_azureStorageConfig.Value.ConnectionString, blobContainerName);
            BlobClient blobClient = blobContainerClient.GetBlobClient(blobName);
            BlobDownloadResult blobDownloadResult = await blobClient.DownloadContentAsync();
            return blobDownloadResult.Content.ToString();
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
