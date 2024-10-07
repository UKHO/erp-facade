using Microsoft.AspNetCore.Mvc;

namespace UKHO.ERPFacade.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SapCallbackController : BaseController<WebhookController>
    {
        public SapCallbackController(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
        }

        [HttpPost]
        [Route("/erpfacade/sapcallback")]
        public virtual async Task<IActionResult> SapCallBack()
        {
            await Task.CompletedTask;
            return new OkObjectResult(true);
        }
    }
}
