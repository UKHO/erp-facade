using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using UKHO.ERPFacade.Common.Logging;

namespace UKHO.ERPFacade.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class S100WebhookController : BaseController<S100WebhookController>
    {
        private readonly ILogger<S100WebhookController> _logger;

        private const string CorrelationIdKey = "data.correlationId";

        public S100WebhookController(IHttpContextAccessor contextAccessor, ILogger<S100WebhookController> logger) : base(contextAccessor)
        {
            _logger = logger;
        }

        [HttpOptions]
        [Route("/s100webhook/s100datacontentpublishedeventreceived")]
        [Authorize(Policy = "S100DataContentPublishedWebhookCaller")]
        public IActionResult S100DataContentPublishedEventOptions()
        {
            var s100WebhookRequestOrigin = HttpContext.Request.Headers["WebHook-Request-Origin"].FirstOrDefault();

            _logger.LogInformation(EventIds.S100DataContentPublishedEventOptionsCallStarted.ToEventId(), "Started processing the Options request for the S100 Data Content Published event for webhook. | WebHook-Request-Origin : {s100WebhookRequestOrigin}", s100WebhookRequestOrigin);

            HttpContext.Response.Headers.Append("WebHook-Allowed-Rate", "*");
            HttpContext.Response.Headers.Append("WebHook-Allowed-Origin", s100WebhookRequestOrigin);

            _logger.LogInformation(EventIds.S100DataContentPublishedEventOptionsCallCompleted.ToEventId(), "Completed processing the Options request for the S100 Data Content Published event for webhook. | WebHook-Request-Origin : {s100WebhookRequestOrigin}", s100WebhookRequestOrigin);

            return new OkObjectResult(StatusCodes.Status200OK);
        }

        [HttpPost]
        [Route("/s100webhook/s100datacontentpublishedeventreceived")]
        [Authorize(Policy = "S100DataContentPublishedWebhookCaller")]
        public virtual async Task<IActionResult> S100DataContentPublishedEventReceived([FromBody] JObject s100dataEventJson)
        {
            _logger.LogInformation(EventIds.S100DataContentPublishedEventReceived.ToEventId(), "ERP Facade webhook has received s100datacontentpublished event from EES.");

            string correlationId = s100dataEventJson.SelectToken(CorrelationIdKey)?.Value<string>();

            if (string.IsNullOrEmpty(correlationId))
            {
                _logger.LogWarning(EventIds.CorrelationIdMissingInS100DataContentPublishedEvent.ToEventId(), "CorrelationId is missing in s100 data content published event.");
                return new BadRequestObjectResult(StatusCodes.Status400BadRequest);
            }
            return new OkObjectResult(StatusCodes.Status200OK);
        }
    }
}
