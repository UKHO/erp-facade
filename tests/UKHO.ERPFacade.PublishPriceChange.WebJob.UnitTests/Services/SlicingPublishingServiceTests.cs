﻿using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using UKHO.ERPFacade.Common.Infrastructure.EventService;
using UKHO.ERPFacade.Common.Infrastructure.EventService.EventProvider;
using UKHO.ERPFacade.Common.IO;
using UKHO.ERPFacade.Common.IO.Azure;
using UKHO.ERPFacade.Common.Logging;
using UKHO.ERPFacade.Common.Models.TableEntities;
using UKHO.ERPFacade.Common.Services;
using UKHO.ERPFacade.PublishPriceChange.WebJob.Services;

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

        private readonly string downloadPayload = "[\r\n  {\r\n    \"corrid\": \"\",\r\n    \"org\": \"UKHO\",\r\n    \"productname\": \"PAYSF\",\r\n    \"duration\": \"12\",\r\n    \"effectivedate\": \"20230427\",\r\n    \"effectivetime\": \"101454\",\r\n    \"price\": \"156.00 \",\r\n    \"currency\": \"USD\",\r\n    \"futuredate\": \"20230527\",\r\n    \"futuretime\": \"000001\",\r\n    \"futureprice\": \"180.00\",\r\n    \"futurecurr\": \"USD\",\r\n    \"reqdate\": \"20230328\",\r\n    \"reqtime\": \"160000\"\r\n  }\r\n]";

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

            A.CallTo(() => _fakeAzureTableReaderWriter.GetMasterEntities(A<string>.Ignored, A<string>.Ignored)).Returns(priceChangeMasterEntities);
            A.CallTo(() => _fakeAzureBlobEventWriter.DownloadEvent(A<string>.Ignored, A<string>.Ignored)).Returns(downloadPayload);
            A.CallTo(() => _fakeAzureTableReaderWriter.GetUnitPriceChangeEventsEntities(A<string>.Ignored, A<string>.Ignored, A<string>.Ignored, A<string>.Ignored)).Returns(unitPriceChangeEntities);

            _fakeSlicingPublishingService.SliceAndPublishPriceChangeEvents();

            A.CallTo(() => _fakeAzureBlobEventWriter.DownloadEvent(A<string>.Ignored, A<string>.Ignored)).MustHaveHappened();
            A.CallTo(() => _fakeAzureTableReaderWriter.GetUnitPriceChangeEventsEntities(A<string>.Ignored, A<string>.Ignored, A<string>.Ignored, A<string>.Ignored)).MustHaveHappened();
            A.CallTo(() => _fakeAzureBlobEventWriter.UploadEvent(A<string>.Ignored, A<string>.Ignored, A<string>.Ignored)).MustHaveHappened();
            A.CallTo(() => _fakeAzureTableReaderWriter.UpdateUnitPriceChangeStatusEntity(A<string>.Ignored, A<string>.Ignored, A<string>.Ignored)).MustHaveHappened();
            A.CallTo(() => _fakeAzureTableReaderWriter.AddUnitPriceChangeEntity(A<string>.Ignored, A<string>.Ignored, A<string>.Ignored)).MustHaveHappened();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
            && call.GetArgument<LogLevel>(0) == LogLevel.Information
            && call.GetArgument<EventId>(1) == EventIds.AppendingUnitofSalePricesToEncEventInWebJob.ToEventId()
            && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Appending UnitofSale prices to ENC event in webjob.").MustHaveHappened();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
            && call.GetArgument<LogLevel>(0) == LogLevel.Information
            && call.GetArgument<EventId>(1) == EventIds.DownloadBulkPriceInformationEventFromAzureBlob.ToEventId()
            && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Downloading Price Change information from blob").MustHaveHappened();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
            && call.GetArgument<LogLevel>(0) == LogLevel.Information
            && call.GetArgument<EventId>(1) == EventIds.UploadedSlicedEventInAzureBlob.ToEventId()
            && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Sliced event is uploaded in blob storage successfully.").MustHaveHappened();

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

            A.CallTo(() => _fakeAzureTableReaderWriter.GetMasterEntities(A<string>.Ignored, A<string>.Ignored)).Returns(priceChangeMasterEntities);
            A.CallTo(() => _fakeAzureBlobEventWriter.DownloadEvent(A<string>.Ignored, A<string>.Ignored)).Returns(downloadPayload);
            A.CallTo(() => _fakeAzureTableReaderWriter.GetUnitPriceChangeEventsEntities(A<string>.Ignored, A<string>.Ignored, A<string>.Ignored, A<string>.Ignored)).Returns(unitPriceChangeEntities);

            _fakeSlicingPublishingService.SliceAndPublishPriceChangeEvents();

            A.CallTo(() => _fakeAzureBlobEventWriter.DownloadEvent(A<string>.Ignored, A<string>.Ignored)).MustHaveHappened();
            A.CallTo(() => _fakeAzureTableReaderWriter.GetUnitPriceChangeEventsEntities(A<string>.Ignored, A<string>.Ignored, A<string>.Ignored, A<string>.Ignored)).MustHaveHappened();
            A.CallTo(() => _fakeAzureBlobEventWriter.UploadEvent(A<string>.Ignored, A<string>.Ignored, A<string>.Ignored)).MustHaveHappened();
            A.CallTo(() => _fakeAzureTableReaderWriter.UpdateUnitPriceChangeStatusEntity(A<string>.Ignored, A<string>.Ignored, A<string>.Ignored)).MustHaveHappened();
            A.CallTo(() => _fakeAzureTableReaderWriter.UpdatePriceMasterStatusEntity(A<string>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => _fakeAzureTableReaderWriter.AddUnitPriceChangeEntity(A<string>.Ignored, A<string>.Ignored, A<string>.Ignored)).MustNotHaveHappened();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
            && call.GetArgument<LogLevel>(0) == LogLevel.Information
            && call.GetArgument<EventId>(1) == EventIds.AppendingUnitofSalePricesToEncEventInWebJob.ToEventId()
            && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Appending UnitofSale prices to ENC event in webjob.").MustHaveHappenedOnceExactly();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
            && call.GetArgument<LogLevel>(0) == LogLevel.Information
            && call.GetArgument<EventId>(1) == EventIds.DownloadBulkPriceInformationEventFromAzureBlob.ToEventId()
            && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Downloading Price Change information from blob").MustHaveHappened();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
            && call.GetArgument<LogLevel>(0) == LogLevel.Information
            && call.GetArgument<EventId>(1) == EventIds.UploadedSlicedEventInAzureBlobForUnitPrices.ToEventId()
            && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Sliced event is uploaded in blob storage successfully for incomplete unit prices.").MustHaveHappened();

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
            A.CallTo(() => _fakeAzureBlobEventWriter.DownloadEvent(A<string>.Ignored, A<string>.Ignored)).Returns(downloadPayload);
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
    }
}
