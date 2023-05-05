using Azure.Data.Tables.Models;
using Azure.Data.Tables;
using Azure;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System.Diagnostics.CodeAnalysis;
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
        private const string ErpFacadeTableName = "eesevents";
        private const int DefaultCallbackDuration = 5;

        public AzureTableReaderWriter(ILogger<AzureTableReaderWriter> logger,
                                        IOptions<AzureStorageConfiguration> azureStorageConfig,
                                        IOptions<ErpFacadeWebJobConfiguration> erpFacadeWebjobConfig)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _azureStorageConfig = azureStorageConfig ?? throw new ArgumentNullException(nameof(azureStorageConfig));
            _erpFacadeWebjobConfig = erpFacadeWebjobConfig ?? throw new ArgumentNullException(nameof(erpFacadeWebjobConfig));
        }

        public async Task UpsertEntity(JObject eesEvent, string traceId)
        {
            TableClient tableClient = GetTableClient(ErpFacadeTableName);

            EESEventEntity existingEntity = await GetEntity(traceId);

            if (existingEntity == null)
            {
                EESEventEntity eESEvent = new()
                {
                    RowKey = Guid.NewGuid().ToString(),
                    PartitionKey = Guid.NewGuid().ToString(),
                    Timestamp = DateTime.UtcNow,
                    TraceID = traceId,
                    EventData = eesEvent.ToString(),
                    RequestDateTime = null,
                    ResponseDateTime = null,
                    IsNotified = false
                };

                await tableClient.AddEntityAsync(eESEvent, CancellationToken.None);

                _logger.LogInformation(EventIds.AddedEncContentPublishedEventInAzureTable.ToEventId(), "ENC content published event is added in azure table successfully.");
            }
            else
            {
                _logger.LogWarning(EventIds.ReceivedDuplicateEncContentPublishedEvent.ToEventId(), "Duplicate ENC content published event received.");

                existingEntity.Timestamp = DateTime.UtcNow;
                existingEntity.EventData = eesEvent.ToString();

                await tableClient.UpdateEntityAsync(existingEntity, ETag.All, TableUpdateMode.Replace);

                _logger.LogInformation(EventIds.UpdatedEncContentPublishedEventInAzureTable.ToEventId(), "Existing ENC content published event is updated in azure table successfully.");
            }
        }

        public async Task<EESEventEntity> GetEntity(string traceId)
        {
            IList<EESEventEntity> records = new List<EESEventEntity>();
            TableClient tableClient = GetTableClient(ErpFacadeTableName);
            var entities = tableClient.QueryAsync<EESEventEntity>(filter: TableClient.CreateQueryFilter($"TraceID eq {traceId}"), maxPerPage: 1);
            await foreach (var entity in entities)
            {
                records.Add(entity);
            }
            return records.FirstOrDefault();
        }

        public async Task UpdateRequestTimeEntity(string traceId)
        {
            TableClient tableClient = GetTableClient(ErpFacadeTableName);
            EESEventEntity existingEntity = await GetEntity(traceId);
            if (existingEntity != null)
            {
                existingEntity.RequestDateTime = DateTime.UtcNow;
                await tableClient.UpdateEntityAsync(existingEntity, ETag.All, TableUpdateMode.Replace);
                _logger.LogInformation(EventIds.UpdateRequestTimeEntitySuccessful.ToEventId(), "RequestDateTime is updated in azure table successfully.");
            }
        }

        public async Task UpdateResponseTimeEntity(string traceId)
        {
            TableClient tableClient = GetTableClient(ErpFacadeTableName);
            EESEventEntity existingEntity = await GetEntity(traceId);
            if (existingEntity != null)
            {
                existingEntity.ResponseDateTime = DateTime.UtcNow;
                await tableClient.UpdateEntityAsync(existingEntity, ETag.All, TableUpdateMode.Replace);
                _logger.LogInformation(EventIds.UpdateResponseTimeEntitySuccessful.ToEventId(), "ResponseDateTime is updated in azure table successfully.");
            }
        }

        public void ValidateAndUpdateIsNotifiedEntity()
        {
            TableClient tableClient = GetTableClient(ErpFacadeTableName);
            var callBackDuration = string.IsNullOrEmpty(_erpFacadeWebjobConfig.Value.SapCallbackDurationInMins) ? DefaultCallbackDuration
                : int.Parse(_erpFacadeWebjobConfig.Value.SapCallbackDurationInMins);
            var entities = tableClient.Query<EESEventEntity>(entity => entity.IsNotified!.Value == false);
            foreach (var tableitem in entities)
            {
                if (tableitem.RequestDateTime.HasValue)
                {
                    if (!tableitem.ResponseDateTime.HasValue && (tableitem.RequestDateTime.Value - DateTime.Now) <= TimeSpan.FromMinutes(callBackDuration)
                        ||
                        tableitem.ResponseDateTime.HasValue && ((tableitem.ResponseDateTime.Value - tableitem.RequestDateTime.Value) > TimeSpan.FromMinutes(callBackDuration)))
                    {
                        _logger.LogWarning(EventIds.WebjobCallbackTimeoutEventFromSAP.ToEventId(), $"Request is timed out for the traceid : {tableitem.TraceID}.");

                        TableEntity tableEntity = new TableEntity(tableitem.PartitionKey, tableitem.RowKey)
                            {
                                 {"IsNotified", true }
                            };

                        tableClient.UpdateEntity(tableEntity, tableitem.ETag);
                    }
                }
                else
                {
                    _logger.LogError(EventIds.EmptyRequestDateTime.ToEventId(), $"Empty RequestDateTime column for traceid : {tableitem.TraceID}");
                }
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
                throw new Exception();
            }

            TableClient tableClient = serviceClient.GetTableClient(tableName);
            return tableClient;
        }
    }
}