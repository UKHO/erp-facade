using Azure.Data.Tables.Models;
using Azure.Data.Tables;
using Azure;
using UKHO.ERPFacade.Common.Models.TableEntities;
using UKHO.ERPFacade.API.FunctionalTests.Configuration;

namespace UKHO.ERPFacade.API.FunctionalTests.Helpers
{
    public class AzureTableHelper
    {
        private const string ErpFacadeTableName = "encevents";
        private const string RoSEventsTableName = "recordofsaleevents";

        //Private Methods
        private static TableClient GetTableClient(string tableName)
        {
            TableServiceClient serviceClient = new(Config.TestConfig.AzureStorageConfiguration.ConnectionString);
            Pageable<TableItem> queryTableResults = serviceClient.Query(filter: $"TableName eq '{tableName}'");
            TableItem tableExists = queryTableResults.FirstOrDefault(t => t.Name == tableName);

            if (tableExists == null)
            {
                Console.WriteLine("Table doesn't exist, please check Azure portal");
            }

            TableClient tableClient = serviceClient.GetTableClient(tableName);
            return tableClient;
        }

        
        public static string GetSapStatus(string correlationId)
        {
            TableClient tableClient = GetTableClient(RoSEventsTableName);
            Pageable<RecordOfSaleEventEntity> existingEntity = tableClient.Query<RecordOfSaleEventEntity>(filter: TableClient.CreateQueryFilter($"CorrelationId eq {correlationId}"));
            return existingEntity != null ? existingEntity.FirstOrDefault()?.Status : string.Empty;
        }
    }
}
