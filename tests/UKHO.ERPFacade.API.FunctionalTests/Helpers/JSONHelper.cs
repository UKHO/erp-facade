﻿using Newtonsoft.Json.Linq;

using UKHO.ERPFacade.API.FunctionalTests.Model;


namespace UKHO.ERPFacade.API.FunctionalTests.Helpers
{
    public class JSONHelper
    {
        public string getDeserializedString(string filePath)
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

        public List<string> GetProductListProductListFromSAPPayload(List<JsonInputPriceChangeHelper> _jsonInputPriceChangeHelper)
        {
            List<string> result = new List<string>();
            int count = _jsonInputPriceChangeHelper.Count;
            for (int i=0; i<count; i++)
            {
                result.Add(_jsonInputPriceChangeHelper[i].Productname);
               
            }
            List<string> finalProducts = new HashSet<string>(result).ToList();
            return finalProducts;
            
        }

        
    }
}


