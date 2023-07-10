using System;
using System.Collections.Generic;

namespace UKHO.ERPFacade.Common.UnitTests.Infrastructure.EventService.EventProvider
{
    public class CloudEventFactoryTests
    {
        private DateTime _fakeCurrentDateTime;
        private CloudEventFactory _fakeCloudEventFactory;
        private IDateTimeProvider _fakeDateTimeProvider;
        private ErpPublishEventSource _fakeErpPublishEventSource;

        [SetUp]
        public void Setup()
        {
            _fakeDateTimeProvider = A.Fake<IDateTimeProvider>();
            _fakeCurrentDateTime = new DateTime(1983, 4, 27);
            A.CallTo(() => _fakeDateTimeProvider.UtcNow).Returns(_fakeCurrentDateTime);
            _fakeErpPublishEventSource = new ErpPublishEventSource
            {
                ApplicationUri = "https://ourdomain.org/"
            };

            _fakeCloudEventFactory = new CloudEventFactory(_fakeDateTimeProvider, new OptionsWrapper<ErpPublishEventSource>(_fakeErpPublishEventSource));
        }

        [Test]
        public void WhenCloudEventFactoryCreateIsCalled_ThenObjectWithTheCorrectMappingsIsReturned()
        {
            var unitOfSaleUpdatedEventData = new UnitOfSaleUpdatedEventData()
            {
                CorrelationId = "CorrelationId",
                Products = new List<Product>(),
                UnitsOfSale = new List<UnitOfSale>(),
                UnitsOfSalePrices = new List<UnitsOfSalePrices>()
            };

            var result = _fakeCloudEventFactory.Create(new UnitOfSaleUpdatedEventPayload(unitOfSaleUpdatedEventData, "fakeSubject"));

            result.Data.Should().Be(unitOfSaleUpdatedEventData);
            result.Type.Should().Be("uk.gov.ukho.erp.unitOfSaleUpdated.v1");
            result.Subject.Should().Be("fakeSubject");
            result.Time.Should().Be(_fakeCurrentDateTime);
            result.Source.Should().Be(_fakeErpPublishEventSource.ApplicationUri);
            result.SpecVersion.Should().Be("1.0");
            result.DataContentType.Should().Be("application/json");
        }
    }
}
