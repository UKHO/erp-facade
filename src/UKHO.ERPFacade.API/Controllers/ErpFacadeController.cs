using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UKHO.ERPFacade.API.Models;
using UKHO.ERPFacade.API.Services;
using UKHO.ERPFacade.Common.Exceptions;
using UKHO.ERPFacade.Common.IO;
using UKHO.ERPFacade.Common.IO.Azure;
using UKHO.ERPFacade.Common.Logging;

namespace UKHO.ERPFacade.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ErpFacadeController : BaseController<ErpFacadeController>
    {
        private readonly ILogger<ErpFacadeController> _logger;
        private readonly IAzureTableReaderWriter _azureTableReaderWriter;
        private readonly IAzureBlobEventWriter _azureBlobEventWriter;
        private readonly IErpFacadeService _erpFacadeService;
        private readonly IJsonHelper _jsonHelper;

        private const string CorrelationIdKey = "corrid";
        private const string RequestFormat = "json";
        private const int EventSizeLimit = 1000000;

        public ErpFacadeController(IHttpContextAccessor contextAccessor,
                                   ILogger<ErpFacadeController> logger,
                                   IAzureTableReaderWriter azureTableReaderWriter,
                                   IAzureBlobEventWriter azureBlobEventWriter,
                                   IErpFacadeService erpFacadeService,
                                   IJsonHelper jsonHelper)
        : base(contextAccessor)
        {
            _logger = logger;
            _azureTableReaderWriter = azureTableReaderWriter;
            _azureBlobEventWriter = azureBlobEventWriter;
            _erpFacadeService = erpFacadeService;
            _jsonHelper = jsonHelper;
        }

        [HttpPost]
        [Route("/erpfacade/priceinformation")]
        [Authorize(Policy = "PriceInformationApiCaller")]
        public virtual async Task<IActionResult> PostPriceInformation([FromBody] JArray priceInformationJson)
        {
            JObject unitsOfSaleUpdatedEventPayloadJson;

            _logger.LogInformation(EventIds.SapUnitsOfSalePriceInformationPayloadReceived.ToEventId(), "UnitsOfSale price information payload received from SAP.");

            string? correlationId = priceInformationJson.First.SelectToken(CorrelationIdKey)?.Value<string>();

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

            List<PriceInformation> priceInformationList = JsonConvert.DeserializeObject<List<PriceInformation>>(priceInformationJson.ToString());

            if (priceInformationList.Count > 0 && priceInformationList.Any(x => x.ProductName != string.Empty))
            {
                _logger.LogInformation(EventIds.DownloadEncEventPayloadStarted.ToEventId(), "Downloading the ENC event payload from azure blob storage.");

                string encEventPayloadJson = _azureBlobEventWriter.DownloadEvent(correlationId + '.' + RequestFormat, correlationId);

                EncEventPayload encEventPayloadData = JsonConvert.DeserializeObject<EncEventPayload>(encEventPayloadJson.ToString());

                _logger.LogInformation(EventIds.DownloadEncEventPayloadCompleted.ToEventId(), "ENC event payload is downloaded from azure blob storage successfully.");

                List<UnitsOfSalePrices> unitsOfSalePriceList = _erpFacadeService.MapAndBuildUnitsOfSalePrices(priceInformationList, encEventPayloadData.Data.UnitsOfSales);

                UnitOfSaleUpdatedEventPayload unitsOfSaleUpdatedEventPayload = _erpFacadeService.BuildUnitsOfSaleUpdatedEventPayload(unitsOfSalePriceList, encEventPayloadJson);

                unitsOfSaleUpdatedEventPayloadJson = JObject.Parse(JsonConvert.SerializeObject(unitsOfSaleUpdatedEventPayload));

                int eventSize = _jsonHelper.GetPayloadJsonSize(unitsOfSaleUpdatedEventPayloadJson.ToString());

                if (eventSize > EventSizeLimit)
                {
                    _logger.LogError(EventIds.PriceEventExceedSizeLimit.ToEventId(), "UnitsOfSale price event exceeds the size limit of 1 MB.");
                    throw new ERPFacadeException(EventIds.PriceEventExceedSizeLimit.ToEventId());
                }

                // Add code to publish the event in EES
            }
            else
            {
                _logger.LogError(EventIds.NoDataFoundInSAPPriceInformationPayload.ToEventId(), "No data found in SAP price information payload.");
                throw new ERPFacadeException(EventIds.NoDataFoundInSAPPriceInformationPayload.ToEventId());
            }

            return new OkObjectResult(unitsOfSaleUpdatedEventPayloadJson);
        }

        [HttpPost]
        [Route("/erpfacade/bulkpriceinformation")]
        [Authorize(Policy = "PriceInformationApiCaller")]
        public virtual async Task<IActionResult> PostBulkPriceInformation([FromBody] JArray requestJson)
        {
            await Task.CompletedTask;
            return new OkObjectResult(StatusCodes.Status200OK);
        }
    }
}
