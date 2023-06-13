using FakeItEasy;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using UKHO.ERPFacade.Common.IO.Azure;
using UKHO.ERPFacade.PublishPriceChange.WebJob.Services;
using FluentAssertions;

namespace UKHO.ERPFacade.PublishPriceChange.WebJob.UnitTests.Services
{
    [TestFixture]
    public class SlicingPublishingServiceTests
    {
        private IAzureTableReaderWriter _fakeAzureTableReaderWriter;
        private ILogger<SlicingPublishingService> _fakeLogger;
        private IAzureBlobEventWriter _fakeAzureBlobEventWriter;
        private SlicingPublishingService _fakeMonitoringService;

        [SetUp]
        public void Setup()
        {
            _fakeAzureTableReaderWriter = A.Fake<IAzureTableReaderWriter>();
            _fakeLogger = A.Fake<ILogger<SlicingPublishingService>>();
            _fakeAzureBlobEventWriter = A.Fake<IAzureBlobEventWriter>();
            _fakeMonitoringService = new SlicingPublishingService(_fakeLogger, _fakeAzureTableReaderWriter, _fakeAzureBlobEventWriter);
        }

        [Test]
        public void Does_Constructor_Throws_ArgumentNullException_When_AzureTableReaderWriter_Paramter_Is_Null()
        {
            Assert.Throws<ArgumentNullException>(
             () => new SlicingPublishingService(_fakeLogger, null, _fakeAzureBlobEventWriter))
             .ParamName
             .Should().Be("azureTableReaderWriter");
        }

        [Test]
        public void Does_Constructor_Throws_ArgumentNullException_When_Logger_Paramter_Is_Null()
        {
            Assert.Throws<ArgumentNullException>(
             () => new SlicingPublishingService(null, _fakeAzureTableReaderWriter, _fakeAzureBlobEventWriter))
             .ParamName
             .Should().Be("logger");
        }

        [Test]
        public void Does_Constructor_Throws_ArgumentNullException_When_AzureBlobEventWriter_Paramter_Is_Null()
        {
            Assert.Throws<ArgumentNullException>(
             () => new SlicingPublishingService(_fakeLogger, _fakeAzureTableReaderWriter, null))
             .ParamName
             .Should().Be("azureBlobEventWriter");
        }
    }
}
