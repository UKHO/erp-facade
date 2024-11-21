using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using RichardSzalay.MockHttp;
using UKHO.ERPFacade.Common.Authentication;
using UKHO.ERPFacade.Common.Configuration;
using UKHO.ERPFacade.Common.HttpClients;
using UKHO.ERPFacade.Common.Logging;
using UKHO.ERPFacade.Common.Models.CloudEvents;

namespace UKHO.ERPFacade.Common.UnitTests.HttpClients
{
    [TestFixture]
    public class EesClientTests
    {
        private HttpClient _fakeHttpClient;
        private MockHttpMessageHandler _fakeHttpClientMessageHandler;
        private ILogger<EesClient> _fakeLogger;
        private ITokenProvider _fakeTokenProvider;
        private EESConfiguration _fakeEesConfiguration;
        private OptionsWrapper<EESConfiguration> _optionsWrapper;
        private EesClient _fakeEesClient;

        [SetUp]
        public void Setup()
        {
            _fakeLogger = A.Fake<ILogger<EesClient>>();
            _fakeHttpClientMessageHandler = new MockHttpMessageHandler();
            _fakeHttpClient = A.Fake<HttpClient>();
            _fakeTokenProvider = A.Fake<ITokenProvider>();
            _fakeEesConfiguration = new EESConfiguration
            {
                ClientId = "testClientId",
                PublishEndpoint = "testPublishEndpoint",
                PublisherScope = "testScope",
                BaseAddress = "https://testbaseaddress.com",
                UseLocalResources = true,
            };

            _optionsWrapper = new OptionsWrapper<EESConfiguration>(_fakeEesConfiguration);
            _fakeHttpClient = _fakeHttpClientMessageHandler.ToHttpClient();
            _fakeHttpClient.BaseAddress = new Uri(_fakeEesConfiguration.BaseAddress);

            _fakeEesClient = new EesClient(_fakeHttpClient, _fakeLogger, _fakeTokenProvider, _optionsWrapper);
        }

        [TearDown]
        public void TearDown()
        {
            _fakeHttpClientMessageHandler.Dispose();
            _fakeHttpClient.Dispose();
        }

        [Test]
        public async Task Publish_GetData_UsingEventServiceHttpClient()
        {
            var cloudEvent = new BaseCloudEvent { Data = "test" };

            _fakeHttpClientMessageHandler
                .Expect(HttpMethod.Get, $"{_fakeEesConfiguration.BaseAddress}/{_fakeEesConfiguration.PublishEndpoint}")
                .Respond(req => new HttpResponseMessage());

           var result = await _fakeEesClient.Get($"{_fakeEesConfiguration.BaseAddress}/{_fakeEesConfiguration.PublishEndpoint}");

            _fakeHttpClientMessageHandler.VerifyNoOutstandingExpectation();
        }

        [Test]
        public async Task Publish_PostSerializedEventData_SendsAValidCloudEventRequest_ResultedInStatusSuccess()
        {
            var cloudEvent = new BaseCloudEvent { Data = "test" };

            _fakeHttpClientMessageHandler
                .Expect(HttpMethod.Post, $"{_fakeEesConfiguration.BaseAddress}/{_fakeEesConfiguration.PublishEndpoint}")
                .Respond(req => new HttpResponseMessage(HttpStatusCode.OK));
            A.CallTo(() => _fakeTokenProvider.GetTokenAsync(A<string>.Ignored)).Returns("fakeAuthToken");

            var result = await _fakeEesClient.PostAsync(cloudEvent);

            _fakeHttpClientMessageHandler.VerifyNoOutstandingExpectation();
            Assert.That(result.IsSuccess, Is.EqualTo(true));

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
            && call.GetArgument<LogLevel>(0) == LogLevel.Information
            && call.GetArgument<EventId>(1) == EventIds.StartingEnterpriseEventServiceEventPublisher.ToEventId()
            && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Attempting to publish {cloudEventType} event to Enterprise Event Service.").MustHaveHappenedOnceExactly();
        }

        [Test]
        public async Task Publish_PostSerializedEventData_SendsAValidCloudEventRequest_ResultedInStatusFailure()
        {
            var cloudEvent = new BaseCloudEvent { Data = "test" };
            _fakeEesConfiguration.UseLocalResources = false;

            _fakeHttpClientMessageHandler
                .Expect(HttpMethod.Post, $"{_fakeEesConfiguration.BaseAddress}/{_fakeEesConfiguration.PublishEndpoint}")
                .Respond(req => new HttpResponseMessage(HttpStatusCode.InternalServerError));
            A.CallTo(() => _fakeTokenProvider.GetTokenAsync(A<string>.Ignored)).Returns("fakeAuthToken");

            var result = await _fakeEesClient.PostAsync(cloudEvent);

            _fakeHttpClientMessageHandler.VerifyNoOutstandingExpectation();
            Assert.That(result.IsSuccess, Is.EqualTo(false));

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
            && call.GetArgument<LogLevel>(0) == LogLevel.Information
            && call.GetArgument<EventId>(1) == EventIds.StartingEnterpriseEventServiceEventPublisher.ToEventId()
            && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Attempting to publish {cloudEventType} event to Enterprise Event Service.").MustHaveHappenedOnceExactly();
        }
    }
}
