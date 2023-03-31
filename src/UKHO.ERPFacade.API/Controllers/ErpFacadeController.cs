using Microsoft.AspNetCore.Mvc;
using UKHO.ERPFacade.Common.Logging;

namespace UKHO.ERPFacade.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ErpFacadeController : BaseController<ErpFacadeController>
    {
        private readonly ILogger<ErpFacadeController> _logger;
        public ErpFacadeController(IHttpContextAccessor contextAccessor,
                                   ILogger<ErpFacadeController> logger)
            : base(contextAccessor, logger)
        {
            _logger = logger;
        }

        [HttpPost]
        [Route("/webhook/erpfacadecontroller")]
        public virtual async Task<IActionResult> Post()
        {
            _logger.LogInformation(EventIds.Start.ToEventId(), "Start of Post endpoint with _X-Correlation-ID:{correlationId}", GetCurrentCorrelationId());
            return Ok();
        }
    }
}
