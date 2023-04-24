using RestSharp;

namespace UKHO.ERPFacade.API.FunctionalTests.Helpers
{
    public class WebhookEndpoint
    {
        public static Config config;
        private RestClient client;
        private readonly ADAuthTokenProvider _authToken;

        public WebhookEndpoint()
        {
            config = new();
            _authToken = new();
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
    }
}
