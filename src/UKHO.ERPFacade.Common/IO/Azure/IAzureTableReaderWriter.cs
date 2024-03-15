using UKHO.ERPFacade.Common.Models.TableEntities;

namespace UKHO.ERPFacade.Common.IO.Azure
{
    public interface IAzureTableReaderWriter
    {
        Task UpsertEntity(string correlationId);
        Task UpsertLicenceUpdatedEntity(string correlationId);
        Task<EESEventEntity> GetEntity(string correlationId);
        Task UpdateRequestTimeEntity(string correlationId);        
        IList<EESEventEntity> GetAllEntityForEESTable();
        Task DeleteEESEntity(string correlationId);
        Task UpsertRecordOfSaleEntity(string correlationId);
        Task<RecordOfSaleEventEntity> GetRecordOfSaleEntity(string correlationId, string tableName);
        Task UpdateRecordOfSaleEventStatus(string correlationId);
        Task UpdateLicenceUpdatedEventStatus(string correlationId);
        string GetEntityStatus(string correlationId);
    }
}
