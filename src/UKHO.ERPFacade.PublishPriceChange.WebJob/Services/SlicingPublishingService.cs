using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UKHO.ERPFacade.Common.Infrastructure;
using UKHO.ERPFacade.Common.Infrastructure.EventService;
using UKHO.ERPFacade.Common.Infrastructure.EventService.EventProvider;
using UKHO.ERPFacade.Common.IO.Azure;
using UKHO.ERPFacade.Common.Logging;
using UKHO.ERPFacade.Common.Models;
using UKHO.ERPFacade.Common.Services;

namespace UKHO.ERPFacade.PublishPriceChange.WebJob.Services
{
    public class SlicingPublishingService : ISlicingPublishingService
    {
        private readonly ILogger<SlicingPublishingService> _logger;
        private readonly IAzureTableReaderWriter _azureTableReaderWriter;
        private readonly IAzureBlobEventWriter _azureBlobEventWriter;
        private readonly IErpFacadeService _erpFacadeService;
        private readonly IEventPublisher _eventPublisher;
        private readonly ICloudEventFactory _cloudEventFactory;

        private const string IncompleteStatus = "Incomplete";
        private const string ContainerName = "pricechangeblobs";
        private const string BulkPriceInformationFileName = "BulkPriceInformation.json";
        private const string PriceInformationFileName = "PriceInformation.json";
        private const string PriceChangeEventFileName = "PriceChangeEvent.json";
        private int PublishProductsCounter = 0;
        private int UnpublishProductsCounter = 0;

        public SlicingPublishingService(ILogger<SlicingPublishingService> logger, IAzureTableReaderWriter azureTableReaderWriter, IAzureBlobEventWriter azureBlobEventWriter, IErpFacadeService erpFacadeService, IEventPublisher eventPublisher, ICloudEventFactory cloudEventFactory)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _azureTableReaderWriter = azureTableReaderWriter ?? throw new ArgumentNullException(nameof(azureTableReaderWriter));
            _azureBlobEventWriter = azureBlobEventWriter ?? throw new ArgumentNullException(nameof(azureBlobEventWriter));
            _erpFacadeService = erpFacadeService ?? throw new ArgumentNullException(nameof(erpFacadeService));
            _eventPublisher = eventPublisher ?? throw new ArgumentNullException(nameof(eventPublisher));
            _cloudEventFactory = cloudEventFactory ?? throw new ArgumentNullException(nameof(cloudEventFactory));
        }

        public void SliceAndPublishPriceChangeEvents()
        {
            var entities = _azureTableReaderWriter.GetMasterEntities(IncompleteStatus);
            string priceChangeInformationJson;

            foreach (var entity in entities)
            {
                _logger.LogInformation(EventIds.DownloadBulkPriceInformationEventFromAzureBlob.ToEventId(), "Webjob started downloading pricechange information from blob.");

                priceChangeInformationJson = _azureBlobEventWriter.DownloadEvent(entity.CorrId + '/' + BulkPriceInformationFileName, ContainerName);
                if (!string.IsNullOrEmpty(priceChangeInformationJson))
                {
                    List<PriceInformation> priceInformationList = JsonConvert.DeserializeObject<List<PriceInformation>>(priceChangeInformationJson);

                    var unitPriceInformationEntities = _azureTableReaderWriter.GetUnitPriceChangeEventsEntities(entity.CorrId);
                    if (unitPriceInformationEntities.Count > 0)
                    {
                        if (unitPriceInformationEntities.Any(i => i.Status == IncompleteStatus))
                        {
                            Parallel.ForEach(unitPriceInformationEntities.Where(i => i.Status == IncompleteStatus).ToList(), unitPriceInformation =>
                            {
                                lock (this)
                                {
                                    var unitPriceInformationList = priceInformationList.Where(p => p.ProductName == unitPriceInformation.UnitName).ToList();

                                    PriceChangeEventPayload priceChangeEventPayload = MapAndBuildPriceChangeEventPayload(unitPriceInformationList, entity.CorrId, unitPriceInformation.UnitName, unitPriceInformation.EventId);

                                    var priceChangeCloudEventData = _cloudEventFactory.Create(priceChangeEventPayload);

                                    var priceChangeCloudEventDataJson = JsonConvert.SerializeObject(priceChangeCloudEventData, Formatting.Indented);

                                    SavePriceChangeEventPayloadInAzureBlob(priceChangeCloudEventDataJson, entity.CorrId, unitPriceInformation.UnitName, unitPriceInformation.EventId);

                                    PublishEvent(priceChangeCloudEventData, entity.CorrId, unitPriceInformation.UnitName, unitPriceInformation.EventId);
                                }
                            });
                        }
                        else
                        {
                            _azureTableReaderWriter.UpdatePriceMasterStatusAndPublishDateTimeEntity(entity.CorrId);
                        }
                    }
                    else
                    {
                        var slicedPrices = priceInformationList.Select(p => p.ProductName).Distinct().ToList();
                        _logger.LogInformation(EventIds.ProductsToSliceCount.ToEventId(), "Total products to slice are {Count} | _X-Correlation-ID : {_X-Correlation-ID}", slicedPrices.Count, entity.CorrId);

                        string eventId;
                        PublishProductsCounter = 0;
                        UnpublishProductsCounter = 0;

                        Parallel.ForEach(slicedPrices, unitName =>
                        {
                            lock (this)
                            {
                                eventId = Guid.NewGuid().ToString();
                                var prices = priceInformationList.Where(p => p.ProductName == unitName).ToList();
                                var pricesJson = JArray.Parse(JsonConvert.SerializeObject(prices));

                                _azureTableReaderWriter.AddUnitPriceChangeEntity(entity.CorrId, eventId, unitName);

                                _logger.LogInformation(EventIds.UploadSlicedPriceInformationEventInAzureBlob.ToEventId(), "Uploading the sliced price information in blob storage. | _X-Correlation-ID : {_X-Correlation-ID} | PublishedEventId : {PublishedEventId}", entity.CorrId, eventId);

                                _azureBlobEventWriter.UploadEvent(pricesJson.ToString(), ContainerName, entity.CorrId + '/' + unitName + '/' + PriceInformationFileName);

                                _logger.LogInformation(EventIds.UploadedSlicedPriceInformationEventInAzureBlob.ToEventId(), "Sliced price information is uploaded in blob storage successfully. | _X-Correlation-ID : {_X-Correlation-ID} | PublishedEventId : {PublishedEventId}", entity.CorrId, eventId);

                                PriceChangeEventPayload priceChangeEventPayload = MapAndBuildPriceChangeEventPayload(prices, entity.CorrId, unitName, eventId);

                                var priceChangeCloudEventData = _cloudEventFactory.Create(priceChangeEventPayload);

                                var priceChangeCloudEventDataJson = JsonConvert.SerializeObject(priceChangeCloudEventData, Formatting.Indented);

                                SavePriceChangeEventPayloadInAzureBlob(priceChangeCloudEventDataJson.ToString(), entity.CorrId, unitName, eventId);

                                PublishEvent(priceChangeCloudEventData, entity.CorrId, unitName, eventId);
                            }
                        });

                        if (PublishProductsCounter == slicedPrices.Count)
                        {
                            _azureTableReaderWriter.UpdatePriceMasterStatusAndPublishDateTimeEntity(entity.CorrId);
                        }
                        _logger.LogInformation(EventIds.ProductsPublishedUnpublishedCount.ToEventId(), "Total products published are {Count} and unpublished are {unpublishedCount} | _X-Correlation-ID : {_X-Correlation-ID}", PublishProductsCounter, UnpublishProductsCounter, entity.CorrId);
                    }
                }
            }
        }

        private PriceChangeEventPayload MapAndBuildPriceChangeEventPayload(List<PriceInformation> unitPriceInformationList, string masterCorrId, string unitName, string eventId)
        {
            List<UnitsOfSalePrices> unitsOfSalePriceList = _erpFacadeService.MapAndBuildUnitsOfSalePrices(unitPriceInformationList, new() { unitName}, masterCorrId, eventId);

            PriceChangeEventPayload priceChangeEventPayload = _erpFacadeService.BuildPriceChangeEventPayload(unitsOfSalePriceList, unitName, masterCorrId, eventId);
            return priceChangeEventPayload;
        }

        private void PublishEvent(CloudEvent<PriceChangeEventData> priceChangeCloudEventData, string masterCorrId, string unitName, string eventId)
        {
            var result = _eventPublisher.Publish(priceChangeCloudEventData);

            if (result.Result.Status == Result.Statuses.Success)
            {
                _azureTableReaderWriter.UpdateUnitPriceChangeStatusAndPublishDateTimeEntity(masterCorrId, unitName, eventId);
                PublishProductsCounter++;
            }
            else
            {
                UnpublishProductsCounter++;
                _logger.LogWarning(EventIds.ProductsUnpublishedCount.ToEventId(), "Product {unitName} was not published successfully | _X-Correlation-ID : {_X-Correlation-ID} | PublishedEventId : {PublishedEventId}", unitName, masterCorrId, eventId);
            }
        }

        private void SavePriceChangeEventPayloadInAzureBlob(string priceChangeCloudEventDataJson, string masterCorrId, string unitName, string eventId)
        {
            _logger.LogInformation(EventIds.UploadPriceChangeEventPayloadInAzureBlob.ToEventId(), "Uploading the PriceChange event payload json for {UnitName} in blob storage. | _X-Correlation-ID : {_X-Correlation-ID} | PublishedEventId : {PublishedEventId}",unitName, masterCorrId, eventId);

            _azureBlobEventWriter.UploadEvent(priceChangeCloudEventDataJson, ContainerName, masterCorrId + '/' + unitName + '/' + PriceChangeEventFileName);

            _logger.LogInformation(EventIds.UploadedPriceChangeEventPayloadInAzureBlob.ToEventId(), "PriceChange event payload json for {UnitName} is uploaded in blob storage successfully. | _X-Correlation-ID : {_X-Correlation-ID} | PublishedEventId : {PublishedEventId}",unitName, masterCorrId, eventId);
        }
    }
}
