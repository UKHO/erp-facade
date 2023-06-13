using Azure.Data.Tables.Models;
using Azure.Data.Tables;
using Azure;
using UKHO.ERPFacade.Common.Models.TableEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace UKHO.ERPFacade.API.FunctionalTests.Helpers
{
    
    public class AzureTableHelper
    {
        private const string ErpFacadeTableName = "eesevents";
 
        //Private Methods
        private TableClient GetTableClient(string tableName)
        {
            TableServiceClient serviceClient = new(Config.TestConfig.AzureStorageConfiguration.ConnectionString);
            Pageable<TableItem> queryTableResults = serviceClient.Query(filter: $"TableName eq '{tableName}'");
            var tableExists = queryTableResults.FirstOrDefault(t => t.Name == tableName);

            if (tableExists == null)
            {
                Console.WriteLine("Table doesn't exist, please check Azure portal");
            }

            TableClient tableClient = serviceClient.GetTableClient(tableName);
            return tableClient;
        }
       
        public bool CheckResponseDateTime( string traceId)
        {
            TableClient tableClient = GetTableClient(ErpFacadeTableName);
            var existingEntity = tableClient.Query<EESEventEntity>(filter: TableClient.CreateQueryFilter($"TraceID eq {traceId}"));
            if (existingEntity != null)
            {
                foreach (var tableitem in existingEntity)
                {
                    if (tableitem.ResponseDateTime.HasValue)
                    {
                        return true;
                    }
                }
                return false;    
            }
            else { return false; }
        }


    }
}
