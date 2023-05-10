using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Amqp.Framing;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Globalization;
using System.Security.Policy;
using System.Text.Json.Nodes;
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

            var sapEvent = JsonConvert.DeserializeObject<List<PriceInformationEvent>>(requestJson.ToString());

            List<UnitsOfSalePrices> unitsOfSalePriceList = new();

            if (sapEvent != null)
            {
                //var unitNames = sapEvent.Select(x => x.ProductName).Distinct<string>().ToList();
                foreach (var item in sapEvent)
                {
                    List<Price> priceList = new();
                    UnitsOfSalePrices unitsOfSalePrices = new();
                    Price price = new();
                    Standard standard = new();
                    PriceDurations priceDurations = new();

                    var sapEventData = unitsOfSalePriceList.Where(x => x.UnitName.Contains(item.ProductName)).ToList();

                    if (sapEventData.Count > 0)
                    {

                        if (!string.IsNullOrEmpty(item.EffectiveDate))
                        {
                            List<PriceDurations> priceDurationsList = new();

                            priceDurations.NumberOfMonths = Convert.ToInt32(item.Duration);
                            priceDurations.Rrp = Convert.ToDouble(item.Price);
                            priceDurationsList.Add(priceDurations);

                            standard.PriceDurations = priceDurationsList;

                            price.EffectiveDate = Convert.ToDateTime(DateTime.ParseExact(item.EffectiveDate, "yyyyMMdd", CultureInfo.InvariantCulture));
                            price.Currency = item.Currency;
                            price.Standard = standard;
                            priceList.Add(price);
                        }
                    }
                     




                    //var futureDates = sapEventData.Select(x => x.FutureDate).Distinct<string>().Where(i => !string.IsNullOrEmpty(i)).ToList();

                    //foreach (var futureDate in futureDates)
                    //{
                    //    Price futurePrice = new();
                    //    Standard futureStandard = new();

                    //    List<PriceDurations> priceDurationsList1 = new();

                    //    if (!string.IsNullOrEmpty(futureDate))
                    //    {
                    //        var sapEventDataUnit = sapEventData.Where(x => x.FutureDate.Contains(futureDate)).ToList();

                    //        foreach (var data in sapEventDataUnit)
                    //        {
                    //            PriceDurations priceDurations = new();
                                
                    //            priceDurations.NumberOfMonths = Convert.ToInt32(data.Duration);
                    //            priceDurations.Rrp = Convert.ToDouble(data.FuturePrice);
                    //            priceDurationsList1.Add(priceDurations);
                    //        }
                    //    }
                        
                    //    futureStandard.PriceDurations = priceDurationsList1;

                    //    futurePrice.EffectiveDate = Convert.ToDateTime(DateTime.ParseExact(futureDate, "yyyyMMdd", CultureInfo.InvariantCulture));
                    //    futurePrice.Currency = sapEventData.Select(x => x.FutureCurr).First();
                    //    futurePrice.Standard = futureStandard;
                    //    priceList.Add(futurePrice);
                    //}

                    //unitsOfSalePrices.Price = priceList;
                    //unitsOfSalePriceList.Add(unitsOfSalePrices);
                }
            }
            return new OkObjectResult(StatusCodes.Status200OK);
        }
    }
}