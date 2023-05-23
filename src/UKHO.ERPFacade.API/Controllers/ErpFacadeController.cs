using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Text;
using System.Text.Encodings.Web;
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
    public class ErpFacadeController : BaseController<ErpFacadeController>
    {
        private readonly ILogger<ErpFacadeController> _logger;
        private readonly IAzureTableReaderWriter _azureTableReaderWriter;
        private readonly IAzureBlobEventWriter _azureBlobEventWriter;
        private readonly IERPFacadeService _erpFacadeService;

        private const string CorrIdKey = "corrid";
        private const string RequestFormat = "json";
        private const int EventSizeLimit = 1000000;

        public ErpFacadeController(IHttpContextAccessor contextAccessor,
                                   ILogger<ErpFacadeController> logger,
                                   IAzureTableReaderWriter azureTableReaderWriter,
                                   IAzureBlobEventWriter azureBlobEventWriter,
                                   IERPFacadeService erpFacadeService)
        : base(contextAccessor)
        {
            _logger = logger;
            _azureTableReaderWriter = azureTableReaderWriter;
            _azureBlobEventWriter = azureBlobEventWriter;
            _erpFacadeService = erpFacadeService;
        }

        [HttpPost]
        [Route("/erpfacade/priceinformation")]
        public virtual async Task<IActionResult> PostPriceInformation([FromBody] JArray requestJson)
        {
            _logger.LogInformation(EventIds.SapUnitOfSalePriceEventReceived.ToEventId(), "ERP Facade has received UnitOfSale price event from SAP.");

            var corrId = requestJson.First.SelectToken(CorrIdKey)?.Value<string>();

            if (string.IsNullOrEmpty(corrId))
            {
                _logger.LogWarning(EventIds.CorrIdMissingInSAPEvent.ToEventId(), "Correlation Id is missing in the event received from the SAP.");
                return new BadRequestObjectResult(StatusCodes.Status400BadRequest);
            }

            await _azureTableReaderWriter.UpdateResponseTimeEntity(corrId);

            var isBlobExists = _azureBlobEventWriter.CheckIfContainerExists(corrId);

            if (!isBlobExists)
            {
                _logger.LogError(EventIds.BlobNotFoundInAzure.ToEventId(), "Blob does not exist in the Azure Storage for the correlation ID received from SAP event.");
                return new NotFoundObjectResult(StatusCodes.Status404NotFound);
            }

            _logger.LogInformation(EventIds.BlobExistsInAzure.ToEventId(), "Blob exists in the Azure Storage for the correlation ID received from SAP event.");

            List<PriceInformationEvent> priceInformationList = JsonConvert.DeserializeObject<List<PriceInformationEvent>>(requestJson.ToString());

            if (priceInformationList.Count > 0)
            {
                List<UnitsOfSalePrices> unitsOfSalePriceList = _erpFacadeService.BuildUnitOfSalePricePayload(priceInformationList);

                _logger.LogInformation(EventIds.DownloadExistingEesEventFromBlob.ToEventId(), "Downloading the existing EES event from azure blob storage with give Correlation ID.");
                var exisitingEesEvent = _azureBlobEventWriter.DownloadEvent(corrId + '.' + RequestFormat, corrId);
                _logger.LogInformation(EventIds.DownloadedExistingEesEventFromBlob.ToEventId(), "Existing EES event is downloaded from azure blob storage successfully.");

                JObject eesPriceEventPayloadJson = _erpFacadeService.BuildPriceEventPayload(unitsOfSalePriceList, exisitingEesEvent);

                var eventSize = CommonHelper.GetEventSize(eesPriceEventPayloadJson);
                if (eventSize > EventSizeLimit)
                {
                    _logger.LogWarning(EventIds.PriceEventExceedSizeLimit.ToEventId(), "Unit of Sale Price Event exceeds the size limit of 1 MB.");
                    throw new ERPFacadeException(EventIds.PriceEventExceedSizeLimit.ToEventId());
                }

                // Add code to publish the event in EES
            }
            else
            {
                _logger.LogError(EventIds.NoPriceInformationFound.ToEventId(), "No price information found in incoming SAP event.");
                throw new ERPFacadeException(EventIds.NoPriceInformationFound.ToEventId());
            }

            return new OkObjectResult(StatusCodes.Status200OK);
        }

        [HttpPost]
        [Route("/erpfacade/bulkpriceinformation")]
        public virtual async Task<IActionResult> PostBulkPriceInformation([FromBody] JArray requestJson)
        {
            _logger.LogInformation(EventIds.SapBulkPriceEventReceived.ToEventId(), "ERP Facade has received bulk price event from SAP.");

            List<PriceInformationEvent> bulkpriceInformationList = JsonConvert.DeserializeObject<List<PriceInformationEvent>>(requestJson.ToString());

            if (bulkpriceInformationList.Count > 0)
            {
                List<UnitsOfSalePrices> unitsOfSalePriceList = _erpFacadeService.BuildUnitOfSalePricePayload(bulkpriceInformationList);
                
                foreach(var priceList  in unitsOfSalePriceList)
                {
                    JObject eesBulkPriceEventPayloadJson = _erpFacadeService.BuildBulkPriceEventPayload(priceList);

                    //Add code to publish this event to EES
                }
            }
            else
            {
                _logger.LogError(EventIds.NoBulkPriceInformationFound.ToEventId(), "No bulk price information found in incoming SAP event.");
                throw new ERPFacadeException(EventIds.NoBulkPriceInformationFound.ToEventId());
            }

            return new OkObjectResult(StatusCodes.Status200OK);
        }
    }
}