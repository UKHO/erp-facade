using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NUnit.Framework;
using UKHO.ERPFacade.Common.Exceptions;
using UKHO.ERPFacade.Common.Infrastructure;
using UKHO.ERPFacade.Common.Infrastructure.EventService;
using UKHO.ERPFacade.Common.Infrastructure.EventService.EventProvider;
using UKHO.ERPFacade.Common.IO;
using UKHO.ERPFacade.Common.IO.Azure;
using UKHO.ERPFacade.Common.Logging;
using UKHO.ERPFacade.Common.Models;
using UKHO.ERPFacade.Common.Models.TableEntities;
using UKHO.ERPFacade.Common.Services;
using UKHO.ERPFacade.PublishPriceChange.WebJob.Services;
using static UKHO.ERPFacade.Common.Infrastructure.Result;

namespace UKHO.ERPFacade.PublishPriceChange.WebJob.UnitTests.Services
{
    [TestFixture]
    public class SlicingPublishingServiceTests
    {
        private IAzureTableReaderWriter _fakeAzureTableReaderWriter;
        private ILogger<SlicingPublishingService> _fakeLogger;
        private IAzureBlobEventWriter _fakeAzureBlobEventWriter;
        private IErpFacadeService _fakeErpFacadeService;
        private IEventPublisher _fakeEventPublisher;
        private ICloudEventFactory _fakeCloudEventFactory;
        private IJsonHelper _fakeJsonHelper;
        private SlicingPublishingService _fakeSlicingPublishingService;

        [SetUp]
        public void Setup()
        {
            _fakeAzureTableReaderWriter = A.Fake<IAzureTableReaderWriter>();
            _fakeLogger = A.Fake<ILogger<SlicingPublishingService>>();
            _fakeAzureBlobEventWriter = A.Fake<IAzureBlobEventWriter>();
            _fakeErpFacadeService = A.Fake<IErpFacadeService>();
            _fakeEventPublisher = A.Fake<IEventPublisher>();
            _fakeCloudEventFactory = A.Fake<ICloudEventFactory>();
            _fakeJsonHelper = A.Fake<IJsonHelper>();
            _fakeSlicingPublishingService = new SlicingPublishingService(_fakeLogger, _fakeAzureTableReaderWriter, _fakeAzureBlobEventWriter, _fakeErpFacadeService, _fakeEventPublisher, _fakeCloudEventFactory, _fakeJsonHelper);
        }

        private readonly string PriceChangeInformationJson = "[\r\n  {\r\n    \"corrid\": \"\",\r\n    \"org\": \"UKHO\",\r\n    \"productname\": \"PAYSF\",\r\n    \"duration\": \"12\",\r\n    \"effectivedate\": \"20230427\",\r\n    \"effectivetime\": \"101454\",\r\n    \"price\": \"156.00 \",\r\n    \"currency\": \"USD\",\r\n    \"futuredate\": \"20230527\",\r\n    \"futuretime\": \"000001\",\r\n    \"futureprice\": \"180.00\",\r\n    \"futurecurr\": \"USD\",\r\n    \"reqdate\": \"20230328\",\r\n    \"reqtime\": \"160000\"\r\n  }\r\n]";

        [Test]
        public void Does_Constructor_Throws_ArgumentNullException_When_AzureTableReaderWriter_Paramter_Is_Null()
        {
            Assert.Throws<ArgumentNullException>(
             () => new SlicingPublishingService(_fakeLogger, null, _fakeAzureBlobEventWriter, _fakeErpFacadeService, _fakeEventPublisher, _fakeCloudEventFactory, _fakeJsonHelper))
             .ParamName
             .Should().Be("azureTableReaderWriter");
        }

        [Test]
        public void Does_Constructor_Throws_ArgumentNullException_When_Logger_Paramter_Is_Null()
        {
            Assert.Throws<ArgumentNullException>(
             () => new SlicingPublishingService(null, _fakeAzureTableReaderWriter, _fakeAzureBlobEventWriter, _fakeErpFacadeService, _fakeEventPublisher, _fakeCloudEventFactory, _fakeJsonHelper))
             .ParamName
             .Should().Be("logger");
        }

        [Test]
        public void Does_Constructor_Throws_ArgumentNullException_When_AzureBlobEventWriter_Paramter_Is_Null()
        {
            Assert.Throws<ArgumentNullException>(
             () => new SlicingPublishingService(_fakeLogger, _fakeAzureTableReaderWriter, null, _fakeErpFacadeService, _fakeEventPublisher, _fakeCloudEventFactory, _fakeJsonHelper))
             .ParamName
             .Should().Be("azureBlobEventWriter");
        }

        [Test]
        public void WhenErpFacadeServiceParamterIsNull_ThenConstructorThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(
             () => new SlicingPublishingService(_fakeLogger, _fakeAzureTableReaderWriter, _fakeAzureBlobEventWriter, null, _fakeEventPublisher, _fakeCloudEventFactory, _fakeJsonHelper))
             .ParamName
             .Should().Be("erpFacadeService");
        }

        [Test]
        public void WhenEventPublisherParamterIsNull_ThenConstructorThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(
             () => new SlicingPublishingService(_fakeLogger, _fakeAzureTableReaderWriter, _fakeAzureBlobEventWriter, _fakeErpFacadeService, null, _fakeCloudEventFactory, _fakeJsonHelper))
             .ParamName
             .Should().Be("eventPublisher");
        }

        [Test]
        public void WhenCloudEventFactoryParamterIsNull_ThenConstructorThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(
             () => new SlicingPublishingService(_fakeLogger, _fakeAzureTableReaderWriter, _fakeAzureBlobEventWriter, _fakeErpFacadeService, _fakeEventPublisher, null, _fakeJsonHelper))
             .ParamName
             .Should().Be("cloudEventFactory");
        }

        [Test]
        public void WhenJsonHelperParamterIsNull_ThenConstructorThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(
             () => new SlicingPublishingService(_fakeLogger, _fakeAzureTableReaderWriter, _fakeAzureBlobEventWriter, _fakeErpFacadeService, _fakeEventPublisher, _fakeCloudEventFactory, null))
             .ParamName
             .Should().Be("jsonHelper");
        }

        [Test]
        public void WhenAllPriceEntitiesAreCompleted_ThenShouldNotPublish()
        {
            IList<PriceChangeMasterEntity> priceChangeMasterEntities = new List<PriceChangeMasterEntity>();
            A.CallTo(() => _fakeAzureTableReaderWriter.GetMasterEntities(A<string>.Ignored, A<string>.Ignored)).Returns(priceChangeMasterEntities);

            _fakeSlicingPublishingService.SliceAndPublishPriceChangeEvents();

            A.CallTo(() => _fakeAzureBlobEventWriter.DownloadEvent(A<string>.Ignored, A<string>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => _fakeAzureTableReaderWriter.GetUnitPriceChangeEventsEntities(A<string>.Ignored, A<string>.Ignored, A<string>.Ignored, A<string>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => _fakeAzureBlobEventWriter.UploadEvent(A<string>.Ignored, A<string>.Ignored, A<string>.Ignored)).MustNotHaveHappened();
        }

        [Test]
        public void WhenPriceEntitiesAreIncompleted_ThenShouldSliceAndPublish()
        {
            IList<PriceChangeMasterEntity> priceChangeMasterEntities = new List<PriceChangeMasterEntity>
            {
                new()
                {
                    CorrId = "FakeCorrID",
                    ETag = new(),
                    PartitionKey = "FakeCorrID",
                    RowKey = "FakeCorrID",
                    Status = "Incomplete",
                    Timestamp = new DateTimeOffset()
                }
            };
            IList<UnitPriceChangeEntity> unitPriceChangeEntities = new List<UnitPriceChangeEntity>();
            List<UnitsOfSalePrices> unitsOfSalePriceList = GetUnitsOfSalePrices();
            PriceChangeEventPayload priceChangeEventPayload = GePriceChangeEventPayload();
            CloudEvent<PriceChangeEventData> priceChangeCloudEventData = GeCloudEventPayload();

            A.CallTo(() => _fakeAzureTableReaderWriter.GetMasterEntities(A<string>.Ignored, A<string>.Ignored)).Returns(priceChangeMasterEntities);
            A.CallTo(() => _fakeAzureBlobEventWriter.DownloadEvent(A<string>.Ignored, A<string>.Ignored)).Returns(PriceChangeInformationJson);
            A.CallTo(() => _fakeAzureTableReaderWriter.GetUnitPriceChangeEventsEntities(A<string>.Ignored, A<string>.Ignored, A<string>.Ignored, A<string>.Ignored)).Returns(unitPriceChangeEntities);
            A.CallTo(() => _fakeErpFacadeService.MapAndBuildUnitsOfSalePrices(A<List<PriceInformation>>.Ignored, A<List<string>>.Ignored)).Returns(unitsOfSalePriceList);
            A.CallTo(() => _fakeErpFacadeService.BuildPriceChangeEventPayload(A<List<UnitsOfSalePrices>>.Ignored, A<string>.Ignored, A<string>.Ignored, A<string>.Ignored)).Returns(priceChangeEventPayload);
            A.CallTo(() => _fakeCloudEventFactory.Create(A<PriceChangeEventPayload>.Ignored)).Returns(priceChangeCloudEventData);
            A.CallTo(() => _fakeJsonHelper.GetPayloadJsonSize(A<string>.Ignored)).Returns(1000000);
            A.CallTo(() => _fakeEventPublisher.Publish(A<CloudEvent<PriceChangeEventData>>.Ignored)).Returns(new Result(Statuses.Success, "Successfully sent event"));

            _fakeSlicingPublishingService.SliceAndPublishPriceChangeEvents();

            A.CallTo(() => _fakeAzureTableReaderWriter.GetMasterEntities(A<string>.Ignored, A<string>.Ignored)).MustHaveHappened();
            A.CallTo(() => _fakeAzureBlobEventWriter.DownloadEvent(A<string>.Ignored, A<string>.Ignored)).MustHaveHappened();
            A.CallTo(() => _fakeAzureTableReaderWriter.GetUnitPriceChangeEventsEntities(A<string>.Ignored, A<string>.Ignored, A<string>.Ignored, A<string>.Ignored)).MustHaveHappened();
            A.CallTo(() => _fakeAzureTableReaderWriter.AddUnitPriceChangeEntity(A<string>.Ignored, A<string>.Ignored, A<string>.Ignored)).MustHaveHappened();
            A.CallTo(() => _fakeErpFacadeService.MapAndBuildUnitsOfSalePrices(A<List<PriceInformation>>.Ignored, A<List<string>>.Ignored)).MustHaveHappened();
            A.CallTo(() => _fakeErpFacadeService.BuildPriceChangeEventPayload(A<List<UnitsOfSalePrices>>.Ignored, A<string>.Ignored, A<string>.Ignored, A<string>.Ignored)).MustHaveHappened();
            A.CallTo(() => _fakeCloudEventFactory.Create(A<PriceChangeEventPayload>.Ignored)).MustHaveHappened();
            A.CallTo(() => _fakeAzureBlobEventWriter.UploadEvent(A<string>.Ignored, A<string>.Ignored, A<string>.Ignored)).MustHaveHappened();
            A.CallTo(() => _fakeJsonHelper.GetPayloadJsonSize(A<string>.Ignored)).MustHaveHappened();
            A.CallTo(() => _fakeEventPublisher.Publish(A<CloudEvent<PriceChangeEventData>>.Ignored)).MustHaveHappened();
            A.CallTo(() => _fakeAzureTableReaderWriter.UpdateUnitPriceChangeStatusEntity(A<string>.Ignored, A<string>.Ignored, A<string>.Ignored)).MustHaveHappened();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
            && call.GetArgument<LogLevel>(0) == LogLevel.Information
            && call.GetArgument<EventId>(1) == EventIds.DownloadBulkPriceInformationEventFromAzureBlob.ToEventId()
            && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Webjob started downloading pricechange information from blob.").MustHaveHappenedOnceExactly();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
            && call.GetArgument<LogLevel>(0) == LogLevel.Information
            && call.GetArgument<EventId>(1) == EventIds.UploadPriceChangeEventPayloadInAzureBlob.ToEventId()
            && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Uploading the pricechange event payload json in blob storage. | _X-Correlation-ID : {_X-Correlation-ID}").MustHaveHappenedOnceExactly();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
            && call.GetArgument<LogLevel>(0) == LogLevel.Information
            && call.GetArgument<EventId>(1) == EventIds.UploadedPriceChangeEventPayloadInAzureBlob.ToEventId()
            && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "pricechange event payload json is uploaded in blob storage successfully. | _X-Correlation-ID : {_X-Correlation-ID}").MustHaveHappenedOnceExactly();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
            && call.GetArgument<LogLevel>(0) == LogLevel.Information
            && call.GetArgument<EventId>(1) == EventIds.PriceChangeEventPushedToEES.ToEventId()
            && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "pricechange event has been sent to EES successfully. | _X-Correlation-ID : {_X-Correlation-ID}").MustHaveHappenedOnceExactly();
        }

        [Test]
        public void WhenUnitPriceEntitiesAreIncompleted_ThenShouldSliceAndPublish()
        {
            IList<PriceChangeMasterEntity> priceChangeMasterEntities = new List<PriceChangeMasterEntity>
            {
                new()
                {
                    CorrId = "FakeCorrID",
                    ETag = new(),
                    PartitionKey = "FakeCorrID",
                    RowKey = "FakeCorrID",
                    Status = "Incomplete",
                    Timestamp = new DateTimeOffset()
                }
            };
            IList<UnitPriceChangeEntity> unitPriceChangeEntities = new List<UnitPriceChangeEntity>
            {
                new()
                {
                    EventId = "FakeEventID",
                    MasterCorrId = "FakeCorrID",
                    RowKey = "FakeEventID",
                    ETag = new(),
                    PartitionKey = "FakeEventID",
                    Status = "Incomplete",
                    Timestamp = new DateTimeOffset(),
                    UnitName = "FakeUnitName"
                },
                new()
                {
                    EventId = "FakeEventID2",
                    MasterCorrId = "FakeCorrID",
                    RowKey = "FakeEventID2",
                    ETag = new(),
                    PartitionKey = "FakeEventID2",
                    Status = "Complete",
                    Timestamp = new DateTimeOffset(),
                    UnitName = "FakeUnitName"
                }
            };
            List<UnitsOfSalePrices> unitsOfSalePriceList = GetUnitsOfSalePrices();
            PriceChangeEventPayload priceChangeEventPayload = GePriceChangeEventPayload();
            CloudEvent<PriceChangeEventData> priceChangeCloudEventData = GeCloudEventPayload();

            A.CallTo(() => _fakeAzureTableReaderWriter.GetMasterEntities(A<string>.Ignored, A<string>.Ignored)).Returns(priceChangeMasterEntities);
            A.CallTo(() => _fakeAzureBlobEventWriter.DownloadEvent(A<string>.Ignored, A<string>.Ignored)).Returns(PriceChangeInformationJson);
            A.CallTo(() => _fakeAzureTableReaderWriter.GetUnitPriceChangeEventsEntities(A<string>.Ignored, A<string>.Ignored, A<string>.Ignored, A<string>.Ignored)).Returns(unitPriceChangeEntities);
            A.CallTo(() => _fakeErpFacadeService.MapAndBuildUnitsOfSalePrices(A<List<PriceInformation>>.Ignored, A<List<string>>.Ignored)).Returns(unitsOfSalePriceList);
            A.CallTo(() => _fakeErpFacadeService.BuildPriceChangeEventPayload(A<List<UnitsOfSalePrices>>.Ignored, A<string>.Ignored, A<string>.Ignored, A<string>.Ignored)).Returns(priceChangeEventPayload);
            A.CallTo(() => _fakeCloudEventFactory.Create(A<PriceChangeEventPayload>.Ignored)).Returns(priceChangeCloudEventData);
            A.CallTo(() => _fakeJsonHelper.GetPayloadJsonSize(A<string>.Ignored)).Returns(1000000);
            A.CallTo(() => _fakeEventPublisher.Publish(A<CloudEvent<PriceChangeEventData>>.Ignored)).Returns(new Result(Statuses.Success, "Successfully sent event"));

            _fakeSlicingPublishingService.SliceAndPublishPriceChangeEvents();

            A.CallTo(() => _fakeAzureTableReaderWriter.GetMasterEntities(A<string>.Ignored, A<string>.Ignored)).MustHaveHappened();
            A.CallTo(() => _fakeAzureBlobEventWriter.DownloadEvent(A<string>.Ignored, A<string>.Ignored)).MustHaveHappened();
            A.CallTo(() => _fakeAzureTableReaderWriter.GetUnitPriceChangeEventsEntities(A<string>.Ignored, A<string>.Ignored, A<string>.Ignored, A<string>.Ignored)).MustHaveHappened();
            A.CallTo(() => _fakeErpFacadeService.MapAndBuildUnitsOfSalePrices(A<List<PriceInformation>>.Ignored, A<List<string>>.Ignored)).MustHaveHappened();
            A.CallTo(() => _fakeErpFacadeService.BuildPriceChangeEventPayload(A<List<UnitsOfSalePrices>>.Ignored, A<string>.Ignored, A<string>.Ignored, A<string>.Ignored)).MustHaveHappened();
            A.CallTo(() => _fakeCloudEventFactory.Create(A<PriceChangeEventPayload>.Ignored)).MustHaveHappened();
            A.CallTo(() => _fakeAzureBlobEventWriter.UploadEvent(A<string>.Ignored, A<string>.Ignored, A<string>.Ignored)).MustHaveHappened();
            A.CallTo(() => _fakeJsonHelper.GetPayloadJsonSize(A<string>.Ignored)).MustHaveHappened();
            A.CallTo(() => _fakeEventPublisher.Publish(A<CloudEvent<PriceChangeEventData>>.Ignored)).MustHaveHappened();
            A.CallTo(() => _fakeAzureTableReaderWriter.UpdateUnitPriceChangeStatusEntity(A<string>.Ignored, A<string>.Ignored, A<string>.Ignored)).MustHaveHappened();
            A.CallTo(() => _fakeAzureTableReaderWriter.UpdatePriceMasterStatusEntity(A<string>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => _fakeAzureTableReaderWriter.AddUnitPriceChangeEntity(A<string>.Ignored, A<string>.Ignored, A<string>.Ignored)).MustNotHaveHappened();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
           && call.GetArgument<LogLevel>(0) == LogLevel.Information
           && call.GetArgument<EventId>(1) == EventIds.DownloadBulkPriceInformationEventFromAzureBlob.ToEventId()
           && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Webjob started downloading pricechange information from blob.").MustHaveHappenedOnceExactly();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
            && call.GetArgument<LogLevel>(0) == LogLevel.Information
            && call.GetArgument<EventId>(1) == EventIds.UploadPriceChangeEventPayloadInAzureBlob.ToEventId()
            && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Uploading the pricechange event payload json in blob storage. | _X-Correlation-ID : {_X-Correlation-ID}").MustHaveHappenedOnceExactly();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
            && call.GetArgument<LogLevel>(0) == LogLevel.Information
            && call.GetArgument<EventId>(1) == EventIds.UploadedPriceChangeEventPayloadInAzureBlob.ToEventId()
            && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "pricechange event payload json is uploaded in blob storage successfully. | _X-Correlation-ID : {_X-Correlation-ID}").MustHaveHappenedOnceExactly();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
            && call.GetArgument<LogLevel>(0) == LogLevel.Information
            && call.GetArgument<EventId>(1) == EventIds.PriceChangeEventPushedToEES.ToEventId()
            && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "pricechange event has been sent to EES successfully. | _X-Correlation-ID : {_X-Correlation-ID}").MustHaveHappenedOnceExactly();
        }

        [Test]
        public void WhenUnitPriceEntitiesAreCompleted_ThenShouldUpdatePriceMasterStatusToComplete()
        {
            IList<PriceChangeMasterEntity> priceChangeMasterEntities = new List<PriceChangeMasterEntity>
            {
                new()
                {
                    CorrId = "FakeCorrID",
                    ETag = new(),
                    PartitionKey = "FakeCorrID",
                    RowKey = "FakeCorrID",
                    Status = "Incomplete",
                    Timestamp = new DateTimeOffset()
                }
            };
            IList<UnitPriceChangeEntity> unitPriceChangeEntities = new List<UnitPriceChangeEntity>
            {
                new UnitPriceChangeEntity()
                {
                    EventId = "FakeEventID",
                    MasterCorrId = "FakeCorrID",
                    RowKey = "FakeEventID",
                    ETag = new(),
                    PartitionKey = "FakeEventID",
                    Status = "Complete",
                    Timestamp = new DateTimeOffset(),
                    UnitName = "FakeUnitName"
                }
            };

            A.CallTo(() => _fakeAzureTableReaderWriter.GetMasterEntities(A<string>.Ignored, A<string>.Ignored)).Returns(priceChangeMasterEntities);
            A.CallTo(() => _fakeAzureBlobEventWriter.DownloadEvent(A<string>.Ignored, A<string>.Ignored)).Returns(PriceChangeInformationJson);
            A.CallTo(() => _fakeAzureTableReaderWriter.GetUnitPriceChangeEventsEntities(A<string>.Ignored, A<string>.Ignored, A<string>.Ignored, A<string>.Ignored)).Returns(unitPriceChangeEntities);

            _fakeSlicingPublishingService.SliceAndPublishPriceChangeEvents();

            A.CallTo(() => _fakeAzureBlobEventWriter.DownloadEvent(A<string>.Ignored, A<string>.Ignored)).MustHaveHappened();
            A.CallTo(() => _fakeAzureTableReaderWriter.GetUnitPriceChangeEventsEntities(A<string>.Ignored, A<string>.Ignored, A<string>.Ignored, A<string>.Ignored)).MustHaveHappened();
            A.CallTo(() => _fakeAzureBlobEventWriter.UploadEvent(A<string>.Ignored, A<string>.Ignored, A<string>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => _fakeAzureTableReaderWriter.UpdateUnitPriceChangeStatusEntity(A<string>.Ignored, A<string>.Ignored, A<string>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => _fakeAzureTableReaderWriter.UpdatePriceMasterStatusEntity(A<string>.Ignored)).MustHaveHappened();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
            && call.GetArgument<LogLevel>(0) == LogLevel.Information
            && call.GetArgument<EventId>(1) == EventIds.AppendingUnitofSalePricesToEncEventInWebJob.ToEventId()
            && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Appending UnitofSale prices to ENC event in webjob.").MustNotHaveHappened();
        }

        [Test]
        public void WhenPublishToEESFails_ThenErpFacadeThrowsExceptionErrorOccuredInEES()
        {
            IList<PriceChangeMasterEntity> priceChangeMasterEntities = new List<PriceChangeMasterEntity>
            {
                new()
                {
                    CorrId = "FakeCorrID",
                    ETag = new(),
                    PartitionKey = "FakeCorrID",
                    RowKey = "FakeCorrID",
                    Status = "Incomplete",
                    Timestamp = new DateTimeOffset()
                }
            };
            IList<UnitPriceChangeEntity> unitPriceChangeEntities = new List<UnitPriceChangeEntity>();
            List<UnitsOfSalePrices> unitsOfSalePriceList = GetUnitsOfSalePrices();
            PriceChangeEventPayload priceChangeEventPayload = GePriceChangeEventPayload();
            CloudEvent<PriceChangeEventData> priceChangeCloudEventData = GeCloudEventPayload();

            A.CallTo(() => _fakeAzureTableReaderWriter.GetMasterEntities(A<string>.Ignored, A<string>.Ignored)).Returns(priceChangeMasterEntities);
            A.CallTo(() => _fakeAzureBlobEventWriter.DownloadEvent(A<string>.Ignored, A<string>.Ignored)).Returns(PriceChangeInformationJson);
            A.CallTo(() => _fakeAzureTableReaderWriter.GetUnitPriceChangeEventsEntities(A<string>.Ignored, A<string>.Ignored, A<string>.Ignored, A<string>.Ignored)).Returns(unitPriceChangeEntities);
            A.CallTo(() => _fakeErpFacadeService.MapAndBuildUnitsOfSalePrices(A<List<PriceInformation>>.Ignored, A<List<string>>.Ignored)).Returns(unitsOfSalePriceList);
            A.CallTo(() => _fakeErpFacadeService.BuildPriceChangeEventPayload(A<List<UnitsOfSalePrices>>.Ignored, A<string>.Ignored, A<string>.Ignored, A<string>.Ignored)).Returns(priceChangeEventPayload);
            A.CallTo(() => _fakeCloudEventFactory.Create(A<PriceChangeEventPayload>.Ignored)).Returns(priceChangeCloudEventData);
            A.CallTo(() => _fakeJsonHelper.GetPayloadJsonSize(A<string>.Ignored)).Returns(1000000);
            A.CallTo(() => _fakeEventPublisher.Publish(A<CloudEvent<PriceChangeEventData>>.Ignored)).Returns(new Result(Statuses.Failure, "exception"));

            Assert.Throws<ERPFacadeException>(() => _fakeSlicingPublishingService.SliceAndPublishPriceChangeEvents());

            A.CallTo(() => _fakeAzureTableReaderWriter.GetMasterEntities(A<string>.Ignored, A<string>.Ignored)).MustHaveHappened();
            A.CallTo(() => _fakeAzureBlobEventWriter.DownloadEvent(A<string>.Ignored, A<string>.Ignored)).MustHaveHappened();
            A.CallTo(() => _fakeAzureTableReaderWriter.GetUnitPriceChangeEventsEntities(A<string>.Ignored, A<string>.Ignored, A<string>.Ignored, A<string>.Ignored)).MustHaveHappened();
            A.CallTo(() => _fakeAzureTableReaderWriter.AddUnitPriceChangeEntity(A<string>.Ignored, A<string>.Ignored, A<string>.Ignored)).MustHaveHappened();
            A.CallTo(() => _fakeErpFacadeService.MapAndBuildUnitsOfSalePrices(A<List<PriceInformation>>.Ignored, A<List<string>>.Ignored)).MustHaveHappened();
            A.CallTo(() => _fakeErpFacadeService.BuildPriceChangeEventPayload(A<List<UnitsOfSalePrices>>.Ignored, A<string>.Ignored, A<string>.Ignored, A<string>.Ignored)).MustHaveHappened();
            A.CallTo(() => _fakeCloudEventFactory.Create(A<PriceChangeEventPayload>.Ignored)).MustHaveHappened();
            A.CallTo(() => _fakeAzureBlobEventWriter.UploadEvent(A<string>.Ignored, A<string>.Ignored, A<string>.Ignored)).MustHaveHappened();
            A.CallTo(() => _fakeJsonHelper.GetPayloadJsonSize(A<string>.Ignored)).MustHaveHappened();
            A.CallTo(() => _fakeEventPublisher.Publish(A<CloudEvent<PriceChangeEventData>>.Ignored)).MustHaveHappened();
            A.CallTo(() => _fakeAzureTableReaderWriter.UpdateUnitPriceChangeStatusEntity(A<string>.Ignored, A<string>.Ignored, A<string>.Ignored)).MustNotHaveHappened();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
            && call.GetArgument<LogLevel>(0) == LogLevel.Information
            && call.GetArgument<EventId>(1) == EventIds.DownloadBulkPriceInformationEventFromAzureBlob.ToEventId()
            && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Webjob started downloading pricechange information from blob.").MustHaveHappenedOnceExactly();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
            && call.GetArgument<LogLevel>(0) == LogLevel.Information
            && call.GetArgument<EventId>(1) == EventIds.UploadPriceChangeEventPayloadInAzureBlob.ToEventId()
            && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Uploading the pricechange event payload json in blob storage. | _X-Correlation-ID : {_X-Correlation-ID}").MustHaveHappenedOnceExactly();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
            && call.GetArgument<LogLevel>(0) == LogLevel.Information
            && call.GetArgument<EventId>(1) == EventIds.UploadedPriceChangeEventPayloadInAzureBlob.ToEventId()
            && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "pricechange event payload json is uploaded in blob storage successfully. | _X-Correlation-ID : {_X-Correlation-ID}").MustHaveHappenedOnceExactly();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
            && call.GetArgument<LogLevel>(0) == LogLevel.Error
            && call.GetArgument<EventId>(1) == EventIds.ErrorOccuredInEES.ToEventId()
            && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "An error occured for pricechange event while processing your request in EES. | _X-Correlation-ID : {_X-Correlation-ID} | {Status}").MustHaveHappenedOnceExactly();
        }

        [Test]
        public void WhenPriceChangeEventPayloadJsonSizeGreaterThanOneMb_ThenErpFacadeThrowsExceptionPriceChangeEventSizeLimit()
        {
            IList<PriceChangeMasterEntity> priceChangeMasterEntities = new List<PriceChangeMasterEntity>
            {
                new()
                {
                    CorrId = "FakeCorrID",
                    ETag = new(),
                    PartitionKey = "FakeCorrID",
                    RowKey = "FakeCorrID",
                    Status = "Incomplete",
                    Timestamp = new DateTimeOffset()
                }
            };
            IList<UnitPriceChangeEntity> unitPriceChangeEntities = new List<UnitPriceChangeEntity>();
            List<UnitsOfSalePrices> unitsOfSalePriceList = GetUnitsOfSalePrices();
            PriceChangeEventPayload priceChangeEventPayload = GePriceChangeEventPayload();
            CloudEvent<PriceChangeEventData> priceChangeCloudEventData = GeCloudEventPayload();

            A.CallTo(() => _fakeAzureTableReaderWriter.GetMasterEntities(A<string>.Ignored, A<string>.Ignored)).Returns(priceChangeMasterEntities);
            A.CallTo(() => _fakeAzureBlobEventWriter.DownloadEvent(A<string>.Ignored, A<string>.Ignored)).Returns(PriceChangeInformationJson);
            A.CallTo(() => _fakeAzureTableReaderWriter.GetUnitPriceChangeEventsEntities(A<string>.Ignored, A<string>.Ignored, A<string>.Ignored, A<string>.Ignored)).Returns(unitPriceChangeEntities);
            A.CallTo(() => _fakeErpFacadeService.MapAndBuildUnitsOfSalePrices(A<List<PriceInformation>>.Ignored, A<List<string>>.Ignored)).Returns(unitsOfSalePriceList);
            A.CallTo(() => _fakeErpFacadeService.BuildPriceChangeEventPayload(A<List<UnitsOfSalePrices>>.Ignored, A<string>.Ignored, A<string>.Ignored, A<string>.Ignored)).Returns(priceChangeEventPayload);
            A.CallTo(() => _fakeCloudEventFactory.Create(A<PriceChangeEventPayload>.Ignored)).Returns(priceChangeCloudEventData);
            A.CallTo(() => _fakeJsonHelper.GetPayloadJsonSize(A<string>.Ignored)).Returns(2000000);

            Assert.Throws<ERPFacadeException>(() => _fakeSlicingPublishingService.SliceAndPublishPriceChangeEvents());

            A.CallTo(() => _fakeAzureTableReaderWriter.GetMasterEntities(A<string>.Ignored, A<string>.Ignored)).MustHaveHappened();
            A.CallTo(() => _fakeAzureBlobEventWriter.DownloadEvent(A<string>.Ignored, A<string>.Ignored)).MustHaveHappened();
            A.CallTo(() => _fakeAzureTableReaderWriter.GetUnitPriceChangeEventsEntities(A<string>.Ignored, A<string>.Ignored, A<string>.Ignored, A<string>.Ignored)).MustHaveHappened();
            A.CallTo(() => _fakeAzureTableReaderWriter.AddUnitPriceChangeEntity(A<string>.Ignored, A<string>.Ignored, A<string>.Ignored)).MustHaveHappened();
            A.CallTo(() => _fakeErpFacadeService.MapAndBuildUnitsOfSalePrices(A<List<PriceInformation>>.Ignored, A<List<string>>.Ignored)).MustHaveHappened();
            A.CallTo(() => _fakeErpFacadeService.BuildPriceChangeEventPayload(A<List<UnitsOfSalePrices>>.Ignored, A<string>.Ignored, A<string>.Ignored, A<string>.Ignored)).MustHaveHappened();
            A.CallTo(() => _fakeCloudEventFactory.Create(A<PriceChangeEventPayload>.Ignored)).MustHaveHappened();
            A.CallTo(() => _fakeAzureBlobEventWriter.UploadEvent(A<string>.Ignored, A<string>.Ignored, A<string>.Ignored)).MustHaveHappened();
            A.CallTo(() => _fakeJsonHelper.GetPayloadJsonSize(A<string>.Ignored)).MustHaveHappened();
            A.CallTo(() => _fakeEventPublisher.Publish(A<CloudEvent<PriceChangeEventData>>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => _fakeAzureTableReaderWriter.UpdateUnitPriceChangeStatusEntity(A<string>.Ignored, A<string>.Ignored, A<string>.Ignored)).MustNotHaveHappened();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
            && call.GetArgument<LogLevel>(0) == LogLevel.Information
            && call.GetArgument<EventId>(1) == EventIds.DownloadBulkPriceInformationEventFromAzureBlob.ToEventId()
            && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Webjob started downloading pricechange information from blob.").MustHaveHappenedOnceExactly();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
            && call.GetArgument<LogLevel>(0) == LogLevel.Information
            && call.GetArgument<EventId>(1) == EventIds.UploadPriceChangeEventPayloadInAzureBlob.ToEventId()
            && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Uploading the pricechange event payload json in blob storage. | _X-Correlation-ID : {_X-Correlation-ID}").MustHaveHappenedOnceExactly();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
            && call.GetArgument<LogLevel>(0) == LogLevel.Information
            && call.GetArgument<EventId>(1) == EventIds.UploadedPriceChangeEventPayloadInAzureBlob.ToEventId()
            && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "pricechange event payload json is uploaded in blob storage successfully. | _X-Correlation-ID : {_X-Correlation-ID}").MustHaveHappenedOnceExactly();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
            && call.GetArgument<LogLevel>(0) == LogLevel.Error
            && call.GetArgument<EventId>(1) == EventIds.PriceChangeEventSizeLimit.ToEventId()
            && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "pricechange event exceeds the size limit of 1 MB. | _X-Correlation-ID : {_X-Correlation-ID}").MustHaveHappenedOnceExactly();
        }

        private List<UnitsOfSalePrices> GetUnitsOfSalePrices()
        {
            List<PriceInformation> priceInformationList = JsonConvert.DeserializeObject<List<PriceInformation>>(PriceChangeInformationJson)!;
            var unitsOfSalePriceList = _fakeErpFacadeService.MapAndBuildUnitsOfSalePrices(priceInformationList, priceInformationList.Select(u => u.ProductName).Distinct().ToList());

            return unitsOfSalePriceList;
        }

        private PriceChangeEventPayload GePriceChangeEventPayload()
        {
            List<UnitsOfSalePrices> unitsOfSalePriceList = GetUnitsOfSalePrices();
            PriceChangeEventPayload priceChangeEventPayload = _fakeErpFacadeService.BuildPriceChangeEventPayload(unitsOfSalePriceList, Guid.NewGuid().ToString(), "PAYSF", "FakeCorrID");

            return priceChangeEventPayload;
        }

        private CloudEvent<PriceChangeEventData> GeCloudEventPayload()
        {
            PriceChangeEventPayload priceChangeEventPayload = GePriceChangeEventPayload();
            var priceChangeCloudEventData = _fakeCloudEventFactory.Create(priceChangeEventPayload);

            return priceChangeCloudEventData;
        }
    }
}
