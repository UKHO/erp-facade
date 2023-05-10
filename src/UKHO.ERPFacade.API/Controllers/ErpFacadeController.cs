using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Globalization;
using UKHO.ERPFacade.API.Models;
using UKHO.ERPFacade.Common.IO;
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

        private const string CorrIdKey = "corrid";

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
        public virtual async Task<IActionResult> Post([FromBody] JArray requestJson)
        {
            _logger.LogInformation("ERP Facade has received UnitOfSale event from SAP with price information.");

            var traceId = requestJson.First.SelectToken(CorrIdKey)?.Value<string>();

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

            //Add code to map SAP event data to UnitOfSale event with price information

            var priceInformationList = JsonConvert.DeserializeObject<List<PriceInformationEvent>>(requestJson.ToString());

            List<UnitsOfSalePrices> unitsOfSalePriceList = new();

            if (priceInformationList.Count > 0)
            {
                foreach (var priceInformation in priceInformationList)
                {
                    UnitsOfSalePrices unitsOfSalePrice = new();
                    List<Price> priceList = new();

                    var unitOfSalePriceExists = unitsOfSalePriceList.Any(x => x.UnitName.Contains(priceInformation.ProductName));

                    if (!unitOfSalePriceExists)
                    {
                        if (!string.IsNullOrEmpty(priceInformation.EffectiveDate))
                        {
                            Price price = new();
                            Standard standard = new();
                            PriceDurations priceDurations = new();

                            List<PriceDurations> priceDurationsList = new();

                            priceDurations.NumberOfMonths = Convert.ToInt32(priceInformation.Duration);
                            priceDurations.Rrp = Convert.ToDouble(priceInformation.Price);
                            priceDurationsList.Add(priceDurations);

                            standard.PriceDurations = priceDurationsList;

                            price.EffectiveDate = Convert.ToDateTime(DateTime.ParseExact(priceInformation.EffectiveDate, "yyyyMMdd", CultureInfo.InvariantCulture));
                            price.Currency = priceInformation.Currency;
                            price.Standard = standard;
                            priceList.Add(price);
                        }

                        if (!string.IsNullOrEmpty(priceInformation.FutureDate))
                        {
                            Price price = new();
                            Standard standard = new();
                            PriceDurations priceDurations = new();

                            List<PriceDurations> priceDurationsList = new();

                            priceDurations.NumberOfMonths = Convert.ToInt32(priceInformation.Duration);
                            priceDurations.Rrp = Convert.ToDouble(priceInformation.FuturePrice);
                            priceDurationsList.Add(priceDurations);

                            standard.PriceDurations = priceDurationsList;

                            price.EffectiveDate = Convert.ToDateTime(DateTime.ParseExact(priceInformation.FutureDate, "yyyyMMdd", CultureInfo.InvariantCulture));
                            price.Currency = priceInformation.Currency;
                            price.Standard = standard;
                            priceList.Add(price);
                        }

                        unitsOfSalePrice.UnitName = priceInformation.ProductName;
                        unitsOfSalePrice.Price = priceList;

                        unitsOfSalePriceList.Add(unitsOfSalePrice);
                    }

                    else
                    {
                        PriceDurations priceDuration = new();

                        var existingUnitOfSalePrice = unitsOfSalePriceList.Where(x => x.UnitName.Contains(priceInformation.ProductName)).FirstOrDefault();

                        var effectiveUnitOfSalePriceDurations = existingUnitOfSalePrice.Price.Where(x => x.EffectiveDate.ToString("yyyyMMdd") == priceInformation.EffectiveDate).ToList();
                        var effectiveStandard = effectiveUnitOfSalePriceDurations.Select(x => x.Standard).FirstOrDefault();

                        var futureUnitOfSalePriceDurations = existingUnitOfSalePrice.Price.Where(x => x.EffectiveDate.ToString("yyyyMMdd") == priceInformation.FutureDate).ToList();
                        var futureStandard = futureUnitOfSalePriceDurations.Select(x => x.Standard).FirstOrDefault();

                        if (!string.IsNullOrEmpty(priceInformation.EffectiveDate))
                        {
                            priceDuration.NumberOfMonths = Convert.ToInt32(priceInformation.Duration);
                            priceDuration.Rrp = Convert.ToDouble(priceInformation.Price);

                            effectiveStandard.PriceDurations.Add(priceDuration);
                        }
                        if (!string.IsNullOrEmpty(priceInformation.FutureDate))
                        {
                            priceDuration.NumberOfMonths = Convert.ToInt32(priceInformation.Duration);
                            priceDuration.Rrp = Convert.ToDouble(priceInformation.FuturePrice);

                            futureStandard.PriceDurations.Add(priceDuration);
                        }
                    }
                }
            }
            return new OkObjectResult(StatusCodes.Status200OK);
        }
    }
}