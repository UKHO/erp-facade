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
    }
}


