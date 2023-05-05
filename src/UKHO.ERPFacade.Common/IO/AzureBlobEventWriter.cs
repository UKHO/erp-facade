using Azure.Storage.Blobs;
using Microsoft.Extensions.Options;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using UKHO.ERPFacade.Common.Configuration;

namespace UKHO.ERPFacade.Common.IO
{
    [ExcludeFromCodeCoverage]
    public class AzureBlobEventWriter : IAzureBlobEventWriter
    {
        private readonly IOptions<AzureStorageConfiguration> _azureStorageConfig;

        public AzureBlobEventWriter(IOptions<AzureStorageConfiguration> azureStorageConfig)
        {
            _azureStorageConfig = azureStorageConfig ?? throw new ArgumentNullException(nameof(azureStorageConfig));
        }

        public async Task UploadEvent(string requestEvent, string blobContainerName, string blobName)
        {
            BlobClient blobClient = GetBlobClient(blobContainerName, blobName);

            Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(requestEvent ?? ""));

            await blobClient.UploadAsync(stream, overwrite: true);
        }

        //Private Methods
        private BlobClient GetBlobClient(string containerName, string blobName)
        {
            BlobContainerClient blobContainerClient = new(_azureStorageConfig.Value.ConnectionString, containerName);
            blobContainerClient.CreateIfNotExists();

            BlobClient blobClient = blobContainerClient.GetBlobClient(blobName);
            return blobClient;
        }

        public bool CheckIfContainerExists(string containerName)
        {
            BlobServiceClient serviceClient = new(_azureStorageConfig.Value.ConnectionString);

            var container = serviceClient.GetBlobContainerClient(containerName);
            var isExists = container.Exists();

            return isExists;
        }
    }
}