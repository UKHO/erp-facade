using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace UKHO.ERPFacade.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WebhookController : ControllerBase
    {
        [HttpOptions]
        [Route("/webhook/enccontentpublished")]
        public IActionResult EncContentPublishedOptions()
        {
            var webHookRequestOrigin = HttpContext.Request.Headers["WebHook-Request-Origin"].FirstOrDefault();

            HttpContext.Response.Headers.Add("WebHook-Allowed-Rate", "*");
            HttpContext.Response.Headers.Add("WebHook-Allowed-Origin", webHookRequestOrigin);

            return new OkObjectResult(StatusCodes.Status200OK);
        }

        [HttpPost]
        [Route("/webhook/enccontentpublished")]
        public virtual async Task<IActionResult> EncContentPublished([FromBody] JObject request)
        {
            await Task.CompletedTask;
            Console.WriteLine($"Webhook Receieved from EES");
            return new OkObjectResult(StatusCodes.Status200OK);
        }
    }
}