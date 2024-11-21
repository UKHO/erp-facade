using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using UKHO.ERPFacade.API.Filters;
using UKHO.ERPFacade.API.Services.EventPublishingServices;
using UKHO.ERPFacade.Common.Exceptions;
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
        private readonly IS100UnitOfSaleUpdatedEventPublishingService _s100UnitOfSaleUpdatedEventPublishingService;

        private const string CorrelationId = "correlationId";

        public SapCallbackController(IHttpContextAccessor contextAccessor,
                                     ILogger<SapCallbackController> logger,
                                     ISapCallbackService sapCallbackService,
                                     IS100UnitOfSaleUpdatedEventPublishingService s100UnitOfSaleUpdatedEventPublishingService)
        : base(contextAccessor)
        {
            _logger = logger;
            _s100UnitOfSaleUpdatedEventPublishingService = s100UnitOfSaleUpdatedEventPublishingService;
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

            _logger.LogInformation(EventIds.ValidS100SapCallback.ToEventId(), "Processing of valid S-100 SAP callback request started.");

            await _sapCallbackService.LogCallbackResponseTimeAsync(correlationId);

            _logger.LogInformation(EventIds.DownloadS100UnitOfSaleUpdatedEventIsStarted.ToEventId(), "Download S-100 Unit Of Sale Updated Event from blob container is started.");

            var baseCloudEvent = await _sapCallbackService.GetEventPayload(correlationId);

            _logger.LogInformation(EventIds.DownloadS100UnitOfSaleUpdatedEventIsCompleted.ToEventId(), "Download S-100 Unit Of Sale Updated Event from blob container is completed.");

            _logger.LogInformation(EventIds.PublishingUnitOfSaleUpdatedEventToEesStarted.ToEventId(), "The publishing unit of sale updated event to EES is started.");

            var result = await _s100UnitOfSaleUpdatedEventPublishingService.PublishEvent(baseCloudEvent, correlationId);

            if (!result.IsSuccess)
            {
                _logger.LogError(EventIds.ErrorOccurredWhilePublishingUnitOfSaleUpdatedEventToEes.ToEventId(), "Error occurred while publishing S-100 unit of sale updated event to EES. | Status : {Status}", result.Error);
                throw new ERPFacadeException(EventIds.ErrorOccurredWhilePublishingUnitOfSaleUpdatedEventToEes.ToEventId(), "Error occurred while publishing S-100 unit of sale updated event to EES.");
            }

            _logger.LogInformation(EventIds.UnitOfSaleUpdatedEventPublished.ToEventId(), "The unit of sale updated event published to EES successfully.");

            await _sapCallbackService.UpdateEventStatusAndEventPublishDateTimeEntity(correlationId);

            _logger.LogInformation(EventIds.S100DataContentPublishedEventTableEntryUpdated.ToEventId(), "Status and event published date time for S-100 data content published event is updated successfully.");

            return new OkObjectResult(StatusCodes.Status200OK);
        }
    }
}
