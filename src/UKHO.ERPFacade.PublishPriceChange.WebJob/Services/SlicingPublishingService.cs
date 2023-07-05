using System.Globalization;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UKHO.ERPFacade.Common.IO.Azure;
using UKHO.ERPFacade.Common.Logging;
using UKHO.ERPFacade.Common.Models;

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
        private const string PriceInformationFileName = "PriceInformation.json";

        public SlicingPublishingService(ILogger<SlicingPublishingService> logger, IAzureTableReaderWriter azureTableReaderWriter, IAzureBlobEventWriter azureBlobEventWriter)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _azureTableReaderWriter = azureTableReaderWriter ?? throw new ArgumentNullException(nameof(azureTableReaderWriter));
            _azureBlobEventWriter = azureBlobEventWriter ?? throw new ArgumentNullException(nameof(azureBlobEventWriter));
        }

        public void SliceAndPublishPriceChangeEvents()
        {
            var entities = _azureTableReaderWriter.GetMasterEntities(IncompleteStatus);
            string priceChangeJson;
            JObject unitsOfSaleUpdatedEventPayloadJson;

            foreach (var entity in entities)
            {
                _logger.LogInformation(EventIds.DownloadBulkPriceInformationEventFromAzureBlob.ToEventId(), "Downloading Price Change information from blob");
                priceChangeJson = _azureBlobEventWriter.DownloadEvent(entity.CorrId + '/' + PriceInformationFileName, ContainerName);
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
                                UnitOfSalePriceEventPayload unitsOfSaleUpdatedEventPayload = BuildUnitsOfSaleUpdatedEventPayload(unitsOfSalePriceList, unitPriceChange.Eventid, unitPriceChange.UnitName, entity.CorrId);
                                unitsOfSaleUpdatedEventPayloadJson = JObject.Parse(JsonConvert.SerializeObject(unitsOfSaleUpdatedEventPayload.EventData));
                                _azureBlobEventWriter.UploadEvent(unitsOfSaleUpdatedEventPayloadJson.ToString(), ContainerName, entity.CorrId + '/' + unitPriceChange.UnitName + '/' + unitPriceChange.UnitName + '.' + RequestFormat);
                                _logger.LogInformation(EventIds.UploadedSlicedEventInAzureBlobForUnitPrices.ToEventId(), "Sliced event is uploaded in blob storage successfully for incomplete unit prices.");
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
                        var slicedPrices = priceInformationList.Select(p => p.ProductName).Distinct().ToList();
                        string eventId;

                        foreach (var unitName in slicedPrices)
                        {
                            eventId = Guid.NewGuid().ToString();
                            var prices = priceInformationList.Where(p => p.ProductName == unitName).ToList();
                            _azureTableReaderWriter.AddUnitPriceChangeEntity(entity.CorrId, eventId, unitName);
                            List<UnitsOfSalePrices> unitsOfSalePriceList = MapAndBuildUnitsOfSalePrices(prices);
                            UnitOfSalePriceEventPayload unitsOfSaleUpdatedEventPayload = BuildUnitsOfSaleUpdatedEventPayload(unitsOfSalePriceList, eventId, unitName, entity.CorrId);
                            unitsOfSaleUpdatedEventPayloadJson = JObject.Parse(JsonConvert.SerializeObject(unitsOfSaleUpdatedEventPayload.EventData));
                            _azureBlobEventWriter.UploadEvent(unitsOfSaleUpdatedEventPayloadJson.ToString(), ContainerName, entity.CorrId + '/' + unitName + '/' + unitName + '.' + RequestFormat);
                            _logger.LogInformation(EventIds.UploadedSlicedEventInAzureBlob.ToEventId(), "Sliced event is uploaded in blob storage successfully.");
                            //publish event 

                            if (true) //check publish status
                                _azureTableReaderWriter.UpdateUnitPriceChangeStatusEntity(entity.CorrId, unitName, eventId);

                        }
                    }
                }
            }
        }


        //private methods
        private UnitOfSalePriceEventPayload BuildUnitsOfSaleUpdatedEventPayload(List<UnitsOfSalePrices> unitsOfSalePriceList, string eventId, string unitName, string corrID)
        {
            _logger.LogInformation(EventIds.AppendingUnitofSalePricesToEncEventInWebJob.ToEventId(), "Appending UnitofSale prices to ENC event in webjob.");

            return new UnitOfSalePriceEventPayload(new UnitOfSalePriceEvent
            {
                SpecVersion = "1.0",
                Type = "uk.gov.ukho.erp.pricechange.v1",
                Source = "https://erp.ukho.gov.uk",
                Id = eventId,
                Time = new DateTimeOffset(DateTime.UtcNow).ToString(),
                _COMMENT = "A comma separated list of products",
                Subject = unitName,
                DataContentType = "application/json",
                Data = new UnitOfSalePriceEventData
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
                if (!(priceInformation.ProductName == "PAYSF" && priceInformation.Duration =="12"))
                {
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
