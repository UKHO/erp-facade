using NUnit.Framework;
using System;
using Newtonsoft.Json;
using RestSharp;
using UKHO.ERPFacade.API.FunctionalTests.Model;

using UKHO.ERPFacade.API.FunctionalTests.Helpers;
using Microsoft.Extensions.Options;
using UKHO.ERPFacade.API.FunctionalTests.Model.Latest_Model;

namespace UKHO.ERPFacade.API.FunctionalTests.Helpers
{
    public class PriceChangeEndpoint
    {

        private readonly RestClient client;
        private readonly JSONHelper _jSONHelper;
        private readonly string _projectDir = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, "..\\..\\.."));

        public JsonOutputPriceChangeHelper _jsonOuputPriceChangeHelper { get; set; }
        public List<JsonInputPriceChangeHelper> _jsonInputPriceChangeHelper { get; set; }
        
        
        private AzureBlobStorageHelper azureBlobStorageHelper;
        private JSONHelper jsonHelper;
        List<string> UniquePdtFromInputPayload;
        

        public BulkPriceUpdateEndpoint(string url)

        public PriceChangeEndpoint(string url)
        {
            var options = new RestClientOptions(url);
            client = new RestClient(options);
            _jSONHelper = new JSONHelper();
            azureBlobStorageHelper = new();
            jsonHelper = new JSONHelper();
        }
        public void PostBulkPriceUpdateResponse(string url)
        {
            Console.WriteLine("In Bulk Price Update");
            var options = new RestClientOptions(url);
            return;

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
            request.AddQueryParameter("Key", sharedKey);
            request.AddParameter("application/json", requestBody, ParameterType.RequestBody);
            RestResponse response = await client.ExecuteAsync(request);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                Thread.Sleep(5000);
                
                responseHeadercorrelationID = responseHeaderCorrelationID(response);
                
                

                /*1.2 Check list of folders created for the unique products.
                    pt#1: unique product list from input JSON */
                UniquePdtFromInputPayload =inputPayloadProducts();

               // string correlation_ID = desiailzedoutput.data.correlationId;
                
                  /*pt#2: Azure container-->pricechangeBlob--><CorrelationID>
                      get the list from Azure sub containers */
                List<string> UniquePdtFromAzureStorage = azureBlobStorageHelper.GetProductListFromBlobContainerAsync(responseHeadercorrelationID).Result;
                //pt#3: compare listed from pt#1 and pt#2
                Assert.That(UniquePdtFromInputPayload.Count.Equals(UniquePdtFromAzureStorage.Count), Is.True, "Slicing is correct");
                // for loop for iteration for each product json            
                foreach (string pdt in UniquePdtFromAzureStorage)
                {
                    string generatedProductJsonFile = azureBlobStorageHelper.DownloadJSONFromAzureBlob(generatedProductJsonFolder, responseHeadercorrelationID, pdt, "ProductChange");
                    Console.WriteLine(generatedProductJsonFile);

                    JsonOutputPriceChangeHelper desiailzedoutput = productOutputDeserialize(generatedProductJsonFile);
                    string correlation_ID = desiailzedoutput.data.correlationId;

                    Assert.That(correlation_ID.Equals(responseHeadercorrelationID), Is.True, "response header corerelationId is same as generated product correlation id");
                    List<UnitsOfSalePricePriceChangeOutput> data = desiailzedoutput.data.unitsOfSalePrices;
                    List<EffectiveDatesPerProductPC> effectiveDates = new List<EffectiveDatesPerProductPC>();

                    if (correlation_ID.Equals(responseHeadercorrelationID))
                    {
                        foreach (UnitsOfSalePricePriceChangeOutput unit in data)
                        {
                            foreach (var prices in unit.price)
                            {

                                

                                EffectiveDatesPerProductPC effectiveDate = new EffectiveDatesPerProductPC();
                                    effectiveDate.ProductName = unit.unitName;
                                    effectiveDate.EffectiveDates = prices.effectiveDate;
                                foreach (var durations in prices.standard.priceDurations)
                                {


                                    effectiveDate.Duration = durations.numberOfMonths;
                                    effectiveDate.rrp = durations.rrp;
                                   
                                    effectiveDates.Add(effectiveDate);
                                    
                                }
                                    
                                
                            }
                            foreach (IGrouping<string, EffectiveDatesPerProductPC> date in effectiveDates.GroupBy(x => x.ProductName))
                            {
                                var product = date.Key;
                                var effdates = date.Select(x => x.EffectiveDates).ToList();
                                var distinctEffDates = effdates.Distinct().ToList();
                                var duration = date.Select(x => x.Duration).ToList();
                                var distinctDurations = duration.Distinct().ToList();
                                var rrp = date.Select(x => x.rrp).ToList();
                                var distinctrrp = rrp.Distinct().ToList();
                                Assert.That(effdates.All(distinctEffDates.Contains) && distinctEffDates.All(effdates.Contains), Is.True, "Effective dates for {0} are not distinct.");
                                Assert.That(duration.All(distinctDurations.Contains) && distinctDurations.All(duration.Contains), Is.True, "Duration for {0} are not distinct.");
                                Assert.That(rrp.All(distinctrrp.Contains) && distinctrrp.All(rrp.Contains), Is.True, "RRP for {0} are not distinct.");
                            }
                        }
                        


                    }
                }

            }
            return response;
        }

        private List<string> inputPayloadProducts()
        {
            string inputJSONFilePath = Path.Combine(_projectDir, Config.TestConfig.PayloadFolder, Config.TestConfig.PriceChangePayloadFileName);
            string jsonPayload = _jSONHelper.getDeserializedString(inputJSONFilePath);

            _jsonInputPriceChangeHelper = JsonConvert.DeserializeObject<List<JsonInputPriceChangeHelper>>(jsonPayload);
            UniquePdtFromInputPayload= _jSONHelper.GetProductListProductListFromSAPPayload(_jsonInputPriceChangeHelper);
            return UniquePdtFromInputPayload;
        }

        private JsonOutputPriceChangeHelper productOutputDeserialize(string generatedProductJson)
        {
            //string filePathProductJSON = Path.Combine(generatedProductJson);
            string jsonString = _jSONHelper.getDeserializedString(generatedProductJson);
            _jsonOuputPriceChangeHelper = JsonConvert.DeserializeObject<JsonOutputPriceChangeHelper>(jsonString);
            return _jsonOuputPriceChangeHelper;
        }

        private static String responseHeaderCorrelationID(RestResponse response)
        {
            string correlationID = response.Headers.ToList().Find(x => x.Name == "_X-Correlation-ID").Value.ToString();
            Console.WriteLine(correlationID);
            return correlationID;
        }

    }

        
    }

