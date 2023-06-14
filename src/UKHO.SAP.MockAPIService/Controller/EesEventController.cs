using Microsoft.AspNetCore.Mvc;
using UKHO.SAP.MockAPIService.Enums;
using UKHO.SAP.MockAPIService.Services;

namespace UKHO.SAP.MockAPIService.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class EesEventController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly MockService _mockService;

        public EesEventController(IConfiguration configuration, MockService mockService)
        {
            _mockService = mockService;
            _configuration = configuration;
        }

        [HttpPost]
        [Route("/api/events")]
        public virtual async Task<IActionResult> Post()
        {
            if (bool.Parse(_configuration["IsFTRunning"]))
            {
                string currentTestCase = _mockService.GetCurrentTestCase();

                if (currentTestCase == TestCase.EESInternalServerError401.ToString())
                {
                    _mockService.CleanUp();
                    return new UnauthorizedObjectResult(StatusCodes.Status401Unauthorized);
                }
            }
            await Task.CompletedTask;
            return new OkObjectResult(StatusCodes.Status200OK);
        }
    }
}
