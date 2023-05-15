using FakeItEasy;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System;
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
        private ILoggerFactory _fakeLoggerFactory;
        private ILogger<LoggingMiddleware> _fakeLogger;

        [SetUp]
        public void SetUp()
        {
            _fakeNextMiddleware = A.Fake<RequestDelegate>();
            _fakeLoggerFactory = A.Fake<ILoggerFactory>();
            _fakeLogger = A.Fake<ILogger<LoggingMiddleware>>();
            _middleware = new LoggingMiddleware(_fakeNextMiddleware, _fakeLoggerFactory);
            _fakeHttpContext = A.Fake<HttpContext>();
            _fakeHttpContext.RequestServices = new ServiceCollection().AddSingleton(_fakeLogger).BuildServiceProvider();
        }

        [Test]
        public async Task WhenExceptionIsOfTypeException_ThenLogErrorAndThrowException()
        {
            A.CallTo(() => _fakeNextMiddleware(_fakeHttpContext)).Throws(new Exception());

            await _middleware.InvokeAsync(_fakeHttpContext);

            A.CallTo(() => _fakeLoggerFactory.CreateLogger(_fakeHttpContext.Request.Path))
                .MustHaveHappenedOnceExactly();

            Assert.That((int)HttpStatusCode.InternalServerError, Is.EqualTo(_fakeHttpContext.Response.StatusCode));
        }

        [Test]
        public async Task WhenExceptionIsNotOfTypeException_ThenLogErrorAndThrowERPFacadeException()
        {
            A.CallTo(() => _fakeNextMiddleware(_fakeHttpContext)).Throws(new ERPFacadeException(EventIds.NoScenarioFound.ToEventId()));
            var eventId = EventIds.NoScenarioFound;
            var exception = A.Fake<ERPFacadeException>(x => x.WithArgumentsForConstructor(() => new ERPFacadeException(eventId.ToEventId())));
            await _middleware.InvokeAsync(_fakeHttpContext);

            A.CallTo(() => _fakeLoggerFactory.CreateLogger(_fakeHttpContext.Request.Path))
            .MustHaveHappenedOnceExactly();

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