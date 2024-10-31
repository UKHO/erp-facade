using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using NUnit.Framework;
using RestSharp;
using System.Net;
using UKHO.ERPFacade.API.FunctionalTests.Configuration;
using UKHO.ERPFacade.API.FunctionalTests.Model;
using UKHO.ERPFacade.API.FunctionalTests.Modifiers;
using UKHO.ERPFacade.API.FunctionalTests.Operations;
using UKHO.ERPFacade.API.FunctionalTests.Validators;
using UKHO.ERPFacade.Common.Constants;

namespace UKHO.ERPFacade.API.FunctionalTests.Service
{
    public class LicenceUpdatedEndpoint : TestFixtureBase
    {
        private readonly RestClient _client;
        private readonly RestClientOptions _options;
        private readonly AzureBlobReaderWriter _azureBlobStorageHelper;
        private readonly ErpFacadeConfiguration _erpFacadeConfiguration;

        public static string GeneratedCorrelationId = string.Empty;

        public LicenceUpdatedEndpoint()
        {
            var serviceProvider = GetServiceProvider();
            _erpFacadeConfiguration = serviceProvider!.GetRequiredService<IOptions<ErpFacadeConfiguration>>().Value;

            _options = new RestClientOptions(_erpFacadeConfiguration.BaseUrl);
            _client = new RestClient(_options);
            _azureBlobStorageHelper = new AzureBlobReaderWriter();
        }

        public async Task<RestResponse> OptionLicenceUpdatedWebhookResponseAsync(string token)
        {
            var request = new RestRequest(_erpFacadeConfiguration.LicenceUpdatedRequestEndPoint, Method.Options);
            request.AddHeader("Authorization", "Bearer " + token);
            RestResponse response = await _client.ExecuteAsync(request);
            return response;
        }

        public async Task<RestResponse> PostLicenceUpdatedWebhookResponseAsync(string payloadFilePath, string token)
        {
            string requestBody;
            string correlationId;

            using (StreamReader streamReader = new(payloadFilePath))
            {
                requestBody = await streamReader.ReadToEndAsync();
            }

            requestBody = JsonModifier.UpdateTime(requestBody);
            (requestBody, correlationId) = JsonModifier.UpdateCorrelationId(requestBody);

            var request = new RestRequest(_erpFacadeConfiguration.LicenceUpdatedRequestEndPoint, Method.Post);
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Authorization", "Bearer " + token);
            request.AddParameter("application/json", requestBody, ParameterType.RequestBody);
            RestResponse response = await _client.ExecuteAsync(request);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                bool isBlobCreated = _azureBlobStorageHelper.VerifyBlobExists(AzureStorage.LicenceUpdatedEventContainerName, GeneratedCorrelationId);
                Assert.That(isBlobCreated, Is.True, $"Blob {GeneratedCorrelationId} not created in licenceupdatedblobs");
            }

            return response;
        }

        public async Task<RestResponse> PostLicenceUpdatedWebhookResponseAsync(string scenarioName, string payloadFilePath, string token)
        {
            string requestBody;

            using (StreamReader streamReader = new StreamReader(payloadFilePath))
            {
                requestBody = await streamReader.ReadToEndAsync();
            }

            if (scenarioName == "Bad Request")
            {
                var request = new RestRequest(_erpFacadeConfiguration.LicenceUpdatedRequestEndPoint, Method.Post);
                request.AddHeader("Content-Type", "application/json");
                request.AddHeader("Authorization", "Bearer " + token);
                request.AddParameter("application/json", requestBody, ParameterType.RequestBody);
                RestResponse response = await _client.ExecuteAsync(request);
                return response;

            }
            else if (scenarioName == "Unsupported Media Type")
            {
                var request = new RestRequest(_erpFacadeConfiguration.LicenceUpdatedRequestEndPoint, Method.Post);
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
            string correlationId;
            using (StreamReader streamReader = new(filePath))
            {
                requestBody = await streamReader.ReadToEndAsync();
            }
            requestBody = JsonModifier.UpdateTime(requestBody);
            (requestBody, correlationId) = JsonModifier.UpdateCorrelationId(requestBody);

            var request = new RestRequest(_erpFacadeConfiguration.LicenceUpdatedRequestEndPoint, Method.Post);
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Authorization", "Bearer " + token);
            request.AddParameter("application/json", requestBody, ParameterType.RequestBody);
            RestResponse response = await _client.ExecuteAsync(request);
            JsonInputLicenceUpdateHelper jsonPayload = JsonConvert.DeserializeObject<JsonInputLicenceUpdateHelper>(requestBody);
            string generatedXmlFilePath = _azureBlobStorageHelper.DownloadDirectoryFile(generatedXmlFolder, correlationId, AzureStorage.LicenceUpdatedEventContainerName);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                Assert.That(LicenceUpdateXMLValidator.CheckXmlAttributes(jsonPayload, generatedXmlFilePath, requestBody).Result, Is.True, "CheckXmlAttributes Failed");
            }
            return response;
        }
    }
}
