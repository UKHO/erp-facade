using Newtonsoft.Json.Linq;
using UKHO.ERPFacade.Common.Models.TableEntities;

namespace UKHO.ERPFacade.Common.Helpers
{
    public interface IAzureTableStorageHelper
    {
        Task UpsertEntity(JObject eesEvent, string traceId, string correlationId);
        Task<EESEventTable> GetEntity(string traceId);
    }
}
