using System;
using FakeItEasy;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using UKHO.ERPFacade.Common.Infrastructure.Config;
using UKHO.ERPFacade.Common.Infrastructure.EventService.EventProvider;
using UKHO.ERPFacade.Common.Providers;

namespace UKHO.ERPFacade.Common.UnitTests.Infrastructure.EventService.EventProvider
{
    public class CloudEventFactoryTests
    {
        private DateTime _fakeCurrentDateTime;
        private CloudEventFactory _fakeCloudEventFactory;
        private IDateTimeProvider _fakeDateTimeProvider;
        private IUniqueIdentifierFactory _fakeUniqueIdentifierFactory;
        private NotificationsConfiguration _fakeNotificationsConfiguration;

        [SetUp]
        public void Setup()
        {
            _fakeDateTimeProvider = A.Fake<IDateTimeProvider>();
            _fakeCurrentDateTime = new DateTime(1983, 4, 27);
            A.CallTo(() => _fakeDateTimeProvider.UtcNow).Returns(_fakeCurrentDateTime);

            _fakeUniqueIdentifierFactory = A.Fake<IUniqueIdentifierFactory>();
            A.CallTo(() => _fakeUniqueIdentifierFactory.Create()).Returns("myId");

            _fakeNotificationsConfiguration = new NotificationsConfiguration()
            {
                ApplicationUri = "https://ourdomain.org/"
            };

            _fakeCloudEventFactory = new CloudEventFactory(_fakeDateTimeProvider, _fakeUniqueIdentifierFactory, new OptionsWrapper<NotificationsConfiguration>(_fakeNotificationsConfiguration));
        }

        //[Test]
        //public void WhenCloudEventFactoryCreateIsCalled_ThenObjectWithTheCorrectMappingsIsReturned()
        //{
        //    var productUpdatedData = new UnitOfSaleUpdatedEventData()
        //    {
        //        Subject = "MyProductName"
        //    };

        //    var result = _fakeCloudEventFactory.Create(new UnitOfSaleUpdatedEventPayload(productUpdatedData));

        //    result.Data.Should().Be(productUpdatedData);
        //    result.Type.Should().Be("uk.gov.ukho.encpublishing.enccontentpublished.v2");
        //    result.Subject.Should().Be("MyProductName");
        //    result.Time.Should().Be(_fakeCurrentDateTime);
        //    result.Id.Should().Be("myId");
        //    result.Source.Should().Be(_fakeNotificationsConfiguration.ApplicationUri);
        //    result.SpecVersion.Should().Be("1.0");
        //    result.DataContentType.Should().Be("application/json");
        //}
    }
}
