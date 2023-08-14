using NUnit.Framework;
using RestSharp;
using System.Net;
using UKHO.ERPFacade.API.FunctionalTests.Configuration;
using UKHO.ERPFacade.API.FunctionalTests.Helpers;

namespace UKHO.ERPFacade.API.FunctionalTests.Service
{
    internal class RoSWebhookEndpoint
    {
        private readonly RestClient _client;
        private readonly AzureBlobStorageHelper _azureBlobStorageHelper;
        private readonly RestClientOptions _options;

        private const string RoSWebhookRequestEndPoint = "/webhook/recordofsalepublishedeventreceived";

        public static string generatedCorrelationId = string.Empty;

        public RoSWebhookEndpoint()
        {
            _azureBlobStorageHelper = new();
            _options = new(Config.TestConfig.ErpFacadeConfiguration.BaseUrl);
            _client = new(_options);
        }

        public async Task<RestResponse> OptionRosWebhookResponseAsync(string token)
        {
            var request = new RestRequest(RoSWebhookRequestEndPoint, Method.Options);
            request.AddHeader("Authorization", "Bearer " + token);
            RestResponse response = await _client.ExecuteAsync(request);
            return response;
        }

        public async Task<RestResponse> PostWebhookResponseAsync(string payloadFilePath, string token)
        {
            string requestBody;

            using (StreamReader streamReader = new StreamReader(payloadFilePath))
            {
                requestBody = streamReader.ReadToEnd();
            }

            generatedCorrelationId = SAPXmlHelper.GenerateRandomCorrelationId();
            requestBody = SAPXmlHelper.UpdateTimeAndCorrIdField(requestBody, generatedCorrelationId);

            var request = new RestRequest(RoSWebhookRequestEndPoint, Method.Post);
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Authorization", "Bearer " + token);
            request.AddParameter("application/json", requestBody, ParameterType.RequestBody);
            RestResponse response = await _client.ExecuteAsync(request);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                bool isBlobCreated = _azureBlobStorageHelper.VerifyBlobExists("recordofsaleblobs", generatedCorrelationId);
                Assert.That(isBlobCreated, Is.True, $"Blob {generatedCorrelationId} not created");
            }

            return response;
        }

        public async Task<RestResponse> PostWebhookResponseAsync(string scenarioName, string payloadFilePath, string token)
        {
            string requestBody;

            using (StreamReader streamReader = new StreamReader(payloadFilePath))
            {
                requestBody = streamReader.ReadToEnd();
            }

            if (scenarioName == "Bad Request")
            {
                var request = new RestRequest(RoSWebhookRequestEndPoint, Method.Post);
                request.AddHeader("Content-Type", "application/json");
                request.AddHeader("Authorization", "Bearer " + token);
                request.AddParameter("application/json", requestBody, ParameterType.RequestBody);
                RestResponse response = await _client.ExecuteAsync(request);
                return response;

            } else if (scenarioName == "Unsupported Media Type")
            {
                var request = new RestRequest(RoSWebhookRequestEndPoint, Method.Post);
                request.AddHeader("Content-Type", "application/xml");
                request.AddHeader("Authorization", "Bearer " + token);
                request.AddParameter("application/xml", requestBody, ParameterType.RequestBody);
                RestResponse response = await _client.ExecuteAsync(request);
                return response;
            }
            else
            {
                Console.WriteLine("Scenario Not Mentioned");
                return null;
            }
        }
    }
}
