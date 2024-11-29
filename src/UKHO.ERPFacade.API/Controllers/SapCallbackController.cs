using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using UKHO.ERPFacade.API.Filters;
using UKHO.ERPFacade.Common.Constants;
using UKHO.ERPFacade.Common.Logging;
using UKHO.ERPFacade.Common.Models;
using UKHO.ERPFacade.Common.Operations;
using UKHO.ERPFacade.Services;

namespace UKHO.ERPFacade.API.Controllers
{
    [Route("api")]
    [ApiController]
    public class SapCallbackController : BaseController<SapCallbackController>
    {
        private readonly ILogger<SapCallbackController> _logger;
        private readonly IS100SapCallBackService _s100SapCallbackService;

        public SapCallbackController(IHttpContextAccessor contextAccessor,
                                     ILogger<SapCallbackController> logger,
                                     IS100SapCallBackService sapCallbackService)
        : base(contextAccessor)
        {
            _logger = logger;
            _s100SapCallbackService = sapCallbackService;
        }

        [HttpPost]
        [ServiceFilter(typeof(SharedApiKeyAuthFilter))]
        [Route("v2/callback/sap/s100actions/processed")]
        public virtual async Task<IActionResult> S100SapCallback([FromBody] JObject sapCallbackJson)
        {
            var correlationId = Extractor.ExtractTokenValue(sapCallbackJson, JsonFields.CorrelationIdKey);

            _logger.LogInformation(EventIds.S100SapCallbackPayloadReceived.ToEventId(), "S-100 SAP callback received.");

            if (string.IsNullOrEmpty(correlationId))
            {
                _logger.LogError(EventIds.CorrelationIdMissingInS100SapCallBack.ToEventId(), "CorrelationId is missing in S-100 SAP callback request.");

                var error = new List<Error>
                {
                    new()
                    {
                        Source = ErrorDetails.Source,
                        Description = ErrorDetails.CorrelationIdNotFoundMessage
                    }
                };

                return BuildBadRequestErrorResponse(error);
            }

            if (!await _s100SapCallbackService.IsValidCallbackAsync(correlationId))
            {
                _logger.LogError(EventIds.InvalidS100SapCallback.ToEventId(), "Invalid S-100 SAP callback request. Requested correlationId not found.");
                return new NotFoundObjectResult(StatusCodes.Status404NotFound);
            }

            await _s100SapCallbackService.ProcessSapCallbackAsync(correlationId);

            return new OkObjectResult(StatusCodes.Status200OK);
        }
    }
}
