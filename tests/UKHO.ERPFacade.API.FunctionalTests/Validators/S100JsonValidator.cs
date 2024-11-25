using Newtonsoft.Json.Linq;
using UKHO.ERPFacade.Common.Constants;

namespace UKHO.ERPFacade.API.FunctionalTests.Validators
{
    public class S100JsonValidator
    {
        public async Task<bool> VerifyS100UnitOfSaleUpdatedEventJson(string filePath)
        {
            string unitOfSaleUpdatedEvent = await File.ReadAllTextAsync(filePath);

            JObject eventJson = JObject.Parse(unitOfSaleUpdatedEvent);

            return eventJson["type"].ToString() == EventTypes.S100UnitOfSaleUpdatedEventType &&
                   eventJson["source"].ToString() == JsonFields.Source &&
                   DateTime.Parse(eventJson["time"].ToString()).Date.Equals(DateTime.Now.Date);
        }
    }
}
