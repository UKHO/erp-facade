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
        private IOptions<CleanupWebJobConfiguration> _fakeCleanupWebjobConfig;

        [SetUp]
        public void Setup()
        {
            _fakeLogger = A.Fake<ILogger<CleanUpService>>();
            _fakeAzureTableHelper = A.Fake<IAzureTableHelper>();
            _fakeAzureBlobHelper = A.Fake<IAzureBlobHelper>();
            _fakeCleanupWebjobConfig = Options.Create(new CleanupWebJobConfiguration()
            {
                CleanUpDurationInDays = "30"
            });

            _fakeCleanUpService = new CleanUpService(_fakeLogger, _fakeCleanupWebjobConfig, _fakeAzureTableHelper, _fakeAzureBlobHelper);
        }

        [Test]
        public void Does_Constructor_Throws_ArgumentNullException_When_AzureTableHelper_Parameter_Is_Null()
        {
            Assert.Throws<ArgumentNullException>(
             () => new CleanUpService(_fakeLogger, _fakeCleanupWebjobConfig, null, _fakeAzureBlobHelper))
             .ParamName
             .Should().Be("azureTableHelper");
        }

        [Test]
        public void Does_Constructor_Throws_ArgumentNullException_When_Logger_Parameter_Is_Null()
        {
            Assert.Throws<ArgumentNullException>(
             () => new CleanUpService(null, _fakeCleanupWebjobConfig, _fakeAzureTableHelper, _fakeAzureBlobHelper))
             .ParamName
             .Should().Be("logger");
        }

        [Test]
        public void Does_Constructor_Throws_ArgumentNullException_When_CleanupWebjobConfig_Parameter_Is_Null()
        {
            Assert.Throws<ArgumentNullException>(
             () => new CleanUpService(_fakeLogger, null, _fakeAzureTableHelper, _fakeAzureBlobHelper))
             .ParamName
             .Should().Be("cleanupWebjobConfig");
        }

        [Test]
        public void Does_Constructor_Throws_ArgumentNullException_When_AzureBlobHelper_Parameter_Is_Null()
        {
            Assert.Throws<ArgumentNullException>(
             () => new CleanUpService(_fakeLogger, _fakeCleanupWebjobConfig, _fakeAzureTableHelper, null))
             .ParamName
             .Should().Be("azureBlobHelper");
        }

        [Test]
        public void WhenEventDataIsOlderThanConfiguredDays_ThenWebjobCleanupEventData()
        {
            List<TableEntity> eventData = new()
            {
               new TableEntity()
               {
                    { "CorrelationId", "corrid" },
                    { "RequestDateTime", DateTime.Now.AddDays(-31) },
                    { "PartitionKey", Guid.NewGuid().ToString() },
                    { "RowKey", Guid.NewGuid().ToString() },
                    { "Timestamp", DateTime.Now }
               }
            };

            A.CallTo(() => _fakeAzureTableHelper.GetAllEntities(A<string>.Ignored)).Returns(eventData);

            _fakeCleanUpService.Clean();

            A.CallTo(() => _fakeAzureTableHelper.GetAllEntities(A<string>.Ignored)).MustHaveHappened();
            A.CallTo(() => _fakeAzureTableHelper.DeleteEntity(A<string>.Ignored)).MustHaveHappened();
            A.CallTo(() => _fakeAzureBlobHelper.DeleteContainer(A<string>.Ignored)).MustHaveHappened();
        }

        [Test]
        public void WhenEventRequestDateTimeisNull_ThenWebhojobDoesNotCleanupEventData()
        {
            List<TableEntity> eesEventData = new()
            {
               new TableEntity()
               {
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
            A.CallTo(() => _fakeAzureTableHelper.DeleteEntity(A<string>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => _fakeAzureBlobHelper.DeleteDirectory(A<string>.Ignored, A<string>.Ignored)).MustNotHaveHappened();
        }

        [Test]
        public void WhenEventDataIsNotOlderThanConfiguredDays_ThenWebjobDoesNotCleanupEventData()
        {
            List<TableEntity> eesEventData = new()
            {
               new TableEntity()
               {
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
            A.CallTo(() => _fakeAzureTableHelper.DeleteEntity(A<string>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => _fakeAzureBlobHelper.DeleteContainer(A<string>.Ignored)).MustNotHaveHappened();
        }

        [Test]
        public void WhenEventDataIsExactlyConfiguredDayOlder_ThenWebjobDoesNotCleanupEventData()
        {
            List<TableEntity> eesEventData = new()
            {
               new TableEntity()
               {
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
            A.CallTo(() => _fakeAzureTableHelper.DeleteEntity(A<string>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => _fakeAzureBlobHelper.DeleteDirectory(A<string>.Ignored, A<string>.Ignored)).MustNotHaveHappened();
        }
    }
}
