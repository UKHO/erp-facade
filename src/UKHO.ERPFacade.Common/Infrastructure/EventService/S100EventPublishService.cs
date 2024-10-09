using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using UKHO.ERPFacade.Common.Converters;
using UKHO.ERPFacade.Common.Infrastructure.Config;
using UKHO.ERPFacade.Common.Infrastructure.EventProvider;
using UKHO.ERPFacade.Common.Logging;

namespace UKHO.ERPFacade.Common.Infrastructure.EventService
{
    public class S100EventPublishService
    {
        public const string EventServiceClientName = "S100Client";
        private const string CorrelationIdKey = "properties.CorrelationId";
        private readonly ILogger<S100EventPublishService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _eventServiceEndpoint;

        public S100EventPublishService(ILogger<S100EventPublishService> logger, IHttpClientFactory httpClientFactory, IOptions<S100EventServiceConfiguration> options)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _eventServiceEndpoint = options.Value.PublishEndpoint;
        }

        public async Task<Result> Publish<TData>(CloudEvent<TData> eventData)
        {
            var serializerOptions = new JsonSerializerOptions();

            serializerOptions.Converters.Add(new RoundTripDateTimeConverter());
            serializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;

            var cloudEventPayload = JsonSerializer.SerializeToUtf8Bytes(eventData, serializerOptions);

            var cloudEventPayloadJson = JObject.Parse(JsonSerializer.Serialize(eventData));

            string correlationId = cloudEventPayloadJson.SelectToken(CorrelationIdKey)?.Value<string>();

            var content = new ByteArrayContent(cloudEventPayload);

            content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/cloudevents+json; charset=utf-8");

            using var client = _httpClientFactory.CreateClient(EventServiceClientName);

            try
            {
                _logger.LogInformation(EventIds.StartingEnterpriseEventServiceEventPublisher.ToEventId(), "Attempting to publish {cloudEventType} event for {cloudEventSubject} to S100 Event Service | _X-Correlation-ID : {_X-Correlation-ID} | PublishedEventId : {PublishedEventId}", eventData.Type, eventData.Subject, correlationId, eventData.Id);

                var response = await client.PostAsync(_eventServiceEndpoint, content);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError(EventIds.EnterpriseEventServiceEventPublisherFailure.ToEventId(), "Failed to publish {cloudEventType} event to the S100 Event Service for {cloudEventSubject} | Status Code : {StatusCode} | _X-Correlation-ID : {_X-Correlation-ID} | PublishedEventId : {PublishedEventId}", eventData.Type, eventData.Subject, response.StatusCode.ToString(), correlationId, eventData.Id);

                    return Result.Failure(response.StatusCode.ToString());
                }

                _logger.LogInformation(EventIds.EnterpriseEventServiceEventPublisherSuccess.ToEventId(), "Event {cloudEventType} for {cloudEventSubject} is published to S100 Event Service successfully | _X-Correlation-ID : {_X-Correlation-ID} | PublishedEventId : {PublishedEventId}", eventData.Type, eventData.Subject, correlationId, eventData.Id);

                return Result.Success("Event published successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(EventIds.EnterpriseEventServiceEventConnectionFailure.ToEventId(), "Failed to connect to S100 Event Service. | Exception Message : {ExceptionMessage} | _X-Correlation-ID : {_X-Correlation-ID} | PublishedEventId : {PublishedEventId}", ex.Message, correlationId, eventData.Id);

                return Result.Failure(ex.Message);
            }
        }
    }
}
