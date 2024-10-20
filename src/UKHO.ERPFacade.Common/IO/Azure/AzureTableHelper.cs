using System.Diagnostics.CodeAnalysis;
using Azure;
using Azure.Data.Tables;
using Azure.Data.Tables.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UKHO.ERPFacade.Common.Configuration;
using UKHO.ERPFacade.Common.Logging;

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

        public async Task UpsertEntity(string correlationId, ITableEntity entity)
        {
            _logger.LogInformation(EventIds.AddingEntryForEncContentPublishedEventInAzureTable.ToEventId(), "Adding/Updating entry for enccontentpublished event in azure table.");

            TableClient tableClient = GetTableClient(Constants.Constants.EventTableName);

            TableEntity existingEntity = await GetEntity(correlationId, entity.PartitionKey);

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

        public async Task<TableEntity> GetEntity(string correlationId, string partitionKey)
        {
            try
            {
                IList<TableEntity> records = new List<TableEntity>();
                TableClient tableClient = GetTableClient(Constants.Constants.EventTableName);
                return await tableClient.GetEntityAsync<TableEntity>(partitionKey, correlationId);
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                return null;
            }
        }

        public async Task UpdateEntity<TKey, TValue>(string correlationId, string tableName, KeyValuePair<TKey, TValue>[] entitiesToUpdate)
        {
            TableClient tableClient = GetTableClient(tableName);
            TableEntity existingEntity = await GetEntity(correlationId, tableName);
            if (existingEntity != null)
            {
                foreach (var entity in entitiesToUpdate)
                {
                    existingEntity[entity.Key.ToString()] = entity.Value;
                }
                await tableClient.UpdateEntityAsync(existingEntity, ETag.All, TableUpdateMode.Replace);
            }
        }

        public IList<TableEntity> GetAllEntities(string tableName)
        {
            var records = new List<TableEntity>();
            TableClient tableClient = GetTableClient(tableName);
            var entities = tableClient.Query<TableEntity>();
            foreach (var entity in entities)
            {
                records.Add(entity);
            }
            return records;
        }

        public async Task DeleteEntity(string correlationId, string tableName)
        {
            TableClient tableClient = GetTableClient(tableName);
            TableEntity existingEntity = await GetEntity(correlationId, tableName);
            if (existingEntity != null)
            {
                tableClient.DeleteEntity(existingEntity.PartitionKey, existingEntity.RowKey);
                _logger.LogInformation(EventIds.DeletedEESEntitySuccessful.ToEventId(), "EES entity is deleted from azure table successfully.");
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
                _logger.LogWarning(EventIds.AzureTableNotFound.ToEventId(), "Azure table not found. Created new azure table.");
            }

            TableClient tableClient = serviceClient.GetTableClient(tableName);
            return tableClient;
        }
    }
}
