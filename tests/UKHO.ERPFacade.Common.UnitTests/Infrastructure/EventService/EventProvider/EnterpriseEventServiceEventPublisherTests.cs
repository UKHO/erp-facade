using System;
using System.Net.Http;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using RichardSzalay.MockHttp;
using UKHO.ERPFacade.Common.Infrastructure.Config;
using UKHO.ERPFacade.Common.Infrastructure.EventService;
using UKHO.ERPFacade.Common.Infrastructure.EventService.EventProvider;
using UKHO.ERPFacade.Common.IO.Azure;

namespace UKHO.ERPFacade.Common.UnitTests.Infrastructure.EventService.EventProvider
{
    public class EnterpriseEventServiceEventPublisherTests
    {
        private readonly string _fakeServiceUrl = "https://testservice.com";
        private EnterpriseEventServiceEventPublisher _fakeEnterpriseEventServiceEventPublisher;
        private ICloudEventFactory _fakeCloudEventFactory;
        private IHttpClientFactory _fakeHttpClientFactory;
        private MockHttpMessageHandler _fakeHttpClientMessageHandler;
        private OptionsWrapper<ErpPublishEventSource> _optionsWrapper;
        private HttpClient _fakeHttpClient;
        private ErpPublishEventSource _fakeErpPublishEventSource;
        private ILogger<EnterpriseEventServiceEventPublisher> _fakeLogger;
        private IAzureBlobEventWriter _fakeAzureBlobEventWriter;

        [SetUp]
        public void Setup()
        {
            _fakeLogger = A.Fake<ILogger<EnterpriseEventServiceEventPublisher>>();
            _fakeCloudEventFactory = A.Fake<ICloudEventFactory>();
            _fakeHttpClientFactory = A.Fake<IHttpClientFactory>();
            _fakeHttpClientMessageHandler = new MockHttpMessageHandler();
            _fakeAzureBlobEventWriter = A.Fake<IAzureBlobEventWriter>();
            _fakeErpPublishEventSource = new ErpPublishEventSource
            {
                ClientId = "testClientId",
                PublishEndpoint = "testPublishEndpoint",
                PublisherScope = "testScope",
                ServiceUrl = _fakeServiceUrl,
            };

            _optionsWrapper = new OptionsWrapper<ErpPublishEventSource>(_fakeErpPublishEventSource);

            _fakeHttpClient = _fakeHttpClientMessageHandler.ToHttpClient();
            _fakeHttpClient.BaseAddress = new Uri(_fakeServiceUrl);
            A.CallTo(() => _fakeHttpClientFactory.CreateClient(EnterpriseEventServiceEventPublisher.EventServiceClientName)).Returns(_fakeHttpClient);

            _fakeEnterpriseEventServiceEventPublisher = new EnterpriseEventServiceEventPublisher(_fakeLogger, _fakeCloudEventFactory, _fakeHttpClientFactory, _optionsWrapper, _fakeAzureBlobEventWriter);
        }

        //[Test]
        //public async Task Publish_SendsSerializedEventData_UsingEventServiceHttpClient()
        //{
        //    var eventData = A.Dummy<EventBase<string>>();
        //    var cloudEvent = new CloudEvent<string>
        //    {
        //        Data = "test"
        //    };

        //    A.CallTo(() => _fakeCloudEventFactory.Create(eventData)).Returns(cloudEvent);
        //    _fakeHttpClientMessageHandler
        //        .Expect(HttpMethod.Post, $"{_fakeServiceUrl}/{_fakeErpPublishEventSource.PublishEndpoint}")
        //        .Respond(req => new HttpResponseMessage());

        //    await _fakeEnterpriseEventServiceEventPublisher.Publish(cloudEvent);

        //    A.CallTo(() => _fakeHttpClientFactory.CreateClient(EnterpriseEventServiceEventPublisher.EventServiceClientName)).MustHaveHappened();
        //    _fakeHttpClientMessageHandler.VerifyNoOutstandingExpectation();
        //}

        //[Test]
        //public async Task Publish_SendsSerializedEventData_SendsAValidCloudEventRequest()
        //{
        //    var eventData = A.Dummy<EventBase<string>>();
        //    var cloudEvent = new CloudEvent<string>
        //    {
        //        Data = "test"
        //    };

        //    var jsonOptions = new JsonSerializerOptions();
        //    jsonOptions.Converters.Add(new RoundTripDateTimeConverter());
        //    A.CallTo(() => _fakeCloudEventFactory.Create(eventData)).Returns(cloudEvent);

        //    _fakeHttpClientMessageHandler
        //        .Expect("*")
        //        .With(req => string.Equals(req.Content!.Headers.ContentType!.MediaType, "application/cloudevents+json", StringComparison.CurrentCultureIgnoreCase))
        //        .With(req => req.Content!.ReadAsByteArrayAsync().WaitForResult().AreEquivalent(JsonSerializer.SerializeToUtf8Bytes(cloudEvent, jsonOptions)))
        //        .Respond(req => new HttpResponseMessage());

        //    var result = await _fakeEnterpriseEventServiceEventPublisher.Publish(cloudEvent);

        //    A.CallTo(() => _fakeHttpClientFactory.CreateClient(EnterpriseEventServiceEventPublisher.EventServiceClientName)).MustHaveHappened();
        //    _fakeHttpClientMessageHandler.VerifyNoOutstandingExpectation();
        //    Assert.That(result.Status, Is.EqualTo(Result.Statuses.Success));
        //}

        //[Test]
        //public async Task Publish_SendsSerializedEventData_ReturnsFailureIfHttpRequestThrows()
        //{
        //    var eventData = A.Dummy<EventBase<string>>();
        //    var cloudEvent = new CloudEvent<string>
        //    {
        //        Data = "test"
        //    };

        //    A.CallTo(() => _fakeCloudEventFactory.Create(eventData)).Returns(cloudEvent);
        //    _fakeHttpClientMessageHandler
        //        .Expect("*")
        //        .Throw(new Exception());

        //    var result = await _fakeEnterpriseEventServiceEventPublisher.Publish(cloudEvent);

        //    _fakeHttpClientMessageHandler.VerifyNoOutstandingExpectation();
        //    Assert.That(result.Status, Is.EqualTo(Result.Statuses.Failure));
        //}

        //[Test]
        //public async Task Publish_SendsSerializedEventData_ReturnsFailureIfHttpResponseIsNotSuccess()
        //{
        //    var eventData = A.Dummy<EventBase<string>>();
        //    var cloudEvent = new CloudEvent<string>
        //    {
        //        Data = "test"
        //    };

        //    A.CallTo(() => _fakeCloudEventFactory.Create(eventData)).Returns(cloudEvent);
        //    _fakeHttpClientMessageHandler
        //        .Expect("*")
        //        .Respond(req => new HttpResponseMessage(HttpStatusCode.InternalServerError));

        //    var result = await _fakeEnterpriseEventServiceEventPublisher.Publish(cloudEvent);

        //    _fakeHttpClientMessageHandler.VerifyNoOutstandingExpectation();
        //    Assert.That(result.Status, Is.EqualTo(Result.Statuses.Failure));
        //}
    }
}
