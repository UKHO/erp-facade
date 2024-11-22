﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using RestSharp;
using UKHO.ERPFacade.API.FunctionalTests.Configuration;

namespace UKHO.ERPFacade.API.FunctionalTests.Service
{
    public class WebhookEndpoint : TestFixtureBase
    {
        private readonly RestClient _client;
        private readonly RestClientOptions _options;
        private readonly ErpFacadeConfiguration _erpFacadeConfiguration;
        private readonly SharedApiKeyConfiguration _sharedApiKeyConfiguration;

        public WebhookEndpoint()
        {
            var serviceProvider = GetServiceProvider();
            _erpFacadeConfiguration = serviceProvider!.GetRequiredService<IOptions<ErpFacadeConfiguration>>().Value;
            _sharedApiKeyConfiguration = serviceProvider!.GetRequiredService<IOptions<SharedApiKeyConfiguration>>().Value;

            _options = new RestClientOptions(_erpFacadeConfiguration.BaseUrl);
            _client = new RestClient(_options);
        }

        public async Task<RestResponse> OptionWebhookResponseAsync(string token)
        {
            var request = new RestRequest(_erpFacadeConfiguration.WebhookEndpointUrl, Method.Options);

            request.AddHeader("Authorization", "Bearer " + token);

            return await _client.ExecuteAsync(request);
        }

        public async Task<RestResponse> PostWebhookResponseAsync(string requestBody, string token)
        {
            var request = new RestRequest(_erpFacadeConfiguration.WebhookEndpointUrl, Method.Post);

            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Authorization", "Bearer " + token);
            request.AddParameter("application/json", requestBody, ParameterType.RequestBody);

            return await _client.ExecuteAsync(request);
        }

        public async Task<RestResponse> PostSapCallbackEndPointResponseAsync(string requestBody, bool isWrongKey = false)
        {
            var request = new RestRequest(_erpFacadeConfiguration.SapCallbackRequestEndPoint, Method.Post);

            string key = isWrongKey ? "6CAxzdYcEtNDuRhMbDpkBF8e4d4Kib46dwL9ZE5egiL0iL5Y3dzREUBSUYVUwUkM" : _sharedApiKeyConfiguration.SharedApiKey;

            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("X-API-Key", key);
            request.AddParameter("application/json", requestBody, ParameterType.RequestBody);

            return await _client.ExecuteAsync(request);
        }
    }
}
