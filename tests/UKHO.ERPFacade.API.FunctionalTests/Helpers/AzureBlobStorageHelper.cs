using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using UKHO.ERPFacade.API.FunctionalTests.Configuration;

namespace UKHO.ERPFacade.API.FunctionalTests.Helpers
{
    public class AzureBlobStorageHelper
    {
        

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
                if (blobItem.Name.Substring(0, 36) == subContainerName)
                {
                    return true;
                }
            }

            return false;
        }

        public List<string> GetBlobNamesInFolder(string blobContainerName, string corrId)
        {
            List<string> blobList = new();
            BlobContainerClient blobContainerClient = new(Config.TestConfig.AzureStorageConfiguration.ConnectionString, blobContainerName);

            var blobs = blobContainerClient.GetBlobs(BlobTraits.None, BlobStates.None, corrId);

            foreach (BlobItem blob in blobs)
            {
                var blobName = blob.Name.Split("/");
                var fileName = blobName[1].Split(".");
                blobList.Add(fileName[0]);
            }

            return blobList;
        }

        public string DownloadGeneratedXMLFile(string expectedXMLfilePath, string blobContainer, string parentContainerName)
        {
            string fileName = "";
            string licenceUpdatedXMLFile = "SapXmlPayload";
            BlobServiceClient blobServiceClient = new BlobServiceClient(Config.TestConfig.AzureStorageConfiguration.ConnectionString);

            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(parentContainerName + "\\" + blobContainer);
            BlobClient blobClient = containerClient.GetBlobClient(blobContainer + "/" + licenceUpdatedXMLFile + ".xml");
            try
            {
                BlobDownloadInfo blobDownload = blobClient.Download();
                fileName = expectedXMLfilePath + "\\" + blobContainer + "\\" + licenceUpdatedXMLFile + ".xml";
                Directory.CreateDirectory(Path.GetDirectoryName(fileName));
                using (FileStream downloadFileStream = new(fileName, FileMode.Create))
                {
                    blobDownload.Content.CopyTo(downloadFileStream);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(blobContainer + " " + ex.Message);
            }
            return fileName;
        }
    }
}
