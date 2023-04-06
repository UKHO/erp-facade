using Azure;
using Azure.Data.Tables;
using Azure.Data.Tables.Models;
using Azure.Storage.Blobs.Specialized;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System.Diagnostics.CodeAnalysis;
using UKHO.ERPFacade.Common.Configuration;
using UKHO.ERPFacade.Common.Logging;
using UKHO.ERPFacade.Common.Models.TableEntities;

namespace UKHO.ERPFacade.Common.Helpers
{
    [ExcludeFromCodeCoverage]
    public class AzureTableStorageHelper : IAzureTableStorageHelper
    {
        private readonly ILogger<AzureTableStorageHelper> _logger;
        private const string ERP_FACADE_TABLE_NAME = "abc";

        private readonly IOptions<AzureStorageConfiguration> _azureStorageConfig;

        public AzureTableStorageHelper(ILogger<AzureTableStorageHelper> logger,
                                        IOptions<AzureStorageConfiguration> azureStorageConfig)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _azureStorageConfig = azureStorageConfig;
        }

        public async Task UpsertEntity(JObject eesEvent, string traceId, string correlationId)
        {
            TableClient tableClient = GetTableClient(ERP_FACADE_TABLE_NAME);

            EESEventTable? existingEntity = await GetEntity(traceId);

            if (existingEntity == null)
            {

                EESEventTable eESEvent = new()
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
                _logger.LogInformation(EventIds.AddedEncContentPublishedEventInAzureTable.ToEventId(), "Added new ENC content published event in Azure table storage successfully. | _X-Correlation-ID : {CorrelationId}", correlationId);
            }
            else
            {
                _logger.LogWarning(EventIds.CheckDuplicateEncContentPublishedEvent.ToEventId(), "ENC content published event already exists! | _X-Correlation-ID : {CorrelationId}", correlationId);

                existingEntity.Timestamp = DateTime.UtcNow;
                existingEntity.EventData = eesEvent.ToString();

                await tableClient.UpdateEntityAsync(existingEntity, ETag.All, TableUpdateMode.Replace);
                _logger.LogInformation(EventIds.UpdatedEncContentPublishedEventInAzureTable.ToEventId(), "Updated the existing ENC content published event in Azure table storage successfully. | _X-Correlation-ID : {CorrelationId}", correlationId);
            }
        }

        public async Task<EESEventTable?> GetEntity(string traceId)
        {
            IList<EESEventTable> records = new List<EESEventTable>();
            TableClient tableClient = GetTableClient(ERP_FACADE_TABLE_NAME);
            var entities = tableClient.QueryAsync<EESEventTable>(filter: TableClient.CreateQueryFilter($"TraceID eq {traceId}"), maxPerPage: 1);
            await foreach (var entity in entities)
            {
                records.Add(entity);
            }
            return records.FirstOrDefault();
        }

        //Private Methods
        private TableClient GetTableClient(string tableName)
        {
            var serviceClient = new TableServiceClient(_azureStorageConfig.Value.ConnectionString);
            Pageable<TableItem> queryTableResults = serviceClient.Query(filter: $"TableName eq '{tableName}'");
            var tableExists = queryTableResults.FirstOrDefault(t => t.Name == tableName);

            if (tableExists == null)
            {
                throw new Exception();
            }

            TableClient? tableClient = serviceClient.GetTableClient(tableName);
            return tableClient;
        }
    }
}
