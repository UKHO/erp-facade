using Azure.Data.Tables;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using UKHO.ERPFacade.CleanUp.WebJob.Services;
using UKHO.ERPFacade.Common.Configuration;
using UKHO.ERPFacade.Common.Enums;
using UKHO.ERPFacade.Common.Operations.IO.Azure;

namespace UKHO.ERPFacade.CleanUp.WebJob.UnitTests.Services
{
    [TestFixture]
    public class CleanUpServiceTests
    {
        private IAzureTableReaderWriter _fakeAzureTableReaderWriter;
        private IAzureBlobReaderWriter _fakeAzureBlobReaderWriter;
        private CleanUpService _fakeCleanUpService;
        private IOptions<CleanupWebJobConfiguration> _fakeCleanupWebjobConfig;

        [SetUp]
        public void Setup()
        {
            _fakeAzureTableReaderWriter = A.Fake<IAzureTableReaderWriter>();
            _fakeAzureBlobReaderWriter = A.Fake<IAzureBlobReaderWriter>();
            _fakeCleanupWebjobConfig = Options.Create(new CleanupWebJobConfiguration()
            {
                CleanUpDurationInDays = "30"
            });

            _fakeCleanUpService = new CleanUpService(_fakeCleanupWebjobConfig, _fakeAzureTableReaderWriter, _fakeAzureBlobReaderWriter);
        }

        [Test]
        public void Does_Constructor_Throws_ArgumentNullException_When_AzureTableReaderWriter_Parameter_Is_Null()
        {
            Assert.Throws<ArgumentNullException>(
             () => new CleanUpService(_fakeCleanupWebjobConfig, null, _fakeAzureBlobReaderWriter))
             .ParamName
             .Should().Be("azureTableReaderWriter");
        }

        [Test]
        public void Does_Constructor_Throws_ArgumentNullException_When_CleanupWebjobConfig_Parameter_Is_Null()
        {
            Assert.Throws<ArgumentNullException>(
             () => new CleanUpService(null, _fakeAzureTableReaderWriter, _fakeAzureBlobReaderWriter))
             .ParamName
             .Should().Be("cleanupWebjobConfig");
        }

        [Test]
        public void Does_Constructor_Throws_ArgumentNullException_When_AzureBlobReaderWriter_Parameter_Is_Null()
        {
            Assert.Throws<ArgumentNullException>(
             () => new CleanUpService(_fakeCleanupWebjobConfig, _fakeAzureTableReaderWriter, null))
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
                    { "RequestDateTime", DateTime.Now.AddDays(-31)},
                    { "PartitionKey", Guid.NewGuid().ToString() },
                    { "RowKey", Guid.NewGuid().ToString() },
                    { "Timestamp", DateTime.Now.AddDays(-31)},
                    { "Status", Status.Complete.ToString()}

               }
            };

            A.CallTo(() => _fakeAzureTableReaderWriter.GetAllEntities()).Returns(eventData);

            _fakeCleanUpService.Clean();

            A.CallTo(() => _fakeAzureTableReaderWriter.GetAllEntities()).MustHaveHappened();
            A.CallTo(() => _fakeAzureTableReaderWriter.DeleteEntityAsync(A<string>.Ignored, A<string>.Ignored)).MustHaveHappened();
            A.CallTo(() => _fakeAzureBlobReaderWriter.DeleteContainer(A<string>.Ignored)).MustHaveHappened();
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
                    { "Timestamp", DateTime.Now.AddDays(-21) },
                    { "Status", Status.Complete.ToString()}
               }
            };

            A.CallTo(() => _fakeAzureTableReaderWriter.GetAllEntities()).Returns(eesEventData);

            _fakeCleanUpService.Clean();

            A.CallTo(() => _fakeAzureTableReaderWriter.GetAllEntities()).MustHaveHappened();
            A.CallTo(() => _fakeAzureTableReaderWriter.DeleteEntityAsync(A<string>.Ignored, A<string>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => _fakeAzureBlobReaderWriter.DeleteContainer(A<string>.Ignored)).MustNotHaveHappened();
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
                    { "Timestamp", DateTime.Now.AddDays(-30) },
                    { "Status", Status.Complete.ToString()}
               }
            };

            A.CallTo(() => _fakeAzureTableReaderWriter.GetAllEntities()).Returns(eesEventData);

            _fakeCleanUpService.Clean();

            A.CallTo(() => _fakeAzureTableReaderWriter.GetAllEntities()).MustHaveHappened();
            A.CallTo(() => _fakeAzureTableReaderWriter.DeleteEntityAsync(A<string>.Ignored, A<string>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => _fakeAzureBlobReaderWriter.DeleteDirectory(A<string>.Ignored, A<string>.Ignored)).MustNotHaveHappened();
        }
    }
}
