using Microsoft.AspNetCore.Mvc;

namespace UKHO.ERPFacade.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HealthCheckController : BaseController<HealthCheckController>
    {      
        public HealthCheckController(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
        }

        [HttpPost]
        [Route("/erpfacade/healthcheck")]
        public virtual async Task<IActionResult> ApiHealthCheckEvent() {

            await Task.CompletedTask;

            return new OkObjectResult(StatusCodes.Status200OK);
        }
    }
}
