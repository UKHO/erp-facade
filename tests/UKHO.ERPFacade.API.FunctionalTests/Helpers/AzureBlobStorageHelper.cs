using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;


namespace UKHO.ERPFacade.API.FunctionalTests.Helpers
{
    public class AzureBlobStorageHelper
    {

        public string DownloadJSONFromAzureBlob(string expectedJSONfilePath, string containerAndBlobName)
        {

            try
            {
                BlobServiceClient blobServiceClient = new BlobServiceClient(Config.TestConfig.AzureStorageConfiguration.ConnectionString);
                BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerAndBlobName);
                BlobClient blobClient = containerClient.GetBlobClient(containerAndBlobName + ".JSON");

                BlobDownloadInfo blobDownload = blobClient.Download();
                using (FileStream downloadFileStream = new FileStream((expectedJSONfilePath + "\\" + containerAndBlobName + ".JSON"), FileMode.Create))
                {
                    blobDownload.Content.CopyTo(downloadFileStream);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(containerAndBlobName + " " + ex.Message);
            }
            return (expectedJSONfilePath + "\\" + containerAndBlobName + ".JSON");
        }
        public string DownloadJSONFromAzureBlob(string expectedJSONfilePath, string containerAndBlobName, string productName, string endPoint)
        {
            string fileName = "";
            try
            {
                if (endPoint == "ProductChange")
                {
                    BlobServiceClient blobServiceClient = new BlobServiceClient(Config.TestConfig.AzureStorageConfiguration.ConnectionString);

                    BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient("pricechangeblobs" + "\\" + containerAndBlobName + "\\" + productName);
                    BlobClient blobClient = containerClient.GetBlobClient(containerAndBlobName + "/" + productName + "/" + productName + ".json");


                    BlobDownloadInfo blobDownload = blobClient.Download();
                    fileName = expectedJSONfilePath + "\\" + containerAndBlobName + "\\" + productName + ".JSON";
                    Directory.CreateDirectory(Path.GetDirectoryName(fileName));
                    using (FileStream downloadFileStream = new FileStream(fileName, FileMode.Create))
                    {
                        blobDownload.Content.CopyTo(downloadFileStream);
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(containerAndBlobName + " " + ex.Message);
            }
            return fileName;
        }
        public async Task<List<string>> GetProductListFromBlobContainerAsync(string containerAndBlobName)
        {
            List<string> directoryNames = new List<string>();
            try
            {
                BlobServiceClient blobServiceClient = new BlobServiceClient(Config.TestConfig.AzureStorageConfiguration.ConnectionString);
                BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient("pricechangeblobs");

                // List all the directories
                await foreach (BlobHierarchyItem blobHierarchyItem in containerClient.GetBlobsByHierarchyAsync(prefix: containerAndBlobName + "/", delimiter: "/"))
                {
                    if (blobHierarchyItem.IsPrefix)
                    {                        
                        string str = blobHierarchyItem.Prefix;
                        var start = str.IndexOf("/");
                        var end = str.LastIndexOf("/");
                        var length = end - start;
                        var productName=str.Substring(start+1, length-1);
                        directoryNames.Add(productName);
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(containerAndBlobName + " " + ex.Message);
            }
            return directoryNames;
        }
    }
}
