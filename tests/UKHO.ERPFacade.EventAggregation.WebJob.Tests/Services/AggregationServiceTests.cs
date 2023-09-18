using Azure.Storage.Queues.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using UKHO.ERPFacade.Common.Configuration;
using UKHO.ERPFacade.Common.HttpClients;
using UKHO.ERPFacade.Common.IO.Azure;
using UKHO.ERPFacade.EventAggregation.WebJob.Helpers;
using UKHO.ERPFacade.EventAggregation.WebJob.Services;
using FakeItEasy;
using FluentAssertions;
using UKHO.ERPFacade.Common.Logging;

namespace UKHO.ERPFacade.EventAggregation.WebJob.Tests.Services
{
    [TestFixture]
    public class AggregationServiceTests
    {
        private ILogger<AggregationService> _fakeLogger;
        private IAzureTableReaderWriter _fakeAzureTableReaderWriter;
        private IAzureBlobEventWriter _fakeAzureBlobEventWriter;
        private ISapClient _fakeSapClient;
        private IOptions<SapConfiguration> _fakeSapConfig;
        private IRecordOfSaleSapMessageBuilder _fakeRecordOfSaleSapMessageBuilder;
        private AggregationService _fakeAggregationService;

        [SetUp]
        public void Setup()
        {
            _fakeLogger = A.Fake<ILogger<AggregationService>>();
            _fakeAzureTableReaderWriter = A.Fake<IAzureTableReaderWriter>();
            _fakeAzureBlobEventWriter = A.Fake<IAzureBlobEventWriter>();
            _fakeSapClient = A.Fake<ISapClient>();
            _fakeSapConfig = Options.Create(new SapConfiguration()
            {
                SapServiceOperationForRecordOfSale = "Z_ADDS_ROS"
            });
            _fakeRecordOfSaleSapMessageBuilder = A.Fake<IRecordOfSaleSapMessageBuilder>();

            _fakeAggregationService = new AggregationService(_fakeLogger, _fakeAzureTableReaderWriter, _fakeAzureBlobEventWriter, _fakeSapClient, _fakeSapConfig, _fakeRecordOfSaleSapMessageBuilder);
        }

        [Test]
        public void Does_Constructor_Throws_ArgumentNullException_When_AzureTableReaderWriter_Paramter_Is_Null()
        {
            Assert.Throws<ArgumentNullException>(
                    () => new AggregationService(_fakeLogger, null!, _fakeAzureBlobEventWriter, _fakeSapClient, _fakeSapConfig, _fakeRecordOfSaleSapMessageBuilder))
                .ParamName
                .Should().Be("azureTableReaderWriter");
        }
         
        [Test]
        public void Does_Constructor_Throws_ArgumentNullException_When_AzureBlobEventWriter_Paramter_Is_Null()
        {
            Assert.Throws<ArgumentNullException>(
                    () => new AggregationService(_fakeLogger, _fakeAzureTableReaderWriter, null!, _fakeSapClient, _fakeSapConfig, _fakeRecordOfSaleSapMessageBuilder))
                .ParamName
                .Should().Be("azureBlobEventWriter");
        }

        [Test]
        public void Does_Constructor_Throws_ArgumentNullException_When_Paramter_Is_Null()
        {
            Assert.Throws<ArgumentNullException>(
                    () => new AggregationService(_fakeLogger,
                        _fakeAzureTableReaderWriter,
                        _fakeAzureBlobEventWriter,
                        _fakeSapClient,
                        null!,
                        _fakeRecordOfSaleSapMessageBuilder))
                .ParamName
                .Should().Be("sapConfig");
        }

        [Test]
        public async Task WhenRecordOfSaleEntityStatusIsCompleted_ThenShouldNotMerge()
        {
            string messageText =
                "{\"type\":\"uk.gov.ukho.shop.recordOfSale.v1\",\"eventId\":\"ad5b0ca4-2668-4345-9699-49d8f2c5a006\",\"correlationId\":\"999ce4a4-1d62-4f56-b359-59e178d77003\",\"relatedEvents\":[\"e744fa37-0c9f-4795-adc9-7f42ad8f005\",\"ad5b0ca4-2668-4345-9699-49d8f2c5a006\"],\"transactionType\":\"NEWLICENCE\"}";

            QueueMessage queueMessage = QueuesModelFactory.QueueMessage("12345", "pr1", messageText, 1, DateTimeOffset.UtcNow);

            A.CallTo(() => _fakeAzureTableReaderWriter.GetEntityStatus(A<string>.Ignored)).Returns("Complete");

            await _fakeAggregationService.MergeRecordOfSaleEvents(queueMessage);

            A.CallTo(() => _fakeAzureBlobEventWriter.GetBlobNamesInFolder(A<string>.Ignored, A<string>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => _fakeAzureBlobEventWriter.DownloadEvent(A<string>.Ignored, A<string>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => _fakeAzureTableReaderWriter.UpdateRecordOfSaleEventStatus(A<string>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => _fakeAzureBlobEventWriter.UploadEvent(A<string>.Ignored, A<string>.Ignored, A<string>.Ignored)).MustNotHaveHappened();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
            && call.GetArgument<LogLevel>(0) == LogLevel.Warning
            && call.GetArgument<EventId>(1) == EventIds.RequestAlreadyCompleted.ToEventId()
            && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "The record has been completed already.").MustHaveHappenedOnceExactly();
        }
    }
}
