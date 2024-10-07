using Newtonsoft.Json.Linq;

namespace UKHO.ERPFacade.API.Services
{
    public interface IRecordOfSaleService
    {
        Task ProcessRecordOfSaleEvent(string correlationId, JObject recordOfSaleEventJson, string eventId);
    }
}
