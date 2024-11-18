using Azure.Data.Tables;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using UKHO.ERPFacade.API.Filters;
using UKHO.ERPFacade.API.Services;
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
        private readonly IAzureTableReaderWriter _azureTableReaderWriter;
        private readonly ISapCallBackService _sapCallBackService;
        private readonly IAzureBlobReaderWriter _azureBlobReaderWriter;
        private const string CorrelationId = "correlationId";

        public SapCallbackController(IHttpContextAccessor contextAccessor, ILogger<SapCallbackController> logger, IAzureTableReaderWriter azureTableReaderWriter, ISapCallBackService sapCallBackService, IAzureBlobReaderWriter azureBlobReaderWriter) : base(contextAccessor)
        {
            _logger = logger;
            _azureTableReaderWriter = azureTableReaderWriter;
            _sapCallBackService = sapCallBackService;
            _azureBlobReaderWriter = azureBlobReaderWriter;
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

            Task<TableEntity> entity = _azureTableReaderWriter.GetEntityAsync(PartitionKeys.S100PartitionKey, correlationId);
            if (entity is not { Result: not null })
            {
                _logger.LogError(EventIds.InvalidS100SapCallback.ToEventId(), "Invalid SAP callback. Request from ERP Facade to SAP not found.");
                return new NotFoundObjectResult(StatusCodes.Status404NotFound);
            }

            _logger.LogInformation(EventIds.ValidS100SapCallback.ToEventId(), "Valid SAP callback.");

            await _azureTableReaderWriter.UpdateResponseTimeEntity(correlationId);

            bool isBlobExists = _azureBlobReaderWriter.CheckIfContainerExists(correlationId);

            if (!isBlobExists)
            {
                _logger.LogError(EventIds.BlobContainerIsNotExists.ToEventId(), "S-100 data publishing container is not exists in azure blob storage.");
                return new NotFoundObjectResult(StatusCodes.Status404NotFound);
            }

            await _sapCallBackService.DownloadS100EventAndPublishToEes(correlationId);

            _logger.LogInformation(EventIds.LicenceUpdatedPublishedEventUpdatePushedToSap.ToEventId(), "The publishing unit of sale updated event successfully to EES.");

            await _azureTableReaderWriter.UpdateEntityAsync(PartitionKeys.S100PartitionKey, correlationId, new[] { new KeyValuePair<string, string>("Status", Status.Complete.ToString()) });

            return new OkResult();
        }
    }
}
