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

namespace UKHO.ERPFacade.Common.IO
{
    [ExcludeFromCodeCoverage]
    public class AzureTableReaderWriter : IAzureTableReaderWriter
    {
        private readonly ILogger<AzureTableReaderWriter> _logger;
        private const string ERP_FACADE_TABLE_NAME = "eesevents";

        private readonly IOptions<AzureStorageConfiguration> _azureStorageConfig;

        public AzureTableReaderWriter(ILogger<AzureTableReaderWriter> logger,
                                        IOptions<AzureStorageConfiguration> azureStorageConfig)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _azureStorageConfig = azureStorageConfig ?? throw new ArgumentNullException(nameof(azureStorageConfig));
        }

        public async Task UpsertEntity(JObject eesEvent, string traceId)
        {
            TableClient tableClient = GetTableClient(ERP_FACADE_TABLE_NAME);

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
                    ResponseDateTime = null
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
            TableClient tableClient = GetTableClient(ERP_FACADE_TABLE_NAME);
            var entities = tableClient.QueryAsync<EESEventEntity>(filter: TableClient.CreateQueryFilter($"TraceID eq {traceId}"), maxPerPage: 1);
            await foreach (var entity in entities)
            {
                records.Add(entity);
            }
            return records.FirstOrDefault();
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