using Microsoft.AspNetCore.Mvc;
using UKHO.ERPFacade.Common.Logging;

namespace UKHO.ERPFacade.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WebhookController : BaseController<WebhookController>
    {
        private readonly ILogger<WebhookController> _logger;
        public WebhookController(IHttpContextAccessor contextAccessor,
                                   ILogger<WebhookController> logger)
            : base(contextAccessor, logger)
        {
            _logger = logger;
        }

        [HttpOptions]
        [Route("/webhook/newenseventpublished")]
        public IActionResult NewEnsEventPublishedOptions()
        {
            _logger.LogInformation(EventIds.Start.ToEventId(), "Start of webhook newFile published event with _X-Correlation-ID:{correlationId}", GetCurrentCorrelationId());
            var webhookRequestOrigin = HttpContext.Request.Headers["WebHook-Request-Origin"].FirstOrDefault();

            HttpContext.Response.Headers.Add("WebHook-Allowed-Rate", "*");
            HttpContext.Response.Headers.Add("WebHook-Allowed-Origin", webhookRequestOrigin);

            return new OkObjectResult(StatusCodes.Status200OK);
        }
    }
}
