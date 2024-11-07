using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using UKHO.ERPFacade.API.Filters;

namespace UKHO.ERPFacade.API.Controllers
{
    [Route("api")]
    [ApiController]
    public class SapCallbackController : BaseController<SapCallbackController>
    {
        public SapCallbackController(IHttpContextAccessor contextAccessor) : base(contextAccessor)
        {
        }

        [HttpPost]
        [ServiceFilter(typeof(SharedApiKeyAuthFilter))]
        [Route("v2/callback/sap/s100actions/processed")]
        public virtual async Task<IActionResult> S100SapCallBack([FromBody] JObject sapCallBackJson)
        {
            return new OkObjectResult(StatusCodes.Status200OK);
        }
    }
}
