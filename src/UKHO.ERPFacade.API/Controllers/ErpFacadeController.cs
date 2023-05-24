using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using UKHO.ERPFacade.API.Models;
using UKHO.ERPFacade.Common.Infrastructure.EventService;
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
        private readonly IEventPublisher _eventPublisher;
        private const string TraceIdKey = "data.traceId";

        public ErpFacadeController(IHttpContextAccessor contextAccessor,
                                   ILogger<ErpFacadeController> logger,
                                   IAzureTableReaderWriter azureTableReaderWriter,
                                   IAzureBlobEventWriter azureBlobEventWriter,
                                   IEventPublisher eventPublisher)
        : base(contextAccessor)
        {
            _logger = logger;
            _azureTableReaderWriter = azureTableReaderWriter;
            _azureBlobEventWriter = azureBlobEventWriter;
            _eventPublisher = eventPublisher;
        }

        [HttpPost]
        [Route("/priceinformation")]
        public virtual async Task<IActionResult> Post([FromBody] JObject requestJson)
        {
            var traceId = requestJson.SelectToken(TraceIdKey)?.Value<string>();

            if (string.IsNullOrEmpty(traceId))
            {
                _logger.LogWarning(EventIds.TraceIdMissingInSAPEvent.ToEventId(), "TraceId is missing in the event received from the SAP.");
                return new BadRequestObjectResult(StatusCodes.Status400BadRequest);
            }

            await _azureTableReaderWriter.UpdateResponseTimeEntity(traceId);

            var isBlobExists = _azureBlobEventWriter.CheckIfContainerExists(traceId);

            if (!isBlobExists)
            {
                _logger.LogError(EventIds.BlobNotFoundInAzure.ToEventId(), "Blob does not exist in the Azure Storage for the trace ID received from SAP event.");
                return new NotFoundObjectResult(StatusCodes.Status404NotFound);
            }

            _logger.LogInformation(EventIds.BlobExistsInAzure.ToEventId(), "Blob exists in the Azure Storage for the trace ID received from SAP event.");

            var payload = CreateUnitOfSalePriceEventPayload(requestJson);
            await _eventPublisher.Publish(payload);

            return new OkObjectResult(StatusCodes.Status200OK);
        }

        public UnitOfSalePriceEventPayload CreateUnitOfSalePriceEventPayload(JObject requestJson)
        {
            var eesEventData = JsonConvert.DeserializeObject<UnitOfSalePriceEvent>(requestJson.ToString());

            return new UnitOfSalePriceEventPayload(new UnitOfSalePriceEvent
            {
                SpecVersion = eesEventData.SpecVersion,
                Type = eesEventData.Type,
                Source = eesEventData.Source,
                Id = eesEventData.Id,
                Time = eesEventData.Time,
                Subject = eesEventData.Subject,
                DataContentType = eesEventData.DataContentType,
                Data = new UnitOfSalePriceEventData
                {
                    TraceId = eesEventData.Data.TraceId,
                    Products = eesEventData.Data.Products,
                    UnitsOfSales = eesEventData.Data.UnitsOfSales,
                    UnitsOfSalePrices = eesEventData.Data.UnitsOfSalePrices,
                }
            });
        }
    }
}