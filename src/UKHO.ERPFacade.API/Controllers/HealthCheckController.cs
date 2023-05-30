using Microsoft.AspNetCore.Mvc;

namespace UKHO.ERPFacade.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HealthCheckController : BaseController<HealthCheckController>
    {
        private readonly ILogger<HealthCheckController> _logger;
        public HealthCheckController(IHttpContextAccessor httpContextAccessor,
                                     ILogger<HealthCheckController> logger) 
        : base(httpContextAccessor)
        {
            _logger = logger;
        }

        [HttpPost]
        [Route("/erpfacade/healthcheck")]
        public virtual async Task<IActionResult> ApiHealthCheckEvent() {

            return new   OkObjectResult(StatusCodes.Status200OK);
        }



    }
}
