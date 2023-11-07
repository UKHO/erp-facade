﻿using Newtonsoft.Json;
using NUnit.Framework;
using RestSharp;
using System.Net;
using UKHO.ERPFacade.API.FunctionalTests.Configuration;
using UKHO.ERPFacade.API.FunctionalTests.Helpers;
using UKHO.ERPFacade.API.FunctionalTests.Model;

namespace UKHO.ERPFacade.API.FunctionalTests.Service
{
    public class LicenceUpdatedEndpoint
    {
        private readonly RestClient _client;
        private readonly AzureBlobStorageHelper _azureBlobStorageHelper;

        private const string LicenceUpdatedRequestEndPoint = "/webhook/licenceupdatedpublishedeventreceived";

        public static string GeneratedCorrelationId = string.Empty;

        public LicenceUpdatedEndpoint()
        {
           _azureBlobStorageHelper = new AzureBlobStorageHelper();
           RestClientOptions options = new RestClientOptions(Config.TestConfig.ErpFacadeConfiguration.BaseUrl);
           _client = new RestClient(options);
        }

        public async Task<RestResponse> OptionLicenceUpdatedWebhookResponseAsync(string token)
        {
            var request = new RestRequest(LicenceUpdatedRequestEndPoint, Method.Options);
            request.AddHeader("Authorization", "Bearer " + token);
            RestResponse response = await _client.ExecuteAsync(request);
            return response;
        }

        public async Task<RestResponse> PostLicenceUpdatedWebhookResponseAsync(string payloadFilePath, string token)
        {
            string requestBody;

            using (StreamReader streamReader = new (payloadFilePath))
            {
                requestBody = await streamReader.ReadToEndAsync();
            }

            GeneratedCorrelationId = SAPXmlHelper.GenerateRandomCorrelationId();
            requestBody = SAPXmlHelper.UpdateTimeAndCorrIdField(requestBody, GeneratedCorrelationId);

            var request = new RestRequest(LicenceUpdatedRequestEndPoint, Method.Post);
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Authorization", "Bearer " + token);
            request.AddParameter("application/json", requestBody, ParameterType.RequestBody);
            RestResponse response = await _client.ExecuteAsync(request);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                bool isBlobCreated = _azureBlobStorageHelper.VerifyBlobExists("licenceupdatedblobs", GeneratedCorrelationId);
                Assert.That(isBlobCreated, Is.True, $"Blob {GeneratedCorrelationId} not created in licenceupdatedblobs");
            }

            return response;
        }

        public async Task<RestResponse> PostLicenceUpdatedWebhookResponseAsync(string scenarioName, string payloadFilePath, string token)
        {
            string requestBody;

            using (StreamReader streamReader = new StreamReader(payloadFilePath))
            {
                requestBody = streamReader.ReadToEnd();
            }

            if (scenarioName == "Bad Request")
            {
                var request = new RestRequest(LicenceUpdatedRequestEndPoint, Method.Post);
                request.AddHeader("Content-Type", "application/json");
                request.AddHeader("Authorization", "Bearer " + token);
                request.AddParameter("application/json", requestBody, ParameterType.RequestBody);
                RestResponse response = await _client.ExecuteAsync(request);
                return response;

            }
            else if (scenarioName == "Unsupported Media Type")
            {
                var request = new RestRequest(LicenceUpdatedRequestEndPoint, Method.Post);
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

        public async Task<RestResponse> PostLicenceUpdatedResponseAsyncForXML(string filePath, string generatedXmlFolder, string token)
        {
            string requestBody;

            using (StreamReader streamReader = new(filePath))
            {
                requestBody = streamReader.ReadToEnd();
            }
            GeneratedCorrelationId = SAPXmlHelper.GenerateRandomCorrelationId();
            requestBody = SAPXmlHelper.UpdateTimeAndCorrIdField(requestBody, GeneratedCorrelationId);
            var request = new RestRequest(LicenceUpdatedRequestEndPoint, Method.Post);
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Authorization", "Bearer " + token);
            request.AddParameter("application/json", requestBody, ParameterType.RequestBody);
            RestResponse response = await _client.ExecuteAsync(request);
            JsonInputLicenceUpdateHelper jsonPayload = JsonConvert.DeserializeObject<JsonInputLicenceUpdateHelper>(requestBody);
            string generatedXmlFilePath = _azureBlobStorageHelper.DownloadGeneratedXMLFile(generatedXmlFolder, GeneratedCorrelationId, "licenceupdatedblobs");
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                Assert.That(LicenceUpdateXmlHelper.CheckXmlAttributes(jsonPayload, generatedXmlFilePath, requestBody).Result, Is.True, "CheckXMLAttributes Failed");
            }
            return response;
        }
    }
}
