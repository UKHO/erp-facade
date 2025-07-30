using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using RestSharp;
using UKHO.ERPFacade.API.FunctionalTests.Configuration;

namespace UKHO.ERPFacade.API.FunctionalTests.Service
{
    public class SapCallbackEndpoint : TestFixtureBase
    {
        private readonly RestClient _client;
        private readonly RestClientOptions _options;
        private readonly ErpFacadeConfiguration _erpFacadeConfiguration;
        private readonly SharedApiKeyConfiguration _sharedApiKeyConfiguration;

        public SapCallbackEndpoint()
        {
            var serviceProvider = GetServiceProvider();
            _erpFacadeConfiguration = serviceProvider!.GetRequiredService<IOptions<ErpFacadeConfiguration>>().Value;
            _sharedApiKeyConfiguration = serviceProvider!.GetRequiredService<IOptions<SharedApiKeyConfiguration>>().Value;

            _options = new RestClientOptions(_erpFacadeConfiguration.BaseUrl);
            _client = new RestClient(_options);
        }

        public async Task<RestResponse> PostSapCallbackEndPointResponseAsync(string requestBody, bool isInvalidKey = false)
        {
            var request = new RestRequest(_erpFacadeConfiguration.SapCallbackRequestEndPoint, Method.Post);

            var sharedApiKey = isInvalidKey ? _sharedApiKeyConfiguration.InvalidSharedApiKey : _sharedApiKeyConfiguration.SharedApiKey;

            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("X-API-Key", sharedApiKey);
            request.AddParameter("application/json", requestBody, ParameterType.RequestBody);

            return await _client.ExecuteAsync(request);
        }
    }
}
