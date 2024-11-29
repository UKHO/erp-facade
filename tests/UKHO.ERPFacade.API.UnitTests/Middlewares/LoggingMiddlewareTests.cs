﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using UKHO.ERPFacade.API.Middlewares;
using UKHO.ERPFacade.Common.Constants;
using UKHO.ERPFacade.Common.Exceptions;
using UKHO.ERPFacade.Common.Logging;

namespace UKHO.ERPFacade.API.UnitTests.Middlewares
{
    [TestFixture]
    public class LoggingMiddlewareTests
    {
        private RequestDelegate _fakeNextMiddleware;
        private HttpContext _fakeHttpContext;
        private ILogger<LoggingMiddleware> _fakeLogger;
        private LoggingMiddleware _middleware;

        [SetUp]
        public void SetUp()
        {
            _fakeNextMiddleware = A.Fake<RequestDelegate>();
            _fakeLogger = A.Fake<ILogger<LoggingMiddleware>>();
            _fakeHttpContext = new DefaultHttpContext();

            _middleware = new LoggingMiddleware(_fakeNextMiddleware, _fakeLogger);
        }

        [Test]
        public async Task WhenExceptionIsOfTypeException_ThenLogsErrorWithUnhandledExceptionEventId()
        {
            var memoryStream = new MemoryStream();
            _fakeHttpContext.Request.Headers.Append(ApiHeaderKeys.XCorrelationIdHeaderKeyName, "fakeCorrelationId");
            _fakeHttpContext.Response.Body = memoryStream;
            A.CallTo(() => _fakeNextMiddleware(_fakeHttpContext)).Throws(new Exception("fake exception"));

            await _middleware.InvokeAsync(_fakeHttpContext);

            _fakeHttpContext.Response.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);
            _fakeHttpContext.Response.ContentType.Should().Be("application/json");

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
            && call.GetArgument<LogLevel>(0) == LogLevel.Error
            && call.GetArgument<EventId>(1) == EventIds.UnhandledException.ToEventId()
            && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "fake exception").MustHaveHappenedOnceExactly();
        }

        [Test]
        public async Task WhenExceptionIsOfTypeErpFacadeException_ThenLogsErrorWithErpFacadeExceptionEventId()
        {
            var memoryStream = new MemoryStream();
            _fakeHttpContext.Request.Headers.Append(ApiHeaderKeys.XCorrelationIdHeaderKeyName, "fakeCorrelationId");
            _fakeHttpContext.Response.Body = memoryStream;
            A.CallTo(() => _fakeNextMiddleware(_fakeHttpContext)).Throws(new ERPFacadeException(EventIds.SapXmlTemplateNotFoundException.ToEventId(), "fakemessage"));

            await _middleware.InvokeAsync(_fakeHttpContext);

            _fakeHttpContext.Response.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);
            _fakeHttpContext.Response.ContentType.Should().Be("application/json");

            A.CallTo(_fakeLogger).Where(call => call.Method.Name == "Log"
            && call.GetArgument<LogLevel>(0) == LogLevel.Error
            && call.GetArgument<EventId>(1) == EventIds.SapXmlTemplateNotFoundException.ToEventId()
            && call.GetArgument<IEnumerable<KeyValuePair<string, object>>>(2)!.ToDictionary(c => c.Key, c => c.Value)["{OriginalFormat}"].ToString() == "fakemessage").MustHaveHappenedOnceExactly();
        }
    }
}
