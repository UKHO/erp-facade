using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using RestSharp;
using System.Text;
using UKHO.ERPFacade.API.FunctionalTests.Model;

namespace UKHO.ERPFacade.API.FunctionalTests.Helpers
{
    public class WebhookEndpoint
    {
        public static Config config;
        private readonly RestClient client;
        private readonly RestClient client2;
        private readonly ADAuthTokenProvider _authToken;
        private SAPXmlHelper SapXmlHelper { get; set; }
        public static string genratedTraceId = "";
        public static Dictionary<string, string> genratedTraceId1 = new();

        public WebhookEndpoint()
        {
            config = new();
            _authToken = new();
            SapXmlHelper = new SAPXmlHelper();
            var options = new RestClientOptions(config.TestConfig.ErpFacadeConfiguration.BaseUrl);
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

            //genratedTraceId1.Add("traceId", SAPXmlHelper.generateRandomTraceId());
            //requestBody = SAPXmlHelper.updateTimeField(requestBody, genratedTraceId1["traceId"]);
            genratedTraceId = SAPXmlHelper.generateRandomTraceId();
            requestBody = SAPXmlHelper.updateTimeField(requestBody, genratedTraceId);

            var request = new RestRequest("/webhook/newenccontentpublishedeventreceived", Method.Post);
            var now1 = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fffffff'z'");

            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Authorization", "Bearer " + token);
            request.AddParameter("application/json", requestBody, ParameterType.RequestBody);

            RestResponse response = await client.ExecuteAsync(request);
            return response;
        }

        public async Task<RestResponse> PostWebhookResponseAsyncForXML(string filePath, string generatedXMLFolder, string token)
        {
            string requestBody;

            using (StreamReader streamReader = new StreamReader(filePath))
            {
                requestBody = streamReader.ReadToEnd();
            }

            //genratedTraceId1.Add("traceId",SAPXmlHelper.generateRandomTraceId());
            //requestBody = SAPXmlHelper.updateTimeField(requestBody, genratedTraceId1["traceId"]);
            genratedTraceId = SAPXmlHelper.generateRandomTraceId();
            requestBody = SAPXmlHelper.updateTimeField(requestBody, genratedTraceId);

            var request = new RestRequest("/webhook/newenccontentpublishedeventreceived", Method.Post);
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Authorization", "Bearer " + token);
            request.AddParameter("application/json", requestBody, ParameterType.RequestBody);
            RestResponse response = await client.ExecuteAsync(request);

            JsonPayloadHelper jsonPayload = JsonConvert.DeserializeObject<JsonPayloadHelper>(requestBody);
            string traceID = jsonPayload.Data.TraceId;

            //Logic to download XML from container using TraceID from JSON
            string generatedXMLFilePath = SapXmlHelper.downloadGeneratedXML(generatedXMLFolder, traceID);

            //genratedTraceId1.Clear();

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

        public async Task PostMockSapResponseAsync()
        {
            var cred = $"{config.TestConfig.SapMockConfiguration.Username}:{config.TestConfig.SapMockConfiguration.Password}";

            var request = new RestRequest("/api/ConfigureTestCase/SAPInternalServerError500", Method.Post);
            request.AddHeader("Content-Type", "application/xml");
            request.AddHeader("Authorization", "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(cred)));

            await client2.ExecuteAsync(request);

        }

    }
}