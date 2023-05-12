using Newtonsoft.Json;
using NUnit.Framework;
using RestSharp;
using UKHO.ERPFacade.API.FunctionalTests.Model;
using System.Text;

namespace UKHO.ERPFacade.API.FunctionalTests.Helpers
{
    public class WebhookEndpoint
    {
        public static Config config;
        private readonly RestClient client;
        private readonly RestClient client2;
        private readonly ADAuthTokenProvider _authToken;
        private SAPXmlHelper SapXmlHelper { get; set; }

        public WebhookEndpoint()
        {
            config = new();
            _authToken = new();
            SapXmlHelper = new SAPXmlHelper();
            var options = new RestClientOptions(config.testConfig.ErpFacadeConfiguration.BaseUrl);
            client = new RestClient(options);
            var options2 = new RestClientOptions(config.TestConfig.SapMockConfiguration.BaseUrl);
            client2 = new RestClient(options2);

        }

        public async Task<RestResponse> OptionWebhookResponseAsync(string token)
        {
            var request = new RestRequest("/webhook/newenccontentpublishedeventoptions");
            request.AddHeader("Authorization", "Bearer " + token);
            var response = await client.OptionsAsync(request);
            return response;
        }


        public async Task<RestResponse> PostWebhookResponseAsync(string filePath, string token)
        {
            string requestBody;

            using (StreamReader streamReader = new StreamReader(filePath))
            {
                requestBody = streamReader.ReadToEnd();
            }

            var request = new RestRequest("/webhook/newenccontentpublishedeventreceived", Method.Post);
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Authorization", "Bearer " + token);
            request.AddParameter("application/json", requestBody, ParameterType.RequestBody);

            RestResponse response = await client.ExecuteAsync(request);
            return response;
        }

        public async Task<RestResponse> PostWebhookResponseAsyncForXML(string filePath, string expectedXMLfilePath, string token)
        {
            string requestBody;

            using (StreamReader streamReader = new StreamReader(filePath))
            {
                requestBody = streamReader.ReadToEnd();
            }

            var request = new RestRequest("/webhook/newenccontentpublishedeventreceived", Method.Post);
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Authorization", "Bearer " + token);
            request.AddParameter("application/json", requestBody, ParameterType.RequestBody);
            RestResponse response = await client.ExecuteAsync(request);

            JsonPayloadHelper jsonPayload  = JsonConvert.DeserializeObject<JsonPayloadHelper>(requestBody);
            string traceID = jsonPayload.Data.TraceId;

            //Logic to download XML from container using TraceID from JSON
            //string generatedXMLFilePath = SapXmlHelper.downloadGeneratedXML(expectedXMLfilePath,traceID); // string path will be returned

            string generatedXMLFilePath = expectedXMLfilePath;
            //Logic to verifyxml
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                Assert.That(SAPXmlHelper.verifyInitialXMLHeaders(jsonPayload, generatedXMLFilePath), Is.True);
                Assert.That(SAPXmlHelper.verifyOrderOfActions(jsonPayload, generatedXMLFilePath), Is.True);
                Assert.That(SAPXmlHelper.CheckXMLAttributes(jsonPayload, generatedXMLFilePath).Result, Is.True);
            }

            return response;
        }
        public async Task<RestResponse> PostWebhookResponseAsyncForXML(string filePath, string token)
        {
            string requestBody;

            using (StreamReader streamReader = new StreamReader(filePath))
            {
                requestBody = streamReader.ReadToEnd();
            }

            var request = new RestRequest("/webhook/newenccontentpublishedeventreceived", Method.Post);
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Authorization", "Bearer " + token);
            request.AddParameter("application/json", requestBody, ParameterType.RequestBody);
            RestResponse response = await client.ExecuteAsync(request);

            

            return response;
        }

        public async void PostMockSapResponseAsync(string filePath)
        {
            string requestBody;
            var cred = $"{config.TestConfig.SapMockConfiguration.Username}:{config.TestConfig.SapMockConfiguration.Password}";

            using (StreamReader streamReader = new StreamReader(filePath))
            {
                requestBody = streamReader.ReadToEnd();
            }

            var request = new RestRequest("/z_adds_mat_info.asmx", Method.Post);
            request.AddHeader("Content-Type", "application/xml");
            request.AddHeader("Authorization", "Basic " +Convert.ToBase64String(Encoding.UTF8.GetBytes(cred)));
            request.AddParameter("application/xml", requestBody, ParameterType.RequestBody);

            RestResponse response = await client2.ExecuteAsync(request);
        }

    }
}
