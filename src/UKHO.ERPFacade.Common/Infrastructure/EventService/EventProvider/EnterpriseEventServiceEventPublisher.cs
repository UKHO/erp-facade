using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UKHO.ERPFacade.Common.Converters;
using UKHO.ERPFacade.Common.Infrastructure.Config;

namespace UKHO.ERPFacade.Common.Infrastructure.EventService.EventProvider
{
    public class EnterpriseEventServiceEventPublisher : IEventPublisher
    {
        public const string EventServiceClientName = "EventServiceClient";
        private readonly ILogger<EnterpriseEventServiceEventPublisher> _logger;
        private readonly ICloudEventFactory _cloudEventFactory;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _eventServiceEndpoint;

        public EnterpriseEventServiceEventPublisher(ILogger<EnterpriseEventServiceEventPublisher> logger, ICloudEventFactory cloudEventFactory, IHttpClientFactory httpClientFactory, IOptions<EnterpriseEventServiceConfiguration> options)
        {
            _logger = logger;
            _cloudEventFactory = cloudEventFactory;
            _httpClientFactory = httpClientFactory;
            _eventServiceEndpoint = options.Value.PublishEndpoint;
        }

        public async Task<Result> Publish<TData>(EventBase<TData> eventData)
        {
            var cloudEventData = _cloudEventFactory.Create(eventData);

            var serializerOptions = new JsonSerializerOptions();
            serializerOptions.Converters.Add(new RoundTripDateTimeConverter());
            var cloudEventPayload = JsonSerializer.SerializeToUtf8Bytes(cloudEventData, serializerOptions);

            var content = new ByteArrayContent(cloudEventPayload);

            content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/cloudevents+json; charset=utf-8");

            using var client = _httpClientFactory.CreateClient(EventServiceClientName);

            try
            {
                //_logger.LogTrace(EventIds.StartingEnterpriseEventServiceEventPublisher.ToEventId(), "Attempting to send {cloudEventType} for {cloudEventSubject} to Enterprise Event Service", cloudEventData.Type, cloudEventData.Subject);
                var response = await client.PostAsync(_eventServiceEndpoint, content);
                response.EnsureSuccessStatusCode();
                //_logger.LogTrace(EventIds.EnterpriseEventServiceEventPublisherSuccess.ToEventId(), "Successfully sent {cloudEventType} for {cloudEventSubject} to Enterprise Event Service", cloudEventData.Type, cloudEventData.Subject);
                return Result.Success("Successfully sent event");
            }
            catch (Exception ex)
            {
                //_logger.LogError(EventIds.EnterpriseEventServiceEventPublisherFailure.ToEventId(), ex, "Failed to send event type: {cloudEventType} to the enterprise event service for product: {cloudEventSubject}", cloudEventData.Type, cloudEventData.Subject);
                return Result.Failure(ex.Message);
            }
        }
    }
}