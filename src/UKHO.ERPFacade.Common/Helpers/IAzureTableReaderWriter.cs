using Newtonsoft.Json.Linq;
using UKHO.ERPFacade.Common.Models.TableEntities;

namespace UKHO.ERPFacade.Common.Helpers
{
    public interface IAzureTableReaderWriter
    {
        Task UpsertEntity(JObject eesEvent, string traceId, string correlationId);
        Task<EESEventEntity> GetEntity(string traceId);
    }
}
