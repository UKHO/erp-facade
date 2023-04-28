using System.Diagnostics.CodeAnalysis;
using System.Text;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Options;
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

        public async Task UploadEvent(object requestObject, string requestFormat, string traceId)
        {
            BlobClient blobClient = GetBlobClient(traceId, requestFormat);

            Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(requestObject.ToString() ?? ""));

            await blobClient.UploadAsync(stream, overwrite: true);
        }

        //Private Methods
        private BlobClient GetBlobClient(string containerName, string requestFormat)
        {
            BlobContainerClient blobContainerClient = new(_azureStorageConfig.Value.ConnectionString, containerName);
            blobContainerClient.CreateIfNotExists();

            var blobName = containerName + '.' + requestFormat;

            BlobClient blobClient = blobContainerClient.GetBlobClient(blobName);
            return blobClient;
        }
    }
}