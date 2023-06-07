using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using UKHO.ERPFacade.Common.IO.Azure;
using UKHO.SAP.MockAPIService.Enums;
using UKHO.SAP.MockAPIService.Services;

namespace UKHO.SAP.MockAPIService.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class EesEventController : ControllerBase
    {
        private readonly IAzureBlobEventWriter _azureBlobEventWriter;
        private readonly IConfiguration _configuration;
        private readonly MockService _mockService;

        private const string CorrelationIdKey = "data.correlationId";
        private const string RequestFormat = "json";

        public EesEventController(IAzureBlobEventWriter azureBlobEventWriter, IConfiguration configuration, MockService mockService)
        {
            _azureBlobEventWriter = azureBlobEventWriter;           
            _mockService = mockService;
            _configuration = configuration;
        }

        [HttpPost]
        [Route("/api/events")]
        public virtual async Task<IActionResult> Post([FromBody] JObject eventJson)
        {
            var correlationId = eventJson.SelectToken(CorrelationIdKey)?.Value<string>();
            
            await _azureBlobEventWriter.UploadEvent(eventJson.ToString(), correlationId!, correlationId + "_ees." + RequestFormat);

            if (bool.Parse(_configuration["IsFTRunning"]))
            {
                string currentTestCase = _mockService.GetCurrentTestCase();
   
                if (currentTestCase == TestCase.EESInternalServerError401.ToString())
                {
                    _mockService.CleanUp();
                    return new UnauthorizedObjectResult(StatusCodes.Status401Unauthorized);
                }
            }            
            return new OkObjectResult(StatusCodes.Status200OK);
        }
    }
}