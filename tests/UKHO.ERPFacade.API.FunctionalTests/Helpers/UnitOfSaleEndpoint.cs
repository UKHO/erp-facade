using Newtonsoft.Json;
using NUnit.Framework;
using RestSharp;
using UKHO.ERPFacade.API.FunctionalTests.Model;
using UKHO.ERPFacade.API.FunctionalTests.Model.Latest_Model;

namespace UKHO.ERPFacade.API.FunctionalTests.Helpers
{
    public class UnitOfSaleEndpoint
    {

        private readonly RestClient client;
        private JsonPayloadHelper generatedJsonPayload { get; set; }
        private AzureTableHelper azureTableHelper { get; set; }
        private WebhookEndpoint _webhook { get; set; }
        private AzureBlobStorageHelper azureBlobStorageHelper { get; set; }
        private readonly string _projectDir = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory));
        private JSONHelper _jsonHelper { get; set; }

        public UnitOfSaleEndpoint(string url)
        {
            _webhook = new WebhookEndpoint();
            _jsonHelper = new JSONHelper();
            var options = new RestClientOptions(url);
            client = new RestClient(options);
            azureTableHelper = new AzureTableHelper();
            azureBlobStorageHelper = new();
        }

        public async Task<RestResponse> PostUoSResponseAsync(string filePathWebhook, string generatedXMLFolder,string webhookToken, string filePath, string sharedKey)
        {


            await _webhook.PostWebhookResponseAsyncForXML(filePathWebhook, generatedXMLFolder, webhookToken);
            //string requestBody = _jsonHelper.getDeserializedString(filePath);

            string requestBody;

            using (StreamReader streamReader = new StreamReader(filePath))
            {
                requestBody = streamReader.ReadToEnd();
            }
            requestBody = JSONHelper.replaceCorrID(requestBody, WebhookEndpoint.generatedCorrelationId);

            var request = new RestRequest("/erpfacade/priceinformation", Method.Post);


            request.AddHeader("Content-Type", "application/json");
            request.AddQueryParameter("Key", sharedKey);
            request.AddParameter("application/json", requestBody, ParameterType.RequestBody);

            RestResponse response = await client.ExecuteAsync(request);
            List<UoSInputJSONHelper> jsonPayload = JsonConvert.DeserializeObject<List<UoSInputJSONHelper>>(requestBody);
            string corrId = jsonPayload[0].Corrid;

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                Assert.That(azureTableHelper.CheckResponseDateTime(corrId), Is.True, "ResponseDateTime Not updated in Azure table");
            }

            return response;
        }

        public async Task<RestResponse> PostUoSResponseAsyncWithJSON(string filePath, string generatedJSONFolder, string sharedKey)
        {
            string requestBody;

            using (StreamReader streamReader = new StreamReader(filePath))
            {
                requestBody = streamReader.ReadToEnd();
            }
            var request = new RestRequest("/erpfacade/priceinformation", Method.Post);

            request.AddHeader("Content-Type", "application/json");
            request.AddQueryParameter("Key", sharedKey);
            request.AddParameter("application/json", requestBody, ParameterType.RequestBody);

            RestResponse response = await client.ExecuteAsync(request);
            List<UoSInputJSONHelper> jsonPayload = JsonConvert.DeserializeObject<List<UoSInputJSONHelper>>(requestBody);
            string correlationId = jsonPayload[0].Corrid;


            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                string generatedJSONFilePath = azureBlobStorageHelper.DownloadJSONFromAzureBlob(generatedJSONFolder, correlationId, "json");
            }

            return response;
        }

        public async Task<RestResponse> PostWebHookAndUoSResponseAsyncWithJSON(string webHookfilePath, string uosFilePath, string generatedXMLFolder, string generatedJSONFolder, string webhookToken, string uostoken)
        {
            await _webhook.PostWebhookResponseAsyncForXML(webHookfilePath, generatedXMLFolder, webhookToken);
            //await _webhook.PostWebhookResponseAsync(webHookfilePath, webhookToken);
            string requestBody = _jsonHelper.getDeserializedString(uosFilePath);
            requestBody=JSONHelper.replaceCorrID(requestBody, WebhookEndpoint.generatedCorrelationId);
            var request = new RestRequest("/erpfacade/priceinformation", Method.Post);
            request.AddHeader("Content-Type", "application/json");
            request.AddQueryParameter("Key", uostoken);
            request.AddParameter("application/json", requestBody, ParameterType.RequestBody);
            RestResponse response = await client.ExecuteAsync(request);
            List<UoSInputJSONHelper> jsonPayload = JsonConvert.DeserializeObject<List<UoSInputJSONHelper>>(requestBody);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                string correlationId = jsonPayload[0].Corrid;
                string generatedJsonCorrelationId = "";
                string generatedJSONBody = "";
                string generatedJSONFilePath = azureBlobStorageHelper.DownloadJSONFromAzureBlob(generatedJSONFolder, correlationId, "json");
                using (StreamReader streamReader = new StreamReader(generatedJSONFilePath))
                {
                    generatedJSONBody = streamReader.ReadToEnd();
                }
                FinalUoSOutput generatedJsonPayload = JsonConvert.DeserializeObject<FinalUoSOutput>(generatedJSONBody);
                generatedJsonCorrelationId = generatedJsonPayload.data.correlationId;
                Assert.That(correlationId.Equals(generatedJsonCorrelationId), Is.True, "correlationIds from SAP and FinalUOS are not same");

                Unitsofsaleprice[] data = generatedJsonPayload.data.unitsOfSalePrices;
                List<string> finalUintNames = data.Select(x => x.unitName).ToList();
                List<string> SAPProductNames = jsonPayload.Select(x => x.Productname).Distinct().ToList();
                Assert.That(SAPProductNames.All(finalUintNames.Contains) && finalUintNames.All(SAPProductNames.Contains), Is.True, "ProductNames from SAP and FinalUOS are not same");
                var effectivePrices = data.Select(x => x.price);
                List<EffectiveDatesPerProduct> effectiveDates = new List<EffectiveDatesPerProduct>();
                foreach (Unitsofsaleprice unit in data)
                {
                    foreach (var prices in unit.price)
                    {
                        EffectiveDatesPerProduct effectiveDate = new EffectiveDatesPerProduct();
                        effectiveDate.ProductName = unit.unitName;
                        effectiveDate.EffectiveDates = prices.effectiveDate;
                        effectiveDate.Duration = prices.standard.priceDurations[0].numberOfMonths;
                        effectiveDate.rrp = prices.standard.priceDurations[0].rrp;
                        effectiveDates.Add(effectiveDate);
                    }
                }
                foreach (IGrouping<string, EffectiveDatesPerProduct> date in effectiveDates.GroupBy(x => x.ProductName))
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
            return response;
        }

        public async Task<RestResponse> PostWebHookAndUoSResponseAsyncWithNullProduct(string webHookfilePath, string uosFilePath, string generatedXMLFolder, string generatedJSONFolder, string webhookToken, string uostoken)
        {
            await _webhook.PostWebhookResponseAsyncForXML(webHookfilePath, generatedXMLFolder, webhookToken);
            string requestBody = _jsonHelper.getDeserializedString(uosFilePath);
            requestBody = JSONHelper.replaceCorrID(requestBody, WebhookEndpoint.generatedCorrelationId);
            var request = new RestRequest("/erpfacade/priceinformation", Method.Post);
            request.AddHeader("Content-Type", "application/json");
            request.AddQueryParameter("Key", uostoken);
            request.AddParameter("application/json", requestBody, ParameterType.RequestBody);
            RestResponse response = await client.ExecuteAsync(request);
            List<UoSInputJSONHelper> jsonPayload = JsonConvert.DeserializeObject<List<UoSInputJSONHelper>>(requestBody);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                string correlationId = jsonPayload[0].Corrid;
                string generatedJsonCorrelationId = "";
                string generatedJSONBody = "";
                string generatedJSONFilePath = azureBlobStorageHelper.DownloadJSONFromAzureBlob(generatedJSONFolder, correlationId, "json");
                using (StreamReader streamReader = new StreamReader(generatedJSONFilePath))
                {
                    generatedJSONBody = streamReader.ReadToEnd();
                }
                FinalUoSOutput generatedJsonPayload = JsonConvert.DeserializeObject<FinalUoSOutput>(generatedJSONBody);
                generatedJsonCorrelationId = generatedJsonPayload.data.correlationId;
                Assert.That(correlationId.Equals(generatedJsonCorrelationId), Is.True, "correlationIds from SAP and FinalUOS are not same");
                Unitsofsaleprice[] data = generatedJsonPayload.data.unitsOfSalePrices;
                List<string> finalUintNames = data.Select(x => x.unitName).ToList();
                List<string> SAPProductNames = jsonPayload.Select(x => x.Productname).Distinct().ToList();
                Assert.That(SAPProductNames.All(finalUintNames.Contains) && finalUintNames.All(SAPProductNames.Contains), Is.False, "ProductNames from SAP and FinalUOS are not same");             
            }
            return response;
        }

        public async Task<RestResponse> PostWebHookAndUoSNoCorrIdResponse400BadRequest(string webHookfilePath, string uosFilePath, string generatedXMLFolder, string generatedJSONFolder, string webhookToken, string uostoken)
        {
            await _webhook.PostWebhookResponseAsyncForXML(webHookfilePath, generatedXMLFolder, webhookToken);
            string requestBody = _jsonHelper.getDeserializedString(uosFilePath);
            requestBody = JSONHelper.replaceCorrID(requestBody, "");
            var request = new RestRequest("/erpfacade/priceinformation", Method.Post);
            request.AddHeader("Content-Type", "application/json");
            request.AddQueryParameter("Key", uostoken);
            request.AddParameter("application/json", requestBody, ParameterType.RequestBody);
            RestResponse response = await client.ExecuteAsync(request);
            return response;
        }

        public async Task<RestResponse> PostWebHookAndUoSInvalidCorrIdResponse404NotFound(string webHookfilePath, string uosFilePath, string generatedXMLFolder, string generatedJSONFolder, string webhookToken, string uostoken)
        {
            await _webhook.PostWebhookResponseAsyncForXML(webHookfilePath, generatedXMLFolder, webhookToken);
            string requestBody = _jsonHelper.getDeserializedString(uosFilePath);
            requestBody = JSONHelper.replaceCorrID(requestBody, "ssyyttddhhttaaww");
            var request = new RestRequest("/erpfacade/priceinformation", Method.Post);
            request.AddHeader("Content-Type", "application/json");
            request.AddQueryParameter("Key", uostoken);
            request.AddParameter("application/json", requestBody, ParameterType.RequestBody);
            RestResponse response = await client.ExecuteAsync(request);
            return response;
        }

        public async Task<RestResponse> PostWebHookAndUoSResponse200OK(string webHookfilePath, string uosFilePath, string generatedXMLFolder, string generatedJSONFolder, string webhookToken, string uostoken)
        {
            await _webhook.PostWebhookResponseAsyncForXML(webHookfilePath, generatedXMLFolder, webhookToken);
            string requestBody = _jsonHelper.getDeserializedString(uosFilePath);
            requestBody = JSONHelper.replaceCorrID(requestBody, WebhookEndpoint.generatedCorrelationId);
            var request = new RestRequest("/erpfacade/priceinformation", Method.Post);
            request.AddHeader("Content-Type", "application/json");
            request.AddQueryParameter("Key", uostoken);
            request.AddParameter("application/json", requestBody, ParameterType.RequestBody);
            RestResponse response = await client.ExecuteAsync(request);
            List<UoSInputJSONHelper> jsonSAPPriceInfoPayload = JsonConvert.DeserializeObject<List<UoSInputJSONHelper>>(requestBody);
            //Adding new code
           if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var SAPProductData = jsonSAPPriceInfoPayload.Select(x => new
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
                string correlationId = jsonSAPPriceInfoPayload[0].Corrid;
                string generatedJsonCorrelationId = "";
                string generatedJSONBody = "";
                string generatedJSONFilePath = azureBlobStorageHelper.DownloadJSONFromAzureBlob(generatedJSONFolder, correlationId, "json");
                using (StreamReader streamReader = new StreamReader(generatedJSONFilePath))
                {
                    generatedJSONBody = streamReader.ReadToEnd();
                }
                FinalUoSOutput generatedJsonPayload = JsonConvert.DeserializeObject<FinalUoSOutput>(generatedJSONBody);
                generatedJsonCorrelationId = generatedJsonPayload.data.correlationId;
                Assert.That(correlationId.Equals(generatedJsonCorrelationId), Is.True, "correlationIds from SAP and FinalUOS are not same");

                Unitsofsaleprice[] data = generatedJsonPayload.data.unitsOfSalePrices;
                List<string> finalUintNames = data.Select(x => x.unitName).ToList();
                List<string> SAPProductNames = jsonSAPPriceInfoPayload.Select(x => x.Productname).Distinct().ToList();
                
                var effectivePrices = data.Select(x => x.price);
                List<EffectiveDatesPerProduct> effectiveDates = new List<EffectiveDatesPerProduct>();
                foreach (Unitsofsaleprice unit in data)
                {
                    foreach (var prices in unit.price)
                    {
                        EffectiveDatesPerProduct effectiveDate = new EffectiveDatesPerProduct();
                        effectiveDate.ProductName = unit.unitName;
                        effectiveDate.EffectiveDates = prices.effectiveDate;
                        effectiveDate.Duration = prices.standard.priceDurations[0].numberOfMonths;
                        effectiveDate.rrp = prices.standard.priceDurations[0].rrp;
                        effectiveDates.Add(effectiveDate);
                    }
                }
                foreach (IGrouping<string, EffectiveDatesPerProduct> date in effectiveDates.GroupBy(x => x.ProductName))
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

                Console.WriteLine(Environment.NewLine);

                foreach (var SAPProduct in SAPProductData)
                {
                    Console.WriteLine(Environment.NewLine);
                    Console.WriteLine(string.Format("Comparing product - {0} for Effective dates and prices", SAPProduct.Productname));

                    EffectiveDatesPerProduct? findProduct = effectiveDates.FirstOrDefault(x => x.ProductName == SAPProduct.Productname
                                                     && x.EffectiveDates.Date == SAPProduct.EffectiveDateTime.Date
                                                     && x.rrp == SAPProduct.EffectivePrice && x.Duration == SAPProduct.Duration);


                    if (findProduct != null)
                    {
                        
                        Console.WriteLine(string.Format("Product - {0} found in Final UOS for Effective Date - {1}, Duration - {2} and Price - {3}", SAPProduct.Productname, SAPProduct.EffectiveDateTime.Date, SAPProduct.Duration, SAPProduct.EffectivePrice));
                    }
                    else
                    {
                        
                        Console.WriteLine(string.Format("Product - {0} Not found in Final UOS for Effective Date - {1}, Duration - {2} and Price - {3}", SAPProduct.Productname, SAPProduct.EffectiveDateTime.Date, SAPProduct.Duration, SAPProduct.EffectivePrice));
                    }

                    if (SAPProduct.FutureDateTime != new DateTime())
                    {
                        EffectiveDatesPerProduct? findFutureProduct = effectiveDates.FirstOrDefault(x => x.ProductName == SAPProduct.Productname
                                                     && x.EffectiveDates.Date == SAPProduct.FutureDateTime.Date
                                                     && x.rrp == SAPProduct.FuturePrice);

                        if (findFutureProduct != null)
                        {
                            
                            Console.WriteLine(string.Format("Product - {0} found in Final UOS for Future Date - {1}, Duration - {2} and Price - {3}", SAPProduct.Productname, SAPProduct.FutureDateTime.Date, SAPProduct.Duration, SAPProduct.FuturePrice));
                        }
                        else
                        {
                            
                            Console.WriteLine(string.Format("Product - {0} Not found in Final UOS for Future Date - {1}, Duration - {2} and Price - {3}", SAPProduct.Productname, SAPProduct.FutureDateTime.Date, SAPProduct.Duration, SAPProduct.FuturePrice));
                        }
                    }

                }
            }
            return response;
        }


    }
}

