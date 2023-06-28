using Newtonsoft.Json.Linq;

using UKHO.ERPFacade.API.FunctionalTests.Model;


namespace UKHO.ERPFacade.API.FunctionalTests.Helpers
{
    public class JSONHelper
    {
        public string getDeserializedString(String filePath)
        {
            string requestBody;

            using (StreamReader streamReader = new StreamReader(filePath))
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
        public List<string> GetProductListProductListFromSAPPayload(List<JsonInputPriceChangeHelper> _jsonInputPriceChangehelperString)
        {
          
            List<string> result = new List<string>();
            //code to get Unique list from inout payload
            // List<string> data = new() { _jsonInputPriceUpdatehelperString.productname };
            //  List<string> finalProducts = data.Select(x => x.data).ToList();
            //  List<string> finalProducts = new HashSet<string>(data).ToList();
            // return finalProducts;
            int count = _jsonInputPriceChangehelperString.Count;
            for (int i=0; i<count; i++)
            {
                result.Add(_jsonInputPriceChangehelperString[i].productname);
               
            }
            List<string> finalProducts = new HashSet<string>(result).ToList();
            return finalProducts;
            
        }

        //internal List<string> GetProductListProductListFromSAPPayload(string jsonPayload) => throw new NotImplementedException();
    }
}


