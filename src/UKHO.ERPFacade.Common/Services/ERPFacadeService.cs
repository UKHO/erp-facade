﻿using System.Globalization;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using UKHO.ERPFacade.Common.Logging;
using UKHO.ERPFacade.Common.Models;

namespace UKHO.ERPFacade.Common.Services
{
    public class ErpFacadeService : IErpFacadeService
    {
        private readonly ILogger<ErpFacadeService> _logger;

        private const string EffectiveDateFormat = "yyyyMMdd";
        private const string DateTimeFormat = "yyyyMMddHHmmss";

        public ErpFacadeService(ILogger<ErpFacadeService> logger)
        {
            _logger = logger;
        }

        public List<UnitsOfSalePrices> MapAndBuildUnitsOfSalePrices(List<PriceInformation> priceInformationList, List<string> unitOfSalesList, string correlationId, string eventId)
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

                                var effectiveUnitOfSalePriceDurations = existingUnitOfSalePrice!.Price.Where(x => x.EffectiveDate.ToString(EffectiveDateFormat) == priceInformation.EffectiveDate).ToList();
                                var effectiveStandard = effectiveUnitOfSalePriceDurations.Select(x => x.Standard).FirstOrDefault();

                                var futureUnitOfSalePriceDurations = existingUnitOfSalePrice.Price.Where(x => x.EffectiveDate.ToString(EffectiveDateFormat) == priceInformation.FutureDate).ToList();
                                var futureStandard = futureUnitOfSalePriceDurations.Select(x => x.Standard).FirstOrDefault();

                                if (effectiveStandard != null)
                                {
                                    if (!effectiveStandard.PriceDurations.Any(x => x.NumberOfMonths == Convert.ToInt32(priceInformation.Duration) && x.Rrp == GetPriceInDecimal(priceInformation.Price)))
                                    {
                                        PriceDurations priceDuration = new()
                                        {
                                            NumberOfMonths = Convert.ToInt32(priceInformation.Duration),
                                            Rrp = GetPriceInDecimal(priceInformation.Price)
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
                                    if (!futureStandard.PriceDurations.Any(x => x.NumberOfMonths == Convert.ToInt32(priceInformation.Duration) && x.Rrp == GetPriceInDecimal(priceInformation.FuturePrice)))
                                    {
                                        PriceDurations priceDuration = new()
                                        {
                                            NumberOfMonths = Convert.ToInt32(priceInformation.Duration),
                                            Rrp = GetPriceInDecimal(priceInformation.FuturePrice)
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
                    _logger.LogWarning(EventIds.UnitsOfSaleNotFoundInSAPPriceInformationPayload.ToEventId(), "PriceInformation is missing for {UnitName} in price information payload received from SAP. | _X-Correlation-ID : {_X-Correlation-ID} | PublishedEventId : {PublishedEventId}", unitOfSale, correlationId, eventId);

                    unitsOfSalePrice.UnitName = unitOfSale;
                    unitsOfSalePrice.Price = new();

                    unitsOfSalePriceList.Add(unitsOfSalePrice);
                }
            }
            return unitsOfSalePriceList;
        }

        public UnitOfSaleUpdatedEventPayload BuildUnitsOfSaleUpdatedEventPayload(List<UnitsOfSalePrices> unitsOfSalePriceList, string encEventPayloadJson, string correlationId, string eventId)
        {
            _logger.LogInformation(EventIds.AppendingUnitofSalePricesToEncEvent.ToEventId(), "Appending UnitofSale prices to ENC event. | _X-Correlation-ID : {_X-Correlation-ID} | PublishedEventId : {PublishedEventId}", correlationId, eventId);

            EncEventPayload encEventPayload = JsonConvert.DeserializeObject<EncEventPayload>(encEventPayloadJson)!;

            UnitOfSaleUpdatedEventData unitOfSaleUpdatedEventData = new()
            {
                CorrelationId = encEventPayload!.Data.CorrelationId,
                Products = encEventPayload.Data.Products,
                UnitsOfSale = encEventPayload.Data.UnitsOfSales,
                UnitsOfSalePrices = unitsOfSalePriceList
            };

            UnitOfSaleUpdatedEventPayload unitOfSaleUpdatedEventPayload = new(unitOfSaleUpdatedEventData, encEventPayload.Subject, eventId);

            _logger.LogInformation(EventIds.UnitsOfSaleUpdatedEventPayloadCreated.ToEventId(), "UnitofSale updated event payload created. | _X-Correlation-ID : {_X-Correlation-ID} | PublishedEventId : {PublishedEventId}", correlationId, eventId);

            return unitOfSaleUpdatedEventPayload;
        }

        public PriceChangeEventPayload BuildPriceChangeEventPayload(List<UnitsOfSalePrices> unitsOfSalePriceList, string unitName, string correlationId, string eventId)
        {
            _logger.LogInformation(EventIds.AppendingUnitofSalePricesToEncEventInWebJob.ToEventId(), "Appending UnitofSale prices to ENC event in webjob. | _X-Correlation-ID : {_X-Correlation-ID} | PublishedEventId : {PublishedEventId}", correlationId, eventId);

            PriceChangeEventData priceChangeEventData = new()
            {
                CorrelationId = correlationId,
                UnitsOfSalePrices = unitsOfSalePriceList,
            };

            PriceChangeEventPayload priceChangeEventPayload = new(priceChangeEventData, unitName, eventId);

            _logger.LogInformation(EventIds.PriceChangeEventPayloadCreated.ToEventId(), "pricechange event payload created. | _X-Correlation-ID : {_X-Correlation-ID} | PublishedEventId : {PublishedEventId}", correlationId, eventId);

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
            priceDurations.Rrp = GetPriceInDecimal(rrp);
            priceDurationsList.Add(priceDurations);

            standard.PriceDurations = priceDurationsList;

            price.EffectiveDate = date;
            price.Currency = currency;
            price.Standard = standard;

            return price;
        }

        private static DateTimeOffset GetDate(string date, string time)
        {
            DateTime dateTime = DateTime.ParseExact(date + "" + time, DateTimeFormat, CultureInfo.InvariantCulture);
            DateTimeOffset dateTimeOffset = new(dateTime);

            return dateTimeOffset;
        }

        private static Decimal GetPriceInDecimal(string price)
        {
            Decimal priceInDecimal = Convert.ToDecimal(string.Format("{0:0.00}", Math.Round(Convert.ToDecimal(price), 2)));
            return priceInDecimal;
        }
    }
}
