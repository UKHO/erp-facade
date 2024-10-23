using Azure.Data.Tables;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using UKHO.ERPFacade.CleanUp.WebJob.Services;
using UKHO.ERPFacade.Common.Configuration;
using UKHO.ERPFacade.Common.IO.Azure;
using UKHO.ERPFacade.Common.Logging;

namespace UKHO.ERPFacade.CleanUp.WebJob.UnitTests.Services
{
    [TestFixture]
    public class CleanUpServiceTests
    {
        private ILogger<CleanUpService> _fakeLogger;
        private IAzureTableHelper _fakeAzureTableHelper;
        private IAzureBlobHelper _fakeAzureBlobHelper;
        private CleanUpService _fakeCleanUpService;
        private IOptions<ErpFacadeWebJobConfiguration> _fakeErpFacadeWebjobConfig;

        [SetUp]
        public void Setup()
        {
            _fakeLogger = A.Fake<ILogger<CleanUpService>>();
            _fakeAzureTableHelper = A.Fake<IAzureTableHelper>();
            _fakeAzureBlobHelper = A.Fake<IAzureBlobHelper>();
            _fakeErpFacadeWebjobConfig = Options.Create(new ErpFacadeWebJobConfiguration()
            {
                CleanUpDurationInDays = "30"
            });
            _fakeCleanUpService = new CleanUpService(_fakeLogger, _fakeErpFacadeWebjobConfig, _fakeAzureTableHelper, _fakeAzureBlobHelper);
        }

        [Test]
        public void Does_Constructor_Throws_ArgumentNullException_When_AzureTableReaderWriter_Parameter_Is_Null()
        {
            Assert.Throws<ArgumentNullException>(
             () => new CleanUpService(_fakeLogger, _fakeErpFacadeWebjobConfig, null, _fakeAzureBlobHelper))
             .ParamName
             .Should().Be("azureTableReaderWriter");
        }

        [Test]
        public void Does_Constructor_Throws_ArgumentNullException_When_Logger_Parameter_Is_Null()
        {
            Assert.Throws<ArgumentNullException>(
             () => new CleanUpService(null, _fakeErpFacadeWebjobConfig, _fakeAzureTableHelper, _fakeAzureBlobHelper))
             .ParamName
             .Should().Be("logger");
        }

        [Test]
        public void Does_Constructor_Throws_ArgumentNullException_When_ErpFacadeWebjobConfig_Parameter_Is_Null()
        {
            Assert.Throws<ArgumentNullException>(
             () => new CleanUpService(_fakeLogger, null, _fakeAzureTableHelper, _fakeAzureBlobHelper))
             .ParamName
             .Should().Be("erpFacadeWebjobConfig");
        }

        [Test]
        public void Does_Constructor_Throws_ArgumentNullException_When_AzureBlobEventWriter_Parameter_Is_Null()
        {
            Assert.Throws<ArgumentNullException>(
             () => new CleanUpService(_fakeLogger, _fakeErpFacadeWebjobConfig, _fakeAzureTableHelper, null))
             .ParamName
             .Should().Be("azureBlobEventWriter");
        }

        [Test]
        public void WhenEESEventDataIsMoreForThanConfiguredDays_ThenDeleteRelatedTablesAndBlobs()
        {
            List<TableEntity> eesEventData = new()
            {
               new TableEntity() {
                { "CorrelationId", "corrid" },
                { "RequestDateTime", DateTime.Now.AddDays(-31) },
                { "PartitionKey", Guid.NewGuid().ToString() },
                { "RowKey", Guid.NewGuid().ToString() },
                { "Timestamp", DateTime.Now }
            }
            };

            A.CallTo(() => _fakeAzureTableHelper.GetAllEntities(A<string>.Ignored)).Returns(eesEventData);

            _fakeCleanUpService.Clean();

            A.CallTo(() => _fakeAzureTableHelper.GetAllEntities(A<string>.Ignored)).MustHaveHappened();
            A.CallTo(() => _fakeAzureTableHelper.DeleteEntity(A<string>.Ignored, A<string>.Ignored)).MustHaveHappened();
            A.CallTo(() => _fakeAzureBlobHelper.DeleteContainer(A<string>.Ignored)).MustHaveHappened();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
            && call.GetArgument<LogLevel>(0) == LogLevel.Information
            && call.GetArgument<EventId>(1) == EventIds.FetchEESEntities.ToEventId()
            && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Fetching all records from azure table {TableName}").MustHaveHappened();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
           && call.GetArgument<LogLevel>(0) == LogLevel.Information
           && call.GetArgument<EventId>(1) == EventIds.DeletedContainerSuccessful.ToEventId()
           && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Event data cleaned up for {CorrelationId} successfully.").MustHaveHappened();

        }

        [Test]
        public void WhenRequestDateTimeisNull_ThenShouldNotDeleteRelatedTablesAndBlobs()
        {
            List<TableEntity> eesEventData = new()
            {
               new TableEntity() {
                { "CorrelationId", "corrid" },
                { "RequestDateTime", null },
                { "PartitionKey", Guid.NewGuid().ToString() },
                { "RowKey", Guid.NewGuid().ToString() },
                { "Timestamp", DateTime.Now }
            }
            };

            A.CallTo(() => _fakeAzureTableHelper.GetAllEntities(A<string>.Ignored)).Returns(eesEventData);

            _fakeCleanUpService.Clean();

            A.CallTo(() => _fakeAzureTableHelper.GetAllEntities(A<string>.Ignored)).MustHaveHappened();
            A.CallTo(() => _fakeAzureTableHelper.DeleteEntity(A<string>.Ignored, A<string>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => _fakeAzureBlobHelper.DeleteDirectory(A<string>.Ignored, A<string>.Ignored)).MustNotHaveHappened();


            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
            && call.GetArgument<LogLevel>(0) == LogLevel.Information
            && call.GetArgument<EventId>(1) == EventIds.FetchEESEntities.ToEventId()
            && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Fetching all records from azure table {TableName}").MustHaveHappened();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
           && call.GetArgument<LogLevel>(0) == LogLevel.Information
           && call.GetArgument<EventId>(1) == EventIds.DeletedContainerSuccessful.ToEventId()
           && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Deleting directory {CorrelationId} from {EventContainerName} container").MustNotHaveHappened();

        }

        [Test]
        public void WhenEESEventDataIsWithinConfiguredDays_ThenShouldNotDeleteRelatedTablesAndBlobs()
        {
            List<TableEntity> eesEventData = new()
            {
               new TableEntity() {
                { "CorrelationId", "corrid" },
                { "RequestDateTime", DateTime.Now.AddDays(-21) },
                { "PartitionKey", Guid.NewGuid().ToString() },
                { "RowKey", Guid.NewGuid().ToString() },
                { "Timestamp", DateTime.Now }
            }
            };

            A.CallTo(() => _fakeAzureTableHelper.GetAllEntities(A<string>.Ignored)).Returns(eesEventData);

            _fakeCleanUpService.Clean();

            A.CallTo(() => _fakeAzureTableHelper.GetAllEntities(A<string>.Ignored)).MustHaveHappened();
            A.CallTo(() => _fakeAzureTableHelper.DeleteEntity(A<string>.Ignored, A<string>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => _fakeAzureBlobHelper.DeleteContainer(A<string>.Ignored)).MustNotHaveHappened();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
            && call.GetArgument<LogLevel>(0) == LogLevel.Information
            && call.GetArgument<EventId>(1) == EventIds.FetchEESEntities.ToEventId()
            && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Fetching all records from azure table {TableName}").MustHaveHappened();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
           && call.GetArgument<LogLevel>(0) == LogLevel.Information
           && call.GetArgument<EventId>(1) == EventIds.DeletedContainerSuccessful.ToEventId()
           && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Deleting directory {CorrelationId} from {EventContainerName} container").MustNotHaveHappened();

        }

        [Test]
        public void WhenEESEventDataIsEqualConfiguredDays_ThenShouldNotDeleteRelatedTablesAndBlobs()
        {
            List<TableEntity> eesEventData = new()
            {
               new TableEntity() {
                { "CorrelationId", "corrid" },
                { "RequestDateTime", DateTime.Now.AddDays(-30) },
                { "PartitionKey", Guid.NewGuid().ToString() },
                { "RowKey", Guid.NewGuid().ToString() },
                { "Timestamp", DateTime.Now }
            }
            };

            A.CallTo(() => _fakeAzureTableHelper.GetAllEntities(A<string>.Ignored)).Returns(eesEventData);

            _fakeCleanUpService.Clean();

            A.CallTo(() => _fakeAzureTableHelper.GetAllEntities(A<string>.Ignored)).MustHaveHappened();
            A.CallTo(() => _fakeAzureTableHelper.DeleteEntity(A<string>.Ignored, A<string>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => _fakeAzureBlobHelper.DeleteDirectory(A<string>.Ignored, A<string>.Ignored)).MustNotHaveHappened();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
            && call.GetArgument<LogLevel>(0) == LogLevel.Information
            && call.GetArgument<EventId>(1) == EventIds.FetchEESEntities.ToEventId()
            && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Fetching all records from azure table {TableName}").MustHaveHappened();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
           && call.GetArgument<LogLevel>(0) == LogLevel.Information
           && call.GetArgument<EventId>(1) == EventIds.DeletedContainerSuccessful.ToEventId()
           && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Deleting directory {CorrelationId} from {EventContainerName} container").MustNotHaveHappened();
        }
    }
}
