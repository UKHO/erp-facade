using Newtonsoft.Json.Linq;

namespace UKHO.ERPFacade.API.Services
{
    public interface IS57Service
    {
        Task ProcessS57Event(JObject encEventJson);
    }
}
