using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using UKHO.ERPFacade.API.Services.EventPublishingServices;
using UKHO.ERPFacade.Common.Configuration;
using UKHO.ERPFacade.Common.Constants;
using UKHO.ERPFacade.Common.HttpClients;
using UKHO.ERPFacade.Common.Logging;
using UKHO.ERPFacade.Common.Models.CloudEvents;
using UKHO.ERPFacade.Common.Operations.IO.Azure;

namespace UKHO.ERPFacade.API.UnitTests.Services.EventPublishingServicesTests
{
    [TestFixture]
    public class S100UnitOfSaleUpdatedEventPublishingServiceTests
    {
        private IEesClient _fakeEesClient;
        private IOptions<EESConfiguration> _fakeEesConfig;
        private IAzureBlobReaderWriter _fakeAzureBlobReaderWriter;
        private ILogger<S100UnitOfSaleUpdatedEventPublishingService> _fakeLogger;
        private S100UnitOfSaleUpdatedEventPublishingService _fakeS100UnitOfSaleUpdatedEventPublishingService;

        [SetUp]
        public void Setup()
        {
            _fakeEesClient = A.Fake<IEesClient>();
            _fakeEesConfig = A.Fake<IOptions<EESConfiguration>>();
            _fakeAzureBlobReaderWriter = A.Fake<IAzureBlobReaderWriter>();
            _fakeLogger = A.Fake<ILogger<S100UnitOfSaleUpdatedEventPublishingService>>();
            _fakeS100UnitOfSaleUpdatedEventPublishingService = new S100UnitOfSaleUpdatedEventPublishingService(_fakeEesClient, _fakeEesConfig, _fakeAzureBlobReaderWriter, _fakeLogger);
        }

        [Test]
        public async Task WhenValidCorrelationIdAndPayloadIsProvided_ThenEventPublishedSuccessfully()
        {
            string correlationId = Guid.NewGuid().ToString();
            BaseCloudEvent fakeBaseCloudEvent =new()
            {
                Type = EventTypes.S100UnitOfSaleUpdatedEventType,
                Source = _fakeEesConfig.Value.SourceApplicationUri,
                Id = correlationId,
                Time = DateTime.UtcNow.ToString()
            };

            await _fakeS100UnitOfSaleUpdatedEventPublishingService.BuildAndPublishEventAsync(fakeBaseCloudEvent, correlationId);

            A.CallTo(() => _fakeAzureBlobReaderWriter.UploadEventAsync(A<string>.Ignored, A<string>.Ignored, A<string>.Ignored)).MustHaveHappenedOnceOrMore();
            A.CallTo(() => _fakeEesClient.PublishEventAsync(A<BaseCloudEvent>.Ignored)).MustHaveHappenedOnceOrMore();
            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
                                                && call.GetArgument<LogLevel>(0) == LogLevel.Information
                                                && call.GetArgument<EventId>(1) == EventIds.S100UnitOfSaleUpdatedEventJsonStoredInAzureBlobContainer.ToEventId()
                                                && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "S-100 unit of sale updated event json payload is stored in azure blob container.").MustHaveHappenedOnceExactly();
        }
    }
}
