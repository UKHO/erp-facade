using NUnit.Framework;
using RestSharp;
using Newtonsoft.Json;
using UKHO.ERPFacade.API.FunctionalTests.Model;

namespace UKHO.ERPFacade.API.FunctionalTests.Helpers
{
    public class PriceChangeEndpoint
    {

        private readonly RestClient client;
        private AzureBlobStorageHelper azureBlobStorageHelper;
        private JSONHelper jsonHelper;

        public PriceChangeEndpoint(string url)
        {
            var options = new RestClientOptions(url);
            client = new RestClient(options);
            azureBlobStorageHelper = new();
            jsonHelper = new JSONHelper();
        }

        public async Task<RestResponse> PostPriceChangeResponseAsync(string filePath, string sharedKey)
        {
            string requestBody;

            using (StreamReader streamReader = new StreamReader(filePath))
            {
                requestBody = streamReader.ReadToEnd();
            }
            var request = new RestRequest("/erpfacade/bulkpriceinformation", Method.Post);

            request.AddHeader("Content-Type", "application/json");
            request.AddParameter("application/json", requestBody, ParameterType.RequestBody);
            request.AddQueryParameter("Key", sharedKey);
           RestResponse response = await client.ExecuteAsync(request);
            return response;
        }

        public async Task<RestResponse> PostPriceChangeResponseAsyncWithJson(string filePath, string generatedProductJsonFolder, string sharedKey)
        {
            string requestBody;

            using (StreamReader streamReader = new StreamReader(filePath))
            {
                requestBody = streamReader.ReadToEnd();
            }
            var request = new RestRequest("/erpfacade/bulkpriceinformation", Method.Post);

            request.AddHeader("Content-Type", "application/json");
            request.AddQueryParameter("Key", sharedKey);
            request.AddParameter("application/json", requestBody, ParameterType.RequestBody);

            RestResponse response = await client.ExecuteAsync(request);

            //Logic to verify JSON
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                List<UoSInputJSONHelper> jsonPayload = JsonConvert.DeserializeObject <List<UoSInputJSONHelper>>(requestBody);

                //1.1 get the corelation id from response headers.
                var correlationID = "5ae46db3-d184-49e4-a933-ff9bdf2fe573";

                /*1.2 Check list of folders created for the unique products.
                    pt#1: unique product list from input JSON */
                List<string> UniquePdtFromInputPayload = jsonHelper.GetProductListProductListFromSAPPayload(jsonPayload);

                    /*pt#2: Azure container-->pricechangeBlob--><CorrelationID>
                          get the list from Azure sub containers */
                List<string> UniquePdtFromAzureStorage = azureBlobStorageHelper.GetProductListFromBlobContainerAsync(correlationID).Result;
                //pt#3: compare listed from pt#1 and pt#2   
                Assert.That(UniquePdtFromInputPayload.SequenceEqual(UniquePdtFromAzureStorage), Is.True, "Slicing is correct");

                foreach (string pdt in UniquePdtFromAzureStorage)
                {
                    //1.3 for each unique products - download the product.JSON
                    string generatedProductJsonFile = azureBlobStorageHelper.DownloadJSONFromAzureBlob(generatedProductJsonFolder, correlationID, pdt, "ProductChange");

                    //1.3.1 compare corr Id from JSON corelation id from response headers              
                    //1.3.2 Check for unique effective date array for every product
                    //1.3.3 Check for unique duration for every effective date
                    //1.3.4 Check RRP field values for all unitofsale prices

                }
            }
            return response;
        }
    }
}
