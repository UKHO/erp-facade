using Newtonsoft.Json.Linq;
using UKHO.ERPFacade.Common.Models.TableEntities;

namespace UKHO.ERPFacade.Common.IO.Azure
{
    public interface IAzureTableReaderWriter
    {
        Task UpsertEntity(string correlationId);
        Task<EESEventEntity> GetEntity(string correlationId);
        Task UpdateRequestTimeEntity(string correlationId);
        Task UpdateResponseTimeEntity(string correlationId);
        void ValidateAndUpdateIsNotifiedEntity();
        IList<PriceChangeMasterEntity> GetMasterEntities(string status, string correlationId = "");
        Task AddPriceChangeEntity(string correlationId);
        IList<UnitPriceChangeEntity> GetUnitPriceChangeEventsEntities(string masterCorrId, string status = "", string unitName = "", string eventId = "");
        void AddUnitPriceChangeEntity(string correlationId, string eventId, string unitName);
        void UpdateUnitPriceChangeStatusEntity(string correlationId, string unitName, string eventId);
        void UpdatePriceMasterStatusEntity(string correlationId);
        void DeletePriceMasterEntity(string correlationId);
        void DeleteUnitPriceChangeEntityForMasterCorrId(string correlationId);
        IList<EESEventEntity> GetAllEntityForEESTable();
        Task DeleteEESEntity(string correlationId);
    }
}
