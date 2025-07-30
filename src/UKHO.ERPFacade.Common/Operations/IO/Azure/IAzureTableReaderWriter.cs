using Azure.Data.Tables;

namespace UKHO.ERPFacade.Common.Operations.IO.Azure
{
    public interface IAzureTableReaderWriter
    {
        Task UpsertEntityAsync(ITableEntity entity);
        Task UpdateEntityAsync(string partitionKey, string rowKey, Dictionary<string, object> entitiesToUpdate);
        Task<TableEntity> GetEntityAsync(string partitionKey, string rowKey);
        Task<IList<TableEntity>> GetFilteredEntitiesAsync(Dictionary<string, string> filters);
        Task DeleteEntityAsync(string partitionKey, string rowKey);
    }
}
