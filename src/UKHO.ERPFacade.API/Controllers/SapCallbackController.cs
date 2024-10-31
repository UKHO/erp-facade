using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using UKHO.ERPFacade.API.Filters;

namespace UKHO.ERPFacade.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SapCallbackController : BaseController<SapCallbackController>
    {
        public SapCallbackController(IHttpContextAccessor contextAccessor) : base(contextAccessor)
        {
        }

        [HttpPost]
        [ServiceFilter(typeof(SharedApiKeyAuthFilter))]
        [Route("/api/v2/callback/sap/s100actions/processed")]
        public virtual async Task<IActionResult> S100ErpFacadeCallBack([FromBody] JObject sapCallBackJson)
        {
            return new OkObjectResult(StatusCodes.Status200OK);
        }
    }
}
