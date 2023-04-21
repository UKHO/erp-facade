using RestSharp;

namespace UKHO.ERPFacade.API.FunctionalTests.Helpers
{
    public class WebhookEndpoint
    {
        public Config config;
        private RestClient client;
        public WebhookEndpoint()
        {
            config = new();
            var options = new RestClientOptions(config.testConfig.ErpFacadeConfiguration.BaseUrl);
            client = new RestClient(options);
        }

        public async Task<RestResponse> OptionWebhookResponseAsync(String token)
        {
            var request = new RestRequest("/webhook/newenccontentpublishedeventoptions", Method.Options);
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Authorization", "Bearer " + token);
            request.AddBody("{}");
            RestResponse response = await client.ExecuteAsync(request);
            return response;
        }

        public async Task<RestResponse> PostWebhookResponseAsync(String filePath, String token)
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
