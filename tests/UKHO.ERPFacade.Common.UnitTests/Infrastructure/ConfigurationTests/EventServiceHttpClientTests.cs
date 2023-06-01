using Azure.Core;
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
using UKHO.ERPFacade.Common.Infrastructure.EventService.EventProvider;
using UKHO.ERPFacade.Common.Infrastructure;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace UKHO.ERPFacade.Common.UnitTests.Infrastructure.ConfigurationTests
{
    public class EventServiceHttpClientTests
    {
        private WireMockServer _wireMockServer;
        private ITokenProvider _mockTokenProvider;
        private EnterpriseEventServiceConfiguration _config;
        private ServiceProvider _serviceProvider;

        [SetUp]
        public void Setup()
        {
            _wireMockServer = WireMockServer.Start();
            _mockTokenProvider = A.Fake<ITokenProvider>();
            _config = new EnterpriseEventServiceConfiguration
            {
                ServiceUrl = _wireMockServer.Urls.First(),
            };

            IServiceCollection services = new ServiceCollection();
            services.AddInfrastructure();
            services.Replace(ServiceDescriptor.Singleton(typeof(ITokenProvider), _mockTokenProvider));
            services.Replace(ServiceDescriptor.Singleton(typeof(IOptions<EnterpriseEventServiceConfiguration>), new OptionsWrapper<EnterpriseEventServiceConfiguration>(_config)));
            _serviceProvider = services.BuildServiceProvider();
        }

        [TearDown]
        public void Teardown()
        {
            _wireMockServer.Stop();
        }

        [TestCase(HttpStatusCode.InternalServerError)]
        [TestCase(HttpStatusCode.ServiceUnavailable)]
        [TestCase(HttpStatusCode.RequestTimeout)]
        [TestCase(HttpStatusCode.GatewayTimeout)]
        public async Task Test_EnterpriseEventServiceEventPublisherClient_Retries3TimesAfterInitialRequest_ForErrorStatuses(HttpStatusCode statusCode)
        {
            var sut = _serviceProvider.GetRequiredService<IHttpClientFactory>().CreateClient(EnterpriseEventServiceEventPublisher.EventServiceClientName);

            var testEndpoint = "/test-endpoint";
            _wireMockServer
                .Given(Request.Create().WithPath(testEndpoint).UsingGet())
                .RespondWith(Response.Create().WithStatusCode(statusCode));

            await sut.GetAsync(testEndpoint);
            Assert.That(_wireMockServer.FindLogEntries(Request.Create().WithPath(testEndpoint).UsingGet()).Count(), Is.EqualTo(4));
        }

        [TestCase(HttpStatusCode.Unauthorized)]
        [TestCase(HttpStatusCode.NotAcceptable)]
        [TestCase(HttpStatusCode.BadRequest)]
        [TestCase(HttpStatusCode.Forbidden)]
        public async Task Test_EnterpriseEventServiceEventPublisherClient_DoesNotRetry_ForNonTransientErrors(HttpStatusCode statusCode)
        {
            var sut = _serviceProvider.GetRequiredService<IHttpClientFactory>().CreateClient(EnterpriseEventServiceEventPublisher.EventServiceClientName);

            var testEndpoint = "/test-endpoint";
            _wireMockServer
                .Given(Request.Create().WithPath(testEndpoint).UsingGet())
                .RespondWith(Response.Create().WithStatusCode(statusCode));

            await sut.GetAsync(testEndpoint);
            Assert.That(_wireMockServer.FindLogEntries(Request.Create().WithPath(testEndpoint).UsingGet()).Count(), Is.EqualTo(1));
        }

        [Test]
        public async Task Test_EnterpriseEventServiceEventPublisherClient_AddsBearerAuthToken()
        {
            var testToken = "TOKEN";
            var expectedBearerHeader = $"Bearer {testToken}";
            var testClientId = "CLIENT_ID";
            var testScope = "SCOPE";

            _config.PublisherScope = testScope;
            _config.ClientId = testClientId;

            A.CallTo(() => _mockTokenProvider.GetTokenAsync($"{testClientId}/{testScope}")).Returns(new AccessToken(testToken, DateTimeOffset.MaxValue));

            var sut = _serviceProvider.GetRequiredService<IHttpClientFactory>().CreateClient(EnterpriseEventServiceEventPublisher.EventServiceClientName);

            var testEndpoint = "/test-endpoint";
            _wireMockServer
                .Given(Request.Create().WithPath(testEndpoint).WithHeader("authorization", expectedBearerHeader).UsingGet())
                .RespondWith(Response.Create().WithSuccess());

            await sut.GetAsync(testEndpoint);
            Assert.That(_wireMockServer.FindLogEntries(Request.Create()
                                                              .WithPath(testEndpoint)
                                                              .WithHeader("authorization", expectedBearerHeader)
                                                              .UsingGet()
                                                              ).Count(), Is.EqualTo(1));
        }
    }
}