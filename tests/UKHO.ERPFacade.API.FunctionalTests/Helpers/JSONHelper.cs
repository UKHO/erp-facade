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
        public List<string> GetProductListProductListFromSAPPayload(List<JsonInputPriceChangeHelper> _jsonInputPriceUpdatehelperString)
        {
          
            List<string> result = new List<string>();
            //code to get Unique list from inout payload
            // List<string> data = new() { _jsonInputPriceUpdatehelperString.productname };
            //  List<string> finalProducts = data.Select(x => x.data).ToList();
            //  List<string> finalProducts = new HashSet<string>(data).ToList();
            // return finalProducts;
            int count = _jsonInputPriceUpdatehelperString.Count;
            for (int i=0; i<count; i++)
            {
                result.Add(_jsonInputPriceUpdatehelperString[i].productname);
               
            }
            List<string> finalProducts = new HashSet<string>(result).ToList();
            return finalProducts;
            
        }

        //internal List<string> GetProductListProductListFromSAPPayload(string jsonPayload) => throw new NotImplementedException();
    }
}


