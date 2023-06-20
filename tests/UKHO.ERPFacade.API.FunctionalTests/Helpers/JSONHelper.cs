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
        public List<string> GetProductListProductListFromSAPPayload(List<UoSInputJSONHelper> InputJSONHelper)
        {
            List<string> result = new List<string>();
            //code to get Unique list from inout payload
            return result;
        }
    }
}
