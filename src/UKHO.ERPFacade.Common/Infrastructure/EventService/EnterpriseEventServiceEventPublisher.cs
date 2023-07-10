using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UKHO.ERPFacade.Common.Converters;
using UKHO.ERPFacade.Common.Infrastructure.Config;
using UKHO.ERPFacade.Common.Infrastructure.EventService.EventProvider;
using UKHO.ERPFacade.Common.IO.Azure;
using UKHO.ERPFacade.Common.Logging;
using UKHO.ERPFacade.Common.Models;

namespace UKHO.ERPFacade.Common.Infrastructure.EventService
{
    public class EnterpriseEventServiceEventPublisher : IEventPublisher
    {
        public const string EventServiceClientName = "EventServiceClient";
        private readonly ILogger<EnterpriseEventServiceEventPublisher> _logger;
        private readonly ICloudEventFactory _cloudEventFactory;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IAzureBlobEventWriter _azureBlobEventWriter;
        private readonly string _eventServiceEndpoint;

        public EnterpriseEventServiceEventPublisher(ILogger<EnterpriseEventServiceEventPublisher> logger, ICloudEventFactory cloudEventFactory, IHttpClientFactory httpClientFactory, IOptions<ErpPublishEventSource> options, IAzureBlobEventWriter azureBlobEventWriter)
        {
            _logger = logger;
            _cloudEventFactory = cloudEventFactory;
            _httpClientFactory = httpClientFactory;
            _azureBlobEventWriter = azureBlobEventWriter;
            _eventServiceEndpoint = options.Value.PublishEndpoint;
        }

        public async Task<Result> Publish<TData>(CloudEvent<TData> eventData)
        {
            var serializerOptions = new JsonSerializerOptions();
            serializerOptions.Converters.Add(new RoundTripDateTimeConverter());
            var cloudEventPayload = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(eventData, serializerOptions);

            var content = new ByteArrayContent(cloudEventPayload);

            Byte[] byteArray = content.ReadAsByteArrayAsync().Result;

            var unitsOfSaleUpdatedCloudEventData = JsonConvert.DeserializeObject<CloudEvent<UnitOfSalePriceEventData>>(Encoding.UTF8.GetString(byteArray));

            JObject eventJson = JObject.Parse(JsonConvert.SerializeObject(unitsOfSaleUpdatedCloudEventData.Data));

            var correlationId = eventJson.SelectToken("correlationId")?.Value<string>();

            await _azureBlobEventWriter.UploadEvent(JsonConvert.SerializeObject(unitsOfSaleUpdatedCloudEventData, Formatting.Indented).ToString(), correlationId!, "UnitOfSaleUpdatedEvent_EES.json");

            content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/cloudevents+json; charset=utf-8");

            using var client = _httpClientFactory.CreateClient(EventServiceClientName);

            try
            {
                _logger.LogInformation(EventIds.StartingEnterpriseEventServiceEventPublisher.ToEventId(), "Attempting to send {cloudEventType} for {cloudEventSubject} to Enterprise Event Service", eventData.Type, eventData.Subject);
                var response = await client.PostAsync(_eventServiceEndpoint, content);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError(EventIds.EnterpriseEventServiceEventPublisherFailure.ToEventId(), "Failed to send event type: {cloudEventType} to the enterprise event service for product: {cloudEventSubject} | Status Code : {StatusCode}", eventData.Type, eventData.Subject, response.StatusCode.ToString());
                    return Result.Failure(response.StatusCode.ToString());
                }

                _logger.LogInformation(EventIds.EnterpriseEventServiceEventPublisherSuccess.ToEventId(), "Successfully sent {cloudEventType} for {cloudEventSubject} to Enterprise Event Service", eventData.Type, eventData.Subject);
                return Result.Success("Successfully sent event");
            }
            catch (Exception ex)
            {
                _logger.LogError(EventIds.EnterpriseEventServiceEventPublisherFailure.ToEventId(), "Failed to send event type: {cloudEventType} to the enterprise event service for product: {cloudEventSubject} | Exception Message : {ExceptionMessage}", eventData.Type, eventData.Subject, ex.Message);
                return Result.Failure(ex.Message);
            }
        }
    }
}
