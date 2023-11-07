using NUnit.Framework;
using RestSharp;
using System.Net;
using Microsoft.Identity.Client;
using UKHO.ERPFacade.API.FunctionalTests.Configuration;
using UKHO.ERPFacade.API.FunctionalTests.Helpers;
using UKHO.ERPFacade.API.FunctionalTests.Model;
using Newtonsoft.Json;

namespace UKHO.ERPFacade.API.FunctionalTests.Service
{
    public class RoSWebhookEndpoint
    {
        private readonly RestClient _client;
        private readonly AzureBlobStorageHelper _azureBlobStorageHelper;

        private const string RoSWebhookRequestEndPoint = "/webhook/recordofsalepublishedeventreceived";
        public static string generatedCorrelationId = string.Empty;
        public static string recordOfSalesContainerName = "recordofsaleblobs";

        public RoSWebhookEndpoint()
        {
            RestClientOptions options = new(Config.TestConfig.ErpFacadeConfiguration.BaseUrl);

            _client = new RestClient(options);
            _azureBlobStorageHelper = new AzureBlobStorageHelper();
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
                bool isBlobCreated = _azureBlobStorageHelper.VerifyBlobExists(recordOfSalesContainerName, generatedCorrelationId);
                Assert.That(isBlobCreated, Is.True, $"Blob {generatedCorrelationId} not created");
            }

            return response;
        }

        public async Task<RestResponse> PostWebhookResponseAsyncForXML(string correlationId, string payloadFilePath, bool isFirstEvent, bool isLastEvent, string generatedXmlFolder, List<JsonInputRoSWebhookEvent> listOfEventJsons, string token)
        {
            string requestBody;
            List<string> blobList = new();

            using (StreamReader streamReader = new(payloadFilePath))
            {
                requestBody = await streamReader.ReadToEndAsync();
            }

            requestBody = SAPXmlHelper.UpdateTimeAndCorrIdField(requestBody, correlationId);

            var request = new RestRequest(RoSWebhookRequestEndPoint, Method.Post);
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
                bool isBlobCreated = _azureBlobStorageHelper.VerifyBlobExists(recordOfSalesContainerName, correlationId);
                Assert.That(isBlobCreated, Is.True, $"Blob for {correlationId} not created");
            }

            blobList = _azureBlobStorageHelper.GetBlobNamesInFolder(recordOfSalesContainerName, correlationId);

            switch (isLastEvent)
            {
                case true:
                    DateTime startTime = DateTime.UtcNow;
                    //10minutes polling after every 30 seconds to check if xml payload is generated during webjob execution.
                    while (!blobList.Contains("SapXmlPayload") && DateTime.UtcNow - startTime < TimeSpan.FromMinutes(10))
                    {
                        blobList = _azureBlobStorageHelper.GetBlobNamesInFolder(recordOfSalesContainerName, correlationId);
                        await Task.Delay(30000);
                    }
                    Assert.That(blobList, Does.Contain("SapXmlPayload"), $"XML is not generated for {correlationId} at {DateTime.Now}.");
                    string generatedXmlFilePath = _azureBlobStorageHelper.DownloadGeneratedXMLFile(generatedXmlFolder, correlationId, "recordofsaleblobs");
                    Assert.That(RoSXmlHelper.CheckXmlAttributes(generatedXmlFilePath, requestBody, listOfEventJsons).Result, Is.True, "CheckXMLAttributes Failed");
                    Assert.That(AzureTableHelper.GetSapStatus(correlationId), Is.EqualTo("Complete"), $"SAP status is Incomplete for {correlationId}");
                    break;
                case false:
                    Assert.That(blobList, Does.Not.Contain("SapXmlPayload"), $"XML is generated for {correlationId} before we receive all related events.");
                    Assert.That(AzureTableHelper.GetSapStatus(correlationId), Is.EqualTo("Incomplete"), $"SAP status is Complete for {correlationId}");
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
                        var request = new RestRequest(RoSWebhookRequestEndPoint, Method.Post);
                        request.AddHeader("Content-Type", "application/json");
                        request.AddHeader("Authorization", "Bearer " + token);
                        request.AddParameter("application/json", requestBody, ParameterType.RequestBody);
                        RestResponse response = await _client.ExecuteAsync(request);
                        return response;
                    }
                case "Unsupported Media Type":
                    {
                        var request = new RestRequest(RoSWebhookRequestEndPoint, Method.Post);
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
