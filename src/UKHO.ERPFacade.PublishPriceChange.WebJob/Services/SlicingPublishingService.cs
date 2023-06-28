using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UKHO.ERPFacade.Common.Exceptions;
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
                                    var prices = priceInformationList.Where(p => p.ProductName == unitPriceInformation.UnitName).ToList();

                                    PriceChangeEventPayload priceChangeEventPayload = MapAndBuildPriceChangeEventPayload(prices, entity.CorrId, unitPriceInformation.UnitName, unitPriceInformation.EventId);

                                    var priceChangeCloudEventData = _cloudEventFactory.Create(priceChangeEventPayload);

                                    var priceChangeCloudEventDataJson = JObject.Parse(JsonConvert.SerializeObject(priceChangeCloudEventData));

                                    SavePriceChangeEventPayloadInAzureBlob(priceChangeCloudEventDataJson, entity.CorrId, unitPriceInformation.UnitName, unitPriceInformation.EventId);

                                    PublishEvent(priceChangeCloudEventData, entity.CorrId, unitPriceInformation.UnitName, unitPriceInformation.EventId);


                                }
                            });

                            //foreach (var unitPriceInformation in unitPriceInformationEntities.Where(i => i.Status == IncompleteStatus).ToList())
                            //{
                            //    var prices = priceInformationList.Where(p => p.ProductName == unitPriceInformation.UnitName).ToList();

                            //    PriceChangeEventPayload priceChangeEventPayload = MapAndBuildPriceChangeEventPayload(prices, entity.CorrId, unitPriceInformation.UnitName, unitPriceInformation.EventId);

                            //    var priceChangeCloudEventData = _cloudEventFactory.Create(priceChangeEventPayload);

                            //    var priceChangeCloudEventDataJson = JObject.Parse(JsonConvert.SerializeObject(priceChangeCloudEventData));

                            //    SavePriceChangeEventPayloadInAzureBlob(priceChangeCloudEventDataJson, entity.CorrId, unitPriceInformation.UnitName, unitPriceInformation.EventId);

                            //    PublishEvent(priceChangeCloudEventData, entity.CorrId, unitPriceInformation.UnitName, unitPriceInformation.EventId);
                            //}
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


                        Parallel.ForEach(slicedPrices, unitName =>
                        {
                            lock (this)
                            {
                                eventId = Guid.NewGuid().ToString();
                                var prices = priceInformationList.Where(p => p.ProductName == unitName).ToList();
                                var pricesJson = JArray.Parse(JsonConvert.SerializeObject(prices));

                                _azureTableReaderWriter.AddUnitPriceChangeEntity(entity.CorrId, eventId, unitName);

                                _azureBlobEventWriter.UploadEvent(pricesJson.ToString(), ContainerName, entity.CorrId + '/' + unitName + '/' + PriceInformationFileName);

                                PriceChangeEventPayload priceChangeEventPayload = MapAndBuildPriceChangeEventPayload(prices, entity.CorrId, unitName, eventId);

                                var priceChangeCloudEventData = _cloudEventFactory.Create(priceChangeEventPayload);

                                var priceChangeCloudEventDataJson = JObject.Parse(JsonConvert.SerializeObject(priceChangeCloudEventData));

                                SavePriceChangeEventPayloadInAzureBlob(priceChangeCloudEventDataJson, entity.CorrId, unitName, eventId);

                                PublishEvent(priceChangeCloudEventData, entity.CorrId, unitName, eventId);



                            }
                        });

                        //foreach (var unitName in slicedPrices)
                        //{
                        //    eventId = Guid.NewGuid().ToString();
                        //    var prices = priceInformationList.Where(p => p.ProductName == unitName).ToList();
                        //    var pricesJson = JArray.Parse(JsonConvert.SerializeObject(prices));

                        //    _azureTableReaderWriter.AddUnitPriceChangeEntity(entity.CorrId, eventId, unitName);

                        //    _azureBlobEventWriter.UploadEvent(pricesJson.ToString(), ContainerName, entity.CorrId + '/' + unitName + '/' + PriceInformationFileName);

                        //    PriceChangeEventPayload priceChangeEventPayload = MapAndBuildPriceChangeEventPayload(prices, entity.CorrId, unitName, eventId);

                        //    var priceChangeCloudEventData = _cloudEventFactory.Create(priceChangeEventPayload);

                        //    var priceChangeCloudEventDataJson = JObject.Parse(JsonConvert.SerializeObject(priceChangeCloudEventData));

                        //    SavePriceChangeEventPayloadInAzureBlob(priceChangeCloudEventDataJson, entity.CorrId, unitName, eventId);

                        //    PublishEvent(priceChangeCloudEventData, entity.CorrId, unitName, eventId);
                        //}
                    }
                }
            }
        }

        private PriceChangeEventPayload MapAndBuildPriceChangeEventPayload(List<PriceInformation> priceInformationList, string masterCorrId, string unitName, string eventId)
        {
            var prices = priceInformationList.Where(p => p.ProductName == unitName).ToList();

            List<UnitsOfSalePrices> unitsOfSalePriceList = _erpFacadeService.MapAndBuildUnitsOfSalePrices(prices, prices.Select(u => u.ProductName).Distinct().ToList());

            PriceChangeEventPayload priceChangeEventPayload = _erpFacadeService.BuildPriceChangeEventPayload(unitsOfSalePriceList, eventId, unitName, masterCorrId);
            return priceChangeEventPayload;
        }

        private void PublishEvent(CloudEvent<PriceChangeEventData> priceChangeCloudEventData, string masterCorrId, string unitName, string eventId)
        {
            var result = _eventPublisher.Publish(priceChangeCloudEventData);
            if (result.Result.Status == Result.Statuses.Success)
            {
                _azureTableReaderWriter.UpdateUnitPriceChangeStatusEntity(masterCorrId, unitName, eventId);

                _logger.LogInformation(EventIds.PriceChangeEventPushedToEES.ToEventId(), "pricechange event has been sent to EES successfully. | _X-Correlation-ID : {_X-Correlation-ID}", eventId);
            }
            else
            {
                _logger.LogError(EventIds.ErrorOccuredInEES.ToEventId(), "An error occured for pricechange event while processing your request in EES. | _X-Correlation-ID : {_X-Correlation-ID} | {Status}", eventId, result.Status);
                throw new ERPFacadeException(EventIds.ErrorOccuredInEES.ToEventId());
            }
        }

        private void SavePriceChangeEventPayloadInAzureBlob(JObject priceChangeCloudEventDataJson, string masterCorrId, string unitName, string correlationId)
        {
            _logger.LogInformation(EventIds.UploadPriceChangeEventPayloadInAzureBlob.ToEventId(), "Uploading the pricechange event payload json in blob storage. | _X-Correlation-ID : {_X-Correlation-ID}", correlationId);

            _azureBlobEventWriter.UploadEvent(priceChangeCloudEventDataJson.ToString(), ContainerName, masterCorrId + '/' + unitName + '/' + PriceChangeEventFileName);

            _logger.LogInformation(EventIds.UploadedPriceChangeEventPayloadInAzureBlob.ToEventId(), "pricechange event payload json is uploaded in blob storage successfully. | _X-Correlation-ID : {_X-Correlation-ID}", correlationId);
        }
    }
}
