using FakeItEasy;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using System;
using UKHO.ERPFacade.Common.Infrastructure.Config;
using UKHO.ERPFacade.Common.Infrastructure.EventService.EventProvider;
using UKHO.ERPFacade.Common.Providers;
using UKHO.ERPFacade.MockAPIService.Models;

namespace UKHO.ERPFacade.Common.UnitTests.Infrastructure.EventService.EventProvider
{
    internal class CloudEventFactoryTests
    {
        private DateTime _currentDateTime;
        private CloudEventFactory _factory;
        private IDateTimeProvider _fakeDateTimeProvider;
        private IUniqueIdentifierFactory _fakeUniqueIdentifierFactory;
        private NotificationsConfiguration _notificationsConfiguration;

        [SetUp]
        public void Setup()
        {
            _fakeDateTimeProvider = A.Fake<IDateTimeProvider>();
            _currentDateTime = new DateTime(1983, 4, 27);
            A.CallTo(() => _fakeDateTimeProvider.UtcNow).Returns(_currentDateTime);

            _fakeUniqueIdentifierFactory = A.Fake<IUniqueIdentifierFactory>();
            A.CallTo(() => _fakeUniqueIdentifierFactory.Create()).Returns("myId");

            _notificationsConfiguration = new NotificationsConfiguration()
            {
                ApplicationUri = "https://ourdomain.org/"
            };

            _factory = new CloudEventFactory(_fakeDateTimeProvider, _fakeUniqueIdentifierFactory, new OptionsWrapper<NotificationsConfiguration>(_notificationsConfiguration));
        }

        [Test]
        public void TestFactoryCreatesObjectWithTheCorrectMappings()
        {
            var productUpdatedData = new UnitOfSalePriceEvent()
            {
                Subject = "MyProductName"
            };

            var result = _factory.Create(new UnitOfSalePriceEventPayload(productUpdatedData));

            Assert.AreSame(productUpdatedData, result.Data);
            Assert.AreEqual("uk.gov.UKHO.catalogue.productUpdated.v1", result.Type);
            Assert.AreEqual("MyProductName", result.Subject);
            Assert.AreEqual(_currentDateTime, result.Time);
            Assert.AreEqual("myId", result.Id);
            Assert.AreEqual(_notificationsConfiguration.ApplicationUri, result.Source);
            Assert.AreEqual("1.0", result.SpecVersion);
            Assert.AreEqual("application/json", result.DataContentType);
        }
    }
}