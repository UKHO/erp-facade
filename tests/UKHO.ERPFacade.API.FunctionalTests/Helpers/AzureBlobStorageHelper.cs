using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using UKHO.ERPFacade.API.FunctionalTests.Configuration;

namespace UKHO.ERPFacade.API.FunctionalTests.Helpers
{
    public class AzureBlobStorageHelper
    {
        public string DownloadJsonFromAzureBlob(string expectedfilePath, string containerAndBlobName, string fileType)
        {
            try
            {
                BlobServiceClient blobServiceClient = new(Config.TestConfig.AzureStorageConfiguration.ConnectionString);
                BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerAndBlobName);
                BlobClient blobClient = containerClient.GetBlobClient("UnitOfSaleUpdatedEvent" + "." + fileType);

                BlobDownloadInfo blobDownload = blobClient.Download();
                using (FileStream downloadFileStream = new((expectedfilePath + "\\" + "UnitOfSaleUpdatedEvent" + "." + fileType), FileMode.Create))
                {
                    blobDownload.Content.CopyTo(downloadFileStream);
                }

                return (expectedfilePath + "\\" + "UnitOfSaleUpdatedEvent" + "." + fileType);
            }
            catch (Exception ex)
            {
                Console.WriteLine(containerAndBlobName + " " + ex.Message);
            }
            return (expectedfilePath + "\\" + containerAndBlobName + ".JSON");
        }

        public static string DownloadJsonFromAzureBlob(string expectedJSONfilePath, string blobContainer, string productName, string endPoint)
        {
            string fileName = "";
            try
            {
                if (endPoint == "ProductChange")
                {
                    BlobServiceClient blobServiceClient = new BlobServiceClient(Config.TestConfig.AzureStorageConfiguration.ConnectionString);

                    BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient("pricechangeblobs" + "\\" + blobContainer + "\\" + productName);
                    BlobClient blobClient = containerClient.GetBlobClient(blobContainer + "/" + productName + "/" + "PriceChangeEvent" + ".json");

                    BlobDownloadInfo blobDownload = blobClient.Download();
                    fileName = expectedJSONfilePath + "\\" + blobContainer + "\\" + productName + ".JSON";
                    Directory.CreateDirectory(Path.GetDirectoryName(fileName));
                    using (FileStream downloadFileStream = new(fileName, FileMode.Create))
                    {
                        blobDownload.Content.CopyTo(downloadFileStream);
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(blobContainer + " " + ex.Message);
            }
            return fileName;
        }

        public async Task<List<string>> GetProductListFromBlobContainerAsync(string blobContainer)
        {
            List<string> directoryNames = new();
            try
            {
                BlobServiceClient blobServiceClient = new(Config.TestConfig.AzureStorageConfiguration.ConnectionString);
                BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient("pricechangeblobs");

                // List all the directories
                await foreach (BlobHierarchyItem blobHierarchyItem in containerClient.GetBlobsByHierarchyAsync(prefix: blobContainer + "/", delimiter: "/"))
                {
                    if (blobHierarchyItem.IsPrefix)
                    {
                        string str = blobHierarchyItem.Prefix;
                        var start = str.IndexOf("/");
                        var end = str.LastIndexOf("/");
                        var length = end - start;
                        var productName = str.Substring(start + 1, length - 1);
                        directoryNames.Add(productName);
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(blobContainer + " " + ex.Message);
            }
            return directoryNames;
        }

        public string DownloadGeneratedXML(string expectedXMLfilePath, string blobContainer)
        {
            BlobServiceClient blobServiceClient = new BlobServiceClient(Config.TestConfig.AzureStorageConfiguration.ConnectionString);
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(blobContainer);
            BlobClient blobClient = containerClient.GetBlobClient("SapXmlPayload.xml");
            try
            {
                BlobDownloadInfo blobDownload = blobClient.Download();
                using (FileStream downloadFileStream = new((expectedXMLfilePath + "\\" + blobContainer + ".xml"), FileMode.Create))
                {
                    blobDownload.Content.CopyTo(downloadFileStream);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(blobContainer + " " + ex.Message);
            }
            return (expectedXMLfilePath + "\\" + blobContainer + ".xml");
        }

        public bool VerifyBlobExists(string parentContainerName, string subContainerName)
        {
            BlobServiceClient blobServiceClient = new BlobServiceClient(Config.TestConfig.AzureStorageConfiguration.ConnectionString);
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(parentContainerName);
            BlobClient blobClient = containerClient.GetBlobClient(subContainerName);

            foreach (BlobItem blobItem in containerClient.GetBlobs())
            {
                if (blobItem.Name.Substring(0,36) == subContainerName)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
