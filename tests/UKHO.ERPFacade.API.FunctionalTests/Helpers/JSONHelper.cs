using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UKHO.ERPFacade.API.FunctionalTests.Configuration;
using UKHO.ERPFacade.API.FunctionalTests.Model;
using UKHO.ERPFacade.Common.Constants;

namespace UKHO.ERPFacade.API.FunctionalTests.Helpers
{
    public class JsonHelper
    {
        private static readonly string projectDir = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory));
        //for local
        //private static readonly string _projectDir = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, "..\\..\\.."));

        public static async Task<List<JsonInputRoSWebhookEvent>> GetEventJsonListUsingFileNameAsync(List<string> fileNames)
        {
            List<JsonInputRoSWebhookEvent> listOfEventJson = new();

            foreach (var filePath in fileNames.Select(fileName => Path.Combine(projectDir, Config.TestConfig.PayloadFolder, Constants.RosPayloadTestDataFolder, fileName)))
            {
                string requestBody;

                using (StreamReader streamReader = new(filePath))
                {
                    requestBody = await streamReader.ReadToEndAsync();
                }
                JsonInputRoSWebhookEvent eventPayloadJson = JsonConvert.DeserializeObject<JsonInputRoSWebhookEvent>(requestBody);
                listOfEventJson.Add(eventPayloadJson);
            }
            return listOfEventJson;
        }

        public static string ModifyMandatoryAttribute(string payload, string attributeName, int index, string action)
        {
            payload = SapXmlHelper.UpdatePermitField(payload, Constants.PermitWithSameKey);
            JObject jsonObject = JObject.Parse(payload);


            string[] types = attributeName.Split(".");

            var tokens = types[0] == Constants.Products
                ? jsonObject.SelectTokens($"$.{Constants.ProductsNode}[{index}].{types[1]}").ToList()
                : types[0] == Constants.UnitsOfSale
                    ? jsonObject.SelectTokens($"$.{Constants.UnitsOfSaleNode}[{index}].{types[1]}").ToList()
                    : jsonObject.SelectTokens($"$.{Constants.UKHOWeekNumber}.{types[0]}").ToList();

            if (action == "Remove")
            {
                foreach (var token in tokens)
                {
                    JProperty parentProperty = (JProperty)token.Parent;
                    parentProperty?.Remove();
                }
            }
            else
            {
                foreach (var token in tokens)
                {
                    token.Replace(JValue.CreateNull());
                }
            }

            payload = jsonObject.ToString();
            return payload;
        }
    }
}
