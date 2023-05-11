using Microsoft.Extensions.Options;
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
