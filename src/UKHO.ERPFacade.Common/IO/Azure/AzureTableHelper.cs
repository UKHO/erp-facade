using System.Diagnostics.CodeAnalysis;
using Azure;
using Azure.Data.Tables;
using Azure.Data.Tables.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UKHO.ERPFacade.Common.Configuration;

namespace UKHO.ERPFacade.Common.IO.Azure
{
    [ExcludeFromCodeCoverage]
    public class AzureTableHelper : IAzureTableHelper
    {
        private readonly ILogger<AzureTableHelper> _logger;
        private readonly IOptions<AzureStorageConfiguration> _azureStorageConfig;

        public AzureTableHelper(ILogger<AzureTableHelper> logger,
                                        IOptions<AzureStorageConfiguration> azureStorageConfig)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _azureStorageConfig = azureStorageConfig ?? throw new ArgumentNullException(nameof(azureStorageConfig));
        }

        public async Task UpsertEntity(ITableEntity entity)
        {
            TableClient tableClient = GetTableClient(Constants.Constants.EventTableName);

            TableEntity existingEntity = await GetEntity(entity.PartitionKey, entity.RowKey);

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

        public async Task<TableEntity> GetEntity(string partitionKey, string rowKey)
        {
            TableClient tableClient = GetTableClient(Constants.Constants.EventTableName);
            try
            {
                return await tableClient.GetEntityAsync<TableEntity>(partitionKey, rowKey);
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                return null;
            }
        }

        public async Task UpdateEntity<TKey, TValue>(string partitionKey, string rowKey, KeyValuePair<TKey, TValue>[] entitiesToUpdate)
        {
            TableClient tableClient = GetTableClient(Constants.Constants.EventTableName);
            TableEntity existingEntity = await GetEntity(partitionKey, rowKey);
            if (existingEntity != null)
            {
                foreach (var entity in entitiesToUpdate)
                {
                    existingEntity[entity.Key.ToString()] = entity.Value;
                }
                await tableClient.UpdateEntityAsync(existingEntity, ETag.All, TableUpdateMode.Replace);
            }
        }

        public IList<TableEntity> GetAllEntities(string partitionKey)
        {
            var records = new List<TableEntity>();
            TableClient tableClient = GetTableClient(Constants.Constants.EventTableName);
            var entities = tableClient.Query<TableEntity>(filter: $"PartitionKey eq '{partitionKey}'");
            foreach (var entity in entities)
            {
                records.Add(entity);
            }
            return records;
        }

        public async Task DeleteEntity(string correlationId)
        {
            TableClient tableClient = GetTableClient(Constants.Constants.EventTableName);
            TableEntity existingEntity = await GetEntity(correlationId, Constants.Constants.EventTableName);
            if (existingEntity != null)
            {
                tableClient.DeleteEntity(existingEntity.PartitionKey, existingEntity.RowKey);
            }
        }

        //Private Methods
        private TableClient GetTableClient(string tableName)
        {
            TableServiceClient serviceClient = new(_azureStorageConfig.Value.ConnectionString);
            Pageable<TableItem> queryTableResults = serviceClient.Query(filter: $"TableName eq '{tableName}'");
            var tableExists = queryTableResults.FirstOrDefault(t => t.Name == tableName);

            if (tableExists == null)
            {
                serviceClient.GetTableClient(tableName).CreateIfNotExistsAsync();
            }

            TableClient tableClient = serviceClient.GetTableClient(tableName);
            return tableClient;
        }
    }
}
