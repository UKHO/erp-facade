using RestSharp;

namespace UKHO.ERPFacade.API.FunctionalTests.Helpers
{
    
    public class WebhookEndpoint
    {
        public readonly Config config = new();
        private RestClient client;
        public WebhookEndpoint()
        {
            var options = new RestClientOptions(config.testConfig.ErpFacadeConfiguration.BaseUrl);
            client = new RestClient(options);
        }

        public async Task<RestResponse> OptionWebhookResponseAsync()
        {
            String requestBody = "{ }";
            var request = new RestRequest("/webhook/newenccontentpublishedeventoptions").AddJsonBody(requestBody);
            var response = await client.OptionsAsync(request);
            return response;
        }

        
        public async Task<RestResponse> PostWebhookResponseAsync(String filePath)
        {
            String requestBody;
            using (StreamReader r = new StreamReader(filePath))
            {
                requestBody = r.ReadToEnd();
            }
            var request = new RestRequest("/webhook/newenccontentpublishedeventreceived").AddJsonBody(requestBody);
            var response = await client.PostAsync(request);
            return response;
        }
    }
}
