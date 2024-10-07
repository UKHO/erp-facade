using Azure.Data.Tables;
using UKHO.ERPFacade.Common.Models.TableEntities;

namespace UKHO.ERPFacade.Common.IO.Azure
{
    public interface IAzureTableReaderWriter
    {
        Task UpsertEntity(string correlationId, string tableName, ITableEntity entity);
        Task UpdateEntity<TKey, TValue>(string correlationId, string tableName, KeyValuePair<TKey, TValue>[] entitiesToUpdate);        IList<TableEntity> GetAllEntities(string tableName);
        Task DeleteEntity(string correlationId, string tableName);
        Task<string> GetEntityValue(string correlationId, string tableName, string entityProperty);
    }
}
