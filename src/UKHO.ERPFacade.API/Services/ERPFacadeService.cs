using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Globalization;
using UKHO.ERPFacade.API.Helpers;
using UKHO.ERPFacade.API.Models;

namespace UKHO.ERPFacade.API.Services
{
    public class ERPFacadeService : IERPFacadeService
    {
        private readonly ILogger<ERPFacadeService> _logger;

        public ERPFacadeService(ILogger<ERPFacadeService> logger)
        {
            _logger = logger;
        }

        public List<UnitsOfSalePrices> BuildUnitOfSalePricePayload(List<PriceInformationEvent> priceInformationList)
        {
            List<UnitsOfSalePrices> unitsOfSalePriceList = new();

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

            return unitsOfSalePriceList;
        }

        public JObject BuildEESEventWithPriceInformation(List<UnitsOfSalePrices> unitsOfSalePriceList, string exisitingEesEvent)
        {
            EESEventPayload eESEventPayload = JsonConvert.DeserializeObject<EESEventPayload>(exisitingEesEvent);

            UnitOfSalePriceEventPayload unitOfSalePriceEventPayload = new();
            UnitOfSalePriceEventData unitOfSalePriceEventData = new();

            unitOfSalePriceEventData.TraceId = eESEventPayload.Data.TraceId;
            unitOfSalePriceEventData.Products = eESEventPayload.Data.Products;
            unitOfSalePriceEventData._COMMENT = "Prices for all units in event will be included, including Cancelled Cell";
            unitOfSalePriceEventData.UnitsOfSales = eESEventPayload.Data.UnitsOfSales;
            unitOfSalePriceEventData.UnitsOfSalePrices = unitsOfSalePriceList;

            unitOfSalePriceEventPayload.SpecVersion = eESEventPayload.SpecVersion;
            unitOfSalePriceEventPayload.Type = eESEventPayload.Type;
            unitOfSalePriceEventPayload.Source = eESEventPayload.Source;
            unitOfSalePriceEventPayload.Id = eESEventPayload.Id;
            unitOfSalePriceEventPayload.Time = eESEventPayload.Time;
            unitOfSalePriceEventPayload._COMMENT = "A comma separated list of products";
            unitOfSalePriceEventPayload.Subject = eESEventPayload.Subject;
            unitOfSalePriceEventPayload.DataContentType = eESEventPayload.DataContentType;
            unitOfSalePriceEventPayload.Data = unitOfSalePriceEventData;

            JObject unitOfSalePriceEventPayloadJson = JObject.Parse(JsonConvert.SerializeObject(unitOfSalePriceEventPayload));

            return unitOfSalePriceEventPayloadJson;
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
