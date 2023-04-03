using Azure;
using Azure.Data.Tables;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;
using System.Data.Common;
using System.IO.Compression;
using System.Text;
using System.Text.Json.Serialization;

namespace UKHO.ERPFacade.API.StorageAccount
{
    public class StoreEssEventInBlob
    {
        public async Task StoreEventDataInBlob(string inputJson)
        {
            //Deserialize the Event data and get the traceID
            ESSEventData eSSEventData = JsonConvert.DeserializeObject<ESSEventData>(inputJson);
            string traceId = eSSEventData.data.traceId;

            //extract traceid from the eventdata and keep it as container name
            string containerName = traceId;

            //craete a blob name using traceid in json format
            string fileName = traceId + ".json";

            //get Storageaccount connection string from appsettings.json
            string connectionString = Environment.GetEnvironmentVariable("StorageConnectionString");

            //upload the event data in blob
            BlobContainerClient bcc = new BlobContainerClient(connectionString, containerName);
            bcc.CreateIfNotExists();
            BlobClient bc = bcc.GetBlobClient(fileName);
            
            Stream streamForm = new MemoryStream(Encoding.UTF8.GetBytes(inputJson ?? ""));
            await bc.UploadAsync(streamForm, overwrite: true);

            //upload the event data in table
            await StoreEventDataInTable(inputJson, traceId, connectionString);
        }

        public async Task StoreEventDataInTable(string inputJson, string traceId, string connectionString)
        {
            var guid = Guid.NewGuid();
            var guid1 = Guid.NewGuid();

            TableServiceClient tsc = new TableServiceClient(connectionString);

            //Create table in Azure storage
            TableClient tc = tsc.GetTableClient(tableName: "ESSEvents");
            tc.CreateIfNotExists();

            //read a item from table
            var entityData = tc.Query<TableColumns>(x => x.TraceID == traceId).Count<TableColumns>();

            //Check for duplicate entities
             
            if (entityData == 0) //entity does not exist
            {
                //Upload data in table
                TableColumns tableColumns = new TableColumns();
                tableColumns.PartitionKey = guid.ToString();
                tableColumns.RowKey = guid1.ToString();
                tableColumns.Timestamp = DateTime.UtcNow;
                tableColumns.TraceID = traceId;
                tableColumns.EventData = inputJson;
                tableColumns.RequestDateTime = null;
                tableColumns.ResponseDateTime = null;

                await tc.AddEntityAsync<TableColumns>(tableColumns);
            }
        }

        //Add a class to create custom columns in table
        public record TableColumns : ITableEntity
        {
            public string RowKey { get; set; } = default!;

            public string PartitionKey { get; set; } = default!;

            public DateTimeOffset? Timestamp { get; set; } = default!;
            
            public string TraceID { get; set; }

            public string EventData { get; set; }

            public DateTime? RequestDateTime { get; set; }

            public DateTime? ResponseDateTime { get; set; }

            public ETag ETag { get; set; } = default!;

        }
    }
}
