using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using UKHO.ERPFacade.Common.Converters;
using UKHO.ERPFacade.Common.Infrastructure.Config;
using UKHO.ERPFacade.Common.Infrastructure.EventService.EventProvider;
using UKHO.ERPFacade.Common.Logging;

namespace UKHO.ERPFacade.Common.Infrastructure.EventService
{
    public class EnterpriseEventServiceEventPublisher : IEventPublisher
    {
        public const string EventServiceClientName = "EventServiceClient";
        private const string CorrelationIdKey = "data.CorrelationId";
        private readonly ILogger<EnterpriseEventServiceEventPublisher> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _eventServiceEndpoint;

        public EnterpriseEventServiceEventPublisher(ILogger<EnterpriseEventServiceEventPublisher> logger, IHttpClientFactory httpClientFactory, IOptions<EnterpriseEventServiceConfiguration> options)
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
                _logger.LogInformation(EventIds.StartingEnterpriseEventServiceEventPublisher.ToEventId(), "Attempting to publish {cloudEventType} event for {cloudEventSubject} to Enterprise Event Service | _X-Correlation-ID : {_X-Correlation-ID} | PublishedEventId : {PublishedEventId}", eventData.Type, eventData.Subject, correlationId, eventData.Id);

                var response = await client.PostAsync(_eventServiceEndpoint, content);

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
