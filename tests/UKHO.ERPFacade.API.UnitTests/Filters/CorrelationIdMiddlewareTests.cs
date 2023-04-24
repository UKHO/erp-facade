using FakeItEasy;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System;
using UKHO.ERPFacade.API.Filters;

namespace UKHO.ERPFacade.API.UnitTests.Filters
{
    [TestFixture]
    public class CorrelationIdMiddlewareTests
    {
        private RequestDelegate _fakeNextMiddleware;
        private CorrelationIdMiddleware _middleware;
        private HttpContext _fakeHttpContext;
        private ILogger<CorrelationIdMiddleware> _fakeLogger;

        [SetUp]
        public void SetUp()
        {
            _fakeNextMiddleware = A.Fake<RequestDelegate>();
            _middleware = new CorrelationIdMiddleware(_fakeNextMiddleware);
            _fakeLogger = A.Fake<ILogger<CorrelationIdMiddleware>>();
            _fakeHttpContext = A.Fake<HttpContext>();
            _fakeHttpContext.RequestServices = new ServiceCollection().AddSingleton(_fakeLogger).BuildServiceProvider();
        }

        [Test]
        public async Task WhenTraceIdKeyExistsInRequestBody_ThenXCorrelationIdHeaderKeyAddedToRequestAndResponseHeaders()
        {
            var traceId = Guid.NewGuid().ToString();
            var bodyAsJson = new JObject { { "data", new JObject { { "traceId", traceId } } } };
            var bodyAsText = bodyAsJson.ToString();

            _fakeHttpContext.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(bodyAsText));
            _fakeHttpContext.Request.ContentLength = bodyAsText.Length;
            _fakeHttpContext.Response.Body = new MemoryStream();

            await _middleware.InvokeAsync(_fakeHttpContext);

            A.CallTo(() => _fakeHttpContext.Request.Headers[CorrelationIdMiddleware.XCORRELATIONIDHEADERKEY]).Returns(traceId);
            A.CallTo(() => _fakeHttpContext.Response.Headers[CorrelationIdMiddleware.XCORRELATIONIDHEADERKEY]).Returns(traceId);
            A.CallTo(() => _fakeHttpContext.Request.Headers.Add(CorrelationIdMiddleware.XCORRELATIONIDHEADERKEY, traceId)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _fakeHttpContext.Response.Headers.Add(CorrelationIdMiddleware.XCORRELATIONIDHEADERKEY, traceId)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _fakeLogger.BeginScope(A<Dictionary<string, object>>._)).MustHaveHappenedOnceExactly();
        }

        [Test]
        public async Task WhenTraceIdKeyDoesNotExistInRequestBody_ThenGenerateNewCorrelationId()
        {
            var traceId = Guid.NewGuid().ToString();
            var bodyAsJson = new JObject { { "data", new JObject { { "corId", traceId } } } };
            var bodyAsText = bodyAsJson.ToString();

            _fakeHttpContext.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(bodyAsText));
            _fakeHttpContext.Request.ContentLength = bodyAsText.Length;
            _fakeHttpContext.Response.Body = new MemoryStream();

            await _middleware.InvokeAsync(_fakeHttpContext);

            A.CallTo(() => _fakeHttpContext.Request.Headers.ContainsKey(CorrelationIdMiddleware.XCORRELATIONIDHEADERKEY)).Returns(true);
            A.CallTo(() => _fakeHttpContext.Response.Headers.ContainsKey(CorrelationIdMiddleware.XCORRELATIONIDHEADERKEY)).Returns(true);
            A.CallTo(() => _fakeLogger.BeginScope(A<Dictionary<string, object>>.Ignored)).MustHaveHappenedOnceExactly();
        }

        [Test]
        public async Task WhenInvokeAsyncIsCalled_ThenNextMiddlewareShouldBeInvoked()
        {
            var traceId = Guid.NewGuid().ToString();
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