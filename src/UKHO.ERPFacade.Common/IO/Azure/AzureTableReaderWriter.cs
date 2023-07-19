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
        private readonly IOptions<ErpFacadeWebJobConfiguration> _erpFacadeWebjobConfig;
        private readonly TableClient _unitPriceChangeTableClient;
        private const string ErpFacadeTableName = "encevents";
        private const string PriceChangeMasterTableName = "pricechangemaster";
        private const string UnitPriceChangeTableName = "unitpricechangeevents";
        private const int DefaultCallbackDuration = 5;

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
            _erpFacadeWebjobConfig = erpFacadeWebjobConfig ?? throw new ArgumentNullException(nameof(erpFacadeWebjobConfig));

            _unitPriceChangeTableClient = GetTableClient(UnitPriceChangeTableName);
        }

        public async Task UpsertEntity(string correlationId)
        {
            TableClient tableClient = GetTableClient(ErpFacadeTableName);

            EESEventEntity existingEntity = await GetEntity(correlationId);

            if (existingEntity == null)
            {
                EESEventEntity eESEvent = new()
                {
                    RowKey = Guid.NewGuid().ToString(),
                    PartitionKey = Guid.NewGuid().ToString(),
                    Timestamp = DateTime.UtcNow,
                    CorrelationId = correlationId,
                    RequestDateTime = null,
                    ResponseDateTime = null,
                    PublishDateTime = null,
                    IsNotified = false
                };

                await tableClient.AddEntityAsync(eESEvent, CancellationToken.None);

                _logger.LogInformation(EventIds.AddedEncContentPublishedEventInAzureTable.ToEventId(), "ENC content published event is added in azure table successfully.");
            }
            else
            {
                _logger.LogWarning(EventIds.ReceivedDuplicateEncContentPublishedEvent.ToEventId(), "Duplicate ENC content published event received.");

                existingEntity.Timestamp = DateTime.UtcNow;

                await tableClient.UpdateEntityAsync(existingEntity, ETag.All, TableUpdateMode.Replace);

                _logger.LogInformation(EventIds.UpdatedEncContentPublishedEventInAzureTable.ToEventId(), "Existing ENC content published event is updated in azure table successfully.");
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
                _logger.LogInformation(EventIds.UpdateRequestTimeEntitySuccessful.ToEventId(), "RequestDateTime is updated in azure table successfully.");
            }
        }

        public async Task UpdateResponseTimeEntity(string correlationId)
        {
            TableClient tableClient = GetTableClient(ErpFacadeTableName);
            EESEventEntity existingEntity = await GetEntity(correlationId);
            if (existingEntity != null)
            {
                existingEntity.ResponseDateTime = DateTime.UtcNow;
                await tableClient.UpdateEntityAsync(existingEntity, ETag.All, TableUpdateMode.Replace);
                _logger.LogInformation(EventIds.UpdateResponseTimeEntitySuccessful.ToEventId(), "ResponseDateTime is updated in azure table successfully.");
            }
        }

        public async Task UpdatePublishDateTimeEntity(string correlationId, string eventId)
        {
            TableClient tableClient = GetTableClient(ErpFacadeTableName);
            EESEventEntity existingEntity = await GetEntity(correlationId);
            if (existingEntity != null)
            {
                existingEntity.PublishDateTime = DateTime.UtcNow;
                await tableClient.UpdateEntityAsync(existingEntity, ETag.All, TableUpdateMode.Replace);
                _logger.LogInformation(EventIds.UpdatePublishDateTimeEntitySuccessful.ToEventId(), "PublishDateTime is updated in azure table successfully. | _X-Correlation-ID : {_X-Correlation-ID} | PublishedEventId : {PublishedEventId}", correlationId, eventId);
            }
        }

        public void ValidateAndUpdateIsNotifiedEntity()
        {
            TableClient tableClient = GetTableClient(ErpFacadeTableName);
            var callBackDuration = string.IsNullOrEmpty(_erpFacadeWebjobConfig.Value.SapCallbackDurationInMins) ? DefaultCallbackDuration
                : int.Parse(_erpFacadeWebjobConfig.Value.SapCallbackDurationInMins);
            var entities = tableClient.Query<EESEventEntity>(entity => entity.IsNotified!.Value == false);
            foreach (var tableItem in entities)
            {
                if (tableItem.RequestDateTime.HasValue)
                {
                    if (!tableItem.ResponseDateTime.HasValue && (DateTime.UtcNow - tableItem.RequestDateTime.Value) > TimeSpan.FromMinutes(callBackDuration)
                        ||
                        tableItem.ResponseDateTime.HasValue && ((tableItem.ResponseDateTime.Value - tableItem.RequestDateTime.Value) > TimeSpan.FromMinutes(callBackDuration)))
                    {
                        _logger.LogWarning(EventIds.WebjobCallbackTimeoutEventFromSAP.ToEventId(), "Request is timed out for the correlationid : {tableItem.CorrelationId}.", tableItem.CorrelationId);

                        TableEntity tableEntity = new(tableItem.PartitionKey, tableItem.RowKey)
                        {
                            { "IsNotified", true }
                        };

                        tableClient.UpdateEntity(tableEntity, tableItem.ETag);
                    }
                }
                else
                {
                    _logger.LogError(EventIds.EmptyRequestDateTime.ToEventId(), "Empty RequestDateTime column for correlationid : {tableItem.CorrelationId}.", tableItem.CorrelationId);
                }
            }
        }

        public IList<PriceChangeMasterEntity> GetMasterEntities(string status, string correlationId = "")
        {
            IList<PriceChangeMasterEntity> records = new List<PriceChangeMasterEntity>();
            TableClient tableClient = GetTableClient(PriceChangeMasterTableName);
            Pageable<PriceChangeMasterEntity> entities = string.IsNullOrEmpty(correlationId)
                ? tableClient.Query<PriceChangeMasterEntity>(filter: TableClient.CreateQueryFilter($"Status eq {status}"), maxPerPage: 1)
                : tableClient.Query<PriceChangeMasterEntity>(filter: TableClient.CreateQueryFilter($"Status eq {status} and CorrId eq {correlationId}"), maxPerPage: 1);
            foreach (var entity in entities)
            {
                records.Add(entity);
            }
            return records;
        }

        public IList<UnitPriceChangeEntity> GetUnitPriceChangeEventsEntities(string masterCorrId, string status = "", string unitName = "", string eventId = "")
        {
            IList<UnitPriceChangeEntity> records = new List<UnitPriceChangeEntity>();
            TableClient tableClient = GetTableClient(UnitPriceChangeTableName);
            Pageable<UnitPriceChangeEntity> entities = string.IsNullOrEmpty(status)
                ? tableClient.Query<UnitPriceChangeEntity>(filter: TableClient.CreateQueryFilter($"MasterCorrId eq {masterCorrId}"), maxPerPage: 1)
                : string.IsNullOrEmpty(unitName) && string.IsNullOrEmpty(eventId)
                ? tableClient.Query<UnitPriceChangeEntity>(filter: TableClient.CreateQueryFilter($"Status eq {status} and MasterCorrId eq {masterCorrId}"), maxPerPage: 1)
                : tableClient.Query<UnitPriceChangeEntity>(filter: TableClient.CreateQueryFilter($"Status eq {status} and MasterCorrId eq {masterCorrId} and UnitName eq {unitName} and EventId eq {eventId}"), maxPerPage: 1);
            foreach (var entity in entities)
            {
                records.Add(entity);
            }
            return records;
        }

        public async Task AddPriceChangeEntity(string correlationId)
        {
            TableClient tableClient = GetTableClient(PriceChangeMasterTableName);

            PriceChangeMasterEntity priceChangeEventEntity = new()
            {
                RowKey = correlationId,
                PartitionKey = correlationId,
                Timestamp = DateTime.UtcNow,
                CorrId = correlationId,
                PublishDateTime = null,
                Status = "Incomplete"
            };

            await tableClient.AddEntityAsync(priceChangeEventEntity, CancellationToken.None);

            _logger.LogInformation(EventIds.AddedBulkPriceInformationEventInAzureTable.ToEventId(), "Bulk price information event is added in azure table successfully. | _X-Correlation-ID : {_X-Correlation-ID}", correlationId);
        }

        public void AddUnitPriceChangeEntity(string correlationId, string eventId, string unitName)
        {
            UnitPriceChangeEntity unitPriceChangeEventEntity = new()
            {
                RowKey = eventId,
                PartitionKey = eventId,
                Timestamp = DateTime.UtcNow,
                MasterCorrId = correlationId,
                EventId = eventId,
                UnitName = unitName,
                PublishDateTime = null,
                Status = "Incomplete"
            };

            _unitPriceChangeTableClient.AddEntity(unitPriceChangeEventEntity, CancellationToken.None);

            _logger.LogInformation(EventIds.AddedUnitPriceChangeEventInAzureTable.ToEventId(), "Unit price change event in added in azure table successfully. | _X-Correlation-ID : {_X-Correlation-ID} | PublishedEventId : {PublishedEventId}", correlationId, eventId);
        }

        public void UpdateUnitPriceChangeStatusAndPublishDateTimeEntity(string correlationId, string unitName, string eventId)
        {
            TableClient tableClient = GetTableClient(UnitPriceChangeTableName);
            UnitPriceChangeEntity? existingEntity = GetUnitPriceChangeEventsEntities(correlationId, Statuses.Incomplete.ToString(), unitName, eventId).ToList().FirstOrDefault();
            if (existingEntity != null)
            {
                existingEntity.Status = "Complete";
                existingEntity.PublishDateTime = DateTime.UtcNow;
                tableClient.UpdateEntity(existingEntity, ETag.All, TableUpdateMode.Replace);
                _logger.LogInformation(EventIds.UpdatedPriceChangeStatusEntitySuccessful.ToEventId(), "Unit price change status and PublishingDateTime is updated in azure table successfully. | _X-Correlation-ID : {_X-Correlation-ID} | PublishedEventId : {PublishedEventId}", correlationId, eventId);
            }
        }

        public void UpdatePriceMasterStatusAndPublishDateTimeEntity(string correlationId)
        {
            TableClient tableClient = GetTableClient(PriceChangeMasterTableName);
            PriceChangeMasterEntity? existingEntity = GetMasterEntities(Statuses.Incomplete.ToString(), correlationId).ToList().FirstOrDefault();
            if (existingEntity != null)
            {
                existingEntity.Status = "Complete";
                existingEntity.PublishDateTime = DateTime.UtcNow;
                tableClient.UpdateEntity(existingEntity, ETag.All, TableUpdateMode.Replace);
                _logger.LogInformation(EventIds.UpdatedPriceChangeMasterStatusEntitySuccessful.ToEventId(), "Price change master status and PublishDatetime is updated in azure table successfully. | _X-Correlation-ID : {_X-Correlation-ID}", correlationId);
            }
        }

        public void DeletePriceMasterEntity(string correlationId)
        {
            TableClient tableClient = GetTableClient(PriceChangeMasterTableName);
            PriceChangeMasterEntity? existingEntity = GetMasterEntities(Statuses.Complete.ToString(), correlationId).ToList().FirstOrDefault();
            if (existingEntity != null)
            {
                tableClient.DeleteEntity(existingEntity.PartitionKey, existingEntity.RowKey);
                _logger.LogInformation(EventIds.DeletedPriceChangeMasterEntitySuccessful.ToEventId(), "Price change master entity is deleted from azure table successfully.");
            }
        }

        public void DeleteUnitPriceChangeEntityForMasterCorrId(string correlationId)
        {
            TableClient tableClient = GetTableClient(UnitPriceChangeTableName);
            var existingEntities = GetUnitPriceChangeEventsEntities(correlationId).ToList();
            if (existingEntities != null)
            {
                foreach (var entity in existingEntities)
                {
                    tableClient.DeleteEntity(entity.PartitionKey, entity.RowKey);
                }
                _logger.LogInformation(EventIds.DeletedUnitPriceChangeEntitySuccessful.ToEventId(), "Unit price change status is deleted from azure table successfully.");
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
