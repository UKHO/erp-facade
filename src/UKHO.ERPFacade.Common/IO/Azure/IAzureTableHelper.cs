using Azure.Data.Tables;

namespace UKHO.ERPFacade.Common.IO.Azure
{
    public interface IAzureTableHelper
    {
        Task UpsertEntity(ITableEntity entity);
        Task UpdateEntity<TKey, TValue>(string partitionKey, string rowKey, KeyValuePair<TKey, TValue>[] entitiesToUpdate);
        Task<TableEntity> GetEntity(string partitionKey, string rowKey);
        IList<TableEntity> GetAllEntities(string tableName);
        Task DeleteEntity(string correlationId, string tableName);
    }
}
