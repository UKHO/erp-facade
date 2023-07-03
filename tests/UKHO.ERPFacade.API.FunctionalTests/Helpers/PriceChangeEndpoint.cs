using NUnit.Framework;
using RestSharp;
using Newtonsoft.Json;
using UKHO.ERPFacade.API.FunctionalTests.Model;

namespace UKHO.ERPFacade.API.FunctionalTests.Helpers
{
    public class PriceChangeEndpoint
    {

        private readonly RestClient client;
        private readonly JSONHelper _jSONHelper;
        //private readonly string _projectDir = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, "..\\..\\.."));

        public JsonOutputPriceChangeHelper _jsonOuputPriceChangeHelper { get; set; }
        public List<JsonInputPriceChangeHelper> _jsonInputPriceChangeHelper { get; set; }
        

        private AzureBlobStorageHelper azureBlobStorageHelper;
        List<string> UniquePdtFromInputPayload;




        public PriceChangeEndpoint(string url)
        {
            var options = new RestClientOptions(url);
            client = new RestClient(options);
            azureBlobStorageHelper = new();
            _jSONHelper = new JSONHelper();

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
            return response;
        }

        public async Task<RestResponse> PostPriceChangeResponseAsyncForJSON(string filePath, string generatedProductJsonFolder, string sharedKey)
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
                Thread.Sleep(30000);

                responseHeadercorrelationID = getResponseHeaderCorrelationID(response);
                UniquePdtFromInputPayload = getProductListFromInputPayload(filePath);

                List<string> UniquePdtFromAzureStorage = azureBlobStorageHelper.GetProductListFromBlobContainerAsync(responseHeadercorrelationID).Result;
                
                Assert.That(UniquePdtFromInputPayload.Count.Equals(UniquePdtFromAzureStorage.Count), Is.True, "Slicing is not correct");
                       
                foreach (string products in UniquePdtFromAzureStorage)
                {
                    string generatedProductJsonFile = azureBlobStorageHelper.DownloadJSONFromAzureBlob(generatedProductJsonFolder, responseHeadercorrelationID, products, "ProductChange");
                    Console.WriteLine(generatedProductJsonFile);


                    JsonOutputPriceChangeHelper desiailzedProductOutput = getDeserializedProductJson(generatedProductJsonFile);
                    string correlation_ID = desiailzedProductOutput.data.correlationId;

                    Assert.That(correlation_ID.Equals(responseHeadercorrelationID), Is.True, "response header corerelationId is same as generated product correlation id");
                    unitsOfSalePricesData[] data = desiailzedProductOutput.data.unitsOfSalePrices;


                    EffectiveDatesPerProductPC effectiveDate = new EffectiveDatesPerProductPC();
                    List<EffectiveDatesPerProductPC> effectiveDates = new List<EffectiveDatesPerProductPC>();
                    foreach (unitsOfSalePricesData unitOfSalesPrice in data)
                    {

                        foreach (var prices in unitOfSalesPrice.price)
                        {

                            foreach (PriceDurationsPriceChangeOutput priceDuration in prices.standard.priceDurations)
                            {
                                effectiveDate = new EffectiveDatesPerProductPC();
                                effectiveDate.ProductName = unitOfSalesPrice.unitName;
                                effectiveDate.EffectiveDates = prices.effectiveDate;
                                effectiveDate.Duration = priceDuration.numberOfMonths;
                                effectiveDate.rrp = priceDuration.rrp;
                                effectiveDates.Add(effectiveDate);
                            }
                        }

                    }

                   
                    var inputData = _jsonInputPriceChangeHelper.Select(x => new
                    {
                        x.Productname,
                        EffectiveDateTime = new DateTime(Convert.ToInt32(x.Effectivedate.ToString().Substring(0, 4)),
                        Convert.ToInt32(x.Effectivedate.ToString().Substring(4, 2)), Convert.ToInt32(x.Effectivedate.ToString().Substring(6, 2))),
                        EffectivePrice = x.Price,
                        Duration = x.Duration,

                        FutureDateTime = x.Futuredate != null ? new DateTime(Convert.ToInt32(x.Futuredate.ToString().Substring(0, 4)),
                       Convert.ToInt32(x.Futuredate.ToString().Substring(4, 2)), Convert.ToInt32(x.Futuredate.ToString().Substring(6, 2))) : new DateTime(),
                        FuturePrice = x.Futureprice
                    }).ToList();

                    foreach (var SAPProduct in inputData)
                    {
                        Console.WriteLine(Environment.NewLine);
                        Console.WriteLine(string.Format("Comparing product - {0} for Effective dates and prices", SAPProduct.Productname));

                        EffectiveDatesPerProductPC? findProduct = effectiveDates.FirstOrDefault(x => x.ProductName == SAPProduct.Productname
                                                         && x.EffectiveDates.Date == SAPProduct.EffectiveDateTime.Date
                                                         && x.rrp == SAPProduct.EffectivePrice && x.Duration == SAPProduct.Duration);


                        if (findProduct != null)
                        {
                            // Match Found for Product , Date and price combination
                            Console.WriteLine(string.Format("Product - {0} found in Final UOS for Effective Date - {1}, Duration - {2} and Price - {3}", SAPProduct.Productname, SAPProduct.EffectiveDateTime.Date, SAPProduct.Duration, SAPProduct.EffectivePrice));
                        }
                        else
                        {
                            // Match not Found for Product , Date and price combination
                            Console.WriteLine(string.Format("Product - {0} Not found in Final UOS for Effective Date - {1}, Duration - {2} and Price - {3}", SAPProduct.Productname, SAPProduct.EffectiveDateTime.Date, SAPProduct.Duration, SAPProduct.EffectivePrice));
                        }

                        if (SAPProduct.FutureDateTime != new DateTime())
                        {
                            EffectiveDatesPerProductPC? findFutureProduct = effectiveDates.FirstOrDefault(x => x.ProductName == SAPProduct.Productname
                                                         && x.EffectiveDates.Date == SAPProduct.FutureDateTime.Date
                                                         && x.rrp == SAPProduct.FuturePrice);

                            if (findFutureProduct != null)
                            {
                                // Match Found for Product , Date and price combination
                                Console.WriteLine(string.Format("Product - {0} found in Final UOS for Future Date - {1}, Duration - {2} and Price - {3}", SAPProduct.Productname, SAPProduct.FutureDateTime.Date, SAPProduct.Duration, SAPProduct.FuturePrice));
                            }
                            else
                            {
                                // Match not Found for Product , Date and price combination
                                Console.WriteLine(string.Format("Product - {0} Not found in Final UOS for Future Date - {1}, Duration - {2} and Price - {3}", SAPProduct.Productname, SAPProduct.FutureDateTime.Date, SAPProduct.Duration, SAPProduct.FuturePrice));
                            }
                        }

                    }



                }

            }
            return response;
        }

        private List<string> getProductListFromInputPayload(string inputJSONFilePath)
        {
            string jsonPayload = _jSONHelper.getDeserializedString(inputJSONFilePath);
            _jsonInputPriceChangeHelper = JsonConvert.DeserializeObject<List<JsonInputPriceChangeHelper>>(jsonPayload);
            UniquePdtFromInputPayload = _jSONHelper.GetProductListProductListFromSAPPayload(_jsonInputPriceChangeHelper);
            return UniquePdtFromInputPayload;
        }

        private JsonOutputPriceChangeHelper getDeserializedProductJson(string generatedProductJson)
        {
           
            string jsonString = _jSONHelper.getDeserializedString(generatedProductJson);
            _jsonOuputPriceChangeHelper = JsonConvert.DeserializeObject<JsonOutputPriceChangeHelper>(jsonString);
            return _jsonOuputPriceChangeHelper;
        }

        private static String getResponseHeaderCorrelationID(RestResponse response)
        {
            string correlationID = response.Headers.ToList().Find(x => x.Name == "_X-Correlation-ID").Value.ToString();
            Console.WriteLine(correlationID);
            return correlationID;
        }

    }


}

