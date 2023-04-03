using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using UKHO.ERPFacade.Common.Configuration;
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
        : base(contextAccessor)
        {
            _logger = logger;
        }

        [HttpOptions]
        [Route("/webhook/enccontentpublished")]
        public IActionResult EncContentPublishedOptions()
        {
            var webhookRequestOrigin = HttpContext.Request.Headers["WebHook-Request-Origin"].FirstOrDefault();

            _logger.LogInformation(EventIds.NewEnsEventPublishedWebhookOptionsCallStarted.ToEventId(), "Started processing the Options request for the New Ens Published event for webhook. | WebHook-Request-Origin : {webhookRequestOrigin} | _X-Correlation-ID : {CorrelationId}", webhookRequestOrigin, GetCurrentCorrelationId());

            HttpContext.Response.Headers.Add("WebHook-Allowed-Rate", "*");
            HttpContext.Response.Headers.Add("WebHook-Allowed-Origin", webhookRequestOrigin);

            _logger.LogInformation(EventIds.NewEnsEventPublishedWebhookOptionsCallCompleted.ToEventId(), "Completed processing the Options request for the New Ens Published event for webhook. | WebHook-Request-Origin : {webhookRequestOrigin} | _X-Correlation-ID : {CorrelationId}", webhookRequestOrigin, GetCurrentCorrelationId());

            return new OkObjectResult(StatusCodes.Status200OK);
        }

        [HttpPost]
        [Route("/webhook/enccontentpublished")]
        public virtual async Task<IActionResult> EncContentPublished([FromBody] JObject request)
        {
            _logger.LogInformation(EventIds.NewEnsEventReceived.ToEventId(), "ERP Facade webhook has received new event fron EES. | _X-Correlation-ID : {CorrelationId}", GetCurrentCorrelationId());

            await Task.CompletedTask;

            return new OkObjectResult(StatusCodes.Status200OK);
        }
    }
}