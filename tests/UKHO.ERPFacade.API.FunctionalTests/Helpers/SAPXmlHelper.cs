using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UKHO.ERPFacade.API.FunctionalTests.Helpers
{
    public class SAPXmlHelper
    {
        private JsonPayloadHelper jsonPayloadHelper { get; set; }
        private string _storageAccount_connectionString = "";
        static string _path = @"projectDirectoryaddresslogic\ERPFacadeGeneratedXmlFiles\";
        
        public string downloadGeneratedXML(string containerAndBlobName)
        {
            BlobServiceClient blobServiceClient = new BlobServiceClient(_storageAccount_connectionString);
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerAndBlobName);
            BlobClient blobClient = containerClient.GetBlobClient(containerAndBlobName+".xml");

            BlobDownloadInfo blobDownload = blobClient.Download();
            using (FileStream downloadFileStream = new FileStream((_path+containerAndBlobName+".xml"), FileMode.Create))
            {
                blobDownload.Content.CopyTo(downloadFileStream);
            }

            return (_path + containerAndBlobName + ".xml"); 
        }

        public string getTraceID(string jsonFilePath)
        {
            
            using (StreamReader r = new StreamReader(jsonFilePath))
            {
                string jsonOutput = r.ReadToEnd();
                jsonPayloadHelper = JsonConvert.DeserializeObject<JsonPayloadHelper>(jsonOutput);
            }
            
            return jsonPayloadHelper.Id;
        }
    }
}
