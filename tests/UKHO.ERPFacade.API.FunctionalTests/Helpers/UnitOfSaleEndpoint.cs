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

        public async Task<RestResponse> PostUoSResponseAsync(string filePath, string token)
        {
            string requestBody;

            using (StreamReader streamReader = new StreamReader(filePath))
            {
                requestBody = streamReader.ReadToEnd();
            }


            var request = new RestRequest("/erpfacade/priceinformation", Method.Post);


            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Authorization", "Bearer " + token);
            request.AddParameter("application/json", requestBody, ParameterType.RequestBody);

            RestResponse response = await client.ExecuteAsync(request);
            List<UoSInputJSONHelper> jsonPayload = JsonConvert.DeserializeObject<List<UoSInputJSONHelper>>(requestBody);
            string traceID = jsonPayload[0].Corrid;


            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                Assert.That(azureTableHelper.CheckResponseDateTime(traceID), Is.True, "ResponseDateTime Not updated in Azure table");
            }

            return response;
        }

        public async Task<RestResponse> PostUoSResponseAsyncWithJSON(string filePath, string generatedJSONFolder, string token)
        {
            string requestBody;

            using (StreamReader streamReader = new StreamReader(filePath))
            {
                requestBody = streamReader.ReadToEnd();
            }
            var request = new RestRequest("/erpfacade/priceinformation", Method.Post);

            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Authorization", "Bearer " + token);
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
            string requestBody;
            using (StreamReader streamReader = new StreamReader(uosFilePath))
            {
                requestBody = streamReader.ReadToEnd();
            }
            var request = new RestRequest("/erpfacade/priceinformation", Method.Post);
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Authorization", "Bearer " + uostoken);
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
                generatedJsonCorrelationId = generatedJsonPayload.EventData.data.correlationId;
                Assert.That(correlationId.Equals(generatedJsonCorrelationId), Is.True, "correlationIds from SAP and FinalUOS are not same");

                Unitsofsaleprice[] data = generatedJsonPayload.EventData.data.unitsOfSalePrices;
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
            string requestBody;
            using (StreamReader streamReader = new StreamReader(uosFilePath))
            {
                requestBody = streamReader.ReadToEnd();
            }
            var request = new RestRequest("/erpfacade/priceinformation", Method.Post);
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Authorization", "Bearer " + uostoken);
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
                generatedJsonCorrelationId = generatedJsonPayload.EventData.data.correlationId;
                Assert.That(correlationId.Equals(generatedJsonCorrelationId), Is.True, "correlationIds from SAP and FinalUOS are not same");
                Unitsofsaleprice[] data = generatedJsonPayload.EventData.data.unitsOfSalePrices;
                List<string> finalUintNames = data.Select(x => x.unitName).ToList();
                List<string> SAPProductNames = jsonPayload.Select(x => x.Productname).Distinct().ToList();
                Assert.That(SAPProductNames.All(finalUintNames.Contains) && finalUintNames.All(SAPProductNames.Contains), Is.False, "ProductNames from SAP and FinalUOS are not same");
                
            }
            return response;
        }
        
    }
}

