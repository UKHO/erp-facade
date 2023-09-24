using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UKHO.ERPFacade.API.FunctionalTests.Configuration;
using UKHO.ERPFacade.API.FunctionalTests.Model;

namespace UKHO.ERPFacade.API.FunctionalTests.Helpers
{
    public class JsonHelper
    {
        private static readonly string _projectDir = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory));
        //for local
        //private static readonly string _projectDir = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, "..\\..\\.."));

        public string GetDeserializedString(string filePath)
        {
            string requestBody;

            using (StreamReader streamReader = new(filePath))
            {
                requestBody = streamReader.ReadToEnd();
            }
            return requestBody;
        }

        public static string replaceCorrID(string requestBody, string corrIdGeneratedInWebhookEndpoint)
        {
            JArray jsonArray = JArray.Parse(requestBody);
            foreach (JObject jObj in jsonArray)
            {
                jObj["corrid"] = corrIdGeneratedInWebhookEndpoint;
            }
            string updatedUoSRequestBody = jsonArray.ToString();
            return updatedUoSRequestBody;
        }

        public static List<string> GetProductListFromSAPPayload(List<JsonInputPriceChangeHelper> _jsonInputPriceChangeHelper)
        {
            List<string> result = new List<string>();
            int count = _jsonInputPriceChangeHelper.Count;
            for (int i = 0; i < count; i++)
            {
                result.Add(_jsonInputPriceChangeHelper[i].Productname);

            }
            List<string> finalProducts = new HashSet<string>(result).ToList();
            return finalProducts;
        }

        public static async Task<List<JsonInputRoSWebhookEvent>> GetEventJsonListUsingFileNameAsync(List<string> fileNames)
        {
            List<JsonInputRoSWebhookEvent> listOfEventJsons = new();

            foreach (var filePath in fileNames.Select(fileName => Path.Combine(_projectDir, Config.TestConfig.PayloadFolder, "RoSPayloadTestData", fileName)))
            {
                string requestBody;

                using (StreamReader streamReader = new(filePath))
                {
                    requestBody = await streamReader.ReadToEndAsync();
                }
                JsonInputRoSWebhookEvent eventPayloadJson = JsonConvert.DeserializeObject<JsonInputRoSWebhookEvent>(requestBody);
                listOfEventJsons.Add(eventPayloadJson);
            }
            return listOfEventJsons;
        }
    }
}
