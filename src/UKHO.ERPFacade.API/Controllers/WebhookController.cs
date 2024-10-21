using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UKHO.ERPFacade.API.Handlers;
using UKHO.ERPFacade.Common.Logging;
using UKHO.ERPFacade.Common.Models.CloudEvents;

namespace UKHO.ERPFacade.API.Controllers
{
    public class WebhookController : BaseController<WebhookController>
    {
        private readonly ILogger<WebhookController> _logger;
        private readonly IServiceProvider _serviceProvider;

        public WebhookController(IHttpContextAccessor contextAccessor,
                                 IServiceProvider serviceProvider,
                                 ILogger<WebhookController> logger)
        : base(contextAccessor)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        [HttpOptions]
        [Route("/webhook/newenccontentpublishedeventreceived")]
        [Route("api/v2/webhook")]
        [Authorize(Policy = "EncContentPublishedWebhookCaller")]
        [Authorize(Policy = "WebhookCaller")]
        public IActionResult ReceiveEvents()
        {
            var webhookRequestOrigin = HttpContext.Request.Headers["WebHook-Request-Origin"].FirstOrDefault();

            _logger.LogInformation(EventIds.NewEncContentPublishedEventOptionsCallStarted.ToEventId(), "Started processing the Options request for the New ENC Content Published event for webhook. | WebHook-Request-Origin : {webhookRequestOrigin}", webhookRequestOrigin);

            HttpContext.Response.Headers.Append("WebHook-Allowed-Rate", "*");
            HttpContext.Response.Headers.Append("WebHook-Allowed-Origin", webhookRequestOrigin);

            _logger.LogInformation(EventIds.NewEncContentPublishedEventOptionsCallCompleted.ToEventId(), "Completed processing the Options request for the New ENC Content Published event for webhook. | WebHook-Request-Origin : {webhookRequestOrigin}", webhookRequestOrigin);

            return new OkObjectResult(StatusCodes.Status200OK);
        }

        [HttpPost]
        [Route("/webhook/newenccontentpublishedeventreceived")]
        [Route("api/v2/webhook")]
        [Authorize(Policy = "EncContentPublishedWebhookCaller")]
        [Authorize(Policy = "WebhookCaller")]
        public virtual async Task<IActionResult> ReceiveEventsAsync([FromBody] JObject cloudEvent)
        {
            var baseCloudEvent = JsonConvert.DeserializeObject<BaseCloudEvent>(cloudEvent.ToString());
            var eventType = baseCloudEvent.Type;

            var eventHandler = _serviceProvider.GetKeyedService<IEventHandler>(eventType);

            if (eventHandler is null)
            {
                _logger.LogWarning("No handler registred for event type {EventType}", eventType);
                return new BadRequestObjectResult(StatusCodes.Status400BadRequest);
            }

            await eventHandler.ProcessEventAsync(baseCloudEvent);

            return new OkObjectResult(StatusCodes.Status200OK);
        }
    }
}
