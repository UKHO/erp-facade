using Newtonsoft.Json.Linq;

namespace UKHO.ERPFacade.API.Services
{
    public interface IS57Service
    {
        Task ProcessEncContentPublishedEvent(string correlationId, JObject encEventJson);
    }
}
