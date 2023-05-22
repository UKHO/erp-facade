using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using FakeItEasy;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using NUnit.Framework.Internal.Execution;
using UKHO.ERPFacade.API.Filters;
using UKHO.ERPFacade.Common.Exceptions;
using UKHO.ERPFacade.Common.Logging;

namespace UKHO.ERPFacade.API.UnitTests.Filters
{
    [TestFixture]
    public class LoggingMiddlewareTests
    {
        private RequestDelegate _fakeNextMiddleware;
        private LoggingMiddleware _middleware;
        private HttpContext _fakeHttpContext;
        private ILogger<LoggingMiddleware> _fakeLogger;

        [SetUp]
        public void SetUp()
        {
            _fakeNextMiddleware = A.Fake<RequestDelegate>();
            _fakeLogger = A.Fake<ILogger<LoggingMiddleware>>();
            _middleware = new LoggingMiddleware(_fakeNextMiddleware, _fakeLogger);
            _fakeHttpContext = A.Fake<HttpContext>();
        }

        [Test]
        public async Task WhenExceptionIsOfTypeException_ThenLogsErrorWithUnhandledExceptionEventId()
        {
            A.CallTo(() => _fakeNextMiddleware(_fakeHttpContext)).Throws(new Exception());
            var exception = A.Fake<Exception>(x => x.WithArgumentsForConstructor(() => new Exception()));
            var correlationId = Guid.NewGuid().ToString();
            A.CallTo(() => _fakeHttpContext.Request.Headers[CorrelationIdMiddleware.XCorrelationIdHeaderKey]).Returns(correlationId);
            await _middleware.InvokeAsync(_fakeHttpContext);

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
            && call.GetArgument<LogLevel>(0) == LogLevel.Error
            && call.GetArgument<EventId>(1) == EventIds.UnhandledException.ToEventId()
            && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "Exception occured while processing ErpFacade API." + " | _X-Correlation-ID : {_X-Correlation-ID}").MustHaveHappenedOnceExactly();

            Assert.That((int)HttpStatusCode.InternalServerError, Is.EqualTo(_fakeHttpContext.Response.StatusCode));
        }

        [Test]
        public async Task WhenExceptionOfTypeERPFacadeException_ThenLogsErrorWithNoScenarioFoundEventId()
        {
            var eventId = EventIds.NoScenarioFound;
            A.CallTo(() => _fakeNextMiddleware(_fakeHttpContext)).Throws(new ERPFacadeException(eventId.ToEventId()));

            var exception = A.Fake<ERPFacadeException>(x => x.WithArgumentsForConstructor(() => new ERPFacadeException(eventId.ToEventId())));

            var correlationId = Guid.NewGuid().ToString();
            A.CallTo(() => _fakeHttpContext.Request.Headers[CorrelationIdMiddleware.XCorrelationIdHeaderKey]).Returns(correlationId);

            await _middleware.InvokeAsync(_fakeHttpContext);

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
            && call.GetArgument<LogLevel>(0) == LogLevel.Error
            && call.GetArgument<EventId>(1) == eventId.ToEventId()
            && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == eventId.ToString() + ". | _X-Correlation-ID : {_X-Correlation-ID}").MustHaveHappenedOnceExactly();

            Assert.That((int)HttpStatusCode.InternalServerError, Is.EqualTo(_fakeHttpContext.Response.StatusCode));
        }

        [Test]
        public async Task WhenInvokeAsyncIsCalled_ThenNextMiddlewareShouldBeInvoked()
        {
            var bodyAsJson = new JObject { { "data", new JObject { } } };
            var bodyAsText = bodyAsJson.ToString();

            _fakeHttpContext.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(bodyAsText));
            _fakeHttpContext.Request.ContentLength = bodyAsText.Length;
            _fakeHttpContext.Response.Body = new MemoryStream();

            await _middleware.InvokeAsync(_fakeHttpContext);

            A.CallTo(() => _fakeNextMiddleware(_fakeHttpContext)).MustHaveHappenedOnceExactly();
        }
    }
}