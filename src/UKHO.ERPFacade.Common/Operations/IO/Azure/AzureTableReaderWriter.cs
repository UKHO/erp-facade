using System.Diagnostics.CodeAnalysis;
using Azure;
using Azure.Data.Tables;
using Microsoft.Extensions.Options;
using UKHO.ERPFacade.Common.Configuration;
using UKHO.ERPFacade.Common.Constants;
using UKHO.ERPFacade.Common.Models.TableEntities;

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
            await tableClient.UpsertEntityAsync(entity, TableUpdateMode.Replace);
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

        public async Task UpdateEntityAsync(string partitionKey, string rowKey, Dictionary<string, object> entitiesToUpdate)
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

        public async Task<IList<TableEntity>> GetFilteredEntitiesAsync(Dictionary<string, string> filters)
        {
            TableClient tableClient = await GetTableClientAsync(AzureStorage.EventTableName);
            string filterQuery = string.Join(" and ", filters.Select(filter => $"{filter.Key} eq '{filter.Value}'"));
            return [.. tableClient.Query<TableEntity>(filter: filterQuery)];
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
            TableClient tableClient = serviceClient.GetTableClient(tableName);
            await tableClient.CreateIfNotExistsAsync();
            return tableClient;
        }
    }
}
