using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Polly;
using Polly.Extensions.Http;
using UKHO.ERPFacade.Common.Configuration;
using UKHO.ERPFacade.Common.EventPublisher.Authentication;
using UKHO.ERPFacade.Common.HttpClients;
using UKHO.ERPFacade.Common.Logging;
using UKHO.ERPFacade.Common.Models;
using UKHO.ERPFacade.Common.Models.CloudEvents;

namespace UKHO.ERPFacade.Common.EventPublisher
{
    public class EventPublisher : IEventPublisher
    {
        private readonly ILogger<EventPublisher> _logger;
        private readonly IEESClient _eesClient;
        private readonly IOptions<EESConfiguration> _eesConfiguration;
        private readonly IOptions<RetryPolicyConfiguration> _retryPolicyConfiguration;
        private readonly IAccessTokenCache _accessTokenCache;

        public EventPublisher(ILogger<EventPublisher> logger, IHttpClientFactory httpClientFactory, IEESClient eesClient, IOptions<EESConfiguration> eesConfiguration, IOptions<RetryPolicyConfiguration> retryPolicyConfiguration, IAccessTokenCache accessTokenCache)
        {
            _logger = logger;
            _eesClient = eesClient;
            _eesConfiguration = eesConfiguration;
            _retryPolicyConfiguration = retryPolicyConfiguration;
            _accessTokenCache = accessTokenCache;
        }

        public async Task<Result> Publish(BaseCloudEvent cloudEvent)
        {
            var cloudEventPayload = JsonConvert.SerializeObject(cloudEvent);

            string token = _accessTokenCache.GetTokenAsync($"{_eesConfiguration.Value.ClientId}/{_eesConfiguration.Value.PublisherScope}").Result;

            try
            {
                _logger.LogInformation(EventIds.StartingEnterpriseEventServiceEventPublisher.ToEventId(), "Attempting to publish {cloudEventType} event to Enterprise Event Service", cloudEvent.Type);

                var _retryPolicy = HttpPolicyExtensions
                  .HandleTransientHttpError()
                  .WaitAndRetryAsync(_retryPolicyConfiguration.Value.Count, retryAttempt => TimeSpan.FromSeconds(_retryPolicyConfiguration.Value.Duration),
                  onRetry: (retryAttempt, context) =>
                  {
                      _logger.LogInformation(EventIds.RetryAttemptForEnterpriseEventServiceEvent.ToEventId(), "Retry attempt to publish {cloudEventType} to Enterprise Event Service failed at count : {retryAttemptCount}.", cloudEvent.Type, retryAttempt);
                  });

                HttpResponseMessage response = await _retryPolicy.ExecuteAsync(() =>
                {
                    return _eesClient.PostAsync(_eesConfiguration.Value.PublishEndpoint, token, cloudEventPayload);
                });

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError(EventIds.EnterpriseEventServiceEventPublisherFailure.ToEventId(), "Failed to publish {cloudEventType} event to the Enterprise Event Service | Status Code : {StatusCode}", cloudEvent.Type, response.StatusCode.ToString());
                    return Result.Failure(response.StatusCode.ToString());
                }

                _logger.LogInformation(EventIds.EnterpriseEventServiceEventPublisherSuccess.ToEventId(), "Event {cloudEventType} is published to Enterprise Event Service successfully.", cloudEvent.Type);

                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(EventIds.EnterpriseEventServiceEventConnectionFailure.ToEventId(), "An error ocurred while publishing {cloudEventType} to Enterprise Event Service. | Exception Message : {ExceptionMessage}", cloudEvent.Type, ex.Message);
                return Result.Failure(ex.Message);
            }
        }
    }
}
