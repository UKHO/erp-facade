﻿using Azure.Storage.Blobs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System.Diagnostics.CodeAnalysis;
using System.Text;
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
            _azureStorageConfig = azureStorageConfig ?? throw new ArgumentNullException(nameof(azureStorageConfig));
        }

        public async Task UploadEvent(JObject eesEvent, string traceId, string correlationId)
        {
            BlobClient blobClient = GetBlobClient(traceId);

            Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(eesEvent.ToString() ?? ""));

            await blobClient.UploadAsync(stream, overwrite: true);

            _logger.LogInformation(EventIds.UploadedEncContentPublishedEventInAzureBlob.ToEventId(), "ENC content published event is uploaded in blob storage successfully. | _X-Correlation-ID : {CorrelationId}", correlationId);
        }

        //Private Methods
        private BlobClient GetBlobClient(string containerName)
        {
            BlobContainerClient blobContainerClient = new (_azureStorageConfig.Value.ConnectionString, containerName);
            blobContainerClient.CreateIfNotExists();

            var blobName = containerName + ".json";

            BlobClient blobClient = blobContainerClient.GetBlobClient(blobName);
            return blobClient;
        }
    }
}
