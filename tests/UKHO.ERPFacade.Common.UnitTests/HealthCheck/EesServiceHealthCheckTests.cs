using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using UKHO.ERPFacade.Common.Configuration;
using UKHO.ERPFacade.Common.HealthCheck;
using UKHO.ERPFacade.Common.HttpClients;
using UKHO.ERPFacade.Common.Logging;

namespace UKHO.ERPFacade.Common.UnitTests.HealthCheck
{
    public class EesServiceHealthCheckTests
    {
        private ILogger<EESServiceHealthCheck> _fakeLogger;
        private IEesClient _fakeEesClient;
        private IOptions<EESHealthCheckEnvironmentConfiguration> _fakeEesConfig;
        private EESServiceHealthCheck _fakeEesServiceHealthCheck;

        private const string EesHealthCheckUrl = "http://fakeeeshealthcheckurl.com/health";

        [SetUp]
        public void Setup()
        {
            _fakeLogger = A.Fake<ILogger<EESServiceHealthCheck>>();
            _fakeEesClient = A.Fake<IEesClient>();
            _fakeEesConfig = Options.Create(new EESHealthCheckEnvironmentConfiguration
            {
                EESHealthCheckUrl = EesHealthCheckUrl
            });

            _fakeEesServiceHealthCheck = new EESServiceHealthCheck(_fakeLogger, _fakeEesClient, _fakeEesConfig);
        }

        [Test]
        public async Task WhenEESServiceReturnsResponseHealthy_ThenEESServiceHealthCheckIsHealthy()
        {
            CancellationToken fakeCancellationToken = default;

            A.CallTo(() => _fakeEesClient.GetAsync(EesHealthCheckUrl))
                .Returns(new HttpResponseMessage { StatusCode = HttpStatusCode.OK, RequestMessage = new HttpRequestMessage { RequestUri = new Uri("http://abc.com") }, Content = new StreamContent(new MemoryStream(Encoding.UTF8.GetBytes("ServiceUnavailable"))) });

            var result = await _fakeEesServiceHealthCheck.CheckHealthAsync(new HealthCheckContext(), fakeCancellationToken);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.Status, Is.EqualTo(HealthStatus.Healthy));
                Assert.That(result.Description, Is.EqualTo("Reports unhealthy if EES endpoint is not reachable, or does not return 200 status"));
            }

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
                && call.GetArgument<LogLevel>(0) == LogLevel.Information
                && call.GetArgument<EventId>(1) == EventIds.EESHealthCheckRequestSentToEES.ToEventId()
                && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "EES health check request has been sent to EES successfully. | {StatusCode}").MustHaveHappenedOnceExactly();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
                && call.GetArgument<LogLevel>(0) == LogLevel.Debug
                && call.GetArgument<EventId>(1) == EventIds.EESIsHealthy.ToEventId()
                && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "EES is Healthy").MustHaveHappenedOnceExactly();
        }

        [Test]
        public async Task WhenEESServiceReturnsResponseUnhealthy_ThenEESServiceHealthCheckIsUnhealthy()
        {
            CancellationToken fakeCancellationToken = default;

            A.CallTo(() => _fakeEesClient.GetAsync(EesHealthCheckUrl))
                .Returns(new HttpResponseMessage { StatusCode = HttpStatusCode.ServiceUnavailable, RequestMessage = new HttpRequestMessage { RequestUri = new Uri("http://abc.com") }, Content = new StreamContent(new MemoryStream(Encoding.UTF8.GetBytes("ServiceUnavailable"))) });

            var result = await _fakeEesServiceHealthCheck.CheckHealthAsync(new HealthCheckContext(), fakeCancellationToken);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.Status, Is.EqualTo(HealthStatus.Unhealthy));
                Assert.That(result.Description, Is.EqualTo("Reports unhealthy if EES endpoint is not reachable, or does not return 200 status"));
            }

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
                && call.GetArgument<LogLevel>(0) == LogLevel.Information
                && call.GetArgument<EventId>(1) == EventIds.EESHealthCheckRequestSentToEES.ToEventId()
                && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "EES health check request has been sent to EES successfully. | {StatusCode}").MustHaveHappenedOnceExactly();

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
                && call.GetArgument<LogLevel>(0) == LogLevel.Error
                && call.GetArgument<EventId>(1) == EventIds.EESIsUnhealthy.ToEventId()
                && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "EES is Unhealthy").MustHaveHappenedOnceExactly();
        }

        [Test]
        public async Task WhenEESServiceThrowsAnException_ThenEESServiceHealthCheckIsUnHealthy()
        {
            CancellationToken fakeCancellationToken = default;

            A.CallTo(() => _fakeEesClient.GetAsync(EesHealthCheckUrl)).Throws<Exception>();

            var result = await _fakeEesServiceHealthCheck.CheckHealthAsync(new HealthCheckContext(), fakeCancellationToken);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.Status, Is.EqualTo(HealthStatus.Unhealthy));
                Assert.That(result.Description, Does.StartWith("EES is Unhealthy "));
            }

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
                && call.GetArgument<LogLevel>(0) == LogLevel.Information
                && call.GetArgument<EventId>(1) == EventIds.ErrorOccurredInEES.ToEventId()
                && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "An error occurred while processing your request in EES. | {Message}").MustHaveHappenedOnceExactly();
        }
    }
}
