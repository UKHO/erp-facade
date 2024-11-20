using Azure.Data.Tables;

namespace UKHO.ERPFacade.Common.Operations.IO.Azure
{
    public interface IAzureTableReaderWriter
    {
        Task UpsertEntityAsync(ITableEntity entity);
        Task UpdateEntityAsync<TKey, TValue>(string partitionKey, string rowKey, KeyValuePair<TKey, TValue>[] entitiesToUpdate);
        Task<TableEntity> GetEntityAsync(string partitionKey, string rowKey);
        Task<IList<TableEntity>> GetFilteredEntitiesAsync(Dictionary<string, string> filters);
        Task DeleteEntityAsync(string partitionKey, string rowKey);
    }
}
