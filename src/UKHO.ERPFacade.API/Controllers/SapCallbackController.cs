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
        private readonly ISapCallBackService _sapCallBackService;
        private readonly IS100UnitOfSaleUpdatedEventPublishingService _s100UnitOfSaleUpdatedEventPublishingService;

        private const string CorrelationId = "correlationId";

        public SapCallbackController(IHttpContextAccessor contextAccessor,
                                     ILogger<SapCallbackController> logger,
                                     ISapCallBackService sapCallBackService,
                                     IS100UnitOfSaleUpdatedEventPublishingService s100UnitOfSaleUpdatedEventPublishingService)
        : base(contextAccessor)
        {
            _logger = logger;
            _s100UnitOfSaleUpdatedEventPublishingService = s100UnitOfSaleUpdatedEventPublishingService;
            _sapCallBackService = sapCallBackService;
        }

        [HttpPost]
        [ServiceFilter(typeof(SharedApiKeyAuthFilter))]
        [Route("v2/callback/sap/s100actions/processed")]
        public virtual async Task<IActionResult> S100SapCallBack([FromBody] JObject sapCallBackJson)
        {
            _logger.LogInformation(EventIds.S100SapCallBackPayloadReceived.ToEventId(), "S-100 sap callBack payload received from SAP.");

            string correlationId = sapCallBackJson.SelectToken(CorrelationId)?.Value<string>();

            if (string.IsNullOrEmpty(correlationId))
            {
                _logger.LogWarning(EventIds.CorrelationIdMissingInS100SapCallBack.ToEventId(), "CorrelationId is missing in S-100 sap call back.");
                return new BadRequestObjectResult(StatusCodes.Status400BadRequest);
            }

            if (!await _sapCallBackService.IsValidCallback(correlationId))
            {
                _logger.LogError(EventIds.InvalidS100SapCallback.ToEventId(), "Invalid SAP callback. Request from ERP Facade to SAP not found.");
                return new NotFoundObjectResult(StatusCodes.Status404NotFound);
            }

            _logger.LogInformation(EventIds.ValidS100SapCallback.ToEventId(), "Valid SAP callback.");

            await _sapCallBackService.UpdateResponseTimeEntity(correlationId);

            _logger.LogInformation(EventIds.DownloadS100UnitOfSaleUpdatedEventIsStarted.ToEventId(), "Download S100 Unit Of Sale Updated Event from blob container is started.");

            var baseCloudEvent = await _sapCallBackService.GetEventPayload(correlationId);

            _logger.LogInformation(EventIds.DownloadS100UnitOfSaleUpdatedEventIsCompleted.ToEventId(), "Download S100 Unit Of Sale Updated Event from blob container is completed.");

            _logger.LogInformation(EventIds.PublishingUnitOfSaleUpdatedEventToEesStarted.ToEventId(), "The publishing unit of sale updated event to EES is started.");

            var result = await _s100UnitOfSaleUpdatedEventPublishingService.PublishEvent(baseCloudEvent);

            if (!result.IsSuccess)
            {
                throw new ERPFacadeException(EventIds.ErrorOccurredInSapForRecordOfSalePublishedEvent.ToEventId(), "Error occurred while publishing the publishing unit of sale updated event to EES.");
            }

            _logger.LogInformation(EventIds.PublishingUnitOfSaleUpdatedEventSuccessfullyToEes.ToEventId(), "The publishing unit of sale updated event successfully to EES.");

            await _sapCallBackService.UpdateEventStatusAndEventPublishDateTimeEntity(correlationId);

            _logger.LogInformation(EventIds.UpdatedTheEncEventStatusAndPublishDateTimeEntity.ToEventId(), "Updated The Enc Event StatusAnd Publish Date Time Entity in enc event table.");

            return new OkObjectResult(StatusCodes.Status200OK);
        }
    }
}
