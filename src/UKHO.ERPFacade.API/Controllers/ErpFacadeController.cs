using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using UKHO.ERPFacade.Common.IO.Azure;
using UKHO.ERPFacade.Common.Logging;

namespace UKHO.ERPFacade.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ErpFacadeController : BaseController<ErpFacadeController>
    {
        private readonly ILogger<ErpFacadeController> _logger;
        private readonly IAzureTableReaderWriter _azureTableReaderWriter;
        private readonly IAzureBlobEventWriter _azureBlobEventWriter;

        private const string CorrelationIdKey = "data.correlationId";

        public ErpFacadeController(IHttpContextAccessor contextAccessor,
                                   ILogger<ErpFacadeController> logger,
                                   IAzureTableReaderWriter azureTableReaderWriter,
                                   IAzureBlobEventWriter azureBlobEventWriter)
        : base(contextAccessor)
        {
            _logger = logger;
            _azureTableReaderWriter = azureTableReaderWriter;
            _azureBlobEventWriter = azureBlobEventWriter;
        }

        [HttpPost]
        [Route("/erpfacade/priceinformation")]
        public virtual async Task<IActionResult> Post([FromBody] JObject requestJson)
        {
            var correlationId = requestJson.SelectToken(CorrelationIdKey)?.Value<string>();

            if (string.IsNullOrEmpty(correlationId))
            {
                _logger.LogWarning(EventIds.CorrelationIdMissingInSAPEvent.ToEventId(), "CorrelationId is missing in the event received from the SAP.");
                return new BadRequestObjectResult(StatusCodes.Status400BadRequest);
            }

            await _azureTableReaderWriter.UpdateResponseTimeEntity(correlationId);

            var isBlobExists = _azureBlobEventWriter.CheckIfContainerExists(correlationId);

            if (!isBlobExists)
            {
                _logger.LogError(EventIds.BlobNotFoundInAzure.ToEventId(), "Blob does not exist in the Azure Storage for the Correlation ID received from SAP event.");
                return new NotFoundObjectResult(StatusCodes.Status404NotFound);
            }

            _logger.LogInformation(EventIds.BlobExistsInAzure.ToEventId(), "Blob exists in the Azure Storage for the Correlation ID received from SAP event.");
            return new OkObjectResult(StatusCodes.Status200OK);
        }
    }
}