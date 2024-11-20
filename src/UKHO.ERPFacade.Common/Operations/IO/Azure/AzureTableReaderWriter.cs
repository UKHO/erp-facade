using System.Diagnostics.CodeAnalysis;
using Azure;
using Azure.Data.Tables;
using Azure.Data.Tables.Models;
using Microsoft.Extensions.Options;
using UKHO.ERPFacade.Common.Configuration;
using UKHO.ERPFacade.Common.Constants;

namespace UKHO.ERPFacade.Common.Operations.IO.Azure
{
    [ExcludeFromCodeCoverage]
    public class AzureTableReaderWriter : IAzureTableReaderWriter
    {

        private readonly IOptions<AzureStorageConfiguration> _azureStorageConfig;

        public AzureTableReaderWriter(IOptions<AzureStorageConfiguration> azureStorageConfig)
        {
            _azureStorageConfig = azureStorageConfig ?? throw new ArgumentNullException(nameof(azureStorageConfig));
        }

        public async Task UpsertEntityAsync(ITableEntity entity)
        {
            TableClient tableClient = await GetTableClientAsync(AzureStorage.EventTableName);

            TableEntity existingEntity = await GetEntityAsync(entity.PartitionKey, entity.RowKey);

            if (existingEntity == null!)
            {
                await tableClient.AddEntityAsync(entity, CancellationToken.None);
            }
            else
            {
                existingEntity.Timestamp = DateTime.UtcNow;

                await tableClient.UpdateEntityAsync(existingEntity, ETag.All, TableUpdateMode.Replace);
            }
        }

        public async Task<TableEntity> GetEntityAsync(string partitionKey, string rowKey)
        {
            TableClient tableClient = await GetTableClientAsync(AzureStorage.EventTableName);
            try
            {
                return await tableClient.GetEntityAsync<TableEntity>(partitionKey, rowKey);
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                return null;
            }
        }

        public async Task UpdateEntityAsync<TKey, TValue>(string partitionKey, string rowKey, KeyValuePair<TKey, TValue>[] entitiesToUpdate)
        {
            TableClient tableClient = await GetTableClientAsync(AzureStorage.EventTableName);
            TableEntity existingEntity = await GetEntityAsync(partitionKey, rowKey);
            if (existingEntity != null)
            {
                foreach (var entity in entitiesToUpdate)
                {
                    existingEntity[entity.Key.ToString()] = entity.Value;
                }
                await tableClient.UpdateEntityAsync(existingEntity, ETag.All, TableUpdateMode.Replace);
            }
        }

        public async Task<IList<TableEntity>> GetEntitiesByQueryParameterAsync<TKey, TValue>(KeyValuePair<TKey, TValue> parameter)
        {
            var records = new List<TableEntity>();
            TableClient tableClient = await GetTableClientAsync(AzureStorage.EventTableName);
            var entities = tableClient.Query<TableEntity>(filter: $"{parameter.Key} eq '{parameter.Value}'");
            foreach (var entity in entities)
            {
                records.Add(entity);
            }
            return records;
        }

        public async Task DeleteEntityAsync(string partitionKey, string rowKey)
        {
            TableClient tableClient = await GetTableClientAsync(AzureStorage.EventTableName);
            TableEntity existingEntity = await GetEntityAsync(partitionKey, rowKey);
            if (existingEntity != null)
            {
                await tableClient.DeleteEntityAsync(existingEntity.PartitionKey, existingEntity.RowKey);
            }
        }

        //Private Methods
        private async Task<TableClient> GetTableClientAsync(string tableName)
        {
            TableServiceClient serviceClient = new(_azureStorageConfig.Value.ConnectionString);
            Pageable<TableItem> queryTableResults = serviceClient.Query(filter: $"TableName eq '{tableName}'");
            var tableExists = queryTableResults.FirstOrDefault(t => t.Name == tableName);

            if (tableExists == null)
            {
                await serviceClient.GetTableClient(tableName).CreateIfNotExistsAsync();
            }

            TableClient tableClient = serviceClient.GetTableClient(tableName);
            return tableClient;
        }
    }
}
