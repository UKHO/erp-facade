using Newtonsoft.Json;
using NUnit.Framework;
using RestSharp;
using UKHO.ERPFacade.API.FunctionalTests.Helpers;
using UKHO.ERPFacade.API.FunctionalTests.Model;

namespace UKHO.ERPFacade.API.FunctionalTests.Service
{
    public class PriceChangeEndpoint
    {
        private readonly RestClient _client;
        private readonly JsonHelper _jsonHelper;
        private readonly AzureBlobStorageHelper _azureBlobStorageHelper;
        private const string ErpFacadeBulkPriceInformationEndPoint = "/erpfacade/bulkpriceinformation";
        public const string XCorrelationIdHeaderKey = "_X-Correlation-ID";

        private List<string> _uniquePdtFromInputPayload;
        public List<JsonInputPriceChangeHelper> _jsonInputPriceChangeHelper { get; set; }

        public AzureBlobStorageHelper AzureBlobStorageHelper => _azureBlobStorageHelper;

        public PriceChangeEndpoint(string url)
        {
            var options = new RestClientOptions(url);
            _client = new RestClient(options);
            _azureBlobStorageHelper = new();
            _jsonHelper = new();
        }

        public async Task<RestResponse> PostPriceChangeResponseAsync(string filePath, string sharedKey)
        {
            RestRequest request = ErpFacadeBulkPriceInformationRequest(filePath, sharedKey);
            RestResponse response = await _client.ExecuteAsync(request);
            return response;
        }

        public async Task<RestResponse> PostPriceChangeResponseAsyncWithJson(string filePath, string generatedProductJsonFolder, string sharedKey)
        {
            RestRequest request = ErpFacadeBulkPriceInformationRequest(filePath, sharedKey);
            RestResponse response = await _client.ExecuteAsync(request);
            return response;
        }

        public async Task<RestResponse> PostPriceChangeResponseAsyncForJSON(string filePath, string generatedProductJsonFolder, string sharedKey)
        {
            RestRequest request = ErpFacadeBulkPriceInformationRequest(filePath, sharedKey);
            RestResponse response = await _client.ExecuteAsync(request);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                Thread.Sleep(120000);

                string responseHeaderCorrelationId = GetResponseHeaderCorrelationId(response);
                List<string> uniqueProductFromInputPayload = GetProductListFromInputPayload(filePath);

                List<string> uniqueProductFromAzureStorage = AzureBlobStorageHelper.GetProductListFromBlobContainerAsync(responseHeaderCorrelationId).Result;

                Assert.That(uniqueProductFromInputPayload.Count.Equals(uniqueProductFromAzureStorage.Count), Is.True, "Slicing is not correct");

                foreach (string products in uniqueProductFromAzureStorage)
                {
                    string generatedProductJsonFile = AzureBlobStorageHelper.DownloadJSONFromAzureBlob(generatedProductJsonFolder, responseHeaderCorrelationId, products, "ProductChange");
                    Console.WriteLine(generatedProductJsonFile);

                    JsonOutputPriceChangeHelper deserializedProductOutput = GetDeserializedProductJson(generatedProductJsonFile);
                    string correlationId = deserializedProductOutput.data.correlationId;

                    Assert.That(correlationId.Equals(responseHeaderCorrelationId), Is.True, "response header corerelationId is same as generated product correlation id");
                    unitsOfSalePricesData[] data = deserializedProductOutput.data.unitsOfSalePrices;

                    EffectiveDatesPerProductPC effectiveDate = new();
                    List<EffectiveDatesPerProductPC> effectiveDates = new();
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

        private List<string> GetProductListFromInputPayload(string inputJSONFilePath)
        {
            string jsonPayload = _jsonHelper.GetDeserializedString(inputJSONFilePath);
            _jsonInputPriceChangeHelper = JsonConvert.DeserializeObject<List<JsonInputPriceChangeHelper>>(jsonPayload);
            return JsonHelper.GetProductListFromSAPPayload(_jsonInputPriceChangeHelper);
        }

        private JsonOutputPriceChangeHelper GetDeserializedProductJson(string generatedProductJson)
        {
            string jsonString = _jsonHelper.GetDeserializedString(generatedProductJson);
            return JsonConvert.DeserializeObject<JsonOutputPriceChangeHelper>(jsonString);
        }

        private static string GetResponseHeaderCorrelationId(RestResponse response)
        {
            string correlationId = response.Headers.ToList().Find(x => x.Name == XCorrelationIdHeaderKey).Value.ToString();
            Console.WriteLine(correlationId);
            return correlationId;
        }

        private static RestRequest ErpFacadeBulkPriceInformationRequest(string filePath, string sharedKey)
        {
            string requestBody;

            using (StreamReader streamReader = new StreamReader(filePath))
            {
                requestBody = streamReader.ReadToEnd();
            }
            var request = new RestRequest(ErpFacadeBulkPriceInformationEndPoint, Method.Post);
            request.AddHeader("Content-Type", "application/json");
            request.AddQueryParameter("Key", sharedKey);
            request.AddParameter("application/json", requestBody, ParameterType.RequestBody);
            return request;
        }

        public async Task<bool> PostPriceChangeResponse200OKPAYSF12Months(string filePath, string generatedProductJsonFolder, string sharedKey)
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
            bool productValue = true;
            RestResponse response = await _client.ExecuteAsync(request);
            Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                Thread.Sleep(120000);
                responseHeadercorrelationID = GetResponseHeaderCorrelationId(response);
                _uniquePdtFromInputPayload = GetProductListFromInputPayload(filePath);
                List<string> UniquePdtFromAzureStorage = _azureBlobStorageHelper.GetProductListFromBlobContainerAsync(responseHeadercorrelationID).Result;
                Assert.That(_uniquePdtFromInputPayload.Count.Equals(UniquePdtFromAzureStorage.Count), Is.True, "Slicing is not correct");
                foreach (string products in UniquePdtFromAzureStorage)
                {
                    string generatedProductJsonFile = AzureBlobStorageHelper.DownloadJSONFromAzureBlob(generatedProductJsonFolder, responseHeadercorrelationID, products, "ProductChange");
                    Console.WriteLine(generatedProductJsonFile);

                    JsonOutputPriceChangeHelper desiailzedProductOutput = GetDeserializedProductJson(generatedProductJsonFile);
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
                    foreach (var SAPProduct in inputData.Where(x => x.Duration == 12 && x.Productname.Equals("PAYSF")))
                    {
                        Console.WriteLine(Environment.NewLine);
                        Console.WriteLine(string.Format("Comparing product - {0} for PAYSF 12 month Duration condition", SAPProduct.Productname));
                        var findProduct = data.FirstOrDefault(x => x.unitName == "PAYSF");
                        if (findProduct == null)
                        {
                            productValue = true;
                        }
                        else
                        {
                            productValue = false;
                        }
                    }
                }
            }
            return productValue;
        }

    }
}
