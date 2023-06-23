using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using UKHO.ERPFacade.CleanUp.WebJob.Services;
using UKHO.ERPFacade.Common.Configuration;
using UKHO.ERPFacade.Common.IO.Azure;
using UKHO.ERPFacade.Common.Logging;
using UKHO.ERPFacade.Common.Models.TableEntities;

namespace UKHO.ERPFacade.CleanUp.WebJob.UnitTests.Services
{
    [TestFixture]
    public class CleanUpServiceTests
    {
        private ILogger<CleanUpService> _fakeLogger;
        private IAzureTableReaderWriter _fakeAzureTableReaderWriter;
        private IAzureBlobEventWriter _fakeAzureBlobEventWriter;
        private CleanUpService _fakeCleanUpService;
        private IOptions<ErpFacadeWebJobConfiguration> _fakeErpFacadeWebjobConfig;

        private const string CompleteStatus = "Complete";

        [SetUp]
        public void Setup()
        {
            _fakeLogger = A.Fake<ILogger<CleanUpService>>();
            _fakeAzureTableReaderWriter = A.Fake<IAzureTableReaderWriter>();
            _fakeAzureBlobEventWriter = A.Fake<IAzureBlobEventWriter>();
            _fakeErpFacadeWebjobConfig = Options.Create(new ErpFacadeWebJobConfiguration()
            {
                CleanUpDurationInDays = "30"
            });
            _fakeCleanUpService = new CleanUpService(_fakeLogger, _fakeErpFacadeWebjobConfig, _fakeAzureTableReaderWriter,_fakeAzureBlobEventWriter);
        }

        [Test]
        public void Does_Constructor_Throws_ArgumentNullException_When_AzureTableReaderWriter_Parameter_Is_Null()
        {
            Assert.Throws<ArgumentNullException>(
             () => new CleanUpService(_fakeLogger, _fakeErpFacadeWebjobConfig, null, _fakeAzureBlobEventWriter))
             .ParamName
             .Should().Be("azureTableReaderWriter");
        }

        [Test]
        public void Does_Constructor_Throws_ArgumentNullException_When_Logger_Parameter_Is_Null()
        {
            Assert.Throws<ArgumentNullException>(
             () => new CleanUpService(null, _fakeErpFacadeWebjobConfig, _fakeAzureTableReaderWriter, _fakeAzureBlobEventWriter))
             .ParamName
             .Should().Be("logger");
        }

        [Test]
        public void Does_Constructor_Throws_ArgumentNullException_When_ErpFacadeWebjobConfig_Parameter_Is_Null()
        {
            Assert.Throws<ArgumentNullException>(
             () => new CleanUpService(_fakeLogger, null, _fakeAzureTableReaderWriter, _fakeAzureBlobEventWriter))
             .ParamName
             .Should().Be("erpFacadeWebjobConfig");
        }

        [Test]
        public void Does_Constructor_Throws_ArgumentNullException_When_AzureBlobEventWriter_Parameter_Is_Null()
        {
            Assert.Throws<ArgumentNullException>(
             () => new CleanUpService(_fakeLogger, _fakeErpFacadeWebjobConfig, _fakeAzureTableReaderWriter, null))
             .ParamName
             .Should().Be("azureBlobEventWriter");
        }

        [Test]
        public void WhenBlobCreatedIsMoreForThanConfiguredDays_ThenDeleteRelatedTablesAndBlobs()
        {
            List<PriceChangeMasterEntity> priceChangeMasters = new()
            {
                new PriceChangeMasterEntity()
                {
                    CorrId = "123",
                    PartitionKey ="123",
                    RowKey = "123",
                    Status = "Complete"
                }                
            };
            DateTime createDate =  DateTime.UtcNow.AddDays(-31);
            List<string> blobs = new()
            {
                "Blob1","Blob2"
            };

            A.CallTo(() => _fakeAzureTableReaderWriter.GetMasterEntities(A<string>.Ignored, A<string>.Ignored)).Returns(priceChangeMasters);
            A.CallTo(() => _fakeAzureBlobEventWriter.GetBlobCreateDate(A<string>.Ignored, A<string>.Ignored)).Returns(createDate);
            A.CallTo(() => _fakeAzureBlobEventWriter.GetBlobsInContainer(A<string>.Ignored)).Returns(blobs);
            A.CallTo(() => _fakeAzureBlobEventWriter.DeleteBlob(A<string>.Ignored, A<string>.Ignored)).Returns(true);
            A.CallTo(() => _fakeAzureTableReaderWriter.GetAllEntityForEESTable()).Returns(new List<EESEventEntity>());

            _fakeCleanUpService.CleanUpAzureTableAndBlobs();

            A.CallTo(() => _fakeAzureTableReaderWriter.GetMasterEntities(A<string>.Ignored, A<string>.Ignored)).MustHaveHappened();
            A.CallTo(() => _fakeAzureTableReaderWriter.DeleteUnitPriceChangeEntityForMasterCorrId(A<string>.Ignored)).MustHaveHappened();
            A.CallTo(() => _fakeAzureTableReaderWriter.DeletePriceMasterEntity(A<string>.Ignored)).MustHaveHappened();
            A.CallTo(() => _fakeAzureBlobEventWriter.GetBlobCreateDate(A<string>.Ignored, A<string>.Ignored)).MustHaveHappened();
            A.CallTo(() => _fakeAzureBlobEventWriter.GetBlobsInContainer(A<string>.Ignored)).MustHaveHappened();
            A.CallTo(() => _fakeAzureBlobEventWriter.DeleteBlob(A<string>.Ignored, A<string>.Ignored)).MustHaveHappened();



            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
            && call.GetArgument<LogLevel>(0) == LogLevel.Information
            && call.GetArgument<EventId>(1) == EventIds.FetchMasterEntities.ToEventId()
            && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Fetching master entities from azure table").MustHaveHappened();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
           && call.GetArgument<LogLevel>(0) == LogLevel.Information
           && call.GetArgument<EventId>(1) == EventIds.FetchBlobCreateDate.ToEventId()
           && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Fetching create date of blob : {0}").MustHaveHappened();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
           && call.GetArgument<LogLevel>(0) == LogLevel.Information
           && call.GetArgument<EventId>(1) == EventIds.FetchBlobsFromContainer.ToEventId()
           && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Fetching all blobs present in container").MustHaveHappened();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
           && call.GetArgument<LogLevel>(0) == LogLevel.Information
           && call.GetArgument<EventId>(1) == EventIds.DeletedBlobSuccessful.ToEventId()
           && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Deleted blob : {0}  from container").MustHaveHappened();

        }

        [Test]
        public void WhenBlobCreatedIsWithinConfiguredDays_ThenRelatedTablesAndBlobsShouldNotBeDeleted()
        {
            List<PriceChangeMasterEntity> priceChangeMasters = new()
            {
                new PriceChangeMasterEntity()
                {
                    CorrId = "123",
                    PartitionKey ="123",
                    RowKey = "123",
                    Status = "Complete"
                }
            };
            DateTime createDate = DateTime.UtcNow.AddDays(-20);
            List<string> blobs = new()
            {
                "Blob1","Blob2"
            };

            A.CallTo(() => _fakeAzureTableReaderWriter.GetMasterEntities(A<string>.Ignored, A<string>.Ignored)).Returns(priceChangeMasters);
            A.CallTo(() => _fakeAzureBlobEventWriter.GetBlobCreateDate(A<string>.Ignored, A<string>.Ignored)).Returns(createDate);
            A.CallTo(() => _fakeAzureTableReaderWriter.GetAllEntityForEESTable()).Returns(new List<EESEventEntity>());

            _fakeCleanUpService.CleanUpAzureTableAndBlobs();

            A.CallTo(() => _fakeAzureTableReaderWriter.GetMasterEntities(A<string>.Ignored, A<string>.Ignored)).MustHaveHappened();
            A.CallTo(() => _fakeAzureTableReaderWriter.DeleteUnitPriceChangeEntityForMasterCorrId(A<string>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => _fakeAzureTableReaderWriter.DeletePriceMasterEntity(A<string>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => _fakeAzureBlobEventWriter.GetBlobCreateDate(A<string>.Ignored, A<string>.Ignored)).MustHaveHappened();
            A.CallTo(() => _fakeAzureBlobEventWriter.GetBlobsInContainer(A<string>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => _fakeAzureBlobEventWriter.DeleteBlob(A<string>.Ignored, A<string>.Ignored)).MustNotHaveHappened();



            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
            && call.GetArgument<LogLevel>(0) == LogLevel.Information
            && call.GetArgument<EventId>(1) == EventIds.FetchMasterEntities.ToEventId()
            && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Fetching master entities from azure table").MustHaveHappened();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
           && call.GetArgument<LogLevel>(0) == LogLevel.Information
           && call.GetArgument<EventId>(1) == EventIds.FetchBlobCreateDate.ToEventId()
           && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Fetching create date of blob : {0}").MustHaveHappened();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
           && call.GetArgument<LogLevel>(0) == LogLevel.Information
           && call.GetArgument<EventId>(1) == EventIds.FetchBlobsFromContainer.ToEventId()
           && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Fetching all blobs present in container").MustNotHaveHappened();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
           && call.GetArgument<LogLevel>(0) == LogLevel.Information
           && call.GetArgument<EventId>(1) == EventIds.DeletedBlobSuccessful.ToEventId()
           && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Deleted blob : {0}  from container").MustNotHaveHappened();

        }

        [Test]
        public void WhenBlobCreatedIsEqualsConfiguredDays_ThenRelatedTablesAndBlobsShouldNotBeDeleted()
        {
            List<PriceChangeMasterEntity> priceChangeMasters = new()
            {
                new PriceChangeMasterEntity()
                {
                    CorrId = "123",
                    PartitionKey ="123",
                    RowKey = "123",
                    Status = "Complete"
                }
            };
            DateTime createDate = DateTime.UtcNow.AddDays(-30);
            List<string> blobs = new()
            {
                "Blob1","Blob2"
            };

            A.CallTo(() => _fakeAzureTableReaderWriter.GetMasterEntities(A<string>.Ignored, A<string>.Ignored)).Returns(priceChangeMasters);
            A.CallTo(() => _fakeAzureBlobEventWriter.GetBlobCreateDate(A<string>.Ignored, A<string>.Ignored)).Returns(createDate);
            A.CallTo(() => _fakeAzureTableReaderWriter.GetAllEntityForEESTable()).Returns(new List<EESEventEntity>());

            _fakeCleanUpService.CleanUpAzureTableAndBlobs();

            A.CallTo(() => _fakeAzureTableReaderWriter.GetMasterEntities(A<string>.Ignored, A<string>.Ignored)).MustHaveHappened();
            A.CallTo(() => _fakeAzureTableReaderWriter.DeleteUnitPriceChangeEntityForMasterCorrId(A<string>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => _fakeAzureTableReaderWriter.DeletePriceMasterEntity(A<string>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => _fakeAzureBlobEventWriter.GetBlobCreateDate(A<string>.Ignored, A<string>.Ignored)).MustHaveHappened();
            A.CallTo(() => _fakeAzureBlobEventWriter.GetBlobsInContainer(A<string>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => _fakeAzureBlobEventWriter.DeleteBlob(A<string>.Ignored, A<string>.Ignored)).MustNotHaveHappened();



            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
            && call.GetArgument<LogLevel>(0) == LogLevel.Information
            && call.GetArgument<EventId>(1) == EventIds.FetchMasterEntities.ToEventId()
            && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Fetching master entities from azure table").MustHaveHappened();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
           && call.GetArgument<LogLevel>(0) == LogLevel.Information
           && call.GetArgument<EventId>(1) == EventIds.FetchBlobCreateDate.ToEventId()
           && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Fetching create date of blob : {0}").MustHaveHappened();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
           && call.GetArgument<LogLevel>(0) == LogLevel.Information
           && call.GetArgument<EventId>(1) == EventIds.FetchBlobsFromContainer.ToEventId()
           && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Fetching all blobs present in container").MustNotHaveHappened();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
           && call.GetArgument<LogLevel>(0) == LogLevel.Information
           && call.GetArgument<EventId>(1) == EventIds.DeletedBlobSuccessful.ToEventId()
           && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Deleted blob : {0}  from container").MustNotHaveHappened();

        }

        [Test]
        public void WhenEESEventDataHasNoResponseDateTime_ThenShouldNotDeleteRelatedTablesAndBlobs()
        {
            List<PriceChangeMasterEntity> priceChangeMasters = new();
            List<EESEventEntity> eesEventData = new()
            {
                new EESEventEntity()
                {
                    CorrelationId = "corrid",
                    IsNotified = false,
                    RequestDateTime = DateTime.Now.AddDays(-31),
                    PartitionKey= Guid.NewGuid().ToString(),
                    RowKey= Guid.NewGuid().ToString(),
                    ResponseDateTime = null,
                    Timestamp = DateTime.Now
                }
            };


            A.CallTo(() => _fakeAzureTableReaderWriter.GetMasterEntities(A<string>.Ignored, A<string>.Ignored)).Returns(priceChangeMasters);
            A.CallTo(() => _fakeAzureTableReaderWriter.GetAllEntityForEESTable()).Returns(eesEventData);

            _fakeCleanUpService.CleanUpAzureTableAndBlobs();

            A.CallTo(() => _fakeAzureTableReaderWriter.GetMasterEntities(A<string>.Ignored, A<string>.Ignored)).MustHaveHappened();
            A.CallTo(() => _fakeAzureTableReaderWriter.GetAllEntityForEESTable()).MustHaveHappened();
            A.CallTo(() => _fakeAzureTableReaderWriter.DeleteEESEntity(A<string>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => _fakeAzureBlobEventWriter.DeleteContainer(A<string>.Ignored)).MustNotHaveHappened();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
            && call.GetArgument<LogLevel>(0) == LogLevel.Information
            && call.GetArgument<EventId>(1) == EventIds.FetchEESEntities.ToEventId()
            && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Fetching all EES entities from azure table").MustHaveHappened();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
           && call.GetArgument<LogLevel>(0) == LogLevel.Information
           && call.GetArgument<EventId>(1) == EventIds.DeletedContainerSuccessful.ToEventId()
           && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Deleting container : {0}").MustNotHaveHappened();

        }

        [Test]
        public void WhenEESEventDataIsMoreForThanConfiguredDays_ThenDeleteRelatedTablesAndBlobs()
        {
            List<PriceChangeMasterEntity> priceChangeMasters = new();
            List<EESEventEntity> eesEventData = new()
            {
                new EESEventEntity()
                {
                    CorrelationId = "corrid",
                    IsNotified = false,
                    RequestDateTime = DateTime.Now.AddDays(-31),
                    PartitionKey= Guid.NewGuid().ToString(),
                    RowKey= Guid.NewGuid().ToString(),
                    ResponseDateTime = DateTime.Now.AddDays(-29),
                    Timestamp = DateTime.Now
                }
            };


            A.CallTo(() => _fakeAzureTableReaderWriter.GetMasterEntities(A<string>.Ignored, A<string>.Ignored)).Returns(priceChangeMasters);
            A.CallTo(() => _fakeAzureTableReaderWriter.GetAllEntityForEESTable()).Returns(eesEventData);

            _fakeCleanUpService.CleanUpAzureTableAndBlobs();

            A.CallTo(() => _fakeAzureTableReaderWriter.GetMasterEntities(A<string>.Ignored, A<string>.Ignored)).MustHaveHappened();
            A.CallTo(() => _fakeAzureTableReaderWriter.GetAllEntityForEESTable()).MustHaveHappened();
            A.CallTo(() => _fakeAzureTableReaderWriter.DeleteEESEntity(A<string>.Ignored)).MustHaveHappened();
            A.CallTo(() => _fakeAzureBlobEventWriter.DeleteContainer(A<string>.Ignored)).MustHaveHappened();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
            && call.GetArgument<LogLevel>(0) == LogLevel.Information
            && call.GetArgument<EventId>(1) == EventIds.FetchEESEntities.ToEventId()
            && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Fetching all EES entities from azure table").MustHaveHappened();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
           && call.GetArgument<LogLevel>(0) == LogLevel.Information
           && call.GetArgument<EventId>(1) == EventIds.DeletedContainerSuccessful.ToEventId()
           && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Deleting container : {0}").MustHaveHappened();

        }

        [Test]
        public void WhenRequestDateTimeisNull_ThenShouldNotDeleteRelatedTablesAndBlobs()
        {
            List<PriceChangeMasterEntity> priceChangeMasters = new();
            List<EESEventEntity> eesEventData = new()
            {
                new EESEventEntity()
                {
                    CorrelationId = "corrid",
                    IsNotified = false,
                    RequestDateTime = null,
                    PartitionKey= Guid.NewGuid().ToString(),
                    RowKey= Guid.NewGuid().ToString(),
                    ResponseDateTime = DateTime.Now.AddDays(-29),
                    Timestamp = DateTime.Now
                }
            };

            A.CallTo(() => _fakeAzureTableReaderWriter.GetMasterEntities(A<string>.Ignored, A<string>.Ignored)).Returns(priceChangeMasters);
            A.CallTo(() => _fakeAzureTableReaderWriter.GetAllEntityForEESTable()).Returns(eesEventData);

            _fakeCleanUpService.CleanUpAzureTableAndBlobs();

            A.CallTo(() => _fakeAzureTableReaderWriter.GetMasterEntities(A<string>.Ignored, A<string>.Ignored)).MustHaveHappened();
            A.CallTo(() => _fakeAzureTableReaderWriter.GetAllEntityForEESTable()).MustHaveHappened();
            A.CallTo(() => _fakeAzureTableReaderWriter.DeleteEESEntity(A<string>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => _fakeAzureBlobEventWriter.DeleteContainer(A<string>.Ignored)).MustNotHaveHappened();


            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
            && call.GetArgument<LogLevel>(0) == LogLevel.Information
            && call.GetArgument<EventId>(1) == EventIds.FetchEESEntities.ToEventId()
            && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Fetching all EES entities from azure table").MustHaveHappened();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
           && call.GetArgument<LogLevel>(0) == LogLevel.Information
           && call.GetArgument<EventId>(1) == EventIds.DeletedContainerSuccessful.ToEventId()
           && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Deleting container : {0}").MustNotHaveHappened();

        }

        [Test]
        public void WhenEESEventDataIsWithinConfiguredDays_ThenShouldNotDeleteRelatedTablesAndBlobs()
        {
            List<PriceChangeMasterEntity> priceChangeMasters = new();
            List<EESEventEntity> eesEventData = new()
            {
                new EESEventEntity()
                {
                    CorrelationId = "corrid",
                    IsNotified = false,
                    RequestDateTime = DateTime.Now.AddDays(-21),
                    PartitionKey= Guid.NewGuid().ToString(),
                    RowKey= Guid.NewGuid().ToString(),
                    ResponseDateTime = DateTime.Now.AddDays(-29),
                    Timestamp = DateTime.Now
                }
            };


            A.CallTo(() => _fakeAzureTableReaderWriter.GetMasterEntities(A<string>.Ignored, A<string>.Ignored)).Returns(priceChangeMasters);
            A.CallTo(() => _fakeAzureTableReaderWriter.GetAllEntityForEESTable()).Returns(eesEventData);

            _fakeCleanUpService.CleanUpAzureTableAndBlobs();

            A.CallTo(() => _fakeAzureTableReaderWriter.GetMasterEntities(A<string>.Ignored, A<string>.Ignored)).MustHaveHappened();
            A.CallTo(() => _fakeAzureTableReaderWriter.GetAllEntityForEESTable()).MustHaveHappened();
            A.CallTo(() => _fakeAzureTableReaderWriter.DeleteEESEntity(A<string>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => _fakeAzureBlobEventWriter.DeleteContainer(A<string>.Ignored)).MustNotHaveHappened();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
            && call.GetArgument<LogLevel>(0) == LogLevel.Information
            && call.GetArgument<EventId>(1) == EventIds.FetchEESEntities.ToEventId()
            && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Fetching all EES entities from azure table").MustHaveHappened();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
           && call.GetArgument<LogLevel>(0) == LogLevel.Information
           && call.GetArgument<EventId>(1) == EventIds.DeletedContainerSuccessful.ToEventId()
           && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Deleting container : {0}").MustNotHaveHappened();

        }

        [Test]
        public void WhenEESEventDataIsEqualConfiguredDays_ThenShouldNotDeleteRelatedTablesAndBlobs()
        {
            List<PriceChangeMasterEntity> priceChangeMasters = new();
            List<EESEventEntity> eesEventData = new()
            {
                new EESEventEntity()
                {
                    CorrelationId = "corrid",
                    IsNotified = false,
                    RequestDateTime = DateTime.Now.AddDays(-30),
                    PartitionKey= Guid.NewGuid().ToString(),
                    RowKey= Guid.NewGuid().ToString(),
                    ResponseDateTime = DateTime.Now.AddDays(-29),
                    Timestamp = DateTime.Now
                }
            };


            A.CallTo(() => _fakeAzureTableReaderWriter.GetMasterEntities(A<string>.Ignored, A<string>.Ignored)).Returns(priceChangeMasters);
            A.CallTo(() => _fakeAzureTableReaderWriter.GetAllEntityForEESTable()).Returns(eesEventData);

            _fakeCleanUpService.CleanUpAzureTableAndBlobs();

            A.CallTo(() => _fakeAzureTableReaderWriter.GetMasterEntities(A<string>.Ignored, A<string>.Ignored)).MustHaveHappened();
            A.CallTo(() => _fakeAzureTableReaderWriter.GetAllEntityForEESTable()).MustHaveHappened();
            A.CallTo(() => _fakeAzureTableReaderWriter.DeleteEESEntity(A<string>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => _fakeAzureBlobEventWriter.DeleteContainer(A<string>.Ignored)).MustNotHaveHappened();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
            && call.GetArgument<LogLevel>(0) == LogLevel.Information
            && call.GetArgument<EventId>(1) == EventIds.FetchEESEntities.ToEventId()
            && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Fetching all EES entities from azure table").MustHaveHappened();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
           && call.GetArgument<LogLevel>(0) == LogLevel.Information
           && call.GetArgument<EventId>(1) == EventIds.DeletedContainerSuccessful.ToEventId()
           && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Deleting container : {0}").MustNotHaveHappened();

        }
    }
}
