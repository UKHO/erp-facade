using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using UKHO.ERPFacade.API.Dispatcher;
using UKHO.ERPFacade.Common.Logging;

namespace UKHO.ERPFacade.API.Controllers
{
    public class WebhookController : BaseController<WebhookController>
    {
        private readonly ILogger<WebhookController> _logger;
        private readonly IEventDispatcher _eventDispatcher;

        public WebhookController(IHttpContextAccessor contextAccessor,
                                 IEventDispatcher eventDispatcher,
                                 ILogger<WebhookController> logger)
        : base(contextAccessor)
        {
            _logger = logger;
            _eventDispatcher = eventDispatcher;
        }

        [HttpOptions]
        [Route("api/v2/webhook")]
        //[Authorize(Policy = "EncContentPublishedWebhookCaller")]
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
        [Route("api/v2/webhook")]
        //[Authorize(Policy = "EncContentPublishedWebhookCaller")]
        public async Task<IActionResult> ReceiveEventsAsync([FromBody] JObject payload)
        {
            await _eventDispatcher.DispatchEventAsync(payload);
            return new OkObjectResult(StatusCodes.Status200OK);
        }
    }
}
