using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using UKHO.ERPFacade.API.FunctionalTests.Configuration;
using UKHO.ERPFacade.Common.Constants;

namespace UKHO.ERPFacade.API.FunctionalTests.Helpers
{
    public class AzureBlobStorageHelper
    {
        public bool VerifyBlobExists(string parentContainerName, string subContainerName)
        {
            BlobServiceClient blobServiceClient = new BlobServiceClient(Config.TestConfig.AzureStorageConfiguration.ConnectionString);
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(parentContainerName);
            return containerClient.GetBlobs().Any(blobItem => blobItem.Name.Substring(0, 36) == subContainerName);
        }

        public List<string> GetBlobNamesInFolder(string blobContainerName, string corrId)
        {
            BlobContainerClient blobContainerClient = new(Config.TestConfig.AzureStorageConfiguration.ConnectionString, blobContainerName);

            var blobs = blobContainerClient.GetBlobs(BlobTraits.None, BlobStates.None, corrId);

            return (from blob in blobs select blob.Name.Split("/") into blobName select blobName[1].Split(".") into fileName select fileName[0]).ToList();
        }

        public string DownloadGeneratedXml(string expectedXmLfilePath, string blobContainer)
        {
            BlobServiceClient blobServiceClient = new(Config.TestConfig.AzureStorageConfiguration.ConnectionString);
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(blobContainer);
            BlobClient blobClient = containerClient.GetBlobClient(Constants.SapXmlPayloadFileName);
            try
            {
                BlobDownloadInfo blobDownload = blobClient.Download();
                using FileStream downloadFileStream = new((expectedXmLfilePath + "\\" + blobContainer + ".xml"), FileMode.Create);
                blobDownload.Content.CopyTo(downloadFileStream);
            }
            catch (Exception ex)
            {
                Console.WriteLine(blobContainer + " " + ex.Message);
            }
            return (expectedXmLfilePath + "\\" + blobContainer + ".xml");
        }

        public string DownloadGeneratedXmlFile(string expectedXmlFilePath, string blobContainer, string parentContainerName)
        {
            string fileName = "";
            BlobServiceClient blobServiceClient = new BlobServiceClient(Config.TestConfig.AzureStorageConfiguration.ConnectionString);

            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(parentContainerName + "\\" + blobContainer);
            BlobClient blobClient = containerClient.GetBlobClient(blobContainer + "/" + Constants.SapXmlPayloadFileName);
            try
            {
                BlobDownloadInfo blobDownload = blobClient.Download();
                fileName = expectedXmlFilePath + "\\" + blobContainer + "\\" + Constants.SapXmlPayloadFileName;
                Directory.CreateDirectory(Path.GetDirectoryName(fileName));
                using FileStream downloadFileStream = new(fileName, FileMode.Create);
                blobDownload.Content.CopyTo(downloadFileStream);
            }
            catch (Exception ex)
            {
                Console.WriteLine(blobContainer + " " + ex.Message);
            }
            return fileName;
        }
    }
}
