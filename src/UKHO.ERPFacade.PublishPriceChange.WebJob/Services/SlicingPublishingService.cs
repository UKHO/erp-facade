using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using UKHO.ERPFacade.Common.IO.Azure;
using UKHO.ERPFacade.Common.Models;
using System.Globalization;
using UKHO.ERPFacade.Common.Logging;

namespace UKHO.ERPFacade.PublishPriceChange.WebJob.Services
{
    public class SlicingPublishingService : ISlicingPublishingService
    {
        private readonly ILogger<SlicingPublishingService> _logger;
        private readonly IAzureTableReaderWriter _azureTableReaderWriter;
        private readonly IAzureBlobEventWriter _azureBlobEventWriter;
        private const string IncompleteStatus = "Incomplete";
        private const string RequestFormat = "json";
        private const string ContainerName = "pricechangeblobs";

        public SlicingPublishingService(ILogger<SlicingPublishingService> logger, IAzureTableReaderWriter azureTableReaderWriter, IAzureBlobEventWriter azureBlobEventWriter)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _azureTableReaderWriter = azureTableReaderWriter ?? throw new ArgumentNullException(nameof(azureTableReaderWriter));
            _azureBlobEventWriter = azureBlobEventWriter ?? throw new ArgumentNullException(nameof(azureBlobEventWriter));
        }

        public void SliceAndPublishIncompeleteEvents()
        {
            var entities = _azureTableReaderWriter.GetMasterEntities(IncompleteStatus);
            string priceChangeJson = string.Empty;
            JObject unitsOfSaleUpdatedEventPayloadJson;

            foreach (var entity in entities)
            {
                priceChangeJson = _azureBlobEventWriter.DownloadEvent(entity.CorrId + '/' + entity.CorrId + '.' + RequestFormat, ContainerName);
                if (!string.IsNullOrEmpty(priceChangeJson))
                {
                    List<PriceChange> priceInformationList = JsonConvert.DeserializeObject<List<PriceChange>>(priceChangeJson);
                    var unitPriceChangeEntities = _azureTableReaderWriter.GetUnitPriceChangeEventsEntities(entity.CorrId);
                    if (unitPriceChangeEntities.Count() > 0)
                    {
                        if (unitPriceChangeEntities.Any(i => i.Status == IncompleteStatus))
                        {
                            foreach (var unitPriceChange in unitPriceChangeEntities.Where(i => i.Status == IncompleteStatus).ToList())
                            {
                                var prices = priceInformationList.Where(p => p.ProductName == unitPriceChange.UnitName).ToList();
                                List<UnitsOfSalePrices> unitsOfSalePriceList = MapAndBuildUnitsOfSalePrices(prices);
                                UnitOfSaleUpdatedEventPayload unitsOfSaleUpdatedEventPayload = BuildUnitsOfSaleUpdatedEventPayload(unitsOfSalePriceList, unitPriceChange.Eventid, unitPriceChange.UnitName, entity.CorrId);
                                unitsOfSaleUpdatedEventPayloadJson = JObject.Parse(JsonConvert.SerializeObject(unitsOfSaleUpdatedEventPayload.EventData));
                                _azureBlobEventWriter.UploadEvent(unitsOfSaleUpdatedEventPayloadJson.ToString(), ContainerName, entity.CorrId + '/' + unitPriceChange.UnitName + '/' + unitPriceChange.UnitName + '.' + RequestFormat);
                                //publish event 

                                if (true) //check publish status
                                    _azureTableReaderWriter.UpdateUnitPriceChangeStatusEntity(entity.CorrId, unitPriceChange.UnitName, unitPriceChange.Eventid);
                            }
                        }
                        else
                        {
                            _azureTableReaderWriter.UpdatePriceMasterStatusEntity(entity.CorrId);
                        }
                    }
                    else
                    {
                        var splicedPrices = priceInformationList.Select(p => p.ProductName).Distinct().ToList();
                        var eventId = string.Empty;


                        foreach (var unitName in splicedPrices)
                        {
                            eventId = Guid.NewGuid().ToString();
                            var prices = priceInformationList.Where(p => p.ProductName == unitName).ToList();
                            _azureTableReaderWriter.AddUnitPriceChangeEntity(entity.CorrId, eventId, unitName);
                            List<UnitsOfSalePrices> unitsOfSalePriceList = MapAndBuildUnitsOfSalePrices(prices);
                            UnitOfSaleUpdatedEventPayload unitsOfSaleUpdatedEventPayload = BuildUnitsOfSaleUpdatedEventPayload(unitsOfSalePriceList, eventId, unitName, entity.CorrId);
                            unitsOfSaleUpdatedEventPayloadJson = JObject.Parse(JsonConvert.SerializeObject(unitsOfSaleUpdatedEventPayload));
                            _azureBlobEventWriter.UploadEvent(unitsOfSaleUpdatedEventPayloadJson.ToString(), ContainerName, entity.CorrId + '/' + unitName + '/' + unitName + '.' + RequestFormat);
                            //publish event 

                            if (true) //check publish status
                                _azureTableReaderWriter.UpdateUnitPriceChangeStatusEntity(entity.CorrId, unitName, eventId);

                        }
                    }
                }
            }
        }


        //private methods
        private UnitOfSaleUpdatedEventPayload BuildUnitsOfSaleUpdatedEventPayload(List<UnitsOfSalePrices> unitsOfSalePriceList, string eventId, string unitName, string corrID)
        {
            _logger.LogInformation(EventIds.AppendingUnitofSalePricesToEncEventInWebJob.ToEventId(), "Appending UnitofSale prices to ENC event in webjob.");

            return new UnitOfSaleUpdatedEventPayload(new UnitOfSaleUpdatedEvent
            {
                SpecVersion = "1.0",
                Type = "uk.gov.ukho.erp.bulkpricechange.v1",
                Source = "https://erp.ukho.gov.uk",
                Id = eventId,
                Time = new DateTimeOffset(DateTime.UtcNow).ToString(),
                _COMMENT = "A comma separated list of products",
                Subject = unitName,
                DataContentType = "application/json",
                Data = new UnitOfSaleUpdatedEventData
                {
                    CorrelationId = corrID,
                    UnitsOfSalePrices = unitsOfSalePriceList,
                }
            });
        }

        private List<UnitsOfSalePrices> MapAndBuildUnitsOfSalePrices(List<PriceChange> priceInformationList)
        {
            List<UnitsOfSalePrices> unitsOfSalePriceList = new();

            foreach (var priceInformation in priceInformationList)
            {
                UnitsOfSalePrices unitsOfSalePrice = new();
                List<Price> priceList = new();

                var isUnitOfSalePriceExists = unitsOfSalePriceList.Any(x => x.UnitName.Contains(priceInformation.ProductName));

                if (!isUnitOfSalePriceExists)
                {
                    if (!string.IsNullOrEmpty(priceInformation.EffectiveDate))
                    {
                        DateTimeOffset effectiveDate = GetDate(priceInformation.EffectiveDate, priceInformation.EffectiveTime);
                        Price effectivePrice = BuildPricePayload(priceInformation.Duration, priceInformation.Price, effectiveDate, priceInformation.Currency);
                        priceList.Add(effectivePrice);
                    }

                    if (!string.IsNullOrEmpty(priceInformation.FutureDate))
                    {
                        DateTimeOffset futureDate = GetDate(priceInformation.FutureDate, priceInformation.FutureTime);
                        Price futurePrice = BuildPricePayload(priceInformation.Duration, priceInformation.FuturePrice, futureDate, priceInformation.FutureCurr);
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
                        priceDuration.Rrp = priceInformation.Price;

                        effectiveStandard.PriceDurations.Add(priceDuration);
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(priceInformation.EffectiveDate))
                        {
                            DateTimeOffset effectiveDate = GetDate(priceInformation.EffectiveDate, priceInformation.EffectiveTime);
                            Price effectivePrice = BuildPricePayload(priceInformation.Duration, priceInformation.Price, effectiveDate, priceInformation.Currency);
                            existingUnitOfSalePrice.Price.Add(effectivePrice);
                        }
                    }
                    if (futureStandard != null && !string.IsNullOrEmpty(priceInformation.FutureDate))
                    {
                        priceDuration.NumberOfMonths = Convert.ToInt32(priceInformation.Duration);
                        priceDuration.Rrp = priceInformation.FuturePrice;

                        futureStandard.PriceDurations.Add(priceDuration);
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(priceInformation.FutureDate))
                        {
                            DateTimeOffset futureDate = GetDate(priceInformation.FutureDate, priceInformation.FutureTime);
                            Price futurePrice = BuildPricePayload(priceInformation.Duration, priceInformation.FuturePrice, futureDate, priceInformation.FutureCurr);
                            existingUnitOfSalePrice.Price.Add(futurePrice);
                        }
                    }
                }
            }
            return unitsOfSalePriceList;
        }

        private static Price BuildPricePayload(string duration, string rrp, DateTimeOffset date, string currency)
        {
            Price price = new();
            Standard standard = new();
            PriceDurations priceDurations = new();

            List<PriceDurations> priceDurationsList = new();

            priceDurations.NumberOfMonths = Convert.ToInt32(duration);
            priceDurations.Rrp = rrp;
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
