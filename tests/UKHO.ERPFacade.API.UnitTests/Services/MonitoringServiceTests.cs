using FakeItEasy;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UKHO.ERPFacade.Common.IO;
using UKHO.ERPFacade.WebJob.Services;

namespace UKHO.ERPFacade.API.UnitTests.Services
{
    [TestFixture]
    public class MonitoringServiceTests
    {
        private ILogger<MonitoringService> _fakeLogger;
        private IAzureTableReaderWriter _fakeAzureTableReaderWriter;

        private MonitoringService _fakeMonitoringService;

        [SetUp]
        public void Setup()
        {
            _fakeLogger = A.Fake<ILogger<MonitoringService>>();
            _fakeAzureTableReaderWriter = A.Fake<IAzureTableReaderWriter>();
            _fakeMonitoringService = new MonitoringService(_fakeLogger, _fakeAzureTableReaderWriter);
        }

        [Test]
        public void WhenWebJobStartsAndMonitorsIncompleteTransactions_ThenValidateEntityMethodMustbeCalled()
        {
            _fakeMonitoringService.MonitorIncompleteTransactions();

            A.CallTo(() => _fakeAzureTableReaderWriter.ValidateEntity()).MustHaveHappened();
        }

        [Test]
        public void WhenValidateEntityThrowsException_ThenThrowsException()
        {

            A.CallTo(() => _fakeAzureTableReaderWriter.ValidateEntity()).Throws(new NotSupportedException("Fake Exception"));

            var ex = Assert.Throws<NotSupportedException>(() =>
                _fakeMonitoringService.MonitorIncompleteTransactions());

            Assert.That(ex.Message, Is.EqualTo("Fake Exception"));

        }
    }
}
