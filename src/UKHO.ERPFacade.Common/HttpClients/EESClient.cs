using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Polly;
using Polly.Extensions.Http;
using UKHO.ERPFacade.Common.Authentication;
using UKHO.ERPFacade.Common.Configuration;
using UKHO.ERPFacade.Common.Logging;
using UKHO.ERPFacade.Common.Models;
using UKHO.ERPFacade.Common.Models.CloudEvents;

namespace UKHO.ERPFacade.Common.HttpClients
{
    [ExcludeFromCodeCoverage]
    public class EesClient : IEesClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<EesClient> _logger;
        private readonly ITokenProvider _tokenProvider;
        private readonly IOptions<EESConfiguration> _eesConfiguration;
        private readonly IOptions<RetryPolicyConfiguration> _retryPolicyConfiguration;
        private readonly IConfiguration _configuration;

        public EesClient(HttpClient httpClient, ILogger<EesClient> logger, ITokenProvider tokenProvider, IOptions<EESConfiguration> eesConfiguration, IOptions<RetryPolicyConfiguration> retryPolicyConfiguration, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _logger = logger;
            _tokenProvider = tokenProvider;
            _eesConfiguration = eesConfiguration;
            _retryPolicyConfiguration = retryPolicyConfiguration;
            _configuration = configuration;
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

            try
            {
                _logger.LogInformation(EventIds.StartingEnterpriseEventServiceEventPublisher.ToEventId(), "Attempting to publish {cloudEventType} event to Enterprise Event Service.", cloudEvent.Type);

                var _retryPolicy = HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(_retryPolicyConfiguration.Value.Count, retryAttempt => TimeSpan.FromSeconds(_retryPolicyConfiguration.Value.Duration),
                onRetry: (outcome, timespan, retryAttempt, context) =>
                {
                    _logger.LogInformation(EventIds.RetryAttemptForEnterpriseEventServiceEvent.ToEventId(), "Retry attempt to publish {cloudEventType} to Enterprise Event Service failed at count : {retryAttemptCount}.", cloudEvent.Type, retryAttempt);
                });

                var httpContent = new StringContent(cloudEventPayload, Encoding.UTF8);
                httpContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json; charset=utf-8");

                HttpResponseMessage response = await _retryPolicy.ExecuteAsync(() =>
                {
                    return _httpClient.PostAsync(_eesConfiguration.Value.PublishEndpoint, httpContent);
                });

                if (!response.IsSuccessStatusCode)
                {
                    return Result.Failure(response.StatusCode.ToString());
                }

                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(EventIds.EnterpriseEventServiceEventPublishException.ToEventId(), "An error ocurred while publishing {cloudEventType} to Enterprise Event Service. | Exception Message : {ExceptionMessage}", cloudEvent.Type, ex.Message);
                return Result.Failure(ex.Message);
            }
        }
    }
}
