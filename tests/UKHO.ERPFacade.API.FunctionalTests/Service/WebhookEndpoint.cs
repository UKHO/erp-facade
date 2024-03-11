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

        public static string GeneratedCorrelationId = string.Empty;

        public WebhookEndpoint()
        {
            _azureBlobStorageHelper = new AzureBlobStorageHelper();
            _options = new RestClientOptions(Config.TestConfig.ErpFacadeConfiguration.BaseUrl);
            _client = new RestClient(_options);
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

            using (StreamReader streamReader = new(filePath))
            {
                requestBody = streamReader.ReadToEnd();
            }
            GeneratedCorrelationId = SapXmlHelper.GenerateRandomCorrelationId();
            requestBody = SapXmlHelper.UpdateTimeAndCorrIdField(requestBody, GeneratedCorrelationId);
            var request = new RestRequest(WebhookRequestEndPoint, Method.Post);
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Authorization", "Bearer " + token);
            request.AddParameter("application/json", requestBody, ParameterType.RequestBody);

            RestResponse response = await _client.ExecuteAsync(request);
            return response;
        }

        public async Task<RestResponse> PostWebhookResponseAsyncForXml(string filePath, string generatedXmlFolder, string token, string correctionTag = "N", string permitState = "permitString")
        {
            string requestBody;

            using (StreamReader streamReader = new(filePath))
            {
                requestBody = await streamReader.ReadToEndAsync();
            }
            GeneratedCorrelationId = SapXmlHelper.GenerateRandomCorrelationId();
            requestBody = SapXmlHelper.UpdateTimeAndCorrIdField(requestBody, GeneratedCorrelationId);
            requestBody = SapXmlHelper.UpdatePermitField(requestBody, permitState);

            var request = new RestRequest(WebhookRequestEndPoint, Method.Post);
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Authorization", "Bearer " + token);
            request.AddParameter("application/json", requestBody, ParameterType.RequestBody);
            RestResponse response = await _client.ExecuteAsync(request);

            JsonPayloadHelper jsonPayload = JsonConvert.DeserializeObject<JsonPayloadHelper>(requestBody);
            string correlationId = jsonPayload.Data.correlationId;

            //Logic to download XML from container using TraceID from JSON
            string generatedXmlFilePath = _azureBlobStorageHelper.DownloadGeneratedXML(generatedXmlFolder, correlationId);

            //Logic to verifyxml
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                Assert.That(SapXmlHelper.VerifyInitialXmlHeaders(jsonPayload, generatedXmlFilePath), Is.True, "Initial Header Value Not Correct");
                Assert.That(SapXmlHelper.VerifyOrderOfActions(jsonPayload, generatedXmlFilePath), Is.True, "Order of Action Not Correct in XML File");
                Assert.That(SapXmlHelper.CheckXmlAttributes(jsonPayload, generatedXmlFilePath, requestBody, correctionTag, permitState).Result, Is.True, "CheckXmlAttributes Failed");
            }
            return response;
        }


    }
}
