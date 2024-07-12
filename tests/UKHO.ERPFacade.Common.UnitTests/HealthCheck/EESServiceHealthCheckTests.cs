using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using UKHO.ERPFacade.Common.HealthCheck;
using UKHO.ERPFacade.Common.HttpClients;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using FluentAssertions;
using UKHO.ERPFacade.Common.Logging;

namespace UKHO.ERPFacade.Common.UnitTests.HealthCheck
{
    //public class EESServiceHealthCheckTests
    //{
    //    private  ILogger<EESServiceHealthCheck> _fakeLogger;
    //    private  IEESClient _fakeEESClient;
    //    private EESServiceHealthCheck _fakeEESServiceHealthCheck;

    //    [SetUp]
    //    public void Setup()
    //    {
    //        _fakeLogger = A.Fake<ILogger<EESServiceHealthCheck>>();
    //        _fakeEESClient = A.Fake<IEESClient>();

    //        _fakeEESServiceHealthCheck = new EESServiceHealthCheck(_fakeLogger, _fakeEESClient);
    //    }

    //    [Test]
    //    public async Task WhenEESServiceReturnsResponseHealthy_ThenEESServiceHealthCheckIsHealthy()
    //    {
    //        CancellationToken fakeCancellationToken = default;

    //        A.CallTo(() => _fakeEESClient.EESHealthCheck())
    //           .Returns(new HttpResponseMessage() { StatusCode = HttpStatusCode.OK, RequestMessage = new HttpRequestMessage() { RequestUri = new Uri("http://abc.com") }, Content = new StreamContent(new MemoryStream(Encoding.UTF8.GetBytes("ServiceUnavailable"))) });
    //        HealthCheckResult result = await _fakeEESServiceHealthCheck.CheckHealthAsync(new HealthCheckContext(), fakeCancellationToken);

    //        result.Status.Should().Be(HealthStatus.Healthy);
    //        result.Description.Should().Be("EES is Healthy !!!");

    //        A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
    //                && call.GetArgument<LogLevel>(0) == LogLevel.Information
    //                && call.GetArgument<EventId>(1) == EventIds.EESHealthCheckRequestSentToEES.ToEventId()
    //                && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)
    //                ["{OriginalFormat}"].ToString() == "EES health check request has been sent to EES successfully. | {StatusCode}").MustHaveHappenedOnceExactly();

    //        A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
    //                && call.GetArgument<LogLevel>(0) == LogLevel.Debug
    //                && call.GetArgument<EventId>(1) == EventIds.EESIsHealthy.ToEventId()
    //                && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)
    //                ["{OriginalFormat}"].ToString() == "EES is Healthy").MustHaveHappenedOnceExactly();
    //    }

    //    [Test]
    //    public async Task WhenEESServiceReturnsResponseUnHealthy_ThenEESServiceHealthCheckIsUnHealthy()
    //    {
    //        CancellationToken fakeCancellationToken = default;

    //        A.CallTo(() => _fakeEESClient.EESHealthCheck())
    //           .Returns(new HttpResponseMessage() { StatusCode = HttpStatusCode.ServiceUnavailable,
    //               RequestMessage = new HttpRequestMessage() { RequestUri = new Uri("http://abc.com") }, Content = new StreamContent(new MemoryStream(Encoding.UTF8.GetBytes("ServiceUnavailable"))) });

    //        HealthCheckResult result = await _fakeEESServiceHealthCheck.CheckHealthAsync(new HealthCheckContext(), fakeCancellationToken);

    //        result.Status.Should().Be(HealthStatus.Unhealthy);
    //        result.Description.Should().Be("EES is Unhealthy");

    //        A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
    //                 && call.GetArgument<LogLevel>(0) == LogLevel.Information
    //                 && call.GetArgument<EventId>(1) == EventIds.EESHealthCheckRequestSentToEES.ToEventId()
    //                 && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)
    //                 ["{OriginalFormat}"].ToString() == "EES health check request has been sent to EES successfully. | {StatusCode}").MustHaveHappenedOnceExactly();

    //        A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
    //                 && call.GetArgument<LogLevel>(0) == LogLevel.Error
    //                 && call.GetArgument<EventId>(1) == EventIds.EESIsUnhealthy.ToEventId()
    //                 && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)
    //                 ["{OriginalFormat}"].ToString() == "EES is Unhealthy !!!").MustHaveHappenedOnceExactly();
    //    }

    //    [Test]
    //    public async Task WhenEESServiceThrowsAnException_ThenEESServiceHealthCheckIsUnHealthy()
    //    {
    //        CancellationToken fakeCancellationToken = default;

    //        A.CallTo(() => _fakeEESClient.EESHealthCheck()).Throws<Exception>();

    //        HealthCheckResult result = await _fakeEESServiceHealthCheck.CheckHealthAsync(new HealthCheckContext(), fakeCancellationToken);

    //        result.Status.Should().Be(HealthStatus.Unhealthy);
    //        result.Description.Should().Be("EES is UnhealthyException of type 'System.Exception' was thrown.");

    //        A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
    //                 && call.GetArgument<LogLevel>(0) == LogLevel.Information
    //                 && call.GetArgument<EventId>(1) == EventIds.ErrorOccurredInEES.ToEventId()
    //                 && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)
    //                 ["{OriginalFormat}"].ToString() == "An error occurred while processing your request in EES").MustHaveHappenedOnceExactly();
    //    }

    //}
}
