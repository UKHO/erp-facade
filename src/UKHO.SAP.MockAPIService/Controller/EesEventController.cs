using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace UKHO.SAP.MockAPIService.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class EesEventController : ControllerBase
    {
        [HttpPost]
        [Route("/api/events")]
        public virtual async Task<IActionResult> Post([FromBody] JObject eventJson)
        {
            //store event in blob container
            await Task.CompletedTask;
            return new OkObjectResult(StatusCodes.Status200OK); //if test case set to 401-Unauthorized then 401
        }
    }
}
