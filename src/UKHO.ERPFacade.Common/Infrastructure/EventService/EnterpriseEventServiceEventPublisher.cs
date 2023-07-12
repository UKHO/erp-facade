﻿using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UKHO.ERPFacade.Common.Converters;
using UKHO.ERPFacade.Common.Infrastructure.Config;
using UKHO.ERPFacade.Common.Infrastructure.EventService.EventProvider;
using UKHO.ERPFacade.Common.Logging;

namespace UKHO.ERPFacade.Common.Infrastructure.EventService
{
    public class EnterpriseEventServiceEventPublisher : IEventPublisher
    {
        public const string EventServiceClientName = "EventServiceClient";
        private readonly ILogger<EnterpriseEventServiceEventPublisher> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _eventServiceEndpoint;

        public EnterpriseEventServiceEventPublisher(ILogger<EnterpriseEventServiceEventPublisher> logger, IHttpClientFactory httpClientFactory, IOptions<ErpPublishEventSource> options)
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

            var content = new ByteArrayContent(cloudEventPayload);

            content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/cloudevents+json; charset=utf-8");

            using var client = _httpClientFactory.CreateClient(EventServiceClientName);

            try
            {
                _logger.LogInformation(EventIds.StartingEnterpriseEventServiceEventPublisher.ToEventId(), "Attempting to publish {cloudEventType} event for {cloudEventSubject} to Enterprise Event Service", eventData.Type, eventData.Subject);

                var response = await client.PostAsync(_eventServiceEndpoint, content);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError(EventIds.EnterpriseEventServiceEventPublisherFailure.ToEventId(), "Failed to publish {cloudEventType} event to the Enterprise Event Service for {cloudEventSubject} | Status Code : {StatusCode}", eventData.Type, eventData.Subject, response.StatusCode.ToString());
                    return Result.Failure(response.StatusCode.ToString());
                }

                _logger.LogInformation(EventIds.EnterpriseEventServiceEventPublisherSuccess.ToEventId(), "Event {cloudEventType} for {cloudEventSubject} is published to Enterprise Event Service successfully", eventData.Type, eventData.Subject);

                return Result.Success("Event published successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(EventIds.EnterpriseEventServiceEventConnectionFailure.ToEventId(), "Failed to connect to Enterprise Event Service. | Exception Message : {ExceptionMessage}", ex.Message);
                return Result.Failure(ex.Message);
            }
        }
    }
}
