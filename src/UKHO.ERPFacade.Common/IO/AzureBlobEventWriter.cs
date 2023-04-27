using Azure.Storage.Blobs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using UKHO.ERPFacade.Common.Configuration;
using UKHO.ERPFacade.Common.Logging;

namespace UKHO.ERPFacade.Common.IO
{
    [ExcludeFromCodeCoverage]
    public class AzureBlobEventWriter : IAzureBlobEventWriter
    {
        private readonly ILogger<AzureBlobEventWriter> _logger;
        private readonly IOptions<AzureStorageConfiguration> _azureStorageConfig;

        public AzureBlobEventWriter(ILogger<AzureBlobEventWriter> logger,
                                        IOptions<AzureStorageConfiguration> azureStorageConfig)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _azureStorageConfig = azureStorageConfig ?? throw new ArgumentNullException(nameof(azureStorageConfig));
        }

        public async Task UploadEvent(JObject eesEvent, string traceId)
        {
            BlobClient blobClient = GetBlobClient(traceId, ".json");

            Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(eesEvent.ToString() ?? ""));

            await blobClient.UploadAsync(stream, overwrite: true);

            _logger.LogInformation(EventIds.UploadedEncContentPublishedEventInAzureBlob.ToEventId(), "ENC content published event is uploaded in blob storage successfully.");
        }

        public async Task UploadXMLEvent(string xml, string traceId)
        {
            BlobClient blobClient = GetBlobClient(traceId, ".xml");

            Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(xml ?? ""));

            await blobClient.UploadAsync(stream, overwrite: true);
        }

        //Private Methods
        private BlobClient GetBlobClient(string containerName, string fileExtension)
        {
            BlobContainerClient blobContainerClient = new(_azureStorageConfig.Value.ConnectionString, containerName);
            blobContainerClient.CreateIfNotExists();

            var blobName = containerName + fileExtension;

            BlobClient blobClient = blobContainerClient.GetBlobClient(blobName);
            return blobClient;
        }
    }
}