using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Polly;
using Polly.Extensions.Http;
using UKHO.ERPFacade.Common.Configuration;
using UKHO.ERPFacade.Common.Constants;
using UKHO.ERPFacade.Common.EventPublisher.Authentication;
using UKHO.ERPFacade.Common.Logging;
using UKHO.ERPFacade.Common.Models;

namespace UKHO.ERPFacade.Common.EventPublisher
{
    public class EESEventPublisher : IEventPublisher
    {
        private readonly ILogger<EESEventPublisher> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IOptions<EESConfiguration> _eesConfiguration;
        private readonly IOptions<RetryPolicyConfiguration> _retryPolicyConfiguration;
        private readonly IAccessTokenCache _accessTokenCache;

        public EESEventPublisher(ILogger<EESEventPublisher> logger, IHttpClientFactory httpClientFactory, IOptions<EESConfiguration> eesConfiguration, IOptions<RetryPolicyConfiguration> retryPolicyConfiguration, IAccessTokenCache accessTokenCache)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _eesConfiguration = eesConfiguration;
            _retryPolicyConfiguration = retryPolicyConfiguration;
            _accessTokenCache = accessTokenCache;
        }

        public async Task<Result> Publish<TData>(TData eventData)
        {
            var eventPayload = JsonConvert.SerializeObject(eventData);
            var eventPayloadJson = JObject.Parse(eventPayload);

            string correlationId = eventPayloadJson.SelectToken(JsonFields.CorrelationIdKey)?.Value<string>();
            string type = eventPayloadJson.SelectToken(JsonFields.Type)?.Value<string>();

            var httpContent = new StringContent(eventPayload, Encoding.UTF8);
            httpContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json; charset=utf-8");

            using var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(_eesConfiguration.Value.ServiceUrl);

            string token = _accessTokenCache.GetTokenAsync($"{_eesConfiguration.Value.ClientId}/{_eesConfiguration.Value.PublisherScope}").Result;
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            try
            {
                _logger.LogInformation(EventIds.StartingEnterpriseEventServiceEventPublisher.ToEventId(), "Attempting to publish {cloudEventType} event to Enterprise Event Service | _X-Correlation-ID : {_X-Correlation-ID}", type, correlationId);

                var _retryPolicy = HttpPolicyExtensions
                  .HandleTransientHttpError()
                  .WaitAndRetryAsync(_retryPolicyConfiguration.Value.Count, retryAttempt => TimeSpan.FromSeconds(_retryPolicyConfiguration.Value.Duration),
                  onRetry: (retryAttempt, context) =>
                  {
                      _logger.LogInformation(EventIds.RetryAttemptForEnterpriseEventServiceEvent.ToEventId(), "Retry attempt to publish {cloudEventType} to Enterprise Event Service failed at count : {retryAttemptCount}.", type, retryAttempt);
                  });

                HttpResponseMessage response = await _retryPolicy.ExecuteAsync(() =>
                {
                    return client.PostAsync(_eesConfiguration.Value.PublishEndpoint, httpContent);
                });

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError(EventIds.EnterpriseEventServiceEventPublisherFailure.ToEventId(), "Failed to publish {cloudEventType} event to the Enterprise Event Service | Status Code : {StatusCode} | _X-Correlation-ID : {_X-Correlation-ID}", type, response.StatusCode.ToString(), correlationId);
                    return Result.Failure(response.StatusCode.ToString());
                }

                _logger.LogInformation(EventIds.EnterpriseEventServiceEventPublisherSuccess.ToEventId(), "Event {cloudEventType} is published to Enterprise Event Service successfully | _X-Correlation-ID : {_X-Correlation-ID}", type, correlationId);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(EventIds.EnterpriseEventServiceEventConnectionFailure.ToEventId(), "An error ocurred while publishing {cloudEventType} to Enterprise Event Service. | Exception Message : {ExceptionMessage} | _X-Correlation-ID : {_X-Correlation-ID}", type, ex.Message, correlationId);
                return Result.Failure(ex.Message);
            }
        }
    }
}
