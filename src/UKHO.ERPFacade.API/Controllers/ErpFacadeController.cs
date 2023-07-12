using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UKHO.ERPFacade.API.Filters;
using UKHO.ERPFacade.Common.Exceptions;
using UKHO.ERPFacade.Common.Infrastructure;
using UKHO.ERPFacade.Common.Infrastructure.EventService;
using UKHO.ERPFacade.Common.Infrastructure.EventService.EventProvider;
using UKHO.ERPFacade.Common.IO;
using UKHO.ERPFacade.Common.IO.Azure;
using UKHO.ERPFacade.Common.Logging;
using UKHO.ERPFacade.Common.Models;
using UKHO.ERPFacade.Common.Services;

namespace UKHO.ERPFacade.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ErpFacadeController : BaseController<ErpFacadeController>
    {
        private readonly ILogger<ErpFacadeController> _logger;
        private readonly IAzureTableReaderWriter _azureTableReaderWriter;
        private readonly IAzureBlobEventWriter _azureBlobEventWriter;
        private readonly IErpFacadeService _erpFacadeService;
        private readonly IJsonHelper _jsonHelper;
        private readonly IEventPublisher _eventPublisher;
        private readonly ICloudEventFactory _cloudEventFactory;

        private const string CorrelationIdKey = "corrid";
        private const string PriceInformationFileName = "PriceInformation.json";
        private const string EncEventFileName = "EncPublishingEvent.json";
        private const int EventSizeLimit = 1000000;
        private const string ContainerName = "pricechangeblobs";
        private const string UnitOfSaleUpdatedEventFileName = "UnitOfSaleUpdatedEvent.json";
        private const string BulkPriceInformationFileName = "BulkPriceInformation.json";

        public ErpFacadeController(IHttpContextAccessor contextAccessor,
                                   ILogger<ErpFacadeController> logger,
                                   IAzureTableReaderWriter azureTableReaderWriter,
                                   IAzureBlobEventWriter azureBlobEventWriter,
                                   IErpFacadeService erpFacadeService,
                                   IJsonHelper jsonHelper,
                                   IEventPublisher eventPublisher,
                                   ICloudEventFactory cloudEventFactory)
        : base(contextAccessor)
        {
            _logger = logger;
            _azureTableReaderWriter = azureTableReaderWriter;
            _azureBlobEventWriter = azureBlobEventWriter;
            _erpFacadeService = erpFacadeService;
            _jsonHelper = jsonHelper;
            _eventPublisher = eventPublisher;
            _cloudEventFactory = cloudEventFactory;
        }

        [HttpPost]
        [Route("/erpfacade/priceinformation")]
        [ServiceFilter(typeof(SharedKeyAuthFilter))]
        public virtual async Task<IActionResult> PostPriceInformation([FromBody] JArray priceInformationJson)
        {
            _logger.LogInformation(EventIds.SapUnitsOfSalePriceInformationPayloadReceived.ToEventId(), "UnitsOfSale price information payload received from SAP.");

            string correlationId = priceInformationJson.First.SelectToken(CorrelationIdKey)?.Value<string>();

            if (string.IsNullOrEmpty(correlationId))
            {
                _logger.LogWarning(EventIds.CorrelationIdMissingInSAPPriceInformationPayload.ToEventId(), "CorrelationId is missing in price information payload recieved from SAP.");
                return new BadRequestObjectResult(StatusCodes.Status400BadRequest);
            }

            await _azureTableReaderWriter.UpdateResponseTimeEntity(correlationId);

            bool isBlobExists = _azureBlobEventWriter.CheckIfContainerExists(correlationId);

            if (!isBlobExists)
            {
                _logger.LogError(EventIds.ERPFacadeToSAPRequestNotFound.ToEventId(), "Invalid SAP callback. Request from ERP Facade to SAP not found.");
                return new NotFoundObjectResult(StatusCodes.Status404NotFound);
            }

            _logger.LogInformation(EventIds.ERPFacadeToSAPRequestFound.ToEventId(), "Valid SAP callback.");

            _logger.LogInformation(EventIds.UploadPriceInformationEventInAzureBlob.ToEventId(), "Uploading the price information received from SAP in blob storage.");

            await _azureBlobEventWriter.UploadEvent(priceInformationJson.ToString(), correlationId, PriceInformationFileName);

            _logger.LogInformation(EventIds.UploadedPriceInformationEventInAzureBlob.ToEventId(), "Price information is uploaded in blob storage successfully.");

            List<PriceInformation> priceInformationList = JsonConvert.DeserializeObject<List<PriceInformation>>(priceInformationJson.ToString());

            if (priceInformationList.Count > 0 && priceInformationList.Any(x => x.ProductName != string.Empty))
            {
                _logger.LogInformation(EventIds.DownloadEncEventPayloadStarted.ToEventId(), "Downloading the ENC event payload from azure blob storage.");

                string encEventPayloadJson = _azureBlobEventWriter.DownloadEvent(EncEventFileName, correlationId.ToLower());

                EncEventPayload encEventPayloadData = JsonConvert.DeserializeObject<EncEventPayload>(encEventPayloadJson.ToString());

                _logger.LogInformation(EventIds.DownloadEncEventPayloadCompleted.ToEventId(), "ENC event payload is downloaded from azure blob storage successfully.");

                List<string> encEventUnitOfSalesList = encEventPayloadData.Data.UnitsOfSales.Select(x => x.UnitName).ToList();

                List<UnitsOfSalePrices> unitsOfSalePriceList = _erpFacadeService.MapAndBuildUnitsOfSalePrices(priceInformationList, encEventUnitOfSalesList);

                UnitOfSaleUpdatedEventPayload unitsOfSaleUpdatedEventPayload = _erpFacadeService.BuildUnitsOfSaleUpdatedEventPayload(unitsOfSalePriceList, encEventPayloadJson);

                var unitsOfSaleUpdatedCloudEventData = _cloudEventFactory.Create(unitsOfSaleUpdatedEventPayload);

                var unitsOfSaleUpdatedCloudEventDataJson = JsonConvert.SerializeObject(unitsOfSaleUpdatedCloudEventData, Formatting.Indented);

                _logger.LogInformation(EventIds.UploadUnitsOfSaleUpdatedEventPayloadInAzureBlob.ToEventId(), "Uploading the UnitsOfSaleUpdated event payload json in blob storage.");

                await _azureBlobEventWriter.UploadEvent(unitsOfSaleUpdatedCloudEventDataJson.ToString(), correlationId!, UnitOfSaleUpdatedEventFileName);

                _logger.LogInformation(EventIds.UploadedUnitsOfSaleUpdatedEventPayloadInAzureBlob.ToEventId(), "UnitsOfSaleUpdated event payload json is uploaded in blob storage successfully.");

                int eventSize = _jsonHelper.GetPayloadJsonSize(unitsOfSaleUpdatedCloudEventDataJson.ToString());

                if (eventSize > EventSizeLimit)
                {
                    _logger.LogError(EventIds.UnitsOfSaleUpdatedEventSizeLimit.ToEventId(), "UnitsOfSaleUpdated event exceeds the size limit of 1 MB.");
                    throw new ERPFacadeException(EventIds.UnitsOfSaleUpdatedEventSizeLimit.ToEventId());
                }

                Result result = await _eventPublisher.Publish(unitsOfSaleUpdatedCloudEventData);

                if (result.Status == Result.Statuses.Success)
                {
                    await _azureTableReaderWriter.UpdatePublishDateTimeEntity(correlationId);
                    return new OkObjectResult(StatusCodes.Status200OK);
                }
                else
                {
                    throw new ERPFacadeException(EventIds.ErrorOccuredInEES.ToEventId());
                }
            }
            else
            {
                _logger.LogError(EventIds.NoDataFoundInSAPPriceInformationPayload.ToEventId(), "No data found in SAP price information payload.");
                throw new ERPFacadeException(EventIds.NoDataFoundInSAPPriceInformationPayload.ToEventId());
            }
        }

        [HttpPost]
        [Route("/erpfacade/bulkpriceinformation")]
        [ServiceFilter(typeof(SharedKeyAuthFilter))]
        public virtual async Task<IActionResult> PostBulkPriceInformation([FromBody] JArray bulkPriceInformationJson)
        {
            _logger.LogInformation(EventIds.SapBulkPriceInformationPayloadReceived.ToEventId(), "Bulk price information payload received from SAP.");

            string correlationId = GetCurrentCorrelationId();

            _logger.LogInformation(EventIds.StoreBulkPriceInformationEventInAzureTable.ToEventId(), "Storing the received Bulk price information event in azure table.");

            await _azureTableReaderWriter.AddPriceChangeEntity(correlationId);

            _logger.LogInformation(EventIds.UploadBulkPriceInformationEventInAzureBlob.ToEventId(), "Uploading the received Bulk price information event in blob storage.");

            await _azureBlobEventWriter.UploadEvent(bulkPriceInformationJson.ToString(), ContainerName, correlationId + '/' + BulkPriceInformationFileName);

            _logger.LogInformation(EventIds.UploadedBulkPriceInformationEventInAzureBlob.ToEventId(), "Bulk price information event is uploaded in blob storage successfully.");

            return new OkObjectResult(StatusCodes.Status200OK);
        }
    }
}
