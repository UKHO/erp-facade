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
        : base(contextAccessor)
        {
            _logger = logger;
        }

        [HttpPost]
        [Route("/erpfacade/getpriceinfo")]
        public virtual async Task<IActionResult> Post()
        {
            //_logger.LogInformation(EventIds.ErpFacadeApiCalled.ToEventId(), "ERP Facade endpoint is called. | _X-Correlation-ID:{correlationId}", GetCurrentCorrelationId());
            await Task.CompletedTask;
            return Ok();
        }
    }
}
