﻿using System;
using System.Collections.Generic;
using System.Linq;
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
using UKHO.ERPFacade.Common.Infrastructure.EventService;
using UKHO.ERPFacade.Common.Infrastructure.EventService.EventProvider;
using UKHO.ERPFacade.Common.Logging;

namespace UKHO.ERPFacade.Common.UnitTests.Infrastructure.EventService.EventProvider
{
    public class EnterpriseEventServiceEventPublisherTests
    {
        private readonly string _fakeServiceUrl = "https://testservice.com";
        private EnterpriseEventServiceEventPublisher _fakeEnterpriseEventServiceEventPublisher;
        private ICloudEventFactory _fakeCloudEventFactory;
        private IHttpClientFactory _fakeHttpClientFactory;
        private MockHttpMessageHandler _fakeHttpClientMessageHandler;
        private OptionsWrapper<EnterpriseEventServiceConfiguration> _optionsWrapper;
        private HttpClient _fakeHttpClient;
        private EnterpriseEventServiceConfiguration _fakeEnterpriseEventServiceConfiguration;
        private ILogger<EnterpriseEventServiceEventPublisher> _fakeLogger;

        [SetUp]
        public void Setup()
        {
            _fakeLogger = A.Fake<ILogger<EnterpriseEventServiceEventPublisher>>();
            _fakeCloudEventFactory = A.Fake<ICloudEventFactory>();
            _fakeHttpClientFactory = A.Fake<IHttpClientFactory>();
            _fakeHttpClientMessageHandler = new MockHttpMessageHandler();
            _fakeEnterpriseEventServiceConfiguration = new EnterpriseEventServiceConfiguration
            {
                ClientId = "testClientId",
                PublishEndpoint = "testPublishEndpoint",
                PublisherScope = "testScope",
                ServiceUrl = _fakeServiceUrl,
            };

            _optionsWrapper = new OptionsWrapper<EnterpriseEventServiceConfiguration>(_fakeEnterpriseEventServiceConfiguration);

            _fakeHttpClient = _fakeHttpClientMessageHandler.ToHttpClient();
            _fakeHttpClient.BaseAddress = new Uri(_fakeServiceUrl);
            A.CallTo(() => _fakeHttpClientFactory.CreateClient(EnterpriseEventServiceEventPublisher.EventServiceClientName)).Returns(_fakeHttpClient);

            _fakeEnterpriseEventServiceEventPublisher = new EnterpriseEventServiceEventPublisher(_fakeLogger, _fakeCloudEventFactory, _fakeHttpClientFactory, _optionsWrapper);
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
                .Expect(HttpMethod.Post, $"{_fakeServiceUrl}/{_fakeEnterpriseEventServiceConfiguration.PublishEndpoint}")
                .Respond(req => new HttpResponseMessage());

            await _fakeEnterpriseEventServiceEventPublisher.Publish(eventData);

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
                .With(req => string.Equals(req.Content!.Headers.ContentType!.MediaType, "application/cloudevents+json", StringComparison.CurrentCultureIgnoreCase))
                .With(req => req.Content!.ReadAsByteArrayAsync().WaitForResult().AreEquivalent(JsonSerializer.SerializeToUtf8Bytes(cloudEvent, jsonOptions)))
                .Respond(req => new HttpResponseMessage());

            var result = await _fakeEnterpriseEventServiceEventPublisher.Publish(eventData);

            A.CallTo(() => _fakeHttpClientFactory.CreateClient(EnterpriseEventServiceEventPublisher.EventServiceClientName)).MustHaveHappened();
            _fakeHttpClientMessageHandler.VerifyNoOutstandingExpectation();
            Assert.That(result.Status, Is.EqualTo(Result.Statuses.Success));
            Assert.That(result.Message, Is.EqualTo("Successfully sent event"));

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
            && call.GetArgument<LogLevel>(0) == LogLevel.Trace
            && call.GetArgument<EventId>(1) == EventIds.StartingEnterpriseEventServiceEventPublisher.ToEventId()
            && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Attempting to send {cloudEventType} for {cloudEventSubject} to Enterprise Event Service").MustHaveHappenedOnceExactly();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
            && call.GetArgument<LogLevel>(0) == LogLevel.Trace
            && call.GetArgument<EventId>(1) == EventIds.EnterpriseEventServiceEventPublisherSuccess.ToEventId()
            && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Successfully sent {cloudEventType} for {cloudEventSubject} to Enterprise Event Service").MustHaveHappenedOnceExactly();
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

            var result = await _fakeEnterpriseEventServiceEventPublisher.Publish(eventData);

            _fakeHttpClientMessageHandler.VerifyNoOutstandingExpectation();
            Assert.That(result.Status, Is.EqualTo(Result.Statuses.Failure));

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
            && call.GetArgument<LogLevel>(0) == LogLevel.Trace
            && call.GetArgument<EventId>(1) == EventIds.StartingEnterpriseEventServiceEventPublisher.ToEventId()
            && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Attempting to send {cloudEventType} for {cloudEventSubject} to Enterprise Event Service").MustHaveHappenedOnceExactly();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
            && call.GetArgument<LogLevel>(0) == LogLevel.Error
            && call.GetArgument<EventId>(1) == EventIds.EnterpriseEventServiceEventPublisherFailure.ToEventId()
            && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Failed to send event type: {cloudEventType} to the enterprise event service for product: {cloudEventSubject}").MustHaveHappenedOnceExactly();
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

            var result = await _fakeEnterpriseEventServiceEventPublisher.Publish(eventData);

            _fakeHttpClientMessageHandler.VerifyNoOutstandingExpectation();
            Assert.That(result.Status, Is.EqualTo(Result.Statuses.Failure));

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
            && call.GetArgument<LogLevel>(0) == LogLevel.Trace
            && call.GetArgument<EventId>(1) == EventIds.StartingEnterpriseEventServiceEventPublisher.ToEventId()
            && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Attempting to send {cloudEventType} for {cloudEventSubject} to Enterprise Event Service").MustHaveHappenedOnceExactly();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
            && call.GetArgument<LogLevel>(0) == LogLevel.Error
            && call.GetArgument<EventId>(1) == EventIds.EnterpriseEventServiceEventPublisherFailure.ToEventId()
            && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Failed to send event type: {cloudEventType} to the enterprise event service for product: {cloudEventSubject}").MustHaveHappenedOnceExactly();
        }
    }
}
