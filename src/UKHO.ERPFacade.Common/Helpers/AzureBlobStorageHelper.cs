using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using UKHO.ERPFacade.Common.Configuration;
using UKHO.ERPFacade.Common.Logging;

namespace UKHO.ERPFacade.Common.Helpers
{
    [ExcludeFromCodeCoverage]
    public class AzureBlobStorageHelper : IAzureBlobStorageHelper
    {
        private readonly ILogger<AzureBlobStorageHelper> _logger;
        private readonly IOptions<AzureStorageConfiguration> _azureStorageConfig;

        public AzureBlobStorageHelper(ILogger<AzureBlobStorageHelper> logger,
                                        IOptions<AzureStorageConfiguration> azureStorageConfig)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _azureStorageConfig = azureStorageConfig;
        }

        public async Task UploadEvent(JObject eesEvent, string traceId, string correlationId)
        {
            BlobClient blobClient = GetBlobClient(traceId);

            Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(eesEvent.ToString() ?? ""));

            await blobClient.UploadAsync(stream, overwrite: true);
            _logger.LogInformation(EventIds.UploadedEncContentPublishedEventInAzureBlob.ToEventId(), "Uploaded ENC content published event in Azure Blob storage successfully. | _X-Correlation-ID : {CorrelationId}", correlationId);
        }

        //Private Methods
        private BlobClient GetBlobClient(string containerName)
        {
            BlobContainerClient blobContainerClient = new BlobContainerClient(_azureStorageConfig.Value.ConnectionString, containerName);
            blobContainerClient.CreateIfNotExists();
             
            var blobName = containerName + ".json";
           
            BlobClient blobClient = blobContainerClient.GetBlobClient(blobName);
            return blobClient;
        }
    }
}
