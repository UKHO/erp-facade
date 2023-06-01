using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using RichardSzalay.MockHttp;
using UKHO.ERPFacade.Common.Converters;
using UKHO.ERPFacade.Common.Infrastructure;
using UKHO.ERPFacade.Common.Infrastructure.Config;
using UKHO.ERPFacade.Common.Infrastructure.EventService.EventProvider;

namespace UKHO.ERPFacade.Common.UnitTests.Infrastructure.EventService.EventProvider
{
    public class EnterpriseEventServiceEventPublisherTests
    {
        private readonly string _serviceUrl = "https://testservice.com";
        private EnterpriseEventServiceEventPublisher _sut;
        private ICloudEventFactory _fakeCloudEventFactory;
        private IHttpClientFactory _fakeHttpClientFactory;
        private MockHttpMessageHandler _fakeHttpClientMessageHandler;
        private OptionsWrapper<EnterpriseEventServiceConfiguration> _optionsWrapper;
        private HttpClient _fakeHttpClient;
        private EnterpriseEventServiceConfiguration _testConfiguration;
        private ILogger<EnterpriseEventServiceEventPublisher> _fakeLogger;

        [SetUp]
        public void Setup()
        {
            _fakeLogger = A.Fake<ILogger<EnterpriseEventServiceEventPublisher>>();
            _fakeCloudEventFactory = A.Fake<ICloudEventFactory>();
            _fakeHttpClientFactory = A.Fake<IHttpClientFactory>();
            _fakeHttpClientMessageHandler = new MockHttpMessageHandler();
            _testConfiguration = new EnterpriseEventServiceConfiguration
            {
                ClientId = "testClientId",
                PublishEndpoint = "testPublishEndpoint",
                PublisherScope = "testScope",
                ServiceUrl = _serviceUrl,
            };

            _optionsWrapper = new OptionsWrapper<EnterpriseEventServiceConfiguration>(_testConfiguration);

            _fakeHttpClient = _fakeHttpClientMessageHandler.ToHttpClient();
            _fakeHttpClient.BaseAddress = new Uri(_serviceUrl);
            A.CallTo(() => _fakeHttpClientFactory.CreateClient(EnterpriseEventServiceEventPublisher.EventServiceClientName)).Returns(_fakeHttpClient);

            _sut = new EnterpriseEventServiceEventPublisher(_fakeLogger, _fakeCloudEventFactory, _fakeHttpClientFactory, _optionsWrapper);
        }

        [Test]
        public async Task Publish_SendsSerializedEventData_UsingEventServiceHttpClient()
        {
            var eventData = A.Dummy<EventBase<string>>();
            var cloudEvent = new CloudEvent<string>
            {
                Data = "test"
            };

            A.CallTo(() => _fakeCloudEventFactory.Create(eventData)).Returns(cloudEvent);
            _fakeHttpClientMessageHandler
                .Expect(HttpMethod.Post, $"{_serviceUrl}/{_testConfiguration.PublishEndpoint}")
                .Respond(req => new HttpResponseMessage());

            await _sut.Publish(eventData);

            A.CallTo(() => _fakeHttpClientFactory.CreateClient(EnterpriseEventServiceEventPublisher.EventServiceClientName)).MustHaveHappened();
            _fakeHttpClientMessageHandler.VerifyNoOutstandingExpectation();
        }

        [Test]
        public async Task Publish_SendsSerializedEventData_SendsAValidCloudEventRequest()
        {
            var eventData = A.Dummy<EventBase<string>>();
            var cloudEvent = new CloudEvent<string>
            {
                Data = "test"
            };

            var jsonOptions = new JsonSerializerOptions();
            jsonOptions.Converters.Add(new RoundTripDateTimeConverter());
            A.CallTo(() => _fakeCloudEventFactory.Create(eventData)).Returns(cloudEvent);

            _fakeHttpClientMessageHandler
                .Expect("*")
                .With(req => string.Equals(req.Content.Headers.ContentType.MediaType, "application/cloudevents+json", StringComparison.CurrentCultureIgnoreCase))
                //.With(req => req.Content.ReadAsByteArrayAsync().WaitForResult().AreEquivalent(JsonSerializer.SerializeToUtf8Bytes(cloudEvent, jsonOptions)))
                .Respond(req => new HttpResponseMessage());

            var result = await _sut.Publish(eventData);

            A.CallTo(() => _fakeHttpClientFactory.CreateClient(EnterpriseEventServiceEventPublisher.EventServiceClientName)).MustHaveHappened();
            _fakeHttpClientMessageHandler.VerifyNoOutstandingExpectation();
            Assert.That(result.Status, Is.EqualTo(Result.Statuses.Success));
        }

        [Test]
        public async Task Publish_SendsSerializedEventData_ReturnsFailureIfHttpRequestThrows()
        {
            var eventData = A.Dummy<EventBase<string>>();
            var cloudEvent = new CloudEvent<string>
            {
                Data = "test"
            };

            A.CallTo(() => _fakeCloudEventFactory.Create(eventData)).Returns(cloudEvent);
            _fakeHttpClientMessageHandler
                .Expect("*")
                .Throw(new Exception());

            var result = await _sut.Publish(eventData);

            _fakeHttpClientMessageHandler.VerifyNoOutstandingExpectation();
            Assert.That(result.Status, Is.EqualTo(Result.Statuses.Failure));
        }

        [Test]
        public async Task Publish_SendsSerializedEventData_ReturnsFailureIfHttpResponseIsNotSuccess()
        {
            var eventData = A.Dummy<EventBase<string>>();
            var cloudEvent = new CloudEvent<string>
            {
                Data = "test"
            };

            A.CallTo(() => _fakeCloudEventFactory.Create(eventData)).Returns(cloudEvent);
            _fakeHttpClientMessageHandler
                .Expect("*")
                .Respond(req => new HttpResponseMessage(HttpStatusCode.InternalServerError));

            var result = await _sut.Publish(eventData);

            _fakeHttpClientMessageHandler.VerifyNoOutstandingExpectation();
            Assert.That(result.Status, Is.EqualTo(Result.Statuses.Failure));
        }
    }

    public static class TaskSynchronousExtensions
    {
        public static T WaitForResult<T>(this Task<T> task)
        {
            return task.ConfigureAwait(false).GetAwaiter().GetResult();
        }
    }
}