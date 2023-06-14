using FakeItEasy;
using FluentAssertions;
using NUnit.Framework;
using UKHO.ERPFacade.Common.IO.Azure;
using UKHO.ERPFacade.Monitoring.WebJob.Services;

namespace UKHO.ERPFacade.Monitoring.WebJob.UnitTests.Services
{
    [TestFixture]
    public class MonitoringServiceTests
    {
        private IAzureTableReaderWriter _fakeAzureTableReaderWriter;

        private MonitoringService _fakeMonitoringService;

        [SetUp]
        public void Setup()
        {
            _fakeAzureTableReaderWriter = A.Fake<IAzureTableReaderWriter>();
            _fakeMonitoringService = new MonitoringService(_fakeAzureTableReaderWriter);
        }

        [Test]
        public void WhenWebJobStartsAndMonitorsIncompleteTransactions_ThenValidateEntityMethodMustbeCalled()
        {
            _fakeMonitoringService.MonitorIncompleteTransactions();

            A.CallTo(() => _fakeAzureTableReaderWriter.ValidateAndUpdateIsNotifiedEntity()).MustHaveHappened();
        }

        [Test]
        public void WhenValidateEntityThrowsException_ThenThrowsException()
        {
            A.CallTo(() => _fakeAzureTableReaderWriter.ValidateAndUpdateIsNotifiedEntity()).Throws(new NotSupportedException("Fake Exception"));

            var ex = Assert.Throws<NotSupportedException>(() =>
                _fakeMonitoringService.MonitorIncompleteTransactions());

            Assert.That(ex.Message, Is.EqualTo("Fake Exception"));
        }

        [Test]
        public void Does_Constructor_Throws_ArgumentNullException_When_Paramter_Is_Null()
        {
            Assert.Throws<ArgumentNullException>(
             () => new MonitoringService(null))
             .ParamName
             .Should().Be("azureTableReaderWriter");
        }
    }
}