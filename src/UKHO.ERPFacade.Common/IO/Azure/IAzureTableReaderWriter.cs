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
    }
}
