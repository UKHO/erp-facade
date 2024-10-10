using System.Diagnostics.CodeAnalysis;
using Azure;
using Azure.Data.Tables;
using Azure.Data.Tables.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UKHO.ERPFacade.Common.Configuration;
using UKHO.ERPFacade.Common.Logging;
using UKHO.ERPFacade.Common.Models.TableEntities;

namespace UKHO.ERPFacade.Common.IO.Azure
{
    [ExcludeFromCodeCoverage]
    public class AzureTableReaderWriter : IAzureTableReaderWriter
    {
        private readonly ILogger<AzureTableReaderWriter> _logger;
        private readonly IOptions<AzureStorageConfiguration> _azureStorageConfig;
        private const string ErpFacadeTableName = "encevents";
        private const string LicenceUpdateTableName = "licenceupdatedevents";
        private const string RecordOfSaleTableName = "recordofsaleevents";

        private enum Statuses
        {
            Incomplete,
            Complete
        }

        public AzureTableReaderWriter(ILogger<AzureTableReaderWriter> logger,
                                        IOptions<AzureStorageConfiguration> azureStorageConfig,
                                        IOptions<ErpFacadeWebJobConfiguration> erpFacadeWebjobConfig)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _azureStorageConfig = azureStorageConfig ?? throw new ArgumentNullException(nameof(azureStorageConfig));
        }

        public async Task UpsertEntity(string correlationId)
        {
            TableClient tableClient = GetTableClient(ErpFacadeTableName);

            EESEventEntity existingEntity = await GetEntity(correlationId);

            if (existingEntity == null!)
            {
                EESEventEntity eESEvent = new()
                {
                    RowKey = Guid.NewGuid().ToString(),
                    PartitionKey = Guid.NewGuid().ToString(),
                    Timestamp = DateTime.UtcNow,
                    CorrelationId = correlationId,
                    RequestDateTime = null
                };

                await tableClient.AddEntityAsync(eESEvent, CancellationToken.None);

                _logger.LogInformation(EventIds.AddedEntryForEncContentPublishedEventInAzureTable.ToEventId(), "New enccontentpublished event entry is added in azure table successfully.");
            }
            else
            {
                _logger.LogWarning(EventIds.ReceivedDuplicateEncContentPublishedEvent.ToEventId(), "Duplicate enccontentpublished event received.");

                existingEntity.Timestamp = DateTime.UtcNow;

                await tableClient.UpdateEntityAsync(existingEntity, ETag.All, TableUpdateMode.Replace);

                _logger.LogInformation(EventIds.UpdatedEncContentPublishedEventInAzureTable.ToEventId(), "Existing enccontentpublished event entry is updated in azure table successfully.");
            }
        }

        public async Task UpsertLicenceUpdatedEntity(string correlationId)
        {
            TableClient tableClient = GetTableClient(LicenceUpdateTableName);

            var existingEntity = await GetRecordOfSaleEntity(correlationId, LicenceUpdateTableName);

            if (existingEntity == null!)
            {
                RecordOfSaleEventEntity licenceUpdatedEventsEntity = new()
                {
                    RowKey = Guid.NewGuid().ToString(),
                    PartitionKey = Guid.NewGuid().ToString(),
                    Timestamp = DateTime.UtcNow,
                    CorrelationId = correlationId,
                    Status = Statuses.Incomplete.ToString()
                };

                await tableClient.AddEntityAsync(licenceUpdatedEventsEntity, CancellationToken.None);

                _logger.LogInformation(EventIds.AddedLicenceUpdatedPublishedEventInAzureTable.ToEventId(), "Licence updated published event is added in azure table successfully.");
            }
            else
            {
                _logger.LogWarning(EventIds.ReceivedDuplicateLicenceUpdatedPublishedEvent.ToEventId(), "Duplicate Licence updated published event received.");

                existingEntity.Timestamp = DateTime.UtcNow;

                await tableClient.UpdateEntityAsync(existingEntity, ETag.All, TableUpdateMode.Replace);

                _logger.LogInformation(EventIds.UpdatedLicenceUpdatedPublishedEventInAzureTable.ToEventId(), "Existing Licence updated published event is updated in azure table successfully.");
            }
        }

        public async Task<EESEventEntity> GetEntity(string correlationId)
        {
            IList<EESEventEntity> records = new List<EESEventEntity>();
            TableClient tableClient = GetTableClient(ErpFacadeTableName);
            var entities = tableClient.QueryAsync<EESEventEntity>(filter: TableClient.CreateQueryFilter($"CorrelationId eq {correlationId}"), maxPerPage: 1);
            await foreach (var entity in entities)
            {
                records.Add(entity);
            }
            return records.FirstOrDefault();
        }

        public async Task UpdateRequestTimeEntity(string correlationId)
        {
            TableClient tableClient = GetTableClient(ErpFacadeTableName);
            EESEventEntity existingEntity = await GetEntity(correlationId);
            if (existingEntity != null)
            {
                existingEntity.RequestDateTime = DateTime.UtcNow;
                await tableClient.UpdateEntityAsync(existingEntity, ETag.All, TableUpdateMode.Replace);
                _logger.LogInformation(EventIds.UpdateRequestTimeEntitySuccessful.ToEventId(), "SAP request time for {CorrelationId} is updated in azure table successfully.", correlationId);
            }
        }

        public IList<EESEventEntity> GetAllEntityForEESTable()
        {
            IList<EESEventEntity> records = new List<EESEventEntity>();
            TableClient tableClient = GetTableClient(ErpFacadeTableName);
            var entities = tableClient.Query<EESEventEntity>();
            foreach (var entity in entities)
            {
                records.Add(entity);
            }
            return records;
        }

        public async Task DeleteEESEntity(string correlationId)
        {
            TableClient tableClient = GetTableClient(ErpFacadeTableName);
            EESEventEntity existingEntity = await GetEntity(correlationId);
            if (existingEntity != null)
            {
                tableClient.DeleteEntity(existingEntity.PartitionKey, existingEntity.RowKey);
                _logger.LogInformation(EventIds.DeletedEESEntitySuccessful.ToEventId(), "EES entity is deleted from azure table successfully.");
            }
        }

        public async Task UpsertRecordOfSaleEntity(string correlationId)
        {
            TableClient tableClient = GetTableClient(RecordOfSaleTableName);

            RecordOfSaleEventEntity existingEntity = await GetRecordOfSaleEntity(correlationId, RecordOfSaleTableName);

            if (existingEntity == null!)
            {
                RecordOfSaleEventEntity recordOfSaleEvent = new()
                {
                    RowKey = Guid.NewGuid().ToString(),
                    PartitionKey = Guid.NewGuid().ToString(),
                    Timestamp = DateTime.UtcNow,
                    CorrelationId = correlationId,
                    Status = Statuses.Incomplete.ToString()
                };

                await tableClient.AddEntityAsync(recordOfSaleEvent, CancellationToken.None);

                _logger.LogInformation(EventIds.AddedRecordOfSalePublishedEventInAzureTable.ToEventId(), "Record Of Sale published event is added in azure table successfully.");
            }
            else
            {
                _logger.LogWarning(EventIds.ReceivedDuplicateRecordOfSalePublishedEvent.ToEventId(), "Duplicate record of sale published event received.");

                existingEntity.Timestamp = DateTime.UtcNow;

                await tableClient.UpdateEntityAsync(existingEntity, ETag.All, TableUpdateMode.Replace);

                _logger.LogInformation(EventIds.UpdatedRecordOfSalePublishedEventInAzureTable.ToEventId(), "Existing Record Of Sale published event is updated in azure table successfully.");
            }
        }

        public async Task<RecordOfSaleEventEntity> GetRecordOfSaleEntity(string correlationId, string tableName)
        {
            IList<RecordOfSaleEventEntity> records = new List<RecordOfSaleEventEntity>();
            TableClient tableClient = GetTableClient(tableName);
            var entities = tableClient.QueryAsync<RecordOfSaleEventEntity>(filter: TableClient.CreateQueryFilter($"CorrelationId eq {correlationId}"), maxPerPage: 1);
            await foreach (var entity in entities)
            {
                records.Add(entity);
            }
            return records.FirstOrDefault();
        }

        public async Task UpdateRecordOfSaleEventStatus(string correlationId)
        {
            TableClient tableClient = GetTableClient(RecordOfSaleTableName);
            RecordOfSaleEventEntity existingEntity = await GetRecordOfSaleEntity(correlationId, RecordOfSaleTableName);

            if (existingEntity != null!)
            {
                existingEntity.Status = Statuses.Complete.ToString();
                await tableClient.UpdateEntityAsync(existingEntity, ETag.All, TableUpdateMode.Replace);

                _logger.LogInformation(EventIds.UpdatedStatusOfRecordOfSalePublishedEventInAzureTable.ToEventId(), "Status of existing record of sale published event updated in azure table successfully.");
            }
        }

        public async Task UpdateLicenceUpdatedEventStatus(string correlationId)
        {
            TableClient tableClient = GetTableClient(LicenceUpdateTableName);
            RecordOfSaleEventEntity existingEntity = await GetRecordOfSaleEntity(correlationId, LicenceUpdateTableName);

            if (existingEntity != null!)
            {
                existingEntity.Status = Statuses.Complete.ToString();
                await tableClient.UpdateEntityAsync(existingEntity, ETag.All, TableUpdateMode.Replace);

                _logger.LogInformation(EventIds.UpdatedStatusOfLicenceUpdatedPublishedEventInAzureTable.ToEventId(), "Status of existing licence updated published event updated in azure table successfully.");
            }
        }

        public string GetEntityStatus(string correlationId)
        {
            string status = string.Empty;
            ;
            TableClient tableClient = GetTableClient(RecordOfSaleTableName);

            var entities = tableClient.Query<RecordOfSaleEventEntity>(filter: TableClient.CreateQueryFilter($"CorrelationId eq {correlationId}"), maxPerPage: 1);

            foreach (var entity in entities)
            {
                status = entity.Status;
            }

            return status;
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
                _logger.LogWarning(EventIds.AzureTableNotFound.ToEventId(), "Azure table not found");
            }

            TableClient tableClient = serviceClient.GetTableClient(tableName);
            return tableClient;
        }
    }
}
