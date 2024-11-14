using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using Polly;
using Polly.Extensions.Http;
using UKHO.ERPFacade.Common.Configuration;
using UKHO.ERPFacade.Common.Converters;
using UKHO.ERPFacade.Common.Infrastructure.Authentication;
using UKHO.ERPFacade.Common.Logging;
using UKHO.ERPFacade.Common.Models.CloudEvents;

namespace UKHO.ERPFacade.Common.Infrastructure.EventService
{
    public class EnterpriseEventServiceEventPublisher : IEventPublisher
    {
        public const string EventServiceClientName = "EventServiceClient";
        private const string CorrelationIdKey = "data.CorrelationId";
        private readonly ILogger<EnterpriseEventServiceEventPublisher> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly EnterpriseEventServiceConfiguration _enterpriseEventServiceConfiguration;
        private readonly RetryPolicyConfiguration _retryPolicyConfiguration;
        private readonly InteractiveLoginConfiguration _interactiveLoginConfiguration;
        private readonly IAccessTokenCache _accessTokenCache;

        public EnterpriseEventServiceEventPublisher(ILogger<EnterpriseEventServiceEventPublisher> logger, IHttpClientFactory httpClientFactory, IOptions<EnterpriseEventServiceConfiguration> options, IOptions<InteractiveLoginConfiguration> interactiveLoginConfiguration, IOptions<RetryPolicyConfiguration> retryPolicyConfiguration, IAccessTokenCache accessTokenCache)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _enterpriseEventServiceConfiguration = options.Value;
            _interactiveLoginConfiguration = interactiveLoginConfiguration.Value;
            _accessTokenCache = accessTokenCache;
            _retryPolicyConfiguration = retryPolicyConfiguration.Value;
        }

        public async Task<Result> Publish(BaseCloudEvent eventData)
        {
            var serializerOptions = new JsonSerializerOptions();

            serializerOptions.Converters.Add(new RoundTripDateTimeConverter());
            serializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;

            var cloudEventPayloadJson = JObject.Parse(JsonSerializer.Serialize(eventData));
            string correlationId = cloudEventPayloadJson.SelectToken(CorrelationIdKey)?.Value<string>();

            var cloudEventPayload = JsonSerializer.SerializeToUtf8Bytes(eventData, serializerOptions);
            var content = new ByteArrayContent(cloudEventPayload);

            content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json; charset=utf-8");

            using var client = _httpClientFactory.CreateClient(EventServiceClientName);
            client.BaseAddress = new Uri(_enterpriseEventServiceConfiguration.ServiceUrl);
         
            try
            {
                _logger.LogInformation(EventIds.StartingEnterpriseEventServiceEventPublisher.ToEventId(), "Attempting to publish {cloudEventType} event for {cloudEventSubject} to Enterprise Event Service | _X-Correlation-ID : {_X-Correlation-ID} | PublishedEventId : {PublishedEventId}", eventData.Type, eventData.Subject, correlationId, eventData.Id);

                var _retryPolicy = HttpPolicyExtensions
                  .HandleTransientHttpError()
                  .WaitAndRetryAsync(_retryPolicyConfiguration.Count, retryAttempt => TimeSpan.FromSeconds(_retryPolicyConfiguration.Duration),
                  onRetry: (outcome, timespan, retryAttempt, context) =>
                  {
                      _logger.LogInformation(EventIds.RetryAttemptForEnterpriseEventServiceEvent.ToEventId(), "Retry attempt to Enterprise Event Service failed at URI count : {retryAttemptCount}.", retryAttempt);
                  });

                HttpResponseMessage response = await _retryPolicy.ExecuteAsync(() =>
                {
                    return client.PostAsync(_enterpriseEventServiceConfiguration.PublishEndpoint, content);
                });

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError(EventIds.EnterpriseEventServiceEventPublisherFailure.ToEventId(), "Failed to publish {cloudEventType} event to the Enterprise Event Service for {cloudEventSubject} | Status Code : {StatusCode} | _X-Correlation-ID : {_X-Correlation-ID} | PublishedEventId : {PublishedEventId}", eventData.Type, eventData.Subject, response.StatusCode.ToString(), correlationId, eventData.Id);
                    return Result.Failure(response.StatusCode.ToString());
                }

                _logger.LogInformation(EventIds.EnterpriseEventServiceEventPublisherSuccess.ToEventId(), "Event {cloudEventType} for {cloudEventSubject} is published to Enterprise Event Service successfully | _X-Correlation-ID : {_X-Correlation-ID} | PublishedEventId : {PublishedEventId}", eventData.Type, eventData.Subject, correlationId, eventData.Id);
                return Result.Success("Event published successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(EventIds.EnterpriseEventServiceEventConnectionFailure.ToEventId(), "Failed to connect to Enterprise Event Service. | Exception Message : {ExceptionMessage} | _X-Correlation-ID : {_X-Correlation-ID} | PublishedEventId : {PublishedEventId}", ex.Message, correlationId, eventData.Id);
                return Result.Failure(ex.Message);
            }
        }
    }
}
