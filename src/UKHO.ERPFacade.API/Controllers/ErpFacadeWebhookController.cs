using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using UKHO.ERPFacade.API.Handlers;
using UKHO.ERPFacade.Common.Logging;

namespace UKHO.ERPFacade.API.Controllers
{
    public class ErpFacadeWebhookController : BaseController<ErpFacadeWebhookController>
    {
        private readonly ILogger<ErpFacadeWebhookController> _logger;
        private readonly Dictionary<string, IEventHandler> _eventHandlers;

        public ErpFacadeWebhookController(IHttpContextAccessor contextAccessor,
                                          IEnumerable<IEventHandler> eventHandlers,
                                          ILogger<ErpFacadeWebhookController> logger)
        : base(contextAccessor)
        {
            _logger = logger;

            _eventHandlers = new Dictionary<string, IEventHandler>();

            foreach (var handler in eventHandlers)
            {
                _eventHandlers.Add(handler.EventType, handler);
            }
        }

        [HttpOptions]
        [Route("/webhook/event")]
        [Authorize(Policy = "EncContentPublishedWebhookCaller")]
        public IActionResult HandleEvents()
        {
            var webhookRequestOrigin = HttpContext.Request.Headers["WebHook-Request-Origin"].FirstOrDefault();

            _logger.LogInformation(EventIds.NewEncContentPublishedEventOptionsCallStarted.ToEventId(), "Started processing the Options request for the New ENC Content Published event for webhook. | WebHook-Request-Origin : {webhookRequestOrigin}", webhookRequestOrigin);

            HttpContext.Response.Headers.Append("WebHook-Allowed-Rate", "*");
            HttpContext.Response.Headers.Append("WebHook-Allowed-Origin", webhookRequestOrigin);

            _logger.LogInformation(EventIds.NewEncContentPublishedEventOptionsCallCompleted.ToEventId(), "Completed processing the Options request for the New ENC Content Published event for webhook. | WebHook-Request-Origin : {webhookRequestOrigin}", webhookRequestOrigin);

            return new OkObjectResult(StatusCodes.Status200OK);
        }

        [HttpPost]
        [Route("/webhook/event")]
        //[Authorize(Policy = "EncContentPublishedWebhookCaller")]
        public async Task<IActionResult> HandleEvents([FromBody] JObject payload)
        {
            var eventType = payload["type"]?.ToString();

            if (string.IsNullOrEmpty(eventType))
            {
                return BadRequest("Invalid event type");
            }

            if (_eventHandlers.TryGetValue(eventType, out var eventHandler))
            {
                await eventHandler.HandleEventAsync(payload);
            }
            else
            {
                return BadRequest($"Unsupported event type: {eventType}");
            }

            return Ok();
        }
    }
}
