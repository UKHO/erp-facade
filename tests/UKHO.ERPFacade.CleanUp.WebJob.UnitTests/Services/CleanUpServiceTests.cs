using Azure.Data.Tables;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using UKHO.ERPFacade.CleanUp.WebJob.Services;
using UKHO.ERPFacade.Common.Configuration;
using UKHO.ERPFacade.Common.Constants;
using UKHO.ERPFacade.Common.Enums;
using UKHO.ERPFacade.Common.Logging;
using UKHO.ERPFacade.Common.Operations.IO.Azure;

namespace UKHO.ERPFacade.CleanUp.WebJob.UnitTests.Services
{
    [TestFixture]
    public class CleanUpServiceTests
    {
        private ILogger<CleanUpService> _fakeLogger;
        private IAzureTableReaderWriter _fakeAzureTableReaderWriter;
        private IAzureBlobReaderWriter _fakeAzureBlobReaderWriter;
        private CleanUpService _fakeCleanUpService;
        private IOptions<CleanupWebJobConfiguration> _fakeCleanupWebjobConfig;

        [SetUp]
        public void Setup()
        {
            _fakeLogger = A.Fake<ILogger<CleanUpService>>();
            _fakeAzureTableReaderWriter = A.Fake<IAzureTableReaderWriter>();
            _fakeAzureBlobReaderWriter = A.Fake<IAzureBlobReaderWriter>();
            _fakeCleanupWebjobConfig = Options.Create(new CleanupWebJobConfiguration()
            {
                CleanUpDurationInDays = "30"
            });

            _fakeCleanUpService = new CleanUpService(_fakeLogger, _fakeCleanupWebjobConfig, _fakeAzureTableReaderWriter, _fakeAzureBlobReaderWriter);
        }

        [Test]
        public void WhenLoggerParameterIsNull_ThenConstructorThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(
                    () => new CleanUpService(null, _fakeCleanupWebjobConfig, _fakeAzureTableReaderWriter, _fakeAzureBlobReaderWriter))
                .ParamName
                .Should().Be("logger");
        }

        [Test]
        public void WhenAzureTableReaderWriterParameterIsNull_ThenConstructorThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(
             () => new CleanUpService(_fakeLogger, _fakeCleanupWebjobConfig, null, _fakeAzureBlobReaderWriter))
             .ParamName
             .Should().Be("azureTableReaderWriter");
        }

        [Test]
        public void WhenCleanupWebjobConfigParameterIsNull_ThenConstructorThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(
             () => new CleanUpService(_fakeLogger, null, _fakeAzureTableReaderWriter, _fakeAzureBlobReaderWriter))
             .ParamName
             .Should().Be("cleanupWebjobConfig");
        }

        [Test]
        public void WhenAzureBlobReaderWriterParameterIsNull_ThenConstructorThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(
             () => new CleanUpService(_fakeLogger, _fakeCleanupWebjobConfig, _fakeAzureTableReaderWriter, null))
             .ParamName
             .Should().Be("azureBlobReaderWriter");
        }

        [Test]
        public void WhenEventDataIsOlderThanConfiguredDays_ThenWebjobCleanupEventData()
        {
            List<TableEntity> eventData = new()
            {
               new TableEntity()
               {
                    { "CorrelationId", "corrid" },
                    { "RequestDateTime", DateTime.UtcNow.AddDays(-31)},
                    { "PartitionKey", Guid.NewGuid().ToString() },
                    { "RowKey", Guid.NewGuid().ToString() },
                    { "Timestamp", DateTime.UtcNow.AddDays(-31)},
                    { "Status", Status.Complete.ToString()}

               }
            };

            var statusFilter = new Dictionary<string, string> { { AzureStorage.EventStatus, Status.Complete.ToString() } };

            A.CallTo(() => _fakeAzureTableReaderWriter.GetFilteredEntitiesAsync(statusFilter)).Returns(eventData);

            _ = _fakeCleanUpService.CleanAsync();

            A.CallTo(() => _fakeAzureTableReaderWriter.GetFilteredEntitiesAsync(A<Dictionary<string, string>>.Ignored)).MustHaveHappened();
            A.CallTo(() => _fakeAzureTableReaderWriter.DeleteEntityAsync(A<string>.Ignored, A<string>.Ignored)).MustHaveHappened();
            A.CallTo(() => _fakeAzureBlobReaderWriter.DeleteContainerAsync(A<string>.Ignored)).MustHaveHappened();
            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
                                                && call.GetArgument<LogLevel>(0) == LogLevel.Debug
                                                && call.GetArgument<EventId>(1) == EventIds.EventCleanupSuccessful.ToEventId()
                                                && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Data clean up completed for {CorrelationId} successfully.").MustHaveHappened();
        }

        [Test]
        public void WhenEventDataIsNotOlderThanConfiguredDays_ThenWebjobDoesNotCleanupEventData()
        {
            List<TableEntity> eventData = new()
            {
               new TableEntity()
               {
                    { "CorrelationId", "corrid" },
                    { "RequestDateTime", DateTime.UtcNow.AddDays(-21) },
                    { "PartitionKey", Guid.NewGuid().ToString() },
                    { "RowKey", Guid.NewGuid().ToString() },
                    { "Timestamp", DateTime.UtcNow.AddDays(-21) },
                    { "Status", Status.Complete.ToString()}
               }
            };

            A.CallTo(() => _fakeAzureTableReaderWriter.GetFilteredEntitiesAsync(A<Dictionary<string, string>>.Ignored)).Returns(eventData);

            _ = _fakeCleanUpService.CleanAsync();

            A.CallTo(() => _fakeAzureTableReaderWriter.GetFilteredEntitiesAsync(A<Dictionary<string, string>>.Ignored)).MustHaveHappened();
            A.CallTo(() => _fakeAzureTableReaderWriter.DeleteEntityAsync(A<string>.Ignored, A<string>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => _fakeAzureBlobReaderWriter.DeleteContainerAsync(A<string>.Ignored)).MustNotHaveHappened();
        }

        [Test]
        public void WhenEventDataIsExactlyConfiguredDayOlder_ThenWebjobDoesNotCleanupEventData()
        {
            List<TableEntity> eventData = new()
            {
               new TableEntity()
               {
                    { "CorrelationId", "corrid" },
                    { "RequestDateTime", DateTime.UtcNow.AddDays(-30) },
                    { "PartitionKey", Guid.NewGuid().ToString() },
                    { "RowKey", Guid.NewGuid().ToString() },
                    { "Timestamp", DateTime.UtcNow.AddDays(-30) },
                    { "Status", Status.Complete.ToString()}
               }
            };

            A.CallTo(() => _fakeAzureTableReaderWriter.GetFilteredEntitiesAsync(A<Dictionary<string, string>>.Ignored)).Returns(eventData);

            _ = _fakeCleanUpService.CleanAsync();

            A.CallTo(() => _fakeAzureTableReaderWriter.GetFilteredEntitiesAsync(A<Dictionary<string, string>>.Ignored)).MustHaveHappened();
            A.CallTo(() => _fakeAzureTableReaderWriter.DeleteEntityAsync(A<string>.Ignored, A<string>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => _fakeAzureBlobReaderWriter.DeleteContainerAsync(A<string>.Ignored)).MustNotHaveHappened();
        }

        [Test]
        public void WhenCleanupServiceOccurAnyError_ThenLogtheException()
        {
            List<TableEntity> eventData = new()
            {
                new TableEntity()
                {
                    { "CorrelationId", "corrid" },
                    { "RequestDateTime", DateTime.UtcNow.AddDays(-31) },
                    { "PartitionKey", Guid.NewGuid().ToString() },
                    { "RowKey", null },
                    { "Timestamp", DateTime.UtcNow.AddDays(-31) },
                    { "Status", Status.Complete.ToString()}
                }
            };

            A.CallTo(() => _fakeAzureTableReaderWriter.GetFilteredEntitiesAsync(A<Dictionary<string, string>>.Ignored)).Returns(eventData);

            _ = _fakeCleanUpService.CleanAsync();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
                                                && call.GetArgument<LogLevel>(0) == LogLevel.Error
                                                && call.GetArgument<EventId>(1) == EventIds.ErrorOccurredInCleanupWebJob.ToEventId()
                                                && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "An error occured during clean up webjob process. ErrorMessage : {Exception}").MustHaveHappenedOnceExactly();
        }
    }
}
