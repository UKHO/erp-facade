﻿using Azure.Data.Tables;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using UKHO.ERPFacade.CleanUp.WebJob.Services;
using UKHO.ERPFacade.Common.Configuration;
using UKHO.ERPFacade.Common.Enums;
using UKHO.ERPFacade.Common.Operations.IO.Azure;
using UKHO.ERPFacade.Common.Logging;

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
        public void Does_Constructor_Throws_ArgumentNullException_When_Logger_Parameter_Is_Null()
        {
            Assert.Throws<ArgumentNullException>(
                    () => new CleanUpService(null, _fakeCleanupWebjobConfig, _fakeAzureTableReaderWriter, _fakeAzureBlobReaderWriter))
                .ParamName
                .Should().Be("logger");
        }

        [Test]
        public void Does_Constructor_Throws_ArgumentNullException_When_AzureTableReaderWriter_Parameter_Is_Null()
        {
            Assert.Throws<ArgumentNullException>(
             () => new CleanUpService(_fakeLogger, _fakeCleanupWebjobConfig, null, _fakeAzureBlobReaderWriter))
             .ParamName
             .Should().Be("azureTableReaderWriter");
        }

        [Test]
        public void Does_Constructor_Throws_ArgumentNullException_When_CleanupWebjobConfig_Parameter_Is_Null()
        {
            Assert.Throws<ArgumentNullException>(
             () => new CleanUpService(_fakeLogger, null, _fakeAzureTableReaderWriter, _fakeAzureBlobReaderWriter))
             .ParamName
             .Should().Be("cleanupWebjobConfig");
        }

        [Test]
        public void Does_Constructor_Throws_ArgumentNullException_When_AzureBlobReaderWriter_Parameter_Is_Null()
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

            A.CallTo(() => _fakeAzureTableReaderWriter.GetFilteredEntitiesAsync(A<Dictionary<string, string>>.Ignored)).Returns(eventData);

            _ = _fakeCleanUpService.Clean();

            A.CallTo(() => _fakeAzureTableReaderWriter.GetFilteredEntitiesAsync(A<Dictionary<string, string>>.Ignored)).MustHaveHappened();
            A.CallTo(() => _fakeAzureTableReaderWriter.DeleteEntityAsync(A<string>.Ignored, A<string>.Ignored)).MustHaveHappened();
            A.CallTo(() => _fakeAzureBlobReaderWriter.DeleteContainerAsync(A<string>.Ignored)).MustHaveHappened();
            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
                                                && call.GetArgument<LogLevel>(0) == LogLevel.Debug
                                                && call.GetArgument<EventId>(1) == EventIds.EventCleanupSuccessful.ToEventId()
                                                && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == $"Event data cleaned up for {eventData[0].RowKey.ToString()} successfully.").MustHaveHappened();
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

            _ = _fakeCleanUpService.Clean();

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

            _ = _fakeCleanUpService.Clean();

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

            _ = _fakeCleanUpService.Clean();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
                                                && call.GetArgument<LogLevel>(0) == LogLevel.Error
                                                && call.GetArgument<EventId>(1) == EventIds.ErrorOccurredInCleanupWebJob.ToEventId()
                                                && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Exception occur during clean up webjob process. ErrorMessage : Object reference not set to an instance of an object.").MustHaveHappenedOnceExactly();
        }
    }
}
