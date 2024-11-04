using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using UKHO.ERPFacade.API.FunctionalTests.Configuration;
using UKHO.ERPFacade.Common.Constants;

namespace UKHO.ERPFacade.API.FunctionalTests.Operations
{
    public class AzureBlobReaderWriter : TestFixtureBase
    {
        private readonly AzureStorageConfiguration _azureStorageConfiguration;

        public AzureBlobReaderWriter()
        {
            var serviceProvider = GetServiceProvider();
            _azureStorageConfiguration = serviceProvider!.GetRequiredService<IOptions<AzureStorageConfiguration>>().Value;
        }

        public bool VerifyBlobExists(string parentContainerName, string subContainerName)
        {
            BlobServiceClient blobServiceClient = new BlobServiceClient(_azureStorageConfiguration.ConnectionString);
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(parentContainerName);
            return containerClient.GetBlobs().Any(blobItem => blobItem.Name.Contains(subContainerName));
        }

        public List<string> GetBlobNamesInFolder(string blobContainerName, string corrId)
        {
            BlobContainerClient blobContainerClient = new(_azureStorageConfiguration.ConnectionString, blobContainerName);
            var blobs = blobContainerClient.GetBlobs(BlobTraits.None, BlobStates.None, corrId);
            return (from blob in blobs select blob.Name.Split("/") into blobName select blobName[1].Split(".") into fileName select fileName[0]).ToList();
        }

        public string DownloadContainerFile(string expectedfilePath, string containerName, string fileExtenstion)
        {
            BlobServiceClient blobServiceClient = new(_azureStorageConfiguration.ConnectionString);
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);
            BlobClient blobClient = containerClient.GetBlobClient(EventPayloadFiles.SapXmlPayloadFileName);

            try
            {
                BlobDownloadInfo blobDownload = blobClient.Download();
                using FileStream downloadFileStream = new(expectedfilePath + "\\" + containerName + fileExtenstion, FileMode.Create);
                blobDownload.Content.CopyTo(downloadFileStream);
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                Console.WriteLine(containerName + " " + ex.Message);
                return string.Empty;
            }
            return expectedfilePath + "\\" + containerName + ".xml";
        }

        public string DownloadDirectoryFile(string expectedfilePath, string containerName, string parentContainerName)
        {
            string fileName = "";
            BlobServiceClient blobServiceClient = new BlobServiceClient(_azureStorageConfiguration.ConnectionString);

            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(parentContainerName + "\\" + containerName);
            BlobClient blobClient = containerClient.GetBlobClient(containerName + "/" + EventPayloadFiles.SapXmlPayloadFileName);
            try
            {
                BlobDownloadInfo blobDownload = blobClient.Download();
                fileName = expectedfilePath + "\\" + containerName + "\\" + EventPayloadFiles.SapXmlPayloadFileName;
                Directory.CreateDirectory(Path.GetDirectoryName(fileName));
                using FileStream downloadFileStream = new(fileName, FileMode.Create);
                blobDownload.Content.CopyTo(downloadFileStream);
            }
            catch (Exception ex)
            {
                Console.WriteLine(containerName + " " + ex.Message);
            }
            return fileName;
        }
    }
}
