using System.Globalization;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using UKHO.ERPFacade.Common.Logging;
using UKHO.ERPFacade.Common.Models;

namespace UKHO.ERPFacade.Common.Services
{
    public class ErpFacadeService : IErpFacadeService
    {
        private readonly ILogger<ErpFacadeService> _logger;

        public ErpFacadeService(ILogger<ErpFacadeService> logger)
        {
            _logger = logger;
        }

        public List<UnitsOfSalePrices> MapAndBuildUnitsOfSalePrices(List<PriceInformation> priceInformationList, List<string> unitOfSalesList)
        {
            List<UnitsOfSalePrices> unitsOfSalePriceList = new();

            foreach (string unitOfSale in unitOfSalesList)
            {
                UnitsOfSalePrices unitsOfSalePrice = new();
                List<Price> priceList = new();

                var unitPriceInformationList = priceInformationList.Where(x => x.ProductName == unitOfSale).ToList();

                if (unitPriceInformationList.Count > 0)
                {
                    foreach (var priceInformation in unitPriceInformationList)
                    {
                        bool isUnitOfSalePriceExists = unitsOfSalePriceList.Any(x => x.UnitName.Contains(priceInformation.ProductName));
                        if (!(priceInformation.ProductName == "PAYSF" && priceInformation.Duration == "12"))
                        {
                            if (!isUnitOfSalePriceExists)
                            {
                                if (!string.IsNullOrEmpty(priceInformation.EffectiveDate))
                                {
                                    DateTimeOffset effectiveDate = GetDate(priceInformation.EffectiveDate, priceInformation.EffectiveTime);
                                    Price effectivePrice = BuildPriceInformation(priceInformation.Duration, priceInformation.Price, effectiveDate, priceInformation.Currency);
                                    priceList.Add(effectivePrice);
                                }

                                if (!string.IsNullOrEmpty(priceInformation.FutureDate))
                                {
                                    DateTimeOffset futureDate = GetDate(priceInformation.FutureDate, priceInformation.FutureTime);
                                    Price futurePrice = BuildPriceInformation(priceInformation.Duration, priceInformation.FuturePrice, futureDate, priceInformation.FutureCurr);
                                    priceList.Add(futurePrice);
                                }

                                unitsOfSalePrice.UnitName = priceInformation.ProductName;
                                unitsOfSalePrice.Price = priceList;

                                unitsOfSalePriceList.Add(unitsOfSalePrice);
                            }
                            else
                            {
                                var existingUnitOfSalePrice = unitsOfSalePriceList.Where(x => x.UnitName.Contains(priceInformation.ProductName)).FirstOrDefault();

                                var effectiveUnitOfSalePriceDurations = existingUnitOfSalePrice!.Price.Where(x => x.EffectiveDate.ToString("yyyyMMdd") == priceInformation.EffectiveDate).ToList();
                                var effectiveStandard = effectiveUnitOfSalePriceDurations.Select(x => x.Standard).FirstOrDefault();

                                var futureUnitOfSalePriceDurations = existingUnitOfSalePrice.Price.Where(x => x.EffectiveDate.ToString("yyyyMMdd") == priceInformation.FutureDate).ToList();
                                var futureStandard = futureUnitOfSalePriceDurations.Select(x => x.Standard).FirstOrDefault();

                                if (effectiveStandard != null)
                                {
                                    if (!effectiveStandard.PriceDurations.Any(x => x.NumberOfMonths == Convert.ToInt32(priceInformation.Duration) && x.Rrp == Convert.ToDecimal(String.Format("{0:0.00}", Math.Round(Convert.ToDecimal(priceInformation.Price), 2)))))
                                    {
                                        PriceDurations priceDuration = new()
                                        {
                                            NumberOfMonths = Convert.ToInt32(priceInformation.Duration),
                                            Rrp = Convert.ToDecimal(string.Format("{0:0.00}", Math.Round(Convert.ToDecimal(priceInformation.Price), 2)))
                                        };

                                        effectiveStandard.PriceDurations.Add(priceDuration);
                                    }
                                }
                                else
                                {
                                    if (!string.IsNullOrEmpty(priceInformation.EffectiveDate))
                                    {
                                        DateTimeOffset effectiveDate = GetDate(priceInformation.EffectiveDate, priceInformation.EffectiveTime);
                                        Price effectivePrice = BuildPriceInformation(priceInformation.Duration, priceInformation.Price, effectiveDate, priceInformation.Currency);
                                        existingUnitOfSalePrice.Price.Add(effectivePrice);
                                    }
                                }
                                if (futureStandard != null)
                                {
                                    if (!futureStandard.PriceDurations.Any(x => x.NumberOfMonths == Convert.ToInt32(priceInformation.Duration) && x.Rrp == Convert.ToDecimal(String.Format("{0:0.00}", Math.Round(Convert.ToDecimal(priceInformation.FuturePrice), 2)))))
                                    {
                                        PriceDurations priceDuration = new()
                                        {
                                            NumberOfMonths = Convert.ToInt32(priceInformation.Duration),
                                            Rrp = Convert.ToDecimal(string.Format("{0:0.00}", Math.Round(Convert.ToDecimal(priceInformation.FuturePrice), 2)))
                                        };

                                        futureStandard.PriceDurations.Add(priceDuration);
                                    }
                                }
                                else
                                {
                                    if (!string.IsNullOrEmpty(priceInformation.FutureDate))
                                    {
                                        DateTimeOffset futureDate = GetDate(priceInformation.FutureDate, priceInformation.FutureTime);
                                        Price futurePrice = BuildPriceInformation(priceInformation.Duration, priceInformation.FuturePrice, futureDate, priceInformation.FutureCurr);
                                        existingUnitOfSalePrice.Price.Add(futurePrice);
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    _logger.LogWarning(EventIds.UnitsOfSaleNotFoundInSAPPriceInformationPayload.ToEventId(), "PriceInformation is missing for {UnitName} in price information payload received from SAP ", unitOfSale);

                    unitsOfSalePrice.UnitName = unitOfSale;
                    unitsOfSalePrice.Price = new();

                    unitsOfSalePriceList.Add(unitsOfSalePrice);
                }
            }
            return unitsOfSalePriceList;
        }

        public UnitOfSaleUpdatedEventPayload BuildUnitsOfSaleUpdatedEventPayload(List<UnitsOfSalePrices> unitsOfSalePriceList, string encEventPayloadJson)
        {
            _logger.LogInformation(EventIds.AppendingUnitofSalePricesToEncEvent.ToEventId(), "Appending UnitofSale prices to ENC event.");

            EncEventPayload encEventPayload = JsonConvert.DeserializeObject<EncEventPayload>(encEventPayloadJson)!;

            UnitOfSaleUpdatedEventData unitOfSaleUpdatedEventData = new()
            {
                CorrelationId = encEventPayload!.Data.CorrelationId,
                Products = encEventPayload.Data.Products,
                UnitsOfSale = encEventPayload.Data.UnitsOfSales,
                UnitsOfSalePrices = unitsOfSalePriceList
            };

            UnitOfSaleUpdatedEventPayload unitOfSaleUpdatedEventPayload = new(unitOfSaleUpdatedEventData, encEventPayload.Subject);

            _logger.LogInformation(EventIds.UnitsOfSaleUpdatedEventPayloadCreated.ToEventId(), "UnitofSale updated event payload created.");

            return unitOfSaleUpdatedEventPayload;
        }

        public PriceChangeEventPayload BuildPriceChangeEventPayload(List<UnitsOfSalePrices> unitsOfSalePriceList, string eventId, string unitName, string corrID)
        {
            _logger.LogInformation(EventIds.AppendingUnitofSalePricesToEncEventInWebJob.ToEventId(), "Appending UnitofSale prices to ENC event in webjob.");

            PriceChangeEventData priceChangeEventData = new()
            {
                CorrelationId = corrID,
                UnitsOfSalePrices = unitsOfSalePriceList,
            };

            PriceChangeEventPayload priceChangeEventPayload = new(priceChangeEventData, unitName, eventId);

            _logger.LogInformation(EventIds.PriceChangeEventPayloadCreated.ToEventId(), "pricechange event payload created.");

            return priceChangeEventPayload;
        }

        //private methods
        private static Price BuildPriceInformation(string duration, string rrp, DateTimeOffset date, string currency)
        {
            Price price = new();
            Standard standard = new();
            PriceDurations priceDurations = new();

            List<PriceDurations> priceDurationsList = new();

            priceDurations.NumberOfMonths = Convert.ToInt32(duration);
            priceDurations.Rrp = Convert.ToDecimal(string.Format("{0:0.00}", Math.Round(Convert.ToDecimal(rrp), 2)));
            priceDurationsList.Add(priceDurations);

            standard.PriceDurations = priceDurationsList;

            price.EffectiveDate = date;
            price.Currency = currency;
            price.Standard = standard;

            return price;
        }

        private static DateTimeOffset GetDate(string date, string time)
        {
            DateTime dateTime = DateTime.ParseExact(date + "" + time, "yyyyMMddHHmmss", CultureInfo.InvariantCulture);
            DateTimeOffset dateTimeOffset = new(dateTime);

            return dateTimeOffset;
        }
    }
}
