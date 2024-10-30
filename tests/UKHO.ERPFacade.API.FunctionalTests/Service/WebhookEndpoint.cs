﻿using Newtonsoft.Json;
using NUnit.Framework;
using RestSharp;
using UKHO.ERPFacade.API.FunctionalTests.Configuration;
using UKHO.ERPFacade.API.FunctionalTests.Helpers;
using UKHO.ERPFacade.API.FunctionalTests.Model;
using UKHO.ERPFacade.Common.Constants;

namespace UKHO.ERPFacade.API.FunctionalTests.Service
{
    public class WebhookEndpoint
    {
        private readonly RestClient _client;
        private readonly AzureBlobStorageHelper _azureBlobStorageHelper;
        private readonly RestClientOptions _options;

        public static string GeneratedCorrelationId = string.Empty;

        public WebhookEndpoint()
        {
            _azureBlobStorageHelper = new AzureBlobStorageHelper();
            _options = new RestClientOptions(Config.TestConfig.ErpFacadeConfiguration.BaseUrl);
            _client = new RestClient(_options);
        }

        public async Task<RestResponse> OptionWebhookResponseAsync(string token)
        {
            var request = new RestRequest(RequestEndPoints.S57RequestEndPoint);
            request.AddHeader("Authorization", "Bearer " + token);
            var response = await _client.OptionsAsync(request);
            return response;
        }

        public async Task<RestResponse> PostWebhookResponseAsync(string filePath, string token, bool validateJson = false)
        {
            string requestBody;

            using (StreamReader streamReader = new(filePath))
            {
                requestBody = await streamReader.ReadToEndAsync();
            }
            GeneratedCorrelationId = SapXmlHelper.GenerateRandomCorrelationId();
            requestBody = SapXmlHelper.UpdateTimeAndCorrIdField(requestBody, GeneratedCorrelationId);
            var request = new RestRequest(RequestEndPoints.S57RequestEndPoint, Method.Post);
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Authorization", "Bearer " + token);
            request.AddParameter("application/json", requestBody, ParameterType.RequestBody);

            RestResponse response = await _client.ExecuteAsync(request);

            if (response.IsSuccessful && validateJson)
            {
                string content = await _azureBlobStorageHelper.GetGeneratedJson(GeneratedCorrelationId);
                Console.WriteLine(content);
                Console.WriteLine("--------------------------------------------------");
                Console.WriteLine(requestBody);
                Assert.That(content.Equals(requestBody));
            }

            return response;
        }

        public async Task<RestResponse> PostWebhookResponseAsyncForXml(string filePath, string generatedXmlFolder, string token, string permitState = "permitString")
        {
            string requestBody;

            using (StreamReader streamReader = new(filePath))
            {
                requestBody = await streamReader.ReadToEndAsync();
            }
            GeneratedCorrelationId = SapXmlHelper.GenerateRandomCorrelationId();
            requestBody = SapXmlHelper.UpdateTimeAndCorrIdField(requestBody, GeneratedCorrelationId);
            requestBody = SapXmlHelper.UpdatePermitField(requestBody, permitState);
            var request = new RestRequest(RequestEndPoints.S57RequestEndPoint, Method.Post);
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Authorization", "Bearer " + token);
            request.AddParameter("application/json", requestBody, ParameterType.RequestBody);
            RestResponse response = await _client.ExecuteAsync(request);

            JsonPayloadHelper jsonPayload = JsonConvert.DeserializeObject<JsonPayloadHelper>(requestBody);
            string correlationId = jsonPayload.Data.correlationId;

            //Logic to verify xml
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                //Logic to download XML from container using TraceID from JSON
                string generatedXmlFilePath = _azureBlobStorageHelper.DownloadGeneratedXml(generatedXmlFolder, correlationId);

                if (filePath.Contains(JsonFields.AioKey) && generatedXmlFilePath.Length == 0)
                {
                    return response;
                }

                //Expected XML 
                string xmlFilePath = filePath.Replace(Config.TestConfig.PayloadFolder, EventPayloadFiles.ErpFacadeExpectedXmlFiles).Replace(".JSON", ".xml");

                Assert.That(SapXmlHelper.VerifyGeneratedXml(generatedXmlFilePath, xmlFilePath, permitState));
            }
            return response;
        }

        public async Task<RestResponse> PostWebhookResponseForMandatoryAttributeValidation(string filePath, string token, string attributeName, int index, string action)
        {
            string requestBody;

            using (StreamReader streamReader = new(filePath))
            {
                requestBody = await streamReader.ReadToEndAsync();
            }
            requestBody = JsonHelper.ModifyMandatoryAttribute(requestBody, attributeName, index, action);

            var request = new RestRequest(RequestEndPoints.S57RequestEndPoint, Method.Post);
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Authorization", "Bearer " + token);
            request.AddParameter("application/json", requestBody, ParameterType.RequestBody);
            RestResponse response = await _client.ExecuteAsync(request);

            return response;
        }

    }
}
