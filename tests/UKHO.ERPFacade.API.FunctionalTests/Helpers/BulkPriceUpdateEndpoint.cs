using NUnit.Framework;
using System;
using Newtonsoft.Json;
using RestSharp;
using UKHO.ERPFacade.API.FunctionalTests.Model;

using UKHO.ERPFacade.API.FunctionalTests.Helpers;
using Microsoft.Extensions.Options;

namespace UKHO.ERPFacade.API.FunctionalTests.Helpers
{
    public class BulkPriceUpdateEndpoint
    {

        private readonly RestClient client;
        private readonly JSONHelper _jSONHelper;
        private readonly string _projectDir = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, "..\\..\\.."));
        
        public  JsonOutputPriceChangeHelper JsonPayloadProductHelper { get;  set; }
        public List<JsonInputPriceChangeHelper> _jsonInputPriceChangeHelper { get; set; }
        
        private AzureBlobStorageHelper azureBlobStorageHelper;
        private JSONHelper jsonHelper;
        

        public BulkPriceUpdateEndpoint(string url)
        {
            var options = new RestClientOptions(url);
            client = new RestClient(options);
            _jSONHelper = new JSONHelper();
             azureBlobStorageHelper = new();
             jsonHelper = new JSONHelper();
        }
        public void  PostBulkPriceUpdateResponse(string url)
        {
            Console.WriteLine("In Bulk Price Update");
            var options = new RestClientOptions(url);
            return;

        }
        public void PostBulkPriceUpdateResponseJSON()
        {
            Console.WriteLine("In Bulk Price Update");
            return;
        }
        public async Task<RestResponse> PostBPUpdateResponseAsync(string filePath, string token)
        {
            string requestBody;

            using (StreamReader streamReader = new StreamReader(filePath))
            {
                requestBody = streamReader.ReadToEnd();
            }
            var request = new RestRequest("/erpfacade/bulkpriceinformation", Method.Post);

            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Authorization", "Bearer " + token);
            request.AddParameter("application/json", requestBody, ParameterType.RequestBody);

            RestResponse response = await client.ExecuteAsync(request);
            return response;
        }

        public async Task<RestResponse> PostBPUpdateResponseAsyncWithJson(string filePath, string generatedProductJsonFolder, string token)
        {
            string requestBody;

            using (StreamReader streamReader = new StreamReader(filePath))
            {
                requestBody = streamReader.ReadToEnd();
            }
            var request = new RestRequest("/erpfacade/bulkpriceinformation", Method.Post);
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Authorization", "Bearer " + token);
            request.AddParameter("application/json", requestBody, ParameterType.RequestBody);

            RestResponse response = await client.ExecuteAsync(request);
            return response;
        }

        public async Task<RestResponse> PostBPUResponseAsyncForJSON(string filePath, string generatedProductJsonFolder, string token)
        {
            string requestBody;
            string responseHeadercorrelationID;

            using (StreamReader streamReader = new StreamReader(filePath))
            {
                requestBody = streamReader.ReadToEnd();
            }
            var request = new RestRequest("/erpfacade/bulkpriceinformation", Method.Post);
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Authorization", "Bearer " + token);
            request.AddParameter("application/json", requestBody, ParameterType.RequestBody);
            RestResponse response = await client.ExecuteAsync(request);
            
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                JsonOutputPriceChangeHelper desiailzedoutput= productOutputDeserialize();
                responseHeadercorrelationID = responseHeaderCorrelationID(response);

                /*1.2 Check list of folders created for the unique products.
                    pt#1: unique product list from input JSON */
                string inputJSONFilePath = Path.Combine(_projectDir, Config.TestConfig.PayloadFolder, Config.TestConfig.BPUpdatePayloadFileName);
                string jsonPayload = _jSONHelper.getDeserializedString(inputJSONFilePath);

                _jsonInputPriceChangeHelper = JsonConvert.DeserializeObject<List<JsonInputPriceChangeHelper>>(jsonPayload);
                List<string> UniquePdtFromInputPayload = _jSONHelper.GetProductListProductListFromSAPPayload(_jsonInputPriceChangeHelper);
                string correlation_ID = desiailzedoutput.data.correlationId;
                List<string> UniquePdtFromAzureStorage = azureBlobStorageHelper.GetProductListFromBlobContainerAsync(correlation_ID).Result;

                // for loop for iteration for each product json

                Assert.That(UniquePdtFromInputPayload.Count.Equals(UniquePdtFromAzureStorage.Count), Is.True, "Slicing is correct");
                foreach (string pdt in UniquePdtFromAzureStorage)
                {
                    string generatedProductJsonFile = azureBlobStorageHelper.DownloadJSONFromAzureBlob(generatedProductJsonFolder, correlation_ID, pdt, "ProductChange");
                    Console.WriteLine(generatedProductJsonFile);
                    Assert.That(correlation_ID.Equals(responseHeadercorrelationID), Is.True, "response header corerelationId is same as generated product correlation id");
                    if (correlation_ID.Equals(responseHeadercorrelationID))
                    {


                    }
                }


            }



            // RestResponse response = await client.ExecuteAsync(request);

            //Logic to verify JSON
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                List<UoSInputJSONHelper> jsonPayload = JsonConvert.DeserializeObject <List<UoSInputJSONHelper>>(requestBody);

                //1.1 get the corelation id from response headers.
                var correlationID = "5ae46db3-d184-49e4-a933-ff9bdf2fe573";

                /*1.2 Check list of folders created for the unique products.
                    pt#1: unique product list from input JSON */
               // List<string> UniquePdtFromInputPayload = jsonHelper.GetProductListProductListFromSAPPayload(jsonPayload);

                    /*pt#2: Azure container-->pricechangeBlob--><CorrelationID>
                          get the list from Azure sub containers */
                List<string> UniquePdtFromAzureStorage = azureBlobStorageHelper.GetProductListFromBlobContainerAsync(correlationID).Result;
                //pt#3: compare listed from pt#1 and pt#2   
               // Assert.That(UniquePdtFromInputPayload.SequenceEqual(UniquePdtFromAzureStorage), Is.True, "Slicing is correct");
            RestResponse response = await client.ExecuteAsync(request);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                 responseHeadercorrelationID = responseHeaderCorrelationID(response);
                
            }
            JsonPayloadProductHelper jsonPayloadProduct = JsonConvert.DeserializeObject<JsonPayloadProductHelper>();
             string correlationIdproduct = jsonPayloadProduct.Data.correlationId;


                foreach (string pdt in UniquePdtFromAzureStorage)
                {
                    //1.3 for each unique products - download the product.JSON
                   // string generatedProductJsonFile = azureBlobStorageHelper.DownloadJSONFromAzureBlob(generatedProductJsonFolder, correlationID, pdt, "ProductChange");

                    //1.3.1 compare corr Id from JSON corelation id from response headers              
                    //1.3.2 Check for unique effective date array for every product
                    //1.3.3 Check for unique duration for every effective date
                    //1.3.4 Check RRP field values for all unitofsale prices

            return response;
        }

        private JsonOutputPriceChangeHelper productOutputDeserialize()
        {
            string filePathProductJSON = "D:\\UpdatedERP\\tests\\UKHO.ERPFacade.API.FunctionalTests\\ERPFacadeGeneratedProductJSON\\ERPFacadeGeneratedProductJSON.JSON";
            var jsonString = _jSONHelper.getDeserializedString(filePathProductJSON);
            JsonPayloadProductHelper = JsonConvert.DeserializeObject<JsonOutputPriceChangeHelper>(jsonString);
            return JsonPayloadProductHelper;
        }

        private static String responseHeaderCorrelationID(RestResponse response)
        {
            string correlationID = response.Headers.ToList().Find(x => x.Name == "_X-Correlation-ID").Value.ToString();
            Console.WriteLine(correlationID);
            return correlationID;
        }
    }
}
