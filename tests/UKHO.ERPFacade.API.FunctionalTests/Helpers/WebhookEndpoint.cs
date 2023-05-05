using RestSharp;
using System.Text;

namespace UKHO.ERPFacade.API.FunctionalTests.Helpers
{
    public class WebhookEndpoint
    {
        public static Config config;
        private RestClient client;
        private RestClient client2;
        private readonly ADAuthTokenProvider _authToken;

        public WebhookEndpoint()
        {
            config = new();
            _authToken = new();
            var options = new RestClientOptions(config.testConfig.ErpFacadeConfiguration.BaseUrl);
            client = new RestClient(options);
            var options2 = new RestClientOptions(config.testConfig.SapConfiguration.BaseUrl);
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

        public async void PostMockSapResponseAsync(string filePath)
        {
            string requestBody;
            var cred = $"{config.testConfig.SapConfiguration.Username}:{config.testConfig.SapConfiguration.Password}";

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
