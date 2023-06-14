﻿using Azure.Core;
using FakeItEasy;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using Request = WireMock.RequestBuilders.Request;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Threading.Tasks;
using System;
using UKHO.ERPFacade.Common.Infrastructure.Authentication;
using UKHO.ERPFacade.Common.Infrastructure.Config;
using UKHO.ERPFacade.Common.Infrastructure.EventService;
using UKHO.ERPFacade.Common.Infrastructure;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace UKHO.ERPFacade.Common.UnitTests.Infrastructure.ConfigurationTests
{
    public class EventServiceHttpClientTests
    {
        private WireMockServer _fakeWireMockServer;
        private ITokenProvider _fakeTokenProvider;
        private EnterpriseEventServiceConfiguration _fakeEnterpriseEventServiceConfiguration;
        private ServiceProvider _serviceProvider;

        [SetUp]
        public void Setup()
        {
            _fakeWireMockServer = WireMockServer.Start();
            _fakeTokenProvider = A.Fake<ITokenProvider>();
            _fakeEnterpriseEventServiceConfiguration = new EnterpriseEventServiceConfiguration
            {
                ServiceUrl = _fakeWireMockServer.Urls.First(),
            };

            IServiceCollection services = new ServiceCollection();
            services.AddInfrastructure();
            services.Replace(ServiceDescriptor.Singleton(typeof(ITokenProvider), _fakeTokenProvider));
            services.Replace(ServiceDescriptor.Singleton(typeof(IOptions<EnterpriseEventServiceConfiguration>), new OptionsWrapper<EnterpriseEventServiceConfiguration>(_fakeEnterpriseEventServiceConfiguration)));
            _serviceProvider = services.BuildServiceProvider();
        }

        [TearDown]
        public void Teardown()
        {
            _fakeWireMockServer.Stop();
        }

        [TestCase(HttpStatusCode.InternalServerError)]
        [TestCase(HttpStatusCode.ServiceUnavailable)]
        [TestCase(HttpStatusCode.RequestTimeout)]
        [TestCase(HttpStatusCode.GatewayTimeout)]
        public async Task WhenEesEventPublisherClientRetries3TimesAfterInitialRequestForErrorStatuses_ThenReturnsRequestTimeout(HttpStatusCode statusCode)
        {
            var sut = _serviceProvider.GetRequiredService<IHttpClientFactory>().CreateClient(EnterpriseEventServiceEventPublisher.EventServiceClientName);

            var testEndpoint = "/test-endpoint";
            _fakeWireMockServer
                .Given(Request.Create().WithPath(testEndpoint).UsingGet())
                .RespondWith(Response.Create().WithStatusCode(statusCode));

            await sut.GetAsync(testEndpoint);
            Assert.That(_fakeWireMockServer.FindLogEntries(Request.Create().WithPath(testEndpoint).UsingGet()).Count(), Is.EqualTo(4));
        }

        [TestCase(HttpStatusCode.Unauthorized)]
        [TestCase(HttpStatusCode.NotAcceptable)]
        [TestCase(HttpStatusCode.BadRequest)]
        [TestCase(HttpStatusCode.Forbidden)]

        public async Task WhenEesEventPublisherClientDoesNotRetryForNonTransientErrors_ThenReturnUnauthorizedWithLogEntries(HttpStatusCode statusCode)
        {
            var sut = _serviceProvider.GetRequiredService<IHttpClientFactory>().CreateClient(EnterpriseEventServiceEventPublisher.EventServiceClientName);

            var testEndpoint = "/test-endpoint";
            _fakeWireMockServer
                .Given(Request.Create().WithPath(testEndpoint).UsingGet())
                .RespondWith(Response.Create().WithStatusCode(statusCode));

            await sut.GetAsync(testEndpoint);
            Assert.That(_fakeWireMockServer.FindLogEntries(Request.Create().WithPath(testEndpoint).UsingGet()).Count(), Is.EqualTo(1));
        }

        [Test]
        public async Task WhenEesEventPublisherClientIscalled_ThenReturnsBearerAuthToken()
        {
            var testToken = "TOKEN";
            var expectedBearerHeader = $"Bearer {testToken}";
            var testClientId = "CLIENT_ID";
            var testScope = "SCOPE";

            _fakeEnterpriseEventServiceConfiguration.PublisherScope = testScope;
            _fakeEnterpriseEventServiceConfiguration.ClientId = testClientId;

            A.CallTo(() => _fakeTokenProvider.GetTokenAsync($"{testClientId}/{testScope}")).Returns(new AccessToken(testToken, DateTimeOffset.MaxValue));

            var sut = _serviceProvider.GetRequiredService<IHttpClientFactory>().CreateClient(EnterpriseEventServiceEventPublisher.EventServiceClientName);

            var testEndpoint = "/test-endpoint";
            _fakeWireMockServer
                .Given(Request.Create().WithPath(testEndpoint).WithHeader("authorization", expectedBearerHeader).UsingGet())
                .RespondWith(Response.Create().WithSuccess());

            await sut.GetAsync(testEndpoint);
            Assert.That(_fakeWireMockServer.FindLogEntries(Request.Create()
                                                              .WithPath(testEndpoint)
                                                              .WithHeader("authorization", expectedBearerHeader)
                                                              .UsingGet()
                                                              ).Count(), Is.EqualTo(1));
        }
    }
}