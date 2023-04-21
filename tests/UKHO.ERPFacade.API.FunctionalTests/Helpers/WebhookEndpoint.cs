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

        public async Task<RestResponse> OptionWebhookResponseAsync()
        {
            var request = new RestRequest("/webhook/newenccontentpublishedeventoptions");
            var response = await client.OptionsAsync(request);
            return response;
        }

        public async Task<RestResponse> PostWebhookResponseAsync(String filePath)
        {
            string requestBody;
            using (StreamReader streamReader = new StreamReader(filePath))
            {
                requestBody = streamReader.ReadToEnd();
            }
            var request = new RestRequest("/webhook/newenccontentpublishedeventreceived").AddJsonBody(requestBody);
            var response = await client.PostAsync(request);
            return response;
        }
    }
}
