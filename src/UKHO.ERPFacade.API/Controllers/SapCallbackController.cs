using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using UKHO.ERPFacade.API.Filters;
using UKHO.ERPFacade.API.Services;
using UKHO.ERPFacade.API.Services.EventPublishingService;
using UKHO.ERPFacade.Common.Constants;
using UKHO.ERPFacade.Common.Enums;
using UKHO.ERPFacade.Common.Logging;
using UKHO.ERPFacade.Common.Operations.IO.Azure;

namespace UKHO.ERPFacade.API.Controllers
{
    [Route("api")]
    [ApiController]
    public class SapCallbackController : BaseController<SapCallbackController>
    {
        private readonly ILogger<SapCallbackController> _logger;
        private readonly ISapCallBackService _sapCallBackService;
        private readonly IS100UnitOfSaleUpdatedEventPublishingService _s100UnitOfSaleUpdatedEventPublishingService;
        private readonly IAzureTableReaderWriter _azureTableReaderWriter;

        private const string CorrelationId = "correlationId";

        public SapCallbackController(IHttpContextAccessor contextAccessor,
                                     ILogger<SapCallbackController> logger,
                                     ISapCallBackService sapCallBackService,
                                     IS100UnitOfSaleUpdatedEventPublishingService s100UnitOfSaleUpdatedEventPublishingService,
                                     IAzureTableReaderWriter azureTableReaderWriter)
        : base(contextAccessor)
        {
            _logger = logger;
            _s100UnitOfSaleUpdatedEventPublishingService = s100UnitOfSaleUpdatedEventPublishingService;
            _sapCallBackService = sapCallBackService;
            _azureTableReaderWriter = azureTableReaderWriter;
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

            var baseCloudEvent = await _sapCallBackService.GetEventPayload(correlationId);

            var result = await _s100UnitOfSaleUpdatedEventPublishingService.PublishEvent(baseCloudEvent);

            if (!result.IsSuccess)
            {
                _logger.LogError(EventIds.LicenceUpdatedPublishedEventUpdatePushedToSap.ToEventId(), "Error occurred while publishing the publishing unit of sale updated event to EES.");
                return new BadRequestObjectResult(StatusCodes.Status400BadRequest);
            }

            _logger.LogInformation(EventIds.LicenceUpdatedPublishedEventUpdatePushedToSap.ToEventId(), "The publishing unit of sale updated event successfully to EES.");

            await _azureTableReaderWriter.UpdateEntityAsync(PartitionKeys.S100PartitionKey, correlationId, new[] { new KeyValuePair<string, string>("Status", Status.Complete.ToString()) });

            return new OkObjectResult(StatusCodes.Status200OK);
        }
    }
}
