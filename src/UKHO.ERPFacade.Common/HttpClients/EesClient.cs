using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using UKHO.ERPFacade.Common.Authentication;
using UKHO.ERPFacade.Common.Configuration;
using UKHO.ERPFacade.Common.Models;
using UKHO.ERPFacade.Common.Models.CloudEvents;

namespace UKHO.ERPFacade.Common.HttpClients
{
    public class EesClient : IEesClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<EesClient> _logger;
        private readonly ITokenProvider _tokenProvider;
        private readonly IOptions<EESConfiguration> _eesConfiguration;

        public EesClient(HttpClient httpClient, ILogger<EesClient> logger, ITokenProvider tokenProvider, IOptions<EESConfiguration> eesConfiguration)
        {
            _httpClient = httpClient;
            _logger = logger;
            _tokenProvider = tokenProvider;
            _eesConfiguration = eesConfiguration;
        }

        public async Task<HttpResponseMessage> Get(string url)
        {
            return await _httpClient.GetAsync(url);
        }

        public async Task<Result> PostAsync(BaseCloudEvent cloudEvent)
        {
            var cloudEventPayload = JsonConvert.SerializeObject(cloudEvent);

            if (!_eesConfiguration.Value.UseLocalResources)
            {
                string authToken = _tokenProvider.GetTokenAsync($"{_eesConfiguration.Value.ClientId}/{_eesConfiguration.Value.PublisherScope}").Result;
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);
            }

            var httpContent = new StringContent(cloudEventPayload, Encoding.UTF8);
            httpContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json; charset=utf-8");

            HttpResponseMessage response = await _httpClient.PostAsync(_eesConfiguration.Value.PublishEndpoint, httpContent);

            if (!response.IsSuccessStatusCode)
            {
                return Result.Failure(response.StatusCode.ToString());
            }
            return Result.Success();
        }
    }
}
