using Microsoft.AspNetCore.Mvc;

namespace UKHO.ERPFacade.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class S100SapCallbackController : BaseController<S100SapCallbackController>
    {
        public S100SapCallbackController(IHttpContextAccessor contextAccessor) : base(contextAccessor)
        {
        }
    }
}
