using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Globalization;
using UKHO.ERPFacade.API.Models;
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
                            Price effectivePrice = BuildPricePayload(priceInformation.Duration, priceInformation.Price, priceInformation.EffectiveDate, priceInformation.Currency);
                            priceList.Add(effectivePrice);
                        }

                        if (!string.IsNullOrEmpty(priceInformation.FutureDate))
                        {
                            Price futurePrice = BuildPricePayload(priceInformation.Duration, priceInformation.FuturePrice, priceInformation.FutureDate, priceInformation.FutureCurr);
                            priceList.Add(futurePrice);
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

                        if (effectiveStandard != null && !string.IsNullOrEmpty(priceInformation.EffectiveDate))
                        {
                            priceDuration.NumberOfMonths = Convert.ToInt32(priceInformation.Duration);
                            priceDuration.Rrp = Convert.ToDouble(priceInformation.Price);

                            effectiveStandard.PriceDurations.Add(priceDuration);
                        }
                        else
                        {
                            Price effectivePrice = BuildPricePayload(priceInformation.Duration, priceInformation.Price, priceInformation.EffectiveDate, priceInformation.Currency);
                            existingUnitOfSalePrice.Price.Add(effectivePrice);
                        }
                        if (futureStandard != null && !string.IsNullOrEmpty(priceInformation.FutureDate))
                        {
                            priceDuration.NumberOfMonths = Convert.ToInt32(priceInformation.Duration);
                            priceDuration.Rrp = Convert.ToDouble(priceInformation.FuturePrice);

                            futureStandard.PriceDurations.Add(priceDuration);
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(priceInformation.FutureDate))
                            {
                                Price futurePrice = BuildPricePayload(priceInformation.Duration, priceInformation.FuturePrice, priceInformation.FutureDate, priceInformation.FutureCurr);
                                existingUnitOfSalePrice.Price.Add(futurePrice);
                            }
                        }
                    }
                }
            }
            return new OkObjectResult(StatusCodes.Status200OK);
        }

        private static Price BuildPricePayload(string duration, string rrp, string date, string currency)
        {
            Price price = new();
            Standard standard = new();
            PriceDurations priceDurations = new();

            List<PriceDurations> priceDurationsList = new();

            priceDurations.NumberOfMonths = Convert.ToInt32(duration);
            priceDurations.Rrp = Convert.ToDouble(rrp);
            priceDurationsList.Add(priceDurations);

            standard.PriceDurations = priceDurationsList;

            price.EffectiveDate = Convert.ToDateTime(DateTime.ParseExact(date, "yyyyMMdd", CultureInfo.InvariantCulture));
            price.Currency = currency;
            price.Standard = standard;

            return price;
        }
    }
}