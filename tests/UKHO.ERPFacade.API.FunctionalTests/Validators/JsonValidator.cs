using Newtonsoft.Json.Linq;
using UKHO.ERPFacade.Common.Constants;

namespace UKHO.ERPFacade.API.FunctionalTests.Validators
{
    public class JsonValidator
    {
        public async Task<bool> VerifyUnitOfSaleEvent(string path)
        {
            string unitOfSaleEvent = await File.ReadAllTextAsync(path);
            JObject jsonObj = JObject.Parse(unitOfSaleEvent);
            if (jsonObj["type"].ToString() != EventTypes.S100UnitOfSaleUpdatedEventType) return false;
            if (jsonObj["source"].ToString() != JsonFields.Source) return false;
            DateTime date = DateTime.Parse(jsonObj["time"].ToString()).Date;
            if (!date.Equals(DateTime.Now.Date)) return false;
            return true;
        }
    }
}
