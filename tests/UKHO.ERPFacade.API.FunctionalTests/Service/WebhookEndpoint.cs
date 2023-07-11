using Newtonsoft.Json;
using NUnit.Framework;
using RestSharp;
using UKHO.ERPFacade.API.FunctionalTests.Configuration;
using UKHO.ERPFacade.API.FunctionalTests.Helpers;
using UKHO.ERPFacade.API.FunctionalTests.Model;

namespace UKHO.ERPFacade.API.FunctionalTests.Service
{
    public class WebhookEndpoint
    {
        private readonly RestClient _client;
        private readonly AzureBlobStorageHelper _azureBlobStorageHelper;
        private readonly RestClientOptions _options;

        private const string WebhookRequestEndPoint = "/webhook/newenccontentpublishedeventreceived";

        public static string generatedCorrelationId = string.Empty;

        public WebhookEndpoint()
        {
            _azureBlobStorageHelper = new();
            _options = new(Config.TestConfig.ErpFacadeConfiguration.BaseUrl);
            _client = new(_options);
        }

        public async Task<RestResponse> OptionWebhookResponseAsync(string token)
        {
            var request = new RestRequest(WebhookRequestEndPoint);
            request.AddHeader("Authorization", "Bearer " + token);
            var response = await _client.OptionsAsync(request);
            return response;
        }

        public async Task<RestResponse> PostWebhookResponseAsync(string filePath, string token)
        {
            string requestBody;

            using (StreamReader streamReader = new StreamReader(filePath))
            {
                requestBody = streamReader.ReadToEnd();
            }

            generatedCorrelationId = SAPXmlHelper.GenerateRandomCorrelationId();
            requestBody = SAPXmlHelper.UpdateTimeAndCorrIdField(requestBody, generatedCorrelationId);

            var request = new RestRequest(WebhookRequestEndPoint, Method.Post);
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Authorization", "Bearer " + token);
            request.AddParameter("application/json", requestBody, ParameterType.RequestBody);

            RestResponse response = await _client.ExecuteAsync(request);
            return response;
        }

        public async Task<RestResponse> PostWebhookResponseAsyncForXML(string filePath, string generatedXMLFolder, string token)
        {
            string requestBody;

            using (StreamReader streamReader = new StreamReader(filePath))
            {
                requestBody = streamReader.ReadToEnd();
            }

            generatedCorrelationId = SAPXmlHelper.GenerateRandomCorrelationId();
            requestBody = SAPXmlHelper.UpdateTimeAndCorrIdField(requestBody, generatedCorrelationId);

            var request = new RestRequest(WebhookRequestEndPoint, Method.Post);
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Authorization", "Bearer " + token);
            request.AddParameter("application/json", requestBody, ParameterType.RequestBody);
            RestResponse response = await _client.ExecuteAsync(request);

            JsonPayloadHelper jsonPayload = JsonConvert.DeserializeObject<JsonPayloadHelper>(requestBody);
            string correlationId = jsonPayload.Data.correlationId;

            //Logic to download XML from container using TraceID from JSON
            string generatedXMLFilePath = _azureBlobStorageHelper.DownloadGeneratedXML(generatedXMLFolder, correlationId);

            //Logic to verifyxml
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                Assert.That(SAPXmlHelper.VerifyInitialXMLHeaders(jsonPayload, generatedXMLFilePath), Is.True);
                Assert.That(SAPXmlHelper.VerifyOrderOfActions(jsonPayload, generatedXMLFilePath), Is.True);
                Assert.That(SAPXmlHelper.CheckXMLAttributes(jsonPayload, generatedXMLFilePath, requestBody).Result, Is.True);
            }

            return response;
        }
    }
}
