using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
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
    public class RoSWebhookEndpoint : TestFixtureBase
    {
        private readonly RestClient _client;
        private readonly RestClientOptions _options;
        private readonly AzureBlobReaderWriter _azureBlobStorageHelper;
        private readonly AzureTableReaderWriter _azureTableReaderWriter;
        private readonly ErpFacadeConfiguration _erpFacadeConfiguration;

        public RoSWebhookEndpoint()
        {
            var serviceProvider = GetServiceProvider();
            _erpFacadeConfiguration = serviceProvider!.GetRequiredService<IOptions<ErpFacadeConfiguration>>().Value;

            _options = new RestClientOptions(_erpFacadeConfiguration.BaseUrl);
            _client = new RestClient(_options);
            _azureBlobStorageHelper = new AzureBlobReaderWriter();
            _azureTableReaderWriter = new AzureTableReaderWriter();
        }

        public async Task<RestResponse> OptionRosWebhookResponseAsync(string token)
        {
            var request = new RestRequest(_erpFacadeConfiguration.RoSWebhookRequestEndPoint, Method.Options);
            request.AddHeader("Authorization", "Bearer " + token);
            RestResponse response = await _client.ExecuteAsync(request);
            return response;
        }

        public async Task<RestResponse> PostWebhookResponseAsync(string payloadFilePath, string token)
        {
            string requestBody;
            string correlationId;
            using (StreamReader streamReader = new(payloadFilePath))
            {
                requestBody = await streamReader.ReadToEndAsync();
            }

            requestBody = JsonModifier.UpdateTime(requestBody);
            (requestBody, correlationId) = JsonModifier.UpdateCorrelationId(requestBody);

            var request = new RestRequest(_erpFacadeConfiguration.RoSWebhookRequestEndPoint, Method.Post);
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Authorization", "Bearer " + token);
            request.AddParameter("application/json", requestBody, ParameterType.RequestBody);
            RestResponse response = await _client.ExecuteAsync(request);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                bool isBlobCreated = _azureBlobStorageHelper.VerifyBlobExists(AzureStorage.RecordOfSaleEventContainerName, correlationId);
                Assert.That(isBlobCreated, Is.True, $"Blob {correlationId} not created");
            }

            return response;
        }

        public async Task<RestResponse> PostWebhookResponseAsyncForXml(string payloadFilePath, bool isFirstEvent, bool isLastEvent, string generatedXmlFolder, List<JsonInputRoSWebhookEvent> listOfEventJsons, string token)
        {
            string requestBody;
            string correlationId;
            List<string> blobList = new();

            using (StreamReader streamReader = new(payloadFilePath))
            {
                requestBody = await streamReader.ReadToEndAsync();
            }

            requestBody = JsonModifier.UpdateTime(requestBody);
            (requestBody, correlationId) = JsonModifier.UpdateCorrelationId(requestBody);

            var request = new RestRequest(_erpFacadeConfiguration.RoSWebhookRequestEndPoint, Method.Post);
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Authorization", "Bearer " + token);
            request.AddParameter("application/json", requestBody, ParameterType.RequestBody);
            RestResponse response = await _client.ExecuteAsync(request);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                return response;
            }

            if (isFirstEvent)
            {
                bool isBlobCreated = _azureBlobStorageHelper.VerifyBlobExists(AzureStorage.RecordOfSaleEventContainerName, correlationId);
                Assert.That(isBlobCreated, Is.True, $"Blob for {correlationId} not created");
            }

            blobList = _azureBlobStorageHelper.GetBlobNamesInFolder(AzureStorage.RecordOfSaleEventContainerName, correlationId);

            switch (isLastEvent)
            {
                case true:
                    DateTime startTime = DateTime.UtcNow;
                    //10minutes polling after every 30 seconds to check if xml payload is generated during webjob execution.
                    while (!blobList.Contains(EventPayloadFiles.SapXmlPayloadFileName) && DateTime.UtcNow - startTime < TimeSpan.FromMinutes(10))
                    {
                        blobList = _azureBlobStorageHelper.GetBlobNamesInFolder(AzureStorage.RecordOfSaleEventContainerName, correlationId);
                        await Task.Delay(30000);
                    }
                    Assert.That(blobList, Does.Contain(EventPayloadFiles.SapXmlPayloadFileName), $"XML is not generated for {correlationId} at {DateTime.Now}.");
                    string generatedXmlFilePath = _azureBlobStorageHelper.DownloadDirectoryFile(generatedXmlFolder, correlationId, AzureStorage.RecordOfSaleEventContainerName);
                    Assert.That(RoSXMLValidator.CheckXmlAttributes(generatedXmlFilePath, requestBody, listOfEventJsons).Result, Is.True, "CheckXmlAttributes Failed");
                    Assert.That(_azureTableReaderWriter.GetSapStatus(correlationId), Is.EqualTo("Complete"), $"SAP status is Incomplete for {correlationId}");
                    break;
                case false:
                    Assert.That(blobList, Does.Not.Contain(EventPayloadFiles.SapXmlPayloadFileName), $"XML is generated for {correlationId} before we receive all related events.");
                    Assert.That(_azureTableReaderWriter.GetSapStatus(correlationId), Is.EqualTo("Incomplete"), $"SAP status is Complete for {correlationId}");
                    break;
            }
            return response;
        }

        public async Task<RestResponse> PostWebhookResponseAsync(string scenarioName, string payloadFilePath, string token)
        {
            string requestBody;

            using (StreamReader streamReader = new(payloadFilePath))
            {
                requestBody = await streamReader.ReadToEndAsync();
            }

            switch (scenarioName)
            {
                case "Bad Request":
                    {
                        var request = new RestRequest(_erpFacadeConfiguration.RoSWebhookRequestEndPoint, Method.Post);
                        request.AddHeader("Content-Type", "application/json");
                        request.AddHeader("Authorization", "Bearer " + token);
                        request.AddParameter("application/json", requestBody, ParameterType.RequestBody);
                        RestResponse response = await _client.ExecuteAsync(request);
                        return response;
                    }
                case "Unsupported Media Type":
                    {
                        var request = new RestRequest(_erpFacadeConfiguration.RoSWebhookRequestEndPoint, Method.Post);
                        request.AddHeader("Content-Type", "application/xml");
                        request.AddHeader("Authorization", "Bearer " + token);
                        request.AddParameter("application/xml", requestBody, ParameterType.RequestBody);
                        RestResponse response = await _client.ExecuteAsync(request);
                        return response;
                    }
                default:
                    Console.WriteLine("Scenario Not Mentioned");
                    return null;
            }
        }
    }
}
