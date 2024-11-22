using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using UKHO.ERPFacade.API.Filters;
using UKHO.ERPFacade.Common.Logging;
using UKHO.ERPFacade.Services;

namespace UKHO.ERPFacade.API.Controllers
{
    [Route("api")]
    [ApiController]
    public class SapCallbackController : BaseController<SapCallbackController>
    {
        private readonly ILogger<SapCallbackController> _logger;
        private readonly ISapCallbackService _sapCallbackService;

        private const string CorrelationId = "correlationId";

        public SapCallbackController(IHttpContextAccessor contextAccessor,
                                     ILogger<SapCallbackController> logger,
                                     ISapCallbackService sapCallbackService)
        : base(contextAccessor)
        {
            _logger = logger;
            _sapCallbackService = sapCallbackService;
        }

        [HttpPost]
        [ServiceFilter(typeof(SharedApiKeyAuthFilter))]
        [Route("v2/callback/sap/s100actions/processed")]
        public virtual async Task<IActionResult> S100SapCallback([FromBody] JObject sapCallbackJson)
        {
            string correlationId = sapCallbackJson.Value<string>(CorrelationId);

            _logger.LogInformation(EventIds.S100SapCallbackPayloadReceived.ToEventId(), "S-100 SAP callback received for {CorrelationId}.", correlationId);

            if (string.IsNullOrEmpty(correlationId))
            {
                _logger.LogWarning(EventIds.CorrelationIdMissingInS100SapCallBack.ToEventId(), "CorrelationId is missing in S-100 SAP callback request.");
                return new BadRequestObjectResult(StatusCodes.Status400BadRequest);
            }

            if (!await _sapCallbackService.IsValidCallbackAsync(correlationId))
            {
                _logger.LogError(EventIds.InvalidS100SapCallback.ToEventId(), "Invalid S-100 SAP callback request. Requested correlationId not found.");
                return new NotFoundObjectResult(StatusCodes.Status404NotFound);
            }

            await _sapCallbackService.ProcessSapCallback(correlationId);

            return new OkObjectResult(StatusCodes.Status200OK);
        }
    }
}
