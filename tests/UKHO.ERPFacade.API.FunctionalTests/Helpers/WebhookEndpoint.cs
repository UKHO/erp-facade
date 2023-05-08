using Newtonsoft.Json;
using NUnit.Framework;
using RestSharp;

namespace UKHO.ERPFacade.API.FunctionalTests.Helpers
{
    public class WebhookEndpoint
    {
        public static Config config;
        private RestClient client;
        private readonly ADAuthTokenProvider _authToken;
        private SAPXmlHelper SapXmlHelper { get; set; }

        public WebhookEndpoint()
        {
            config = new();
            _authToken = new();
            SapXmlHelper = new SAPXmlHelper();
            var options = new RestClientOptions(config.testConfig.ErpFacadeConfiguration.BaseUrl);
            client = new RestClient(options);
            
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
            string generatedXMLFilePath = "C:\\Users\\Prash1501461\\Source\\Repos\\UKHO\\erp-facade\\tests\\UKHO.ERPFacade.API.FunctionalTests\\ERPFacadeExpectedXmlTestData\\3CellsReplace2CellsCancel.xml";
            //if (response.StatusCode==System.Net.HttpStatusCode.OK)

            //Logic to verifyxml                    
            Assert.That(SAPXmlHelper.verifyInitialXMLHeaders(jsonPayload, generatedXMLFilePath), Is.True);
            Assert.That(SAPXmlHelper.verifyOrderOfActions(jsonPayload, generatedXMLFilePath), Is.True);
            Assert.That(SAPXmlHelper.CheckXMLAttributes(jsonPayload, generatedXMLFilePath).Result, Is.True);
            


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
    }
}
