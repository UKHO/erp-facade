using UKHO.ERPFacade.Common.Models.TableEntities;

namespace UKHO.ERPFacade.Common.IO.Azure
{
    public interface IAzureTableReaderWriter
    {
        Task UpsertEntity(string correlationId);
        Task UpsertLicenceUpdatedEntity(string correlationId);
        Task<EESEventEntity> GetEntity(string correlationId);
        Task UpdateRequestTimeEntity(string correlationId);
        Task UpdateResponseTimeEntity(string correlationId);
        Task UpdatePublishDateTimeEntity(string correlationId, string eventId);
        void ValidateAndUpdateIsNotifiedEntity();
        IList<PriceChangeMasterEntity> GetMasterEntities(string status, string correlationId = "");
        Task AddPriceChangeEntity(string correlationId, int productCount);
        IList<UnitPriceChangeEntity> GetUnitPriceChangeEventsEntities(string masterCorrId, string status = "", string unitName = "", string eventId = "");
        void AddUnitPriceChangeEntity(string correlationId, string eventId, string unitName);
        void UpdateUnitPriceChangeStatusAndPublishDateTimeEntity(string correlationId, string unitName, string eventId);
        void UpdatePriceMasterStatusAndPublishDateTimeEntity(string correlationId);
        void DeletePriceMasterEntity(string correlationId);
        void DeleteUnitPriceChangeEntityForMasterCorrId(string correlationId);
        IList<EESEventEntity> GetAllEntityForEESTable();
        Task DeleteEESEntity(string correlationId);
        Task UpsertRecordOfSaleEntity(string correlationId);
        Task<RecordOfSaleEventEntity> GetRecordOfSaleEntity(string correlationId, string tableName);
        Task UpdateRecordOfSaleEventStatus(string correlationId);
        Task UpdateLicenceUpdatedEventStatus(string correlationId);
        string GetEntityStatus(string correlationId);
    }
}
