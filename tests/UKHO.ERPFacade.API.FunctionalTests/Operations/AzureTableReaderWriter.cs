using Azure.Data.Tables.Models;
using Azure.Data.Tables;
using Azure;
using UKHO.ERPFacade.Common.Models.TableEntities;
using UKHO.ERPFacade.API.FunctionalTests.Configuration;
using UKHO.ERPFacade.Common.Constants;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;

namespace UKHO.ERPFacade.API.FunctionalTests.Operations
{
    public class AzureTableReaderWriter : TestFixtureBase
    {
        private readonly AzureStorageConfiguration _azureStorageConfiguration;

        public AzureTableReaderWriter()
        {
            var serviceProvider = GetServiceProvider();
            _azureStorageConfiguration = serviceProvider!.GetRequiredService<IOptions<AzureStorageConfiguration>>().Value;
        }

        //Private Methods
        private TableClient GetTableClient(string tableName)
        {
            TableServiceClient serviceClient = new(_azureStorageConfiguration.ConnectionString);
            Pageable<TableItem> queryTableResults = serviceClient.Query(filter: $"TableName eq '{tableName}'");
            TableItem tableExists = queryTableResults.FirstOrDefault(t => t.Name == tableName);

            if (tableExists == null)
            {
                Console.WriteLine("Table doesn't exist, please check Azure portal");
            }

            TableClient tableClient = serviceClient.GetTableClient(tableName);
            return tableClient;
        }

        public string GetStatus(string correlationId)
        {
            TableClient tableClient = GetTableClient(AzureStorage.EventTableName);
            Pageable<EventEntity> existingEntity = tableClient.Query<EventEntity>(filter: TableClient.CreateQueryFilter($"RowKey eq {correlationId}"));
            return existingEntity != null ? existingEntity.FirstOrDefault()?.Status : string.Empty;
        }
    }
}
