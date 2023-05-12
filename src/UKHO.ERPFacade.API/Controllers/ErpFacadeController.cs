using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Text.Encodings.Web;
using UKHO.ERPFacade.API.Models;
using UKHO.ERPFacade.API.Services;
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
        public virtual async Task<IActionResult> Post([FromBody] JArray requestJson)
        {
            _logger.LogInformation("ERP Facade has received UnitOfSale event from SAP with price information.");

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

                _logger.LogInformation("Downloading the existing EES event from blob storage with give Correlation ID.");

                var exisitingEesEvent = _azureBlobEventWriter.DownloadEvent(corrId + '.' + RequestFormat, corrId);

                _logger.LogInformation("Existing EES event is downloaded from blob storage successfully.");

                JObject eESEventReponseJson = _erpFacadeService.BuildEESEventWithPriceInformation(unitsOfSalePriceList, exisitingEesEvent);

                Console.WriteLine(" Final EES Event JSon Payload created");
            }

            return new OkObjectResult(StatusCodes.Status200OK);
        }
    }
}