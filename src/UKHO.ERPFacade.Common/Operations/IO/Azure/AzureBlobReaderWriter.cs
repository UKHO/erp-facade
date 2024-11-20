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

            BlobDownloadResult downloadResult = await blobClient.DownloadContentAsync();
            string existingEesEvent = downloadResult.Content.ToString();

            return existingEesEvent;
        }

        public async Task<bool> DeleteContainerAsync(string blobContainerName)
        {
            BlobContainerClient blobContainerClient = new(_azureStorageConfig.Value.ConnectionString, blobContainerName);
            return await blobContainerClient.DeleteIfExistsAsync();
        }

        public async Task<List<string>> GetBlobNamesInFolderAsync(string blobContainerName, string corrId)
        {
            List<string> blobList = new List<string>();
            BlobContainerClient blobContainerClient = new BlobContainerClient(_azureStorageConfig.Value.ConnectionString, blobContainerName);

            await foreach (BlobItem blob in blobContainerClient.GetBlobsAsync(prefix: corrId))
            {
                string fileName = Path.GetFileNameWithoutExtension(blob.Name);
                blobList.Add(fileName);
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
    }
}
