using Newtonsoft.Json.Linq;

namespace UKHO.ERPFacade.API.Services
{
    public interface ILicenseUpdatedService
    {
        Task ProcessLicenseUpdatedPublishedEvent(string correlationId, JObject licenceUpdatedEventJson);
    }
}
